using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Hipot_Tester.Devices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hipot_Tester.Services;

namespace Hipot_Tester.ViewModel
{
    public partial class DeviceSelectionViewModel : ObservableObject
    {
        private IDeviceService deviceManager;
        private string selectedPort;
        private string statusMessage;

        public DeviceSelectionViewModel(IDeviceService deviceManager)
        {
            this.deviceManager = deviceManager;
            AvailablePorts = new List<string> { "COM1", "COM2", "COM3", "COM4" };
            ConnectCommand = new RelayCommand(Connect, CanConnect);
            DisconnectCommand = new RelayCommand(Disconnect, CanDisconnect);
        }

        public List<string> AvailablePorts { get; }

        public string SelectedPort
        {
            get => selectedPort;
            set
            {
                if (SetProperty(ref selectedPort, value))
                {
                    ((RelayCommand)ConnectCommand).NotifyCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => statusMessage;
            set => SetProperty(ref statusMessage, value);
        }

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }

        private bool CanConnect()
        {
            return !string.IsNullOrEmpty(SelectedPort) && !deviceManager.IsConnected;
        }

        private bool CanDisconnect()
        {
            return deviceManager.IsConnected;
        }

        private void Connect()
        {
            try
            {
                deviceManager.Connect(SelectedPort);
                StatusMessage = $"Connected to {SelectedPort}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Connection failed: {ex.Message}";
            }
        }

        private void Disconnect()
        {
            try
            {
                deviceManager.Disconnect();
                StatusMessage = "Disconnected";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Disconnection failed: {ex.Message}";
            }
        }
    }
}
