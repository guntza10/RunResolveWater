using System;

namespace Test.models
{
    public class JsonModel
    {
        public string buildingId { get; set; }
        // public string houseNo { get; set; }
        // public string name { get; set; }
        public string status { get; set; }
        public int? completedCount { get; set; }
        public int? unitCount { get; set; }
    }
}