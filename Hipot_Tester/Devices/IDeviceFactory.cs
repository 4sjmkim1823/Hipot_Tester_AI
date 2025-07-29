using System;

namespace Hipot_Tester.Devices
{
    public interface IDeviceFactory
    {
        VisaSerialDevice CreateDevice(string deviceType);
        bool IsDeviceTypeSupported(string deviceType);
    }

    public class DeviceFactory : IDeviceFactory
    {
        public VisaSerialDevice CreateDevice(string deviceType)
        {
            if (deviceType == null)
                throw new ArgumentNullException(nameof(deviceType));

            switch (deviceType.ToUpper())
            {
                case "1903X":
                    return new RS_232_1903X();
                case "1905X":
                    return new RS_232_1905X();
                case "HIPOT_32":
                    return new RS_232_1903X(); // Placeholder for Hipot_32
                case "HIPOT_53":
                    return new RS_232_1905X(); // Placeholder for Hipot_53
                default:
                    throw new NotSupportedException($"Device type '{deviceType}' is not supported");
            }
        }

        public bool IsDeviceTypeSupported(string deviceType)
        {
            if (deviceType == null)
                return false;

            switch (deviceType.ToUpper())
            {
                case "1903X":
                case "1905X":
                case "HIPOT_32":
                case "HIPOT_53":
                    return true;
                default:
                    return false;
            }
        }
    }
}