using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Caching;
using System.Threading;

namespace sandbox
{
    class Program
    {

        private static ConcurrentDictionary<string, string> FilesToProcessD = new ConcurrentDictionary<string, string>();

        private static MemoryCache FilesToProcess = MemoryCache.Default;

        static void Main(string[] args)
        {
            //  DictionaryForSingleProcessing(args);
            Console.WriteLine("Parsing command line options");
            //NewMethod(args);

            var directoryToWatch = args[0];

            if (!Directory.Exists(directoryToWatch))
            {
                System.Console.WriteLine($"ERROR: {directoryToWatch} does not exist");
            }
            else
            {
                System.Console.WriteLine($"Watching directory {directoryToWatch} for changes");

                ProcessExistingFiles(directoryToWatch);

                using (var inputFileWatcher = new FileSystemWatcher(directoryToWatch))
                {


                    inputFileWatcher.IncludeSubdirectories = false;
                    inputFileWatcher.InternalBufferSize = 32768;
                    inputFileWatcher.Filter = "*.*";//default
                    inputFileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

                    inputFileWatcher.Created += FileCreated;
                    inputFileWatcher.Changed += FileChanged;
                    inputFileWatcher.Deleted += FileDeleted;
                    inputFileWatcher.Renamed += FileRenamed;
                    inputFileWatcher.Error += WatcherError;

                    inputFileWatcher.EnableRaisingEvents = true;

                    System.Console.WriteLine("Press enter to quit");
                    Console.ReadLine();


                }
                System.Console.WriteLine("End of Program");
            }
        }

        private static void ProcessExistingFiles(string directoryToWatch)
        {
            System.Console.WriteLine($"Checking {directoryToWatch} for existing files");

            foreach (var filePath in Directory.EnumerateFiles(directoryToWatch))
            {
                System.Console.WriteLine($"- Found {filePath}");
                AddToCache(filePath);
            }
        }

        private static void DictionaryForSingleProcessing(string[] args)
        {
            Console.WriteLine("Parsing command line options");
            //NewMethod(args);

            var directoryToWatch = args[0];

            if (!Directory.Exists(directoryToWatch))
            {
                System.Console.WriteLine($"ERROR: {directoryToWatch} does not exist");
            }
            else
            {
                System.Console.WriteLine($"Watching directory {directoryToWatch} for changes");
                using (var inputFileWatcher = new FileSystemWatcher(directoryToWatch))
                {
                    using (var timer = new Timer(ProcessFilesC, null, 0, 1000))
                    {
                        inputFileWatcher.IncludeSubdirectories = false;
                        inputFileWatcher.InternalBufferSize = 32768;
                        inputFileWatcher.Filter = "*.*";//default
                        inputFileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

                        inputFileWatcher.Created += FileCreated;
                        inputFileWatcher.Changed += FileChanged;
                        inputFileWatcher.Deleted += FileDeleted;
                        inputFileWatcher.Renamed += FileRenamed;
                        inputFileWatcher.Error += WatcherError;

                        inputFileWatcher.EnableRaisingEvents = true;

                        System.Console.WriteLine("Press enter to quit");
                        Console.ReadLine();
                    }
                }
                System.Console.WriteLine("End of Program");
            }
        }

        private static void WatcherError(object sender, ErrorEventArgs e)
        {
            System.Console.WriteLine($"ERROR: file system watching may no longer be active: {e.GetException()}");
        }

        private static void FileRenamed(object sender, RenamedEventArgs e)
        {
            System.Console.WriteLine($"* File renamed: {e.OldName} to {e.Name} - type: {e.ChangeType}");
        }

        private static void FileDeleted(object sender, FileSystemEventArgs e)
        {
            System.Console.WriteLine($"* File deleted: {e.Name} - type: {e.ChangeType}");
        }

        private static void FileChanged(object sender, FileSystemEventArgs e)
        {
            System.Console.WriteLine($"* File changed: {e.Name} - type: {e.ChangeType}");
            // FilesToProcessD.TryAdd(e.FullPath, e.FullPath);
            AddToCache(e.FullPath);
        }

        private static void FileCreated(object sender, FileSystemEventArgs e)
        {
            System.Console.WriteLine($"* File created: {e.Name} - type: {e.ChangeType}");
            // FilesToProcessD.TryAdd(e.FullPath, e.FullPath);
            AddToCache(e.FullPath);
        }

        private static void AddToCache(string fullPath)
        {
            var item = new CacheItem(fullPath, fullPath);
            var policy = new CacheItemPolicy
            {
                RemovedCallback = ProcessFiles,
                SlidingExpiration = TimeSpan.FromSeconds(2)
            };
            FilesToProcess.Add(item, policy);
        }

        private static void ProcessFiles(CacheEntryRemovedArguments args)
        {
            System.Console.WriteLine($"* Cache item removed: {args.CacheItem.Key} because {args.RemovedReason}");

            if (args.RemovedReason == CacheEntryRemovedReason.Expired)
            {
                var fileProcessor = new FileProcessor(args.CacheItem.Key);
                fileProcessor.Process();
            }
            else
            {
                System.Console.WriteLine($"{args.CacheItem.Key} was removed unexpectadly and may not be processed because{args.RemovedReason}");
            }
        }
        private static void NewMethod(string[] args)
        {
            var command = args[0];
            if (command == "--file")
            {
                var filePath = args[1];
                if (!Path.IsPathFullyQualified(filePath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine($"ERROR: path '{filePath}' must be fully qualified");
                    return;
                }
                System.Console.WriteLine($"Single file {filePath} selected");
                ProcessSingleFile(filePath);
            }
            else if (command == "--dir")
            {
                var directoryPath = args[1];
                var fileType = args[2];
                System.Console.WriteLine($"Directory {directoryPath} selected for {fileType} files");
                ProcessDirectory(directoryPath, fileType);
            }
            else
            {
                System.Console.WriteLine("Invalid command line options");
            }
        }


        private static void ProcessFilesC(object stateInfo)
        {
            foreach (var fileName in FilesToProcessD.Keys)
            {
                if (FilesToProcessD.TryRemove(fileName, out _))
                {
                    var fileProcessor = new FileProcessor(fileName);
                    fileProcessor.Process();
                }
            }
        }

        private static void ProcessSingleFile(string filePath)
        {
            var fileProcessor = new FileProcessor(filePath);
            fileProcessor.Process();
        }

        private static void ProcessDirectory(string directoryPath, string fileType)
        {
            switch (fileType)
            {
                case "TEXT":
                    string[] textFiles = Directory.GetFiles(directoryPath, "*.txt");
                    foreach (var textFilePath in textFiles)
                    {
                        var fileProcessor = new FileProcessor(textFilePath);
                        fileProcessor.Process();
                    }
                    break;
                default:
                    System.Console.WriteLine($"ERROR: {fileType} is no supported");
                    return;
            }
        }
    }
}
