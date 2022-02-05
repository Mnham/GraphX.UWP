using System;
using System.Threading;

namespace GraphX
{
    public class BManualResetEvent : BWaitHandle, IDisposable
    {
        private ManualResetEvent _mre;

        public BManualResetEvent(bool initialState)
        {
            _mre = new ManualResetEvent(initialState);
        }

        public bool Reset()
        {
            return _mre.Reset();
        }

        public bool Set()
        {
            return _mre.Set();
        }

        protected override void OnSuccessfullWait()
        {
            // nothing special needed
        }

        public override bool WaitOne()
        {
            return _mre.WaitOne();
        }

        public override bool WaitOne(TimeSpan timeout)
        {
            return _mre.WaitOne(timeout);
        }

        public override bool WaitOne(int millisecondsTimeout)
        {
            return _mre.WaitOne(millisecondsTimeout);
        }

        internal override WaitHandle WaitHandle => _mre;

        public void Dispose()
        {
            if (_mre != null)
            {
                _mre.Dispose();
                _mre = null;
            }
        }
    }
}