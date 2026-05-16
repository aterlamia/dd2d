using System;
using System.Linq;
using Godot;

namespace dd2d.restaurant.customer
{
	public partial class Visitor : Node2D
	{
		[Signal] public delegate void VisitCompletedEventHandler();

		[Export] public core.Navigation.Navigator Navigator { get; set; }
		[Export] public Node Destinations { get; set; }
		[Export] public CustomerData Data { get; set; }

		private core.StateMachine.CustomerStateMachine _stateMachine;
		private readonly Random _random = new();

		public override void _Ready()
		{
			if (Navigator == null)
			{
				GD.PushError("[Visitor] Navigator is not assigned!");
				return;
			}
			if (Destinations == null)
			{
				GD.PushError("[Visitor] Destinations is not assigned!");
				return;
			}

			_stateMachine = new core.StateMachine.CustomerStateMachine();
			AddChild(_stateMachine);
			_stateMachine.Init(this);

			// Deferred so VisitorManager has time to set GlobalPosition before we start
			CallDeferred(nameof(WalkToRandomDestination));
		}

		public void WalkToRandomDestination()
		{
			var startPosition = GlobalPosition;

			var markers = Destinations.GetChildren().OfType<Marker2D>().ToList();
			if (markers.Count == 0)
				return;

			var destination = markers[_random.Next(markers.Count)];
			var walkPath   = Navigator.GetPath(GlobalPosition, destination.GlobalPosition);
			var returnPath = Navigator.GetPath(destination.GlobalPosition, startPosition);
			if (walkPath.Length == 0)
				return;

			float speed    = Data?.Speed ?? 60f;
			float patience = Data?.Patience ?? 10f;

			_stateMachine.BeginVisit(speed, walkPath, destination.GlobalPosition, patience, returnPath, () =>
			{
				GD.Print("[Visitor] Visit complete");
				EmitSignal(SignalName.VisitCompleted);
			});
		}
	}
}
