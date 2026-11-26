using System;

namespace Deadzone.Buildings
{
    /// <summary>
    /// Single source of time for production and construction. The prototype uses the device clock;
    /// a multiplayer build swaps in a server-synchronised implementation without touching gameplay code.
    /// </summary>
    public interface IGameClock
    {
        long UtcNowSeconds { get; }
    }

    public sealed class SystemGameClock : IGameClock
    {
        public long UtcNowSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
