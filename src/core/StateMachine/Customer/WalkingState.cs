using System;
using Godot;

namespace dd2d.core.StateMachine.Customer
{
    public partial class WalkingState : Node
    {
        private restaurant.customer.Visitor _visitor;
        private AnimationPlayer _animationPlayer;
        private float _speed;
        private Vector2[] _path = Array.Empty<Vector2>();
        private int _pathIndex = 0;
        private Action _onArrived;

        public void Init(restaurant.customer.Visitor visitor, float speed, Vector2[] path, Action onArrived)
        {
            _visitor = visitor;
            _animationPlayer = visitor.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
            _speed = speed;
            _path = path;
            _pathIndex = 0;
            _onArrived = onArrived;
        }

        private void PlayAnimation(Vector2 direction)
        {
            if (_animationPlayer == null) return;
            string anim;
            if (direction == Vector2.Zero)
                anim = "still";
            else if (Mathf.Abs(direction.X) >= Mathf.Abs(direction.Y))
                anim = direction.X > 0 ? "walkR" : "walkL";
            else
                anim = direction.Y > 0 ? "walkF" : "walkB";

            if (_animationPlayer.CurrentAnimation != anim)
                _animationPlayer.Play(anim);
        }

        public override void _Process(double delta)
        {
            if (_path.Length == 0 || _pathIndex >= _path.Length)
            {
                PlayAnimation(Vector2.Zero);
                _onArrived?.Invoke();
                QueueFree();
                return;
            }
            Vector2 nextPos = _path[_pathIndex];
            Vector2 direction = (nextPos - _visitor.GlobalPosition).Normalized();
            float distance = _speed * (float)delta;
            PlayAnimation(direction);
            if (_visitor.GlobalPosition.DistanceTo(nextPos) > distance)
            {
                _visitor.GlobalPosition += direction * distance;
            }
            else
            {
                _visitor.GlobalPosition = nextPos;
                _pathIndex++;
            }
        }
    }
}
