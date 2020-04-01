using CsvHelper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Test.models;
using System.Linq;

namespace Test
{
    class CreateFileManager
    {
        private static IMongoCollection<ResultDataEA> collectionResultDataEA { get; set; }
        private static IMongoCollection<ResultDataAreaCode> collectionResultDataAreaCode { get; set; }
        static IMongoCollection<EaInfomation> collectionEaData { get; set; }

        public CreateFileManager()
        {
            var mongo = new MongoClient("mongodb://firstclass:Th35F1rstCla55@mongoquickx4h3q4klpbxtq-vm0.southeastasia.cloudapp.azure.com/wdata");
            var database = mongo.GetDatabase("wdata");
            collectionResultDataEA = database.GetCollection<ResultDataEA>("ResultDataEA");
            collectionResultDataAreaCode = database.GetCollection<ResultDataAreaCode>("ResultDataAreaCode");
        }

        public void ResultDataAreaCodeWriteFile()
        {
            var rawData = collectionResultDataAreaCode.Aggregate()
            .ToList();
            var data = rawData.GroupBy(it => it.Id).ToList();
            Console.WriteLine("Data start processnig ....");
            data.ForEach(it =>
            {
                WriteFile(it.FirstOrDefault().REG,it.Key,it.ToList());
            });

            Console.WriteLine("finish");

        }


        static void WriteFile(string reg, string address ,List<ResultDataAreaCode> dataList )
        {
            var regions = reg;
            var cwt = address.Substring(0, 2);
            var amp = address.Substring(2,2);
            var tam = address.Substring(4, 2);
            var folderPath = $@"C:\Users\Mana PC\Documents\CSVFile\{regions}\{cwt}\{amp}\{tam}";
            var filePath = $@"C:\Users\Mana PC\Documents\CSVFile\{regions}\{cwt}\{amp}\{tam}\{address}.csv";
          
            if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(dataList);
            }
        }


        void ResultDataEa()
        {
            var  rawDataEA = collectionResultDataEA.Aggregate()
            .ToList();
            
        }
    }
}
