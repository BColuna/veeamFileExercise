using veeamFileExercise;

int timeout = int.Parse(args[0]);
string inputPath = @$"{args[1]}";
string outputPath = @$"{args[2]}";
string logPath = @$"{args[3]}";

Watcher watcher = new Watcher(timeout, inputPath, outputPath, logPath);

var _quitEvent = new ManualResetEvent(false);

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    _quitEvent.Set();
};

Console.WriteLine("Press CTRL+C to quit.");

// Wait for SIGINT
_quitEvent.WaitOne();
