using System.Windows;

namespace PunkOrgan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var synth = new PunkHammond();
            DataContext = synth;
            Closing += synth.Window_Closed;
            KeyDown += synth.Window_KeyDown;
            KeyUp += synth.Window_KeyUp;
            InitializeComponent();
        }

    }
}
