using Godot;
using System.Collections.Generic;

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
				GD.PrintErr("[VisitorManager] VisitorScene is not set!");
				return;
			}
			if (EntryPoint == null)
			{
				GD.PrintErr("[VisitorManager] EntryPoint is not set!");
				return;
			}
			if (Navigator == null)
			{
				GD.PrintErr("[VisitorManager] Navigator is not set!");
				return;
			}
			if (FurnitureRegistry.Instance == null)
			{
				GD.PrintErr("[VisitorManager] FurnitureRegistry not found!");
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
				GD.Print("[VisitorManager] No available seats.");
				return;
			}

			var random = new System.Random();
			var (source, seat, destination) = availableSeats[random.Next(availableSeats.Count)];
			seat.IsOccupied = true;

			var visitor = VisitorScene.Instantiate<customer.Visitor>();
			visitor.Navigator = Navigator;
			visitor.GlobalPosition = EntryPoint.GlobalPosition;
			visitor.Data = new customer.CustomerData();
			visitor.WalkToDestination(destination);
			visitor.VisitCompleted += () => OnVisitorDone(visitor, seat);
			_activeVisitors++;
			GD.Print($"[VisitorManager] Spawned visitor ({_activeVisitors}/{MaxVisitors}) to {source} seat");
			GetParent().AddChild(visitor);
		}

		private void OnVisitorDone(customer.Visitor visitor, table.ISeatingSpot seat)
		{
			_activeVisitors--;
			seat.IsOccupied = false;
			GD.Print($"[VisitorManager] Visitor despawned ({_activeVisitors}/{MaxVisitors})");
			visitor.QueueFree();
		}
	}
}