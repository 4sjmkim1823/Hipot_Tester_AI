using System;
using System.Collections.Generic;
using System.Linq;
using Hipot_Tester.Devices;
using Hipot_Tester.Services;

namespace Hipot_Tester.Model
{
    [Obsolete("This class is deprecated. Use IDeviceService instead.", false)]
    public static class DeviceManager
    {
        private static readonly DeviceFactory _deviceFactory = new DeviceFactory();
        private static readonly DeviceService _deviceService = new DeviceService(_deviceFactory);
        
        public static List<string> AvailableDeviceTypes => new List<string> { "1903X", "1905X", "Hipot_32", "Hipot_53" };
        
        public static VisaSerialDevice SelectedDevice => _deviceService.CurrentDevice;
        
        public static bool IsConnected => _deviceService.IsConnected;
        
        public static string ConnectionStatus => _deviceService.ConnectionStatus;

        public static bool ConnectDevice(string selectedDevice)
        {
            try
            {
                var result = _deviceService.ConnectAsync(selectedDevice).Result;
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void DisconnectDevice()
        {
            _deviceService.DisconnectAsync().Wait();
        }

        public static IDeviceService GetDeviceService()
        {
            return _deviceService;
        }

        static DeviceManager()
        {
            _deviceService.ErrorOccurred += (sender, error) => 
            {
                // 오류 로깅을 위한 이벤트 처리
                Console.WriteLine($"Device Error: {error}");
            };
        }
    }
}
