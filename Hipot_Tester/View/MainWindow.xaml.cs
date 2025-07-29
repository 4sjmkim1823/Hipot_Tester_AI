using System.Windows;
using Hipot_Tester.ViewModel;
using Hipot_Tester.Devices;

namespace Hipot_Tester.View
{
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;
        private DeviceManager deviceManager;

        public MainWindow()
        {
            InitializeComponent();

            deviceManager = new DeviceManager();
            mainViewModel = new MainViewModel();
            mainViewModel.DeviceSelectionViewModel = new DeviceSelectionViewModel(deviceManager);
            mainViewModel.IRModeViewModel = new IRModeViewModel(deviceManager, mainViewModel);

            DataContext = mainViewModel;
        }
    }
} 