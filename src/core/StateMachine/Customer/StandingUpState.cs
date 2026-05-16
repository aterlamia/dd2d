using System;
using Godot;

namespace dd2d.core.StateMachine.Customer
{
    public partial class StandingUpState : Node
    {
        private Node2D _entity;
        private Vector2 _stepToPosition;
        private Action _onStoodUp;
        private AnimationPlayer _animationPlayer;

        public void Init(Node2D entity, Vector2 stepToPosition, Action onStoodUp)
        {
            _entity = entity;
            _onStoodUp = onStoodUp;
            _stepToPosition = stepToPosition;
            _animationPlayer = entity.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
            GD.Print("[StandingUpState] Standing up");

            if (_animationPlayer != null && _animationPlayer.HasAnimation(core.AnimationKeys.StandUp))
            {
                _animationPlayer.Connect(
                    AnimationMixer.SignalName.AnimationFinished,
                    Callable.From<StringName>(OnAnimationFinished),
                    (uint)GodotObject.ConnectFlags.OneShot);
                _animationPlayer.Play(core.AnimationKeys.StandUp);
            }
            else
            {
                CallDeferred(nameof(Finish));
            }
        }

        private void OnAnimationFinished(StringName animName)
        {
            if (animName == core.AnimationKeys.StandUp)
                Finish();
        }

        private void Finish()
        {
            GD.Print($"[StandingUpState] Stood up, stepping to {_stepToPosition}");
            _entity.GlobalPosition = _stepToPosition;
            var cb = _onStoodUp;
            _onStoodUp = null;
            QueueFree();
            cb?.Invoke();
        }
    }
}
