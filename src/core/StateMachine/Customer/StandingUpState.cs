using System;
using Godot;
using dd2d.restaurant.table;
using dd2d.core;

namespace dd2d.core.StateMachine.Customer
{
    public partial class StandingUpState : Node
    {
        private Node2D _entity;
        private Vector2 _stepToPosition;
        private Action _onStoodUp;
        private AnimationPlayer _animationPlayer;
        private ISeatingSpot _seat;

		public void Init(Node2D entity, Vector2 stepToPosition, ISeatingSpot seat, Action onStoodUp)
		{
			_entity = entity;
			_onStoodUp = onStoodUp;
			_stepToPosition = stepToPosition;
			_seat = seat;
			_animationPlayer = entity.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
			Log.Debug("Standing up", "StandingUpState");

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
			Log.Debug($"Stood up, stepping to {_stepToPosition}", "StandingUpState");
            if (_seat != null)
                _seat.IsOccupied = false;
            _entity.GlobalPosition = _stepToPosition;
            var cb = _onStoodUp;
            _onStoodUp = null;
            QueueFree();
            cb?.Invoke();
        }
    }
}
