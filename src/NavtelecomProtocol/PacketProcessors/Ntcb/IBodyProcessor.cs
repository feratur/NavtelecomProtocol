using SharpStructures;

namespace NavtelecomProtocol.PacketProcessors.Ntcb
{
    /// <summary>
    /// NTCB body message processor (without a header).
    /// </summary>
    public interface IBodyProcessor
    {
        /// <summary>
        /// NTCB message type string identifier.
        /// </summary>
        string MessageIdentifier { get; }

        /// <summary>
        /// Processes the body of the message.
        /// </summary>
        /// <param name="state">Instance of <see cref="T:NavtelecomProtocol.SessionState" />.</param>
        /// <param name="reader"><see cref="T:SharpStructures.ArrayReader" /> linked to an NTCB message body.</param>
        /// <param name="writer"><see cref="T:SharpStructures.MemoryBufferWriter" /> with data to be sent to the client.</param>
        void ProcessBody(SessionState state, ArrayReader reader, MemoryBufferWriter writer);
    }
}