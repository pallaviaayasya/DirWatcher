namespace FileSystemWatcher.Implementations
{
    using FileSystemWatcher.Enumerations;
    using FileSystemWatcher.Interfaces;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Threading;

    sealed class FileSystemWatcherImplementation : IFileSystemWatcher
    {
        private readonly object synchronizationLock = new object();
        private readonly FileSystemWatcher fileSystemWatcher;
        private IFileSystemWatcherEventHandler fileSystemWatcherEventHandler;
        public FileSystemWatcherImplementation(string path, IFileSystemWatcherEventHandler fileWatcherEventHandler = null)
        {
            this.FileSystemWatcherEventHandler = fileWatcherEventHandler;
            path = path?.Trim() ?? string.Empty;

            // Check if the supplied path refers to an existing file or an existing directory
            var isExistingFile = File.Exists(path);
            var isExistingDirectory = Directory.Exists(path);

            // Throw an exception if the supplied path neither references a file nor a folder
            if (!isExistingFile && !isExistingDirectory)
            {
                throw new ArgumentException($"The supplied path neither is a directory, nor a file: '{path}'.", nameof(path));
            }

            // Set the FileSystemWatcherType accordingly
            this.FileSystemWatcherType = isExistingFile ? FileSystemWatcherType.File : FileSystemWatcherType.Directory;
            this.fileSystemWatcher = new FileSystemWatcher()
            {
                Path = isExistingFile ? Directory.GetParent(path).FullName : path,
                Filter = isExistingFile ? Path.GetFileName(path) : null,
                NotifyFilter = NotifyFilters.LastWrite | (isExistingFile ? NotifyFilters.FileName : NotifyFilters.DirectoryName)
            };
        }
        public string Name => this.FileSystemWatcherType == FileSystemWatcherType.File
            ? this.fileSystemWatcher.Filter
            : Path.GetFileName(this.fileSystemWatcher.Path);
        public string FullPath => this.FileSystemWatcherType == FileSystemWatcherType.File
            ? this.fileSystemWatcher.Path + Path.DirectorySeparatorChar + this.Name
            : this.fileSystemWatcher.Path;
        public string MimeType => this.FileSystemWatcherType == FileSystemWatcherType.Directory
            ? "application/octet-stream"
            : Path.GetExtension(this.Name);
        public FileSystemWatcherType FileSystemWatcherType { get; private set; } = FileSystemWatcherType.Unknown;

        public IFileSystemWatcherEventHandler FileSystemWatcherEventHandler
        {
            get => this.fileSystemWatcherEventHandler;
            set
            {
                // Retrieve a value, indicating whether the FileSystemWatcher instance is currently active
                var isWatching = this.IsWatching;

                // Remove the registrations for the implementation of the IFileSystemWatcherEventHandler interface that will be removed
                if (isWatching)
                {
                    this.StopWatching();
                }
                this.fileSystemWatcherEventHandler = value;

                // Register the new implementation of the IFileSystemWatcherEventHandler interface if the previous handler was already listening
                if (isWatching)
                {
                    this.StartWatching();
                }
            }
        }
        public bool IsWatching
        {
            get
            {
                // Gain exclusive access to the internal FileSystemWatcher instance, because its property might be updated
                lock (this.synchronizationLock)
                {
                    return this.fileSystemWatcher?.EnableRaisingEvents ?? false;
                }
            }
        }
        public ISite Site { get; set; }
        public void StartWatching()
        {
            // Synchronize access to the event handler registrations, so that this method becomes thread-safe
            lock (this.synchronizationLock)
            {
                if (this.fileSystemWatcher != null)
                {
                    this.fileSystemWatcher.Changed += this.OnChanged;
                    this.fileSystemWatcher.Created += this.OnCreated;
                    this.fileSystemWatcher.Deleted += this.OnDeleted;
                    this.fileSystemWatcher.Error += this.OnError;
                    this.fileSystemWatcher.Renamed += this.OnRenamed;

                    // Let the internal FileSystemWatcher instance raise its events
                    this.fileSystemWatcher.EnableRaisingEvents = true;
                }
            }
        }
        public void StopWatching()
        {
            lock (this.synchronizationLock)
            {
                if (this.fileSystemWatcher != null)
                {
                    // Prevent the internal FileSystemWatcher instance from raising any further events
                    this.fileSystemWatcher.EnableRaisingEvents = false;

                    // Unwire the events from the internal FileSystemWatcher instance
                    fileSystemWatcher.Changed -= this.OnChanged;
                    fileSystemWatcher.Created -= this.OnCreated;
                    fileSystemWatcher.Deleted -= this.OnDeleted;
                    fileSystemWatcher.Error -= this.OnError;
                    fileSystemWatcher.Renamed -= this.OnRenamed;
                }
           }
        }

        #region IFileSystemWatcherEventHandler methods
        public void OnChanged(object sender, FileSystemEventArgs eventArgs)
        {
            this.FileSystemWatcherEventHandler?.OnChanged(this, eventArgs);
        }

        public void OnCreated(object sender, FileSystemEventArgs eventArgs)
        {
            this.FileSystemWatcherEventHandler?.OnCreated(this, eventArgs);
        }

        public void OnDeleted(object sender, FileSystemEventArgs eventArgs)
        {
            this.FileSystemWatcherEventHandler?.OnDeleted(this, eventArgs);
        }

        public void OnError(object sender, ErrorEventArgs eventArgs)
        {
            this.FileSystemWatcherEventHandler?.OnError(this, eventArgs);
        }

        public void OnRenamed(object sender, RenamedEventArgs eventArgs)
        {
            this.FileSystemWatcherEventHandler?.OnRenamed(this, eventArgs);
        }

        #endregion

        #region IDisposable Support

        /// <summary>
        ///   An integer value, indicating whether this <see cref="FileSystemWatcherImplementation"/> instance has already been disposed.
        /// </summary>
        private int disposeCalls = 0;

        /// <summary>
        ///   An <see cref="EventHandler"/> to notify observers about the disposal of the <see cref="FileSystemWatcherImplementation"/> instance.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        ///   Provides a mechanism for deterministic finalization of the <see cref="FileSystemWatcherImplementation"/> instance by releasing
        ///   any resources associated with the <see cref="FileSystemWatcherImplementation"/> instance.
        /// </summary>
        public void Dispose()
        {
            // Thread-safe comparison of the integer for equality with 0; if equal, replaces the it with value 1, while returning the original value
            if (0 == Interlocked.CompareExchange(ref this.disposeCalls, 1, 0))
            {
                // Stop watching for changes of the internal FileSystemWatcher instance
                this.StopWatching();

                // Clear the reference to the implementation of the IFileSystemWatcherEventHandler interface
                this.FileSystemWatcherEventHandler = null;

                // Notify all observers about the disposal of this instance
                this.Disposed?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion
    }
}
