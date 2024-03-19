namespace FileSystemWatcher.Interfaces
{
    using FileSystemWatcher.Enumerations;
    using System;
    using System.ComponentModel;
    public interface IFileSystemWatcher : IFileSystemWatcherEventHandler, IComponent
    {
        String Name { get; }
        String FullPath { get; }
        String MimeType { get; }
        IFileSystemWatcherEventHandler FileSystemWatcherEventHandler { get; set; }
        FileSystemWatcherType FileSystemWatcherType { get; }
        bool IsWatching { get; }
        void StartWatching();
        void StopWatching();
    }
}
