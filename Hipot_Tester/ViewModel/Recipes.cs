using Hipot_Tester.Devices;
using Hipot_Tester.Model;
using Hipot_Tester.Services;
using Hipot_Tester.View;
using Hipot_Tester.ViewModel.Control;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClosedXML.Excel;

namespace Hipot_Tester.ViewModel
{
    public class Recipes : INotifyPropertyChanged
    {
        private readonly IDeviceService _deviceService;
        private VisaSerialDevice _currentDevice;
        private System.Timers.Timer _dataTimer;
        private DateTime _startTime; // 테스트 시작 시간 저장

        private bool _isTesting;
        public bool IsTesting
        {
            get => _isTesting;
            set
            {
                if (_isTesting != value)
                {
                    _isTesting = value;
                    OnPropertyChanged(nameof(IsTesting));

                    if (_isTesting)
                    {
                        StartRecipes();
                    }
                    else
                    {
                        StopRecipes();
                    }
                }
            }
        }

        public VisaSerialDevice CurrentDevice
        {
            get => _currentDevice;
            set
            {
                _currentDevice = value;
                OnPropertyChanged(nameof(CurrentDevice));
            }
        }

        private string _selectedDevice;
        public string SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (_selectedDevice != value)
                {
                    _selectedDevice = value;
                    OnPropertyChanged(nameof(SelectedDevice));
                    InitializeDevice();
                }
            }
        }

        private string _testStatus;
        public string TestStatus
        {
            get => _testStatus;
            set
            {
                _testStatus = value;
                OnPropertyChanged(nameof(TestStatus));
            }
        }

        private string _testTimeLeft;
        public string TestTimeLeft
        {
            get => _testTimeLeft;
            set
            {
                _testTimeLeft = value;
                OnPropertyChanged(nameof(TestTimeLeft));
            }
        }

        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DataModel> _data;
        public ObservableCollection<DataModel> Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged(nameof(Data));
            }
        }

        public ICommand ShowACModeCommand { get; }
        public ICommand ShowDCModeCommand { get; }
        public ICommand ShowIRModeCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand StartTest { get; }
        public ICommand StopTest { get; }
        public ICommand CmdSave { get; }
        public ICommand SaveSessionCommand { get; private set; }
        public ICommand ExportToExcelCommand { get; private set; }

        private WindowState _windowState;
        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                _windowState = value;
                OnPropertyChanged(nameof(WindowState));
            }
        }

        public Recipes(IDeviceService deviceService)
        {
            _deviceService = deviceService;

            CloseCommand = new RelayCommand(ExecuteClose);
            Data = new ObservableCollection<DataModel>();

            StartTest = new RelayCommand(obj => IsTesting = true);
            StopTest = new RelayCommand(obj => IsTesting = false);
            CmdSave = new RelayCommand(obj => SaveToExcel());

            SaveSessionCommand = new RelayCommand(obj => SaveSession());
            ExportToExcelCommand = new RelayCommand(obj => ExportToExcel());

            ShowACModeCommand = new RelayCommand(_ => CurrentView = new ACModeView());
            ShowDCModeCommand = new RelayCommand(_ => CurrentView = new IR_1905XView());
            ShowIRModeCommand = new RelayCommand(_ => CurrentView = new IRModeView());

            _dataTimer = new System.Timers.Timer(500);
            _dataTimer.Elapsed += async (s, e) => await FetchData();
        }

        private void InitializeDevice()
        {
            _deviceService.ConnectAsync(SelectedDevice, null).Wait();
            CurrentDevice = _deviceService.CurrentDevice;
        }

        private void ExecuteClose(object parameter)
        {
            Environment.Exit(0);
        }

        private void UpdateDataManager()
        {
            if (Data != null && Data.Count > 0)
            {
                string testMode = GetCurrentTestMode();
                DataManager.Instance.UpdateTestData(Data, testMode, SelectedDevice);
            }
        }

        private void SaveSession()
        {
            try
            {
                if (Data.Count == 0)
                {
                    MessageBox.Show("저장할 데이터가 없습니다. 테스트를 먼저 실행하세요.", "알림",
                MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string testMode = GetCurrentTestMode();

                MessageBox.Show("테스트 데이터가 성공적으로 저장되었습니다.", "저장 완료",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 저장 중 오류 발생: {ex.Message}", "오류",
            MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToExcel()
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"HipotTest_Export_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"엑셀 내보내기 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartRecipes()
        {
            Application.Current.Dispatcher.Invoke(() => Data.Clear());
            _startTime = DateTime.Now;
            TestStatus = "";

            if (_currentDevice != null)
            {
                _currentDevice.START();
            }

            _dataTimer.Start();
        }

        private void StopRecipes()
        {
            if (_currentDevice != null)
            {
                _currentDevice.STOP();
            }

            _dataTimer.Stop();

            UpdateDataManager();
        }

        private string GetCurrentTestMode()
        {
            if (CurrentView is ACModeView)
                return "AC Mode";
            else if (CurrentView is IR_1905XView)
                return "DC Mode";
            else if (CurrentView is IRModeView)
                return "IR Mode";
            else
                return "Unknown Mode";
        }

        private async Task FetchData()
        {
            string rawData = await Task.Run(() =>
            {
                if (_currentDevice != null)
                    return _currentDevice.GetData();
                else
                    return string.Empty;
            });

            if (!string.IsNullOrWhiteSpace(rawData))
            {
                string deviceType = _currentDevice?.GetType().Name ?? "Unknown";
                ParseAndUpdateData(rawData, deviceType);
            }

            string remainingTime = await Task.Run(() =>
            {
                if (_currentDevice != null)
                    return _currentDevice.GetData();
                else
                    return string.Empty;
            });

            Console.WriteLine($"Remaining Time: {remainingTime}");

            string[] timeParts;
            double timeInSeconds = 0;

            if (remainingTime.Contains(";"))
            {
                timeParts = remainingTime.Replace("\r\n", "").Split(';');
                if (timeParts.Length >= 3 && double.TryParse(timeParts[0], NumberStyles.Float | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out timeInSeconds))
                {
                    await HandleTestTime(timeInSeconds);
                }
                else
                {
                    TestStatus = "Invalid time";
                    TestTimeLeft = "Invalid";
                }
            }
            else
            {
                timeParts = remainingTime.Split(',');

                if (timeParts.Length > 0 && double.TryParse(timeParts.Last(), out timeInSeconds))
                {
                    await HandleTestTime(timeInSeconds);
                }
                else
                {
                    TestStatus = "Invalid time";
                    TestTimeLeft = "Invalid";
                }
            }
        }

        private async Task HandleTestTime(double timeInSeconds)
        {
            if (timeInSeconds > 0 && timeInSeconds != 9.91E+37)
            {
                TestStatus = "RUNNING";
                TestTimeLeft = timeInSeconds.ToString("F0");
            }
            else
            {
                string testStatus = await Task.Run(() =>
                {
                    if (_currentDevice != null)
                        return _currentDevice.GetJudgement();
                    else
                        return string.Empty;
                });

                if (timeInSeconds == 9.91E+37)
                {
                    TestStatus = "FINISH";
                    TestTimeLeft = "0";
                    StopRecipes();
                }
                else
                {
                    if (testStatus.Contains("116")) TestStatus = "PASS";
                    else if (testStatus.Contains("65")) TestStatus = "HIGH FAIL";
                    else if (testStatus.Contains("66")) TestStatus = "LOW FAIL";
                    else if (testStatus.Contains("71")) TestStatus = "OUTPUT FAIL";

                    TestTimeLeft = "0";
                    StopRecipes();
                }

                UpdateDataManager();
            }
        }

        private void ParseAndUpdateData(string rawData, string deviceType)
        {
            if (deviceType.Contains("1905"))
            {
                Parse1905XData(rawData);
            }
            else if (deviceType.Contains("1903"))
            {
                Parse1903XData(rawData);
            }
            else
            {
                TestStatus = "Unknown device type";
            }
        }

        private void Parse1903XData(string rawData)
        {
            string[] values = rawData.Split(',');

            if (values.Length >= 2)
            {
                if (double.TryParse(values[0], out double voltage) &&
                    double.TryParse(values[1], out double resistance))
                {
                    double current = (resistance == 0) ? 0 : voltage / resistance;
                    double elapsedTime = (DateTime.Now - _startTime).TotalSeconds;

                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (Data.Count > 1000)
                        {
                            Data.RemoveAt(0);
                        }

                        var newData = new DataModel
                        {
                            Time = elapsedTime,
                            Voltage = voltage,
                            Current = current,
                            Resistance = resistance
                        };

                        Data.Add(newData);
                        OnPropertyChanged(nameof(Data));
                    });
                }
            }
        }

        private void Parse1905XData(string rawData)
        {
            string[] values = rawData.Replace("\r\n", "").Split(';');

            if (values.Length >= 3)
            {
                if (double.TryParse(values[0], out double voltage) &&
                    double.TryParse(values[1], out double resistance) &&
                    double.TryParse(values[2], out double remainingTime))
                {
                    double elapsedTime = (DateTime.Now - _startTime).TotalSeconds;

                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (Data.Count > 1000)
                        {
                            Data.RemoveAt(0);
                        }

                        var newData = new DataModel
                        {
                            Time = elapsedTime,
                            Voltage = voltage,
                            Current = (resistance == 0) ? 0 : voltage / resistance,
                            Resistance = resistance,
                        };

                        Data.Add(newData);
                        OnPropertyChanged(nameof(Data));
                    });
                }
                else
                {
                    TestStatus = "Invalid data format";
                }
            }
            else
            {
                TestStatus = "Invalid data format";
            }
        }

        private void SaveToExcel()
        {
            try
            {
                DataTable dt = ConvertToDataTable(Data);

                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DataGridExport.xlsx");

                using (var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault() ?? workbook.Worksheets.Add("Test Data");

                    int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

                    worksheet.Cell(lastRow + 2, 1).Value = $"--- New Test: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---";
                    worksheet.Cell(lastRow + 2, 1).Style.Font.Bold = true;

                    worksheet.Cell(lastRow + 3, 1).Value = "Time (s)";
                    worksheet.Cell(lastRow + 3, 2).Value = "Voltage (V)";
                    worksheet.Cell(lastRow + 3, 3).Value = "Current (A)";
                    worksheet.Cell(lastRow + 3, 4).Value = "Resistance (Ω)";
                    worksheet.Range(lastRow + 3, 1, lastRow + 3, 4).Style.Font.Bold = true;

                    int row = lastRow + 4;
                    foreach (var data in Data)
                    {
                        worksheet.Cell(row, 1).Value = data.Time;
                        worksheet.Cell(row, 2).Value = data.Voltage;
                        worksheet.Cell(row, 3).Value = data.Current;
                        worksheet.Cell(row, 4).Value = data.Resistance;
                        row++;
                    }

                    workbook.SaveAs(filePath);
                }

                MessageBox.Show($"Excel 저장 완료: {filePath}", "저장 완료", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel 저장 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DataTable ConvertToDataTable(ObservableCollection<DataModel> data)
        {
            DataTable dt = new DataTable();
            if (data == null || data.Count == 0) return dt;

            var properties = typeof(DataModel).GetProperties();
            foreach (var prop in properties)
            {
                dt.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in data)
            {
                var row = dt.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}