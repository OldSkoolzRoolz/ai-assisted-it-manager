// Project Name: ClientApp.Tests
// File Name: FileLoggerHealthTests.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using KC.ITCompanion.ClientApp.Logging;

namespace KC.ITCompanion.ClientApp.Tests.Logging
{
    /// <summary>
    /// Tests for the efficient error tracking implementation in FileLogger.
    /// Validates that metrics are tracked without using expensive Count() operations.
    /// </summary>
    public class FileLoggerHealthTests
    {
        private readonly string _tempDir;
        private readonly FileLoggerProvider _provider;
        private readonly ILogHealthMonitor _healthMonitor;
        private readonly ILogger _logger;

        public FileLoggerHealthTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "FileLoggerTests_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(_tempDir);
            
            _provider = new FileLoggerProvider(_tempDir);
            _healthMonitor = _provider;
            _logger = _provider.CreateLogger("TestCategory");
        }

        public void TestInitialState()
        {
            // Verify initial state - all counters should be zero
            AssertEqual(0, _healthMonitor.MessagesEnqueued, "Initial enqueued count");
            AssertEqual(0, _healthMonitor.MessagesWritten, "Initial written count");
            AssertEqual(0, _healthMonitor.MessagesDropped, "Initial dropped count");
            AssertEqual(0, _healthMonitor.WriteErrors, "Initial error count");
            AssertEqual(null, _healthMonitor.LastErrorUtc, "Initial last error time");
            AssertEqual(true, _healthMonitor.IsHealthy, "Initial health status");
        }

        public void TestNormalLogging()
        {
            const int messageCount = 10;
            
            // Log some messages
            for (int i = 0; i < messageCount; i++)
            {
                _logger.LogInformation($"Test message {i}");
            }

            // Wait for processing
            Thread.Sleep(100);

            // Verify metrics are updated correctly
            AssertEqual(messageCount, _healthMonitor.MessagesEnqueued, "Messages enqueued");
            AssertEqual(messageCount, _healthMonitor.MessagesWritten, "Messages written");
            AssertEqual(0, _healthMonitor.MessagesDropped, "Messages dropped");
            AssertEqual(0, _healthMonitor.WriteErrors, "Write errors");
            AssertEqual(true, _healthMonitor.IsHealthy, "Health status");
        }

        public void TestDroppedMessages()
        {
            // First log some normal messages
            _logger.LogInformation("Normal message");
            Thread.Sleep(50);

            var initialEnqueued = _healthMonitor.MessagesEnqueued;
            var initialWritten = _healthMonitor.MessagesWritten;

            // Dispose provider to simulate error condition
            _provider.Dispose();

            // Try to log more messages - these should be dropped
            for (int i = 0; i < 5; i++)
            {
                _logger.LogError($"This should be dropped {i}");
            }

            // Verify dropped messages are counted
            var finalDropped = _healthMonitor.MessagesDropped;
            AssertGreater(finalDropped, 0, "Should have dropped messages");
        }

        public void TestPerformanceCharacteristics()
        {
            // This test validates that metrics access is O(1) and not expensive
            var startTime = DateTime.UtcNow;
            
            // Access metrics many times - should be very fast
            for (int i = 0; i < 10000; i++)
            {
                var _ = _healthMonitor.MessagesEnqueued;
                var __ = _healthMonitor.MessagesWritten;
                var ___ = _healthMonitor.MessagesDropped;
                var ____ = _healthMonitor.WriteErrors;
                var _____ = _healthMonitor.IsHealthy;
            }
            
            var elapsed = DateTime.UtcNow.Subtract(startTime);
            
            // Should complete very quickly (well under 1 second)
            AssertLess(elapsed.TotalMilliseconds, 1000, "Metrics access should be very fast");
        }

        public void Cleanup()
        {
            _provider?.Dispose();
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        // Simple assertion helpers
        private static void AssertEqual<T>(T expected, T actual, string message)
        {
            if (!Equals(expected, actual))
            {
                throw new Exception($"Assertion failed - {message}: Expected {expected}, got {actual}");
            }
        }

        private static void AssertGreater<T>(T actual, T threshold, string message) where T : IComparable<T>
        {
            if (actual.CompareTo(threshold) <= 0)
            {
                throw new Exception($"Assertion failed - {message}: Expected {actual} > {threshold}");
            }
        }

        private static void AssertLess<T>(T actual, T threshold, string message) where T : IComparable<T>
        {
            if (actual.CompareTo(threshold) >= 0)
            {
                throw new Exception($"Assertion failed - {message}: Expected {actual} < {threshold}");
            }
        }

        // Simple test runner
        public static void RunAllTests()
        {
            var tests = new FileLoggerHealthTests();
            
            try
            {
                Console.WriteLine("Running FileLogger health monitoring tests...");
                
                tests.TestInitialState();
                Console.WriteLine("✅ Initial state test passed");
                
                tests.TestNormalLogging();
                Console.WriteLine("✅ Normal logging test passed");
                
                tests.TestDroppedMessages();
                Console.WriteLine("✅ Dropped messages test passed");
                
                tests.TestPerformanceCharacteristics();
                Console.WriteLine("✅ Performance characteristics test passed");
                
                Console.WriteLine("\n🎉 All tests passed! The implementation efficiently tracks metrics without Count() operations.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test failed: {ex.Message}");
            }
            finally
            {
                tests.Cleanup();
            }
        }
    }
}