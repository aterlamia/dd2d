using Godot;
using System.Linq;
using dd2d.core;

namespace dd2d.restaurant.table
{
    [GlobalClass]
    public partial class TableResource : Resource
    {
        [Export]
        public Vector2I Position { get; set; }
        [Export]
        public Vector2I Coords { get; set; }
        [Export]
        public Vector2I ChairTileCoords { get; set; }
        [Export]
        public Vector2I TableTileCoords { get; set; }
        [Export]
        public int TableTileSetId { get; set; } = 0;
        [Export]
        public int ChairTileSetId { get; set; } = 0;
        [Export]
        public Godot.Collections.Array<ChairResource> Chairs { get; set; } = new();

        public int TotalSeats => Chairs.Count;
        public bool IsOccupied => Chairs.Any(c => c.IsOccupied);
        public int FreeSeatCount => IsOccupied ? 0 : TotalSeats;

        public static TableResource FromDictionary(Godot.Collections.Dictionary tableDict)
        {
            var table = new TableResource();

            if (!tableDict.ContainsKey("position"))
            {
                Log.Error("Table JSON missing 'position' key", "TableResource");
                return null;
            }
            var posArr = (Godot.Collections.Array)tableDict["position"];
            if (posArr.Count < 2)
            {
                Log.Error("Table 'position' must have at least 2 elements", "TableResource");
                return null;
            }
            table.Position = new Vector2I((int)posArr[0], (int)posArr[1]);

            if (!tableDict.ContainsKey("table_tile_coords"))
            {
                Log.Error("Table JSON missing 'table_tile_coords' key", "TableResource");
                return null;
            }
            var tableTileCoordsArr = (Godot.Collections.Array)tableDict["table_tile_coords"];
            if (tableTileCoordsArr.Count < 2)
            {
                Log.Error("Table 'table_tile_coords' must have at least 2 elements", "TableResource");
                return null;
            }
            table.TableTileCoords = new Vector2I((int)tableTileCoordsArr[0], (int)tableTileCoordsArr[1]);

            if (!tableDict.ContainsKey("chair_tile_coords"))
            {
                Log.Error("Table JSON missing 'chair_tile_coords' key", "TableResource");
                return null;
            }
            var chairTileCoordsArr = (Godot.Collections.Array)tableDict["chair_tile_coords"];
            if (chairTileCoordsArr.Count < 2)
            {
                Log.Error("Table 'chair_tile_coords' must have at least 2 elements", "TableResource");
                return null;
            }
            table.ChairTileCoords = new Vector2I((int)chairTileCoordsArr[0], (int)chairTileCoordsArr[1]);

            if (!tableDict.ContainsKey("table_tile_id"))
            {
                Log.Error("Table JSON missing 'table_tile_id' key", "TableResource");
                return null;
            }
            table.TableTileSetId = tableDict["table_tile_id"].AsInt32();

            if (!tableDict.ContainsKey("chair_tile_id"))
            {
                Log.Error("Table JSON missing 'chair_tile_id' key", "TableResource");
                return null;
            }
            table.ChairTileSetId = tableDict["chair_tile_id"].AsInt32();

            if (!tableDict.ContainsKey("chairs"))
            {
                Log.Error("Table JSON missing 'chairs' key", "TableResource");
                return null;
            }
            var chairsArr = (Godot.Collections.Array)tableDict["chairs"];
            foreach (string dirStr in chairsArr)
            {
                table.Chairs.Add(ChairResource.FromDirectionString(dirStr));
            }
            return table;
        }
    }
}