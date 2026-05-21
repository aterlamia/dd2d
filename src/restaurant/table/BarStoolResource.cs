using Godot;

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
            var stool = new BarStoolResource();
            var posArr = (Godot.Collections.Array)dict["position"];
            stool.Position = new Vector2I((int)posArr[0], (int)posArr[1]);
            stool.Direction = dict.ContainsKey("direction")
                ? (ChairDirection)System.Enum.Parse(typeof(ChairDirection), (string)dict["direction"])
                : ChairDirection.Up;
            return stool;
        }
    }
}