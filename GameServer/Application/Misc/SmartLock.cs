using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Misc
{
    /// <summary>
    /// Inspired by:
    /// https://blogs.msdn.microsoft.com/pfxteam/2012/02/12/building-async-coordination-primitives-part-6-asynclock/
    /// </summary>
    public class SmartLock
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly Task<Releaser> _releaserAsync;

        public SmartLock()
        {
            _semaphore = new SemaphoreSlim(1);
            _releaserAsync = Task.FromResult(new Releaser(this));
        }

        public Task<Releaser> LockAsync()
        {
            var wait = _semaphore.WaitAsync();
            return wait.IsCompleted
                ? _releaserAsync
                : wait.ContinueWith((_, state) => new Releaser((SmartLock) state), this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public Releaser Lock()
        {
             _semaphore.Wait();
             return new Releaser(this);
        }

        public struct Releaser : IDisposable
        {
            private readonly SmartLock _toRelease;

            internal Releaser(SmartLock toRelease)
            {
                _toRelease = toRelease;
            }
            public void Dispose()
            {
                _toRelease?._semaphore.Release();
            }
        }
    }
}