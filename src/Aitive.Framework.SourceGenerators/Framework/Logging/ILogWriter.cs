namespace Aitive.Framework.SourceGenerators.Framework.Logging;

public interface ILogWriter
{
    void Write(LogLevel level, string message);
}

public static class LogWriterExtensions
{
    extension(ILogWriter logWriter)
    {
        public void Write(LogLevel level, Exception exception, string? message = null)
        {
            var finalMessage = (message ?? string.Empty) + "\n" + exception.ToString();
            logWriter.Write(level, finalMessage);
        }

        public void Info(string message)
        {
            logWriter.Write(LogLevel.Info, message);
        }

        public void Warning(string message)
        {
            logWriter.Write(LogLevel.Warning, message);
        }

        public void Error(string message)
        {
            logWriter.Write(LogLevel.Error, message);
        }

        public void Error(Exception exception, string? message = null)
        {
            logWriter.Write(LogLevel.Error, exception, message);
        }
    }
}
