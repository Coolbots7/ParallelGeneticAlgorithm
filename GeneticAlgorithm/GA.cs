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
        public GA(int genes, int chromosomes)
        {
            //genes - number of genes in a chromosome
            //chromosomes - number of chromosomes in a generation

            //TODO create initial chromosomes with random genes
        }

        public void Crossover()
        {
            //inputs - Parent A, Parent B
            //output - Child

            //TODO generate corssover index
            //TODO copy up to crossover index of Parent A to Child
            //TODO copy from Parent B to end into Child leaving indexes of genes already in Child blank
            //TODO iterate through Parent B adding missing genes into next blank in Child

        }

        public void Mutate()
        {
            //input - unmutated child, mutation rate
            //output - mutated child

            //TODO generate random index
            //TODO swap genes about index
        }
    }
}
