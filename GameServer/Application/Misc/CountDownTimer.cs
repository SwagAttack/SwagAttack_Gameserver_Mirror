using System;
using System.Timers;
using Application.Interfaces;


namespace Application.Misc
{
    public class CountDownTimer : ITimer
    {
        public event EventHandler TickEvent;
        public event EventHandler ExpiredEvent;

        private readonly System.Timers.Timer _timeoutTimer;
        private int _timeRemainingInSeconds;

        public CountDownTimer()
        {
            _timeoutTimer = new System.Timers.Timer();

            _timeoutTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            _timeoutTimer.Interval = 1000;
            _timeoutTimer.AutoReset = true;
        }
        public void StartWithSeconds(int seconds)
        {
            _timeRemainingInSeconds = seconds;
            _timeoutTimer.Enabled = true;
        }

        public void StartWithMinutes(int minutes)
        {
            StartWithSeconds(minutes * 60);
        }

        public void Start(int minutes, int seconds)
        {
            _timeRemainingInSeconds = (minutes * 60) + seconds;
            _timeoutTimer.Enabled = true;
        }

        public void Stop()
        {
            _timeoutTimer.Enabled = false;
        }

        private void Expire()
        {
            _timeoutTimer.Enabled = false;
            ExpiredEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnTimerEvent(object sender, ElapsedEventArgs args)
        {
            _timeRemainingInSeconds -= 1;
            TickEvent?.Invoke(this, EventArgs.Empty);

            if(_timeRemainingInSeconds <= 0)
                Expire();               
        }
    }
}