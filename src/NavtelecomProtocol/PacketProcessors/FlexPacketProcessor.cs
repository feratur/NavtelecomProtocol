using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharpStructures;
using System.Linq;
using NavtelecomProtocol.Interfaces;

namespace NavtelecomProtocol.PacketProcessors
{
    /// <summary>
    /// FLEX message processor.
    /// </summary>
    public class FlexPacketProcessor : IPacketProcessor
    {
        #region Private members

        private readonly IFlexMessageProcessor[] _processors;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NavtelecomProtocol.PacketProcessors.FlexPacketProcessor" /> class.
        /// </summary>
        /// <param name="processors">Instances of <see cref="T:NavtelecomProtocol.PacketProcessors.Flex.IMessageProcessor" />.</param>
        public FlexPacketProcessor(params IFlexMessageProcessor[] processors) : this(processors.AsEnumerable())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NavtelecomProtocol.PacketProcessors.FlexPacketProcessor" /> class.
        /// </summary>
        /// <param name="processors">Instances of <see cref="T:NavtelecomProtocol.PacketProcessors.Flex.IMessageProcessor" />.</param>
        public FlexPacketProcessor(IEnumerable<IFlexMessageProcessor> processors)
        {
            _processors = new IFlexMessageProcessor[256];

            foreach (var messageProcessor in processors)
                _processors[(byte) messageProcessor.Identifier] = messageProcessor;
        }

        /// <summary>
        /// Returns the number of bytes to read from the stream.
        /// </summary>
        /// <param name="sessionState">Instance of <see cref="T:NavtelecomProtocol.SessionState" />.</param>
        /// <param name="receiveBuffer">A read-only collection of bytes received from a stream.</param>
        /// <param name="sendBuffer"><see cref="T:SharpStructures.MemoryBuffer" /> with data to be sent to the client.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. Contains the total number of bytes to read from the socket. Zero bytes to stop reading and send the response.</returns>
        public async Task<int> GetPendingBytesAsync(SessionState sessionState, IReadOnlyList<byte> receiveBuffer,
            MemoryBuffer sendBuffer, CancellationToken token)
        {
            switch (receiveBuffer.Count)
            {
                case 1:
                    return 1;
                default:
                    var processor = _processors[receiveBuffer[1]];

                    if (processor == null)
                        throw new ArgumentOutOfRangeException(
                            $"Unknown FLEX message identifier '0x{receiveBuffer[1]:X2}'.");

                    var pendingBytes =
                        await
                            processor.GetPendingBytesAsync(sessionState,
                                new BinaryListReader(receiveBuffer, true), 
                                new MemoryBufferWriter(sendBuffer, true), token).ConfigureAwait(false);

                    return pendingBytes;
            }
        }

        /// <summary>
        /// The first byte of the packet that identifies the message type.
        /// </summary>
        public byte MessageTypeIdentifier => 0x7E;
    }
}