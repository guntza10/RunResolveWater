using System;
using MongoDB.Bson;

namespace Test.models
{
    public class dataInsert
    {
        public ObjectId Id { get; set; }
        public string SampleId { get; set; }
        public string EA { get; set; }
        public double WaterSource { get; set; }
    }
}