using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Aitive.Framework.Plugins.Tracing;

public sealed class TracingFileProvider : IFileProvider
{
    private readonly IFileProvider _fileProvider;
    private readonly string _id;
    private readonly Action<string> _loggingAction;

    public TracingFileProvider(
        IFileProvider fileProvider,
        string id,
        Action<string>? loggingAction = null
    )
    {
        _fileProvider = fileProvider;
        _id = id;
        _loggingAction = loggingAction ?? (Console.WriteLine);
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        _loggingAction($"FileProvider {_id}: GetFileInfo, {subpath}");

        var result = _fileProvider.GetFileInfo(subpath);

        _loggingAction(
            $"FileProvider {_id}: GetFileInfo, {subpath} returned exists {result.Exists}, size {(result.Exists ? result.Length : -1)}"
        );

        return result;
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        _loggingAction($"FileProvider {_id}: GetDirectoryContents, {subpath}");

        var result = _fileProvider.GetDirectoryContents(subpath);

        _loggingAction(
            $"FileProvider {_id}: GetDirectoryContents, {subpath} returned exists {result.Exists}, children {result.Count()} "
        );

        return result;
    }

    public IChangeToken Watch(string filter)
    {
        _loggingAction($"FileProvider {_id}: Watch, {filter}");
        return _fileProvider.Watch(filter);
    }
}

public static class TracingFileProviderExtensions
{
    extension(IFileProvider fileProvider)
    {
        public IFileProvider Trace(string id, Action<string>? loggingAction = null)
        {
            return new TracingFileProvider(fileProvider, id, loggingAction);
        }
    }
}
