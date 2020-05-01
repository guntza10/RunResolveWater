using MongoDB.Driver;
using Test.models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Test
{
    public class TestResolve
    {
        private static IMongoCollection<DataProcessed> collectionNewDataProcessDebug { get; set; }
        public TestResolve()
        {
            var mongo = new MongoClient("mongodb://firstclass:Th35F1rstCla55@mongoquickx4h3q4klpbxtq-vm0.southeastasia.cloudapp.azure.com/wdata");
            var database = mongo.GetDatabase("wdata");
            collectionNewDataProcessDebug = database.GetCollection<DataProcessed>("NewDataProcessDebug");
        }

        public void TestIsGovernment()
        {
            var data = collectionNewDataProcessDebug.Aggregate()
             .Match(it => it.IsGovernment == 1)
             .Project(it => new
             {
                 Id = it._id,
                 EA = it.EA,
                 IsGovernment = it.IsGovernment,
                 IsGovernmentUsage = it.IsGovernmentUsage,
                 IsGovernmentWaterQuality = it.IsGovernmentWaterQuality,
                 Road = it.Road
             })
             .ToList();

            // case : IsGovernmentUsage 1 ,IsGovernmentWaterQuality 0 
            var data11012031000032 = data.Where(it => it.EA == "11012031000032" && it.Road == "เย็นอากาศ").ToList();
            var data11012031000032Q = data11012031000032.Where(it => it.IsGovernmentWaterQuality == 1).ToList();
            var data11012031000032Usage = data11012031000032.Where(it => it.IsGovernmentUsage == 1).ToList();
            Console.WriteLine($"data11012031000032 : {data11012031000032.Count}");
            Console.WriteLine($"data11012031000032Q : {data11012031000032Q.Count}");
            Console.WriteLine($"data11012031000032Usage : {data11012031000032Usage.Count}");
            Console.WriteLine("-----------------------------------------------------------------------");

            // IsGovernmentUsage 1 ,IsGovernmentWaterQuality 1
            var data11013011000025 = data.Where(it => it.EA == "11013011000025" && it.Road == "ประชาสงเคราะห์").ToList();
            var data11013011000025Q = data11013011000025.Where(it => it.IsGovernmentWaterQuality == 1).ToList();
            var data11013011000025Usage = data11013011000025.Where(it => it.IsGovernmentUsage == 1).ToList();
            Console.WriteLine($"data11013011000025 : {data11013011000025.Count}");
            Console.WriteLine($"data11013011000025Q : {data11013011000025Q.Count}");
            Console.WriteLine($"data11013011000025Usage : {data11013011000025Usage.Count}");
            Console.WriteLine("-----------------------------------------------------------------------");
            // var t11013011000025 = data11013011000025.Where(it => it.IsGovernmentUsage == 0).Take(3).ToList();
            // t11013011000025.ForEach(it =>
            // {
            //     var builder1 = Builders<DataProcessed>.Update
            //     .Set(i => i.IsGovernmentWaterQuality, 1);
            //     collectionNewDataProcessDebug.UpdateOne(i => i._id == it.Id, builder1);
            // });

            // IsGovernmentUsage 1 ,IsGovernmentWaterQuality 1
            var data11012041000036 = data.Where(it => it.EA == "11012041000036" && it.Road == "สาธุประดิษฐ์").ToList();
            var data11012041000036Q = data11012041000036.Where(it => it.IsGovernmentWaterQuality == 1).ToList();
            var data11012041000036Usage = data11012041000036.Where(it => it.IsGovernmentUsage == 1).ToList();
            Console.WriteLine($"data11012041000036 : {data11012041000036.Count}");
            Console.WriteLine($"data11012041000036Q : {data11012041000036Q.Count}");
            Console.WriteLine($"data11012041000036Usage : {data11012041000036Usage.Count}");
            Console.WriteLine("-----------------------------------------------------------------------");
            // var t11012041000036 = data11012041000036.Where(it => it.IsGovernmentUsage == 1).Take(1).ToList();
            // t11012041000036.ForEach(it =>
            // {
            //     var builder1 = Builders<DataProcessed>.Update
            //     .Set(i => i.IsGovernmentWaterQuality, 1);
            //     collectionNewDataProcessDebug.UpdateOne(i => i._id == it.Id, builder1);
            // });

            // case : IsGovernmentUsage 0 ,IsGovernmentWaterQuality 0

            var data11029011000151 = data.Where(it => it.EA == "11029011000151" && it.Road == "กรุงเทพ-นนทบุรี").ToList();
            var data11029011000151Usage = data11029011000151.Where(it => it.IsGovernmentUsage == 0).ToList();
            Console.WriteLine($"data11029011000151 : {data11029011000151.Count}");
            Console.WriteLine($"data11029011000151Usage : {data11029011000151Usage.Count}");

            // var dataRoadIsUsage0 = data.GroupBy(it => it.EA).Select(it =>
            //  {
            //      var dataIsGovernmentUsage0 = it.GroupBy(i => i.Road).Where(i => i.All(x => x.IsGovernmentUsage == 0)).ToList();
            //      var listData = new List<TestModel>();

            //      dataIsGovernmentUsage0.ForEach(i =>
            //      {
            //          var t = i.Select(x => new TestModel
            //          {
            //              EA = x.EA,
            //              Road = x.Road
            //          }).ToList();
            //          listData.AddRange(t);
            //      });

            //      return new
            //      {
            //          EA = it.Key,
            //          dataRoad = listData
            //      };
            //  })
            //  .Where(it => it.dataRoad.Count > 10)
            //  .ToList();
            // Console.WriteLine($"dataRoadIsUsage0 : {dataRoadIsUsage0.Count()}");



            // var notHasIsGovernmentUsage = data.Where(it => it.IsGovernmentUsage == 0).ToList();
            // var eaNotHasIsGovernmentUsage = notHasIsGovernmentUsage.GroupBy(it => it.EA).ToList();
            // var fin = new List<TestModel>();
            // eaNotHasIsGovernmentUsage.ForEach(d =>
            // {
            //     var tt = d.GroupBy(it => it.Road).Where(x => x.All(i => i.IsGovernmentUsage == 0)).ToList();
            //     var ttt = tt.FirstOrDefault().Select(it => new TestModel
            //     {
            //         EA = it.EA,
            //         Road = it.Road
            //     }).ToList();
            //     fin.AddRange(ttt);
            // });
            // Console.WriteLine($"notHasIsGovernmentUsage : {notHasIsGovernmentUsage.Count}");
            // Console.WriteLine($"eaNotHasIsGovernmentUsage : {eaNotHasIsGovernmentUsage.Count}");

            // var tttt = fin.GroupBy(it => it.EA).FirstOrDefault().GroupBy(it => it.Road).FirstOrDefault()
            //  .Select(it => new
            //  {
            //      Ea = it.EA,
            //      Road = it.Road
            //  }).ToList();

            // var eaDistinct = tttt.Select(it => it.Ea).Distinct().ToList();
            // eaDistinct.ForEach(it =>
            // {
            //     Console.WriteLine($"ea : {it}");
            // });
            // var roadDistinct = tttt.Select(it => it.Road).Distinct().ToList();
            // roadDistinct.ForEach(it =>
            // {
            //     Console.WriteLine($"road : {it}");
            // });

            // var dataIsGovernmentUsage = data.Where(it => it.IsGovernmentUsage == 1).ToList();
            // Console.WriteLine($"dataIsGovernmentUsage : {dataIsGovernmentUsage.Count}");
            // // var eaThatIsGovernmentUsage = dataIsGovernmentUsage.Select(it => it.EA).Distinct().ToList();
            // var dataIsGovernmentWaterQuality = dataIsGovernmentUsage.Where(it => it.IsGovernmentWaterQuality == 1).ToList();
            // Console.WriteLine($"dataIsGovernmentWaterQuality : {dataIsGovernmentWaterQuality.Count}");

            // var eaDataIsGovernment = dataIsGovernmentUsage.Select(it => it.EA).Distinct().Take(5).ToList();
            // Console.WriteLine($"{eaDataIsGovernment.Count}");
            // eaDataIsGovernment.ForEach(it =>
            // {
            //     Console.WriteLine(it);
            // });
        }
    }
}