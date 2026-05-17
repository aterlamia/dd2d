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
		[Export] public table.TableManager TableManager { get; set; }

		internal int _activeVisitors = 0;
		private Timer _spawnTimer;
		private table.ChairResource _assignedChair;

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
			if (TableManager == null)
			{
				GD.PrintErr("[VisitorManager] TableManager is not set!");
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
						if (VisitorScene == null || _activeVisitors >= MaxVisitors)
							return;
						var furnitureLayer = TableManager.GetNodeOrNull<TileMapLayer>(TableManager.FurnitureLayerPath);
						if (furnitureLayer == null)
						{
							GD.PrintErr("[VisitorManager] FurnitureLayer not found!");
							return;
						}
						var availableChairs = new System.Collections.Generic.List<(table.TableResource, table.ChairResource, Vector2)>();
						foreach (var tableRes in TableManager.Tables)
						{
							foreach (var chair in tableRes.Chairs)
							{
								if (!chair.IsOccupied)
								{
									Vector2I offset = chair.Direction switch
									{
										table.ChairDirection.Up => new Vector2I(0, -1),
										table.ChairDirection.Down => new Vector2I(0, 1),
										table.ChairDirection.Left => new Vector2I(-1, 0),
										table.ChairDirection.Right => new Vector2I(1, 0),
										_ => Vector2I.Zero
									};
									Vector2I chairPos = tableRes.Position + offset;
									availableChairs.Add((tableRes, chair, furnitureLayer.MapToLocal(chairPos)));
								}
							}
						}
						if (availableChairs.Count == 0)
						{
							GD.Print("[VisitorManager] No available chairs.");
							return;
						}
						var random = new System.Random();
						var (assignedTable, assignedChair, destination) = availableChairs[random.Next(availableChairs.Count)];
						assignedChair.IsOccupied = true;
						var visitor = VisitorScene.Instantiate<customer.Visitor>();
						visitor.Navigator = Navigator;
						visitor.GlobalPosition = EntryPoint.GlobalPosition;
						visitor.Data = new customer.CustomerData(); // Assign default data
						_assignedChair = assignedChair;
						visitor.WalkToDestination(destination);
						visitor.VisitCompleted += () => OnVisitorDone(visitor, assignedChair);
						_activeVisitors++;
						GD.Print($"[VisitorManager] Spawned visitor ({_activeVisitors}/{MaxVisitors})");
						// Add to scene tree after all properties are set
						GetParent().AddChild(visitor);
		}

		private void OnVisitorDone(customer.Visitor visitor, table.ChairResource assignedChair)
		{
			_activeVisitors--;
			assignedChair.IsOccupied = false;
			GD.Print($"[VisitorManager] Visitor despawned ({_activeVisitors}/{MaxVisitors})");
			visitor.QueueFree();
		}
	}
}
