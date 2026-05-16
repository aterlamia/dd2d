namespace dd2d.core
{
    public static class AnimationKeys
    {
        // Directional walking — shared by all characters
        public const string WalkForward = "walkF";
        public const string WalkBack    = "walkB";
        public const string WalkLeft    = "walkL";
        public const string WalkRight   = "walkR";

        // Shared idle / not-moving state
        public const string Idle = "idle";

        // Player-specific
        public const string Interact = "interact";

        // NPC-specific
        public const string Sit     = "sit";
        public const string StandUp = "stand_up";
    }
}
