using System;
using System.Collections.Generic;
using System.Linq;

namespace Hipot_Tester.Model
{
    /// <summary>
    /// 데이터 필터링 확장 메서드 클래스
    /// </summary>
    public static class DataFilterExtensions
    {
        /// <summary>
        /// 무효한 데이터를 필터링합니다.
        /// </summary>
        /// <param name="dataList">원본 데이터 리스트</param>
        /// <param name="decValues">DEC 값 배열</param>
        /// <returns>필터링된 데이터 리스트</returns>
        public static List<DataModel> FilterInvalidData(List<DataModel> dataList, double[] decValues)
        {
            if (dataList == null || dataList.Count == 0)
                return new List<DataModel>();

            var filteredData = new List<DataModel>();

            for (int i = 0; i < dataList.Count && i < decValues.Length; i++)
            {
                var data = dataList[i];
                var decValue = decValues[i];

                // 기본적인 유효성 검사
                if (IsValidData(data, decValue))
                {
                    filteredData.Add(data);
                }
            }

            return filteredData;
        }

        /// <summary>
        /// 데이터를 분류합니다.
        /// </summary>
        /// <param name="originalData">원본 데이터</param>
        /// <param name="filteredData">필터링된 데이터</param>
        /// <param name="decValues">DEC 값</param>
        /// <returns>인덱스별 분류 결과</returns>
        public static Dictionary<int, DataClassification> ClassifyData(
            List<DataModel> originalData, 
            List<DataModel> filteredData, 
            double[] decValues)
        {
            var classifications = new Dictionary<int, DataClassification>();

            if (originalData == null)
                return classifications;

            for (int i = 0; i < originalData.Count; i++)
            {
                var data = originalData[i];
                var decValue = i < decValues.Length ? decValues[i] : double.MaxValue;

                classifications[i] = ClassifySingleData(data, decValue);
            }

            return classifications;
        }

        /// <summary>
        /// 단일 데이터의 유효성을 검사합니다.
        /// </summary>
        /// <param name="data">검사할 데이터</param>
        /// <param name="decValue">DEC 값</param>
        /// <returns>유효성 여부</returns>
        private static bool IsValidData(DataModel data, double decValue)
        {
            // NULL 체크
            if (data == null)
                return false;

            // 기본적인 수치 유효성 검사
            if (double.IsNaN(data.Voltage) || double.IsInfinity(data.Voltage) ||
                double.IsNaN(data.Current) || double.IsInfinity(data.Current) ||
                double.IsNaN(data.Resistance) || double.IsInfinity(data.Resistance))
                return false;

            // 음수값 검사 (전압, 전류, 저항은 일반적으로 양수)
            if (data.Voltage < 0 || data.Current < 0 || data.Resistance < 0)
                return false;

            // DEC 값 기반 유효성 검사
            if (double.IsNaN(decValue) || double.IsInfinity(decValue) || decValue > 100)
                return false;

            // 극단적인 값 검사
            if (data.Resistance < 1e-6 || data.Resistance > 1e15)
                return false;

            return true;
        }

        /// <summary>
        /// 단일 데이터를 분류합니다.
        /// </summary>
        /// <param name="data">분류할 데이터</param>
        /// <param name="decValue">DEC 값</param>
        /// <returns>분류 결과</returns>
        private static DataClassification ClassifySingleData(DataModel data, double decValue)
        {
            // Dead 상태 검사 (측정 불가)
            if (data == null || 
                (data.Voltage == 0 && data.Current == 0 && data.Resistance == 0))
            {
                return DataClassification.Dead;
            }

            // NaN 또는 Infinity 값 검사
            if (double.IsNaN(data.Voltage) || double.IsInfinity(data.Voltage) ||
                double.IsNaN(data.Current) || double.IsInfinity(data.Current) ||
                double.IsNaN(data.Resistance) || double.IsInfinity(data.Resistance))
            {
                return DataClassification.Error;
            }

            // Critical 상태 검사 (위험한 수준의 전류)
            if (data.Current > 0.01) // 10mA 초과
            {
                return DataClassification.Critical;
            }

            // Out of Range 검사
            if (data.Resistance < 1e3 || data.Resistance > 1e12)
            {
                return DataClassification.OutOfRange;
            }

            // DEC 값 기반 오류 검사
            if (!double.IsNaN(decValue) && !double.IsInfinity(decValue) && decValue > 50)
            {
                return DataClassification.Error;
            }

            // 음수값 검사
            if (data.Voltage < 0 || data.Current < 0 || data.Resistance < 0)
            {
                return DataClassification.Error;
            }

            // 모든 검사를 통과하면 정상
            return DataClassification.Valid;
        }

        /// <summary>
        /// 통계적 이상치를 감지합니다.
        /// </summary>
        /// <param name="dataList">데이터 리스트</param>
        /// <param name="threshold">이상치 임계값 (Z-Score)</param>
        /// <returns>이상치 인덱스 리스트</returns>
        public static List<int> DetectOutliers(List<DataModel> dataList, double threshold = 3.0)
        {
            var outlierIndices = new List<int>();

            if (dataList == null || dataList.Count == 0)
                return outlierIndices;

            // 각 파라미터별로 Z-Score 계산
            var voltages = dataList.Select(d => d.Voltage).ToArray();
            var currents = dataList.Select(d => d.Current).ToArray();
            var resistances = dataList.Select(d => d.Resistance).ToArray();

            var voltageZScores = ChartHelper.CalculateZScores(voltages);
            var currentZScores = ChartHelper.CalculateZScores(currents);
            var resistanceZScores = ChartHelper.CalculateZScores(resistances);

            // 임계값을 초과하는 데이터의 인덱스 수집
            for (int i = 0; i < dataList.Count; i++)
            {
                if (Math.Abs(voltageZScores[i]) > threshold ||
                    Math.Abs(currentZScores[i]) > threshold ||
                    Math.Abs(resistanceZScores[i]) > threshold)
                {
                    outlierIndices.Add(i);
                }
            }

            return outlierIndices;
        }

        /// <summary>
        /// 데이터 품질 점수를 계산합니다.
        /// </summary>
        /// <param name="dataList">데이터 리스트</param>
        /// <returns>품질 점수 (0-100)</returns>
        public static double CalculateDataQualityScore(List<DataModel> dataList)
        {
            if (dataList == null || dataList.Count == 0)
                return 0;

            var decValues = ChartHelper.CalculateDECValues(dataList);
            var classifications = ClassifyData(dataList, FilterInvalidData(dataList, decValues), decValues);

            int validCount = classifications.Values.Count(c => c == DataClassification.Valid);
            int totalCount = dataList.Count;

            double baseScore = (double)validCount / totalCount * 100;

            // 추가적인 품질 지표들
            var outliers = DetectOutliers(dataList);
            double outlierPenalty = (double)outliers.Count / totalCount * 10;

            // 최종 점수 계산
            double finalScore = Math.Max(0, baseScore - outlierPenalty);

            return Math.Min(100, finalScore);
        }
    }
}