namespace FileSystemWatcher
{
    using FileSystemWatcher.Interfaces;
    using FileSystemWatcher.Implementations;
    using System.Threading.Tasks;

    public static class FileSystemWatcherFactory
    {
        public static IFileSystemWatcher CreateInstance(string path, IFileSystemWatcherEventHandler fileWatcherEventHandler = null)
            => new FileSystemWatcherImplementation(path, fileWatcherEventHandler);

        public static Task<IFileSystemWatcher> CreateInstanceAsync(string path, IFileSystemWatcherEventHandler fileWatcherEventHandler = null)
            => new Task<IFileSystemWatcher>(() => new FileSystemWatcherImplementation(path, fileWatcherEventHandler));
    }
}
