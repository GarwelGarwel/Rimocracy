namespace Rimocracy
{
    enum LogLevel { Message = 0, Warning, Error };

    static class Utility
    {
        public static void Log(string message, LogLevel logLevel = LogLevel.Message)
        {
            message = "[Rimocracy] " + message;
            switch (logLevel)
            {
                case LogLevel.Message:
                    Verse.Log.Message(message);
                    break;

                case LogLevel.Warning:
                    Verse.Log.Warning(message);
                    break;

                case LogLevel.Error:
                    Verse.Log.Error(message);
                    break;
            }
        }
    }
}
