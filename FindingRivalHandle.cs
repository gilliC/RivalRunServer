using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace ServerTry1
{
    class FindingRivalHandle
    {
        public static ArrayList[] RivalSearcher = new ArrayList[2];
        DataBaseClass dbAgent;
        List<Client> rivalList;
        List<Client> RivalCopyList;

        public struct Client
        {
            public Socket socket;
            public string userName;
            public int CompetitionType; //time = 0, distance = 1
            public int timer;
            public ServerClass serverAgent;
        }

        public FindingRivalHandle()
        {
            rivalList = new List<Client>();
            RivalSearcher[0] = new ArrayList();
            RivalSearcher[0].Add(null);
            RivalSearcher[0].Add(null);
            RivalSearcher[1] = new ArrayList();
            RivalSearcher[1].Add(null);
            RivalSearcher[1].Add(null);
            dbAgent = new DataBaseClass();
        }
        public bool AddToRivalList(Socket s, string userName, string competitionType,ServerClass s1)//הפעולה של הסרבר
        {
            try
            {
                Client c = new Client();
                c.socket = s;
                c.userName = userName;
                c.serverAgent = s1;
                if (competitionType == "Time")
                    c.CompetitionType = 0;
                else c.CompetitionType = 1;
                rivalList.Add(c);
                return true;
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); return false; }
        }
        protected bool Erase(Client user,int place)
        {
            try
            {
                Remove(user);
                RivalSearcher[0][place] = null;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        protected bool Remove(Client c)
        {
            for (int i = 0; i < rivalList.Capacity; i++)
            {
                if (rivalList[i].socket == c.socket)
                {
                    rivalList.RemoveAt(i);
                    return true;
                }
            }
            return false;

        }
        protected void Found(Client user, Client rival, int place)
        {

            user.timer++;
            if (user.socket != rival.socket)
            {
                if (user.CompetitionType == 0)////////Time Competition
                {
                    CompetitionHandler chAgent = new CompetitionHandler(user.socket, rival.socket, user.userName, rival.userName,user.serverAgent);

                    Erase(user,place);
                    Erase(rival, place);

                    Thread timeCompeThread = new Thread(new ThreadStart(chAgent.CompetitionByTime));
                    timeCompeThread.Start();
                }
            }


        }
        public void FindRival()
        {
            while (true)
            {

                for (int i = 0; i < rivalList.Count; i++)
                {

                    Client clientTemp = rivalList[i];
                    if (clientTemp.timer < 5)
                    {
                        if (clientTemp.timer == 1)
                            ServerClass.RemoveSocket(clientTemp.socket);

                        try
                        {
                            if (RivalSearcher[clientTemp.CompetitionType][dbAgent.GetLevel(clientTemp.userName)] == null)
                                if (RivalSearcher[clientTemp.CompetitionType].Count > 2)
                                {
                                    if (RivalSearcher[clientTemp.CompetitionType][dbAgent.GetLevel(clientTemp.userName) + 1] == null)
                                        if (i > 0)
                                            if (RivalSearcher[clientTemp.CompetitionType][dbAgent.GetLevel(clientTemp.userName) - 1] == null)
                                                RivalSearcher[clientTemp.CompetitionType][dbAgent.GetLevel(clientTemp.userName)] = clientTemp;

                                            else
                                            {
                                                Client x = (Client)RivalSearcher[clientTemp.CompetitionType][dbAgent.GetLevel(clientTemp.userName)];
                                                if (clientTemp.socket!=x.socket)
                                                Found(clientTemp, x, dbAgent.GetLevel(clientTemp.userName));
                                            }
                                }
                                else
                                {
                                    RivalSearcher[clientTemp.CompetitionType][dbAgent.GetLevel(clientTemp.userName)] = clientTemp;
                                }
                            else Found(clientTemp, (Client)RivalSearcher[clientTemp.CompetitionType][dbAgent.GetLevel(clientTemp.userName)], dbAgent.GetLevel(clientTemp.userName));

                        }
                        catch (Exception e) { }
                    }
                    else
                    {
                        string answer = "<Message><Type>AnswerNotFound</Type></Message>";
                        byte[] bAnswer = System.Text.Encoding.ASCII.GetBytes(answer);
                        clientTemp.socket.Send(bAnswer);

                        Erase(clientTemp, dbAgent.GetLevel(clientTemp.userName));
                        ServerClass.AddingSocketToArryList(clientTemp.socket);

                    }
                    clientTemp.timer = clientTemp.timer + 1;

                }

            }

        }
    }

}

