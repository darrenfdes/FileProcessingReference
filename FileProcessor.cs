using System;
using System.IO;

namespace sandbox
{
    class FileProcessor
    {
        private const string BackupDirectoryName = "backup";
        private const string InProgressDirectoryName = "processing";
        private const string CompletedDirectoryName = "complete";
        public string InputFilePath { get; }

        public FileProcessor(string filePath) => InputFilePath = filePath;

        public void Process()
        {
            System.Console.WriteLine($"Begin process of {InputFilePath}");
            if (!File.Exists(InputFilePath))
            {
                System.Console.WriteLine($"ERROR: file {InputFilePath} does not exist.");
                return;
            }

            string rootDirectoryPath = new DirectoryInfo(InputFilePath).Parent.Parent.FullName;
            System.Console.WriteLine($"Root data path is {rootDirectoryPath}");

            string backupDirectoryPath = Path.Combine(rootDirectoryPath, BackupDirectoryName);


            System.Console.WriteLine($"Attempting to create {backupDirectoryPath}");
            Directory.CreateDirectory(backupDirectoryPath);

            string inputFilePath = Path.GetFileName(InputFilePath);
            string backupFilePath = Path.Combine(backupDirectoryPath, inputFilePath);
            System.Console.WriteLine($"Copying {InputFilePath} to {backupFilePath}");
            File.Copy(InputFilePath, backupFilePath, true);
            //Move to in progress dir
            Directory.CreateDirectory(Path.Combine(rootDirectoryPath, InProgressDirectoryName));
            string inProgressFilePath = Path.Combine(rootDirectoryPath, InProgressDirectoryName, inputFilePath);


            System.Console.WriteLine($"Moving {InputFilePath} to {inProgressFilePath}");

            if (File.Exists(inProgressFilePath))
            {
                System.Console.WriteLine($"ERROR: a file with the name {inProgressFilePath} is already being processed");
                return;
            }

            File.Move(InputFilePath, inProgressFilePath);

            string extension = Path.GetExtension(InputFilePath);
            string completedDirectoryPath = Path.Combine(rootDirectoryPath, CompletedDirectoryName);
            Directory.CreateDirectory(completedDirectoryPath);

            System.Console.WriteLine($"Moving {inProgressFilePath} to {completedDirectoryPath}");

            string completedFileName = $"{Path.GetFileNameWithoutExtension(InputFilePath)}--{Guid.NewGuid()}{extension}";

            completedFileName = Path.ChangeExtension(completedFileName, ".complete");

            var completedFilePath = Path.Combine(completedDirectoryPath, completedFileName);

            switch (extension)
            {
                case ".txt":
                    var textFileProcessor = new TextFileProcessor(inProgressFilePath, completedFilePath);
                    textFileProcessor.Process();
                    break;

                case ".data":
                    var binaryProcessor = new BinaryFileProcessor(inProgressFilePath, completedFilePath);
                    binaryProcessor.Process();
                    break;

                case ".csv":
                    var csvProcessor = new CsvFileProcess(inProgressFilePath, completedFilePath);
                    csvProcessor.Process();
                    break;

                default:
                    Console.WriteLine($"{extension} is an unsupported file type");
                    break;
            }
            System.Console.WriteLine($"Completed processing of {inProgressFilePath}");
            System.Console.WriteLine($"Deleting {inProgressFilePath}");
            File.Delete(inProgressFilePath);
        }

    }
}