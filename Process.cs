using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace os_ass2
{
    public class Process :IComparable<Process>
    {
        /////////////Variables
        public double ArrivalTime;
        public double Priority, BurstTime,RemainingTime;
        public int pNumber;
        public double WaitingTime, TurnAround, WeightedTurnAround;
        public List<Burst> bursts; //contains all the bursts made by this process.
        private Process process;

        //////////Methods
        public Process(int pNumber,double ArrivalTime,double BurstTime,double Priority)
        {
            this.pNumber = pNumber;
            this.ArrivalTime = ArrivalTime;
            this.BurstTime = BurstTime;
            this.Priority = Priority;
            this.bursts = new List<Burst>();
            RemainingTime = BurstTime;
        }
        public Process()
        {

        }
        public Process(Process process)
        {
            this.process = process;
        }

        public int CompareTo(Process pCompare) //compares processes based on arrival time.
        {
            // A null value means that this object is greater.
            if (pCompare == null)
                return 1;

            else
                return this.ArrivalTime.CompareTo(pCompare.ArrivalTime);
        }



    }
}
