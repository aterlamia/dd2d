using Godot;
using System.Collections.Generic;
using dd2d.core;

namespace dd2d.restaurant
{
	public partial class VisitorManager : Node
	{
		[Export] public PackedScene VisitorScene { get; set; }
		[Export] public int MaxVisitors { get; set; } = 5;
		[Export] public float SpawnInterval { get; set; } = 10f;
		[Export] public Marker2D EntryPoint { get; set; }
		[Export] public core.Navigation.Navigator Navigator { get; set; }
		[Export] public NodePath FurnitureLayerPath { get; set; }

		internal int _activeVisitors = 0;
		private Timer _spawnTimer;

		public override void _Ready()
		{
			if (VisitorScene == null)
			{
				Log.Error("VisitorScene is not set!", "VisitorManager");
				return;
			}
			if (EntryPoint == null)
			{
				Log.Error("EntryPoint is not set!", "VisitorManager");
				return;
			}
			if (Navigator == null)
			{
				Log.Error("Navigator is not set!", "VisitorManager");
				return;
			}
			if (FurnitureRegistry.Instance == null)
			{
				Log.Error("FurnitureRegistry not found!", "VisitorManager");
				return;
			}

			_spawnTimer = new Timer();
			_spawnTimer.WaitTime = SpawnInterval;
			_spawnTimer.Timeout += TrySpawnVisitor;
			AddChild(_spawnTimer);
			_spawnTimer.Start();

			TrySpawnVisitor();
		}

		internal void TrySpawnVisitor()
		{
			if (_activeVisitors >= MaxVisitors)
				return;

			var furnitureLayer = GetNodeOrNull<TileMapLayer>(FurnitureLayerPath);
			if (furnitureLayer == null)
			{
				GD.PrintErr("[VisitorManager] FurnitureLayer not found!");
				return;
			}

			var availableSeats = new System.Collections.Generic.List<(string source, table.ISeatingSpot seat, Vector2 worldPos)>();

			foreach (var (table, chair, worldPos) in FurnitureRegistry.Instance.GetAvailableTableSeats(furnitureLayer))
			{
				availableSeats.Add(("table", chair, worldPos));
			}

			foreach (var (stool, worldPos) in FurnitureRegistry.Instance.GetAvailableBarStools(furnitureLayer))
			{
				availableSeats.Add(("bar", stool, worldPos));
			}

			if (availableSeats.Count == 0)
			{
				Log.Info("No available seats.", "VisitorManager");
				return;
			}

			var random = new System.Random();
			var (source, seat, destination) = availableSeats[random.Next(availableSeats.Count)];

			var visitor = VisitorScene.Instantiate<customer.Visitor>();
			visitor.Navigator = Navigator;
			visitor.GlobalPosition = EntryPoint.GlobalPosition;
			visitor.Data = new customer.CustomerData();
			visitor.AssignedSeat = seat;

			GetParent().AddChild(visitor);

			visitor.WalkToDestination(destination);
			visitor.VisitCompleted += () => OnVisitorDone(visitor, seat);
			_activeVisitors++;
			Log.Info($"Spawned visitor ({_activeVisitors}/{MaxVisitors}) to {source} seat", "VisitorManager");
		}

		private void OnVisitorDone(customer.Visitor visitor, table.ISeatingSpot seat)
		{
			_activeVisitors--;
			seat.IsOccupied = false;
			Log.Info($"Visitor despawned ({_activeVisitors}/{MaxVisitors})", "VisitorManager");
			visitor.QueueFree();
		}
	}
}