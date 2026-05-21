using Godot;

namespace dd2d.restaurant.table
{
    public enum ChairDirection { Up, Down, Left, Right }

    [GlobalClass]
    public partial class ChairResource : Resource, ISeatingSpot
    {
        [Export]
        public ChairDirection Direction { get; set; }

        [Export]
        public bool IsOccupied { get; set; } = false;

        public static ChairResource FromDirectionString(string dirStr)
        {
            var chair = new ChairResource();
            chair.Direction = (ChairDirection)System.Enum.Parse(typeof(ChairDirection), dirStr);
            return chair;
        }

        public static Vector2I ToOffset(ChairDirection direction) => direction switch
        {
            ChairDirection.Up => new Vector2I(0, -1),
            ChairDirection.Down => new Vector2I(0, 1),
            ChairDirection.Left => new Vector2I(-1, 0),
            ChairDirection.Right => new Vector2I(1, 0),
            _ => Vector2I.Zero
        };

        public Vector2I ToOffset() => ToOffset(Direction);
    }
}