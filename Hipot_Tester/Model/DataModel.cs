using Hipot_Tester.ViewModel.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipot_Tester.Model
{
    public class DataModel : ObservableObject
    {
        private double _time;
        private double _voltage;
        private double _current;
        private double _resistance;
        public double Time
        {
            get => _time;
            set { _time = value; OnPropertyChanged(nameof(Time)); }
        }
        public double Voltage
        {
            get => _voltage;
            set { _voltage = value; OnPropertyChanged(nameof(Voltage)); }
        }
        public double Current
        {
            get => _current;
            set { _current = value; OnPropertyChanged(nameof(Current)); }
        }
        public double Resistance
        {
            get => _resistance;
            set { _resistance = value; OnPropertyChanged(nameof(Current)); }
        }
    }
}
