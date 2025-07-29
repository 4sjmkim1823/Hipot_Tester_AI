using Hipot_Tester.Devices;
using Hipot_Tester.Model;
using Hipot_Tester.ViewModel.Control;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hipot_Tester.Services;

namespace Hipot_Tester.ViewModel
{
    public partial class IRModeViewModel : Hipot_Tester.ViewModel.Control.ObservableObject
    {
        private readonly IDeviceService _deviceService;
        private MainViewModel mainViewModel;
        private string statusMessage;
        private bool isTestRunning;
        private double currentVoltage;
        private double currentCurrent;

        public ICommand StartTestCommand { get; set; }
        public ICommand StopTestCommand { get; set; }

        public IRModeViewModel(IDeviceService deviceService, MainViewModel mainViewModel)
        {
            _deviceService = deviceService;
            this.mainViewModel = mainViewModel;
            StartTestCommand = new Hipot_Tester.ViewModel.Control.RelayCommand(StartTest, CanStartTest);
            StopTestCommand = new Hipot_Tester.ViewModel.Control.RelayCommand(StopTest, CanStopTest);

            // 1905X의 경우 "30nA" 범위를 제외한 옵션만 설정
            if (HipotName == "1905X")
            {
                RangeOptions = new ObservableCollection<string>
            {
                "10mA", "3mA", "300uA", "30uA", "3uA", "300nA", "AUTO"
            };
            }
            else
            {
                RangeOptions = new ObservableCollection<string>
            {
                "10mA", "3mA", "300uA", "30uA", "3uA", "300nA", "30nA", "AUTO"
            };
            }

            SelectedRange = RangeOptions[0];
        }

        private void UpdateHipotDevice()
        {
            OnPropertyChanged(nameof(HipotName)); // UI 갱신
        }

        private void StartTest(object parameter)
        {
            // Add logic to start the test
            isTestRunning = true;
            statusMessage = "Test started.";
            OnPropertyChanged(nameof(StatusMessage));
        }

        private bool CanStartTest(object parameter)
        {
            // Add logic to determine if the test can start
            return !isTestRunning;
        }

        private void StopTest(object parameter)
        {
            // Add logic to stop the test
            isTestRunning = false;
            statusMessage = "Test stopped.";
            OnPropertyChanged(nameof(StatusMessage));
        }

        private bool CanStopTest(object parameter)
        {
            // Add logic to determine if the test can stop
            return isTestRunning;
        }

        public string HipotName => _deviceService.CurrentDevice is RS_232_1903X ? "1903X" : "1905X";

        private readonly Dictionary<string, double> _rangeValues = new Dictionary<string, double>
        {
            { "10mA", 10E-3 },
            { "3mA", 3E-3 },
            { "300uA", 300E-6 },
            { "30uA", 30E-6 },
            { "3uA", 3E-6 },
            { "300nA", 300E-9 },
            { "30nA", 30E-9 },
            { "AUTO", -1 }
        };

        private VisaSerialDevice Hipot => _deviceService.CurrentDevice;

        private double _volt;
        public double Volt
        {
            get => _volt;
            set
            {
                if (_volt != value)
                {
                    _volt = value;
                    OnPropertyChanged(nameof(Volt));
                    (Hipot as RS_232_1903X)?.SetIRLevel(_volt);
                    (Hipot as RS_232_1905X)?.SetIRLevel(_volt);
                }
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        private double _low;
        public double Low
        {
            get => _low;
            set
            {
                if (_low != value)
                {
                    _low = value;
                    OnPropertyChanged(nameof(Low));
                    (Hipot as RS_232_1903X)?.SetIRLow(_low);
                    (Hipot as RS_232_1905X)?.SetIRLow(_low);
                }
            }
        }

        private double _time;
        public double Time
        {
            get => _time;
            set
            {
                if (_time != value)
                {
                    _time = value;
                    OnPropertyChanged(nameof(Time));
                    (Hipot as RS_232_1903X)?.SetIRTime(_time);
                    (Hipot as RS_232_1905X)?.SetIRTime(_time);
                }
            }
        }

        private double _high;
        public double High
        {
            get => _high;
            set
            {
                if (_high != value)
                {
                    _high = value;
                    OnPropertyChanged(nameof(High));
                    (Hipot as RS_232_1903X)?.SetIRHigh(_high);
                    (Hipot as RS_232_1905X)?.SetIRHigh(_high);
                }
            }
        }

        private double _ramp;
        public double Ramp
        {
            get => _ramp;
            set
            {
                if (_ramp != value)
                {
                    _ramp = value;
                    OnPropertyChanged(nameof(Ramp));
                    (Hipot as RS_232_1903X)?.SetIRRamp(_ramp);
                    (Hipot as RS_232_1905X)?.SetIRRamp(_ramp);
                }
            }
        }

        private double _dwell;
        public double Dwell
        {
            get => _dwell;
            set
            {
                if (_dwell != value)
                {
                    _dwell = value;
                    OnPropertyChanged(nameof(Dwell));

                    if (Hipot is RS_232_1903X hipot1903X)
                    {
                        hipot1903X.SetIRDwell(_dwell);
                    }
                }
            }
        }

        private double _fall;
        public double Fall
        {
            get => _fall;
            set
            {
                if (_fall != value)
                {
                    _fall = value;
                    OnPropertyChanged(nameof(Fall));
                    (Hipot as RS_232_1903X)?.SetIRFall(_fall);
                    (Hipot as RS_232_1905X)?.SetIRFall(_fall);
                }
            }
        }

        private string _selectedRange;
        public string SelectedRange
        {
            get => _selectedRange;
            set
            {
                if (_selectedRange != value)
                {
                    _selectedRange = value;
                    OnPropertyChanged(nameof(SelectedRange));
                    ApplySelectedRange();
                }
            }
        }

        private void ApplySelectedRange()
        {
            if (_rangeValues.TryGetValue(SelectedRange, out double value))
            {
                (Hipot as RS_232_1903X)?.SetIRRange(value != -1 ? value : 10E-3);
                (Hipot as RS_232_1905X)?.SetIRRange(value != -1 ? value : 10E-3);
            }
        }

        public ObservableCollection<string> RangeOptions { get; }
    }
}
