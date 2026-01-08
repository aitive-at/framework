namespace Aitive.Framework.SourceGenerators.Framework.Text;

public static class StringExtensions
{
    extension(string value)
    {
        public string[] SplitLines(bool removeEmpty = false)
        {
            return value.Split(
                ["\r\n", "\n", "\r"],
                removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None
            );
        }
    }
}
