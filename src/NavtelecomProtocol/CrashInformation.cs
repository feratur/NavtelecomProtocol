namespace NavtelecomProtocol
{
    /// <summary>
    /// Contains RAIF crash data.
    /// </summary>
    public class CrashInformation
    {
        /// <summary>
        /// Query ('~Q') for crash status.
        /// </summary>
        public static readonly byte[] CrashInfoQuery = { 0x7E, 0x51, 0x7B, 0x00, 0x5B };

        /// <summary>
        /// Query ('~G') for crash binary data.
        /// </summary>
        public static readonly byte[] CrashDataQuery = { 0x7E, 0x47, 0x7B, 0x00 };

        /// <summary>
        /// The name of the crash file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Crash time in UNIX timestamp format.
        /// </summary>
        public uint Timestamp { get; set; }

        /// <summary>
        /// Current crash data offset.
        /// </summary>
        public uint Offset { get; set; }

        /// <summary>
        /// Crash binary data.
        /// </summary>
        public byte[] Data { get; set; }
    }
}