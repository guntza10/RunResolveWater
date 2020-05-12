﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MongoDB.Driver;
using Newtonsoft.Json;
using NSOWater.HotMigration.Models;
using Test.models;

namespace Test
{
    class Program
    {
        private static IMongoCollection<DataProcessed> collectionOldDataProcess { get; set; }
        private static IMongoCollection<DataProcessed> collectionNewDataProcess { get; set; }
        private static IMongoCollection<AmountCommunity> collectionAmountCommunity { get; set; }
        private static IMongoCollection<ResultDataEA> collectionResultDataEA { get; set; }
        private static IMongoCollection<ResultDataAreaCode> collectionResultDataAreaCode { get; set; }
        private static IMongoCollection<ContainerEnlisted> collectionContainerEnlisted { get; set; }
        private static IMongoCollection<EaInfomation> collectionEaData { get; set; }
        private static IMongoCollection<EaApproved> collectionEaApproved { get; set; }
        private static IMongoCollection<ContainerNotFound> collectionContainerNotFound { get; set; }
        private static IMongoCollection<IndexInZip> collectionIndexInZip { get; set; }
        private static IMongoCollection<SurveyData> collectionSurvey { get; set; }
        private static IMongoCollection<Zip> collectionZip { get; set; }
        static void Main(string[] args)
        {
            var mongo = new MongoClient("mongodb://firstclass:Th35F1rstCla55@mongoquickx4h3q4klpbxtq-vm0.southeastasia.cloudapp.azure.com/wdata");
            var database = mongo.GetDatabase("wdata");
            collectionOldDataProcess = database.GetCollection<DataProcessed>("NewDataProcessBKK");
            collectionNewDataProcess = database.GetCollection<DataProcessed>("NewDataProcess");
            collectionAmountCommunity = database.GetCollection<AmountCommunity>("amountCommunity");
            collectionResultDataEA = database.GetCollection<ResultDataEA>("ResultNewDataEAClean");
            collectionResultDataAreaCode = database.GetCollection<ResultDataAreaCode>("ResultNewDataAreaCode");
            collectionEaData = database.GetCollection<EaInfomation>("ea");
            collectionEaApproved = database.GetCollection<EaApproved>("EaApproved");
            collectionContainerEnlisted = database.GetCollection<ContainerEnlisted>("containerEnlisted");
            collectionContainerNotFound = database.GetCollection<ContainerNotFound>("containerNotFound");
            collectionIndexInZip = database.GetCollection<IndexInZip>("IndexInZipFile");
            collectionSurvey = database.GetCollection<SurveyData>("Survey");
            collectionZip = database.GetCollection<Zip>("Zip");
            // // check error 
            // var checkResolve = new CheckResolve();
            // checkResolve.checkResolveIsHouseHold();
            // checkResolve.checkResolveHasntPlumbing();
            // checkResolve.CheckTest();

            // run resolve dataProcess (ระดับ record)
            // ResolveIsHouseHold(); Mongo
            // ResolveIsHouseHoldGoodPlumbing();
            // ResolveCountPopulationOver20000();
            // ResolveAvgWaterHeightCmAndTimeWaterHeightCm(); Mongo
            // ResolveWaterSources(); Mongo
            // ResolveHasntPlumbing();
            // ResolveNewHasntPlumbing();
            // ResolveCountCommunity();
            // ResolveIsGovernmentUsageAndIsGovernmentWaterQuality();

            // run resolve (EA) -> ไปทำ collection sum EA,areacode ก่อน
            // ResolveCountGroundWater();
            // ResolvecountWorkingAge();
            // ResolveFieldCommunity();

            // ResolveCountGroundWaterAndWaterSourcesEA();
            // ResolveCountGroundWaterAndWaterSourcesAreaCode();
            // GetDataAndLookUpForAddAnAddressInfomationInResultDataAreaCode();

            // SetContainerNameForBlobNameNotFound();
            // CheckHasntPlumbing();
            // ResolveHasntPlumbingForResultDataEA();
            // CheckHasntPlumbingForEA();

            // UpdateContainerForMissing();
            var manageMl = new ManageDataMl();
            manageMl.CreateCollectionT();
        }

        // 2.ครัวเรือนทั้งหมด -> IsHouseHold (do) -> ใช้ mongo จะเร็วกว่า (check ก่อนรัน)
        public static void ResolveIsHouseHold()
        {
            Console.WriteLine("Start ResolveIsHouseHold");
            var data = collectionNewDataProcess.Aggregate()
             .Match(it => it.SampleType == "u" && it.IsHouseHold == 0 && it.CountPopulation > 0)
             .Project(it => new
             {
                 id = it._id
             })
             .ToList();
            var count = 0;
            foreach (var item in data)
            {
                count++;
                Console.WriteLine($"Round : {count} / {data.Count}");
                var def = Builders<DataProcessed>.Update
                .Set(it => it.CountPopulation, 0);
                collectionNewDataProcess.UpdateOne(it => it._id == item.id, def);
                Console.WriteLine($"Round : {count} update done!");
            }
            // collectionNewDataProcess.UpdateMany(it => it.SampleType == "u" && it.IsHouseHold == 0, def);
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

        // 3.ครัวเรือนที่มีน้ำประปาคุณภาพดี -> IsHouseHoldGoodPlumbing (do)
        public static void ResolveIsHouseHoldGoodPlumbing()
        {
            Console.WriteLine("Start ResolveIsHouseHoldGoodPlumbing");
            Console.WriteLine("Quering.....");

            var data = collectionNewDataProcess.Aggregate()
            .Match(it => it.SampleType == "b" || it.SampleType == "u")
            .Project(it => new
            {
                Id = it._id,
                SampleType = it.SampleType,
                Area_Code = it.Area_Code,
                IsHouseHold = it.IsHouseHold,
                IsHouseHoldGoodPlumbing = it.IsHouseHoldGoodPlumbing
            })
            .ToList();

            var dataBuilding = data.Where(it => it.SampleType == "b" && it.IsHouseHold != 0)
            .Select(it => new
            {
                Id = it.Id,
                Area_Code = it.Area_Code,
                IsHouseHold = it.IsHouseHold
            })
            .ToList();

            var dataUnit = data.Where(it => it.SampleType == "u" && it.IsHouseHold == 1)
            .Select(it => new DataUnit
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
                    collectionNewDataProcess.UpdateOne(x => x._id == it.Id, def);
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
            var dataUpdate = collectionNewDataProcess.Aggregate()
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
                collectionNewDataProcess.UpdateOne(it => it._id == data._id, def);
                Console.WriteLine($"update done!");
            }
            Console.WriteLine($"All Update Done!");
        }

        //  9.จำนวนบ่อน้ำบาดาล (สน.2) -> CountGroundWater
        public static void ResolveCountGroundWater()
        {
            Console.WriteLine("Start ResolveCountGroundWater");
            Console.WriteLine("Querying.....................");
            var listCom = collectionNewDataProcess.Aggregate()
            .Match(it => it.SampleType == "c")
            .ToList();

            Console.WriteLine($"listCom : {listCom.Count}");

            var dataEACountGroundWaterOver100 = listCom
            .GroupBy(it => it.EA)
            .Select(it => new
            {
                EA = it.Key,
                AreaCode = it.FirstOrDefault().Area_Code,
                sumCountGroundWater = it.Sum(i => i.CountGroundWater)
            })
            .Where(it => it.sumCountGroundWater > 100)
            .ToList();

            Console.WriteLine($"dataEACountGroundWaterOver100 : {dataEACountGroundWaterOver100.Count}");

            var dataEACountGroundWaterComOver100 = collectionResultDataEA.Aggregate()
            .Match(it => it.CountGroundWaterCom > 100)
            .Project(it => new
            {
                EA = it.Id
            })
            .ToList();
            Console.WriteLine($"dataEACountGroundWaterComOver100 : {dataEACountGroundWaterComOver100.Count}");

            var dataAvgCountGroundWater = listCom
            .GroupBy(it => it.Area_Code)
            .Select(it => new
            {
                Area_Code = it.Key,
                sumGroundWaterNotProblem = it.Where(x => !dataEACountGroundWaterOver100.Any(i => i.EA == x.EA))
                .Sum(x => x.CountGroundWater),
                total = it.Where(x => !dataEACountGroundWaterOver100.Any(i => i.EA == x.EA)).Count()
            })
            .Select(it => new
            {
                Area_Code = it.Area_Code,
                avgCountGroundWater = Math.Round(it.sumGroundWaterNotProblem.Value / it.total)
            })
            .ToList();
            Console.WriteLine($"dataAvgCountGroundWater : {dataAvgCountGroundWater.Count}");

            // ส่วน Update
            var count = 0;
            dataEACountGroundWaterOver100.ForEach(it =>
            {
                count++;
                Console.WriteLine($"Round : {count} / {dataEACountGroundWaterOver100.Count},EA = {it.EA}");
                var avg = dataAvgCountGroundWater.FirstOrDefault(x => x.Area_Code == it.AreaCode).avgCountGroundWater;
                var def = Builders<ResultDataEA>.Update
                .Set(x => x.CountGroundWaterCom, avg);
                collectionResultDataEA.UpdateOne(x => x.Id == it.EA, def);
                Console.WriteLine($"EA {it.EA} Update Done!");
            });
            Console.WriteLine("All Update Done!");
        }

        // 10.จำนวนประชากร -> CountPopulation ที่มีค่าเกิน 20000 (do)
        public static void ResolveCountPopulationOver20000()
        {
            Console.WriteLine("Start RunResolveCountPopulation");
            Console.WriteLine("Quering.......");

            var data = collectionNewDataProcess.Aggregate()
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
                    collectionNewDataProcess.UpdateOne(i => i._id == x.Id, def);
                    Console.WriteLine($"Update done!");
                });
            });
        }

        // 11.จำนวนประชากรวัยทำงาน -> countWorkingAge
        public static void ResolvecountWorkingAge()
        {
            Console.WriteLine("Start ResolvecountWorkingAge");
            Console.WriteLine("Querying......................................");
            var data = collectionNewDataProcess.Aggregate()
            .Project(it => new
            {
                EA = it.EA,
                CountWorkingAge = it.CountWorkingAge,
                CountPopulation = it.CountPopulation
            })
            .ToList();

            Console.WriteLine($"data : {data.Count}");

            var listEA = data.Where(it => it.EA != "")
            .GroupBy(it => it.EA)
            .Select(it => new
            {
                Ea = it.Key,
                SumCountWorkingAge = it.Sum(s => s.CountWorkingAge),
                SumCountPopulation = it.Sum(s => s.CountPopulation)
            })
            .ToList();

            Console.WriteLine($"listEA : {listEA.Count}");

            var eaHasProblem = listEA.Where(it => it.SumCountWorkingAge == 0 && it.SumCountPopulation > 0).ToList();
            Console.WriteLine($"eaHasProblem : {eaHasProblem.Count}");

            var dataEAHasProblem = collectionResultDataEA.Aggregate()
            .Match(it => it.CountPopulation > 0 && it.CountWorkingAge == 0)
            .ToList();
            Console.WriteLine($"dataEAHasProblem : {dataEAHasProblem.Count}");

            var dataPercentRegion = listEA.Except(eaHasProblem)
            .GroupBy(it => it.Ea.Substring(0, 1))
            .Select(it =>
            {
                var SumCountWorkingAgeRegion = it.Sum(s => s.SumCountWorkingAge);
                var SumCountPopulationRegion = it.Sum(s => s.SumCountPopulation);
                return new
                {
                    Region = it.Key,
                    percentRegion = SumCountWorkingAgeRegion * 100 / SumCountPopulationRegion
                };
            })
            .ToList();

            Console.WriteLine($"eaNotProblem : {dataPercentRegion.Count}");

            var count = 0;
            eaHasProblem.ForEach(it =>
            {
                count++;
                System.Console.WriteLine($"Round {count} / {eaHasProblem.Count}");
                var newCountWorkingAge = Math.Round(dataPercentRegion
                    .FirstOrDefault(i => i.Region == it.Ea.Substring(0, 1)).percentRegion.Value * it.SumCountPopulation.Value / 100);
                var def = Builders<ResultDataEA>.Update
                .Set(x => x.CountWorkingAge, newCountWorkingAge);
                collectionResultDataEA.UpdateOne(x => x.Id == it.Ea, def);
                Console.WriteLine($"Update done.");
            });
            Console.WriteLine($"All Update done.");
        }

        // 15.พื้นที่ชลประทาน -> FieldCommunity 
        public static void ResolveFieldCommunity()
        {
            Console.WriteLine("Start ResolveFieldCommunity");
            Console.WriteLine("Querying......................................");
            var data = collectionNewDataProcess.Aggregate()
            .Project(it => new
            {
                EA = it.EA,
                FieldCommunity = it.FieldCommunity
            })
            .ToList();
            Console.WriteLine($"data : {data.Count}");

            var listEA = data.Where(it => it.EA != "")
            .GroupBy(it => it.EA)
            .Select(it => new
            {
                EA = it.Key,
                SumFieldCommunity = it.Sum(s => s.FieldCommunity)
            })
            .ToList();
            Console.WriteLine($"listEA : {listEA.Count}");

            var eaProblem = listEA.Where(it => it.SumFieldCommunity >= 300).ToList();
            Console.WriteLine($"eaProblem : {eaProblem.Count}");

            var dataEAHasProblem = collectionResultDataEA.Aggregate()
            .Match(it => it.FieldCommunity >= 300)
            .ToList();
            Console.WriteLine($"dataEAHasProblem : {dataEAHasProblem.Count}");

            var avgReg = listEA.Where(it => !eaProblem.Any(i => i.EA == it.EA))
            .GroupBy(it => it.EA.Substring(0, 1))
            .Select(it =>
             {
                 var sumFieldCommunity = it.Sum(s => s.SumFieldCommunity);
                 var totalFieldCommunity = it.Count();
                 return new
                 {
                     Region = it.Key,
                     avg = sumFieldCommunity.Value / totalFieldCommunity
                 };
             })
             .ToList();
            Console.WriteLine($"avgReg : {avgReg.Count}");
            var count = 0;
            eaProblem.ForEach(it =>
            {
                count++;
                System.Console.WriteLine($"Round {count} / {eaProblem.Count}");
                var dataAvg = avgReg.FirstOrDefault(i => i.Region == it.EA.Substring(0, 1)).avg;
                var def = Builders<ResultDataEA>.Update
                .Set(x => x.FieldCommunity, dataAvg);
                collectionResultDataEA.UpdateOne(x => x.Id == it.EA, def);
                Console.WriteLine($"Update done.");
            });
            Console.WriteLine($"All Update done.");
        }

        //16.ระดับความลึกของน้ำท่วม (ในเขตที่อยู่อาศัย) -> AvgWaterHeightCm (do) -> ใช้ Mongo จะเร็วกว่า
        //17.ระยะเวลาที่น้ำท่วมขัง (ในเขตที่อยู่อาศัย) -> TimeWaterHeightCm (do)
        public static void ResolveAvgWaterHeightCmAndTimeWaterHeightCm()
        {
            Console.WriteLine("Start ResolveAvgWaterHeightCm");
            var def = Builders<DataProcessed>.Update
            .Set(it => it.AvgWaterHeightCm, 0)
            .Set(it => it.TimeWaterHeightCm, 0);
            Console.WriteLine("Updating.......");
            // collectionNewDataProcess.UpdateMany(it => it.EA != "" || it.Area_Code != null && it.AvgWaterHeightCm == 0 || it.TimeWaterHeightCm == 0, def);
            collectionNewDataProcess.UpdateMany(it => it.AvgWaterHeightCm == 0 || it.TimeWaterHeightCm == 0, def);
            Console.WriteLine("update done");
        }

        // 18.ระยะเวลาที่มีน้ำประปาใช้ (HasntPlumbing)
        public static void ResolveHasntPlumbing()
        {
            Console.WriteLine($"Start ResolveHasntPlumbing");
            Console.WriteLine($"Querying..................");
            var dataWrong = collectionNewDataProcess.Aggregate()
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
                collectionNewDataProcess.UpdateMany(it => listUpate.Contains(it._id), def);
                skip += 1000;
                countUpdate += listUpate.Count;
                Console.WriteLine($"count data already update : {countUpdate} / {listIdWrongWillUpdate.Count}");
            }
            Console.WriteLine($"All Update Done!");
        }

        // 18.ระยะเวลาที่มีน้ำประปาใช้ (HasntPlumbing)
        public static void ResolveNewHasntPlumbing()
        {
            Console.WriteLine("Start ResolveNewHasntPlumbing");

            var data = collectionNewDataProcess.Aggregate()
            .Match(it => it.IsHouseHold != 0 ||
            it.IsAgriculture != 0 ||
            it.IsAllFactorial != 0 ||
            it.IsAllCommercial != 0)
            .Project(it => new
            {
                Id = it._id,
                Area_Code = it.Area_Code,
                HasntPlumbing = it.HasntPlumbing
            })
            .ToList();

            Console.WriteLine($"data {data.Count}");

            var dataHasntPlumbing0 = data.Where(it => it.HasntPlumbing == 0)
            .Select(it => it.Id)
            .ToList();

            Console.WriteLine($"dataHasntPlumbing0 {dataHasntPlumbing0.Count}");

            Console.WriteLine($"start Updata Case HasntPlumbing == 0");

            var defHasntPlumbing0 = Builders<DataProcessed>.Update
            .Set(it => it.HasntPlumbing, 12);
            collectionNewDataProcess.UpdateMany(it => dataHasntPlumbing0.Contains(it._id), defHasntPlumbing0);

            Console.WriteLine($"Updata Case HasntPlumbing == 0 Done!");

            var dataHasntPlumbingbetween0To6 = data.Where(it => it.HasntPlumbing > 0 && it.HasntPlumbing < 6)
             .Select(it => new
             {
                 Id = it.Id,
                 Area_Code = it.Area_Code
             })
             .ToList();

            Console.WriteLine($"dataHasntPlumbingbetween0To6 : {dataHasntPlumbingbetween0To6.Count}");

            var avgAreaCode = data.Where(it => it.HasntPlumbing >= 6)
             .GroupBy(it => it.Area_Code)
             .Select(it => new
             {
                 Area_Code = it.Key,
                 Avg = it.Sum(i => i.HasntPlumbing) / it.Count()
             })
             .ToList();

            Console.WriteLine($"avgAreaCode : {avgAreaCode.Count}");

            var count = 0;
            dataHasntPlumbingbetween0To6.ForEach(it =>
            {
                count++;
                Console.WriteLine($"Round : {count} / {dataHasntPlumbingbetween0To6.Count}");
                var avg = avgAreaCode.FirstOrDefault(i => i.Area_Code == it.Area_Code).Avg;
                var def = Builders<DataProcessed>.Update
                .Set(i => i.HasntPlumbing, avg);
                collectionNewDataProcess.UpdateOne(i => i._id == it.Id, def);
                Console.WriteLine($"{it.Id} update done");
            });
            Console.WriteLine("All Update Done!");
        }

        //23.แหล่งน้ำขนาดใหญ่ กลาง และเล็ก (สน.2) -> WaterSources (do) -> ใช้ mongo
        public static void ResolveWaterSources()
        {
            var def = Builders<DataProcessed>.Update
            .Set(it => it.WaterSources, 0);
            // collectionNewDataProcess.UpdateMany(it => it.EA != null || it.Area_Code != null && it.WaterSources < 1260 && it.SampleType == "c", def);
            collectionNewDataProcess.UpdateMany(it => it.SampleType == "c" && it.WaterSources < 1260, def);
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

            var listCom = collectionNewDataProcess.Aggregate()
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

            var dataArea = collectionNewDataProcess.Aggregate(new AggregateOptions { AllowDiskUse = true })
            .Project(it => new
            {
                Area_Code = it.Area_Code,
                EA = it.EA,
                SampleType = it.SampleType
            })
            .ToList();

            var listEACommu = dataArea.GroupBy(it => it.Area_Code)
            .Select(x => new
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

                    for (int i = 0; i < differnt; i++)
                    {
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
                            CommunityNatureDisaster = 0,
                            IsAdditionalCom = true
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
                    collectionNewDataProcess.InsertMany(dataProcessUpdate);
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

        // 20.สถานที่ราชการที่มีน้ำประปาใช้ -> IsGovernmentUsage
        // 21.สถานที่ราชการที่มีน้ำประปาที่มีคุณภาพมาตรฐาน -> IsGovernmentWaterQuality
        public static void ResolveIsGovernmentUsageAndIsGovernmentWaterQuality()
        {
            var listGovernment = collectionNewDataProcess.Find(it => it.IsGovernment == 1).ToList();
            Console.WriteLine($"List Government done : {listGovernment.Count}");

            var eaGroup = listGovernment.GroupBy(it => it.EA).ToList();
            Console.WriteLine($"EA Group complete : {eaGroup.Count}");
            var countEA = 0;
            foreach (var ea in eaGroup)
            {
                countEA++;
                Console.WriteLine($"Process in EA : {ea.Key}");
                Console.WriteLine($"EA round : {countEA}/{eaGroup.Count}");
                var roadGroup = ea.GroupBy(it => it.Road).ToList();
                Console.WriteLine($"Road Group complete : {roadGroup.Count}");
                var countRoad = 0;

                foreach (var road in roadGroup)
                {
                    countRoad++;
                    Console.WriteLine($"road round : {countRoad}/{roadGroup.Count}");
                    var validationWaterUsage = road.Any(it => it.IsGovernmentUsage == 1);
                    var validationWaterQuality = road.Any(it => it.IsGovernmentWaterQuality == 1);
                    if (validationWaterUsage)
                    {
                        var listWrites = new List<WriteModel<DataProcessed>>();
                        var filterDefinition = Builders<DataProcessed>.Filter
                                .Where(it => it.EA == ea.Key && it.Road == road.Key);

                        if (validationWaterUsage && validationWaterQuality)
                        {
                            var updateDefinitionAll = Builders<DataProcessed>.Update
                                .Set(it => it.IsGovernmentUsage, 1)
                                .Set(it => it.IsGovernmentWaterQuality, 1);
                            listWrites.Add(new UpdateManyModel<DataProcessed>(filterDefinition, updateDefinitionAll));
                            Console.WriteLine("Update WaterUsage and WaterQuality");
                        }
                        else
                        {
                            var updateDefinitionOnlyUsage = Builders<DataProcessed>.Update
                                .Set(it => it.IsGovernmentUsage, 1);
                            listWrites.Add(new UpdateManyModel<DataProcessed>(filterDefinition, updateDefinitionOnlyUsage));
                            Console.WriteLine("Update Only WaterUsage");
                        }
                        collectionNewDataProcess.BulkWrite(listWrites);
                        Console.WriteLine("Update data Complete");
                    }
                }
            }
        }


        // ------------------------------------------------------Not use in resolve Data----------------------------------------------------------------------------->
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

        // Resolve countGroundWaterAndWaterSourcesEA
        public static void ResolveCountGroundWaterAndWaterSourcesEA()
        {
            Console.WriteLine("Start ResolveCountGroundWaterAndWaterSourcesEA");
            Console.WriteLine("Querying......................................");
            var listEaUpdate = collectionResultDataEA.Aggregate()
            .Project(it => new
            {
                EA = it.Id
            })
            .ToList();

            Console.WriteLine($"listEAUpdate : {listEaUpdate.Count}");

            var data = collectionNewDataProcess.Aggregate(new AggregateOptions { AllowDiskUse = true })
            .Project(it => new
            {
                EA = it.EA,
                SampleType = it.SampleType,
                CountGroundWater = it.CountGroundWater,
                WaterSources = it.WaterSources
            })
            .ToList();

            Console.WriteLine($"data : {data.Count}");

            var dataEA = data.GroupBy(it => it.EA)
            .Select(it => new
            {
                EA = it.Key,
                listData = it.Select(i => new
                {
                    SampleType = i.SampleType,
                    CountGroundWater = i.CountGroundWater,
                    WaterSources = i.WaterSources
                })
            })
            .Where(it => it.EA != "")
            .ToList();

            Console.WriteLine($"dataEA : {dataEA.Count}");

            var count = 0;
            listEaUpdate.ForEach(it =>
            {
                count++;
                Console.WriteLine($"Round : {count} / {listEaUpdate.Count}, EA = {it.EA}");
                var listDataEA = dataEA.FirstOrDefault(i => i.EA == it.EA).listData;
                var dataUnit = listDataEA.Where(i => i.SampleType == "u").ToList();
                var dataCom = listDataEA.Where(i => i.SampleType == "c").ToList();

                var countGroundWaterUnit = dataUnit.Sum(i => i.CountGroundWater);
                var countGroundWaterCom = dataCom.Sum(i => i.CountGroundWater);
                var waterSourcesUnit = dataUnit.Sum(i => i.WaterSources);
                var waterSourcesCom = dataCom.Sum(i => i.WaterSources);

                var def = Builders<ResultDataEA>.Update
                .Set(i => i.CountGroundWaterUnit, countGroundWaterUnit)
                .Set(i => i.CountGroundWaterCom, countGroundWaterCom)
                .Set(i => i.WaterSourcesUnit, waterSourcesUnit)
                .Set(i => i.WaterSourcesCom, waterSourcesCom);
                collectionResultDataEA.UpdateOne(i => i.Id == it.EA, def);
                Console.WriteLine($"EA {it.EA} Update Done!");
            });
            Console.WriteLine("All Update Done!");
        }

        // Resolve CountGroundWaterAndWaterSourcesAreaCode
        public static void ResolveCountGroundWaterAndWaterSourcesAreaCode()
        {
            Console.WriteLine("Start ResolveCountGroundWaterAndWaterSourcesAreaCode");
            Console.WriteLine("Querying............................................");

            var listAreaCodeUpdate = collectionResultDataAreaCode.Aggregate()
            .Project(it => new
            {
                Area_Code = it.Id
            })
            .ToList();
            Console.WriteLine($"listAreaCodeUpdate : {listAreaCodeUpdate.Count}");

            var data = collectionNewDataProcess.Aggregate(new AggregateOptions { AllowDiskUse = true })
            .Match(it => it.Area_Code != "")
            .Project(it => new
            {
                Area_Code = it.Area_Code,
                SampleType = it.SampleType,
                CountGroundWater = it.CountGroundWater,
                WaterSources = it.WaterSources
            })
            .ToList();
            Console.WriteLine($"data : {data.Count}");

            var dataAreaCode = data.GroupBy(it => it.Area_Code)
            .Select(it => new
            {
                Area_Code = it.Key,
                listData = it.Select(i => new
                {
                    SampleType = i.SampleType,
                    CountGroundWater = i.CountGroundWater,
                    WaterSources = i.WaterSources
                })
            })
            .ToList();

            Console.WriteLine($"dataAreaCode : {dataAreaCode.Count}");

            var count = 0;
            listAreaCodeUpdate.ForEach(it =>
            {
                count++;
                Console.WriteLine($"Round : {count} / {listAreaCodeUpdate.Count} , Area_Code = {it.Area_Code}");
                var listDataAreaCode = dataAreaCode.FirstOrDefault(i => i.Area_Code == it.Area_Code).listData;
                var dataUnit = listDataAreaCode.Where(i => i.SampleType == "u").ToList();
                var dataCom = listDataAreaCode.Where(i => i.SampleType == "c").ToList();

                var countGroundWaterUnit = dataUnit.Sum(i => i.CountGroundWater);
                var countGroundWaterCom = dataCom.Sum(i => i.CountGroundWater);
                var waterSourcesUnit = dataUnit.Sum(i => i.WaterSources);
                var waterSourcesCom = dataCom.Sum(i => i.WaterSources);

                var def = Builders<ResultDataAreaCode>.Update
                .Set(i => i.CountGroundWaterUnit, countGroundWaterUnit)
                .Set(i => i.CountGroundWaterCom, countGroundWaterCom)
                .Set(i => i.WaterSourcesUnit, waterSourcesUnit)
                .Set(i => i.WaterSourcesCom, waterSourcesCom);
                collectionResultDataAreaCode.UpdateOne(i => i.Id == it.Area_Code, def);
                Console.WriteLine($"Area_Code {it.Area_Code} Update Done!");
            });
            Console.WriteLine("Update Done!");
        }

        // resolve add filed info EA 
        public static void GetDataAndLookUpForAddAnAddressInfomationInResultDataEa()
        {
            Console.WriteLine("Start Add Info of DataEA");
            Console.WriteLine("Programe start Process...");
            var eaRawData = collectionEaData.Aggregate()
                .Project(it => new
                {
                    EA = it._id,
                    Area_Code = it.Area_Code,
                    REG = it.REG,
                    REG_NAME = it.REG_NAME,
                    CWT = it.CWT,
                    CWT_NAME = it.CWT_NAME,
                    AMP = it.AMP,
                    AMP_NAME = it.AMP_NAME,
                    TAM = it.TAM,
                    TAM_NAME = it.TAM_NAME
                }).ToList();

            Console.WriteLine($"eaRawData : {eaRawData.Count}");

            var eaExist = collectionResultDataEA.Aggregate()
            .Project(it => new
            {
                EA = it.Id
            })
            .ToList();
            Console.WriteLine($"eaExist : {eaExist.Count}");

            var count = 0;
            eaExist.ForEach(it =>
            {
                count++;
                Console.WriteLine($"Round : {count} / {eaExist.Count()}, EA = {it.EA}");
                var dataInfo = eaRawData.FirstOrDefault(i => i.EA == it.EA);
                var defEA = Builders<ResultDataEA>.Update
                    .Set(data => data.Area_Code, dataInfo.Area_Code)
                    .Set(data => data.REG, dataInfo.REG)
                    .Set(data => data.REG_NAME, dataInfo.REG_NAME)
                    .Set(data => data.CWT, dataInfo.CWT)
                    .Set(data => data.CWT_NAME, dataInfo.CWT_NAME)
                    .Set(data => data.AMP, dataInfo.AMP)
                    .Set(data => data.AMP_NAME, dataInfo.AMP_NAME)
                    .Set(data => data.TAM, dataInfo.TAM)
                    .Set(data => data.TAM_NAME, dataInfo.TAM_NAME);
                collectionResultDataEA.UpdateOne(colldata => colldata.Id == it.EA, defEA);
                Console.WriteLine($"EA {it.EA} Update Done!");
            });
            Console.WriteLine($"All Update Done!");
        }

        // resolve add filed info Area
        public static void GetDataAndLookUpForAddAnAddressInfomationInResultDataAreaCode()
        {
            Console.WriteLine("Start Add Info of DataArea");
            Console.WriteLine("Programe start Process...");
            var eaRawData = collectionEaData.Aggregate()
               .Project(it => new
               {
                   Area_Code = it.Area_Code,
                   REG = it.REG,
                   REG_NAME = it.REG_NAME,
                   CWT = it.CWT,
                   CWT_NAME = it.CWT_NAME,
                   AMP = it.AMP,
                   AMP_NAME = it.AMP_NAME,
                   TAM = it.TAM,
                   TAM_NAME = it.TAM_NAME
               }).ToList();
            Console.WriteLine($"eaRawData : {eaRawData.Count}");

            var areaExist = collectionResultDataAreaCode.Aggregate()
            .Project(it => new
            {
                Area_Code = it.Id
            })
            .ToList();
            Console.WriteLine($"areaExist  : {areaExist.Count}");

            var rawDataAreaCode = eaRawData.GroupBy(it => it.Area_Code)
                .Select(it => new
                {
                    Area_Code = it.Key,
                    REG = it.FirstOrDefault().REG,
                    REG_NAME = it.FirstOrDefault().REG_NAME,
                    CWT = it.FirstOrDefault().CWT,
                    CWT_NAME = it.FirstOrDefault().CWT_NAME,
                    AMP = it.FirstOrDefault().AMP,
                    AMP_NAME = it.FirstOrDefault().AMP_NAME,
                    TAM = it.FirstOrDefault().TAM,
                    TAM_NAME = it.FirstOrDefault().TAM_NAME
                }).ToList();
            Console.WriteLine($"rawDataAreaCode : {rawDataAreaCode.Count}");

            var count = 0;
            areaExist.ForEach(it =>
            {
                count++;
                Console.WriteLine($"Round : {count} / {areaExist.Count()}");
                var dataInfo = rawDataAreaCode.FirstOrDefault(i => i.Area_Code == it.Area_Code);
                var defArea = Builders<ResultDataAreaCode>.Update
                        .Set(data => data.REG, dataInfo.REG)
                        .Set(data => data.REG_NAME, dataInfo.REG_NAME)
                        .Set(data => data.CWT, dataInfo.CWT)
                        .Set(data => data.CWT_NAME, dataInfo.CWT_NAME)
                        .Set(data => data.AMP, dataInfo.AMP)
                        .Set(data => data.AMP_NAME, dataInfo.AMP_NAME)
                        .Set(data => data.TAM, dataInfo.TAM)
                        .Set(data => data.TAM_NAME, dataInfo.TAM_NAME);
                collectionResultDataAreaCode.UpdateOne(x => x.Id == it.Area_Code, defArea);
                Console.WriteLine($"area {it.Area_Code} Update Done!");
            });
            Console.WriteLine("All Update Done!");
        }

        public static void ResolveUpdateHasntPlumbingForEA()
        {
            Console.WriteLine("Start ResolveUpdateHasntPlumbingForEA");

            var listEAUpdate = collectionResultDataEA.Aggregate()
            .Project(it => new
            {
                EA = it.Id
            })
            .ToList();

            Console.WriteLine($"listEAUpdate : {listEAUpdate.Count}");

            var data = collectionNewDataProcess.Aggregate(new AggregateOptions { AllowDiskUse = true })
            .Project(it => new
            {
                EA = it.EA,
                SampleType = it.SampleType,
                IsHouseHold = it.IsHouseHold,
                IsAgriculture = it.IsAgriculture,
                IsAllFactorial = it.IsAllFactorial,
                IsAllCommercial = it.IsAllCommercial,
                HasntPlumbing = it.HasntPlumbing,
            })
            .ToList();

            Console.WriteLine($"data : {data.Count}");

            var listEASumHasntPlumbing = data.GroupBy(it => it.EA)
            .Select(it => new
            {
                EA = it.Key,
                sumHasntPlumbing = it.Where(i => i.SampleType == "u" && (i.IsHouseHold != 0
                || i.IsAgriculture != 0
                || i.IsAllFactorial != 0
                || i.IsAllCommercial != 0))
                .Sum(i => i.HasntPlumbing),
                total = it.Count(i => i.SampleType == "u" && (i.IsHouseHold != 0
                || i.IsAgriculture != 0
                || i.IsAllFactorial != 0
                || i.IsAllCommercial != 0))
            })
            .Select(it =>
            {
                var avg = (it.sumHasntPlumbing == 0 && it.total == 0) ? 0 : it.sumHasntPlumbing / it.total;
                return new
                {
                    EA = it.EA,
                    Avg = avg
                };
            })
            .ToList();

            Console.WriteLine($"listEASumHasntPlumbing : {listEASumHasntPlumbing.Count}");

            var count = 0;
            listEAUpdate.ForEach(ea =>
            {
                count++;
                Console.WriteLine($"Round : {count} / {listEAUpdate.Count}, EA = {ea.EA}");
                var newHasntPlumbing = listEASumHasntPlumbing.FirstOrDefault(it => it.EA == ea.EA).Avg.Value;
                var def = Builders<ResultDataEA>.Update
                .Set(it => it.HasntPlumbing, newHasntPlumbing);
                collectionResultDataEA.UpdateOne(it => it.Id == ea.EA, def);
                Console.WriteLine("Update Done!");
            });
            Console.WriteLine("All Update Done!");
        }

        public static void DataResultEAAddField()
        {
            Console.WriteLine("Start DataResultEAAddField");
            var dataEA = collectionEaData.Aggregate()
            .Project(it => new
            {
                Id = it._id,
                MUN = it.MUN,
                MUN_NAME = it.MUN_NAME,
                TAO = it.TAO,
                TAO_NAME = it.TAO_NAME,
                EA = it.EA,
                VIL = it.VIL,
                VIL_NAME = it.VIL_NAME
            })
            .ToList();

            Console.WriteLine($"dataEA : {dataEA.Count}");

            var dataResultEA = collectionResultDataEA.Aggregate()
            .Project(it => new
            {
                EA = it.Id
            })
            .ToList();

            Console.WriteLine($"dataResultEA : {dataResultEA.Count}");

            var count = 0;
            dataResultEA.ForEach(data =>
            {
                count++;
                Console.WriteLine($"Round {count} / {dataResultEA.Count}, EA = {data.EA}");
                var eaInfo = dataEA.FirstOrDefault(it => it.Id == data.EA);
                var def = Builders<ResultDataEA>.Update
                .Set(it => it.MUN, eaInfo.MUN)
                .Set(it => it.MUN_NAME, eaInfo.MUN_NAME)
                .Set(it => it.TAO, eaInfo.TAO)
                .Set(it => it.TAO_NAME, eaInfo.TAO_NAME)
                .Set(it => it.EA, eaInfo.EA)
                .Set(it => it.VIL, eaInfo.VIL)
                .Set(it => it.VIL_NAME, eaInfo.VIL_NAME);
                collectionResultDataEA.UpdateOne(it => it.Id == data.EA, def);
                Console.WriteLine("Update Done!");
            });
            Console.WriteLine("All Update Done!");
        }

        public static void AddFieldEaInfoForDataProcess()
        {
            Console.WriteLine("Start AddFieldEaInfoForDataProcess");

            var dataEA = collectionEaData.Aggregate()
            .Project(it => new
            {
                EA = it._id,
                REG = it.REG,
                REG_NAME = it.REG_NAME,
                CWT = it.CWT,
                CWT_NAME = it.CWT_NAME,
                AMP = it.AMP,
                AMP_NAME = it.AMP_NAME,
                TAM = it.TAM,
                TAM_NAME = it.TAM_NAME
            })
            .ToList();

            Console.WriteLine($"dataEA : {dataEA.Count}");

            var eaDataProcess = collectionNewDataProcess.Aggregate()
            .Project(it => new
            {
                EA = it.EA
            })
            .ToList();

            Console.WriteLine($"eaDataProcess : {eaDataProcess.Count}");

            var ea = eaDataProcess.Where(it => it.EA != "")
            .Distinct()
            .ToList();

            Console.WriteLine($"ea : {ea.Count}");

            var count = 0;
            ea.ForEach(data =>
            {
                count++;
                Console.WriteLine($"Round : {count} / {ea.Count} , EA = {data.EA}");
                var eaInfo = dataEA.FirstOrDefault(it => it.EA == data.EA);

                var def = Builders<DataProcessed>.Update
                .Set(it => it.REG, eaInfo.REG)
                .Set(it => it.REG_NAME, eaInfo.REG_NAME)
                .Set(it => it.CWT, eaInfo.CWT)
                .Set(it => it.CWT_NAME, eaInfo.CWT_NAME)
                .Set(it => it.AMP, eaInfo.AMP)
                .Set(it => it.AMP_NAME, eaInfo.AMP_NAME)
                .Set(it => it.TAM, eaInfo.TAM)
                .Set(it => it.TAM_NAME, eaInfo.TAM_NAME);

                collectionNewDataProcess.UpdateMany(it => it.EA == data.EA, def);
                Console.WriteLine("Update Done!");

            });
            Console.WriteLine("All Update Done!");
        }

        public static void InsertEaApproved()
        {
            Console.WriteLine("Start Insert EaApproved");
            var dataProcess = collectionNewDataProcess.Aggregate()
            .Project(it => new
            {
                EA = it.EA
            })
            .ToList();

            var eaApproved = dataProcess.GroupBy(it => it.EA).Select(it => new EaApproved
            {
                _id = Guid.NewGuid().ToString(),
                EA = it.Key
            })
            .ToList();
            Console.WriteLine($"eaApproved : {eaApproved.Count}");

            var skip = 0;
            var countDataInsert = 0;
            var round = 0;
            while (skip < eaApproved.Count)
            {
                round++;
                Console.WriteLine($"Round : {round} / {eaApproved.Count}");
                var final = eaApproved.Skip(skip).Take(1000).ToList();
                collectionEaApproved.InsertMany(final);
                skip += 1000;
                countDataInsert += final.Count;
                Console.WriteLine($"total insert already : {countDataInsert} / {eaApproved.Count}");
            }
            Console.WriteLine("EaApproved Insert Done!");
        }

        public static void CreateBlobNotFound()
        {
            var listBlobNotFound = collectionContainerEnlisted.Aggregate()
             .Match(it => it.IsFound == false)
             .Project(it => new
             {
                 ListBlob = it.ListBlob
             })
             .ToList();

            Console.WriteLine($"listBlobNotFound : {listBlobNotFound.Count}");

            var dataWillInsert = listBlobNotFound.Select(it => it.ListBlob).ToList();

            var count = 0;
            var countInsert = 0;
            dataWillInsert.ForEach(data =>
            {
                count++;
                Console.WriteLine($"Round : {count} / {dataWillInsert.Count}");
                var finalDataInsert = data.Select(it => new ContainerNotFound
                {
                    Id = it.Remove(it.IndexOf(".txt")),
                    ContainerName = ""
                })
                .ToList();

                collectionContainerNotFound.InsertMany(finalDataInsert);
                countInsert += finalDataInsert.Count;
                Console.WriteLine($"data insert already {countInsert}");
            });
        }

        public static void SetContainerNameForBlobNameNotFound()
        {
            var zips = collectionZip.Find(it => true).ToList();

            Console.WriteLine($"zips : {zips.Count}");

            var dataNotFound = collectionContainerNotFound.Aggregate()
           .Project(it => new
           {
               BlobName = it.Id
           })
           .ToList();

            var listDataNotFound = dataNotFound.Select(it => it.BlobName).ToList();

            Console.WriteLine($"listDataNotFound : {listDataNotFound.Count}");

            var countZip = 0;

            foreach (var data in zips.OrderByDescending(it => it.Id))
            {
                countZip++;
                Console.WriteLine($"Round Zip : {countZip} / {zips.Count}");

                var dataZip = collectionIndexInZip.Aggregate()
                .Match(it => it.ZipName == data.Id)
                .Project(it => new
                {
                    ContainerName = it.ContainerName,
                    listBlob = it.Filelist
                })
                .ToList();

                Console.WriteLine($"count data Zip {data.Id} : {dataZip.Count}");

                var count = 0;
                dataZip.ForEach(data2 =>
                {
                    count++;
                    Console.WriteLine($"round data zip : {count} / {dataZip.Count} , round zip : {countZip}, now zip = {data.Id}");
                    var blobInZip = listDataNotFound.Intersect(data2.listBlob).ToList();

                    if (blobInZip.Any())
                    {
                        Console.WriteLine($"{data2.ContainerName} is found!");

                        var def = Builders<ContainerNotFound>.Update
                        .Set(it => it.ContainerName, data2.ContainerName);

                        collectionContainerNotFound.UpdateMany(it => blobInZip.Contains(it.Id), def);

                        listDataNotFound = listDataNotFound.Except(blobInZip).ToList();
                    }
                    else
                    {
                        Console.WriteLine($"{data2.ContainerName} is not found!");
                    }
                    Console.WriteLine($"listDataNotFound left for update : {listDataNotFound.Count} / {dataNotFound.Count}");
                });

                if (!listDataNotFound.Any())
                {
                    break;
                }
            }
            Console.WriteLine("Update All Done!");
        }

        public static void CheckHasntPlumbing()
        {
            var data = collectionNewDataProcess.Aggregate()
            .Project(it => new
            {
                IsHouseHold = it.IsHouseHold,
                IsAgriculture = it.IsAgriculture,
                IsAllFactorial = it.IsAllFactorial,
                IsAllCommercial = it.IsAllCommercial,
                HasntPlumbing = it.HasntPlumbing
            })
            .ToList();

            var dataNotG = data.Where(it => it.IsHouseHold == 0
            && it.IsAgriculture == 0
            && it.IsAllFactorial == 0
            && it.IsAllCommercial == 0)
            .ToList();
            Console.WriteLine($"dataNotG : {dataNotG.Count}");

            var dataNotGWrong = dataNotG.Where(it => it.HasntPlumbing > 0).ToList();
            var dataNotCorrect = dataNotG.Where(it => it.HasntPlumbing == 0).ToList();
            Console.WriteLine($"dataNotGWrong : {dataNotGWrong.Count}");
            Console.WriteLine($"dataNotCorrect : {dataNotCorrect.Count}");

            var dataSomeG = data.Where(it => it.IsHouseHold != 0 ||
            it.IsAgriculture != 0 ||
            it.IsAllFactorial != 0 ||
            it.IsAllCommercial != 0)
            .ToList();
            Console.WriteLine($"dataSomeG : {dataSomeG.Count}");

            var dataSomeGHasntPlumbing0 = dataSomeG.Where(it => it.HasntPlumbing == 0).ToList();
            var dataSomeGHasntPlumbing0Resolve = dataSomeG.Where(it => it.HasntPlumbing == 12).ToList();
            Console.WriteLine($"dataSomeGHasntPlumbing0 : {dataSomeGHasntPlumbing0.Count}");
            Console.WriteLine($"dataSomeGHasntPlumbing0Resolve : {dataSomeGHasntPlumbing0Resolve.Count}");

            var dataSomeGHasntPlumbing0between6 = dataSomeG.Where(it => it.HasntPlumbing > 0 && it.HasntPlumbing < 6).ToList();
            var dataSomeGHasntPlumbing0between6Resolve = dataSomeG.Where(it => it.HasntPlumbing > 6 && it.HasntPlumbing < 12).ToList();
            Console.WriteLine($"dataSomeGHasntPlumbing0between6 : {dataSomeGHasntPlumbing0between6.Count}");
            Console.WriteLine($"dataSomeGHasntPlumbing0between6Resolve : {dataSomeGHasntPlumbing0between6Resolve.Count}");
        }

        public static void ResolveHasntPlumbingForResultDataEA()
        {
            var dataProcess = collectionNewDataProcess.Aggregate()
            .Match(it => it.HasntPlumbing != 0 && it.IsAdditionalCom == false)
            .Project(it => new
            {
                EA = it.EA,
                HasntPlumbing = it.HasntPlumbing
            })
            .ToList();

            Console.WriteLine($"dataProcess : {dataProcess.Count}");

            var dataEA = dataProcess.GroupBy(it => it.EA)
            .Select(it => new
            {
                EA = it.Key,
                Avg = it.Sum(x => x.HasntPlumbing) / it.Count()
            })
            .ToList();

            Console.WriteLine($"dataEA : {dataEA.Count}");

            var dataResultEA = collectionResultDataEA.Aggregate()
            .Project(it => new
            {
                EA = it.Id
            })
            .ToList();

            Console.WriteLine($"dataResultEA : {dataResultEA.Count}");

            var count = 0;

            dataResultEA.ForEach(data =>
            {
                count++;
                Console.WriteLine($"Round : {count} / {dataResultEA.Count}");

                var dataMatch = dataEA.FirstOrDefault(it => it.EA == data.EA);

                var def = Builders<ResultDataEA>.Update
                .Set(it => it.HasntPlumbing, dataMatch?.Avg ?? 0);
                collectionResultDataEA.UpdateOne(it => it.Id == data.EA, def);
            });

            Console.WriteLine("Update All Done!");
        }

        public static void CheckHasntPlumbingForEA()
        {
            var dataResultEA = collectionResultDataEA.Aggregate()
            .Project(it => new
            {
                EA = it.Id,
                HasntPlumbing = it.HasntPlumbing
            })
            .ToList();

            Console.WriteLine($"dataResultEA : {dataResultEA.Count}");

            var hasntPlumbing0 = dataResultEA.Where(it => it.HasntPlumbing == 0).ToList();
            Console.WriteLine($"hasntPlumbing0: {hasntPlumbing0.Count}");

            var hasntPlumbing0between6 = dataResultEA.Where(it => it.HasntPlumbing > 0 && it.HasntPlumbing < 6).ToList();
            Console.WriteLine($"hasntPlumbing0between6: {hasntPlumbing0between6.Count}");

            var hasntPlumbing6between12 = dataResultEA.Where(it => it.HasntPlumbing > 6 && it.HasntPlumbing < 12).ToList();
            Console.WriteLine($"hasntPlumbing6between12: {hasntPlumbing6between12.Count}");

            var hasntPlumbing12 = dataResultEA.Where(it => it.HasntPlumbing == 12).ToList();
            Console.WriteLine($"hasntPlumbing12: {hasntPlumbing12.Count}");

            var total = hasntPlumbing0.Count + hasntPlumbing0between6.Count + hasntPlumbing6between12.Count + hasntPlumbing12.Count;
            Console.WriteLine($"dataResultEA: {dataResultEA.Count}");
            Console.WriteLine($"total: {total}");
        }

        public static void UpdateContainerForMissing()
        {
            var containerNotFound = collectionContainerNotFound.Aggregate()
            .Match(it => it.ContainerName != "")
            .ToList();

            var groupContainer = containerNotFound.GroupBy(it => it.ContainerName)
            .Select(it => new
            {
                Container = it.Key,
                listBlob = it.Select(i => i.Id.Split(".").FirstOrDefault()).ToList()
            })
            .ToList();

            groupContainer.ForEach(data =>
            {
                var listWrites = new List<WriteModel<SurveyData>>();
                var filterDefinition = Builders<SurveyData>.Filter.Where(it => data.listBlob.Contains(it.SampleId));
                var updateDefinition = Builders<SurveyData>.Update
                .Set(it => it.ContainerName, data.Container);

                listWrites.Add(new UpdateManyModel<SurveyData>(filterDefinition, updateDefinition));

                collectionSurvey.BulkWrite(listWrites);
            });
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



