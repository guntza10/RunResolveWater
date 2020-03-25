using System.Collections.Generic;

namespace Test.models
{
    public class EaInfo
    {
        // EA
        public string Ea { get; set; }
        public int countBuilding { get; set; }
        public int countBuildingDoneAll { get; set; }
        public int countBuildingPause { get; set; }
        public int countBuildingEyeOff { get; set; }
        public int countBuildingRefresh { get; set; }
        public int countUnit { get; set; }
        public List<string> listCommunity { get; set; }
    }
}