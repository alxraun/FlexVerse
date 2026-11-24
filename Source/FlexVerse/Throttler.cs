// parents: @.frostline/graph/m-behavior-updater.xml

using UnityEngine;

namespace FlexVerse
{
    /// <summary>Rate-limits work against a target frame interval. (M-BEHAVIOR-UPDATER)</summary>
    public class Throttler
    {
        private readonly float _updateInterval;
        private float _timeSinceLastUpdate;
        private bool _forceNextUpdate;

        /// <summary>Configures the throttler from a target frame rate.</summary>
        public Throttler(float targetFrameRate, bool executeImmediatelyFirstTime = true)
        {
            if (targetFrameRate <= 0)
            {
                _updateInterval = 0f;
            }
            else
            {
                _updateInterval = 1.0f / targetFrameRate;
            }

            if (executeImmediatelyFirstTime)
            {
                _timeSinceLastUpdate = _updateInterval;
            }
            else
            {
                _timeSinceLastUpdate = 0f;
            }
            _forceNextUpdate = false;
        }

        /// <summary>Accumulates elapsed unscaled time for the interval check.</summary>
        public void Tick()
        {
            _timeSinceLastUpdate += Time.unscaledDeltaTime;
        }

        /// <summary>Reports whether the interval has elapsed or an update is forced.</summary>
        public bool ShouldUpdate()
        {
            if (_forceNextUpdate)
            {
                return true;
            }

            return _timeSinceLastUpdate >= _updateInterval;
        }

        /// <summary>Resets the elapsed time after a completed update.</summary>
        public void DidUpdate()
        {
            _timeSinceLastUpdate = 0f;
            _forceNextUpdate = false;
        }

        /// <summary>Forces the next interval check to pass.</summary>
        public void ForceNextUpdate()
        {
            _forceNextUpdate = true;
        }
    }
}
