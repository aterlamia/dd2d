using Godot;
using System.Collections.Generic;
using dd2d.core;

namespace dd2d.restaurant
{
    public partial class FurnitureRegistry : Node
    {
        private static FurnitureRegistry _instance;
        public static FurnitureRegistry Instance
        {
            get => _instance;
            private set => _instance = value;
        }

        [Export]
        public Godot.Collections.Array<table.TableResource> Tables { get; set; } = new();

        [Export]
        public Godot.Collections.Array<table.BarStoolResource> BarStools { get; set; } = new();

        public override void _Ready()
        {
            _instance = this;
        }

        public void RegisterTable(table.TableResource table)
        {
            if (!Tables.Contains(table))
            {
                Tables.Add(table);
            }
        }

        public void RegisterStool(table.BarStoolResource stool)
        {
            if (!BarStools.Contains(stool))
            {
                BarStools.Add(stool);
            }
        }

        public List<(table.TableResource table, table.ChairResource chair, Vector2 worldPos)> GetAvailableTableSeats(TileMapLayer layer)
        {
            var result = new List<(table.TableResource, table.ChairResource, Vector2)>();
            foreach (var table in Tables)
            {
                // Skip entire table if any chair is taken — each table seats one group only
                if (table.IsOccupied)
                    continue;
                foreach (var chair in table.Chairs)
                {
                    if (!chair.IsOccupied)
                    {
                        Vector2I chairPos = table.Position + chair.ToOffset();
                        result.Add((table, chair, layer.MapToLocal(chairPos)));
                    }
                }
            }
            return result;
        }

        public List<(table.BarStoolResource stool, Vector2 worldPos)> GetAvailableBarStools(TileMapLayer layer)
        {
            var result = new List<(table.BarStoolResource, Vector2)>();
            foreach (var stool in BarStools)
            {
                if (!stool.IsOccupied)
                {
                    result.Add((stool, layer.MapToLocal(stool.Position)));
                }
            }
            return result;
        }

        public table.TableResource FindNearestTable(Vector2I tilePos, int maxDistance = 10)
        {
            table.TableResource nearest = null;
            int nearestDist = maxDistance + 1;
            foreach (var table in Tables)
            {
                int dist = Mathf.Abs(tilePos.X - table.Position.X) + Mathf.Abs(tilePos.Y - table.Position.Y);
                if (dist <= maxDistance && dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = table;
                }
            }
            return nearest;
        }
    }
}