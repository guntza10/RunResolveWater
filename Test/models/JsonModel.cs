using System;

namespace Test.models
{
    public class JsonModel
    {
        public string Id { get; set; }
        public string Area_Code { get; set; }
        public string EA { get; set; }
        public string SampleType { get; set; }
        public double? CountCommunity { get; set; }
        public double? IsCommunityWaterManagementHasWaterTreatment { get; set; }
        public double? CountCommunityHasDisaster { get; set; }
        public double? CommunityNatureDisaster { get; set; }
    }
}