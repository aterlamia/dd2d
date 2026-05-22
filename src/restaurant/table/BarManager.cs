using Godot;
using System.Collections.Generic;
using dd2d.core;

namespace dd2d.restaurant.table
{
    public partial class BarManager : FurnitureManagerBase
    {
        [Export]
        public int ChairTileSetId { get; set; } = 1;
        [Export]
        public Vector2I ChairTileCoords { get; set; } = new Vector2I(3, 1);

	protected override void LoadFurniture(Godot.Collections.Dictionary root, TileMapLayer furnitureLayer)
		{
			if (!root.ContainsKey("barStools"))
			{
				Log.Error($"JSON missing 'barStools' key in {FurnitureJsonPath}", "BarManager");
				return;
			}
			var arr = (Godot.Collections.Array)root["barStools"];
			foreach (Godot.Collections.Dictionary stoolDict in arr)
			{
				var stool = BarStoolResource.FromDictionary(stoolDict);
				if (stool == null)
				{
					Log.Error("Skipping invalid bar stool entry", "BarManager");
					continue;
				}
                FurnitureRegistry.Instance.RegisterStool(stool);
                furnitureLayer.SetCell(stool.Position, ChairTileSetId, ChairTileCoords);
            }
        }
    }
}