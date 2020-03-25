namespace Test.models
{
    /// <summary>
    /// TODO: MissingData => เข้า 3 ครั้ง ไม่ให้ความร่วมมือ/ไม่พบ (ไม่เอา CanCompute มาคำนวณ)
    /// </summary>
    public enum StatusProcessed
    {
        /// <summary>
        /// เสร็จสมบูรณ์
        /// </summary>
        Complete = 1,

        /// <summary>
        /// ข้อมูลไม่สมบูรณ์ (บาง field ต้องเข้า ML)
        /// </summary>
        MissingData = 2,

        /// <summary>
        /// ไม่มีข้อมูล
        /// </summary>
        NoInformation = 3,

        /// <summary>
        /// บ้านว่าง/ ร้าง
        /// </summary>
        Vacant = 4,

        /// <summary>
        /// ข้อมูลไม่ครบ
        /// </summary>
        Partial = 5,
    }
}