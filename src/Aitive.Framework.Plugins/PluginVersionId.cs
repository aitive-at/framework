using Aitive.Framework.GeneratedCode;
using Aitive.Framework.Versioning;

namespace Aitive.Framework.Plugins;

[TypedId]
public readonly partial record struct PluginVersionId(PluginId PluginId, SemVersion Version) { }
