namespace NavtelecomProtocol
{
    /// <summary>
    /// Session data persistent through a connection.
    /// </summary>
    public class SessionState
    {
        /// <summary>
        /// Receiver (server) ID.
        /// </summary>
        public uint ReceiverId { get; set; }

        /// <summary>
        /// Sender (device) ID.
        /// </summary>
        public uint SenderId { get; set; }

        /// <summary>
        /// RAIF crash data.
        /// </summary>
        public CrashInformation CrashInfo { get; set; }

        /// <summary>
        /// Identifier (IMEI) of the device.
        /// </summary>
        public string DeviceIdentifier { get; set; }

        /// <summary>
        /// Structure version.
        /// </summary>
        public byte StructVersion { get; set; }

        /// <summary>
        /// Protocol version.
        /// </summary>
        public byte ProtocolVersion { get; set; }

        /// <summary>
        /// The mask of transmitted telematics fields (according to the protocol).
        /// </summary>
        public bool[] FieldMask { get; set; }
    }
}