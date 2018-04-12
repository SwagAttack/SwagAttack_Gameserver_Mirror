using System;

namespace Application.Interfaces
{
    public interface ITimer
    {
        event EventHandler TickEvent;
        event EventHandler ExpiredEvent;
        void StartWithSeconds(int seconds);
        void StartWithMinutes(int minutes);
        void Start(int minutes, int seconds);
        void Stop();
    }
}