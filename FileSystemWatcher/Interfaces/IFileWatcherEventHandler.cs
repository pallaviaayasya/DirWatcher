namespace FileSystemWatcher.Interfaces
{
    public interface IFileSystemWatcherEventHandler
    {
        void OnChanged(object sender, System.IO.FileSystemEventArgs eventArgs);
        void OnCreated(object sender, System.IO.FileSystemEventArgs eventArgs);
        void OnDeleted(object sender, System.IO.FileSystemEventArgs eventArgs);
        void OnError(object sender, System.IO.ErrorEventArgs eventArgs);
        void OnRenamed(object sender, System.IO.RenamedEventArgs eventArgs);
    }
}