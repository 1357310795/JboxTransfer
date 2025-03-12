using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SlidingWindowEventDetector
    {
        public static SlidingWindowEventDetector Default { get; } = new SlidingWindowEventDetector();
        private readonly Queue<DateTime> _eventTimes = new Queue<DateTime>();
        private readonly object _lock = new object();
        private readonly TimeSpan _windowSize = TimeSpan.FromMinutes(60);
        private const int _threshold = 10;

        private bool _state;
        public bool State
        {
            get { lock (_lock) return _state; }
            private set { lock (_lock) _state = value; }
        }

        public void RecordEvent()
        {
            lock (_lock)
            {
                var now = DateTime.Now;

                // 1. 添加新事件时间
                _eventTimes.Enqueue(now);

                // 2. 移除超出时间窗口的旧事件
                while (_eventTimes.Count > 0 && now - _eventTimes.Peek() > _windowSize)
                {
                    _eventTimes.Dequeue();
                }

                // 3. 更新状态
                State = _eventTimes.Count >= _threshold;
            }
        }

        // 可选：主动清理方法（如果长时间没有新事件时可以调用）
        public void CleanOldEvents()
        {
            lock (_lock)
            {
                var now = DateTime.Now;
                while (_eventTimes.Count > 0 && now - _eventTimes.Peek() > _windowSize)
                {
                    _eventTimes.Dequeue();
                }
                State = _eventTimes.Count >= _threshold;
            }
        }
    }
}
