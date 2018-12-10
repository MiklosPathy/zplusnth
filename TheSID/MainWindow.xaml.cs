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

namespace TheSID
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SID synth = new SID();

        public MainWindow()
        {
            synth.LoadState();
            synth.LoadPresets();
            DataContext = synth;
            Closing += synth.Window_Closed;
            KeyDown += synth.Window_KeyDown;
            KeyUp += synth.Window_KeyUp;
            InitializeComponent();
        }

        private void slider_Bending_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            slider_Bending.Value = 0;
        }
    }
}
