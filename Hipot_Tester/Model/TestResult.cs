using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.Drawing;

namespace Hipot_Tester.Model
{
    public class TestResult
    {
        public DateTime TestTime { get; set; }
        public TestConfiguration Configuration { get; set; }
        public bool IsPass { get; set; }
        public double MeasuredCurrent { get; set; }
        public double MeasuredVoltage { get; set; }
        public double MeasuredResistance { get; set; } // IR 모드용
        public string ErrorMessage { get; set; }
    }
} 