using System.Numerics;

namespace ChessChallenge.Application
{
    public static class Settings
    {
        public const string Version = "1.17";

        // Game settings
        public const int GameDurationMilliseconds = 60 * 1000;
        public const float MinMoveDelay = 0; //0.1f;
        public static readonly bool RunBotsOnSeparateThread = true;

        // Display settings
        public const bool DisplayBoardCoordinates = true;
        public static readonly Vector2 ScreenSizeSmall = new(1920, 1080);
        public static readonly Vector2 ScreenSizeBig = new(2560, 1440);

        // Other settings
        public const int MaxTokenCount = 1024;
        public const LogType MessagesToLog = LogType.All;

        public enum LogType
        {
            None,
            ErrorOnly,
            All
        }
    }
}
