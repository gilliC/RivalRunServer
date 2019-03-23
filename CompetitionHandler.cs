using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.IO;
using System.Xml;
using System.Threading;

namespace ServerTry1
{
    class CompetitionHandler
    {
        protected string userName, rivalUserName, dataUser1, dataUser2, userName1, userName2;
        protected Socket rivalSocket, userSocket;
        protected DataBaseClass dbAgent;
        protected ServerClass serverAgent;
        protected double distance1, distance2;
        protected bool isCompete = true;

        public CompetitionHandler(Socket s1, Socket s2, string usName1, string usName2, ServerClass server1)
        {
            userSocket = s1;
            userName = usName1;
            rivalSocket = s2;
            rivalUserName = usName2;
            dbAgent = new DataBaseClass();
            serverAgent = server1;
        }

        public void CompetitionByTime()
        {
            string dataUser1 = "", dataUser2 = "";
            //string username="", rivalname="";
            Console.WriteLine(rivalUserName + "  vs  " + userName);

            /////////////////MESSAGE TO USER1///////////////////////////////
            string xmlAnswer = "<Message><Type>AnswerFound</Type>";
            xmlAnswer += "<RivalUserName>" + rivalUserName + "</RivalUserName> " + "<RivalLevel>" + dbAgent.GetLevel(rivalUserName) + "</RivalLevel><UserLevel>" + dbAgent.GetLevel(userName) + "</UserLevel>\n";
            byte[] bAnswer = System.Text.Encoding.ASCII.GetBytes(xmlAnswer);
            userSocket.Send(bAnswer);

            /////////////////MESSAGE TO RIVAL///////////////////////////////
            xmlAnswer = "<Message><Type>AnswerFound</Type>";
            xmlAnswer += "<RivalUserName>" + userName + "</RivalUserName> " + "<RivalLevel>" + dbAgent.GetLevel(userName) + "</RivalLevel><UserLevel>" + dbAgent.GetLevel(rivalUserName) + "</UserLevel>\n";
            bAnswer = System.Text.Encoding.ASCII.GetBytes(xmlAnswer);
            rivalSocket.Send(bAnswer);
            Console.WriteLine(dataUser1 + "\n" + dataUser2);

            Thread getinfo1 = new Thread(new ThreadStart(GetInfo1));
            getinfo1.Start();
            Thread getinfo2 = new Thread(new ThreadStart(GetInfo2));
            getinfo2.Start();
            while (getinfo1.IsAlive || getinfo2.IsAlive) { }
            if(!getinfo1.IsAlive && !getinfo2.IsAlive)
            if (isCompete)
                CalculateWinner();

        }
        public void CalculateWinner()
        {
            if (isCompete)
            {
                if (Math.Round(distance1) != Math.Round(distance2))
                {
                    double max = Math.Max(distance1, distance2);
                    double min = Math.Min(distance1, distance2);
                    Console.WriteLine("Max:" + max + "\nMin:" + min);
                    if (max == distance1)
                    {
                        int points = (int)distance1 - (int)distance2;
                        if (points < 20) points = 20;
                        if (dbAgent.AddPoints(userName, points) && dbAgent.AddPoints(rivalUserName, 5))
                        {
                            string xmlAnswer = "<Message><Winner>" + userName + "</Winner><Distance>" + distance1 + "</Distance><LoserDistance>" + distance2 + "</LoserDistance></Message>\n";
                            byte[] bAnswer = System.Text.Encoding.ASCII.GetBytes(xmlAnswer);
                            rivalSocket.Send(bAnswer);
                            userSocket.Send(bAnswer);
                        }
                    }
                    if (max == distance2)
                    {
                        int points = (int)distance2 - (int)distance1;
                        if (points < 20) points = 20;
                        if (dbAgent.AddPoints(rivalUserName, points) && dbAgent.AddPoints(userName, 5))
                        {
                            string xmlAnswer = "<Message><Winner>" + rivalUserName + "</Winner><Distance>" + distance2 + "</Distance><LoserDistance>" + distance1 + "</LoserDistance></Message>\n";
                            byte[] bAnswer = System.Text.Encoding.ASCII.GetBytes(xmlAnswer);
                            rivalSocket.Send(bAnswer);
                            userSocket.Send(bAnswer);
                        }
                    }
                }
                else
                {
                    string xmlAnswer = "<Message><Winner>Tie</Winner></Message>\n";
                    byte[] bAnswer = System.Text.Encoding.ASCII.GetBytes(xmlAnswer);
                    rivalSocket.Send(bAnswer);
                    userSocket.Send(bAnswer);
                }

                ServerClass.AddingSocketToArryList(userSocket);
                ServerClass.AddingSocketToArryList(rivalSocket);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ServerClass.serverIp), 1323);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(remoteEP);
            }
        }
        public void SuddnEnd(Socket s)
        {
            string xmlAnswer = "<Message><Type>TheUserQuit</Type></Message>\n";
            byte[] bAnswer = System.Text.Encoding.ASCII.GetBytes(xmlAnswer);
            s.Send(bAnswer);

            ServerClass.AddingSocketToArryList(userSocket);
            ServerClass.AddingSocketToArryList(rivalSocket);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ServerClass.serverIp), 1323);

            // Create a TCP/IP  socket.
            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remoteEP);
        }

        public void GetInfo1()
        {

            try
            {
                byte[] bytesClientAnswer = new byte[1024];
                int bytesRec = userSocket.Receive(bytesClientAnswer);
                dataUser1 = Encoding.ASCII.GetString(bytesClientAnswer, 0, bytesRec);
            }
            catch (Exception e)
            {
                if (!userSocket.Connected) return;
                if (!rivalSocket.Connected) return;
                Console.WriteLine("There has been a problem in the competition " + rivalUserName + "VS" + userName);
                return;
            }
            if (isCompete)
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(dataUser1)))
                {
                    reader.ReadToFollowing("Message");
                    reader.MoveToFirstAttribute();
                    string type = reader.Value;

                    //////////////////////   state machine   //////////////////////
                    switch (type)
                    {
                        case "CompetitionEnd":
                            string username1;
                            reader.ReadToFollowing("UserName");
                            username1 = reader.ReadElementContentAsString();
                            distance1 = double.Parse(reader.ReadElementContentAsString());
                            break;
                        case "QuitCompetition":
                            reader.ReadToFollowing("UserName");
                            username1 = reader.ReadElementContentAsString();
                            isCompete = false;
                            {
                                if (userName != username1)
                                    SuddnEnd(userSocket);
                                else SuddnEnd(rivalSocket);
                            }
                            return;
                            break;

                        default:
                            break;
                    }
                }
            }
        }
        public void GetInfo2()
        {

            try
            {
                byte[] bytesClientAnswer = new byte[1024];
                int bytesRec = rivalSocket.Receive(bytesClientAnswer);
                dataUser2 = Encoding.ASCII.GetString(bytesClientAnswer, 0, bytesRec);
            }
            catch (Exception e)
            {
                if (!userSocket.Connected) return;
                if (!rivalSocket.Connected) return;
                Console.WriteLine("There has been a problem in the competition " + rivalUserName + "VS" + userName);
                return;
            }
            if (isCompete)
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(dataUser2)))
                {
                    reader.ReadToFollowing("Message");
                    reader.MoveToFirstAttribute();
                    string type = reader.Value;
                    //////////////////////    state machine  //////////////////////
                    switch (type)
                    {
                        case "CompetitionEnd":
                            reader.ReadToFollowing("UserName");
                            userName2 = reader.ReadElementContentAsString();
                            distance2 = double.Parse(reader.ReadElementContentAsString());
                            break;
                        case "QuitCompetition":
                            reader.ReadToFollowing("UserName");
                            userName2 = reader.ReadElementContentAsString();
                            isCompete = false;
                            {
                                if (userName != userName2)
                                    SuddnEnd(userSocket);
                                else SuddnEnd(rivalSocket);
                            }
                            return;
                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }
}
