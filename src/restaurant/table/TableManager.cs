using Godot;
using System.Collections.Generic;
using dd2d.core;

namespace dd2d.restaurant.table
{
    public partial class TableManager : FurnitureManagerBase
    {
	protected override void LoadFurniture(Godot.Collections.Dictionary root, TileMapLayer furnitureLayer)
		{
			if (!root.ContainsKey("tables"))
			{
				Log.Error($"JSON missing 'tables' key in {FurnitureJsonPath}", "TableManager");
				return;
			}
			var arr = (Godot.Collections.Array)root["tables"];
			foreach (Godot.Collections.Dictionary tableDict in arr)
			{
				var table = TableResource.FromDictionary(tableDict);
				if (table == null)
				{
					Log.Error("Skipping invalid table entry", "TableManager");
					continue;
				}
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