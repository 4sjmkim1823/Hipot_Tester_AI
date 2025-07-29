using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hipot_Tester.Devices;

namespace Hipot_Tester.Services
{
    public interface IDeviceService : IDisposable
    {
        VisaSerialDevice CurrentDevice { get; }
        bool IsConnected { get; }
        string ConnectionStatus { get; }
        
        Task<bool> ConnectAsync(string deviceType, string port);
        Task DisconnectAsync();
        
        Task<double> GetVoltageAsync();
        Task<double> GetCurrentAsync();
        Task StartTestAsync();
        Task StopTestAsync();
        Task<string> GetDeviceIdAsync();
        Task<string> GetStatusAsync();
        IEnumerable<string> GetAvailablePorts();
        IEnumerable<string> GetAvailableDeviceTypes();

        event EventHandler<bool> ConnectionStatusChanged;
        event EventHandler<string> ErrorOccurred;

        void Connect(string port);
        void Disconnect();
    }
}