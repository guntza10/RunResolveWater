namespace Test.models
{
    /// <summary>
    /// TODO: MissingData => เข้า 3 ครั้ง ไม่ให้ความร่วมมือ/ไม่พบ (ไม่เอา CanCompute มาคำนวณ)
    /// </summary>
    public enum StatusCompute
    {
        /// <summary>
        /// คำนวณได้
        /// </summary>
        True = 1,

        /// <summary>
        /// คำนวณไม่ได้
        /// </summary>
        False = 2,

        /// <summary>
        /// ไม่เกี่ยวข้อง
        /// </summary>
        NA = 3,
    }
}