using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Hipot_Tester.Services;
using Hipot_Tester.ViewModel.Control;
using Hipot_Tester.Model;

namespace Hipot_Tester.ViewModel
{
    public class MainViewModel : ObservableObject, IDisposable
    {
        private readonly IDeviceService _deviceService;
        private readonly IDialogService _dialogService;
        
        private string _selectedDeviceType;
        private string _connectionStatus;
        private bool _isConnected;
        private bool _isTestRunning;
        private double _currentVoltage;
        private double _currentCurrent;
        private string _deviceId;
        private ObservableCollection<string> _logMessages;
        private bool _disposed;

        public MainViewModel(IDeviceService deviceService, IDialogService dialogService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            
            InitializeProperties();
            InitializeCommands();
            SubscribeToEvents();
        }

        #region Properties

        public ObservableCollection<string> AvailableDeviceTypes { get; private set; }

        public ObservableCollection<string> AvailablePorts { get; private set; }

        private string _selectedPort;
        public string SelectedPort
        {
            get => _selectedPort;
            set => SetProperty(ref _selectedPort, value);
        }

        public string SelectedDeviceType
        {
            get => _selectedDeviceType;
            set => SetProperty(ref _selectedDeviceType, value);
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (SetProperty(ref _isConnected, value))
                {
                    ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DisconnectCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StartTestCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StopTestCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsTestRunning
        {
            get => _isTestRunning;
            set
            {
                if (SetProperty(ref _isTestRunning, value))
                {
                    ((RelayCommand)StartTestCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StopTestCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public double CurrentVoltage
        {
            get => _currentVoltage;
            set => SetProperty(ref _currentVoltage, value);
        }

        public double CurrentCurrent
        {
            get => _currentCurrent;
            set => SetProperty(ref _currentCurrent, value);
        }

        public string DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }

        public ObservableCollection<string> LogMessages
        {
            get => _logMessages;
            set => SetProperty(ref _logMessages, value);
        }

        #endregion

        #region Commands

        public ICommand ConnectCommand { get; private set; }
        public ICommand DisconnectCommand { get; private set; }
        public ICommand StartTestCommand { get; private set; }
        public ICommand StopTestCommand { get; private set; }
        public ICommand RefreshDataCommand { get; private set; }
        public ICommand ClearLogCommand { get; private set; }

        #endregion

        #region Private Methods

        private void InitializeProperties()
        {
            AvailableDeviceTypes = new ObservableCollection<string>(_deviceService.GetAvailableDeviceTypes());
            SelectedDeviceType = AvailableDeviceTypes.FirstOrDefault();
            AvailablePorts = new ObservableCollection<string>(_deviceService.GetAvailablePorts());
            SelectedPort = AvailablePorts.FirstOrDefault();
            ConnectionStatus = "연결되지 않음";
            IsConnected = false;
            IsTestRunning = false;
            LogMessages = new ObservableCollection<string>();
            
            AddLogMessage("애플리케이션이 시작되었습니다.");
        }

        private void InitializeCommands()
        {
            ConnectCommand = new RelayCommand(async () => await ConnectAsync(), () => !IsConnected && !string.IsNullOrEmpty(SelectedDeviceType));
            DisconnectCommand = new RelayCommand(async () => await DisconnectAsync(), () => IsConnected);
            StartTestCommand = new RelayCommand(async () => await StartTestAsync(), () => IsConnected && !IsTestRunning);
            StopTestCommand = new RelayCommand(async () => await StopTestAsync(), () => IsConnected && IsTestRunning);
            RefreshDataCommand = new RelayCommand(async () => await RefreshDataAsync(), () => IsConnected);
            ClearLogCommand = new RelayCommand(() => LogMessages.Clear());
        }

        private void SubscribeToEvents()
        {
            _deviceService.ConnectionStatusChanged += OnConnectionStatusChanged;
            _deviceService.ErrorOccurred += OnErrorOccurred;
        }

        private void UnsubscribeFromEvents()
        {
            if (_deviceService != null)
            {
                _deviceService.ConnectionStatusChanged -= OnConnectionStatusChanged;
                _deviceService.ErrorOccurred -= OnErrorOccurred;
            }
        }

        private async Task ConnectAsync()
        {
            try
            {
                AddLogMessage($"장치 연결 중: {SelectedDeviceType} on {SelectedPort}");
                var connected = await _deviceService.ConnectAsync(SelectedDeviceType, SelectedPort);
                
                if (connected)
                {
                    DeviceId = await _deviceService.GetDeviceIdAsync();
                    AddLogMessage($"장치 연결 성공: {DeviceId}");
                    await _dialogService.ShowMessageAsync("장치가 성공적으로 연결되었습니다.");
                }
                else
                {
                    AddLogMessage("장치 연결 실패");
                    await _dialogService.ShowErrorAsync("장치 연결에 실패했습니다.");
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"연결 오류: {ex.Message}");
                await _dialogService.ShowErrorAsync($"연결 오류: {ex.Message}");
            }
        }

        private async Task DisconnectAsync()
        {
            try
            {
                if (IsTestRunning)
                {
                    await StopTestAsync();
                }
                
                await _deviceService.DisconnectAsync();
                DeviceId = string.Empty;
                CurrentVoltage = 0;
                CurrentCurrent = 0;
                AddLogMessage("장치 연결이 해제되었습니다.");
            }
            catch (Exception ex)
            {
                AddLogMessage($"연결 해제 오류: {ex.Message}");
                await _dialogService.ShowErrorAsync($"연결 해제 오류: {ex.Message}");
            }
        }

        private async Task StartTestAsync()
        {
            try
            {
                await _deviceService.StartTestAsync();
                IsTestRunning = true;
                AddLogMessage("테스트가 시작되었습니다.");
                
                // 주기적으로 데이터 새로고침 시작
                _ = Task.Run(async () =>
                {
                    while (IsTestRunning && IsConnected)
                    {
                        await RefreshDataAsync();
                        await Task.Delay(1000); // 1초마다 새로고침
                    }
                });
            }
            catch (Exception ex)
            {
                IsTestRunning = false;
                AddLogMessage($"테스트 시작 오류: {ex.Message}");
                await _dialogService.ShowErrorAsync($"테스트 시작 오류: {ex.Message}");
            }
        }

        private async Task StopTestAsync()
        {
            try
            {
                await _deviceService.StopTestAsync();
                IsTestRunning = false;
                AddLogMessage("테스트가 중지되었습니다.");
            }
            catch (Exception ex)
            {
                AddLogMessage($"테스트 중지 오류: {ex.Message}");
                await _dialogService.ShowErrorAsync($"테스트 중지 오류: {ex.Message}");
            }
        }

        private async Task RefreshDataAsync()
        {
            if (!IsConnected) return;

            try
            {
                var voltage = await _deviceService.GetVoltageAsync();
                var current = await _deviceService.GetCurrentAsync();
                
                CurrentVoltage = voltage;
                CurrentCurrent = current;
            }
            catch (Exception ex)
            {
                AddLogMessage($"데이터 새로고침 오류: {ex.Message}");
            }
        }

        private void OnConnectionStatusChanged(object sender, bool isConnected)
        {
            IsConnected = isConnected;
            ConnectionStatus = _deviceService.ConnectionStatus;
        }

        private void OnErrorOccurred(object sender, string error)
        {
            AddLogMessage($"장치 오류: {error}");
        }

        private void AddLogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            LogMessages.Add($"[{timestamp}] {message}");
            
            // 로그 메시지가 너무 많으면 오래된 것부터 제거
            while (LogMessages.Count > 1000)
            {
                LogMessages.RemoveAt(0);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                UnsubscribeFromEvents();
                _deviceService?.Dispose();
                _disposed = true;
            }
        }

        #endregion
    }
}
