using System;
using SharpStructures;
using System.Linq;

namespace NavtelecomProtocol.PacketProcessors.Ntcb
{
    /// <summary>
    /// NTCB handshake containing device identifier.
    /// </summary>
    public class IdentityBodyProcessor
    {
        private static readonly byte[] Response = "*<S".Select(x => (byte)x).ToArray();

        private const string Prefix = "*>S:";

        /// <summary>
        /// NTCB message type string identifier.
        /// </summary>
        public string MessageIdentifier => "*>S";

        /// <summary>
        /// Processes the body of the message.
        /// </summary>
        /// <param name="state">Instance of <see cref="T:NavtelecomProtocol.SessionState" />.</param>
        /// <param name="reader"><see cref="T:SharpStructures.ArrayReader" /> linked to an NTCB message body.</param>
        /// <param name="writer"><see cref="T:SharpStructures.MemoryBufferWriter" /> with data to be sent to the client.</param>
        public void ProcessBody(SessionState state, ArrayReader reader, MemoryBufferWriter writer)
        {
            if (!reader.ReadBytes(Prefix.Length).Select(x => (char)x).SequenceEqual(Prefix))
                throw new ArgumentException("NTCB identity message prefix does not match.");

            state.DeviceIdentifier = new string(reader.ReadBytes(15).Select(x => (char)x).ToArray());

            writer.Write(Response);
        }
    }
}