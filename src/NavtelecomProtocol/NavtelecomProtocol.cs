using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharpNetwork;
using SharpStructures;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using NavtelecomProtocol.Interfaces;

namespace NavtelecomProtocol
{
    /// <summary>
    /// Protocol for TCP processing of network data from Navtelecom telematics devices.
    /// </summary>
    public class NavtelecomProtocol
    {
        #region Private members

        private readonly IPacketProcessor[] _primaryOperators;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NavtelecomProtocol.NavtelecomProtocol" /> class.
        /// </summary>
        /// <param name="processors">Instances of <see cref="T:NavtelecomProtocol.IPacketProcessor" />.</param>
        public NavtelecomProtocol(params IPacketProcessor[] processors) : this(processors.AsEnumerable())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NavtelecomProtocol.NavtelecomProtocol" /> class.
        /// </summary>
        /// <param name="processors">Instances of <see cref="T:NavtelecomProtocol.IPacketProcessor" />.</param>
        public NavtelecomProtocol(IEnumerable<IPacketProcessor> processors)
        {
            _primaryOperators = new IPacketProcessor[256];

            foreach (var processor in processors)
                _primaryOperators[processor.MessageTypeIdentifier] = processor;
        }

        /// <summary>
        /// Processes the established client-server socket connection.
        /// </summary>
        /// <param name="socket">Socket connected to a client device.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task WorkAsync(Socket socket, CancellationToken token)
        {
            using (var stream = new AsyncBufferedSocketStream(socket))
            {
                try
                {
                    var state = new SessionState();

                    for (;;)
                    {
                        stream.ClearReceiveBuffer();

                        await stream.ReadToBufferAsync(1, token).ConfigureAwait(false);

                        var processor = _primaryOperators[stream.ReceiveBufferArray[0]];

                        if (processor == null)
                            throw new ArgumentException($"Unknown message prefix '0x{stream.ReceiveBufferArray[0]:X2}'.");

                        stream.SendBuffer.SetPosition(0);

                        for (;;)
                        {
                            var pendingBytes =
                                await
                                    processor.GetPendingBytesAsync(state, stream.ReceiveBufferArray,
                                        stream.ReceivedBytes,
                                        stream.SendBuffer, token).ConfigureAwait(false);

                            if (pendingBytes == 0)
                                break;

                            await stream.ReadToBufferAsync(pendingBytes, token).ConfigureAwait(false);
                        }

                        await stream.WriteBufferToSocketAsync(token).ConfigureAwait(false);

                        // Workaround until crashes are implemented correctly by the devices
                        var crashDetected = false;

                        if (processor.MessageTypeIdentifier == 0x40 && stream.ReceivedBytes >= 22 &&
                            stream.ReceiveBufferArray.Skip(16).Take(6).Select(x => (char) x).SequenceEqual("*>FLEX"))
                        {
                            crashDetected = true;
                        }
                        else if (processor.MessageTypeIdentifier == 0x7E && stream.ReceivedBytes >= 2)
                        {
                            var reader = new ArrayReader(stream.ReceiveBufferArray, stream.ReceivedBytes, true);

                            const int detectEvent = 0xA03B;

                            switch ((char) stream.ReceiveBufferArray[1])
                            {
                                case 'T':
                                    if (stream.ReceivedBytes < 12)
                                        break;
                                    reader.SetPosition(10);
                                    var numType = reader.ReadUInt16();
                                    if (numType == detectEvent)
                                        crashDetected = true;
                                    break;
                                case 'A':
                                    if (stream.ReceivedBytes < 4)
                                        break;
                                    var eventCount = stream.ReceiveBufferArray[2];
                                    if (eventCount <= 0)
                                        break;
                                    var eventLength = (stream.ReceivedBytes - 4)/eventCount;
                                    if (eventLength < 6)
                                        break;
                                    for (var i = 0; i < eventCount; ++i)
                                    {
                                        reader.SetPosition(7 + i*eventLength);
                                        var eventType = reader.ReadUInt16();
                                        if (eventType != detectEvent)
                                            continue;
                                        crashDetected = true;
                                        break;
                                    }
                                    break;
                            }
                        }

                        if (crashDetected)
                        {
                            stream.SendBuffer.SetPosition(0);

                            new MemoryBufferWriter(stream.SendBuffer, true).Write(CrashInformation.CrashInfoQuery);

                            await stream.WriteBufferToSocketAsync(token).ConfigureAwait(false);
                        }
                    }

                }
                catch (SocketStreamException)
                {
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    string receivedBytes = null;

                    try
                    {
                        var bytesLeft = stream.Client.Available;

                        if (bytesLeft > 0)
                            await stream.ReadToBufferAsync(bytesLeft, CancellationToken.None).ConfigureAwait(false);

                        var hex = new StringBuilder(stream.ReceivedBytes * 2);

                        foreach (var b in stream.ReceiveBufferArray.Take(stream.ReceivedBytes))
                            hex.Append(b.ToString("X2"));

                        receivedBytes = hex.ToString();
                    }
                    catch
                    {
                        // ignored
                    }

                    var exceptionMessage = $"Error: {ex.Message}. Receive buffer: {receivedBytes ?? "NA"}.";

                    throw new Exception(exceptionMessage, ex);
                }
            }
        }
    }
}