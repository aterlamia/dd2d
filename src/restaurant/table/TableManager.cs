using Godot;
using System.Collections.Generic;

namespace dd2d.restaurant.table
{
    public partial class TableManager : Node
    {
        [Export]
        public NodePath FurnitureLayerPath { get; set; }
        [Export]
        public string FurnitureJsonPath { get; set; } = "res://data/furniture.json";

        public override void _Ready()
        {
            GD.Print($"[TableManager] _Ready called. FurnitureLayerPath: {FurnitureLayerPath}");
            var furnitureLayer = GetNodeOrNull<TileMapLayer>(FurnitureLayerPath);
            if (furnitureLayer == null)
            {
                GD.PrintErr("[TableManager] FurnitureLayer not found");
                return;
            }

            if (FurnitureRegistry.Instance == null)
            {
                GD.PrintErr("[TableManager] FurnitureRegistry not initialized");
                return;
            }

            if (FileAccess.FileExists(FurnitureJsonPath))
            {
                GD.Print($"[TableManager] Loading furniture layout from {FurnitureJsonPath}");
                var file = FileAccess.Open(FurnitureJsonPath, FileAccess.ModeFlags.Read);
                var jsonText = file.GetAsText();
                file.Close();
                var root = (Godot.Collections.Dictionary)Json.ParseString(jsonText);
                if (!root.ContainsKey("tables"))
                {
                    GD.PrintErr($"[TableManager] JSON missing 'tables' key in {FurnitureJsonPath}");
                    return;
                }
                var arr = (Godot.Collections.Array)root["tables"];
                foreach (Godot.Collections.Dictionary tableDict in arr)
                {
                    var table = TableResource.FromDictionary(tableDict);
                    FurnitureRegistry.Instance.RegisterTable(table);
                    furnitureLayer.SetCell(table.Position, table.TableTileSetId, table.TableTileCoords);
                    foreach (var chair in table.Chairs)
                    {
                        Vector2I chairPos = table.Position + chair.ToOffset();
                        furnitureLayer.SetCell(chairPos, table.ChairTileSetId, table.ChairTileCoords);
                    }
                }
            }
        }
    }
}