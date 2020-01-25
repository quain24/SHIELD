using System;
using System.Threading;

namespace Shield.Extensions
{   
    public static class ReaderWriterLockSlimExtensions
    {
        private sealed class ReadLockToken : IDisposable
        {
            private ReaderWriterLockSlim _sync;

            public ReadLockToken(ReaderWriterLockSlim sync)
            {
                _sync = sync;
                sync.EnterReadLock();
            }

            public void Dispose()
            {
                if (_sync != null)
                {
                    _sync.ExitReadLock();
                    _sync = null;
                }
            }
        }

        private sealed class WriteLockToken : IDisposable
        {
            private ReaderWriterLockSlim _sync;

            public WriteLockToken(ReaderWriterLockSlim sync)
            {
                _sync = sync;
                sync.EnterWriteLock();
            }

            public void Dispose()
            {
                if (_sync != null)
                {
                    _sync.ExitWriteLock();
                    _sync = null;
                }
            }
        }

        /// <summary>
        /// Extension method simplifying usage of <see cref="ReaderWriterLockSlim.EnterReadLock"/> by removing need for
        /// try-catch block by replacing it with 'using' statement.
        /// </summary>
        public static IDisposable Read(this ReaderWriterLockSlim obj) => new ReadLockToken(obj);

        /// <summary>
        /// Extension method simplifying usage of <see cref="ReaderWriterLockSlim.EnterWriteLock"/> by removing need for
        /// try-catch block by replacing it with 'using' statement.
        /// </summary>
        public static IDisposable Write(this ReaderWriterLockSlim obj) => new WriteLockToken(obj);
    }
}