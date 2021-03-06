﻿using System;

namespace Application.Interfaces
{
    /* Timer utiliy class */
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