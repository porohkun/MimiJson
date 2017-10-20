using System;
using System.Threading;
using System.ComponentModel;
using Timer = System.Timers.Timer;

namespace MimiJson
{
    public class AsyncParser : Component
    {
        public event EventHandler ParseStarted;
        public event EventHandler ParseCompleted;

        [DefaultValue(100)]
        public int CheckTick { get { return _CheckTick; } set { _CheckTick = value; } }
        private int _CheckTick = 100;

        [DefaultValue(1000)]
        public int WaitTick { get { return _WaitTick; } set { _WaitTick = value; } }
        private int _WaitTick = 1000;

        public JsonValue Value { get { return _Value; } }
        private JsonValue _Value = new JsonValue(JsonValueType.Null);

        private string Text = "";
        private EventWaitHandle go = new AutoResetEvent(false);
        private AsyncOperation asyncOperation;
        private readonly SendOrPostCallback operationStarted;
        private readonly SendOrPostCallback operationCompleted;

        private Thread Thread;
        private Thread Timer;

        private bool WantParse = false;

        private DateTime LastParse;

        public AsyncParser()
        {
            asyncOperation = AsyncOperationManager.CreateOperation(null);
            operationStarted = new SendOrPostCallback(AsyncOperationStarted);
            operationCompleted = new SendOrPostCallback(AsyncOperationCompleted);

            Thread = new Thread(Thread_work);

            LastParse = DateTime.Now;

            Timer = new Thread(Tick);
            Timer.IsBackground = true;
            Timer.Priority = ThreadPriority.Lowest;
            Timer.Start();
        }

        void Tick()
        {
            while (true)
            {
                var tick = DateTime.Now;

                var interval = tick - LastParse;

                if (interval.TotalMilliseconds > WaitTick)
                {
                    if (WantParse)
                    {
                        WantParse = false;
                        StartParse();
                    }
                }
                Thread.Sleep(CheckTick);
            }
        }

        public void Parse(string text, bool immediatly = false)
        {
            Thread.Abort();
            LastParse = DateTime.Now;
            Text = text;
            if (immediatly)
                StartParse();
            else
                WantParse = true;
        }

        private void StartParse()
        {
            Thread = new Thread(Thread_work);
            Thread.IsBackground = true;
            Thread.Priority = ThreadPriority.Lowest;
            Thread.Start(Text);
        }

        private void Thread_work(object text)
        {
            this.asyncOperation.Post(operationStarted, new EventArgs());

            var p = 0;

            var jval = new JsonValue(JsonValueType.Null);
            try
            {
                jval = JsonValue.Parse((string)text);
            }
            catch { }

            if (jval != null)
            {
                lock (_Value)
                {
                    _Value = jval;
                }
            }

            this.asyncOperation.Post(operationCompleted, new EventArgs());
        }

        private void AsyncOperationStarted(object arg)
        {
            if (ParseStarted != null)
            {
                ParseStarted(this, (EventArgs)arg);
            }
        }

        private void AsyncOperationCompleted(object arg)
        {
            if (ParseCompleted != null)
            {
                ParseCompleted(this, (EventArgs)arg);
            }
        }

    }
}
