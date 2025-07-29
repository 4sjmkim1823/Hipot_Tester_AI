using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipot_Tester.Model
{
    public class TestConfiguration
    {
        public TestMode Mode { get; set; }
        public TestRange Range { get; set; }
        public double Voltage { get; set; }
        public double CurrentLimit { get; set; }
        public TimeSpan TestDuration { get; set; }
        public TimeSpan RampTime { get; set; }
    }
}
