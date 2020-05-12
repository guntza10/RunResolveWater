using System;
using MongoDB.Driver;
using Test.models;
using System.Linq;
namespace Test
{
    public class ManageDataMl
    {
        private static IMongoCollection<DataProcessed> collectionNewDataProcess { get; set; }

        public ManageDataMl()
        {
            var mongo = new MongoClient("mongodb://firstclass:Th35F1rstCla55@mongoquickx4h3q4klpbxtq-vm0.southeastasia.cloudapp.azure.com/wdata");
            var database = mongo.GetDatabase("wdata");
            collectionNewDataProcess = database.GetCollection<DataProcessed>("NewDataProcess");
        }

        public void CreateCollectionT()
        {
            var data = collectionNewDataProcess.Aggregate()
            .Project(it => new
            {
                CanComputeCubicMeterGroundWaterForAgriculture = it.CanComputeCubicMeterGroundWaterForAgriculture,
                CanComputeCubicMeterGroundWaterForService = it.CanComputeCubicMeterGroundWaterForService,
                CanComputeCubicMeterGroundWaterForProduct = it.CanComputeCubicMeterGroundWaterForProduct,
                CanComputeCubicMeterGroundWaterForDrink = it.CanComputeCubicMeterGroundWaterForDrink,
                CanComputeCubicMeterPlumbingForAgriculture = it.CanComputeCubicMeterPlumbingForAgriculture,
                CanComputeCubicMeterPlumbingForService = it.CanComputeCubicMeterPlumbingForService,
                CanComputeCubicMeterPlumbingForProduct = it.CanComputeCubicMeterPlumbingForProduct,
                CanComputeCubicMeterPlumbingForDrink = it.CanComputeCubicMeterPlumbingForDrink,
                CanComputeCubicMeterSurfaceForAgriculture = it.CanComputeCubicMeterSurfaceForAgriculture,
                CanComputeCubicMeterSurfaceForService = it.CanComputeCubicMeterSurfaceForService,
                CanComputeCubicMeterSurfaceForProduct = it.CanComputeCubicMeterSurfaceForProduct,
                CanComputeCubicMeterSurfaceForDrink = it.CanComputeCubicMeterSurfaceForDrink,
                CanComputeCubicMeterGroundWaterForUse = it.CanComputeCubicMeterGroundWaterForUse,
                CanComputeCubicMeterForDrink = it.CanComputeCubicMeterForDrink
            })
            .ToList();

            var countCanComputeCubicMeterGroundWaterForAgriculture = data.Count(it => it.CanComputeCubicMeterGroundWaterForAgriculture == "True");
            var countCanComputeCubicMeterGroundWaterForService = data.Count(it => it.CanComputeCubicMeterGroundWaterForService == "True");
            var countCanComputeCubicMeterGroundWaterForProduct = data.Count(it => it.CanComputeCubicMeterGroundWaterForProduct == "True");
            var countCanComputeCubicMeterGroundWaterForDrink = data.Count(it => it.CanComputeCubicMeterGroundWaterForDrink == "True");
            var countCanComputeCubicMeterPlumbingForAgriculture = data.Count(it => it.CanComputeCubicMeterPlumbingForAgriculture == "True");
            var countCanComputeCubicMeterPlumbingForService = data.Count(it => it.CanComputeCubicMeterPlumbingForService == "True");
            var countCanComputeCubicMeterPlumbingForProduct = data.Count(it => it.CanComputeCubicMeterPlumbingForProduct == "True");
            var countCanComputeCubicMeterPlumbingForDrink = data.Count(it => it.CanComputeCubicMeterPlumbingForDrink == "True");
            var countCanComputeCubicMeterSurfaceForAgriculture = data.Count(it => it.CanComputeCubicMeterSurfaceForAgriculture == "True");
            var countCanComputeCubicMeterSurfaceForService = data.Count(it => it.CanComputeCubicMeterSurfaceForService == "True");
            var countCanComputeCubicMeterSurfaceForProduct = data.Count(it => it.CanComputeCubicMeterSurfaceForProduct == "True");
            var countCanComputeCubicMeterSurfaceForDrink = data.Count(it => it.CanComputeCubicMeterSurfaceForDrink == "True");
            var countCanComputeCubicMeterGroundWaterForUse = data.Count(it => it.CanComputeCubicMeterGroundWaterForUse == "True");
            var countCanComputeCubicMeterForDrink = data.Count(it => it.CanComputeCubicMeterForDrink == "True");

            Console.WriteLine($"countCanComputeCubicMeterGroundWaterForAgriculture {countCanComputeCubicMeterGroundWaterForAgriculture}");
            Console.WriteLine($"countCanComputeCubicMeterGroundWaterForService {countCanComputeCubicMeterGroundWaterForService}");
            Console.WriteLine($"countCanComputeCubicMeterGroundWaterForProduct {countCanComputeCubicMeterGroundWaterForProduct}");
            Console.WriteLine($"countCanComputeCubicMeterGroundWaterForDrink {countCanComputeCubicMeterGroundWaterForDrink}");
            Console.WriteLine($"countCanComputeCubicMeterPlumbingForAgriculture {countCanComputeCubicMeterPlumbingForAgriculture}");
            Console.WriteLine($"countCanComputeCubicMeterPlumbingForService {countCanComputeCubicMeterPlumbingForService}");
            Console.WriteLine($"countCanComputeCubicMeterPlumbingForProduct {countCanComputeCubicMeterPlumbingForProduct}");
            Console.WriteLine($"countCanComputeCubicMeterPlumbingForDrink {countCanComputeCubicMeterPlumbingForDrink}");
            Console.WriteLine($"countCanComputeCubicMeterSurfaceForAgriculture {countCanComputeCubicMeterSurfaceForAgriculture}");
            Console.WriteLine($"countCanComputeCubicMeterSurfaceForService {countCanComputeCubicMeterSurfaceForService}");
            Console.WriteLine($"countCanComputeCubicMeterSurfaceForProduct {countCanComputeCubicMeterSurfaceForProduct}");
            Console.WriteLine($"countCanComputeCubicMeterSurfaceForDrink {countCanComputeCubicMeterSurfaceForDrink}");
            Console.WriteLine($"countCanComputeCubicMeterGroundWaterForUse {countCanComputeCubicMeterGroundWaterForUse}");
            Console.WriteLine($"countCanComputeCubicMeterForDrink {countCanComputeCubicMeterForDrink}");
        }
    }
}