using System;
using Godot;

namespace dd2d.core.StateMachine.Player
{
    public partial class InteractingState : Node
    {
        private player.Character _character;
        private Action _onDone;

        public void Init(player.Character character, Action onDone = null)
        {
            _character = character;
            _onDone = onDone;
            _character.Velocity = Vector2.Zero;
            var anim = _character.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
            if (anim != null)
            {
                anim.Connect(
                    AnimationMixer.SignalName.AnimationFinished,
                    Callable.From<StringName>(OnAnimationFinished),
                    (uint)GodotObject.ConnectFlags.OneShot);
                anim.Play(core.AnimationKeys.Interact);
            }
            else
            {
                CallDeferred(nameof(Finish));
            }
        }

        private void OnAnimationFinished(StringName animName)
        {
            if (animName == core.AnimationKeys.Interact)
                Finish();
        }

        private void Finish()
        {
            var cb = _onDone;
            _onDone = null;
            QueueFree();
            cb?.Invoke();
        }
    }
}

