using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ServerTry1
{
    class Program
    {
        static void Main(string[] args)
        {
            FindingRivalHandle findRivalHAgent = new FindingRivalHandle();
            ServerClass sv = new ServerClass(findRivalHAgent);
        
            Thread serverThread = new Thread(new ThreadStart(sv.SelectLoop));
            Thread competitionThread = new Thread(new ThreadStart(findRivalHAgent.FindRival));
            

            competitionThread.Start();
            serverThread.Start();

           
        }
    }
}
