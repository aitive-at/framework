namespace Aitive.Framework.SourceGenerators.Framework.Templating;

public static class TemplateFunctions
{
    public static string ToInterpolateValue(string value)
    {
        return "{" + value + "}";
    }
}
