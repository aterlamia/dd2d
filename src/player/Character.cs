using Godot;

namespace dd2d.player
{
    public partial class Character : CharacterBody2D
    {
        public const float Speed = 100.0f;

        private core.StateMachine.Player.PlayerStateMachine _stateMachine;

        public override void _Ready()
        {
            _stateMachine = new core.StateMachine.Player.PlayerStateMachine();
            AddChild(_stateMachine);
            _stateMachine.Init(this);
        }

        public override void _Process(double delta)
        {
            if (_stateMachine.CurrentState == core.StateMachine.Player.PlayerStateType.Interacting)
                return;

            Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            if (direction != Vector2.Zero && _stateMachine.CurrentState != core.StateMachine.Player.PlayerStateType.Walking)
                _stateMachine.SetWalking();
            else if (direction == Vector2.Zero && _stateMachine.CurrentState != core.StateMachine.Player.PlayerStateType.Idle)
                _stateMachine.SetIdle();
        }

        public void Interact(System.Action onDone = null) => _stateMachine.SetInteracting(onDone);
    }
}

