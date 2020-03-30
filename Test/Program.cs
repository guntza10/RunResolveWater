﻿using System;
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
            // checkCountPopulationWrong();
            // ResolveIsHouseHold();
            // ResolveIsHouseHoldGoodPlumbing();
            // ResolveCountPopulationOver20000();
            // ResolveCountCommunity();
            // ResolveAvgWaterHeightCm();
            // ResolveIsHouseHoldHasPlumbingDistrictAndIsHouseHoldHasPlumbingCountryside();
            ResolveHasntPlumbing();
        }

        // 2.ครัวเรือนทั้งหมด -> IsHouseHold 
        public static void ResolveIsHouseHold()
        {
            Console.WriteLine("Start ResolveIsHouseHold");
            var data = collectionOldDataprocess.Aggregate()
             .Match(it => it.SampleType == "u" && it.IsHouseHold == 0 && it.CountPopulation > 0)
             .Project(it => it._id)
             .ToList();
            var count = 0;
            foreach (var item in data)
            {
                count++;
                Console.WriteLine($"Round : {count} / {data.Count}");
                var def = Builders<DataProcessed>.Update
                .Set(it => it.CountPopulation, 0);
                collectionOldDataprocess.UpdateOne(it => it._id == item, def);
                Console.WriteLine($"Round : {count} update done!");
            }
            // collectionOldDataprocess.UpdateMany(it => it.SampleType == "u" && it.IsHouseHold == 0, def);
            Console.WriteLine("all update done");
        }

        public static double percentIsHouseHoldGoodPlumbing(string areaCode, List<DataUnit> dataUnit)
        {
            var dataArea = dataUnit.Where(it => it.AreaCode == areaCode)
            .GroupBy(it => it.AreaCode)
            .Select(it =>
            {
                var sumIsHouseHold = it.Sum(x => x.IsHouseHold);
                var sumIsHouseHoldGoodPlumbing = it.Sum(x => x.IsHouseHoldGoodPlumbing);
                return new
                {
                    percent = sumIsHouseHoldGoodPlumbing * 100 / sumIsHouseHold
                };
            })
            .ToList();

            if (dataArea.Any())
            {
                return dataArea.FirstOrDefault()?.percent.Value ?? 0;
            }
            else
            {
                var dataAmp = dataUnit.Where(it => it.AreaCode.Substring(0, 4) == areaCode.Substring(0, 4))
                .GroupBy(it => it.AreaCode.Substring(0, 4))
                .Select(it =>
                {
                    var sumIsHouseHold = it.Sum(x => x.IsHouseHold);
                    var sumIsHouseHoldGoodPlumbing = it.Sum(x => x.IsHouseHoldGoodPlumbing);
                    return new
                    {
                        percent = sumIsHouseHoldGoodPlumbing * 100 / sumIsHouseHold
                    };
                })
                .ToList();
                return dataAmp.FirstOrDefault()?.percent.Value ?? 0;
            }
        }

        // 3.ครัวเรือนที่มีน้ำประปาคุณภาพดี -> IsHouseHoldGoodPlumbing
        public static void ResolveIsHouseHoldGoodPlumbing()
        {
            Console.WriteLine("Start ResolveIsHouseHoldGoodPlumbing");
            Console.WriteLine("Quering.....");

            var dataBuilding = collectionOldDataprocess.Aggregate()
            .Match(it => it.SampleType == "b" && it.IsHouseHold != 0)
            .Project(it => new
            {
                Id = it._id,
                Area_Code = it.Area_Code,
                IsHouseHold = it.IsHouseHold
            })
            .ToList();
            var dataUnit = collectionOldDataprocess.Aggregate()
            .Match(it => it.SampleType == "u" && it.IsHouseHold == 1)
            .Project(it => new DataUnit
            {
                AreaCode = it.Area_Code,
                IsHouseHold = it.IsHouseHold,
                IsHouseHoldGoodPlumbing = it.IsHouseHoldGoodPlumbing
            })
            .ToList();

            Console.WriteLine("Query Done!");
            Console.WriteLine($"Total Building done : {dataBuilding.Count}");
            Console.WriteLine($"Total Unit done : {dataUnit.Count}");

            var areaGrouping = dataBuilding.GroupBy(it => it.Area_Code).ToList();
            // var areaUse = areaGrouping.Skip(0).ToList();
            Console.WriteLine($"Total area : {areaGrouping.Count}");

            var countAreaUpdate = 0;
            var countTotalBuildingAlreadyUpdate = 0;
            foreach (var areaGroup in areaGrouping)
            {
                countAreaUpdate++;
                Console.WriteLine($"round area : {countAreaUpdate} / {areaGrouping.Count}");

                var areaCode = areaGroup.Key;
                var percent = percentIsHouseHoldGoodPlumbing(areaCode, dataUnit);

                Console.WriteLine($"percent of {areaCode} : {percent}");

                var countBuildingInArea = 0;
                areaGroup.ToList().ForEach(it =>
                {
                    countBuildingInArea++;
                    countTotalBuildingAlreadyUpdate++;
                    Console.WriteLine($"round update building : {countBuildingInArea} / {areaGroup.Count()}");

                    var newIsHouseHoldGoodPlumbing = Math.Round(it.IsHouseHold.Value * percent / 100);

                    Console.WriteLine($"newIsHouseHoldGoodPlumbing : {newIsHouseHoldGoodPlumbing}");

                    var def = Builders<DataProcessed>.Update
                    .Set(x => x.IsHouseHoldGoodPlumbing, newIsHouseHoldGoodPlumbing);
                    collectionOldDataprocess.UpdateOne(x => x._id == it.Id, def);
                    Console.WriteLine($"update done!");
                });
                Console.WriteLine($"count building already update : {countTotalBuildingAlreadyUpdate} / {dataBuilding.Count}");
            }
            Console.WriteLine("all update done!");
        }

        // 5.ครัวเรือนในเขตเมืองที่มีน้ำประปาใช้ (ในเขตเทศบาล) -> IsHouseHoldHasPlumbingDistrict
        // 6.ครัวเรือนในชนบทที่มีน้ำประปาใช้ (นอกเขตเทศบาล) -> IsHouseHoldHasPlumbingCountryside
        public static void ResolveIsHouseHoldHasPlumbingDistrictAndIsHouseHoldHasPlumbingCountryside()
        {
            Console.WriteLine("Start ResolveIsHouseHoldHasPlumbingDistrictAndIsHouseHoldHasPlumbingCountryside");
            Console.WriteLine("Querying.......................................................................");
            var dataUpdate = collectionOldDataprocess.Aggregate()
            .Match(it => it.SampleType == "b" && it.IsHouseHold != 0)
            .Project(it => new
            {
                _id = it._id,
                EA = it.EA,
                IsHouseHold = it.IsHouseHold
            })
            .ToList();
            Console.WriteLine($"Qry done : {dataUpdate.Count}");

            var count = 0;
            foreach (var data in dataUpdate)
            {
                count++;
                Console.WriteLine($"Round : {count} / {dataUpdate.Count}");
                var district = data.EA[7];
                var IsHouseHoldHasPlumbingDistrict = (district == '0' || district == '1') ? data.IsHouseHold : 0;
                var IsHouseHoldHasPlumbingCountryside = (district == '2') ? data.IsHouseHold : 0;
                var def = Builders<DataProcessed>.Update
               .Set(it => it.IsHouseHoldHasPlumbingDistrict, IsHouseHoldHasPlumbingDistrict)
               .Set(it => it.IsHouseHoldHasPlumbingCountryside, IsHouseHoldHasPlumbingCountryside);
                collectionOldDataprocess.UpdateOne(it => it._id == data._id, def);
                Console.WriteLine($"update done!");
            }
            Console.WriteLine($"All Update Done!");
        }

        //  9.จำนวนบ่อน้ำบาดาล (สน.2) -> CountGroundWater
        public static void ResolveCountGroundWater()
        {
            var listCom = collectionOldDataprocess.Aggregate()
            .Match(it => it.SampleType == "c")
            .ToList();

            var dataEACountGroundWaterOver100 = listCom
            .GroupBy(it => it.EA)
            .Select(it => new
            {
                EA = it.Key,
                sumCountGroundWater = it.Sum(i => i.CountGroundWater)
            })
            .Where(it => it.sumCountGroundWater > 100)
            .Select(it => it.EA)
            .ToList();

            var dataAvgCountGroundWater = listCom
            .GroupBy(it => it.Area_Code)
            .Select(it => new
            {
                Area_Code = it.Key,
                sumGroundWaterNotProblem = it.Where(x => !dataEACountGroundWaterOver100.Contains(x.EA))
                .Sum(x => x.CountGroundWater),
                total = it.Where(x => !dataEACountGroundWaterOver100.Contains(x.EA)).Count()
            })
            .Select(it => new
            {
                Area_Code = it.Area_Code,
                avgCountGroundWater = Math.Round(it.sumGroundWaterNotProblem.Value / it.total)
            })
            .ToList();
            // ส่วน Update
        }

        // 10.จำนวนประชากร -> CountPopulation ที่มีค่าเกิน 20000
        public static void ResolveCountPopulationOver20000()
        {
            Console.WriteLine("Start RunResolveCountPopulation");
            Console.WriteLine("Quering.......");

            var data = collectionOldDataprocess.Aggregate()
             .Match(it => it.EA != "" && (it.SampleType == "b" || it.SampleType == "u"))
             .Project(it => new
             {
                 Id = it._id,
                 Ea = it.EA,
                 countPopulation = it.CountPopulation,
                 countWorkingAge = it.CountWorkingAge
             })
             .ToList();

            Console.WriteLine($"Query Done : {data.Count}");

            var EaCountPopulationOver20k = data.GroupBy(it => it.Ea)
            .Select(it => new
            {
                Ea = it.Key,
                sumCountPopulation = it.Sum(x => x.countPopulation)
            })
            .Where(it => it.sumCountPopulation > 20000)
            .Select(it => new
            {
                Ea = it.Ea
            })
            .ToList();

            Console.WriteLine($"EaCountPopulationOver20k : {EaCountPopulationOver20k.Count}");

            var count = 0;
            EaCountPopulationOver20k.ForEach(it =>
            {
                count++;
                Console.WriteLine($"Round Ea : {count} / {EaCountPopulationOver20k.Count}");

                var dataLess20k = data.Where(x => x.Ea == it.Ea && x.countPopulation < 20000).ToList();
                var dataOver20k = data.Where(x => x.Ea == it.Ea && x.countPopulation > 20000).ToList();

                var avg = Math.Round(dataLess20k.Sum(x => x.countPopulation).Value / dataLess20k.Count);

                Console.WriteLine($"Update For dataOver20k");
                var countUpdate = 0;
                dataOver20k.ForEach(x =>
                {
                    countUpdate++;

                    Console.WriteLine($"Round Update : {countUpdate} / {dataOver20k.Count}");
                    Console.WriteLine($"CountPopulaiton : {avg} - CountWokringAge :  {x.countWorkingAge}");

                    var newCountPopulation = (avg >= x.countWorkingAge) ? avg : x.countWorkingAge;
                    Console.WriteLine($"newCountPopulation : {newCountPopulation}");
                    var def = Builders<DataProcessed>.Update
                    .Set(i => i.CountPopulation, newCountPopulation);
                    collectionOldDataprocess.UpdateOne(i => i._id == x.Id, def);
                    Console.WriteLine($"Update done!");
                });
            });
        }

        //16.ระดับความลึกของน้ำท่วม (ในเขตที่อยู่อาศัย) -> AvgWaterHeightCm
        //17.ระยะเวลาที่น้ำท่วมขัง (ในเขตที่อยู่อาศัย) -> TimeWaterHeightCm
        public static void ResolveAvgWaterHeightCm()
        {
            Console.WriteLine("Start ResolveAvgWaterHeightCm");
            var def = Builders<DataProcessed>.Update
            .Set(it => it.AvgWaterHeightCm, 0)
            .Set(it => it.TimeWaterHeightCm, 0);
            Console.WriteLine("Updating.......");
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

        // 39.จำนวนหมู่บ้าน/ชุมชนทั้งหมด -> CountCommunity
        // 14.หมู่บ้านที่มีระบบบำบัดน้ำเสีย -> IsCommunityWaterManagementHasWaterTreatment
        // 40.จำนวนหมู่บ้าน/ชุมชนที่มีอุทกภัย ดินโคลนถล่ม -> CountCommunityHasDisaster
        // 22.หมู่บ้านในพื้นที่น้ำท่วมซ้ำซากที่มีการเตือนภัยและมาตรการช่วยเหลือ -> CommunityNatureDisaster 
        public static void ResolveCountCommunity()
        {
            Console.WriteLine("Start ResolveCountCommunity");
            Console.WriteLine("Quering....................");

            var listCom = collectionOldDataprocess.Aggregate()
            .Match(it => it.SampleType == "c")
            .Project(it => new CommunityUse
            {
                areaCode = it.Area_Code,
                CountCommunityHasDisaster = it.CountCommunityHasDisaster,
                CountCommunity = it.CountCommunity,
                CommunityNatureDisaster = it.CommunityNatureDisaster
            })
            .ToList();

            System.Console.WriteLine($"Qry listCountCommu Done = {listCom.Count}");

            var listAmountCommu = collectionAmountCommunity.Aggregate()
            .Project(it => new
            {
                id = it.Id,
                totalCom = it.totalCom
            })
            .ToList();

            System.Console.WriteLine($"Qry listAmountCommu Done = {listAmountCommu.Count}");

            var listEACommu = collectionOldDataprocess.Aggregate(new AggregateOptions { AllowDiskUse = true })
            .Group(it => it.Area_Code, x => new
            {
                areaCode = x.Key,
                listData = x.Select(it => new
                {
                    Ea = it.EA,
                    sampleType = it.SampleType
                })
            })
            .ToList();

            System.Console.WriteLine($"Qry listEACommu Done = {listEACommu.Count}");

            var groupArea = listCom.GroupBy(it => it.areaCode)
            .Select(it => new CommunityResolve
            {
                areaCode = it.Key,
                SumCountCommunity = it.Sum(s => s.CountCommunity)
            })
            .ToList();

            System.Console.WriteLine($"Qry resultDataArea = {groupArea.Count}");
            var total = new List<JsonModel>();
            var round = 0;
            foreach (var area in groupArea)
            {
                round++;
                System.Console.WriteLine($"Round Area = {round} / {groupArea.Count}, area = {area.areaCode}");
                var totalCom = listAmountCommu.FirstOrDefault(it => it.id == area.areaCode).totalCom;

                var dataProcessUpdate = new List<DataProcessed>();
                if (area.SumCountCommunity < totalCom)
                {
                    var listSampleTypeEa = listEACommu.FirstOrDefault(it => it.areaCode == area.areaCode).listData
                     .GroupBy(it => it.Ea)
                     .Select(it => new
                     {
                         EA_Code = it.Key,
                         SampleTypeExist = it.Any(i => i.sampleType == "c")
                     })
                     .ToList();

                    System.Console.WriteLine($"count Ea don't have commu in {area.areaCode} = {listSampleTypeEa.Count(it => it.SampleTypeExist == false)}");

                    var differnt = totalCom - area.SumCountCommunity;
                    // var dataRecored = new DataProcessed
                    // {
                    //     Area_Code = area.areaCode,
                    //     EA = (listSampleTypeEa.FirstOrDefault(it => it.EA_Code != "" && it.SampleTypeExist == false) != null) ? listSampleTypeEa
                    //     .FirstOrDefault(it => it.EA_Code != "" && it.SampleTypeExist == false).EA_Code :
                    //     listSampleTypeEa.FirstOrDefault(it => it.EA_Code != "" && it.SampleTypeExist == true).EA_Code,
                    //     SampleType = "c",
                    //     CountCommunity = 1,
                    //     IsCommunityWaterManagementHasWaterTreatment = 0,
                    //     CountCommunityHasDisaster = 0,
                    //     CommunityNatureDisaster = 0
                    // };

                    for (int i = 0; i < differnt; i++)
                    {
                        // dataRecored._id = Guid.NewGuid().ToString();
                        dataProcessUpdate.Add(new DataProcessed
                        {
                            _id = Guid.NewGuid().ToString(),
                            Area_Code = area.areaCode,
                            EA = (listSampleTypeEa.FirstOrDefault(it => it.EA_Code != "" && it.SampleTypeExist == false) != null) ? listSampleTypeEa
                            .FirstOrDefault(it => it.EA_Code != "" && it.SampleTypeExist == false).EA_Code :
                            listSampleTypeEa.FirstOrDefault(it => it.EA_Code != "" && it.SampleTypeExist == true).EA_Code,
                            SampleType = "c",
                            CountCommunity = 1,
                            IsCommunityWaterManagementHasWaterTreatment = 0,
                            CountCommunityHasDisaster = 0,
                            CommunityNatureDisaster = 0
                        });
                    }

                    System.Console.WriteLine($"Generate dataProcessUpdate done.");

                    var amountDataUpdateHasDisaster = AmountCountCommunityHasDisaster(listCom, area.areaCode, differnt.Value);
                    if (amountDataUpdateHasDisaster != 0)
                    {
                        dataProcessUpdate.Take((int)amountDataUpdateHasDisaster).ToList().ForEach(it => it.CountCommunityHasDisaster = 1);
                    }
                    System.Console.WriteLine($"Set CountCommunityHasDisaster done.");

                    var count = dataProcessUpdate.Count(it => it.CountCommunityHasDisaster == 1);
                    var amountDataUpdateNatureDisaster = AmountCommunityNatureDisaster(listCom, area.areaCode, count);
                    if (amountDataUpdateNatureDisaster != 0)
                    {
                        dataProcessUpdate.Where(it => it.CountCommunityHasDisaster == 1)
                        .Take((int)amountDataUpdateNatureDisaster)
                        .ToList()
                        .ForEach(it => it.CommunityNatureDisaster = 1);
                    }
                    System.Console.WriteLine($"Set CommunityNatureDisaster done.");
                    collectionOldDataprocess.InsertMany(dataProcessUpdate);
                    System.Console.WriteLine($"{area.areaCode} Insert done!");

                    var getDataProcess = dataProcessUpdate.Select(it => new JsonModel
                    {
                        Id = it._id,
                        Area_Code = it.Area_Code,
                        EA = it.EA,
                        SampleType = it.SampleType,
                        CountCommunity = it.CountCommunity,
                        IsCommunityWaterManagementHasWaterTreatment = it.IsCommunityWaterManagementHasWaterTreatment,
                        CountCommunityHasDisaster = it.CountCommunityHasDisaster,
                        CommunityNatureDisaster = it.CommunityNatureDisaster
                    })
                    .ToList();
                    total.AddRange(getDataProcess);
                }
            }
            var test = total.Where(it => it.CountCommunityHasDisaster != 0 || it.CommunityNatureDisaster != 0).ToList();
            var test2 = total.Where(it => it.CountCommunityHasDisaster != 0 && it.CommunityNatureDisaster != 0).ToList();
            var test3 = total.Where(it => it.CountCommunityHasDisaster != 0 && it.CommunityNatureDisaster == 0).ToList();
            var test5 = total.Where(it => it.CountCommunityHasDisaster == 0 && it.CommunityNatureDisaster == 0).ToList();
            var test4 = total.Where(it => it.EA == "" || it.EA == null).ToList();

            System.Console.WriteLine($"Total Data Insert = {total.Count}");
            System.Console.WriteLine($"data CountCommunityHasDisaster != 0 || CommunityNatureDisaster != 0 => {test.Count}");
            System.Console.WriteLine($"data CountCommunityHasDisaster != 0 && CommunityNatureDisaster != 0 => {test2.Count}");
            System.Console.WriteLine($"data CountCommunityHasDisaster != 0 && CommunityNatureDisaster == 0 => {test3.Count}");
            System.Console.WriteLine($"data CountCommunityHasDisaster == 0 && CommunityNatureDisaster == 0 => {test5.Count}");
            System.Console.WriteLine($"data Ea == null => {test4.Count}");
        }

        // 18.ระยะเวลาที่มีน้ำประปาใช้ (HasntPlumbing)
        public static void ResolveHasntPlumbing()
        {
            Console.WriteLine($"Start ResolveHasntPlumbing");
            Console.WriteLine($"Querying..................");
            var dataWrong = collectionOldDataprocess.Aggregate()
            .Match(it => it.IsHouseHold == 0
            && it.IsAgriculture == 0
            && it.IsAllFactorial == 0
            && it.IsAllCommercial == 0
            && it.HasntPlumbing > 0)
            .ToList();
            Console.WriteLine($"dataWrong : {dataWrong.Count}");
            var listIdWrongWillUpdate = dataWrong.Select(it => it._id).ToList();
            Console.WriteLine($"data hasn't G that HasntPlumbing greater than 0 : {listIdWrongWillUpdate.Count}");
            var skip = 0;
            var countUpdate = 0;
            while (skip <= listIdWrongWillUpdate.Count)
            {
                var listUpate = listIdWrongWillUpdate.Skip(skip).Take(1000).ToList();
                var def = Builders<DataProcessed>.Update
                .Set(it => it.HasntPlumbing, 0);
                collectionOldDataprocess.UpdateMany(it => listUpate.Contains(it._id), def);
                skip += 1000;
                countUpdate += listUpate.Count;
                Console.WriteLine($"count data already update : {countUpdate} / {listIdWrongWillUpdate.Count}");
            }
            Console.WriteLine($"All Update Done!");
        }

        public static double AmountCountCommunityHasDisaster(List<CommunityUse> listCom, string area, double differnt)
        {
            Console.WriteLine("Start AmountCountCommunityHasDisaster");
            var dataArea = listCom.Where(it => it.areaCode == area)
            .GroupBy(it => it.areaCode)
            .Select(it =>
            {
                var SumCountCommunityHasDisaster = it.Sum(s => s.CountCommunityHasDisaster);
                var SumCountCommunity = it.Sum(s => s.CountCommunity);
                return SumCountCommunityHasDisaster * 100 / SumCountCommunity;
            })
            .ToList();

            if (dataArea.Any())
            {
                System.Console.WriteLine($"Data area exist.");
                var percentDataArea = dataArea.FirstOrDefault() ?? 0.0;
                return Math.Round(differnt * percentDataArea / 100);
            }
            else
            {
                System.Console.WriteLine($"Data area not exist.");
                var dataAmp = listCom.Where(it => it.areaCode.Substring(0, 4) == area.Substring(0, 4))
                .GroupBy(it => it.areaCode.Substring(0, 4))
                .Select(it =>
                {
                    var SumCountCommunityHasDisaster = it.Sum(s => s.CountCommunityHasDisaster);
                    var SumCountCommunity = it.Sum(s => s.CountCommunity);
                    return SumCountCommunityHasDisaster * 100 / SumCountCommunity;
                })
                .ToList();
                var percentDataAmp = dataAmp.FirstOrDefault() ?? 0.0;
                return Math.Round(differnt * percentDataAmp / 100);
            }
        }

        public static double AmountCommunityNatureDisaster(List<CommunityUse> listCom, string area, int count)
        {
            Console.WriteLine("Start AmountCommunityNatureDisaster");
            var dataAreaHasDisaster = listCom.Where(it => it.areaCode == area && it.CountCommunityHasDisaster != 0)
            .GroupBy(it => it.areaCode)
            .Select(it =>
            {
                var SumCommunityNatureDisaster = it.Sum(s => s.CommunityNatureDisaster);
                var SumCountCommunity = it.Sum(s => s.CountCommunity);
                return SumCommunityNatureDisaster * 100 / SumCountCommunity;
            })
            .ToList();

            if (dataAreaHasDisaster.Any())
            {
                System.Console.WriteLine($"Data area exist.");
                var percentDataAreaHasDisaster = dataAreaHasDisaster.FirstOrDefault() ?? 0.0;
                return Math.Round(count * percentDataAreaHasDisaster / 100);
            }
            else
            {
                System.Console.WriteLine($"Data area not exist.");
                var dataAmpHasDisaster = listCom.Where(it => it.areaCode.Substring(0, 4) == area.Substring(0, 4) && it.CountCommunityHasDisaster != 0)
               .GroupBy(it => it.areaCode.Substring(0, 4))
               .Select(it =>
               {
                   var SumCommunityNatureDisaster = it.Sum(s => s.CommunityNatureDisaster);
                   var SumCountCommunity = it.Sum(s => s.CountCommunity);
                   return SumCommunityNatureDisaster * 100 / SumCountCommunity;
               })
               .ToList();
                var percentDataAmpHasDisaster = dataAmpHasDisaster.FirstOrDefault() ?? 0.0;
                return Math.Round(count * percentDataAmpHasDisaster / 100);
            }
        }

        public static void checkCountPopulationWrong()
        {
            var checker = new CheckResolve();
            // checker.checkResolveIsHouseHold();
            // checker.checkBuilding();
            // checker.checkIsHouseHoldDistrictCountrySide();
            checker.CheckArea();
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