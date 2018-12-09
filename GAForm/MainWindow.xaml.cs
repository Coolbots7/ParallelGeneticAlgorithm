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
using System.Threading;
using LiveCharts;
using LiveCharts.Wpf;

namespace GAForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StandardGA.StandardGA standardGA;
        
        public MainWindow()
        {
            InitializeComponent();
            
            DataContext = this;

            this.standardGA = new StandardGA.StandardGA();
            (new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    UpdateUI();
                    Thread.Sleep(100);
                }
            }))).Start();
        }

        delegate void UpdateUIMethodInvoker();
        private void UpdateUI()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new UpdateUIMethodInvoker(UpdateUI));
                return;
            }
            GenerationTextBox.Text = this.standardGA.GetGeneration().ToString();
            HighScoreGenerationTextBox.Text = this.standardGA.GetHighScoreGeneration().ToString();
            HighScoreTextBox.Text = this.standardGA.GetHighScore().ToString();
            BestChromosomeTextBox.Text = this.standardGA.GetBestChromosome();
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            this.standardGA.Start();
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
        }

        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            this.standardGA.Stop();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
            Environment.Exit(0);
            return;
        }
    }
}
