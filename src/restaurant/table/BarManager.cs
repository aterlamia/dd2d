using Godot;
using System.Collections.Generic;

namespace dd2d.restaurant.table
{
    public partial class BarManager : Node
    {
        [Export]
        public NodePath FurnitureLayerPath { get; set; }
        [Export]
        public string FurnitureJsonPath { get; set; } = "res://data/furniture.json";

        [Export]
        public int ChairTileSetId { get; set; } = 1;
        [Export]
        public Vector2I ChairTileCoords { get; set; } = new Vector2I(3, 1);

        public override void _Ready()
        {
            var furnitureLayer = GetNodeOrNull<TileMapLayer>(FurnitureLayerPath);
            if (furnitureLayer == null)
            {
                GD.PrintErr("[BarManager] FurnitureLayer not found");
                return;
            }

            if (FurnitureRegistry.Instance == null)
            {
                GD.PrintErr("[BarManager] FurnitureRegistry not initialized");
                return;
            }

            if (FileAccess.FileExists(FurnitureJsonPath))
            {
                var file = FileAccess.Open(FurnitureJsonPath, FileAccess.ModeFlags.Read);
                var jsonText = file.GetAsText();
                file.Close();
                var root = (Godot.Collections.Dictionary)Json.ParseString(jsonText);
                if (!root.ContainsKey("barStools"))
                {
                    GD.PrintErr($"[BarManager] JSON missing 'barStools' key in {FurnitureJsonPath}");
                    return;
                }
                var arr = (Godot.Collections.Array)root["barStools"];
                foreach (Godot.Collections.Dictionary stoolDict in arr)
                {
                    var stool = BarStoolResource.FromDictionary(stoolDict);
                    FurnitureRegistry.Instance.RegisterStool(stool);
                    furnitureLayer.SetCell(stool.Position, ChairTileSetId, ChairTileCoords);
                }
            }
        }
    }
}