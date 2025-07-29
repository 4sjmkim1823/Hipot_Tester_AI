using System;

namespace Hipot_Tester.Devices
{
    [Obsolete("This class has been replaced by DeviceService. Use IDeviceService instead.", true)]
    public class DeviceManager
    {
        public DeviceManager()
        {
            throw new InvalidOperationException(
                "DeviceManager is obsolete. Please use IDeviceService and DeviceService instead. " +
                "Inject IDeviceService through dependency injection or use DeviceFactory to create instances.");
        }
    }
} 