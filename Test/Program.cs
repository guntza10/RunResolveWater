using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using Newtonsoft.Json;
using NSOWater.HotMigration.Models;
using Test.models;

namespace Test
{
    class Program
    {
        private static IMongoCollection<DataProcessed> collectionOldDataprocess { get; set; }
        private static IMongoCollection<AmountCommunity> collectionAmountCommunity { get; set; }
        static void Main(string[] args)
        {
            var mongo = new MongoClient("mongodb://firstclass:Th35F1rstCla55@mongoquickx4h3q4klpbxtq-vm0.southeastasia.cloudapp.azure.com/wdata");
            var database = mongo.GetDatabase("wdata");
            collectionOldDataprocess = database.GetCollection<DataProcessed>("oldDataProcess");
            collectionAmountCommunity = database.GetCollection<AmountCommunity>("amountCommunity");
            checkCountPopulationWrong();
            // resolveCountPopulation();
            // InsertAmountCommunity();

        }

        // 2.ครัวเรือนทั้งหมด -> IsHouseHold 
        public static void ResolveIsHouseHold()
        {
            var def = Builders<DataProcessed>.Update
            .Set(it => it.CountPopulation, 0);
            collectionOldDataprocess.UpdateMany(it => it.SampleType == "u" && it.IsHouseHold == 0, def);
            Console.WriteLine("update done");
        }

        // 3.ครัวเรือนที่มีน้ำประปาคุณภาพดี -> IsHouseHoldGoodPlumbing
        public static void ResolveIsHouseHoldGoodPlumbing()
        {
            var dataBuildingWillUpdate = collectionOldDataprocess.Aggregate()
            .Match(it => it.SampleType == "b")
            .Project(it => new
            {
                Id = it._id,
                EA = it.EA,
                AreaCode = it.Area_Code,
                IsHousHold = it.IsHouseHold
            })
            .ToList();

            foreach (var data in dataBuildingWillUpdate)
            {
                var dataArea = collectionOldDataprocess.Aggregate()
                .Match(it => it.SampleType == "u" && it.Area_Code == data.AreaCode)
                .Group(it => it.Area_Code, x => new
                {
                    areaCode = x.Key,
                    sumIsHouseHoldGoodPlumbing = x.Sum(i => i.IsHouseHoldGoodPlumbing),
                    sumIsHouseHold = x.Sum(i => i.IsHouseHold)
                })
                .Project(it => new
                {
                    AreaCode = it.areaCode,
                    percent = it.sumIsHouseHoldGoodPlumbing * 100 / it.sumIsHouseHold
                })
                .ToList();

                var newIsHouseHoldGoodPlumbing = 0.0;
                if (dataArea.Any())
                {
                    var percentByArea = dataArea.FirstOrDefault(it => it.AreaCode == data.AreaCode)?.percent ?? 0;
                    newIsHouseHoldGoodPlumbing = Math.Round(data.IsHousHold.Value * percentByArea / 100);
                }
                else
                {
                    var dataAmp = collectionOldDataprocess.Aggregate()
                    .Match(it => it.SampleType == "u" && it.Area_Code.Substring(0, 4) == data.AreaCode.Substring(0, 4))
                    .Group(it => it.Area_Code.Substring(0, 4), x => new
                    {
                        amp = x.Key,
                        sumIsHouseHoldGoodPlumbing = x.Sum(i => i.IsHouseHoldGoodPlumbing),
                        sumIsHouseHold = x.Sum(i => i.IsHouseHold)
                    })
                    .Project(it => new
                    {
                        Amp = it.amp,
                        percent = it.sumIsHouseHoldGoodPlumbing * 100 / it.sumIsHouseHold
                    })
                    .ToList();
                    var percentByAmp = dataAmp.FirstOrDefault(it => it.Amp == data.AreaCode.Substring(0, 4))?.percent ?? 0;
                    newIsHouseHoldGoodPlumbing = Math.Round(data.IsHousHold.Value * percentByAmp / 100);
                }

                var def = Builders<DataProcessed>.Update
                .Set(it => it.IsHouseHoldGoodPlumbing, newIsHouseHoldGoodPlumbing);
                collectionOldDataprocess.UpdateOne(it => it._id == data.Id, def);
            }
        }

        // 5.ครัวเรือนในเขตเมืองที่มีน้ำประปาใช้ (ในเขตเทศบาล) -> IsHouseHoldHasPlumbingDistrict
        // 6.ครัวเรือนในชนบทที่มีน้ำประปาใช้ (นอกเขตเทศบาล) -> IsHouseHoldHasPlumbingCountryside
        public static void ResolveIsHouseHoldHasPlumbingDistrictAndIsHouseHoldHasPlumbingCountryside()
        {
            var dataUpdate = collectionOldDataprocess.Aggregate()
            .Match(it => it.SampleType == "b" && it.IsHouseHold != 0)
            .Project(it => new
            {
                _id = it._id,
                EA = it.EA,
                IsHouseHold = it.IsHouseHold
            })
            .ToList();

            foreach (var data in dataUpdate)
            {
                var district = data.EA[7];
                var IsHouseHoldHasPlumbingDistrict = (district == '0' || district == '1') ? data.IsHouseHold : 0;
                var IsHouseHoldHasPlumbingCountryside = (district == '2') ? data.IsHouseHold : 0;
                var def = Builders<DataProcessed>.Update
               .Set(it => it.IsHouseHoldHasPlumbingDistrict, IsHouseHoldHasPlumbingDistrict)
               .Set(it => it.IsHouseHoldHasPlumbingCountryside, IsHouseHoldHasPlumbingCountryside);
                collectionOldDataprocess.UpdateOne(it => it._id == data._id, def);
            }
        }

        //16.ระดับความลึกของน้ำท่วม (ในเขตที่อยู่อาศัย) -> AvgWaterHeightCm
        //17.ระยะเวลาที่น้ำท่วมขัง (ในเขตที่อยู่อาศัย) -> TimeWaterHeightCm
        public static void ResolveAvgWaterHeightCm()
        {
            var def = Builders<DataProcessed>.Update
            .Set(it => it.AvgWaterHeightCm, 0)
            .Set(it => it.TimeWaterHeightCm, 0);
            // collectionOldDataprocess.UpdateMany(it => it.EA != "" || it.Area_Code != null && it.AvgWaterHeightCm == 0 || it.TimeWaterHeightCm == 0, def);
            collectionOldDataprocess.UpdateMany(it => it.AvgWaterHeightCm == 0 || it.TimeWaterHeightCm == 0, def);
            Console.WriteLine("update done");
        }

        //23.แหล่งน้ำขนาดใหญ่ กลาง และเล็ก (สน.2) -> WaterSources
        public static void ResolveWaterSources()
        {
            var def = Builders<DataProcessed>.Update
            .Set(it => it.WaterSources, 0);
            // collectionOldDataprocess.UpdateMany(it => it.EA != null || it.Area_Code != null && it.WaterSources < 1260 && it.SampleType == "c", def);
            collectionOldDataprocess.UpdateMany(it => it.SampleType == "c" && it.WaterSources < 1260, def);
            Console.WriteLine("update done");
        }

        public static void ResolveCountCommunity()
        {
            
        }
        public static void checkCountPopulationWrong()
        {
            var data = collectionOldDataprocess.Aggregate()
           .Project(it => new
           {
               id = it._id,
               sampleType = it.SampleType,
               countPopulation = it.CountPopulation.Value,
               countWorkingAge = it.CountWorkingAge.Value
           })
           .ToList();

            var dataWrong1 = data.Where(it => it.countPopulation < it. ).ToList();
            var dataNotB = dataWrong1.Where(it => it.sampleType != "b").ToList();
            var dataWrong2 = data.Where(it => it.sampleType != "b" && it.countPopulation < it.countWorkingAge).ToList();
            var dataWrong3 = data.Where(it => it.countPopulation > 20000).ToList();
            Console.WriteLine(dataWrong1.Count());
            Console.WriteLine(dataNotB.Count());
            Console.WriteLine(dataWrong2.Count());
            Console.WriteLine(dataWrong3.Count());
        }

        // done
        public static void InsertAmountCommunity()
        {
            var filePath = @"dataCom.csv";
            Console.WriteLine("start insert!");
            using (var reader = new StreamReader(filePath))
            {
                var dataString = reader.ReadToEnd();
                var listData = dataString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var finalData = listData.Skip(1).Select(it =>
                {
                    var data = it.Split(',').ToList();
                    return new AmountCommunity
                    {
                        Id = data[0].Remove(0, 1),
                        region = data[1],
                        regionNo = data[2],
                        cwt = data[3],
                        cwtNo = data[4],
                        amp = data[5],
                        ampNo = data[6],
                        tam = data[7],
                        tamNo = data[8],
                        totalCom = Convert.ToInt32(data[9])
                    };
                })
                .ToList();
                Console.WriteLine($"Total Data will Insert : {finalData.Count}");
                collectionAmountCommunity.InsertMany(finalData);
                Console.WriteLine("Insert Done!");
            }
        }
    }

}
enum WeekDays
{
    Monday = 0,
    Tuesday = 1,
    Wednesday = 2,
    Thursday = 3,
    Friday = 4,
    Saturday = 5,
    Sunday = 6
}

// var reader = new ReadCsv();
// var data = reader.ReadData();
// var data2 = reader.ReadData2();
// var t = Convert.ToInt32("10");
// var t2 = Convert.ToInt32("10");
// // int member;
// var t3 = Int32.TryParse("", out var member);
// Console.WriteLine(t);
// Console.WriteLine(t2);
// var lamp = new Lamps();
// lamp.GeneratePDF();
// var dic = lamp.CreateLamps();
// var showLamp = lamp.ShowLamps(dic);
// Console.WriteLine(showLamp);
// while (true)
// {
//     Console.Write("Press Input Number : ");
//     var input = Console.ReadLine();
//     var isCheckNumber = Int32.TryParse(input, out Int32 check);
//     if (isCheckNumber && check <= 10 && check != 0)
//     {
//         dic[input] = dic[input] == true ? false : true;
//         showLamp = lamp.ShowLamps(dic);
//         Console.WriteLine(showLamp);
//     }
//     else
//     {
//         Console.WriteLine("Input isn't Number Or Number is over !!!!");
//         showLamp = lamp.ShowLamps(dic);
//         Console.WriteLine(showLamp);
//     }
// }