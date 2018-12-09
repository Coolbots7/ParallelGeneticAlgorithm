using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace GeneticAlgorithm
{
    public class GA
    {
        //mutation rate - probability of a mutation occuring
        public List<chromosome> CurrentGeneration;

        private int Genes;
        private int Chromosomes;
        private double MutationProbability;

        public GA(int genes, int chromosomes)
        {
            //genes - number of genes in a chromosome
            //chromosomes - number of chromosomes in a generation
            //success - number of chromosomes to carry over from last generation to next generation and use for crossover to fill remaining chromosomes

            this.Genes = genes;
            this.Chromosomes = chromosomes;
            this.MutationProbability = 0.33;

            //create initial chromosomes with random genes
            this.CurrentGeneration = new List<chromosome>();
            Random rand = new Random();
            for (int i = 0; i < this.Chromosomes; i++)
            {
                chromosome c = new chromosome();
                //c.gene = Enumerable.Range(0, this.Genes) as List<int>;
                c.gene = Enumerable.Range(0, this.Genes)
                    .Select(j => new Tuple<int, int>(rand.Next(this.Genes), j))
                    .OrderBy(k => k.Item1)
                    .Select(l => l.Item2).ToList();

                this.CurrentGeneration.Add(c);
            }
        }

        public chromosome RouletteWheelSelection(List<chromosome> options)
        {
            options = options.OrderBy(c => c.score).ToList();
            double scoreSum = 0;
            double scoreMin = options[0].score;
            foreach(chromosome c in options)
            {
                scoreSum += c.score;
                c.cumulativeScore = scoreSum;
            }

            Random rand = new Random();
            double randval = (rand.NextDouble() * (scoreSum - scoreMin)) + scoreMin;
            return options.OrderBy(c => c.cumulativeScore).FirstOrDefault(c => randval > c.cumulativeScore);
        }

        public chromosome Crossover(chromosome ParentA, chromosome ParentB)
        {
            //inputs - Parent A, Parent B
            //output - Child

            chromosome Child = new chromosome();

            //generate corssover index
            int crossoverIdx = (new Random()).Next(this.Genes);

            //copy up to crossover index of Parent A to Child
            Child.gene = ParentA.gene.GetRange(0, crossoverIdx);

            //copy from Parent B to end into Child leaving indexes of genes already in Child blank
            for (int i = crossoverIdx; i < this.Genes; i++)
            {
                int gene = ParentB.gene[i];

                if(!Child.gene.Contains(gene)) {
                    Child.gene.Add(gene);
                }
                else {
                    Child.gene.Add(Int32.MinValue);
                }
            }

            //iterate through Parent B adding missing genes into next blank in Child
            if (Child.gene.Contains(Int32.MinValue))
            {
                for (int i = 0; i < this.Genes; i++)
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

        public void Mutate(chromosome Child)
        {

            Random rand = new Random();
            //generate random index
            //int mutateIdx = rand.Next(this.Genes - 1);

            //swap genes about index using probability
            //if ((rand.Next(101) / 100) <= this.MutationProbability)
            //{
            //    Int32 temp = Child.gene[mutateIdx];
            //    Child.gene[mutateIdx] = Child.gene[mutateIdx + 1];
            //    Child.gene[mutateIdx + 1] = temp;
            //}

            for(int i=0;i<Child.gene.Count()-1;i++)
            {
                if(rand.NextDouble() < this.MutationProbability)
                {
                    Int32 temp = Child.gene[i];
                    Child.gene[i] = Child.gene[i + 1];
                    Child.gene[i + 1] = temp;
                }
            }
        }
    }
}
