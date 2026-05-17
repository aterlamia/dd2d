using Godot;
using System.Collections.Generic;

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

        public static TableResource FromDictionary(Godot.Collections.Dictionary tableDict)
        {
            var table = new TableResource();
            var posArr = (Godot.Collections.Array)tableDict["position"];
            table.Position = new Vector2I((int)posArr[0], (int)posArr[1]);
            var tableTileCoordsArr = (Godot.Collections.Array)tableDict["table_tile_coords"];
            table.TableTileCoords = new Vector2I((int)tableTileCoordsArr[0], (int)tableTileCoordsArr[1]);
            var chairTileCoordsArr = (Godot.Collections.Array)tableDict["chair_tile_coords"];
            table.ChairTileCoords = new Vector2I((int)chairTileCoordsArr[0], (int)chairTileCoordsArr[1]);
            table.TableTileSetId = (int)tableDict["table_tile_id"];
            table.ChairTileSetId = (int)tableDict["chair_tile_id"];
            var chairsArr = (Godot.Collections.Array)tableDict["chairs"];
            foreach (string dirStr in chairsArr)
            {
                table.Chairs.Add(ChairResource.FromDirectionString(dirStr));
            }
            return table;
        }
    }
}
