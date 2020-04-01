using CsvHelper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Test.models;

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
            var eaRawData = collectionResultDataEA.Aggregate()
            .Match(it=>it.Id=="11001011000001")
            .ToList();
        }


        //private static void WriteFile(this List<ResultDataAreaCode> dataList)
        //{

            ////var reg = ea.Substring(0, 1);
            ////var cwt = ea.Substring(1, 2);
            ////var amp = ea.Substring(1, 4);
            ////var tam = ea.Substring(1, 6);
            //var folderPath = $@"C:\Users\Mana PC\Documents\CSVFile\{dataList.REG}\{cwt}\{amp}\{tam}";
            //var filePath = $@"DataProcesses\{reg}\{cwt}\{amp}\{tam}\{ea}.csv";

            //if (!Directory.Exists(path))
            //{
            //    Directory.CreateDirectory(path);
            //}

            //using (var writer = new StreamWriter(filePath))
            //using (var csv = new CsvWriter(writer))
            //{
            //    csv.WriteRecords(dataLst);
            //}
        //}

    }
}
