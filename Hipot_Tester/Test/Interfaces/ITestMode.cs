using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipot_Tester.Test.Interfaces
{
    public interface ITestModeService
    {
        Task<bool> SetACMode(double voltage, double frequency, double testTime);
        Task<bool> SetDCMode(double voltage, double testTime);
        Task<bool> SetIRMode(double voltage, double testTime);
        Task<TestResult> ExecuteTest();
        Task<bool> SetRange(RangeType rangeType, CurrentRange? fixedRange = null);
    }
}
