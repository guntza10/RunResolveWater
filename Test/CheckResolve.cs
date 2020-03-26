using System;
using System.Linq;
using MongoDB.Driver;
using Test.models;

namespace Test
{
    public class CheckResolve
    {
        private static IMongoCollection<DataProcessed> collectionOldDataprocess { get; set; }
        private static IMongoCollection<AmountCommunity> collectionAmountCommunity { get; set; }
        public CheckResolve()
        {
            var mongo = new MongoClient("mongodb://firstclass:Th35F1rstCla55@mongoquickx4h3q4klpbxtq-vm0.southeastasia.cloudapp.azure.com/wdata");
            var database = mongo.GetDatabase("wdata");
            collectionOldDataprocess = database.GetCollection<DataProcessed>("oldDataProcess");
            collectionAmountCommunity = database.GetCollection<AmountCommunity>("amountCommunity");
        }

        public void checkCountPopulation()
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

            var dataWrong1 = data.Where(it => it.countPopulation < it.countWorkingAge).ToList();
            var dataNotB = dataWrong1.Where(it => it.sampleType != "b").ToList();
            var dataWrong2 = data.Where(it => it.sampleType != "b" && it.countPopulation < it.countWorkingAge).ToList();
            var dataWrong3 = data.Where(it => it.countPopulation > 20000).ToList();
            Console.WriteLine($"countPopulaiton < countWorkingAge : {dataWrong1.Count}");
            Console.WriteLine($"countPopulaiton < countWorkingAge that not building{dataNotB.Count}");
            Console.WriteLine($"not builiding that countPopulaiton < countWorkingAge {dataWrong2.Count}");
            Console.WriteLine($"data that countPopulation over 20000{dataWrong3.Count}");
        }

        public void checkResolveIsHouseHold()
        {
            var data = collectionOldDataprocess.Aggregate()
            .Match(it => it.SampleType == "u" && it.IsHouseHold == 0)
            .Project(it => new
            {
                Id = it._id,
                countPopulation = it.CountPopulation
            })
            .ToList()
            .Where(it => it.countPopulation > 0)
            .ToList();
            Console.WriteLine($"unit IsHouseHold = 0 that have countPopulation > 0 : {data.Count}");
        }

        public void checkBuilding()
        {
            var data = collectionOldDataprocess.Aggregate()
            .Match(it => it.SampleType == "b")
            .Project(it => new
            {
                Id = it._id,
                isHouseHold = it.IsHouseHold,
                isHouseHoldGoodPlumbing = it.IsHouseHoldGoodPlumbing
            })
            .ToList();

            var IsHouseHold = data.Where(it => it.isHouseHold != 0).ToList();
            var IsNotHouseHold = data.Where(it => it.isHouseHold == 0).ToList();
            var IsNotHouseHoldHasGoodPlumbing = IsNotHouseHold.Where(it => it.isHouseHoldGoodPlumbing > 0).ToList();
            var IsNotHouseHoldHasGoodPlumbingZero = IsNotHouseHold.Where(it => it.isHouseHoldGoodPlumbing == 0).ToList();

            Console.WriteLine($"IsHouseHold : {IsHouseHold.Count}");
            Console.WriteLine($"IsNotHouseHold : {IsNotHouseHold.Count}");
            Console.WriteLine($"IsNotHouseHold has IsHouseHoldGoodPlumbing : {IsNotHouseHoldHasGoodPlumbing.Count}");
            Console.WriteLine($"IsNotHouseHold has not IsHouseHoldGoodPlumbing : {IsNotHouseHoldHasGoodPlumbingZero.Count}");
        }
    }
}