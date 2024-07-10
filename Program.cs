using veeamFileExercise;

string inputPath = @$"{args[0]}";
string outputPath = @$"{args[1]}";
string logPath = @$"{args[2]}";

Watcher watcher = new Watcher(inputPath, outputPath, logPath);

var _quitEvent = new ManualResetEvent(false);

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    _quitEvent.Set();
};

Console.WriteLine("Press CTRL+C to quit.");

// Wait for SIGINT
_quitEvent.WaitOne();
