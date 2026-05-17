using Godot;
using System.Collections.Generic;

namespace dd2d.restaurant.table
{
    public partial class Table : Node2D
    {
        [Export]
        public int MaxSeats { get; private set; } = 4;

        private List<Chair> _chairs = new List<Chair>();

        public override void _Ready()
        {
            base._Ready();
            _chairs.Clear();
            foreach (Node child in GetChildren())
            {
                if (child is Chair chair)
                {
                    _chairs.Add(chair);
                    // Add a Marker2D as a destination node at the chair's position
                    var marker = new Marker2D();
                    marker.Position = Vector2.Zero; // Local to chair
                    marker.Name = "Destination";
                    chair.AddChild(marker);
                }
            }

            // --- TileMapLayer logic ---
            // Find the Furniture and UnWalkable TileMapLayers in the scene tree
            var parent = GetTree().Root;
            TileMapLayer furnitureLayer = null;
            TileMapLayer unwalkableLayer = null;
            foreach (var node in parent.GetChildren())
            {
                if (node is TileMapLayer layer)
                {
                    if (layer.Name == "Furniture") furnitureLayer = layer;
                    if (layer.Name == "UnWalkable") unwalkableLayer = layer;
                }
            }
            // If not found at root, try to find in ancestors
            if (furnitureLayer == null || unwalkableLayer == null)
            {
                var ancestor = GetParent();
                while (ancestor != null)
                {
                    foreach (var node in ancestor.GetChildren())
                    {
                        if (node is TileMapLayer layer)
                        {
                            if (layer.Name == "Furniture" && furnitureLayer == null) furnitureLayer = layer;
                            if (layer.Name == "UnWalkable" && unwalkableLayer == null) unwalkableLayer = layer;
                        }
                    }
                    ancestor = ancestor.GetParent();
                }
            }
            // Add tiles to the layers if found
            if (furnitureLayer != null && unwalkableLayer != null)
            {
                // Exported tile IDs for furniture and unwalkable
                int furnitureTileId = 0; // You may want to export this
                int unwalkableTileId = 0; // You may want to export this
                Vector2I tableCell = furnitureLayer.LocalToMap(GlobalPosition);
                furnitureLayer.SetCell(tableCell, furnitureTileId);
                unwalkableLayer.SetCell(tableCell, unwalkableTileId);
                foreach (var chair in _chairs)
                {
                    Vector2I chairCell = furnitureLayer.LocalToMap(chair.GlobalPosition);
                    furnitureLayer.SetCell(chairCell, furnitureTileId);
                    unwalkableLayer.SetCell(chairCell, unwalkableTileId);
                }
            }
        }

        public int AvailableSeats => _chairs.Count - OccupiedSeats;
        public int OccupiedSeats => _chairs.FindAll(c => c.IsOccupied).Count;

        public bool IsOccupied => OccupiedSeats > 0;

        public Chair GetFreeChair()
        {
            foreach (var chair in _chairs)
            {
                if (!chair.IsOccupied)
                    return chair;
            }
            return null;
        }

        public bool CanSeatParty(int partySize)
        {
            return AvailableSeats >= partySize;
        }
    }
}
