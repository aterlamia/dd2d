using Godot;

namespace dd2d.restaurant.table
{
    public enum ChairDirection { Up, Down, Left, Right }

    [GlobalClass]
    public partial class ChairResource : Resource
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
    }
}
