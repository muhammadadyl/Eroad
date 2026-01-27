using Eroad.BFF.Gateway.Application.Interfaces;
using StackExchange.Redis;

namespace Eroad.BFF.Gateway.Application.Services;

public class RedisLockManager : IDistributedLockManager
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisLockManager> _logger;

    public RedisLockManager(IConnectionMultiplexer redis, ILogger<RedisLockManager> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> TryAcquireLockAsync(string lockKey, string owner, TimeSpan timeout)
    {
        try
        {
            var db = _redis.GetDatabase();
            var acquired = await db.StringSetAsync(lockKey, owner, timeout, When.NotExists);
            
            if (acquired)
            {
                _logger.LogInformation("Lock acquired: {LockKey} by {Owner} for {Timeout}", lockKey, owner, timeout);
            }
            else
            {
                _logger.LogWarning("Failed to acquire lock: {LockKey} (already held)", lockKey);
            }
            
            return acquired;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring lock: {LockKey}", lockKey);
            throw;
        }
    }

    public async Task ReleaseLockAsync(string lockKey, string owner)
    {
        try
        {
            var db = _redis.GetDatabase();
            
            // Lua script to ensure only the lock owner can release it
            var script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";
            
            var result = await db.ScriptEvaluateAsync(script, new RedisKey[] { lockKey }, new RedisValue[] { owner });
            
            if ((int)result == 1)
            {
                _logger.LogInformation("Lock released: {LockKey} by {Owner}", lockKey, owner);
            }
            else
            {
                _logger.LogWarning("Failed to release lock: {LockKey} (not owner or already released)", lockKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing lock: {LockKey}", lockKey);
            throw;
        }
    }
}
