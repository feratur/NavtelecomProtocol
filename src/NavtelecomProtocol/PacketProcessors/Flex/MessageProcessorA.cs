using System;
using System.Threading;
using System.Threading.Tasks;
using SharpStructures;
using System.Linq;
using NavtelecomProtocol.Interfaces;

namespace NavtelecomProtocol.PacketProcessors.Flex
{
    /// <summary>
    /// '~A' message processor.
    /// </summary>
    public class MessageProcessorA : IFlexMessageProcessor
    {
        private static readonly byte[] Response = "~A".Select(x => (byte) x).ToArray();

        /// <summary>
        /// Returns the number of bytes to read from the stream.
        /// </summary>
        /// <param name="state">Instance of <see cref="T:NavtelecomProtocol.SessionState" />.</param>
        /// <param name="reader"><see cref="T:SharpStructures.ArrayReader" /> linked to a FLEX message body.</param>
        /// <param name="writer"><see cref="T:SharpStructures.MemoryBufferWriter" /> with data to be sent to the client.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. Contains the total number of bytes to read from the socket. Zero bytes to stop reading and send the response.</returns>
        public Task<int> GetPendingBytesAsync(SessionState state, ArrayReader reader, MemoryBufferWriter writer,
            CancellationToken token)
        {
            switch (reader.Length)
            {
                case 2:
                    return Task.FromResult(1);
                case 3:
                    var messageSize = FlexMessageTable.GetFlexMessageSize(state);
                    var messageCount = reader.Array[2];

                    writer.Write(Response);
                    writer.Write(messageCount);
                    writer.Write(BinaryUtilities.GetCrc8(writer.Buffer));

                    return Task.FromResult(messageSize*messageCount + 1);
                default:
                    if (reader.Array[reader.Length - 1] != BinaryUtilities.GetCrc8(reader.Array.Take(reader.Length - 1)))
                        throw new ArgumentException("Invalid FLEX message CRC.");

                    return Task.FromResult(0);
            }
        }

        /// <summary>
        /// FLEX message type identifier.
        /// </summary>
        public char Identifier => 'A';
    }
}