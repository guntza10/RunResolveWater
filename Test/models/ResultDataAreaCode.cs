using System;

namespace Test.models
{
    public class ResultDataAreaCode
    {
        // AreaCode
        public string Id { get; set; }
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
        public double? CubicMeterGroundWaterForUse { get; set; }
        public int? CountGroundWaterUnit { get; set; }
        public int? CountGroundWaterCom { get; set; }
        public int? WaterSourcesUnit { get; set; }
        public int? WaterSourcesCom { get; set; }
    }
}