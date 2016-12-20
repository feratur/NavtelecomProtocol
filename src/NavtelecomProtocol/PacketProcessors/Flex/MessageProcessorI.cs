using System;
using System.Threading;
using System.Threading.Tasks;
using SharpStructures;
using System.Linq;

namespace NavtelecomProtocol.PacketProcessors.Flex
{
    /// <summary>
    /// '~I' message processor.
    /// </summary>
    public class MessageProcessorI : IMessageProcessor
    {
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
                    return Task.FromResult(12);
                case 14:
                    return Task.FromResult(reader.Array[13] + 1);
                default:
                    if (reader.Array[reader.Length - 1] != BinaryUtilities.GetCrc8(reader.Array.Take(reader.Length - 1)))
                        throw new ArgumentException("Invalid FLEX message CRC.");

                    reader.SetPosition(2);

                    reader.ReadByte();
                    reader.ReadByte();

                    var crashTime = reader.ReadUInt32();
                    var crashLength = reader.ReadUInt32();

                    var flags = reader.ReadByte();

                    var nameLength = reader.ReadByte();
                    var crashName = new string(reader.ReadBytes(nameLength).Select(x => (char) x).ToArray());

                    if (crashTime > 0 && crashLength > 0 && flags == 0xFF)
                    {
                        state.CrashInfo = new CrashInformation
                        {
                            Data = new byte[crashLength],
                            Name = crashName,
                            Timestamp = crashTime
                        };

                        writer.Write(CrashInformation.CrashDataQuery);
                        writer.Write(crashTime);
                        writer.Write(0U);
                        writer.Write(crashLength > ushort.MaxValue ? ushort.MaxValue : (ushort) crashLength);
                        writer.Write(BinaryUtilities.GetCrc8(writer.Buffer));
                    }

                    return Task.FromResult(0);
            }
        }

        /// <summary>
        /// FLEX message type identifier.
        /// </summary>
        public char Identifier => 'I';
    }
}