using ChromaHipotTester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.Drawing;

namespace Hipot_Tester.Test
{
    public class Test
    {
        public string TestId { get; set; }
        public TestMode Mode { get; set; }
        public bool IsPassed { get; set; }
        public double MeasuredCurrent { get; set; }
        public double MeasuredVoltage { get; set; }
        public double? MeasuredResistance { get; set; } // IR 모드용
        public double TestDuration { get; set; }
        public string DeviceId { get; set; }
        public string ErrorMessage { get; set; }
        public System.DateTime Timestamp { get; set; }
        public string DeviceName { get; set; }
        public DateTime TestTime { get; set; }
        public List<double> Voltages { get; set; }
        public List<double> Currents { get; set; }
        public List<DateTime> TimeStamps { get; set; }
        public bool Passed { get; set; }
        public string Remarks { get; set; }

        public Test()
        {
            Voltages = new List<double>();
            Currents = new List<double>();
            TimeStamps = new List<DateTime>();
        }

        public void AddMeasurement(double voltage, double current)
        {
            Voltages.Add(voltage);
            Currents.Add(current);
            TimeStamps.Add(DateTime.Now);
        }

        public void SaveToExcel(string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Test Results");
                
                // 헤더 작성
                worksheet.Cell(1, 1).Value = "Time";
                worksheet.Cell(1, 2).Value = "Voltage (V)";
                worksheet.Cell(1, 3).Value = "Current (A)";

                // 데이터 작성
                for (int i = 0; i < TimeStamps.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = TimeStamps[i];
                    worksheet.Cell(i + 2, 2).Value = Voltages[i];
                    worksheet.Cell(i + 2, 3).Value = Currents[i];
                }

                // 전압 그래프 생성
                var voltageChart = worksheet.Charts.AddChart(0, 5, 20, 15);
                voltageChart.ChartType = XLChartType.Line;
                voltageChart.SetSourceData(worksheet.Range($"A2:B{TimeStamps.Count + 1}"));
                voltageChart.Title = "Voltage vs Time";
                voltageChart.AxisX.Title = "Time";
                voltageChart.AxisY.Title = "Voltage (V)";

                // 전류 그래프 생성
                var currentChart = worksheet.Charts.AddChart(0, 20, 20, 30);
                currentChart.ChartType = XLChartType.Line;
                currentChart.SetSourceData(worksheet.Range($"A2:C{TimeStamps.Count + 1}"));
                currentChart.Title = "Current vs Time";
                currentChart.AxisX.Title = "Time";
                currentChart.AxisY.Title = "Current (A)";

                // 테스트 정보 작성
                worksheet.Cell(TimeStamps.Count + 3, 1).Value = "Device Name:";
                worksheet.Cell(TimeStamps.Count + 3, 2).Value = DeviceName;
                worksheet.Cell(TimeStamps.Count + 4, 1).Value = "Test Time:";
                worksheet.Cell(TimeStamps.Count + 4, 2).Value = TestTime;
                worksheet.Cell(TimeStamps.Count + 5, 1).Value = "Result:";
                worksheet.Cell(TimeStamps.Count + 5, 2).Value = Passed ? "PASS" : "FAIL";
                worksheet.Cell(TimeStamps.Count + 6, 1).Value = "Remarks:";
                worksheet.Cell(TimeStamps.Count + 6, 2).Value = Remarks;

                workbook.SaveAs(filePath);
            }
        }
    }
}
