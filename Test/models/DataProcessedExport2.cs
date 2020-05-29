using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;

namespace Test.models
{
    public class DataProcessedExport2
    {
        // [BsonId]
        // public ObjectId _id { get; set; }
        public string SampleId { get; set; }
        public string SampleType { get; set; }
        public string EA { get; set; }
        public string Area_Code { get; set; }
        public double? CubicMeterGroundWaterForAgriculture { get; set; }
        public double? CubicMeterGroundWaterForService { get; set; }
        public double? CubicMeterGroundWaterForProduct { get; set; }
        public double? CubicMeterGroundWaterForDrink { get; set; }
        public double? CubicMeterPlumbingForAgriculture { get; set; }
        public double? CubicMeterPlumbingForService { get; set; }
        public double? CubicMeterPlumbingForProduct { get; set; }
        public double? CubicMeterPlumbingForDrink { get; set; }
        public double? CubicMeterSurfaceForAgriculture { get; set; }
        public double? CubicMeterSurfaceForService { get; set; }
        public double? CubicMeterSurfaceForProduct { get; set; }
        public double? CubicMeterSurfaceForDrink { get; set; }
    }
}