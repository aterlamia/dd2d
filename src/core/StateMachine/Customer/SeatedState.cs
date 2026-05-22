using System;
using Godot;
using dd2d.restaurant.table;
using dd2d.core;

namespace dd2d.core.StateMachine.Customer
{
	public partial class SeatedState : Node
	{
		private Node2D _entity;
		private Action _onSeated;
		private AnimationPlayer _animationPlayer;
		private ISeatingSpot _seat;

		public void Init(Node2D entity, ISeatingSpot seat, Action onSeated)
		{
			_entity = entity;
			_seat = seat;
			_onSeated = onSeated;
			_animationPlayer = entity.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
			Log.Debug("Sitting down", "SeatedState");

			if (_animationPlayer != null && _animationPlayer.HasAnimation(core.AnimationKeys.Sit))
			{
				_animationPlayer.Connect(
					AnimationMixer.SignalName.AnimationFinished,
					Callable.From<StringName>(OnAnimationFinished),
					(uint)GodotObject.ConnectFlags.OneShot);
				_animationPlayer.Play(core.AnimationKeys.Sit);
			}
			else
			{
				CallDeferred(nameof(Finish));
			}
		}

		private void OnAnimationFinished(StringName animName)
		{
			if (animName == core.AnimationKeys.Sit)
				Finish();
		}

		private void Finish()
		{
			Log.Debug("Now seated, starting wait", "SeatedState");
			if (_seat != null)
				_seat.IsOccupied = true;
			var cb = _onSeated;
			_onSeated = null;
			QueueFree();
			cb?.Invoke();
		}
	}
}
