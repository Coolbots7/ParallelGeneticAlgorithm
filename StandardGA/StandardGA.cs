using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticAlgorithm;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Data.SQLite;

namespace StandardGA
{
    public class StandardGA
    {
        private GA geneticAlgorithm;
        private List<int[]> points;
        private Thread runThread;
        private ManualResetEvent run;

        private Stopwatch runTime;

        private int generation;

        private static int generationSize = 40;
        private static int generationCarryover = 10;

        private double HighScore;
        private int HighScoreGeneration;
        private List<int> BestChromosome;

        private static string LogFilePath = @"C:\Users\coolbots7\Desktop\StandardGA.csv";

        //private static string SQLiteDBPath = "StandardGA.sqlite";
        //private int TestID;

        //private SQLiteConnection m_dbConnection;

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

            //LogFilePath = string.Concat(
            //Path.GetFileNameWithoutExtension(LogFilePath),
            //DateTime.Now.ToFileTime(),
            //Path.GetExtension(LogFilePath)
            //);
            //LogFilePath = Path.GetFileNameWithoutExtension(LogFilePath) + DateTime.Now.ToFileTime() + Path.GetExtension(LogFilePath);

            //bool initDatabase = false;
            //if(!File.Exists(SQLiteDBPath))
            //{
            //    SQLiteConnection.CreateFile(SQLiteDBPath);
            //    initDatabase = true;
            //}

            //this.m_dbConnection = new SQLiteConnection("Data Source=" + SQLiteDBPath + ";Version=3;");
            //this.m_dbConnection.Open();

            //if(initDatabase)
            //{
            //    string createTestSQL = "CREATE TABLE t_Test (ID int NOT NULL PRIMARY KEY AUTO_INCREMENT, CreatedOn DATETIME DEFAULT GETDATE())";
            //    SQLiteCommand createTestTable = new SQLiteCommand(createTestSQL, this.m_dbConnection);
            //    createTestTable.ExecuteNonQuery();

            //    string createTestLogSQL = "CREATE TABLE t_TestLog (ID int NOT NULL PRIMARY KEY AUTO_INCREMENT, TestID int, CreatedOn DATETIME DEFAULT GETDATE())";
            //    SQLiteCommand createTestLogTable = new SQLiteCommand(createTestLogSQL, this.m_dbConnection);
            //    createTestLogTable.ExecuteNonQuery();
            //}

            //SQLiteCommand createTest = new SQLiteCommand("INSERT INTO t_Test () OUTPUT Inserted.ID VALUES ()", this.m_dbConnection);
            //this.TestID = (int)createTest.ExecuteScalar();
            //Console.WriteLine("TEST ID: " + TestID.ToString());


            this.geneticAlgorithm = new GA(points.Count(), generationSize);
            this.runThread = new Thread(loop);
            this.run = new ManualResetEvent(false);
            this.runTime = new Stopwatch();



            this.generation = 0;
            this.HighScore = Int64.MaxValue;
            this.HighScoreGeneration = -1;

            this.runThread.Start();
        }

        public void loop()
        {
            List<chromosome> currentGeneration = this.geneticAlgorithm.CurrentGeneration;
            while (this.run.WaitOne())
            {
                //test current generation
                double generationHighScore = Int64.MaxValue;
                foreach (chromosome c in currentGeneration)
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
                List<chromosome> nextGeneration = currentGeneration.OrderByDescending(c => c.score).ToList().GetRange(0, generationCarryover);

                //crossover
                for (int i = 0; i < generationSize - generationCarryover; i++)
                {
                    chromosome ParentA = this.geneticAlgorithm.RouletteWheelSelection(nextGeneration.GetRange(0, generationCarryover));
                    chromosome ParentB = this.geneticAlgorithm.RouletteWheelSelection(nextGeneration.GetRange(0, generationCarryover));

                    //crossover
                    nextGeneration.Add(this.geneticAlgorithm.Crossover(ParentA, ParentB));
                }

                //mutate
                foreach (chromosome c in nextGeneration)
                {
                    this.geneticAlgorithm.Mutate(c);
                }

                currentGeneration = nextGeneration;

                if (this.generation % 1000 == 0)
                {
                    using (StreamWriter file = File.AppendText(LogFilePath))
                    {
                        file.WriteLineAsync(DateTime.Now.ToString() + ", " + generation + ", " + this.runTime.ElapsedMilliseconds.ToString() + ", " + generationHighScore + ", " + this.HighScore + ", " + this.HighScoreGeneration);
                    }
                }

                this.generation++;
            }
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
        }

        public int GetGeneration()
        {
            return this.generation;
        }

        public long GetRuntime()
        {
            return this.runTime.ElapsedMilliseconds;
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
