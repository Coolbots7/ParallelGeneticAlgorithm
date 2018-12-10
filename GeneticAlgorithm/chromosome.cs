using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm
{
    public class chromosome
    {
        public List<int> gene { get; set; }
        public double score { get; set; }
        public double cumulativeScore { get; set; }

        public chromosome()
        {
            gene = new List<int>();
            score = double.MaxValue;
        }
    }
}
