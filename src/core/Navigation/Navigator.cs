using System;
using System.Linq;
using Godot;

namespace dd2d.core.Navigation
{
    public partial class Navigator : Node
    {
        private TileMapLayer _unwalkableTileMap;
        private AStarGrid2D _astarGrid;

        [Export] public NodePath UnwalkableLayerPath { get; set; }

        public override void _Ready()
        {
            // Auto-find by export path or by name fallback
            if (UnwalkableLayerPath != null && !UnwalkableLayerPath.IsEmpty)
                _unwalkableTileMap = GetNode<TileMapLayer>(UnwalkableLayerPath);
            else
                _unwalkableTileMap = GetParent().GetNode<TileMapLayer>("UnWalkable");

            BuildAStarGrid();
        }

        private void BuildAStarGrid()
        {
            _astarGrid = new AStarGrid2D();
            var size = _unwalkableTileMap.GetUsedRect();
            _astarGrid.Region = new Rect2I(size.Position, size.Size);
            _astarGrid.CellSize = _unwalkableTileMap.TileSet.TileSize;
            _astarGrid.Offset = _unwalkableTileMap.Position;
            _astarGrid.Update();
            for (int x = size.Position.X; x < size.Position.X + size.Size.X; x++)
            {
                for (int y = size.Position.Y; y < size.Position.Y + size.Size.Y; y++)
                {
                    Vector2I cell = new Vector2I(x, y);
                    if (_unwalkableTileMap.GetCellTileData(cell) != null)
                        _astarGrid.SetPointSolid(cell);
                }
            }
        }

        public Vector2[] GetPath(Vector2 from, Vector2 to)
        {
            Vector2I startCell = _unwalkableTileMap.LocalToMap(from);
            Vector2I endCell = _unwalkableTileMap.LocalToMap(to);

            startCell = FindWalkableCell(startCell);
            endCell = FindWalkableCell(endCell);

            if (startCell == new Vector2I(int.MinValue, int.MinValue) || endCell == new Vector2I(int.MinValue, int.MinValue))
                return Array.Empty<Vector2>();

            if (!_astarGrid.IsInBounds(startCell.X, startCell.Y) || !_astarGrid.IsInBounds(endCell.X, endCell.Y))
                return Array.Empty<Vector2>();

            var cellPath = _astarGrid.GetIdPath(startCell, endCell);
            return cellPath.Select(cell => _unwalkableTileMap.MapToLocal(cell)).ToArray();
        }

        private Vector2I FindWalkableCell(Vector2I cell)
        {
            if (_astarGrid.IsInBounds(cell.X, cell.Y) && !_astarGrid.IsPointSolid(cell))
                return cell;

            // Try cell below first, then spiral outward
            Vector2I below = new Vector2I(cell.X, cell.Y + 1);
            if (_astarGrid.IsInBounds(below.X, below.Y) && !_astarGrid.IsPointSolid(below))
                return below;

            int maxRadius = 5;
            for (int r = 1; r <= maxRadius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (Math.Abs(dx) != r && Math.Abs(dy) != r) continue;
                        Vector2I candidate = new Vector2I(cell.X + dx, cell.Y + dy);
                        if (_astarGrid.IsInBounds(candidate.X, candidate.Y) && !_astarGrid.IsPointSolid(candidate))
                            return candidate;
                    }
                }
            }

            return new Vector2I(int.MinValue, int.MinValue);
        }
    }
}
