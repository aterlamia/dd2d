using Godot;

namespace dd2d.player
{
    public partial class Character : CharacterBody2D
    {
        public const float Speed = 100.0f;

        private AnimationPlayer _animationPlayer;
        private string _lastAnimation = "";

        public override void _Ready()
        {
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector2 velocity = Velocity;

            Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            velocity = direction * Speed;
            UpdateAnimation(direction);
            Velocity = velocity;
            MoveAndSlide();
        }

        private void UpdateAnimation(Vector2 direction)
        {
            string anim;

            if (direction == Vector2.Zero)
                anim = "idle";
            else if (direction.Y > 0)
                anim = "WalkF";
            else if (direction.Y < 0)
                anim = "WalkB";
            else if (direction.X < 0)
                anim = "WalkL";
            else if (direction.X > 0)
                anim = "WalkR";
            else
                anim = "idle";

            if (anim != _lastAnimation)
            {
                _lastAnimation = anim;
                _animationPlayer.Play(anim);
            }
        }
    }
}

