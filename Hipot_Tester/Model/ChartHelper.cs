using System;
using System.Collections.Generic;
using System.Linq;

namespace Hipot_Tester.Model
{
    /// <summary>
    /// 차트 관련 도우미 클래스
    /// </summary>
    public static class ChartHelper
    {
        /// <summary>
        /// DEC(Data Error Correction) 값 계산
        /// </summary>
        /// <param name="dataList">데이터 리스트</param>
        /// <returns>DEC 값 배열</returns>
        public static double[] CalculateDECValues(List<DataModel> dataList)
        {
            if (dataList == null || dataList.Count == 0)
                return new double[0];

            var decValues = new double[dataList.Count];

            for (int i = 0; i < dataList.Count; i++)
            {
                var data = dataList[i];
                
                // DEC 값 계산 로직
                // 여기서는 간단한 예시로 구현
                if (data.Current != 0 && data.Voltage != 0)
                {
                    // 기본적인 계산: 저항값의 변화율을 기반으로 한 오차 보정값
                    var expectedResistance = data.Voltage / data.Current;
                    var actualResistance = data.Resistance;
                    
                    // 상대 오차 계산
                    decValues[i] = Math.Abs((expectedResistance - actualResistance) / expectedResistance) * 100;
                }
                else
                {
                    decValues[i] = double.MaxValue; // 무효한 데이터
                }
            }

            return decValues;
        }

        /// <summary>
        /// 데이터의 이동평균 계산
        /// </summary>
        /// <param name="values">값 배열</param>
        /// <param name="windowSize">윈도우 크기</param>
        /// <returns>이동평균 배열</returns>
        public static double[] CalculateMovingAverage(double[] values, int windowSize)
        {
            if (values == null || values.Length == 0 || windowSize <= 0)
                return new double[0];

            var result = new double[values.Length];
            
            for (int i = 0; i < values.Length; i++)
            {
                int start = Math.Max(0, i - windowSize / 2);
                int end = Math.Min(values.Length - 1, i + windowSize / 2);
                
                double sum = 0;
                int count = 0;
                
                for (int j = start; j <= end; j++)
                {
                    if (!double.IsNaN(values[j]) && !double.IsInfinity(values[j]))
                    {
                        sum += values[j];
                        count++;
                    }
                }
                
                result[i] = count > 0 ? sum / count : 0;
            }

            return result;
        }

        /// <summary>
        /// 데이터의 표준편차 계산
        /// </summary>
        /// <param name="values">값 배열</param>
        /// <returns>표준편차</returns>
        public static double CalculateStandardDeviation(double[] values)
        {
            if (values == null || values.Length == 0)
                return 0;

            var validValues = values.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToArray();
            
            if (validValues.Length == 0)
                return 0;

            double mean = validValues.Average();
            double sumOfSquares = validValues.Sum(v => Math.Pow(v - mean, 2));
            
            return Math.Sqrt(sumOfSquares / validValues.Length);
        }

        /// <summary>
        /// 이상치 감지를 위한 Z-Score 계산
        /// </summary>
        /// <param name="values">값 배열</param>
        /// <returns>Z-Score 배열</returns>
        public static double[] CalculateZScores(double[] values)
        {
            if (values == null || values.Length == 0)
                return new double[0];

            var validValues = values.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToArray();
            
            if (validValues.Length == 0)
                return new double[values.Length];

            double mean = validValues.Average();
            double stdDev = CalculateStandardDeviation(validValues);
            
            if (stdDev == 0)
                return new double[values.Length];

            var zScores = new double[values.Length];
            
            for (int i = 0; i < values.Length; i++)
            {
                if (!double.IsNaN(values[i]) && !double.IsInfinity(values[i]))
                {
                    zScores[i] = (values[i] - mean) / stdDev;
                }
                else
                {
                    zScores[i] = 0;
                }
            }

            return zScores;
        }
    }
}