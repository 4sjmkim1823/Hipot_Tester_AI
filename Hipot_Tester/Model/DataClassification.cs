namespace Hipot_Tester.Model
{
    /// <summary>
    /// 데이터 분류 열거형
    /// </summary>
    public enum DataClassification
    {
        /// <summary>
        /// 정상 데이터
        /// </summary>
        Valid,

        /// <summary>
        /// 오류 데이터
        /// </summary>
        Error,

        /// <summary>
        /// 범위 초과 데이터
        /// </summary>
        OutOfRange,

        /// <summary>
        /// 치명적 오류 데이터
        /// </summary>
        Critical,

        /// <summary>
        /// 측정 불가 데이터
        /// </summary>
        Dead
    }
}