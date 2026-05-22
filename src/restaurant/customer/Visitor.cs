using System;
using Godot;
using dd2d.restaurant.table;
using dd2d.core;

namespace dd2d.restaurant.customer
{
	public partial class Visitor : Node2D
	{
		[Signal] public delegate void VisitCompletedEventHandler();

		[Export] public core.Navigation.Navigator Navigator { get; set; }
		[Export] public CustomerData Data { get; set; }
		public ISeatingSpot AssignedSeat { get; set; }
		public int PartyId { get; set; } = -1;
		public Action OnPatienceExpired { get; set; }

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
			Log.Debug($"WalkToDestination called. Data assigned: {Data != null}, Navigator assigned: {Navigator != null}, Destination: {destination}", "Visitor");
			if (Data == null)
			{
				Log.Error("CustomerData is not assigned!", "Visitor");
				return;
			}
			if (Navigator == null)
			{
				Log.Error("Navigator is not assigned!", "Visitor");
				return;
			}
			var startPosition = GlobalPosition;
			var walkPath = Navigator.GetPath(GlobalPosition, destination);
			var returnPath = Navigator.GetPath(destination, startPosition);
			Log.Debug($"walkPath length: {walkPath.Length}, returnPath length: {returnPath.Length}", "Visitor");
			if (walkPath.Length == 0)
			{
				Log.Error("No path to destination!", "Visitor");
				EmitSignal(SignalName.VisitCompleted);
				return;
			}

			float speed    = Data.Speed;
			float patience = Data.Patience;

			_stateMachine.BeginVisit(
				speed,
				walkPath,
				destination,
				patience,
				returnPath,
				AssignedSeat,
				() =>
			{
				Log.Info("Visit complete", "Visitor");
				EmitSignal(SignalName.VisitCompleted);
			},
			OnPatienceExpired
			);
		}

		public void ContinueAfterWaiting()
		{
			_stateMachine.ProceedFromWaiting();
		}
	}
}
