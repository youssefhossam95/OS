using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Troschuetz.Random;
using System.IO;
using Wintellect.PowerCollections;

namespace os_ass2
{
    class ProcessGenerator
    {
        public string line;
        public List<string> lines;
        public int j;
        public double NumOfProcs;
        public double ArrivalTimeMean;
        public double ArrivalTimeSD;
        public double BurstTimeMean;
        public double BurstTimeSD;
        public double PriorityDist;
        public NormalDistribution N;
        public NormalDistribution L;
        public PoissonDistribution P;
        public StreamWriter sw;
        public string OutputFileName;




        public ProcessGenerator() { }


        public string Readfile(string path)
        {
            lines = new List<string>();
            StreamReader theReader;
            try
            {
                 theReader= new StreamReader(path);
            }
            catch(Exception e)
            {
                return e.Message;
            }
            while ((line = theReader.ReadLine()) != null)
            {
                List<string> temp = new List<string>(line.Split(' '));

                for (int i = 0; i < (temp.Count()); i++)
                {
                    lines.Add(temp[i]);
                }
            }



            NumOfProcs = Convert.ToDouble(lines[0]);
            ArrivalTimeMean = Convert.ToDouble(lines[1]);
            ArrivalTimeSD = Convert.ToDouble(lines[2]);
            BurstTimeMean = Convert.ToDouble(lines[3]);
            BurstTimeSD = Convert.ToDouble(lines[4]);
            PriorityDist = Convert.ToDouble(lines[5]);
            N = new NormalDistribution();
            L = new NormalDistribution();
            P = new PoissonDistribution();
            sw = Createfile();
            sw.WriteLine(NumOfProcs);
            N.Sigma = ArrivalTimeSD;
            L.Sigma = BurstTimeSD;
            P.Lambda = PriorityDist;
            for (int i = 0; i < NumOfProcs; i++)
            {
                double y = L.NextDouble();
                double x = N.NextDouble();
                double z = P.NextDouble();
                y = y + BurstTimeMean;
                x = x + ArrivalTimeMean;
                x = Math.Abs(Math.Round(x, 3));
                y = Math.Abs(Math.Round(y, 3));
                sw.Write(i + 1);
                sw.Write(" ");
                sw.Write(x);
                sw.Write(" ");
                sw.Write(y);
                sw.Write(" ");
                sw.Write(z);
                sw.Write("\r\n");
            }
            sw.Close();
            return OutputFileName;
        }

        public StreamWriter Createfile()
        {
            j = 0;
            while (File.Exists("Output" + j.ToString() + ".txt"))
            {
                j++;
            }
            OutputFileName = "Output" + j.ToString() + ".txt";
            StreamWriter sw = File.CreateText("Output" + j.ToString() + ".txt");
            return sw;

        }




    }
}

