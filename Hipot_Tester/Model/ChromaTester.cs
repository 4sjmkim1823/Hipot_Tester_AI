using Hipot_Tester.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipot_Tester.Model
{
    public abstract class ChromaTester
    {
        protected VisaSerialDevice _serialDevice;

        protected ChromaTester(VisaSerialDevice serialDevice)
        {
            _serialDevice = serialDevice;
        }

        public abstract string ModelName { get; }
        public void Disconnect()
        {
            _serialDevice.Dispose();
        }
        public bool IsConnected => _serialDevice.IsConnected;
        public abstract Task<TestResult> RunTest(TestConfiguration config);

        protected abstract string FormatTestCommand(TestConfiguration config);
        protected abstract TestResult ParseTestResponse(string response, TestConfiguration config);
    }
}
