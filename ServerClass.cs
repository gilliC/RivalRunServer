using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace ServerTry1
{
    class ServerClass
    {

        static ArrayList socketReadList;
        static ArrayList syncSocketReadL;
        static ArrayList socketReadCopyList;
        DataBaseClass dbAgent;
        FindingRivalHandle findRivalHAgent;
        IPEndPoint localEndPoint;
        Socket listener;
        byte[] bAnswer;
        string ipstr;
     //   public const string serverIp = "192.168.1.206";
       public const string serverIp = "10.0.0.31";
        public ServerClass(FindingRivalHandle frh)
        {
            socketReadList = new ArrayList();
            dbAgent = new DataBaseClass();
            findRivalHAgent = frh;
            localEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), 1323);

            // Create a TCP/IP socket.
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(10);
            socketReadList.Add(listener);
            //creating thread safe arrayList
            syncSocketReadL = ArrayList.Synchronized(socketReadList);
            socketReadCopyList = new ArrayList(socketReadList);
        }
        public void SelectLoop()
        {
            string answer = "null";
            string xmlAnswer;
            byte[] bytes = new byte[1024];
            Console.WriteLine("The server is open to visitors...");
            while (true)
            {
                Socket.Select(socketReadCopyList, null, null, -1);
                for (int i = 0; i < socketReadCopyList.Count; i++)
                {
                    xmlAnswer = "<Message type = 'Answer'>";
                    Socket socketTemp = (Socket)socketReadCopyList[i];
                    if (socketTemp == listener)
                    {
                        Socket s = listener.Accept();
                        ipstr = s.RemoteEndPoint.ToString();
                        string ip = ipstr.Split(':')[0];
                        if (ip != serverIp)
                        {
                            syncSocketReadL.Add(s);
                            Console.WriteLine("Welcome " + s.RemoteEndPoint.ToString() + " darling");
                        }

                        continue;
                    }
                    int bytesRec = socketTemp.Receive(bytes);
                    string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    if (data != "")
                    {
                        Console.WriteLine(data);
                        try
                        {
                            using (XmlReader reader = XmlReader.Create(new StringReader(data)))
                            {
                                reader.ReadToFollowing("Message");
                                reader.MoveToFirstAttribute();
                                string type = reader.Value;

                                //////////////////////   state machine   //////////////////////

                                switch (type)
                                {
                                    case "Registry":

                                        string username, password, name, country, email, birthdate;
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        name = reader.ReadElementContentAsString();
                                        password = reader.ReadElementContentAsString();
                                        country = reader.ReadElementContentAsString();
                                        birthdate = reader.ReadElementContentAsString();


                                        answer = dbAgent.InsertRunner(username, password, name, birthdate, country, "null");
                                         
                                        break;

                                    case "SignIn":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        password = reader.ReadElementContentAsString();
                                        if (dbAgent.CheckUser(username, password) == true)
                                            answer = "Succeeded";
                                        else answer = "Bad Data";
                                        break;

                                    case "Competition":
                                        string userName, competitionType;
                                        reader.ReadToFollowing("Type");
                                        competitionType = reader.ReadElementContentAsString();
                                        username = reader.ReadElementContentAsString();
                                        RemoveSocket(socketTemp);
                                        findRivalHAgent.AddToRivalList(socketTemp, username, competitionType, this);
                                        answer = "searching for rival";
                                        break;

                                    case "FriendsList":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        answer = "<Type>FriendsList</Type><Answer>" + dbAgent.GetFriendsUserNameList(username) + "</Answer>";
                                        break;

                                    case "RequestFriendsList":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        answer = "<Type>RequestFriendsList</Type><Answer>" + dbAgent.GetFriendsRquestsUserNameList(username) + "</Answer>";
                                        break;

                                    case "AddFriend":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        string friendUserName = reader.ReadElementContentAsString();
                                        answer = dbAgent.RequestFriend(username, friendUserName);
                                        break;

                                    case "AcceptFriend":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        friendUserName = reader.ReadElementContentAsString();
                                        answer = dbAgent.AcceptFriend(username, friendUserName);
                                        break;

                                    case "UnAcceptFriend":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        friendUserName = reader.ReadElementContentAsString();
                                        if (dbAgent.DeleteFriendRequest(username, friendUserName))
                                            answer = "UnAcceptFriend";
                                        else answer = "There has been a problem";
                                        break;

                                    case "DeleteFriend":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        friendUserName = reader.ReadElementContentAsString();
                                        answer = dbAgent.DeleteFriendship(username, friendUserName);
                                        if (answer == "Deleted")
                                            answer = "UnAcceptFriend";
                                        break;

                                    case "GetUserData":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        answer = dbAgent.GetUserData(username);
                                        break;

                                    case "ChangePassword":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        password = reader.ReadElementContentAsString();
                                        string newpassword = reader.ReadElementContentAsString();
                                        answer = dbAgent.ChangePassword(username, password, newpassword);
                                        break;

                                    case "ChangeName":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        password = reader.ReadElementContentAsString();
                                        string newname = reader.ReadElementContentAsString();
                                        answer = dbAgent.ChangeName(username, password, newname);
                                        break;

                                    case "ChangeUserName":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        password = reader.ReadElementContentAsString();
                                        string newUserName = reader.ReadElementContentAsString();
                                        answer = dbAgent.ChangeUserName(username, password, newUserName);
                                        break;

                                    case "ChangeCountry":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        password = reader.ReadElementContentAsString();
                                        string newCountry = reader.ReadElementContentAsString();
                                        answer = dbAgent.ChangeCountry(username, password, newCountry);
                                        break;

                                    case "ChangeBirthDate":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        password = reader.ReadElementContentAsString();
                                        string newbirthdate = reader.ReadElementContentAsString();
                                        answer = dbAgent.ChangeBirthDate(username, password, newbirthdate);
                                        break;

                                    case "ChangeEmail":
                                        reader.ReadToFollowing("UserName");
                                        username = reader.ReadElementContentAsString();
                                        password = reader.ReadElementContentAsString();
                                        string newEmail = reader.ReadElementContentAsString();
                                        answer = dbAgent.ChangeEmail(username, password, newEmail);
                                        break;

                                    default:
                                        break;

                                }


                            }
                        }
                        catch (Exception e)
                        {
                            answer = "Problem with the msg";
                            Console.WriteLine(answer + ":" + socketTemp.RemoteEndPoint.ToString() + "  " + e.ToString());
                        }

                        ///////////Sending an answer:////////////////////////////////
                        xmlAnswer = xmlAnswer + answer + "</Message>\n";
                        bAnswer = System.Text.Encoding.ASCII.GetBytes(xmlAnswer);
                        socketTemp.Send(bAnswer);

                        Console.WriteLine(" answer to " + socketTemp.RemoteEndPoint.ToString() + ":>> " + answer);
                    }
                    else
                    {
                        if ((socketTemp.Poll(1000, SelectMode.SelectRead) && (socketTemp.Available == 0)) || !socketTemp.Connected)
                        {
                            Console.WriteLine("Bye bye:" + socketTemp.RemoteEndPoint.ToString());

                            syncSocketReadL.Remove(socketTemp);
                            socketTemp.Close();
                            continue;
                        }

                        Console.WriteLine("NO MESSEAGE FROM " + socketTemp.RemoteEndPoint.ToString());
                    }

                }
                socketReadCopyList = new ArrayList(syncSocketReadL);

            }
        }

        //methods for the competition thread
        public static void AddingSocketToArryList(Socket s)
        {
            socketReadList.Add(s);
            Console.WriteLine(s.RemoteEndPoint.ToString() + " Added");

        }
        public static void RemoveSocket(Socket s)
        {
            Console.WriteLine(s.RemoteEndPoint.ToString() + " Removed");
            socketReadList.Remove(s);
        }
    }
}
