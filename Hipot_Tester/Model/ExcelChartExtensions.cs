using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hipot_Tester.ViewModel;

namespace Hipot_Tester.Model
{
    public enum XLChartStyle
    {
        Style1,
        Style2,
        Style4,
        Style5
    }

    public static class ExcelChartExtensions
    {
        /// <summary>
        /// 차트 스타일 열거형 - ClosedXML이 제공하는 값과 매칭
        /// </summary>
        

        /// <summary>
        /// 차트 범례 위치 열거형 - ClosedXML이 제공하는 값과 매칭
        /// </summary>
        

        /// <summary>
        /// 차트 스타일 설정 메서드
        /// </summary>
        public static void SetStyle(this IXLChart chart, XLChartStyle style)
        {
            chart.Style = (int)style;
        }

        /// <summary>
        /// 차트 시리즈 스타일 설정 메서드
        /// </summary>
        public static void SetStyle(this IXLChartSeries series, XLChartStyle style)
        {
            // 시리즈 스타일 설정 (ClosedXML에서 직접 지원하지 않을 수 있으므로 아래는 예시 코드)
            switch (style)
            {
                case XLChartStyle.Style1:
                    series.Fill.SetColor(XLColor.Blue);
                    break;
                case XLChartStyle.Style2:
                    series.Fill.SetColor(XLColor.Red);
                    break;
                case XLChartStyle.Style4:
                    series.Fill.SetColor(XLColor.Green);
                    break;
                case XLChartStyle.Style5:
                    series.Fill.SetColor(XLColor.Orange);
                    break;
                // 필요한 경우 다른 스타일 추가
                default:
                    series.Fill.SetColor(XLColor.Blue);
                    break;
            }
        }
    }
}
