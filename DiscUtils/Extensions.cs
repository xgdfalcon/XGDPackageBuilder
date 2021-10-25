using System;

namespace DiscUtils
{
    /// <summary>
    ///     DateTimeOffset extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     The Epoch common to most (all?) Unix systems.
        /// </summary>
        public static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///     Converts the current Unix time to a DateTimeOffset.
        /// </summary>
        /// <param name="seconds">Seconds since UnixEpoch.</param>
        /// <returns>DateTimeOffset.</returns>
        public static DateTimeOffset FromUnixTimeSeconds(this long seconds)
        {
#if NETSTANDARD
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
#else
            var dateTimeOffset = new DateTimeOffset(UnixEpoch);
            dateTimeOffset = dateTimeOffset.AddSeconds(seconds);
            return dateTimeOffset;
#endif
        }

        /// <summary>
        ///     Converts the current DateTimeOffset to Unix time.
        /// </summary>
        /// <param name="dateTimeOffset">DateTimeOffset.</param>
        /// <returns>Seconds since UnixEpoch.</returns>
        public static long ToUnixTimeSeconds(this DateTimeOffset dateTimeOffset)
        {
            var unixTimeStampInTicks = (dateTimeOffset.ToUniversalTime() - UnixEpoch).Ticks;
            return unixTimeStampInTicks / TimeSpan.TicksPerSecond;
        }
    }
}