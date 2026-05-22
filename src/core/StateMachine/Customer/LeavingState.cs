using System;
using Godot;
using dd2d.core;

namespace dd2d.core.StateMachine.Customer
{
    public partial class LeavingState : Node
    {
        private Node2D _entity;
        private AnimationPlayer _animationPlayer;
        private float _speed;
        private Vector2[] _path = Array.Empty<Vector2>();
        private int _pathIndex = 0;
        private Action _onArrived;
        private bool _finished = false;

		public void Init(Node2D entity, float speed, Vector2[] path, Action onArrived = null)
		{
			_entity = entity;
			_animationPlayer = entity.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
			_speed = speed;
			_path = path;
			_onArrived = onArrived;
			Log.Debug($"Init — path length: {path.Length}", "LeavingState");
        }

        private void PlayAnimation(Vector2 direction)
        {
            if (_animationPlayer == null) return;
            string anim;
            if (direction == Vector2.Zero)
                anim = core.AnimationKeys.Idle;
            else if (Mathf.Abs(direction.X) >= Mathf.Abs(direction.Y))
                anim = direction.X > 0 ? core.AnimationKeys.WalkRight : core.AnimationKeys.WalkLeft;
            else
                anim = direction.Y > 0 ? core.AnimationKeys.WalkForward : core.AnimationKeys.WalkBack;

            if (_animationPlayer.CurrentAnimation != anim)
                _animationPlayer.Play(anim);
        }

        public override void _Process(double delta)
        {
            if (_finished) return;

            if (_path.Length == 0 || _pathIndex >= _path.Length)
			{
				_finished = true;
				PlayAnimation(Vector2.Zero);
				Log.Info("Visitor has left", "LeavingState");
                var cb = _onArrived;
                _onArrived = null;
                QueueFree();
                cb?.Invoke();
                return;
            }
            Vector2 nextPos = _path[_pathIndex];
            Vector2 direction = (nextPos - _entity.GlobalPosition).Normalized();
            float distance = _speed * (float)delta;
            PlayAnimation(direction);
            if (_entity.GlobalPosition.DistanceTo(nextPos) > distance)
                _entity.GlobalPosition += direction * distance;
            else
            {
                _entity.GlobalPosition = nextPos;
                _pathIndex++;
            }
        }
    }
}

