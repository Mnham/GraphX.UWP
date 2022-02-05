using System;
using System.Threading;

namespace GraphX
{
    public class BAutoResetEvent : BWaitHandle, IDisposable
    {
        private AutoResetEvent _are;

        public BAutoResetEvent(bool initialState)
        {
            _are = new AutoResetEvent(initialState);
        }

        public bool Reset()
        {
            return _are.Reset();
        }

        public bool Set()
        {
            return _are.Set();
        }

        protected override void OnSuccessfullWait()
        {
            // nothing special needed
        }

        public override bool WaitOne()
        {
            throw new NotImplementedException();
        }

        public override bool WaitOne(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public override bool WaitOne(int millisecondsTimeout)
        {
            throw new NotImplementedException();
        }

        internal override WaitHandle WaitHandle => _are;

        public void Dispose()
        {
            if (_are != null)
            {
                _are.Dispose();
                _are = null;
            }
        }
    }
}