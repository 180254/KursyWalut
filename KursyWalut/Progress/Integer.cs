namespace KursyWalut.Progress
{
    internal class Integer
    {
        private readonly object _syncLock = new object();

        private int _value;

        public Integer(int value)
        {
            _value = value;
        }

        public int Get()
        {
            lock (_syncLock)
            {
                return _value;
            }
        }

        public int Set(int value)
        {
            lock (_syncLock)
            {
                return _value = value;
            }
        }

        public int Increment(int incr)
        {
            lock (_syncLock)
            {
                return _value += incr;
            }
        }

        public int Decrement(int decr)
        {
            lock (_syncLock)
            {
                return _value -= decr;
            }
        }
    }
}