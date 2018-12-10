using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using System.Threading;
using System.Diagnostics;

namespace GeneticAlgorithm
{
    public abstract class GA
    {
        protected List<chromosome> CurrentGeneration;


        private Thread runThread;
        private ManualResetEvent run;
        private Stopwatch runTime;

        //mutation rate - probability of a mutation occuring
        public double MutationProbability { get; set; } = 0.33;

        public GA()
        {
            this.CurrentGeneration = new List<chromosome>();


            this.runThread = new Thread(loop);
            this.run = new ManualResetEvent(false);
            this.runTime = new Stopwatch();

            //Start loop thread with ManualResetEvent reset
            this.runThread.Start();
        }

        private void loop()
        {
            while (this.run.WaitOne())
            {
                Train();
            }
        }

        protected abstract void Train();

        protected chromosome RouletteWheelSelection(List<chromosome> options)
        {
            options = options.OrderBy(c => c.score).ToList();
            double scoreSum = 0;
            double scoreMin = options[0].score;
            foreach (chromosome c in options)
            {
                scoreSum += c.score;
                c.cumulativeScore = scoreSum;
            }

            Random rand = new Random();
            double randval = (rand.NextDouble() * (scoreSum - scoreMin)) + scoreMin;
            return options.OrderBy(c => c.cumulativeScore).FirstOrDefault(c => randval > c.cumulativeScore);
        }

        protected chromosome Crossover(chromosome ParentA, chromosome ParentB)
        {
            //inputs - Parent A, Parent B
            //output - Child

            chromosome Child = new chromosome();

            //generate corssover index
            int crossoverIdx = (new Random()).Next(ParentA.gene.Count());

            //copy up to crossover index of Parent A to Child
            Child.gene = ParentA.gene.GetRange(0, crossoverIdx);

            //copy from Parent B to end into Child leaving indexes of genes already in Child blank
            for (int i = crossoverIdx; i < ParentB.gene.Count(); i++)
            {
                int gene = ParentB.gene[i];

                if (!Child.gene.Contains(gene))
                {
                    Child.gene.Add(gene);
                }
                else
                {
                    Child.gene.Add(Int32.MinValue);
                }
            }

            //iterate through Parent B adding missing genes into next blank in Child
            if (Child.gene.Contains(Int32.MinValue))
            {
                for (int i = 0; i < ParentB.gene.Count(); i++)
                {
                    int gene = ParentB.gene[i];

                    if (!Child.gene.Contains(gene))
                    {
                        int idx = Child.gene.IndexOf(Int32.MinValue);
                        Child.gene[idx] = gene;
                    }
                }
            }

            return Child;
        }

        protected void Mutate(chromosome Child)
        {

            Random rand = new Random();

            for (int i = 0; i < Child.gene.Count() - 1; i++)
            {
                if (rand.NextDouble() < this.MutationProbability)
                {
                    Int32 temp = Child.gene[i];
                    Child.gene[i] = Child.gene[i + 1];
                    Child.gene[i + 1] = temp;
                }
            }
        }

        public bool Start()
        {
            this.run.Set();
            this.runTime.Start();
            return true;
        }

        public bool Stop()
        {
            this.run.Reset();
            this.runTime.Stop();
            return true;
        }

        public bool Reset()
        {
            throw new NotImplementedException();
            this.runTime.Reset();

            return true;
        }

        public long ElapsedMilliseconds
        {
            get
            {
                return this.runTime.ElapsedMilliseconds;
            }
        }

        public Stopwatch GetStopwatch
        {
            get
            {
                return this.runTime;
            }
        }
    }
}
