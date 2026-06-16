namespace Starwick
{
    public static class GameState
    {
        public static int StarsRelit;
        public static int ConstellationsComplete;

        public static void Reset()
        {
            StarsRelit = 0;
            ConstellationsComplete = 0;
        }
    }
}
