using Godot;
using System.Linq;
using System;

namespace dd2d
{
	public partial class Visitor : Node2D
	{
		private float _speed = 100f;
		private Node _destinations;
		private Random _random;
		private TileMapLayer _unwalkableTileMap;
		private AStarGrid2D _astarGrid;
		private Vector2[] _path = new Vector2[0];
		private int _pathIndex = 0;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			_unwalkableTileMap = GetParent().GetNode<TileMapLayer>("UnWalkable");
			_unwalkableTileMap.Visible = false; // Hide in-game
			_destinations = GetParent().GetNode("Destinations");
			_random = new Random((int)DateTime.Now.Ticks);
			BuildAStarGrid();
			WalkToRandomDestination();
		}

		private void BuildAStarGrid()
		{
			_astarGrid = new AStarGrid2D();
			var size = _unwalkableTileMap.GetUsedRect();
			_astarGrid.Region = new Rect2I(size.Position, size.Size);
			_astarGrid.CellSize = _unwalkableTileMap.TileSet.TileSize;
			_astarGrid.Offset = _unwalkableTileMap.Position;
			_astarGrid.Update(); // Initialize the grid before marking solids
			// Mark solid (unwalkable) cells
			for (int x = size.Position.X; x < size.Position.X + size.Size.X; x++)
			{
				for (int y = size.Position.Y; y < size.Position.Y + size.Size.Y; y++)
				{
					Vector2I cell = new Vector2I(x, y);
					if (_unwalkableTileMap.GetCellTileData(cell) != null)
						_astarGrid.SetPointSolid(cell, true);
				}
			}
		}

		public void WalkTo(Vector2 targetPosition)
		{
			Vector2I startCell = _unwalkableTileMap.LocalToMap(GlobalPosition);
			Vector2I endCell = _unwalkableTileMap.LocalToMap(targetPosition);
			// If the end cell is not walkable, first try the cell directly below
			if (_astarGrid.IsPointSolid(endCell))
			{
				Vector2I below = new Vector2I(endCell.X, endCell.Y + 1);
				if (_astarGrid.IsInBounds(below.X, below.Y) && !_astarGrid.IsPointSolid(below))
				{
					endCell = below;
				}
				else
				{
					// Search in a spiral for the nearest walkable cell
					int maxRadius = 5; // You can increase this if needed
					bool found = false;
					for (int r = 1; r <= maxRadius && !found; r++)
					{
						for (int dx = -r; dx <= r && !found; dx++)
						{
							for (int dy = -r; dy <= r && !found; dy++)
							{
								if (Math.Abs(dx) != r && Math.Abs(dy) != r) continue; // Only edge of square
								Vector2I candidate = new Vector2I(endCell.X + dx, endCell.Y + dy);
								if (_astarGrid.IsInBounds(candidate.X, candidate.Y) && !_astarGrid.IsPointSolid(candidate))
								{
									endCell = candidate;
									found = true;
								}
							}
						}
					}
					if (!found) return; // No walkable cell found nearby
				}
			}
			if (!_astarGrid.IsInBounds(startCell.X, startCell.Y) || !_astarGrid.IsInBounds(endCell.X, endCell.Y) || _astarGrid.IsPointSolid(startCell))
				return;
			var cellPath = _astarGrid.GetIdPath(startCell, endCell);
			_path = cellPath.Select(cell => _unwalkableTileMap.MapToLocal((Vector2I)cell)).ToArray();
			_pathIndex = 0;
		}

		public void WalkToRandomDestination()
		{
			if (_destinations == null || _destinations.GetChildCount() == 0)
				return;
			var markers = _destinations.GetChildren().OfType<Marker2D>().ToList();
			if (markers.Count == 0)
				return;
			// Shuffle the markers list for more randomness
			markers = markers.OrderBy(x => _random.Next()).ToList();
			// Pick a random index after shuffle for double randomness
			int idx = _random.Next(markers.Count);
			WalkTo(markers[idx].GlobalPosition);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (Input.IsActionJustPressed("ui_select")) // Space by default in Godot
			{
				WalkToRandomDestination();
			}
			if (_path.Length == 0 || _pathIndex >= _path.Length)
				return;
			Vector2 nextPos = _path[_pathIndex];
			Vector2 direction = (nextPos - GlobalPosition).Normalized();
			float distance = _speed * (float)delta;
			if (GlobalPosition.DistanceTo(nextPos) > distance)
			{
				GlobalPosition += direction * distance;
			}
			else
			{
				GlobalPosition = nextPos;
				_pathIndex++;
			}
		}
	}
}
