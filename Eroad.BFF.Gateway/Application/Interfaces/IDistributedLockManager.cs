namespace Eroad.BFF.Gateway.Application.Interfaces;

public interface IDistributedLockManager
{
    /// <summary>
    /// Attempts to acquire a distributed lock
    /// </summary>
    /// <param name="lockKey">The unique key identifying the lock (e.g., "driver-assignment:guid")</param>
    /// <param name="owner">The unique identifier of the lock owner (e.g., request correlation ID)</param>
    /// <param name="timeout">How long to hold the lock before automatic expiration</param>
    /// <returns>True if lock was acquired, false otherwise</returns>
    Task<bool> TryAcquireLockAsync(string lockKey, string owner, TimeSpan timeout);
    
    /// <summary>
    /// Releases a previously acquired lock
    /// </summary>
    /// <param name="lockKey">The unique key identifying the lock</param>
    /// <param name="owner">The unique identifier of the lock owner (must match acquisition)</param>
    Task ReleaseLockAsync(string lockKey, string owner);
}
