using Aitive.Framework.GeneratedCode;

namespace Aitive.Framework.Samples.PluginWeb.Plugin01;

public enum TestType
{
    Small,
    Large,
}

[TypedId]
public readonly partial record struct TestId(TestType Type, string Value);
