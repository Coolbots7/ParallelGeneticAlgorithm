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
using GeneticAlgorithm;
using System.IO;

namespace GAForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GeneticAlgorithm.StandardGA standardGA;
        private List<int[]> points;


        public MainWindow()
        {
            InitializeComponent();

            //load points from csv
            points = new List<int[]>();
            using (var reader = new StreamReader(@"C:\points.csv"))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    var coords = line.Trim().Split(',');

                    int[] point = new int[2];
                    point[0] = Convert.ToInt32(coords[0].Trim());
                    point[1] = Convert.ToInt32(coords[1].Trim());
                    points.Add(point);
                }
            }

            this.standardGA = new GeneticAlgorithm.StandardGA(points.Count(), 40, 10, new StandardGA.FittnessDelegate(fittness));
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

        private double fittness(chromosome c)
        {
            double score = 0;

            for (int i = 0; i < c.gene.Count() - 1; i++)
            {
                int[] p1 = points[c.gene[i]];
                int[] p2 = points[c.gene[i + 1]];
                score += distance(p1[0], p1[1], p2[0], p2[1]);
            }

            return score;
        }

        private double distance(double x1, double y1, double x2, double y2)
        {
            double xdist = Math.Abs(x1 - x2);
            double ydist = Math.Abs(y1 - y2);
            return Math.Sqrt(Math.Pow(xdist, 2) + Math.Pow(ydist, 2));
        }
    }
}
