using System;
using Godot;

namespace dd2d.core.StateMachine.Player
{
    public enum PlayerStateType { Idle, Walking, Interacting }

    public partial class PlayerStateMachine : core.StateMachine.StateMachine
    {
        public PlayerStateType CurrentState { get; private set; } = PlayerStateType.Idle;

        private player.Character _character;
        private Node _activeState;

        public void Init(player.Character character)
        {
            _character = character;
            GD.Print("[PlayerStateMachine] Initialized");
            SetIdle();
        }

        public void SetIdle()
        {
            GD.Print($"[PlayerStateMachine] {CurrentState} → Idle");
            ClearActiveState();
            CurrentState = PlayerStateType.Idle;
            var state = new IdleState();
            _activeState = state;
            AddChild(state);
            state.Init(_character);
        }

        public void SetWalking()
        {
            GD.Print($"[PlayerStateMachine] {CurrentState} → Walking");
            ClearActiveState();
            CurrentState = PlayerStateType.Walking;
            var state = new WalkingState();
            _activeState = state;
            AddChild(state);
            state.Init(_character);
        }

        public void SetInteracting(Action onDone = null)
        {
            GD.Print($"[PlayerStateMachine] {CurrentState} → Interacting");
            ClearActiveState();
            CurrentState = PlayerStateType.Interacting;
            var state = new InteractingState();
            _activeState = state;
            AddChild(state);
            state.Init(_character, () =>
            {
                GD.Print("[PlayerStateMachine] Interaction done → Idle");
                onDone?.Invoke();
                SetIdle();
            });
        }

        private void ClearActiveState()
        {
            if (_activeState != null && IsInstanceValid(_activeState))
            {
                _activeState.QueueFree();
                _activeState = null;
            }
        }
    }
}

