using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharpStructures;
using System.Linq;
using NavtelecomProtocol.Interfaces;

namespace NavtelecomProtocol.PacketProcessors
{
    /// <summary>
    /// NTCB message processor.
    /// </summary>
    public class NtcbPacketProcessor : IPacketProcessor
    {
        private const int HeaderLength = 16;

        private const string HeaderPreamble = "@NTC";

        #region Private members

        private readonly StringTree<INtcbBodyProcessor> _bodyProcessors = new StringTree<INtcbBodyProcessor>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NavtelecomProtocol.PacketProcessors.NtcbPacketProcessor" /> class.
        /// </summary>
        /// <param name="bodyProcessors">Instances of <see cref="T:NavtelecomProtocol.PacketProcessors.Ntcb.IBodyProcessor" />.</param>
        public NtcbPacketProcessor(params INtcbBodyProcessor[] bodyProcessors) : this(bodyProcessors.AsEnumerable())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NavtelecomProtocol.PacketProcessors.NtcbPacketProcessor" /> class.
        /// </summary>
        /// <param name="bodyProcessors">Instances of <see cref="T:NavtelecomProtocol.PacketProcessors.Ntcb.IBodyProcessor" />.</param>
        public NtcbPacketProcessor(IEnumerable<INtcbBodyProcessor> bodyProcessors)
        {
            foreach (var bodyProcessor in bodyProcessors)
                _bodyProcessors.Add(bodyProcessor.MessageIdentifier, bodyProcessor);
        }

        /// <summary>
        /// Returns the number of bytes to read from the stream.
        /// </summary>
        /// <param name="sessionState">Instance of <see cref="T:NavtelecomProtocol.SessionState" />.</param>
        /// <param name="buffer">Receive buffer (array length may be more than the real number of received bytes - parameter <paramref name="length" />).</param>
        /// <param name="length">The number of received bytes.</param>
        /// <param name="sendBuffer"><see cref="T:SharpStructures.MemoryBuffer" /> with data to be sent to the client.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. Contains the total number of bytes to read from the socket. Zero bytes to stop reading and send the response.</returns>
        public Task<int> GetPendingBytesAsync(SessionState sessionState, byte[] buffer, int length, MemoryBuffer sendBuffer, CancellationToken token)
        {
            switch (length)
            {
                case 1:
                    return Task.FromResult(HeaderLength - 1);
                case HeaderLength:
                    var reader = new ArrayReader(buffer, length, true);

                    if (!reader.ReadBytes(HeaderPreamble.Length).Select(x => (char) x).SequenceEqual(HeaderPreamble))
                        throw new ArgumentException("NTCB header preamble does not match.");

                    sessionState.ReceiverId = reader.ReadUInt32();
                    sessionState.SenderId = reader.ReadUInt32();

                    var payloadLength = reader.ReadUInt16();

                    reader.ReadByte();

                    var headerChecksum = reader.ReadByte();

                    if (BinaryUtilities.GetXorSum(buffer.Take(HeaderLength - 1)) != headerChecksum)
                        throw new ArgumentException("NTCB header checksum does not match.");

                    return Task.FromResult((int) payloadLength);
                default:
                    if (BinaryUtilities.GetXorSum(buffer.Skip(HeaderLength)) != buffer[14])
                        throw new ArgumentException("NTCB body checksum does not match.");

                    var bodyProcessor =
                        _bodyProcessors.GetValueOrDefault(buffer.Skip(HeaderLength).Select(x => (char) x));

                    if (bodyProcessor == null)
                        throw new ArgumentException("Unknown NTCB message type.");

                    var bodyReader = new ArrayReader(buffer, length, true);

                    bodyReader.SetPosition(HeaderLength);

                    sendBuffer.AllocateSpace(HeaderLength);

                    var writer = new MemoryBufferWriter(sendBuffer, true);

                    bodyProcessor.ProcessBody(sessionState, bodyReader, writer);

                    var responseLength = sendBuffer.Position - HeaderLength;

                    sendBuffer.SetPosition(0);

                    writer.Write(HeaderPreamble.Select(x => (byte) x).ToArray());
                    writer.Write(sessionState.SenderId);
                    writer.Write(sessionState.ReceiverId);
                    writer.Write((ushort) responseLength);
                    writer.Write(BinaryUtilities.GetXorSum(sendBuffer.Array.Skip(HeaderLength).Take(responseLength)));
                    writer.Write(BinaryUtilities.GetXorSum(sendBuffer.Array.Take(HeaderLength - 1)));

                    sendBuffer.SetPosition(responseLength + HeaderLength);

                    return Task.FromResult(0);
            }
        }

        /// <summary>
        /// The first byte of the packet that identifies the message type.
        /// </summary>
        public byte MessageTypeIdentifier => 0x40;
    }
}