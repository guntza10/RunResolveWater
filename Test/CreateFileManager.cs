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
        private static IMongoCollection<DataProcessed> collectionNewDataProcess { get; set; }

        public CreateFileManager()
        {
            var mongo = new MongoClient("mongodb://firstclass:Th35F1rstCla55@mongoquickx4h3q4klpbxtq-vm0.southeastasia.cloudapp.azure.com/wdata");
            var database = mongo.GetDatabase("wdata");
            collectionResultDataEA = database.GetCollection<ResultDataEA>("ResultDataEA");
            collectionResultDataAreaCode = database.GetCollection<ResultDataAreaCode>("ResultDataAreaCode");
            collectionNewDataProcess = database.GetCollection<DataProcessed>("NewData0526");
            collectionEaData = database.GetCollection<EaInfomation>("ea");
        }

        public void ResultDataAreaCodeWriteFile()
        {
            var rawData = collectionResultDataAreaCode.Aggregate()
            .ToList();
            var data = rawData.GroupBy(it => it.Id).ToList();
            Console.WriteLine("Data start processnig ....");
            data.ForEach(it =>
            {
                WriteFile(it.FirstOrDefault().REG, it.Key, it.ToList());
            });

            Console.WriteLine("finish");

        }

        static void WriteFile(string reg, string address, List<ResultDataAreaCode> dataList)
        {
            var regions = reg;
            var cwt = address.Substring(0, 2);
            var amp = address.Substring(2, 2);
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
            var rawDataEA = collectionResultDataEA.Aggregate()
            .ToList();
        }

        public void ExportCsvDataAreaProblem()
        {
            Console.WriteLine("Start Export");
            var path = @"C:\Users\Gun\Desktop\Work\RunResolveWater\Test\areaCodeProblem.csv";
            var listAreaCode = new List<string>();
            using (var reader = new StreamReader(path))
            {
                var dataFromRead = reader.ReadToEnd();
                var dataFromSplitLine = dataFromRead.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
                listAreaCode = dataFromSplitLine;
            }

            var count = 0;
            var total = listAreaCode.Count;
            listAreaCode.ForEach(areaCode =>
            {
                count++;
                Console.WriteLine($"round : {count} / {total}");
                var dataAreaCode = collectionNewDataProcess.Find(it => it.Area_Code == areaCode).ToList();
                var finalDataWrite = dataAreaCode.Select(it => new DataProcessedExport
                {
                    _id = it._id,
                    SampleId = it.SampleId,
                    SampleType = it.SampleType,
                    EA = it.EA,
                    Area_Code = it.Area_Code,
                    Status = it.Status,
                    Count = it.Count,
                    CubicMeterPerMonth = it.CubicMeterPerMonth,
                    WaterBill = it.WaterBill,
                    PumpCount = it.PumpCount,
                    PumpAuto = it.PumpAuto,
                    TurbidWater = it.TurbidWater,
                    SaltWater = it.SaltWater,
                    Smell = it.Smell,
                    FilmOfOil = it.FilmOfOil,
                    FogWater = it.FogWater,
                    HardWater = it.HardWater,
                    Agriculture = it.Agriculture,
                    Service = it.Service,
                    Product = it.Product,
                    Drink = it.Drink,
                    Plant = it.Plant,
                    Farm = it.Farm,
                    DoingMWA = it.DoingMWA,
                    CubicMeterPerMonthMWA = it.CubicMeterPerMonthMWA,
                    WaterBillMWA = it.WaterBillMWA,
                    TurbidWaterMWA = it.TurbidWaterMWA,
                    SaltWaterMWA = it.SaltWaterMWA,
                    SmellMWA = it.SmellMWA,
                    FilmOfOilMWA = it.FilmOfOilMWA,
                    FogWaterMWA = it.FogWaterMWA,
                    HardWaterMWA = it.HardWaterMWA,
                    AgricultureMWA = it.AgricultureMWA,
                    ServiceMWA = it.ServiceMWA,
                    ProductMWA = it.ProductMWA,
                    DrinkMWA = it.DrinkMWA,
                    PlantMWA = it.PlantMWA,
                    DoingPWA = it.DoingPWA,
                    CubicMeterPerMonthPWA = it.CubicMeterPerMonthPWA,
                    WaterBillPWA = it.WaterBillPWA,
                    TurbidWaterPWA = it.TurbidWaterPWA,
                    SaltWaterPWA = it.SaltWaterPWA,
                    SmellPWA = it.SmellPWA,
                    FilmOfOilPWA = it.FilmOfOilPWA,
                    FogWaterPWA = it.FogWaterPWA,
                    HardWaterPWA = it.HardWaterPWA,
                    AgriculturePWA = it.AgriculturePWA,
                    ServicePWA = it.ServicePWA,
                    ProductPWA = it.ProductPWA,
                    DrinkPWA = it.DrinkPWA,
                    PlantPWA = it.PlantPWA,
                    DoingOther = it.DoingOther,
                    CubicMeterPerMonthOther = it.CubicMeterPerMonthOther,
                    WaterBillOther = it.WaterBillOther,
                    TurbidWaterOther = it.TurbidWaterOther,
                    SaltWaterOther = it.SaltWaterOther,
                    SmellOther = it.SmellOther,
                    FilmOfOilOther = it.FilmOfOilOther,
                    FogWaterOther = it.FogWaterOther,
                    HardWaterOther = it.HardWaterOther,
                    AgricultureOther = it.AgricultureOther,
                    ServiceOther = it.ServiceOther,
                    ProductOther = it.ProductOther,
                    DrinkOther = it.DrinkOther,
                    PlantOther = it.PlantOther,
                    MeterRentalFee = it.MeterRentalFee,
                    PlumbingPrice = it.PlumbingPrice,
                    PoolCount = it.PoolCount,
                    WaterResourceCount = it.WaterResourceCount,
                    CubicMeterPerMonthPool = it.CubicMeterPerMonthPool,
                    PumpCountPool = it.PumpCountPool,
                    PumpAutoPool = it.PumpAutoPool,
                    SaltWaterPool = it.SaltWaterPool,
                    SmellPool = it.SmellPool,
                    FilmOfOilPool = it.FilmOfOilPool,
                    FogWaterPool = it.FogWaterPool,
                    AgriculturePool = it.AgriculturePool,
                    ServicePool = it.ServicePool,
                    ProductPool = it.ProductPool,
                    DrinkPool = it.DrinkPool,
                    PlantPool = it.PlantPool,
                    FarmPool = it.FarmPool,
                    CubicMeterPerMonthIrrigation = it.CubicMeterPerMonthIrrigation,
                    PumpCountIrrigation = it.PumpCountIrrigation,
                    PumpAutoIrrigation = it.PumpAutoIrrigation,
                    SaltWaterIrrigation = it.SaltWaterIrrigation,
                    SmellIrrigation = it.SmellIrrigation,
                    FilmOfOilIrrigation = it.FilmOfOilIrrigation,
                    FogWaterIrrigation = it.FogWaterIrrigation,
                    AgricultureIrrigation = it.AgricultureIrrigation,
                    ServiceIrrigation = it.ServiceIrrigation,
                    ProductIrrigation = it.ProductIrrigation,
                    DrinkIrrigation = it.DrinkIrrigation,
                    PlantIrrigation = it.PlantIrrigation,
                    FarmIrrigation = it.FarmIrrigation,
                    PumpCountRiver = it.PumpCountRiver,
                    PumpAutoRiver = it.PumpAutoRiver,
                    SaltWaterRiver = it.SaltWaterRiver,
                    SmellRiver = it.SmellRiver,
                    FilmOfOilRiver = it.FilmOfOilRiver,
                    FogWaterRiver = it.FogWaterRiver,
                    AgricultureRiver = it.AgricultureRiver,
                    ServiceRiver = it.ServiceRiver,
                    ProductRiver = it.ProductRiver,
                    DrinkRiver = it.DrinkRiver,
                    PlantRiver = it.PlantRiver,
                    FarmRiver = it.FarmRiver,
                    AgricultureRain = it.AgricultureRain,
                    ServiceRain = it.ServiceRain,
                    ProductRain = it.ProductRain,
                    DrinkRain = it.DrinkRain,
                    PlantRain = it.PlantRain,
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
                    CanComputeCubicMeterForDrink = it.CanComputeCubicMeterForDrink,
                    IsAgriculture = it.IsAgriculture,
                    IsHouseHold = it.IsHouseHold,
                    IsHouseHoldGoodPlumbing = it.IsHouseHoldGoodPlumbing,
                    IsAgricultureHasIrrigationField = it.IsAgricultureHasIrrigationField,
                    IsHouseHoldHasPlumbingDistrict = it.IsHouseHoldHasPlumbingDistrict,
                    IsHouseHoldHasPlumbingCountryside = it.IsHouseHoldHasPlumbingCountryside,
                    IsFactorialWaterQuality = it.IsFactorialWaterQuality,
                    IsCommercialWaterQuality = it.IsCommercialWaterQuality,
                    CountGroundWater = it.CountGroundWater,
                    CountPopulation = it.CountPopulation,
                    CountWorkingAge = it.CountWorkingAge,
                    IsFactorial = it.IsFactorial,
                    IsFactorialWaterTreatment = it.IsFactorialWaterTreatment,
                    IsCommunityWaterManagementHasWaterTreatment = it.IsCommunityWaterManagementHasWaterTreatment,
                    FieldCommunity = it.FieldCommunity,
                    AvgWaterHeightCm = it.AvgWaterHeightCm,
                    TimeWaterHeightCm = it.TimeWaterHeightCm,
                    HasntPlumbing = it.HasntPlumbing,
                    IsGovernment = it.IsGovernment,
                    IsGovernmentUsage = it.IsGovernmentUsage,
                    IsGovernmentWaterQuality = it.IsGovernmentWaterQuality,
                    CommunityNatureDisaster = it.CommunityNatureDisaster,
                    WaterSources = it.WaterSources,
                    IndustryHasWasteWaterTreatment = it.IndustryHasWasteWaterTreatment,
                    PeopleInFloodedArea = it.PeopleInFloodedArea,
                    CubicMeterGroundWaterForAgricultureGroundWater = it.CubicMeterGroundWaterForAgricultureGroundWater,
                    CubicMeterGroundWaterForAgricultureBuying = it.CubicMeterGroundWaterForAgricultureBuying,
                    CubicMeterGroundWaterForAgriculture = it.CubicMeterGroundWaterForAgriculture,
                    CubicMeterGroundWaterForServiceGroundWater = it.CubicMeterGroundWaterForServiceGroundWater,
                    CubicMeterGroundWaterForServiceBuying = it.CubicMeterGroundWaterForServiceBuying,
                    CubicMeterGroundWaterForService = it.CubicMeterGroundWaterForService,
                    CubicMeterGroundWaterForProductGroundWater = it.CubicMeterGroundWaterForProductGroundWater,
                    CubicMeterGroundWaterForProductBuying = it.CubicMeterGroundWaterForProductBuying,
                    CubicMeterGroundWaterForProduct = it.CubicMeterGroundWaterForProduct,
                    CubicMeterGroundWaterForDrinkGroundWater = it.CubicMeterGroundWaterForDrinkGroundWater,
                    CubicMeterGroundWaterForDrinkBuying = it.CubicMeterGroundWaterForDrinkBuying,
                    CubicMeterGroundWaterForDrink = it.CubicMeterGroundWaterForDrink,
                    CubicMeterPlumbingForAgricultureMWA = it.CubicMeterPlumbingForAgricultureMWA,
                    CubicMeterPlumbingForAgriculturePWA = it.CubicMeterPlumbingForAgriculturePWA,
                    CubicMeterPlumbingForAgricultureOther = it.CubicMeterPlumbingForAgricultureOther,
                    CubicMeterPlumbingForAgriculture = it.CubicMeterPlumbingForAgriculture,
                    CubicMeterPlumbingForServiceMWA = it.CubicMeterPlumbingForServiceMWA,
                    CubicMeterPlumbingForServicePWA = it.CubicMeterPlumbingForServicePWA,
                    CubicMeterPlumbingForServiceOther = it.CubicMeterPlumbingForServiceOther,
                    CubicMeterPlumbingForService = it.CubicMeterPlumbingForService,
                    CubicMeterPlumbingForProductMWA = it.CubicMeterPlumbingForProductMWA,
                    CubicMeterPlumbingForProductPWA = it.CubicMeterPlumbingForProductPWA,
                    CubicMeterPlumbingForProductOther = it.CubicMeterPlumbingForProductOther,
                    CubicMeterPlumbingForProduct = it.CubicMeterPlumbingForProduct,
                    CubicMeterPlumbingForDrinkMWA = it.CubicMeterPlumbingForDrinkMWA,
                    CubicMeterPlumbingForDrinkPWA = it.CubicMeterPlumbingForDrinkPWA,
                    CubicMeterPlumbingForDrinkOther = it.CubicMeterPlumbingForDrinkOther,
                    CubicMeterPlumbingForDrink = it.CubicMeterPlumbingForDrink,
                    CubicMeterSurfaceForAgriculturePool = it.CubicMeterSurfaceForAgriculturePool,
                    CubicMeterSurfaceForAgricultureRiver = it.CubicMeterSurfaceForAgricultureRiver,
                    CubicMeterSurfaceForAgricultureIrrigation = it.CubicMeterSurfaceForAgricultureIrrigation,
                    CubicMeterSurfaceForAgricultureRain = it.CubicMeterSurfaceForAgricultureRain,
                    CubicMeterSurfaceForAgriculture = it.CubicMeterSurfaceForAgriculture,
                    CubicMeterSurfaceForServicePool = it.CubicMeterSurfaceForServicePool,
                    CubicMeterSurfaceForServiceRiver = it.CubicMeterSurfaceForServiceRiver,
                    CubicMeterSurfaceForServiceIrrigation = it.CubicMeterSurfaceForServiceIrrigation,
                    CubicMeterSurfaceForServiceRain = it.CubicMeterSurfaceForServiceRain,
                    CubicMeterSurfaceForService = it.CubicMeterSurfaceForService,
                    CubicMeterSurfaceForProductPool = it.CubicMeterSurfaceForProductPool,
                    CubicMeterSurfaceForProductRiver = it.CubicMeterSurfaceForProductRiver,
                    CubicMeterSurfaceForProductIrrigation = it.CubicMeterSurfaceForProductIrrigation,
                    CubicMeterSurfaceForProductRain = it.CubicMeterSurfaceForProductRain,
                    CubicMeterSurfaceForProduct = it.CubicMeterSurfaceForProduct,
                    CubicMeterSurfaceForDrinkPool = it.CubicMeterSurfaceForDrinkPool,
                    CubicMeterSurfaceForDrinkRiver = it.CubicMeterSurfaceForDrinkRiver,
                    CubicMeterSurfaceForDrinkIrrigation = it.CubicMeterSurfaceForDrinkIrrigation,
                    CubicMeterSurfaceForDrinkRain = it.CubicMeterSurfaceForDrinkRain,
                    CubicMeterSurfaceForDrink = it.CubicMeterSurfaceForDrink,
                    CubicMeterGroundWaterForUse = it.CubicMeterGroundWaterForUse,
                    CountCommunity = it.CountCommunity,
                    CountCommunityHasDisaster = it.CountCommunityHasDisaster,
                    IsAllHouseHoldCountryside = it.IsAllHouseHoldCountryside,
                    IsAllHouseHoldDistrict = it.IsAllHouseHoldDistrict,
                    IsAllFactorial = it.IsAllFactorial,
                    IsAllCommercial = it.IsAllCommercial,
                    CubicMeterForDrink = it.CubicMeterForDrink,
                    Duplicate = it.Duplicate,
                    AdjustedCubicMeterGroundWaterForAgriculture = it.AdjustedCubicMeterGroundWaterForAgriculture,
                    AdjustedCubicMeterGroundWaterForService = it.AdjustedCubicMeterGroundWaterForService,
                    AdjustedCubicMeterGroundWaterForProduct = it.AdjustedCubicMeterGroundWaterForProduct,
                    AdjustedCubicMeterGroundWaterForDrink = it.AdjustedCubicMeterGroundWaterForDrink,
                    AdjustedCubicMeterSurfaceForAgriculture = it.AdjustedCubicMeterSurfaceForAgriculture,
                    AdjustedCubicMeterSurfaceForService = it.AdjustedCubicMeterSurfaceForService,
                    AdjustedCubicMeterSurfaceForProduct = it.AdjustedCubicMeterSurfaceForProduct,
                    AdjustedCubicMeterSurfaceForDrink = it.AdjustedCubicMeterSurfaceForDrink,
                    AdjustedCubicMeterGroundWaterForUse = it.AdjustedCubicMeterGroundWaterForUse,
                    Road = it.Road,
                    IsAdditionalCom = it.IsAdditionalCom,
                })
                .ToList();
                WriteFileAreaCodeProblem(areaCode, finalDataWrite);
                Console.WriteLine($"{areaCode} import done!");
            });
        }

        static void WriteFileAreaCodeProblem(string areaCode, List<DataProcessedExport> dataList)
        {
            var eaInfo = collectionEaData.Find(it => it.Area_Code == areaCode).FirstOrDefault();
            var folderPath = $@"C:\Users\Gun\Desktop\Work\RunResolveWater\Test\AreaCodeProblem";
            var filePath = $@"C:\Users\Gun\Desktop\Work\RunResolveWater\Test\AreaCodeProblem\{eaInfo.CWT_NAME}-{eaInfo.AMP_NAME}-{eaInfo.TAM_NAME}.csv";

            if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(dataList);
            }
        }
    }
}
