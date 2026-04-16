namespace Aitive.Framework.Io;

public static class DriveUtilities
{
    public static long GetAvailableFreeBytes(string path)
    {
        var fullPath = System.IO.Path.GetFullPath(path);

        // Pick the drive with the longest matching root name.
        // This correctly handles nested mounts on Linux (e.g. /mnt/data vs /).
        var drive = DriveInfo
            .GetDrives()
            .Where(d =>
                d.IsReady && fullPath.StartsWith(d.Name, StringComparison.OrdinalIgnoreCase)
            )
            .OrderByDescending(d => d.Name.Length)
            .FirstOrDefault();

        if (drive is null)
        {
            throw new DriveNotFoundException($"No ready drive found for path: {fullPath}");
        }

        // AvailableFreeSpace respects per-user quotas (like df).
        // TotalFreeSpace is the raw free space ignoring quotas.
        return drive.AvailableFreeSpace;
    }
}
