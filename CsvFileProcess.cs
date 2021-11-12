using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace sandbox
{
    internal class CsvFileProcess
    {
        private string InputFilePath;
        private string OutPutFilePath;
        public CsvFileProcess(string InputFilePath, string OutPutFilePath)
        {
            this.InputFilePath = InputFilePath;
            this.OutPutFilePath = OutPutFilePath;
        }

        public void Process()
        {
            using StreamReader inputReader = File.OpenText(InputFilePath);

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Comment = '@',
                AllowComments = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,// default
                Delimiter = ","//Default
            };

            using CsvReader csvReader = new CsvReader(inputReader, csvConfiguration);
            csvReader.Context.RegisterClassMap<ProcessedOrderMap>();

            IEnumerable<ProcessedOrder> records = csvReader.GetRecords<ProcessedOrder>();

            using StreamWriter output = File.CreateText(OutPutFilePath);
            using var csvWriter = new CsvWriter(output, CultureInfo.InvariantCulture);

            // csvWriter.WriteRecord(records);

            csvWriter.WriteHeader<ProcessedOrder>();
            csvWriter.NextRecord();

            var recordsArray = records.ToArray();
            for (int i = 0; i < recordsArray.Length; i++)
            {
                csvWriter.WriteField(recordsArray[i].OrderNumber);
                csvWriter.WriteField(recordsArray[i].Customer);
                csvWriter.WriteField(recordsArray[i].Amount);

                bool isLastRecord = i == recordsArray.Length - 1;
                if (!isLastRecord)
                {
                    csvWriter.NextRecord();
                }
            }

            // foreach (ProcessedOrder order in records)
            // {
            //     System.Console.WriteLine($"OrderNumber: {order.OrderNumber}");
            //     System.Console.WriteLine($"Customer :{order.Customer}");
            //     // System.Console.WriteLine($"Description: {order.Description}");
            //     System.Console.WriteLine($"Amount: {order.Amount}");

            //     // System.Console.WriteLine(record.Field1);
            //     // System.Console.WriteLine(record.Field2);
            //     // System.Console.WriteLine(record.Field3);
            //     // System.Console.WriteLine(record.Field4);
            //     // System.Console.WriteLine(record.OrderNumber);
            //     // System.Console.WriteLine(record.CustomerNumber);
            //     // System.Console.WriteLine(record.Description);
            //     // System.Console.WriteLine(record.Quantity);
            // }
        }
    }
}