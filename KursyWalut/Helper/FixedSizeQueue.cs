using System.Collections.Concurrent;

namespace KursyWalut.Helper
{
    /// <summary>
    ///     "Fixed size queue which automatically dequeues old values upon new enques." <br />
    ///     Credits: davel @ stackoverflow.com <br />
    ///     http://stackoverflow.com/users/1351615/davel <br />
    ///     http://stackoverflow.com/questions/5852863/fixed-size-queue-which-automatically-dequeues-old-values-upon-new-enques
    /// </summary>
    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        private readonly object _syncObject = new object();

        public FixedSizedQueue(int size)
        {
            Size = size;
        }

        public int Size { get; }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            lock (_syncObject)
            {
                while (Count > Size)
                {
                    T outObj;
                    TryDequeue(out outObj);
                }
            }
        }
    }
}