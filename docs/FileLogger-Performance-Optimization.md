# FileLogger Performance Optimization

## Issue #97: Efficient Error Tracking

### Problem
The original issue highlighted that using LINQ `Count()` on a `ConcurrentQueue` in every error scenario would be inefficient, as `Count()` operations are O(n) and require iterating through the entire collection.

### Solution
Implemented efficient error tracking using thread-safe counters that provide O(1) metrics access without expensive collection iteration.

## Key Features

### 1. ILogHealthMonitor Interface
```csharp
public interface ILogHealthMonitor
{
    long MessagesEnqueued { get; }    // Total messages added to queue
    long MessagesWritten { get; }     // Successfully written messages
    long MessagesDropped { get; }     // Dropped messages (queue full/errors)
    long WriteErrors { get; }         // Write operation failures
    DateTime? LastErrorUtc { get; }   // Last error timestamp
    bool IsHealthy { get; }           // Overall health status
}
```

### 2. Thread-Safe Counters
- Uses `Interlocked.Read()` for reading counters (thread-safe, lock-free)
- Uses `Interlocked.Increment()` for updating counters (atomic operations)
- No locks required for counter access or updates

### 3. Circuit Breaker Pattern
- Opens circuit after 10 consecutive write errors
- Automatically recovers after 60 seconds
- Prevents cascading failures and resource exhaustion

### 4. Performance Characteristics
- **O(1) metrics access** - no collection iteration
- **Lock-free operations** - uses atomic CPU instructions
- **Minimal overhead** - simple long integers for counters
- **Real-time monitoring** - instant metrics availability

## Usage Example

```csharp
// Register the FileLogger with health monitoring
services.AddLogging(builder => builder.AddFileLogger());

// Inject health monitor in your service
public class MyService
{
    private readonly ILogHealthMonitor _healthMonitor;
    
    public MyService(ILogHealthMonitor healthMonitor)
    {
        _healthMonitor = healthMonitor;
    }
    
    public void CheckHealth()
    {
        var status = new
        {
            Enqueued = _healthMonitor.MessagesEnqueued,
            Written = _healthMonitor.MessagesWritten,
            Dropped = _healthMonitor.MessagesDropped,
            Errors = _healthMonitor.WriteErrors,
            IsHealthy = _healthMonitor.IsHealthy
        };
        
        // These properties access is O(1) - no Count() needed!
    }
}
```

## Benefits

1. **Performance**: O(1) vs O(n) operations for metrics access
2. **Thread Safety**: Lock-free atomic operations
3. **Resource Efficiency**: Minimal memory and CPU overhead
4. **Reliability**: Circuit breaker prevents cascade failures
5. **Observability**: Real-time health monitoring
6. **Scalability**: Performance doesn't degrade with queue size

## Implementation Details

The solution replaces any potential `queue.Count()` calls with efficient counter tracking:

- **Before**: `queue.Count()` - O(n) operation, requires iteration
- **After**: `Interlocked.Read(ref counter)` - O(1) operation, single memory read

This ensures that error tracking and health monitoring remain efficient even under high-load scenarios with large message queues.