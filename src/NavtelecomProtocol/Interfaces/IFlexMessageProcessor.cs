using System.Threading;
using System.Threading.Tasks;
using SharpStructures;

namespace NavtelecomProtocol.Interfaces
{
    /// <summary>
    /// FLEX message processor.
    /// </summary>
    public interface IFlexMessageProcessor
    {
        /// <summary>
        /// Returns the number of bytes to read from the stream.
        /// </summary>
        /// <param name="state">Instance of <see cref="T:NavtelecomProtocol.SessionState" />.</param>
        /// <param name="reader"><see cref="T:SharpStructures.BinaryListReader" /> linked to a FLEX message body.</param>
        /// <param name="writer"><see cref="T:SharpStructures.MemoryBufferWriter" /> with data to be sent to the client.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. Contains the total number of bytes to read from the socket. Zero bytes to stop reading and send the response.</returns>
        Task<int> GetPendingBytesAsync(SessionState state, BinaryListReader reader, MemoryBufferWriter writer,
            CancellationToken token);

        /// <summary>
        /// FLEX message type identifier.
        /// </summary>
        char Identifier { get; }
    }
}