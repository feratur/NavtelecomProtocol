using System;
using System.Threading;
using System.Threading.Tasks;
using SharpStructures;
using System.Linq;
using NavtelecomProtocol.Interfaces;

namespace NavtelecomProtocol.PacketProcessors.Flex
{
    /// <summary>
    /// '~D' message processor.
    /// </summary>
    public class MessageProcessorD : IFlexMessageProcessor
    {
        #region Private members

        private readonly Func<SessionState, CancellationToken, Task> _onReadyCrash;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NavtelecomProtocol.PacketProcessors.Flex.MessageProcessorD" /> class.
        /// </summary>
        public MessageProcessorD() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NavtelecomProtocol.PacketProcessors.Flex.MessageProcessorD" /> class.
        /// </summary>
        /// <param name="onReadyCrash">Async action to execute on a received crash.</param>
        public MessageProcessorD(Func<SessionState, CancellationToken, Task> onReadyCrash)
        {
            _onReadyCrash = onReadyCrash;
        }

        /// <summary>
        /// Returns the number of bytes to read from the stream.
        /// </summary>
        /// <param name="state">Instance of <see cref="T:NavtelecomProtocol.SessionState" />.</param>
        /// <param name="reader"><see cref="T:SharpStructures.ArrayReader" /> linked to a FLEX message body.</param>
        /// <param name="writer"><see cref="T:SharpStructures.MemoryBufferWriter" /> with data to be sent to the client.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. Contains the total number of bytes to read from the socket. Zero bytes to stop reading and send the response.</returns>
        public async Task<int> GetPendingBytesAsync(SessionState state, ArrayReader reader, MemoryBufferWriter writer, CancellationToken token)
        {
            switch (reader.Length)
            {
                case 2:
                    return 13;
                case 15:
                    reader.SetPosition(13);

                    return reader.ReadUInt16() + 1;
                default:
                    if (reader.Array[reader.Length - 1] != BinaryUtilities.GetCrc8(reader.Array.Take(reader.Length - 1)))
                        throw new ArgumentException("Invalid FLEX message CRC.");

                    reader.SetPosition(4);

                    var result = reader.ReadByte();
                    var crashTime = reader.ReadUInt32();
                    var offset = reader.ReadUInt32();
                    var sizeRead = reader.ReadUInt16();

                    if (state.CrashInfo != null && result == 0x00 && state.CrashInfo.Timestamp == crashTime)
                    {
                        Buffer.BlockCopy(reader.Array, 15, state.CrashInfo.Data, (int)offset, sizeRead);
                        state.CrashInfo.Offset = offset + sizeRead;

                        var bytesLeft = state.CrashInfo.Data.Length - state.CrashInfo.Offset;

                        writer.Write(CrashInformation.CrashDataQuery);
                        writer.Write(crashTime);
                        writer.Write(state.CrashInfo.Offset);
                        writer.Write(bytesLeft > ushort.MaxValue ? ushort.MaxValue : (ushort)bytesLeft);
                        writer.Write(BinaryUtilities.GetCrc8(writer.Buffer));

                        if (bytesLeft == 0)
                        {
                            if (_onReadyCrash != null)
                                await _onReadyCrash(state, token).ConfigureAwait(false);

                            state.CrashInfo = null;
                        }
                    }

                    return 0;
            }
        }

        /// <summary>
        /// FLEX message type identifier.
        /// </summary>
        public char Identifier => 'D';
    }
}