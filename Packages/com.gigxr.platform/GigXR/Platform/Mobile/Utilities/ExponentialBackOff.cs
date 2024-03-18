using System;
using System.Diagnostics;

namespace GIGXR.Platform.Mobile.Utilities
{
    /// <summary>
    /// Provides an exponential back-off strategy.
    ///
    /// Usage:
    /// <code>
    /// var backOff = new ExponentialBackOff();
    /// var nextDelay = backOff.NextDelay();
    /// if (nextDelay == null)
    /// {
    ///     // Give up.
    ///     return;
    /// }
    ///
    /// // Schedule a retry after waiting per the value of nextDelay.
    /// // On success: call backOff.Reset();
    /// // On failure: call backOff.NextDelay(); again and repeat
    /// </code>
    ///
    /// The default configuration will retry after:
    ///  - 0 seconds
    ///  - 1 second
    ///  - 3 seconds
    ///  - 7 seconds
    ///  - 15 seconds
    ///  - 30 seconds (max per default `MaxRetryDelay`)
    /// </summary>
    public class ExponentialBackOff
    {
        /// <summary>
        /// The exponential back-off config.
        /// </summary>
        public ExponentialBackOffConfig Config { get; }

        /// <summary>
        /// The number of retries executed against this strategy.
        /// </summary>
        public int RetryCount { get; private set; } = 0;

        /// <summary>
        /// A stopwatch used to circuit break based upon a max retry duration.
        /// </summary>
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public ExponentialBackOff(ExponentialBackOffConfig config)
        {
            Config = config;
        }

        public ExponentialBackOff() : this(new ExponentialBackOffConfig())
        {
        }

        /// <summary>
        /// Resets this exponential back-off strategy.
        /// </summary>
        public void Reset()
        {
            _stopwatch.Reset();
            RetryCount = 0;
        }

        /// <summary>
        /// Determines whether an operation should be retried and the delay before the next attempt.
        /// 
        /// Internally it will increment `RetryCount`.
        /// </summary>
        /// <returns>The delay until the next retry or null for no more retries.</returns>
        public TimeSpan? NextDelay()
        {
            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
            }

            if (RetryCount < Config.MaxRetryCount && _stopwatch.Elapsed < Config.MaxRetryDuration)
            {
                var delta = Math.Pow(Config.ExponentialBase, RetryCount) - 1;
                var delay = Math.Min(
                    Config.Coefficient.TotalMilliseconds * delta,
                    Config.MaxRetryDelay.TotalMilliseconds);
                RetryCount++;

                return TimeSpan.FromMilliseconds(delay);
            }

            return null;
        }
    }

    /// <summary>
    /// An object used to configure an `ExponentialBackOff` strategy.
    /// </summary>
    public class ExponentialBackOffConfig
    {
        /// <summary>
        /// The maximum retries permitted before giving up.
        /// </summary>
        public int MaxRetryCount { get; set; } = 5;

        /// <summary>
        /// The maximum delay between retries.
        /// </summary>
        public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// The maximum duration of retrying before giving up.
        /// </summary>
        public TimeSpan MaxRetryDuration { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// The base used for the exponential function used to compute the delay between retries, must be positive.
        /// </summary>
        public double ExponentialBase { get; set; } = 2;

        /// <summary>
        /// The coefficient for the exponential function used to compute the delay between retires, must be nonnegative.
        /// </summary>
        public TimeSpan Coefficient { get; set; } = TimeSpan.FromSeconds(1);
    }
}