﻿using System;
using System.Threading;
using System.Threading.Tasks;
using SharpStructures;

namespace NavtelecomProtocol
{
    /// <summary>
    /// Represents a general processor of the message distinguished by its first byte.
    /// </summary>
    public interface IPacketProcessor
    {
        /// <summary>
        /// Returns the number of bytes to read from the stream.
        /// </summary>
        /// <param name="sessionState">Instance of <see cref="T:NavtelecomProtocol.SessionState" />.</param>
        /// <param name="buffer">Receive buffer (array length may be more than the real number of received bytes - parameter <paramref name="length" />).</param>
        /// <param name="length">The number of received bytes.</param>
        /// <param name="sendBuffer"><see cref="T:SharpStructures.MemoryBuffer" /> with data to be sent to the client.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. Contains the total number of bytes to read from the socket. Zero bytes to stop reading and send the response.</returns>
        Task<int> GetPendingBytesAsync(SessionState sessionState, byte[] buffer, int length, MemoryBuffer sendBuffer,
            CancellationToken token);

        /// <summary>
        /// The first byte of the packet that identifies the message type.
        /// </summary>
        byte MessageTypeIdentifier { get; }
    }
}