using System;
using SharpStructures;
using System.Linq;
using NavtelecomProtocol.Interfaces;

namespace NavtelecomProtocol.PacketProcessors.Ntcb
{
    /// <summary>
    /// FLEX protocol handshake.
    /// </summary>
    public class ProtocolBodyProcessor : INtcbBodyProcessor
    {
        private const byte ProtocolIdentifier = 0xB0;

        /// <summary>
        /// NTCB message type string identifier.
        /// </summary>
        public string MessageIdentifier => "*>FLEX";

        /// <summary>
        /// Processes the body of the message.
        /// </summary>
        /// <param name="state">Instance of <see cref="T:NavtelecomProtocol.SessionState" />.</param>
        /// <param name="reader"><see cref="T:SharpStructures.ArrayReader" /> linked to an NTCB message body.</param>
        /// <param name="writer"><see cref="T:SharpStructures.MemoryBufferWriter" /> with data to be sent to the client.</param>
        public void ProcessBody(SessionState state, ArrayReader reader, MemoryBufferWriter writer)
        {
            if (!reader.ReadBytes(MessageIdentifier.Length).Select(x => (char)x).SequenceEqual(MessageIdentifier))
                throw new ArgumentException("NTCB identity message prefix does not match.");

            if (reader.ReadByte() != ProtocolIdentifier)
                throw new ArgumentException("Unknown NTCB protocol identifier.");

            state.ProtocolVersion = reader.ReadByte();
            state.StructVersion = reader.ReadByte();

            var dataSize = reader.ReadByte();

            state.FieldMask = new bool[dataSize];

            var maskBytes = reader.ReadBytes(BinaryUtilities.GetByteCountFromBitCount(dataSize));

            for (var i = 0; i < dataSize; ++i)
            {
                var targetByte = maskBytes[i >> 3];

                var mask = 1 << (7 - i & 7);

                state.FieldMask[i] = (targetByte & mask) != 0;
            }

            writer.Write(BinaryUtilities.StringToBytes("*<FLEX"));
            writer.Write(ProtocolIdentifier);
            writer.Write(state.ProtocolVersion);
            writer.Write(state.StructVersion);
        }
    }
}