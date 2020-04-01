using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test.models
{
    class EaInfomation
    {
        [BsonId]
        [JsonProperty("code")]
        public string _id { get; set; }
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
        [JsonProperty("DISTRICT")]
        public int DISTRICT { get; set; }
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
        [JsonProperty("MAP_STATUS")]
        public int? MAP_STATUS { get; set; }
        [JsonProperty("Building")]
        public int? Building { get; set; }
        [JsonProperty("Household")]
        public int? Household { get; set; }
        [JsonIgnore]
        public int? population { get; set; }
        [JsonIgnore]
        public int? Agricultural_HH { get; set; }
        [JsonIgnore]
        public string ES_BUSI { get; set; }
        [JsonIgnore]
        public string ES_INDUS { get; set; }
        [JsonIgnore]
        public string ES_HOTEL { get; set; }
        [JsonIgnore]
        public string ES_PV_HOS { get; set; }
        [JsonProperty("REMARK")]
        public string REMARK { get; set; }
        [JsonIgnore]
        public string linkcode { get; set; }

        [JsonProperty("Center")]
        public MongoDB.Bson.BsonDocument Center { get; set; }

    }
}
