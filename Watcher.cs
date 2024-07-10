using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.VisualBasic.FileIO;

namespace veeamFileExercise;

public class Watcher
{
    private readonly string _inputFolder;
    private readonly string _outputFolder;
    private readonly string _logFile;
    private readonly int _interval;
    private readonly DateTime _startTime;
    private readonly FileSystemWatcher _watcher;

    public Watcher(int interval, string inputFolder, string outputFolder, string logFolder)
    {
        if (
            string.IsNullOrEmpty(inputFolder)
            && string.IsNullOrEmpty(outputFolder)
            && string.IsNullOrEmpty(logFolder)
        )
        {
            throw new ArgumentException("Path cannot be null or empty");
        }

        _startTime = DateTime.Now;
        _interval = interval * 1000;
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
        for (int i = 0; i < 5; i++)
        {
            try
            {
                using (StreamWriter f = new StreamWriter(_logFile, true))
                {
                    f.WriteLineAsync(message);
                }
                break;
            }
            catch (IOException)
            {
                Thread.Sleep(1000);
            }
        }
    }

    private int GetSecondsUntilNextSync()
    {
        TimeSpan timeSinceStart = DateTime.Now - _startTime;
        int secondsSinceStart = Convert.ToInt32(timeSinceStart.TotalSeconds);
        return _interval - (secondsSinceStart % _interval);
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        Action<object?> callback = (state) =>
        {
            string message = $"{DateTime.Now.ToString("h:mm:ss tt")} - Changed: {e.FullPath}";
            string newItem = e.FullPath.Replace(_inputFolder, _outputFolder);

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (!IsDirectory(e.FullPath))
                        File.Copy(e.FullPath, newItem, true);
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(1000);
                }
            }
            AddLogEntry(message);
            Console.WriteLine(message);
        };

        TimerCallback timerCallback = new TimerCallback(callback);

        var timer = new Timer(timerCallback, null, GetSecondsUntilNextSync(), Timeout.Infinite);
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        Action<object?> callback = (state) =>
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
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(1000);
                }
            }
            AddLogEntry(message);
            Console.WriteLine(message);
        };

        TimerCallback timerCallback = new TimerCallback(callback);

        var timer = new Timer(timerCallback, null, GetSecondsUntilNextSync(), Timeout.Infinite);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        Action<object?> callback = (state) =>
        {
            string message = $"{DateTime.Now.ToString("h:mm:ss tt")} - Deleted: {e.FullPath}";
            string newItem = e.FullPath.Replace(_inputFolder, _outputFolder);

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (IsDirectory(newItem))
                        Directory.Delete(newItem, true);
                    else
                        File.Delete(newItem);
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(1000);
                }
            }
            AddLogEntry(message);
            Console.WriteLine(message);
        };

        TimerCallback timerCallback = new TimerCallback(callback);

        var timer = new Timer(timerCallback, null, GetSecondsUntilNextSync(), Timeout.Infinite);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        Action<object?> callback = (state) =>
        {
            string message =
                $"{DateTime.Now.ToString("h:mm:ss tt")} - Renamed: {e.OldFullPath} to {e.FullPath}";
            string oldItemPath = e.OldFullPath.Replace(_inputFolder, _outputFolder);
            string newItemName = Path.GetFileName(e.FullPath);

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (IsDirectory(e.FullPath))
                        FileSystem.RenameDirectory(oldItemPath, newItemName);
                    else
                        FileSystem.RenameFile(oldItemPath, newItemName);
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(1000);
                }
            }

            AddLogEntry(message);
            Console.WriteLine(message);
        };

        TimerCallback timerCallback = new TimerCallback(callback);

        var timer = new Timer(timerCallback, null, GetSecondsUntilNextSync(), Timeout.Infinite);
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
