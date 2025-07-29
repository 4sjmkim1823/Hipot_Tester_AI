using Hipot_Tester.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipot_Tester.Test
{
    public enum ConnectionStatus
    {
        Disconnected,
        Connected,
        Error
    }

    public enum DeviceType
    {
        TypeA,
        TypeB, 
        TypeC, 
        TypeD,
    }

    public class TestDevice
    {
        public string DeviceId { get; set; }
        public string ModelName { get; set; }
        public string SerialNumber { get; set; }
        public DeviceType Type { get; set; }
        public ConnectionStatus Status { get; set; }
    }
}
