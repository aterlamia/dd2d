using Godot;
using System.Collections.Generic;

namespace dd2d.restaurant.table
{
    public partial class TableManager : Node
    {
        [Export]
        public Godot.Collections.Array<TableResource> Tables { get; set; } = new();

        [Export]
        public NodePath FurnitureLayerPath { get; set; }
        [Export]
        public NodePath UnwalkableLayerPath { get; set; }
        [Export]
        public string TableLayoutJsonPath { get; set; } = "res://data/tables.json";

        public override void _Ready()
        {
            GD.Print($"[TableManager] _Ready called. FurnitureLayerPath: {FurnitureLayerPath}");
            // Load from JSON if file exists
            if (FileAccess.FileExists(TableLayoutJsonPath))
            {
                GD.Print($"[TableManager] Loading table layout from {TableLayoutJsonPath}");
                var file = FileAccess.Open(TableLayoutJsonPath, FileAccess.ModeFlags.Read);
                var jsonText = file.GetAsText();
                file.Close();
                var arr = (Godot.Collections.Array)Json.ParseString(jsonText);
                Tables = new Godot.Collections.Array<TableResource>();
                foreach (Godot.Collections.Dictionary tableDict in arr)
                {
                    Tables.Add(TableResource.FromDictionary(tableDict));
                }
            }

            // Get nodes
            var furnitureLayer = GetNodeOrNull<TileMapLayer>(FurnitureLayerPath);
            GD.Print($"[TableManager] FurnitureLayer found: {furnitureLayer != null}");
            if (furnitureLayer == null) {
                GD.PrintErr("[TableManager] FurnitureLayer or Destinations node not found");
                return;
            }
            foreach (var table in Tables)
            {
                GD.Print($"[TableManager] Placing table at {table.Position} with {table.Chairs.Count} chairs");
                // Place table tile
                furnitureLayer.SetCell(table.Position, table.TableTileSetId, table.TableTileCoords);
                // Place chair tiles and add destinations
                foreach (var chair in table.Chairs)
                {
                    GD.Print($"[TableManager] Placing chair at direction {chair.Direction}");
                    Vector2I offset = chair.Direction switch
                    {
                        ChairDirection.Up => new Vector2I(0, -1),
                        ChairDirection.Down => new Vector2I(0, 1),
                        ChairDirection.Left => new Vector2I(-1, 0),
                        ChairDirection.Right => new Vector2I(1, 0),
                        _ => Vector2I.Zero
                    };
                    Vector2I chairPos = table.Position + offset;
                    furnitureLayer.SetCell(chairPos, table.ChairTileSetId, table.ChairTileCoords);
                }
            }
        }

        // Returns a list of world positions for all unoccupied chairs
        public List<Vector2> GetAvailableChairDestinations(TileMapLayer furnitureLayer)
        {
            var result = new List<Vector2>();
            foreach (var table in Tables)
            {
                foreach (var chair in table.Chairs)
                {
                    if (!chair.IsOccupied)
                    {
                        Vector2I offset = chair.Direction switch
                        {
                            ChairDirection.Up => new Vector2I(0, -1),
                            ChairDirection.Down => new Vector2I(0, 1),
                            ChairDirection.Left => new Vector2I(-1, 0),
                            ChairDirection.Right => new Vector2I(1, 0),
                            _ => Vector2I.Zero
                        };
                        Vector2I chairPos = table.Position + offset;
                        result.Add(furnitureLayer.MapToLocal(chairPos));
                    }
                }
            }
            return result;
        }
    }
}
