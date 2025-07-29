using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hipot_Tester.View
{
    /// <summary>
    /// IRModeView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IRModeView : UserControl
    {
        public IRModeView()
        {
            InitializeComponent();

            VoltageTextBox.Text = "50"; 
            ResistanceTextBox.Text = "0.1";  
        }
    }
}
