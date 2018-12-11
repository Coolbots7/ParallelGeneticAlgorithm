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
        private int generation;

        private int generationSize;
        private int generationCarryover;

        private double HighScore;
        private int HighScoreGeneration;
        private List<int> BestChromosome;

        public delegate double FittnessDelegate(GeneticAlgorithm.chromosome c);
        private FittnessDelegate _fittness;

        private static string LogFilePath = @"C:\Users\coolbots7\Desktop\StandardGA.csv";


        public StandardGA(int GeneSize, int GenerationSize, int GenerationCarryover, FittnessDelegate fittness)
        {

            this.generationSize = GenerationSize;
            this.generationCarryover = GenerationCarryover;

            this._fittness = fittness;

            //create random generation 0
            Random rand = new Random();
            for (int i = 0; i < generationSize; i++)
            {
                chromosome c = new chromosome();
                c.gene = Enumerable.Range(0, GeneSize)
                    .Select(j => new Tuple<int, int>(rand.Next(GeneSize), j))
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
            double generationHighScore = double.MaxValue;
            foreach (chromosome c in this.CurrentGeneration)
            {
                c.score = this._fittness(c);
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
            List<chromosome> top = this.CurrentGeneration.OrderBy(c => c.score).ToList().GetRange(0, generationCarryover);
            List<chromosome> nextGeneration = new List<chromosome>();

            //crossover
            for (int i = 0; i < generationSize - generationCarryover; i++)
            {
                chromosome ParentA = this.RouletteWheelSelection(top);
                chromosome ParentB = this.RouletteWheelSelection(top);

                //crossover
                nextGeneration.Add(this.Crossover(ParentA, ParentB));
            }

            //mutate
            foreach (chromosome c in nextGeneration)
            {
                this.Mutate(c);
            }

            //Add top performers from current generation to next generation
            nextGeneration.AddRange(new List<chromosome>(top));
            this.CurrentGeneration = nextGeneration;

            if (this.generation % 1 == 0)
            {
                using (StreamWriter file = File.AppendText(LogFilePath))
                {
                    file.WriteLineAsync(DateTime.Now.ToString() + ", " + generation + ", " + this.ElapsedMilliseconds.ToString() + ", " + generationHighScore + ", " + this.HighScore + ", " + this.HighScoreGeneration);
                }
            }

            this.generation++;
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
