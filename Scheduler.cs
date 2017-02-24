using Wintellect.PowerCollections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace os_ass2
{
    class Scheduler
    {
        public int Algoritm;  // 0=HPF  1=RR   2=SRTN
        public List<Process> processes; //list of processes to be simulated.
        double ContextSwitching;
        double Quanta, averagePriority;
        string input_path;
        public Scheduler(string path, int algorithm, double ContextSwitching, double Quanta)
        {
            this.Algoritm = algorithm;
            input_path = path;
            this.ContextSwitching = ContextSwitching;
            this.Quanta = Quanta;
            this.processes = new List<Process>();
        }
        public void ReadInput() //reads an input file that contains processes data.
        {
            StreamReader sr = new StreamReader(input_path);
            int prCount = Convert.ToInt32(sr.ReadLine());
            double sumPriority = 0;
            for (int i = 0; i < prCount; i++) //building processes list.
            {
                string line = sr.ReadLine();
                string[] processData = line.Split(' ');
                Process temp = new Process(Convert.ToInt32(processData[0]), Convert.ToDouble(processData[1]), Convert.ToDouble(processData[2]), Convert.ToDouble(processData[3]));
                processes.Add(temp);
                sumPriority += Convert.ToDouble(processData[3]);
            }
            averagePriority = sumPriority / processes.Count;
        }
        public List<Process> StartSimulation() //calls the relevant function for the scheduling algorithm used.
        {
            List<Process> temp = new List<Process>();
            for (int i = 0; i < processes.Count; i++)
                temp.Add(processes[i]);
            temp.Sort(); //sort processes based on arrival time.
            Queue<Process> q = new Queue<Process>(); //constructing a queue holding all processes in order.
            for (int i = 0; i < processes.Count; i++)
            {
                q.Enqueue(temp[i]);
            }
            switch (Algoritm) //call relevant function.
            {
                case 0:
                    RunHPF(q);//use HPF.
                    break;
                case 1:
                    RunRR(q);//use RR.
                    break;
                case 2:
                    RunSRTN(q);//use SRTN.
                    break;
                case 3:
                    RunCustomisedRR(q); //use customised RR.
                    break;
            }
            WriteToFile();
            return processes;
        }
        void RunHPF(Queue<Process> q) //performs HPF algorithm simulation on the processes inside q.
        {
            //q has all the processes sorted based on arrival time.
            OrderedBag<Process> PriorityQueue = new OrderedBag<Process>(delegate (Process x, Process y) //Create priority queue where the highest priority process is the last element in the queue.
            {
                return x.Priority.CompareTo(y.Priority);
            });
            double Time = q.First().ArrivalTime;
            PriorityQueue.Add(q.Dequeue()); //add first proccess to arrive.
            //the priority queue represents the processes waiting to run while q will contain processes that still haven't arrived.
            while (q.Count() != 0 || PriorityQueue.Count() != 0)//simulation loop.
            {
                Burst tempB = new Burst();
                tempB.start = Time;
                Time += PriorityQueue.Last().BurstTime;
                tempB.end = Time;
                processes[PriorityQueue.Last().pNumber - 1].bursts.Add(tempB);
                PriorityQueue.RemoveLast();
                if (q.Count == 0)
                {
                    Time += ContextSwitching;
                    continue;
                }
                if (PriorityQueue.Count() == 0 && q.Count != 0 && q.First().ArrivalTime > Time) //done with executing all ready processes but still some processes hadn't arrived yet.
                {
                    Time = q.First().ArrivalTime;
                }
                Time += ContextSwitching;
                while (q.First().ArrivalTime <= Time) //add all arrived processes to priority queue(ready queue).
                {
                    PriorityQueue.Add(q.Dequeue()); //remove from q and add to ready queue.
                    if (q.Count == 0)
                        break;
                }
            }
        }
        void RunRR(Queue<Process> q) //simulates RR algorithm.
        {
            //q sorted by arrival time.
            Queue<Process> readyQueue = new Queue<Process>();
            readyQueue.Enqueue(q.Dequeue());//insert first element to arrive in ready queue.
            double Time = readyQueue.First().ArrivalTime;
            Process tempProcess = null;
            while (readyQueue.Count != 0 || q.Count != 0) //simulation loop.
            {
                Burst tempB = new Burst();

                if (readyQueue.Count == 1 && tempProcess != null) //only one process in ready queue -> remove context switching time.
                    Time -= ContextSwitching;
                tempProcess = null;
                tempB.start = Time;
                if (readyQueue.Count == 0) //no processes ready but still some processes hadn't arrived.
                {
                    Time = q.First().ArrivalTime;
                    readyQueue.Enqueue(q.Dequeue());
                    continue;
                }
                double burstLength = Math.Min(readyQueue.First().RemainingTime, Quanta);
                Time += burstLength;
                readyQueue.First().RemainingTime -= burstLength;
                tempB.end = Time;
                readyQueue.First().bursts.Add(tempB);
                if (readyQueue.First().RemainingTime == 0)
                    readyQueue.Dequeue(); //the process has zero remaining time -> remove from ready queue.
                else
                    tempProcess = readyQueue.Dequeue();//remove the first element and store it in tempProcess.  


                if (q.Count != 0)
                {
                    while (q.First().ArrivalTime <= Time) //moves all processes that arrived to readyqueue.
                    {
                        readyQueue.Enqueue(q.Dequeue());
                        if (q.Count == 0)
                            break;
                    }
                }
                if (tempProcess != null) //push the last executed process to the back.
                    readyQueue.Enqueue(tempProcess);
                Time += ContextSwitching;
            }
        }

        void RunSRTN(Queue<Process> q)  
        {
            //q is sorted on arrival time.
            OrderedBag<Process> ready = new OrderedBag<Process>(delegate (Process x, Process y) //priority queue sorted based on remaining time.
            {
                return x.RemainingTime.CompareTo(y.RemainingTime);
            });
            double Time = 0;
            Burst b = new Burst();
            while (q.Count != 0 || ready.Count != 0)
            {
                if (ready.Count == 0)
                {
                    Time = q.First().ArrivalTime;
                    ready.Add(q.Dequeue());
                    b = new Burst();
                    b.start = Time;
                }
                if (q.Count != 0)
                {
                    int currentPno = ready.First().pNumber;
                    if (q.First().ArrivalTime <= Time)
                        ready.Add(q.Dequeue());
                    int newPno = ready.First().pNumber;
                    if (currentPno != newPno) //a process with shorter remaining time arrived.
                    {
                        b.end = Time;
                        processes[currentPno - 1].bursts.Add(b);
                        Time += ContextSwitching;
                        if (q.Count != 0)
                        {
                            bool added = false;//indicates wither one of the processes that arrived during context switch has less remaining time than the process that is about to start.
                            while (q.First().ArrivalTime <= Time)//add processes that arrived during context switch.
                            {
                                if(q.First().RemainingTime<ready.First().RemainingTime)
                                    added = true;
                                ready.Add(q.Dequeue());
                                if (q.Count == 0)
                                    break;
                            }
                            if (added)//an extra context switch will take place 
                            {
                                Time += ContextSwitching;
                            }
                        }
                        b = new Burst();
                        b.start = Time;
                    }
                }
                Time += 0.001;
                ready.First().RemainingTime -= 0.001;
                if (ready.First().RemainingTime <= 0)
                {
                    b.end = Time;
                    ready.First().bursts.Add(b);
                    b = new Burst();
                    Time += ContextSwitching;
                    b.start = Time;
                    ready.RemoveFirst();
                    if (q.Count == 0)
                        continue;
                    bool added = false;//indicates wither one of the processes that arrived during context switch has less remaining time than the process that is about to start.
                    while (q.First().ArrivalTime <= Time)//add processes that arrived during context switch.
                    {
                        if (q.First().RemainingTime < ready.First().RemainingTime)
                            added = true;
                        ready.Add(q.Dequeue());
                        if (q.Count == 0)
                            break;
                    }
                    if (added)
                    {
                        Time += ContextSwitching; //an extra context switch will take place
                        b.start += ContextSwitching;
                    }
                }
            }
        }
        void RunCustomisedRR(Queue<Process> q) //simulates Customized RR algorithm.
        {
            Deque<Process> readyQueue = new Deque<Process>();

            readyQueue.AddToBack(q.Dequeue());//insert first element in ready queue.
            double Time = readyQueue.First().ArrivalTime;
            Process tempProcess = null;
            while (readyQueue.Count != 0 || q.Count != 0) //simulation loop.
            {
                Burst tempB = new Burst();

                if (readyQueue.Count == 1 && tempProcess != null) //only one process in ready queue -> remove context switching time.
                    Time -= ContextSwitching;
                tempProcess = null;
                tempB.start = Time;
                if (readyQueue.Count == 0) //no processes ready but still some processes hadn't arrived.
                {
                    Time = q.First().ArrivalTime;
                    readyQueue.AddToBack(q.Dequeue());
                    continue;
                }
                double relativeQuanta = Quanta * (readyQueue.First().Priority) / averagePriority;
                if (readyQueue.First().RemainingTime <= relativeQuanta)//last quanta for the process.
                {
                    if (q.Count != 0 && q.First().ArrivalTime < readyQueue.First().RemainingTime + Time && q.First().Priority > readyQueue.First().Priority) //if a higher priority interrupted the process during its remaining time.
                    {
                        readyQueue.First().RemainingTime -= (q.First().ArrivalTime - Time);//run till higher priority arrives.
                        Time = q.First().ArrivalTime;
                        tempB.end = Time;
                        readyQueue.First().bursts.Add(tempB);
                        readyQueue.AddToFront(q.Dequeue());//add higher priority process to the front.
                    }
                    else
                    {
                        Time += readyQueue.First().RemainingTime;
                        readyQueue.First().RemainingTime = 0;
                        tempB.end = Time;
                        readyQueue.First().bursts.Add(tempB);
                        readyQueue.RemoveFromFront(); //the process has zero remaining time -> remove from ready queue.
                    }
                }
                else
                {
                    if (q.Count != 0 && q.First().ArrivalTime < relativeQuanta + Time && q.First().Priority > readyQueue.First().Priority) //if a higher priority interrupted the process during its remaining time.
                    {
                        readyQueue.First().RemainingTime -= (q.First().ArrivalTime - Time);//run till higher priority arrives.
                        Time = q.First().ArrivalTime;
                        tempB.end = Time;
                        readyQueue.First().bursts.Add(tempB);
                        tempProcess = readyQueue.RemoveFromFront();//remove first element and store it in tempProcess.
                        readyQueue.AddToFront(q.Dequeue());//add higher priority process to the front.
                    }
                    else
                    {
                        Time += relativeQuanta;
                        readyQueue.First().RemainingTime -= relativeQuanta;
                        tempB.end = Time;
                        readyQueue.First().bursts.Add(tempB);
                        tempProcess = readyQueue.RemoveFromFront();//remove first element and store it in tempProcess.
                    }
                }
                if (q.Count != 0)
                {
                    while (q.First().ArrivalTime <= Time) //moves all processes that arrived to readyqueue.
                    {
                        readyQueue.AddToBack(q.Dequeue());
                        if (q.Count == 0)
                            break;
                    }
                }
                if (tempProcess != null) //push the last executed process to the back.
                    readyQueue.AddToBack(tempProcess);
                Time += ContextSwitching;
            }

        }
        void WriteToFile()//calculates statistics and writes the results to output file.
        {
            StreamWriter sw = File.CreateText("Statistics.txt");
            double turnAroundSum = 0, weightedTurnAroundSum = 0;
            sw.AutoFlush = true;
            sw.WriteLine("Pnumber Waiting TurnAround WTurnAround ");
            foreach (Process p in processes)
            {
                p.TurnAround = p.bursts.Last().end - p.ArrivalTime;
                p.WaitingTime = p.TurnAround - p.BurstTime;
                p.WeightedTurnAround = Math.Round(p.TurnAround / p.BurstTime, 3);
                turnAroundSum += p.TurnAround;
                weightedTurnAroundSum += p.WeightedTurnAround;
                sw.Write(p.pNumber);
                sw.Write(' ');
                sw.Write(Math.Round(p.WaitingTime, 3));
                sw.Write(' ');
                sw.Write(Math.Round(p.TurnAround, 3));
                sw.Write(' ');
                sw.WriteLine(Math.Round(p.WeightedTurnAround, 3));
            }
            sw.WriteLine(Math.Round(turnAroundSum / processes.Count, 3));//average turnaround
            sw.WriteLine(Math.Round(weightedTurnAroundSum / processes.Count, 3)); //average WTA
            sw.Close();
        }
    }

}
