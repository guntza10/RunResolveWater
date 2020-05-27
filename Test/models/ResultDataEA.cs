using System;
using Newtonsoft.Json;

namespace Test.models
{
    public class ResultDataEA
    {
        // EA
        public string Id { get; set; }
        [JsonProperty("Area_Code")]
        public string Area_Code { get; set; }
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
        [JsonProperty("MUN")]
        public string MUN { get; set; }
        [JsonProperty("MUN_NAME")]
        public string MUN_NAME { get; set; }
        [JsonProperty("TAO")]
        public string TAO { get; set; }
        [JsonProperty("TAO_NAME")]
        public string TAO_NAME { get; set; }
        [JsonProperty("EA")]
        public string EA { get; set; }
        [JsonProperty("VIL")]
        public string VIL { get; set; }
        [JsonProperty("VIL_NAME")]
        public string VIL_NAME { get; set; }

        public double? IsAgriculture { get; set; }
        public double? IsHouseHold { get; set; }
        public double? IsHouseHoldGoodPlumbing { get; set; }
        public double? IsAgricultureHasIrrigationField { get; set; }
        public double? IsHouseHoldHasPlumbingDistrict { get; set; }
        public double? IsHouseHoldHasPlumbingCountryside { get; set; }
        public double? IsFactorialWaterQuality { get; set; }
        public double? IsCommercialWaterQuality { get; set; }
        public double? CountPopulation { get; set; }
        public double? CountWorkingAge { get; set; }
        public double? IsFactorial { get; set; }
        public double? IsFactorialWaterTreatment { get; set; }
        public double? IsCommunityWaterManagementHasWaterTreatment { get; set; }
        public double? FieldCommunity { get; set; }
        public double? AvgWaterHeightCm { get; set; }
        public double? TimeWaterHeightCm { get; set; }
        public double? HasntPlumbing { get; set; }
        public double? IsGovernment { get; set; }
        public double? IsGovernmentUsage { get; set; }
        public double? IsGovernmentWaterQuality { get; set; }
        public double? CommunityNatureDisaster { get; set; }
        public double? IndustryHasWasteWaterTreatment { get; set; }
        public double? PeopleInFloodedArea { get; set; }
        public double? CountCommunity { get; set; }
        public double? CountCommunityHasDisaster { get; set; }
        public double? IsAllHouseHoldCountryside { get; set; }
        public double? IsAllHouseHoldDistrict { get; set; }
        public double? IsAllFactorial { get; set; }
        public double? IsAllCommercial { get; set; }
        public double? CountGroundWaterUnit { get; set; }
        public double? CountGroundWaterCom { get; set; }
        public double? WaterSourcesUnit { get; set; }
        public double? WaterSourcesCom { get; set; }
        public double? Flag { get; set; }
    }
}