using System;

namespace Application.Interfaces
{
    public interface ITimer
    {
        event EventHandler TickEvent;
        event EventHandler ExpiredEvent;
        void Start(int seconds);
        void Stop();
    }
}