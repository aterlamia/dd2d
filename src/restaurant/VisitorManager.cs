using Godot;

namespace dd2d.restaurant
{
	public partial class VisitorManager : Node
	{
		[Export] public PackedScene VisitorScene { get; set; }
		[Export] public int MaxVisitors { get; set; } = 5;
		[Export] public float SpawnInterval { get; set; } = 10f;
		[Export] public Marker2D EntryPoint { get; set; }
		[Export] public core.Navigation.Navigator Navigator { get; set; }
		[Export] public Node Destinations { get; set; }

		private int _activeVisitors = 0;
		private Timer _spawnTimer;

		public override void _Ready()
		{
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
			if (Destinations == null)
			{
				GD.PrintErr("[VisitorManager] Destinations is not set!");
				return;
			}

			_spawnTimer = new Timer();
			_spawnTimer.WaitTime = SpawnInterval;
			_spawnTimer.Timeout += TrySpawnVisitor;
			AddChild(_spawnTimer);
			_spawnTimer.Start();

			TrySpawnVisitor();
		}

		private void TrySpawnVisitor()
		{
			if (VisitorScene == null || _activeVisitors >= MaxVisitors)
				return;

			var visitor = VisitorScene.Instantiate<customer.Visitor>();
			visitor.Navigator = Navigator;
			visitor.Destinations = Destinations;
			GetParent().AddChild(visitor);
			visitor.GlobalPosition = EntryPoint.GlobalPosition;
			visitor.VisitCompleted += () => OnVisitorDone(visitor);
			_activeVisitors++;
			GD.Print($"[VisitorManager] Spawned visitor ({_activeVisitors}/{MaxVisitors})");
		}

		private void OnVisitorDone(customer.Visitor visitor)
		{
			_activeVisitors--;
			GD.Print($"[VisitorManager] Visitor despawned ({_activeVisitors}/{MaxVisitors})");
			visitor.QueueFree();
		}
	}
}
