using System;
using Ivi.Visa;
using System.Configuration;
using System.Threading.Tasks;
using NationalInstruments.Visa;
using Hipot_Tester.Devices;
using System.Collections.Generic;
using System.Linq;

namespace Hipot_Tester.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceFactory _deviceFactory;
        private VisaSerialDevice _currentDevice;
        private SerialSession _session;
        private bool _disposed = false;

        public VisaSerialDevice CurrentDevice => _currentDevice;
        public bool IsConnected => _currentDevice?.IsConnected ?? false;
        public string ConnectionStatus { get; private set; } = "Disconnected";

        public event EventHandler<bool> ConnectionStatusChanged;
        public event EventHandler<string> ErrorOccurred;

        public DeviceService(IDeviceFactory deviceFactory)
        {
            _deviceFactory = deviceFactory ?? throw new ArgumentNullException(nameof(deviceFactory));
        }

        public IEnumerable<string> GetAvailableDeviceTypes()
        {
            return new List<string> { "1903X", "1905X", "Hipot_32", "Hipot_53" };
        }

        public IEnumerable<string> GetAvailablePorts()
        {
            using (var rm = new ResourceManager())
            {
                return rm.Find("ASRL?*::INSTR").ToList();
            }
            // return ResourceManager.GetLocalManager().Find("ASRL?*::INSTR");
        }

        public async Task<bool> ConnectAsync(string deviceType, string port = null)
        {
            try
            {
                if (_currentDevice != null)
                {
                    await DisconnectAsync();
                }

                if (!_deviceFactory.IsDeviceTypeSupported(deviceType))
                {
                    throw new NotSupportedException($"Device type '{deviceType}' is not supported");
                }

                // 포트가 지정되지 않으면 설정에서 가져오기
                if (string.IsNullOrEmpty(port))
                {
                    port = ConfigurationManager.AppSettings["Test"] ?? "1";
                }

                string resource = $"ASRL{port}::INSTR";
                
                await Task.Run(() =>
                {
                    _session = new SerialSession(resource);
                    ConfigureSerialSession(_session);
                    
                    _currentDevice = _deviceFactory.CreateDevice(deviceType);
                    _currentDevice.SetSession(_session);
                });

                // 연결 확인을 위해 디바이스 ID 요청
                var deviceId = await GetDeviceIdAsync();
                var isConnected = !string.IsNullOrEmpty(deviceId);

                ConnectionStatus = isConnected ? "Connected" : "Connection Failed";
                ConnectionStatusChanged?.Invoke(this, isConnected);

                return isConnected;
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Connection Error: {ex.Message}";
                ErrorOccurred?.Invoke(this, ex.Message);
                await DisconnectAsync();
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    _currentDevice?.Dispose();
                    _currentDevice = null;

                    _session?.Dispose();
                    _session = null;
                });

                ConnectionStatus = "Disconnected";
                ConnectionStatusChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Disconnect error: {ex.Message}");
            }
        }

        public async Task<double> GetVoltageAsync()
        {
            EnsureConnected();
            
            try
            {
                return await Task.Run(() =>
                {
                    var response = _currentDevice.QueryMessage("MEAS:VOLT?");
                    return double.Parse(response);
                });
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to get voltage: {ex.Message}");
                throw;
            }
        }

        public async Task<double> GetCurrentAsync()
        {
            EnsureConnected();
            
            try
            {
                return await Task.Run(() =>
                {
                    var response = _currentDevice.QueryMessage("MEAS:CURR?");
                    return double.Parse(response);
                });
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to get current: {ex.Message}");
                throw;
            }
        }

        public async Task StartTestAsync()
        {
            EnsureConnected();
            
            try
            {
                if (_currentDevice is RS_232_1903X device1903X)
                {
                    await device1903X.StartAsync();
                }
                else if (_currentDevice is RS_232_1905X device1905X)
                {
                    await device1905X.StartAsync();
                }
                else
                {
                    // 기본 동기 방식 (다른 디바이스들)
                    await Task.Run(() => _currentDevice.SendMessage("START"));
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to start test: {ex.Message}");
                throw;
            }
        }

        public async Task StopTestAsync()
        {
            EnsureConnected();
            
            try
            {
                if (_currentDevice is RS_232_1903X device1903X)
                {
                    await device1903X.StopAsync();
                }
                else if (_currentDevice is RS_232_1905X device1905X)
                {
                    await device1905X.StopAsync();
                }
                else
                {
                    // 기본 동기 방식 (다른 디바이스들)
                    await Task.Run(() => _currentDevice.SendMessage("STOP"));
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to stop test: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetDeviceIdAsync()
        {
            EnsureConnected();
            
            try
            {
                if (_currentDevice is RS_232_1903X device1903X)
                    return await device1903X.GetIDNAsync();
                if (_currentDevice is RS_232_1905X device1905X)
                    return await device1905X.GetIDNAsync();
                
                throw new NotSupportedException("Device does not support GetIDN operation");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to get device ID: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<string> GetStatusAsync()
        {
            EnsureConnected();
            
            try
            {
                return await Task.Run(() =>
                {
                    if (_currentDevice is RS_232_1903X device1903X)
                        return device1903X.GetStatus();
                    
                    // Other devices can be added here
                    return "Status not available";
                });
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to get status: {ex.Message}");
                return "Error getting status";
            }
        }

        private void ConfigureSerialSession(SerialSession session)
        {
            session.BaudRate = 9600;
            session.DataBits = 8;
            session.StopBits = SerialStopBitsMode.One;
            session.Parity = SerialParity.None;
            session.FlowControl = SerialFlowControlModes.None;
            session.TimeoutMilliseconds = 3000;
        }

        private void EnsureConnected()
        {
            if (_currentDevice == null || !IsConnected)
            {
                throw new InvalidOperationException("Device is not connected");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                DisconnectAsync().Wait();
                _disposed = true;
            }
        }

        public void Connect(string port)
        {
            ConnectAsync("DefaultDeviceType", port).Wait();
        }

        public void Disconnect()
        {
            DisconnectAsync().Wait();
        }
    }
}