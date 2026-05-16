using Godot;

namespace dd2d.core.StateMachine.Player
{
    public partial class WalkingState : Node
    {
        private player.Character _character;
        private AnimationPlayer _animationPlayer;
        private string _lastAnimation = "";

        public void Init(player.Character character)
        {
            _character = character;
            _animationPlayer = character.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            _character.Velocity = direction * player.Character.Speed;
            UpdateAnimation(direction);
            _character.MoveAndSlide();
        }

        private void UpdateAnimation(Vector2 direction)
        {
            if (_animationPlayer == null) return;
            string anim;
            if (direction.Y > 0)
                anim = core.AnimationKeys.WalkForward;
            else if (direction.Y < 0)
                anim = core.AnimationKeys.WalkBack;
            else if (direction.X < 0)
                anim = core.AnimationKeys.WalkLeft;
            else
                anim = core.AnimationKeys.WalkRight;

            if (anim != _lastAnimation)
            {
                _lastAnimation = anim;
                _animationPlayer.Play(anim);
            }
        }
    }
}

