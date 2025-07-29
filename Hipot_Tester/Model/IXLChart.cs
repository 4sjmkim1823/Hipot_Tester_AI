using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipot_Tester.Model
{
    /// <summary>
    /// 차트 축의 틱 마크 열거형 (ClosedXML 호환성을 위해)
    /// </summary>
    public enum ChartTickMarks
    {
        None = 0,
        Inside = 1,
        Outside = 2,
        Cross = 3
    }

    /// <summary>
    /// 차트 범례 위치 열거형 (ClosedXML 호환성을 위해)
    /// </summary>
    public enum ChartLegendPosition
    {
        Bottom = 0,
        Corner = 1,
        Top = 2,
        Right = 3,
        Left = 4
    }
    public interface IXLChart
    {
        IXLChartAxis XAxis { get; }
        IXLChartAxis PrimaryYAxis { get; }
        IXLChartAxis SecondaryYAxis { get; }
        IXLChartLegend Legend { get; }
        int Style { get; set; }
        IXLChartSeriesCollection Series_ { get; }
        void SetTitle(string title);
    }

    /// <summary>
    /// IXLChartAxis 인터페이스
    /// </summary>
    public interface IXLChartAxis
    {
        string Title { get; set; }
        ChartTickMarks MajorTickMarks { get; set; }
    }

    /// <summary>
    /// IXLChartLegend 인터페이스
    /// </summary>
    public interface IXLChartLegend
    {
        ChartLegendPosition Position { get; set; }
    }

    /// <summary>
    /// IXLChartSeries 인터페이스
    /// </summary>
    public interface IXLChartSeries
    {
        string Name { get; set; }
        bool UseSecondaryAxis { get; set; }
        IXLChartSeriesFill Fill { get; }
        IXLChartDataLabel DataLabel { get; }
    }

    /// <summary>
    /// IXLChartSeriesFill 인터페이스
    /// </summary>
    public interface IXLChartSeriesFill
    {
        void SetColor(XLColor color);
    }

    /// <summary>
    /// IXLChartDataLabel 인터페이스
    /// </summary>
    public interface IXLChartDataLabel
    {
        bool ShowPercent { get; set; }
        bool ShowValue { get; set; }
        bool ShowSeriesName { get; set; }
        bool ShowCategoryName { get; set; }
    }

    /// <summary>
    /// IXLChartSeriesCollection 인터페이스
    /// </summary>
    public interface IXLChartSeriesCollection
    {
        IXLChartSeries Add(IXLRange valueRange, IXLRange categoryRange = null);
    }
}
