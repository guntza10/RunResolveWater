using System;
using Newtonsoft.Json;

namespace Test.models
{
    public class ResultDataAreaCode
    {
        // AreaCode
        public string Id { get; set; }
        [JsonProperty("REG")]
        public string REG { get; set; }
        [JsonProperty("REG_NAME")]
        public string REG_NAME { get; set; }
        [JsonProperty("CWT")]
        public string CWT { get; set; }
        [JsonProperty("CWT_NAME")]
        public string CWT_NAME { get; set; }
        [JsonProperty("AMP")]
        public string AMP { get; set; }
        [JsonProperty("AMP_NAME")]
        public string AMP_NAME { get; set; }
        [JsonProperty("TAM")]
        public string TAM { get; set; }
        [JsonProperty("TAM_NAME")]
        public string TAM_NAME { get; set; }
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
        public double? CountGroundWaterUnit { get; set; }
        public double? CountGroundWaterCom { get; set; }
        public double? WaterSourcesUnit { get; set; }
        public double? WaterSourcesCom { get; set; }
    }
}