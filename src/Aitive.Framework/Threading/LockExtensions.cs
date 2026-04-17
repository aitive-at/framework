namespace Aitive.Framework.Threading;

public static class LockExtensions
{
    public struct ReadLockHolder : IDisposable
    {
        private ReaderWriterLockSlim _lock;

        internal ReadLockHolder(ReaderWriterLockSlim @lock)
        {
            _lock = @lock;
        }

        public void Dispose()
        {
            _lock.ExitReadLock();
        }
    }

    public struct UpgradableReadLockHolder : IDisposable
    {
        private ReaderWriterLockSlim _lock;
        private bool _wasUpgraded;

        internal UpgradableReadLockHolder(ReaderWriterLockSlim @lock)
        {
            _lock = @lock;
            _wasUpgraded = false;
        }

        public WriteLockHolder Upgrade()
        {
            _lock.EnterWriteLock();
            _wasUpgraded = true;
            return new WriteLockHolder(_lock);
        }

        public void Dispose()
        {
            if (!_wasUpgraded)
            {
                _lock.ExitUpgradeableReadLock();
            }
        }
    }

    public struct WriteLockHolder : IDisposable
    {
        private ReaderWriterLockSlim _lock;

        internal WriteLockHolder(ReaderWriterLockSlim @lock)
        {
            _lock = @lock;
        }

        public void Dispose()
        {
            _lock.ExitWriteLock();
        }
    }

    extension(ReaderWriterLockSlim @lock)
    {
        public ReadLockHolder AcquireReadLock()
        {
            @lock.EnterReadLock();
            return new ReadLockHolder(@lock);
        }

        public WriteLockHolder AcquireWriteLock()
        {
            @lock.EnterWriteLock();
            return new WriteLockHolder(@lock);
        }

        public UpgradableReadLockHolder AcquireUpgradableReadLock()
        {
            @lock.EnterUpgradeableReadLock();
            return new UpgradableReadLockHolder(@lock);
        }
    }
}
