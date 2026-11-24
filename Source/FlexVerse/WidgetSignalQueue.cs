// parents: @.frostline/graph/m-widget-engine.xml

using System.Collections.Generic;

namespace FlexVerse
{
    internal sealed class WidgetSignalQueue
    {
        private readonly Queue<SignalInfo> _signalQueue = new();

        public void Post(SignalInfo signal)
        {
            if (signal == null)
                return;
            Log.Debug("M-WIDGET-TREE", "signal.posted", ("signal", signal));
            _signalQueue.Enqueue(signal);
        }

        public IEnumerable<SignalInfo> ConsumeAllSignals()
        {
            if (_signalQueue.Count == 0)
            {
                yield break;
            }

            while (_signalQueue.Count > 0)
            {
                yield return _signalQueue.Dequeue();
            }
        }

        public void Clear() => _signalQueue.Clear();
    }
}
