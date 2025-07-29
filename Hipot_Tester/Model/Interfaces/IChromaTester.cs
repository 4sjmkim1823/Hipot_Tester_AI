using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipot_Tester.Model.Interfaces
{
    public interface IChromaTester
    {
        string ModelName { get; set; }
        void Disconnect();
        bool IsConnected { get; }
        Task<TestResult> RunTest(TestConfiguration config);
    }
}
