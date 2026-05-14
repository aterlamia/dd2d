using System;
using Godot;

namespace dd2d.core.StateMachine.Customer
{
    public partial class SeatedState : Node
    {
        private restaurant.customer.Visitor _visitor;
        private float _waitTime;
        private Action _onWaitFinished;
        private Timer _waitTimer;

        public void Init(restaurant.customer.Visitor visitor, float waitTime, Action onWaitFinished)
        {
            _visitor = visitor;
            _waitTime = waitTime;
            _onWaitFinished = onWaitFinished;
            _waitTimer = new Timer();
            _waitTimer.WaitTime = _waitTime;
            _waitTimer.OneShot = true;
            _waitTimer.Timeout += OnWaitTimerTimeout;
            AddChild(_waitTimer);
            _waitTimer.Start();
        }

        private void OnWaitTimerTimeout()
        {
            _onWaitFinished?.Invoke();
            QueueFree();
        }
    }
}

