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

        private int _numThreads;

        private List<chromosome> CompletedTesting;
        private List<chromosome> NextGeneration;

        private double HighScore;
        private int HighScoreGeneration;
        private List<int> BestChromosome;


        public ParallelGA(int GeneSize, int GenerationSize, int GenerationCarryover, int NumThreads, FittnessDelegate fittness)
        {
            this.generationSize = GenerationSize;
            this._fittness = fittness;
            this.generationCarryover = GenerationCarryover;
            this._numThreads = NumThreads;

            this.CompletedTesting = new List<chromosome>();
            this.NextGeneration = new List<chromosome>();

            //genrate rangom 0th generation
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

            //split current generation across threads to test
            this.CompletedTesting.Clear();
            WaitHandle[] waitHandles = new WaitHandle[this._numThreads];
            int numPerThread = (int)Math.Ceiling((double)(this.generationSize / this._numThreads));
            for (int i = 0; i < this._numThreads; i++)
            {
                var j = i;
                var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
                int index = j * numPerThread;
                var thread = new Thread(() => TestThread(handle, this.CurrentGeneration.GetRange(index, Math.Min(numPerThread,this.CurrentGeneration.Count() - index))));

                waitHandles[j] = handle;
                thread.Start();
            }

            //wait for all test threads to finish
            WaitHandle.WaitAll(waitHandles);

            //test fittness of current generation
            foreach (chromosome c in this.CompletedTesting.Where(c => c != null))
            {
                if (c.score < this.HighScore)
                {
                    this.HighScore = c.score;
                    this.HighScoreGeneration = this.generation;
                    this.BestChromosome = c.gene;
                }
            }

            List<chromosome> top = this.CompletedTesting.Where(c => c != null).OrderBy(c => c.score).ToList().GetRange(0, this.generationCarryover);

            this.NextGeneration.Clear();

            WaitHandle[] generateWaitHandles = new WaitHandle[this._numThreads];
            int numToGenerate = (int)Math.Ceiling((double)this.generationSize / this._numThreads);
            for(int i=0;i<this._numThreads;i++)
            {
                var j = i;
                var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
                var thread = new Thread(() => GenerationThread(handle, top, numToGenerate));

                generateWaitHandles[j] = handle;
                thread.Start();
            }
            WaitHandle.WaitAll(generateWaitHandles);

            this.NextGeneration.AddRange(new List<chromosome>(top));
            this.CurrentGeneration = new List<chromosome>(this.NextGeneration);

            this.generation++;
        }

        private void TestThread(EventWaitHandle handle, List<chromosome> subset)
        {
            foreach (chromosome c in subset.Where(c => c != null))
            {
                c.score = this._fittness(c);
                this.CompletedTesting.Add(new chromosome(c));
            }

            handle.Set();
        }

        private void GenerationThread(EventWaitHandle handle, List<chromosome> top, int numToGenerate)
        {
            for (int i = 0; i < numToGenerate; i++)
            {
                chromosome ParentA = this.RouletteWheelSelection(top);
                chromosome ParentB = this.RouletteWheelSelection(top);

                //crossover
                chromosome Child = this.Crossover(ParentA, ParentB);

                this.Mutate(Child);

                this.NextGeneration.Add(Child);
            }

            handle.Set();
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
