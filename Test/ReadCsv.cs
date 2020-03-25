using System.Collections.Generic;
using System.IO;
using Test.models;
using System;
using System.Linq;

namespace Test
{
    public class ReadCsv
    {
        private readonly string FilePath;
        public ReadCsv()
        {
            FilePath = @"DataElection.csv";
        }

        public List<ElectionModel> ReadData()
        {
            var listData = new List<ElectionModel>();
            using (var reader = new StreamReader(FilePath))
            {
                while (!reader.EndOfStream)
                {
                    var dataFromReader = reader.ReadLine();
                    var listDataFromFile = dataFromReader.Split(',');
                    int.TryParse(listDataFromFile[5], out int score);
                    var data = new ElectionModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = listDataFromFile[2],
                        NameParty = listDataFromFile[4],
                        No = listDataFromFile[3],
                        NameArea = $"{listDataFromFile[0]}{listDataFromFile[1]}",
                        Score = score
                    };
                    listData.Add(data);
                    System.Console.WriteLine(dataFromReader);
                }
            }
            return listData;
        }

        public List<ElectionModel> ReadData2()
        {
            var listData = new List<ElectionModel>();
            using (var reader = new StreamReader(FilePath))
            {
                var dataFromRead = reader.ReadToEnd();
                var dataFromSplitLine = dataFromRead.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var item in dataFromSplitLine)
                {
                    var listDataFromFile = item.Split(',');
                    int.TryParse(listDataFromFile[5], out int score);
                    var data = new ElectionModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = listDataFromFile[2],
                        NameParty = listDataFromFile[4],
                        No = listDataFromFile[3],
                        NameArea = $"{listDataFromFile[0]}{listDataFromFile[1]}",
                        Score = score
                    };
                    listData.Add(data);
                }
            }
            return listData;
        }
    }
}

// Convert.ToInt32(listDataFromFile[5])
// Int32.Parse(listDataFromFile[5])
//  int.TryParse(listDataFromFile[5], out int score);

//  listData = data2.Select(it =>
//                   {
//                       var t = it.Split(',').ToList();
//                       int.TryParse(t[5], out int score);
//                       return new ElectionModel
//                       {
//                           Id = Guid.NewGuid().ToString(),
//                           Name = t[2],
//                           NameParty = t[4],
//                           No = t[3],
//                           NameArea = $"{t[0]}{t[1]}",
//                           Score = score
//                       };
//                   }).ToList();