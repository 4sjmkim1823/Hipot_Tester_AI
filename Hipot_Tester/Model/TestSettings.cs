using Hipot_Tester.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.Expressions;

namespace Hipot_Tester.Test
{
    public enum CurrentRange
    {
        Low,
        Medium,
        High
    }

    public class TestSettings
    {
        public TestMode Mode { get; set; }
        public double Voltage { get; set; }
        public double? Frequency { get; set; } // AC 모드용
        public double TestTime { get; set; }
        public RangeType RangeType { get; set; }
        public CurrentRange? FixedRange { get; set; }
    }
}
