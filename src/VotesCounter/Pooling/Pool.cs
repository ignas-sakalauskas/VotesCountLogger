using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace VotesCounter
{
    public class Pool<T> : IStatisticWriter, IDisposable where T : IStatisticWriter
    {
        private bool isDisposed;
        private readonly Func<Pool<T>, T> factory;
        private readonly LoadingMode loadingMode;
        private readonly IItemStore<T> itemStore;
        private readonly int size;
        private int count;
        private readonly Semaphore sync;
        private readonly AccessMode accessMode;

        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public Pool(int size, Func<Pool<T>, T> factory, LoadingMode loadingMode, AccessMode accessMode)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size", size, "Argument 'size' must be greater than zero.");
            }

            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            this.size = size;
            this.factory = factory;
            sync = new Semaphore(size, size);
            this.loadingMode = loadingMode;
            this.accessMode = accessMode;
            itemStore = CreateItemStore(accessMode, size);

            if (loadingMode == LoadingMode.Eager)
            {
                PreloadItems();
            }
        }

        public T Acquire()
        {
            sync.WaitOne();

            switch (loadingMode)
            {
                case LoadingMode.Eager:
                    return AcquireEager();

                case LoadingMode.Lazy:
                    return AcquireLazy();

                case LoadingMode.LazyExpanding:
                    return AcquireLazyExpanding();

                default:
                    throw new ArgumentOutOfRangeException(nameof(loadingMode), loadingMode, "Unkown loading mode value");
            }
        }

        public void Release(T item)
        {
            lock (itemStore)
            {
                itemStore.Store(item);
            }
            sync.Release();
        }

        public void WriteStatistic(StringBuilder sb)
        {
            lock (itemStore)
            {
                sb.AppendLine("Pool size: " + size);
                sb.AppendLine("Pool count: " + count);
                sb.AppendLine("Pool loading mode: " + loadingMode);
                sb.AppendLine("Pool access mode: " + accessMode);
                sb.AppendLine("[Pool items]");
                itemStore.WriteStatistic(sb);
            }
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
            {
                lock (itemStore)
                {
                    while (itemStore.Count > 0)
                    {
                        IDisposable disposable = (IDisposable)itemStore.Fetch();
                        disposable.Dispose();
                    }
                }
            }

            sync.Close();
        }

        private T AcquireEager()
        {
            lock (itemStore)
            {
                return itemStore.Fetch();
            }
        }

        private T AcquireLazy()
        {
            lock (itemStore)
            {
                if (itemStore.Count > 0)
                {
                    return itemStore.Fetch();
                }
            }
            Interlocked.Increment(ref count);
            return factory(this);
        }

        private T AcquireLazyExpanding()
        {
            bool shouldExpand = false;
            if (count < size)
            {
                int newCount = Interlocked.Increment(ref count);
                if (newCount <= size)
                {
                    shouldExpand = true;
                }
                else
                {
                    // Another thread took the last spot - use the store instead
                    Interlocked.Decrement(ref count);
                }
            }
            if (shouldExpand)
            {
                return factory(this);
            }
            else
            {
                lock (itemStore)
                {
                    return itemStore.Fetch();
                }
            }
        }

        private void PreloadItems()
        {
            for (int i = 0; i < size; i++)
            {
                T item = factory(this);
                itemStore.Store(item);
            }
            count = size;
        }

        private IItemStore<T> CreateItemStore(AccessMode mode, int capacity)
        {
            switch (mode)
            {
                case AccessMode.FIFO:
                    return new QueueStore<T>(capacity);

                case AccessMode.LIFO:
                    return new StackStore<T>(capacity);

                case AccessMode.Circular:
                    return new CircularStore<T>(capacity);

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown access mode");
            }
        }
    }
}