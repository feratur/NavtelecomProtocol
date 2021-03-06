﻿using System;
using System.Threading;
using System.Threading.Tasks;
using SharpStructures;
using System.Linq;
using NavtelecomProtocol.Interfaces;

namespace NavtelecomProtocol.PacketProcessors.Flex
{
    /// <summary>
    /// '~T' message processor.
    /// </summary>
    public class MessageProcessorT : IFlexMessageProcessor
    {
        private static readonly byte[] Response = "~T".Select(x => (byte) x).ToArray();

        /// <summary>
        /// Returns the number of bytes to read from the stream.
        /// </summary>
        /// <param name="state">Instance of <see cref="T:NavtelecomProtocol.SessionState" />.</param>
        /// <param name="reader"><see cref="T:SharpStructures.BinaryListReader" /> linked to a FLEX message body.</param>
        /// <param name="writer"><see cref="T:SharpStructures.MemoryBufferWriter" /> with data to be sent to the client.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. Contains the total number of bytes to read from the socket. Zero bytes to stop reading and send the response.</returns>
        public Task<int> GetPendingBytesAsync(SessionState state, BinaryListReader reader, MemoryBufferWriter writer,
            CancellationToken token)
        {
            switch (reader.ByteList.Count)
            {
                case 2:
                    var messageSize = FlexMessageTable.GetFlexMessageSize(state);

                    return Task.FromResult(messageSize + 5);
                default:
                    if (reader.ByteList.Last() != BinaryUtilities.GetCrc8(reader.ByteList.Take(reader.ByteList.Count - 1)))
                        throw new ArgumentException("Invalid FLEX message CRC.");

                    reader.SetPosition(2);

                    var eventIndex = reader.ReadUInt32();

                    writer.Write(Response);
                    writer.Write(eventIndex);
                    writer.Write(BinaryUtilities.GetCrc8(writer.Buffer));

                    return Task.FromResult(0);
            }
        }

        /// <summary>
        /// FLEX message type identifier.
        /// </summary>
        public char Identifier => 'T';
    }
}