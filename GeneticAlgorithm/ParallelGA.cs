using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Security.Permissions;

namespace GeneticAlgorithm
{
    public class ParallelGA : GA
    {
        public delegate double FittnessDelegate(GeneticAlgorithm.chromosome c);
        private FittnessDelegate _fittness;

        private int generation;

        private int generationSize;
        private int generationCarryover;


        private List<chromosome> NextGeneration;
        private List<chromosome> TopPerformers;

        private double HighScore;
        private int HighScoreGeneration;
        private List<int> BestChromosome;

        // The mechanism for waking up the second thread once data is available
        AutoResetEvent _dataAvailable = new AutoResetEvent(false);
        ManualResetEvent _nextGenerationReadt = new ManualResetEvent(false);
        // The mechanism for making sure that the data object is not overwritten while it is being read.
        ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        Thread nextGeneration;

        public ParallelGA(int GeneSize, int GenerationSize, int GenerationCarryover, FittnessDelegate fittness)
        {
            this.generationSize = GenerationSize;
            this._fittness = fittness;
            this.generationCarryover = GenerationCarryover;

            this.NextGeneration = new List<chromosome>();
            this.TopPerformers = new List<chromosome>();



            //genrate rangom 0th generation
            Random rand = new Random();
            for (int i = 0; i < generationSize; i++)
            {
                chromosome c = new chromosome();
                //c.gene = Enumerable.Range(0, this.Genes) as List<int>;
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
            //start thread to generate next generation in parallel
            this.NextGeneration.Clear();
            this.TopPerformers.Clear();

            nextGeneration = new Thread(GenerateNextGeneration);
            nextGeneration.Start();

            //test fittness of current generation
            foreach (chromosome c in this.CurrentGeneration)
            {
                c.score = this._fittness(c);

                //pass to thread making next generaiton
                _readWriteLock.EnterWriteLock();
                try
                {
                    List<chromosome> tested = this.CurrentGeneration.Where(x => x.score < double.MaxValue).ToList();
                    this.TopPerformers = tested.OrderBy(x => x.score).ToList().GetRange(0, Math.Min(this.generationCarryover, tested.Count()));
                    this._dataAvailable.Set();
                }
                finally
                {
                    _readWriteLock.ExitWriteLock();
                }

                if (c.score < this.HighScore)
                {
                    this.HighScore = c.score;
                    this.HighScoreGeneration = this.generation;
                    this.BestChromosome = c.gene;
                }
            }

            //wait for next generaiton to be ready
            this._nextGenerationReadt.WaitOne();
            KillNextGeneraitonThread();

            this.NextGeneration.AddRange(this.TopPerformers);

            while (this.NextGeneration.Count() < this.generationSize)
            {
                chromosome ParentA = this.RouletteWheelSelection(this.TopPerformers);
                chromosome ParentB = this.RouletteWheelSelection(this.TopPerformers);

                chromosome Child = this.Crossover(ParentA, ParentB);
                this.Mutate(Child);
                this.NextGeneration.Add(Child);
            }

            this.CurrentGeneration = new List<chromosome>(this.NextGeneration);

            this.generation++;
        }

        private void GenerateNextGeneration()
        {
            //TODO determine when next generation is ready and leave thread

            List<chromosome> topPerformers;
            while (true)
            {
                _dataAvailable.WaitOne();
                this._nextGenerationReadt.Reset();
                _readWriteLock.EnterReadLock();
                try
                {
                    topPerformers = this.TopPerformers;

                    if (topPerformers.Count() > 2 && this.NextGeneration.Count() < this.generationSize - this.generationCarryover)
                    {
                        //for (int i = 0; i < 1; i++)
                        //{
                        chromosome ParentA = this.RouletteWheelSelection(topPerformers);
                        chromosome ParentB = this.RouletteWheelSelection(topPerformers);

                        chromosome Child = this.Crossover(ParentA, ParentB);
                        this.Mutate(Child);
                        this.NextGeneration.Add(Child);
                        //}
                    }
                }
                finally
                {
                    _readWriteLock.ExitReadLock();
                }

                this._nextGenerationReadt.Set();
            }
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        private void KillNextGeneraitonThread()
        {
            nextGeneration.Abort();
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
