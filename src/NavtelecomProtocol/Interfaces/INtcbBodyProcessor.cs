using SharpStructures;

namespace NavtelecomProtocol.Interfaces
{
    /// <summary>
    /// NTCB body message processor (without a header).
    /// </summary>
    public interface INtcbBodyProcessor
    {
        /// <summary>
        /// NTCB message type string identifier.
        /// </summary>
        string MessageIdentifier { get; }

        /// <summary>
        /// Processes the body of the message.
        /// </summary>
        /// <param name="state">Instance of <see cref="T:NavtelecomProtocol.SessionState" />.</param>
        /// <param name="reader"><see cref="T:SharpStructures.BinaryListReader" /> linked to an NTCB message body.</param>
        /// <param name="writer"><see cref="T:SharpStructures.MemoryBufferWriter" /> with data to be sent to the client.</param>
        void ProcessBody(SessionState state, BinaryListReader reader, MemoryBufferWriter writer);
    }
}