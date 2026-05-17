using System;
using Godot;

namespace dd2d.restaurant.customer
{
	public partial class Visitor : Node2D
	{
		[Signal] public delegate void VisitCompletedEventHandler();

		[Export] public core.Navigation.Navigator Navigator { get; set; }
		[Export] public CustomerData Data { get; set; }

		private core.StateMachine.CustomerStateMachine _stateMachine;
		private readonly Random _random = new();

		public override void _Ready()
		{
			GD.Print($"[Visitor] _Ready called. Navigator assigned: {Navigator != null}");
			if (Navigator == null)
			{
				GD.PushError("[Visitor] Navigator is not assigned!");
				return;
			}

			_stateMachine = GetNode<core.StateMachine.CustomerStateMachine>("StateMachine");
			_stateMachine.Init(this);
		}

		public void WalkToDestination(Vector2 destination)
		{
			GD.Print($"[Visitor] WalkToDestination called. Data assigned: {Data != null}, Navigator assigned: {Navigator != null}, Destination: {destination}");
			if (Data == null)
			{
				GD.PushError("[Visitor] CustomerData is not assigned!");
				return;
			}
			if (Navigator == null)
			{
				GD.PushError("[Visitor] Navigator is not assigned!");
				return;
			}
			var startPosition = GlobalPosition;
			var walkPath = Navigator.GetPath(GlobalPosition, destination);
			var returnPath = Navigator.GetPath(destination, startPosition);
			GD.Print($"[Visitor] walkPath length: {walkPath.Length}, returnPath length: {returnPath.Length}");
			if (walkPath.Length == 0)
				return;

			float speed    = Data.Speed;
			float patience = Data.Patience;

			_stateMachine.BeginVisit(
				speed, 
				walkPath, 
				destination,
				patience,
				returnPath, () =>
			{
				GD.Print("[Visitor] Visit complete");
				EmitSignal(SignalName.VisitCompleted);
			});
		}
	}
}
