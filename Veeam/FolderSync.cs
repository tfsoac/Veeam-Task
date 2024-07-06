using System;
using System.IO;
using System.Linq;
using System.Timers;

class FolderSync {

  private static string sourcePath;
  private static string copySourcePath;
  private static string logFilePath;
  private static int syncInterval;
  private static System.Timers.Timer timer;

  static void Main(string[] args) {

    if (args.Length != 4) {
      Console.WriteLine("Usage: FolderSync <sourcePath> <copySourcePath> <syncIntervalInSeconds> <logFilePath>");
      return;
    }

    sourcePath = args[0];
    copySourcePath = args[1];
    syncInterval = int.Parse(args[2]);
    logFilePath = args[3];

    timer = new System.Timers.Timer(syncInterval * 1000);
    timer.Elapsed += OnTimedEvent;
    timer.AutoReset = true;
    timer.Enabled = true;

    Log("Synchronization started.");
    Console.WriteLine("Synchronization started.");

    SyncFolders();
    Console.WriteLine("Press Enter to exit the program.");
    Console.ReadLine();
  }

  private static void OnTimedEvent(Object source, ElapsedEventArgs e) {
    SyncFolders();
  }

  private static void SyncFolders() {
    try {

      SyncDirectory(new DirectoryInfo(sourcePath), new DirectoryInfo(copySourcePath));
      CleanupReplica(new DirectoryInfo(sourcePath), new DirectoryInfo(copySourcePath));

      Log("Synchronization completed.");
      Console.WriteLine("Synchronization completed.");

    } catch (Exception ex) {
      Log($"Error during synchronization: {ex.Message}");
      Console.WriteLine($"Error during synchronization: {ex.Message}");
    }
  }

  private static void SyncDirectory(DirectoryInfo source, DirectoryInfo target) {

    if (!target.Exists) {
      target.Create();
      Log($"Created directory: {target.FullName}");
      Console.WriteLine($"Created directory: {target.FullName}");
    }

    foreach (FileInfo file in source.GetFiles()) {
      string targetFilePath = Path.Combine(target.FullName, file.Name);
      if (!File.Exists(targetFilePath)) {
        file.CopyTo(targetFilePath, true);
        Log($"Copied file: {file.FullName} to {targetFilePath}");
        Console.WriteLine($"Copied file: {file.FullName} to {targetFilePath}");
      }
    }

    foreach (DirectoryInfo sourceSubDir in source.GetDirectories()) {
      DirectoryInfo targetSubDir = target.CreateSubdirectory(sourceSubDir.Name);
      SyncDirectory(sourceSubDir, targetSubDir);
    }
  }

  private static void CleanupReplica(DirectoryInfo source, DirectoryInfo target) {

    foreach (FileInfo targetFile in target.GetFiles()) {
      string sourceFilePath = Path.Combine(source.FullName, targetFile.Name);
      if (!File.Exists(sourceFilePath)) {
        targetFile.Delete();
        Log($"Deleted file: {targetFile.FullName}");
        Console.WriteLine($"Deleted file: {targetFile.FullName}");
      }
    }


    foreach (DirectoryInfo targetSubDir in target.GetDirectories()) {
      DirectoryInfo sourceSubDir = source.GetDirectories().FirstOrDefault(d => d.Name == targetSubDir.Name);
      if (sourceSubDir == null) {
        targetSubDir.Delete(true);
        Log($"Deleted directory: {targetSubDir.FullName}");
        Console.WriteLine($"Deleted directory: {targetSubDir.FullName}");
      } else {
        CleanupReplica(sourceSubDir, targetSubDir);
      }
    }
  }

  private static void Log(string message) {
    using (StreamWriter writer = new StreamWriter(logFilePath, true)) {
      writer.WriteLine($"{DateTime.Now}: {message}");
    }
  }
}