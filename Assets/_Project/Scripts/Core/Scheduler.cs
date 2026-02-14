using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public class Scheduler : MonoBehaviour
    {
        private static Scheduler _instance;
        private static Scheduler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("[Scheduler]").AddComponent<Scheduler>();
                }
                return _instance;
            }
        }

        private List<Schedule> _schedules = new List<Schedule>();
        private List<Schedule> _removed = new List<Schedule>();
        private List<Schedule> _added = new List<Schedule>();

        public static Schedule Create()
        {
            return new Schedule(Instance);
        }

        public static Schedule Invoke(System.Action callback, float delay = 0f)
        {
            return Create().Wait(delay).Append(callback);
        }

        internal void Register(Schedule seq)
        {
            _added.Add(seq);
        }

        internal void Unregister(Schedule seq)
        {
            _removed.Add(seq);
        }

        public void Update()
        {
            foreach (var s in _added)
            {
                if (!_schedules.Contains(s))
                {
                    _schedules.Add(s);
                }
            }
            _added.Clear();

            foreach (var s in _removed)
            {
                _schedules.Remove(s);
            }
            _removed.Clear();

            foreach (var s in _schedules)
            {
                s.Tick();
            }
        }
    }

    [System.Serializable]
    public class Schedule
    {
        private float _repeatInterval, _repeatTimer, _delay, _delayTimer, _actionDuration, _actionTimer;
        private int _repeatCount, _currentRepeatCount;
        private bool _actionInvoked, _paused, _ignoreTimeScale = true;

        private List<System.Func<float>> _actions = new List<System.Func<float>>();
        private List<System.Func<float>> _actionsComplete = new List<System.Func<float>>();
        private System.Action<float> _onUpdate = null!;
        private System.Func<bool> _condition = null!;

        Scheduler _owner;

        public bool IsAlive { get; private set; }

        public Schedule(Scheduler owner)
        {
            _owner = owner;
            _owner.Register(this);
            IsAlive = true;
        }

        public Schedule Invoke(System.Action callback, float delay = 0)
        {
            Wait(delay);
            Append(callback);
            return this;
        }

        public Schedule Append(Schedule s)
        {
            foreach (var action in s._actions)
            {
                _actions.Add(action);
            }
            s.Kill();
            return this;
        }

        public Schedule Append(System.Action callback, float duration = 0)
        {
            _actions.Add(() =>
            {
                callback?.Invoke();
                return duration;
            });
            return this;
        }

        public Schedule Wait(System.Func<bool> condition)
        {
            Append(() => _condition = condition);
            return this;
        }

        public Schedule Wait(System.Func<float> callback)
        {
            _actions.Add(callback);
            return this;
        }

        public Schedule Wait(float duration)
        {
            _actions.Add(() => duration);
            return this;
        }

        public void SetIgnoreTimeScale(bool ignoreTimeScale)
        {
            _ignoreTimeScale = ignoreTimeScale;
        }

        public Schedule SetOnUpdate(System.Action<float> action)
        {
            _onUpdate = action;
            return this;
        }

        public Schedule SetDelay(float delay)
        {
            _delay = delay;
            return this;
        }

        public Schedule SetRepeat(int repeatCount = -1, float repeatInterval = 0)
        {
            _currentRepeatCount = repeatCount;
            _repeatTimer = repeatInterval;
            return this;
        }

        public Schedule SetPaused(bool paused = true)
        {
            _paused = paused;
            return this;
        }

        public Schedule Restart()
        {
            Reset();
            SetPaused(false);
            _owner.Register(this);
            IsAlive = true;
            return this;
        }

        public void End()
        {
            _delay = 0f;
        }

        public void Kill()
        {
            _owner.Unregister(this);
            IsAlive = false;
        }

        internal void Tick()
        {
            if (_paused)
            {
                return;
            }

            if (_condition != null)
            {
                if (_condition.Invoke())
                {
                    _condition = null!;
                }
                else
                {
                    return;
                }
            }

            var delta = _ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

            if (_delay > 0)
            {
                _delayTimer += delta;
                if (_delayTimer >= _delay)
                {
                    _delayTimer = _delay = 0;
                }
                else
                {
                    return;
                }
            }

            if (_actions.Count > 0)
            {
                var current = _actions[0];

                if (!_actionInvoked)
                {
                    _actionDuration = current.Invoke();
                    _actionInvoked = true;
                }

                _actionTimer += delta;
                if (_actionTimer >= _actionDuration)
                {
                    _actions.Remove(current);
                    _actionsComplete.Add(current);
                    ResetFlags();
                }
            }
            else
            {
                if (_currentRepeatCount != 0)
                {
                    if (_currentRepeatCount == -1 || (_repeatCount < _currentRepeatCount))
                    {
                        _repeatInterval += delta;

                        if (_repeatInterval >= _repeatTimer)
                        {
                            _repeatCount++;
                            _repeatInterval = 0;

                            ResetActions();
                        }

                        return;
                    }
                }

                Kill();
            }

            _onUpdate?.Invoke(delta);
        }

        private void Reset()
        {
            ResetFlags();
            ResetActions();
            _repeatInterval = _delayTimer = _repeatCount = 0;
            _paused = false;
        }

        private void ResetActions()
        {
            for (int i = _actionsComplete.Count - 1; i >= 0; i--)
            {
                _actions.Insert(0, _actionsComplete[i]);
            }
            _actionsComplete.Clear();
        }

        private void ResetFlags()
        {
            _actionInvoked = false;
            _actionTimer = 0;
        }
    }
}