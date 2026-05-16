using Godot;

namespace dd2d.core.StateMachine.Player
{
	public partial class IdleState : Node
	{
		private player.Character _character;

		public void Init(player.Character character)
		{
			_character = character;
			_character.Velocity = Vector2.Zero;
			_character.GetNodeOrNull<AnimationPlayer>("AnimationPlayer")?.Play(core.AnimationKeys.Idle);
		}
	}
}
