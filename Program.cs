using System;
using System.IO;
using System.Threading;

static void OnChanged(object sender, FileSystemEventArgs e)
{
    if (e.ChangeType != WatcherChangeTypes.Changed)
    {
        return;
    }
    Console.WriteLine($"Changed: {e.FullPath}");
}

static void OnCreated(object sender, FileSystemEventArgs e)
{
    string value = $"Created: {e.FullPath}";
    Console.WriteLine(value);
}

static void OnDeleted(object sender, FileSystemEventArgs e) =>
    Console.WriteLine($"Deleted: {e.FullPath}");

static void OnRenamed(object sender, RenamedEventArgs e)
{
    Console.WriteLine($"Renamed:");
    Console.WriteLine($"    Old: {e.OldFullPath}");
    Console.WriteLine($"    New: {e.FullPath}");
}

static void OnError(object sender, ErrorEventArgs e) => PrintException(e.GetException());

static void PrintException(Exception? ex)
{
    if (ex != null)
    {
        Console.WriteLine($"Message: {ex.Message}");
        Console.WriteLine("Stacktrace:");
        Console.WriteLine(ex.StackTrace);
        Console.WriteLine();
        PrintException(ex.InnerException);
    }
}

var watcher = new FileSystemWatcher(@$"{args[0]}");
watcher.NotifyFilter =
    NotifyFilters.Attributes
    | NotifyFilters.CreationTime
    | NotifyFilters.DirectoryName
    | NotifyFilters.FileName
    | NotifyFilters.LastAccess
    | NotifyFilters.LastWrite
    | NotifyFilters.Security
    | NotifyFilters.Size;

watcher.Changed += OnChanged;
watcher.Created += OnCreated;
watcher.Deleted += OnDeleted;
watcher.Renamed += OnRenamed;
watcher.Error += OnError;

watcher.IncludeSubdirectories = true;
watcher.EnableRaisingEvents = true;

Console.WriteLine($"Watching directory: {args[0]}");
Console.WriteLine("Press CTRL+C to quit.");

var _quitEvent = new ManualResetEvent(false);

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    _quitEvent.Set();
};

// Wait for SIGINT
_quitEvent.WaitOne();
