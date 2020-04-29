using MongoDB.Bson.Serialization.Attributes;

namespace Test.models
{
    public class Reservoir
    {
        [BsonId]
        public string _id { get; set; }

        /// <summary>
        /// ชื่อแหล่งน้ำ
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// รหัสที่ตั้ง
        /// </summary>
        public string Area_Code { get; set; }

        /// <summary>
        /// จังหวัด
        /// </summary>
        public string CWT_NAME { get; set; }

        /// <summary>
        /// อำเภอ
        /// </summary>
        public string AMP_NAME { get; set; }

        /// <summary>
        /// ตำบล
        /// </summary>
        public string TAM_NAME { get; set; }

        /// <summary>
        /// ความจุเก็บกักน้ำ (ล้าน ลบ.ม.)
        /// </summary>
        public double CubicMeter { get; set; }
    }
}