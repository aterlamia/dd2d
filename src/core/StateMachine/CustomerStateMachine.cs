using System;
using Godot;

namespace dd2d.core.StateMachine
{
    public enum CustomerStateType { Idle, Walking, Seated, Leaving }

    public partial class CustomerStateMachine : StateMachine
    {
        public CustomerStateType CurrentState { get; private set; } = CustomerStateType.Idle;

        private restaurant.customer.Visitor _visitor;
        private Node _activeState;

        public void Init(restaurant.customer.Visitor visitor)
        {
            _visitor = visitor;
        }

        public void StartWalking(float speed, Vector2[] path, Action onArrived)
        {
            ClearActiveState();
            CurrentState = CustomerStateType.Walking;
            var state = new Customer.WalkingState();
            _activeState = state;
            AddChild(state);
            state.Init(_visitor, speed, path, onArrived);
        }

        public void StartSeated(float waitTime, Action onDone)
        {
            ClearActiveState();
            CurrentState = CustomerStateType.Seated;
            var state = new Customer.SeatedState();
            _activeState = state;
            AddChild(state);
            state.Init(_visitor, waitTime, onDone);
        }

        public void StartLeaving(float speed, Vector2[] path, Action onArrived = null)
        {
            ClearActiveState();
            CurrentState = CustomerStateType.Leaving;
            var state = new Customer.WalkingState();
            _activeState = state;
            AddChild(state);
            state.Init(_visitor, speed, path, onArrived);
        }

        private void ClearActiveState()
        {
            if (_activeState != null && IsInstanceValid(_activeState))
            {
                _activeState.QueueFree();
                _activeState = null;
            }
            CurrentState = CustomerStateType.Idle;
        }
    }
}
