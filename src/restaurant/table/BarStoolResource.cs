using Godot;
using dd2d.core;

namespace dd2d.restaurant.table
{
    [GlobalClass]
    public partial class BarStoolResource : Resource, ISeatingSpot
    {
        [Export]
        public Vector2I Position { get; set; }

        [Export]
        public ChairDirection Direction { get; set; }

        [Export]
        public bool IsOccupied { get; set; } = false;

        public static BarStoolResource FromDictionary(Godot.Collections.Dictionary dict)
        {
            if (!dict.ContainsKey("position"))
            {
                Log.Error("BarStool JSON missing 'position' key", "BarStoolResource");
                return null;
            }
            var stool = new BarStoolResource();
            var posArr = (Godot.Collections.Array)dict["position"];
            if (posArr.Count < 2)
            {
                Log.Error("BarStool 'position' must have at least 2 elements", "BarStoolResource");
                return null;
            }
            stool.Position = new Vector2I((int)posArr[0], (int)posArr[1]);
            stool.Direction = dict.ContainsKey("direction")
                ? (ChairDirection)System.Enum.Parse(typeof(ChairDirection), dict["direction"].AsString())
                : ChairDirection.Up;
            return stool;
        }
    }
}