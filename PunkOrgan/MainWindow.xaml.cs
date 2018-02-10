using System.Windows;

namespace PunkOrgan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PunkHammond synth = new PunkHammond();

        public MainWindow()
        {
            DataContext = synth;
            Closing += synth.Window_Closed;
            KeyDown += synth.Window_KeyDown;
            KeyUp += synth.Window_KeyUp;
            InitializeComponent();
        }

        private void slider_Bending_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            slider_Bending.Value = 0;
        }
    }
}
