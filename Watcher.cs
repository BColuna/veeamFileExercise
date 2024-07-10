using System;
using System.IO;
using System.Threading;

namespace veeamFileExercise;

public class Watcher
{
    private readonly string _inputFolder;
    private readonly string _outputFolder;
    private readonly string _logFile;
    private readonly FileSystemWatcher _watcher;

    public Watcher(string inputFolder, string outputFolder, string logFolder)
    {
        if (
            string.IsNullOrEmpty(inputFolder)
            && string.IsNullOrEmpty(outputFolder)
            && string.IsNullOrEmpty(logFolder)
        )
        {
            throw new ArgumentException("Path cannot be null or empty");
        }

        _inputFolder = @$"{inputFolder}";
        _outputFolder = @$"{outputFolder}";
        _logFile = @$"{logFolder}/log.txt";

        _watcher = new FileSystemWatcher(_inputFolder)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            NotifyFilter =
                NotifyFilters.Attributes
                | NotifyFilters.CreationTime
                | NotifyFilters.DirectoryName
                | NotifyFilters.FileName
                | NotifyFilters.LastAccess
                | NotifyFilters.LastWrite
                | NotifyFilters.Security
                | NotifyFilters.Size
        };

        _watcher.Filter = "*";
        _watcher.Changed += OnChanged;
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
        _watcher.Error += OnError;

        Console.WriteLine($"Watching directory: : {_inputFolder}");
    }

    private static bool IsDirectory(string path)
    {
        FileAttributes attributes = File.GetAttributes(path);
        return (attributes & FileAttributes.Directory) == FileAttributes.Directory;
    }

    private void AddLogEntry(string message)
    {
        using (StreamWriter f = new StreamWriter(_logFile, true))
        {
            f.WriteLine(message);
        }
    }

    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        //TO-DO
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }
        Console.WriteLine($"Changed: {e.FullPath}");
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        string message = $"{DateTime.Now.ToString("h:mm:ss tt")} - Created: {e.FullPath}";
        string newItem = e.FullPath.Replace(_inputFolder, _outputFolder);

        for (int i = 0; i < 5; i++)
        {
            try
            {
                if (IsDirectory(e.FullPath))
                    Directory.CreateDirectory(newItem);
                else
                    File.Copy(e.FullPath, newItem);
            }
            catch (IOException)
            {
                Thread.Sleep(1000);
            }
        }
        AddLogEntry(message);
        Console.WriteLine(message);
    }

    private static void OnDeleted(object sender, FileSystemEventArgs e)
    {
        //TO-DO
        Console.WriteLine($"Deleted: {e.FullPath}");
    }

    private static void OnRenamed(object sender, RenamedEventArgs e)
    {
        //TO-DO
        Console.WriteLine($"Renamed:");
        Console.WriteLine($"    Old: {e.OldFullPath}");
        Console.WriteLine($"    New: {e.FullPath}");
    }

    private static void OnError(object sender, ErrorEventArgs e)
    {
        //TO-DO
        PrintException(e.GetException());
    }

    private static void PrintException(Exception? ex)
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
}
