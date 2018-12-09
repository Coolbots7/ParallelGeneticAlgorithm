using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticAlgorithm;
using System.IO;

namespace GeneticAlgorithm
{
    public class StandardGA : GA
    {
        private List<int[]> points;


        private int generation;

        private static int generationSize = 40;
        private static int generationCarryover = 10;

        private double HighScore;
        private int HighScoreGeneration;
        private List<int> BestChromosome;

        private static string LogFilePath = @"C:\Users\coolbots7\Desktop\StandardGA.csv";


        public StandardGA()
        {
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

            //create dandom generation 0
            Random rand = new Random();
            for (int i = 0; i < generationSize; i++)
            {
                chromosome c = new chromosome();
                //c.gene = Enumerable.Range(0, this.Genes) as List<int>;
                c.gene = Enumerable.Range(0, points.Count())
                    .Select(j => new Tuple<int, int>(rand.Next(points.Count()), j))
                    .OrderBy(k => k.Item1)
                    .Select(l => l.Item2).ToList();

                this.CurrentGeneration.Add(c);
            }

            this.generation = 0;
            this.HighScore = Int64.MaxValue;
            this.HighScoreGeneration = -1;

        }

        protected override void Train()
        {
            //test current generation
            double generationHighScore = Int64.MaxValue;
            foreach (chromosome c in this.CurrentGeneration)
            {
                c.score = fittness(c.gene);
                if (c.score < generationHighScore)
                {
                    generationHighScore = c.score;
                }
                if (c.score < this.HighScore)
                {
                    this.HighScore = c.score;
                    this.HighScoreGeneration = this.generation;
                    this.BestChromosome = c.gene;
                }
            }

            //select top
            List<chromosome> nextGeneration = this.CurrentGeneration.OrderByDescending(c => c.score).ToList().GetRange(0, generationCarryover);

            //crossover
            for (int i = 0; i < generationSize - generationCarryover; i++)
            {
                chromosome ParentA = this.RouletteWheelSelection(nextGeneration.GetRange(0, generationCarryover));
                chromosome ParentB = this.RouletteWheelSelection(nextGeneration.GetRange(0, generationCarryover));

                //crossover
                nextGeneration.Add(this.Crossover(ParentA, ParentB));
            }

            //mutate
            foreach (chromosome c in nextGeneration)
            {
                this.Mutate(c);
            }

            this.CurrentGeneration = nextGeneration;

            if (this.generation % 1000 == 0)
            {
                using (StreamWriter file = File.AppendText(LogFilePath))
                {
                    file.WriteLineAsync(DateTime.Now.ToString() + ", " + generation + ", " + this.ElapsedMilliseconds.ToString() + ", " + generationHighScore + ", " + this.HighScore + ", " + this.HighScoreGeneration);
                }
            }

            this.generation++;
        }

        private double fittness(List<int> order)
        {
            double score = 0;

            for (int i = 0; i < order.Count() - 1; i++)
            {
                int[] p1 = points[order[i]];
                int[] p2 = points[order[i + 1]];
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

        public int GetGeneration()
        {
            return this.generation;
        }

        public double GetHighScore()
        {
            return this.HighScore;
        }

        public int GetHighScoreGeneration()
        {
            return this.HighScoreGeneration;
        }

        public string GetBestChromosome()
        {
            try
            {
                if (this.BestChromosome != null)
                    return String.Join(", ", this.BestChromosome);
            }
            catch
            {

            }

            return "none";
        }
    }
}
