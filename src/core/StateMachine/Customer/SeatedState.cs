using System;
using Godot;

namespace dd2d.core.StateMachine.Customer
{
	public partial class SeatedState : Node
	{
		private Node2D _entity;
		private Action _onSeated;
		private AnimationPlayer _animationPlayer;

		public void Init(Node2D entity, Action onSeated)
		{
			_entity = entity;
			_onSeated = onSeated;
			_animationPlayer = entity.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
			GD.Print("[SeatedState] Sitting down");

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
			GD.Print("[SeatedState] Now seated, starting wait");
			var cb = _onSeated;
			_onSeated = null;
			QueueFree();
			cb?.Invoke();
		}
	}
}
