using Aitive.Framework.SourceGenerators.Framework.Output;
using Aitive.Framework.SourceGenerators.Framework.Text;

namespace Aitive.Framework.SourceGenerators.Framework.Logging;

public sealed class SourceLogWriter : ILogWriter
{
    private readonly SourceWriter _sourceWriter;

    internal SourceLogWriter()
        : this(new()) { }

    internal SourceLogWriter(SourceWriter sourceWriter)
    {
        _sourceWriter = sourceWriter;
    }

    internal int ErrorCount { get; private set; }

    internal SourceWriter InnerWriter => _sourceWriter;

    public void Write(LogLevel level, string message)
    {
        if (level == LogLevel.Error)
        {
            ErrorCount++;
        }

        var lines = message.SplitLines(true);
        var levelString = level + ": ";
        var isFirst = true;
        var indentString = new string(' ', levelString.Length);

        foreach (var line in lines)
        {
            var prefix = isFirst ? levelString : indentString;
            _sourceWriter.WriteLine($"/// {prefix}{line}");

            isFirst = false;
        }
    }

    public override string ToString()
    {
        return _sourceWriter.ToString();
    }
}
