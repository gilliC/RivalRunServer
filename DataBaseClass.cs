using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Xml;
using System.IO;

namespace ServerTry1
{
    class DataBaseClass
    {
        private RNGCryptoServiceProvider csorng = new RNGCryptoServiceProvider();
        private MD5 hashAgent;
        private StringBuilder sBuilder;
        private SqlCommand cmd;
        private SqlConnection myconn;
        private SqlDataReader dr;
        /// <summary>
        /// All GetX methods, when X = int, return -1 if not exist
        /// </summary>
        public DataBaseClass()
        {
            sBuilder = new StringBuilder();
            hashAgent = MD5.Create();
            myconn = new SqlConnection();
            myconn.ConnectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=C:\Users\User\Desktop\ServerTry1\ServerTry1\DataApp\clientsConnection.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True";
        }

        public string GetUserName(int id)
        {

            string answer = "";
            string question = "SELECT UserName FROM ClientsTable WHERE Id='" + id + "'";

            cmd = new SqlCommand(question, myconn);
            try
            {
                myconn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    answer = dr["UserName"].ToString();
                }
                dr.Close(); myconn.Close();

            }
            catch (Exception e) { if (dr != null) { dr.Close(); myconn.Close(); } }
            return answer;

        }

        public int GetLevel(string userName)
        {
            if (GetId(userName) != -1)
            {

                string answer = "";
                string question = "SELECT Level FROM ClientsTable WHERE UserName='" + userName + "'";

                cmd = new SqlCommand(question, myconn);
                try
                {
                    myconn.Open();
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        answer = dr["Level"].ToString();
                    }
                    dr.Close(); myconn.Close();

                }
                catch (Exception e) { if (dr != null) { dr.Close(); myconn.Close(); } }
                return int.Parse(answer);
            }
            else return -1;

        }

        public int GetPoints(int id)
        {
            if (GetUserName(id) != null)
            {

                string answer = "";
                string question = "SELECT Points FROM ClientsTable WHERE Id=" + id;
                cmd = new SqlCommand(question, myconn);
                try
                {
                    myconn.Open();
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        answer = dr["Points"].ToString();
                    }
                    dr.Close(); myconn.Close();

                }
                catch (Exception e) { if (dr != null) { dr.Close(); myconn.Close(); } }
                return int.Parse(answer);
            }
            else return -1;

        }

        private string GetPassword(int id)
        {
            string answer = "";
            string question = "SELECT Password FROM ClientsTable WHERE Id='" + id + "'";
            cmd = new SqlCommand(question, myconn);
            try
            {
                myconn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    answer = dr["Password"].ToString();
                }
                dr.Close(); myconn.Close();

            }
            catch (Exception e) { if (dr != null) { dr.Close(); myconn.Close(); } }
            return answer;
        }
        private int GetId(string userName)
        {
            string answer = "";
            string question = "SELECT Id FROM ClientsTable WHERE UserName='" + userName + "'";
            cmd = new SqlCommand(question, myconn);
            try
            {
                myconn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    answer = dr["Id"].ToString();
                }
                dr.Close(); myconn.Close();
            }
            catch (Exception e) { if (dr != null) { dr.Close(); myconn.Close(); } }
            if (answer != "")
                return int.Parse(answer);
            else return -1;

        }

        private string GetSalt(int id)
        {
            string answer = "";
            string question = "SELECT Salt FROM ClientsTable WHERE Id='" + id + "'";
            cmd = new SqlCommand(question, myconn);
            try
            {
                myconn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    answer = dr["Salt"].ToString();
                }
                dr.Close(); myconn.Close();

            }
            catch (Exception e) { if (dr != null) { dr.Close(); myconn.Close(); } }
            return answer;
        }

        public string InsertRunner(string username, string name, string password, string birthdate, string country, string email)
        {
            if (checkUserName(username) == false)
            {
                ////////////Handling Password with hash/////////////////////////////////////////////////////
                string salt = "";
                string hashCode = "";
                byte[] saltArr = new byte[10];
                csorng.GetBytes(saltArr);
                string datush = password;
                for (int i = 0; i < saltArr.Length; i++)
                {
                    salt += saltArr[i];
                    datush += saltArr[i];
                }

                sBuilder.Clear();
                byte[] data = hashAgent.ComputeHash(Encoding.UTF8.GetBytes(datush));
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                hashCode = sBuilder.ToString();
                sBuilder.Clear();
                ////////////Handling date/////////////////////////////////////////////////////

                string newBirthDate = "";
                int index = birthdate.IndexOf('/');
                if (index == -1)
                {
                    index = birthdate.IndexOf('.');
                    if (index == -1)
                        if (birthdate.Length == 6)
                            newBirthDate = birthdate.Substring(0, 2) + '/' + birthdate.Substring(2, 2) + "/" + birthdate.Substring(4);
                        else newBirthDate = "12/12/12";
                    else
                        newBirthDate = birthdate.Substring(0, 2) + '/' + birthdate.Substring(3, 2) + '/' + birthdate.Substring(6, 2);
                }

                string sql = "INSERT INTO [ClientsTable] VALUES (";
                sql += "'" + username + "','" + name + "','" + hashCode + "','" + newBirthDate + "', 1 ,0,'" + country + "','" + email + "','" + salt + "',null,null)";
                cmd = new SqlCommand(sql, myconn);
                try
                {
                    myconn.Open();
                    cmd.ExecuteNonQuery();
                    return "Succeeded";
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); if (dr != null) { dr.Close(); myconn.Close(); } return "There has been a problem"; }

            }
            else
            {
                return "The user name is allready exists";
            }
        }
        //^Sign in user.

        protected bool checkUserName(string userName)
        {
            string answer = "";
            string question = "SELECT Name FROM ClientsTable WHERE UserName='" + userName + "'";

            cmd = new SqlCommand(question, myconn);
            try
            {
                myconn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    answer = dr["Name"].ToString();
                }
                dr.Close(); myconn.Close();


            }
            catch (Exception e) { Console.WriteLine(e.Message); if (dr != null) { dr.Close(); myconn.Close(); } }

            if (answer == "")
                return false;
            return true;
        }
        //^ if user name exists return true, else false^

        public bool CheckUser(string userName, string password)
        {

            if (checkUserName(userName))
            {
                sBuilder.Clear();
                string salt = GetSalt(GetId(userName));
                string hashCodePassword = "";
                string datush = password + salt;
                byte[] data = hashAgent.ComputeHash(Encoding.UTF8.GetBytes(datush));

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                hashCodePassword = sBuilder.ToString();

                if (GetPassword(GetId(userName)) == hashCodePassword)
                    return true;
            }
            sBuilder.Clear();
            return false;
        }
        //^if the password and the user name is correct and exists return true, else false^

        public bool DeleteUser(int userId)
        {
            if (GetUserName(userId) != null)
            {
                string sql = "DELETE FROM ClientsTable  WHERE Id =" + userId;
                cmd = new SqlCommand(sql, myconn);
                try
                {
                    myconn.Open();
                    cmd.ExecuteReader();
                    myconn.Close();
                }
                catch { if (dr != null) { dr.Close(); myconn.Close(); } return false; }
                return true;
            }
            else return false;

        }

        /// ////////////////////////////////Points & Level/////////////////////////////////////////////////////

        public bool LevelUp(int id)
        {
            if (GetUserName(id) != null)
            {
                int newLevel = GetLevel(GetUserName(id)) + 1;
                string sql = "UPDATE ClientsTable SET Level =" + newLevel + " WHERE Id =" + id;
                cmd = new SqlCommand(sql, myconn);
                try
                {
                    myconn.Open();
                    cmd.ExecuteReader();
                    myconn.Close();
                }
                catch { if (dr != null) { dr.Close(); myconn.Close(); } return false; }
                sql = "Update ClientsTable SET Points =0 WHERE Id =" + id;
                cmd = new SqlCommand(sql, myconn);
                try
                {
                    myconn.Open();
                    cmd.ExecuteReader();
                    myconn.Close();
                    return true;

                }
                catch
                { if (dr != null) { dr.Close(); myconn.Close(); } return false; }
            }
            return false;

        }
        //^level up, return true if succeded. else false^
        public bool AddPoints(string userName, int pointsAdd)
        {
            int id = GetId(userName);
            if (id != -1)
            {
                int newPoints = GetPoints(id) + pointsAdd;
                string sql = "UPDATE ClientsTable SET Points =" + newPoints + " WHERE Id =" + id;
                cmd = new SqlCommand(sql, myconn);
                try
                {
                    myconn.Open();
                    cmd.ExecuteReader();
                    myconn.Close();
                }
                catch { if (dr != null) { dr.Close(); myconn.Close(); } return false; }
                return true;
            }
            else return false;
        }
        ////////////////////////////////SETTINGS////////////////////////////////////////////////////////////
        public string GetUserData(string userName)
        {
            int id = GetId(userName);
            if (id != -1)
            {
                string answer = "";
                string question = "SELECT UserName,Name,BirthDate,Level,Points,Country FROM ClientsTable WHERE Id='" + id + "'";
                cmd = new SqlCommand(question, myconn);
                try
                {
                    myconn.Open();
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        answer += dr["UserName"].ToString() + ",";
                        answer += dr["Name"].ToString() + ",";
                        answer += dr["Password"].ToString() + ",";
                        answer += dr["Level"].ToString() + ",";
                        answer += dr["Points"].ToString() + ",";
                        answer += dr["Country"].ToString();
                    }
                    dr.Close(); myconn.Close();

                }
                catch (Exception e) { if (dr != null) { dr.Close(); myconn.Close(); return "Unknown problem"; } }
                return answer;
            }
            else return "The user does not exist";
        }

        public string ChangeUserName(string userNameOld, string password, string userNameNew)
        {
            int id = (GetId(userNameOld));
            if (id != -1)
            {
                if (CheckUser(userNameOld, password))
                
                {
                    if (checkUserName(userNameNew) == false)
                    {
                        string sql = "UPDATE ClientsTable SET UserName =" + userNameNew + " WHERE Id =" + id;
                        cmd = new SqlCommand(sql, myconn);
                        try
                        {
                            myconn.Open();
                            cmd.ExecuteReader();
                            myconn.Close();
                        }
                        catch { if (dr != null) { dr.Close(); myconn.Close(); } return "Unknown problem"; }
                        return "Succeeded";
                    }
                    else return "The user name is already used";
                }
                else return "Incorrect user name or password";
            }
            else
                return "Incorrect user name or password";
        }
        public string ChangePassword(string userName, string oldPassword, string newPassword)
        {
            int id = (GetId(userName));
            if (id != -1)
            {
                if (CheckUser(userName, oldPassword))
                {
                    string sql = "UPDATE ClientsTable SET Password =" + newPassword + " WHERE Id =" + id;
                    cmd = new SqlCommand(sql, myconn);
                    try
                    {
                        myconn.Open();
                        cmd.ExecuteReader();
                        myconn.Close();
                    }
                    catch { if (dr != null) { dr.Close(); myconn.Close(); } return "Unknown problem"; }
                    return "Succeeded";
                }
                return "Incorrect user name or password";
            }
            else
                return "Incorrect user name or password";
        }
        public string ChangeName(string userName, string password, string newName)
        {
            int id = (GetId(userName));
            if (id != -1)
            {
                if (CheckUser(userName, password))
                {
                    string sql = "UPDATE ClientsTable SET Name =" + newName + " WHERE Id =" + id;
                    cmd = new SqlCommand(sql, myconn);
                    try
                    {
                        myconn.Open();
                        cmd.ExecuteReader();
                        myconn.Close();
                    }
                    catch { if (dr != null) { dr.Close(); myconn.Close(); } return "Unknown problem"; }
                    return "Succeeded";
                }
                return "Incorrect user name or password";
            }
            else
                return "Incorrect user name or password";
        }
        public string ChangeBirthDate(string userName, string password, string newBirthDate)
        {
            int id = (GetId(userName));
            if (id != -1)
            {
                if (CheckUser(userName, password))
                {
                    string birthDate = "";
                    int index = newBirthDate.IndexOf('/');
                    if (index == -1)
                    {
                        index = newBirthDate.IndexOf('.');
                        if (index == -1)
                            newBirthDate = newBirthDate.Substring(0, 2) + '/' + newBirthDate.Substring(2, 2) + '/' + newBirthDate.Substring(4, 2);
                        else
                            newBirthDate = newBirthDate.Substring(0, 2) + '/' + newBirthDate.Substring(3, 2) + '/' + newBirthDate.Substring(6, 2);
                    }
                    string sql = "UPDATE ClientsTable SET BirthDate =" + birthDate + " WHERE Id =" + id;
                    cmd = new SqlCommand(sql, myconn);
                    try
                    {
                        myconn.Open();
                        cmd.ExecuteReader();
                        myconn.Close();
                    }
                    catch { if (dr != null) { dr.Close(); myconn.Close(); } return "Unknown problem"; }
                    return "Succeeded";
                }
                return "Incorrect user name or password";
            }
            else
                return "Incorrect user name or password";
        }
        public string ChangeEmail(string userName, string password, string newEmail)
        {
            int id = (GetId(userName));
            if (id != -1)
            {
                if (CheckUser(userName, password))
                {
                    string sql = "UPDATE ClientsTable SET Email =" + newEmail + " WHERE Id =" + id;
                    cmd = new SqlCommand(sql, myconn);
                    try
                    {
                        myconn.Open();
                        cmd.ExecuteReader();
                        myconn.Close();
                    }
                    catch { if (dr != null) { dr.Close(); myconn.Close(); } return "Unknown problem"; }
                    return "Succeeded";
                }
                return "Incorrect user name or password";
            }
            else
                return "Incorrect user name or password";
        }
        public string ChangeCountry(string userName, string password, string newCountry)
        {
            int id = (GetId(userName));
            if (id != -1)
            {
                if (CheckUser(userName, password))
                {
                    string sql = "UPDATE ClientsTable SET Country =" + newCountry + " WHERE Id =" + id;
                    cmd = new SqlCommand(sql, myconn);
                    try
                    {
                        myconn.Open();
                        cmd.ExecuteReader();
                        myconn.Close();
                    }
                    catch { if (dr != null) { dr.Close(); myconn.Close(); } return "Unknown problem"; }
                    return "Succeeded";
                }
                return "Incorrect user name or password";
            }
            else
                return "Incorrect user name or password";
        }


        /// ////////////////////////////////FRIENDS/////////////////////////////////////////////////////
        public string GetFriends(string userName)
        {
            if (GetId(userName) != -1)
            {
                string answer = "";
                string question = "SELECT Friends FROM ClientsTable WHERE UserName='" + userName + "'";
                cmd = new SqlCommand(question, myconn);
                try
                {
                    myconn.Open();
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        answer = dr["Friends"].ToString();
                    }
                    dr.Close(); myconn.Close();

                }
                catch (Exception e) { if (dr != null) { dr.Close(); myconn.Close(); } }
                return answer;
            }
            else return "The user does not exists";

        }
        public string GetFriendsRequested(string userName)
        {
            string answer = "";
            string question = "SELECT FriendsRequest FROM ClientsTable WHERE UserName='" + userName + "'";
            cmd = new SqlCommand(question, myconn);
            try
            {
                myconn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    answer = dr["FriendsRequest"].ToString();
                }
                dr.Close(); myconn.Close();

            }
            catch (Exception e) { if (dr != null) { dr.Close(); myconn.Close(); } }
            return answer;

        }

        public string[] GetFriendRequestedList(string userName)
        {
            if (GetId(userName) != -1)
            {
                string[] answer = null;
                string answer1 = null;
                string[] friendsListChecked;
                string question = "SELECT FriendsRequest FROM ClientsTable WHERE UserName='" + userName + "'";
                cmd = new SqlCommand(question, myconn);
                try
                {
                    myconn.Open();
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        answer1 = dr["FriendsRequest"].ToString();
                    }
                    dr.Close(); myconn.Close();
                    answer = answer1.Split(',');
                    friendsListChecked = new string[answer.Length];
                    int j = 0;
                    for (int i = 0; i < answer.Length; i++)
                        if (answer[i] != "")
                            if (GetUserName(int.Parse(answer[i])) != null)
                            {
                                friendsListChecked[j] = answer[i];
                                j++;
                            }

                }
                catch (Exception e) { if (dr != null) { dr.Close(); myconn.Close(); } }
                return answer;
            }
            else return null;
        }
        public int[] GetFriendsList(string userName)
        {
            string[] answer = null;
            string answer1 = GetFriends(userName);
            int[] friendsListChecked = null;
            if (answer1 != "")
            {
                answer = answer1.Split(',');
                friendsListChecked = new int[answer.Length];
                int j = 0;
                for (int i = 0; i < answer.Length; i++)
                    if (answer[i] != "")
                        if (GetUserName(int.Parse(answer[i])) != null)
                        {
                            friendsListChecked[j] = int.Parse(answer[i]);
                            j++;
                        }

            }
            else
            {
                friendsListChecked = new int[1];
                friendsListChecked[0] = -1;
            }
            return friendsListChecked;

        }

        public string GetFriendsUserNameList(string userName)
        {
            string[] answer = null;
            string answer1 = null;
            string friendsListChecked = "";
            string question = "SELECT Friends FROM ClientsTable WHERE UserName='" + userName + "'";
            cmd = new SqlCommand(question, myconn);
            try
            {
                myconn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    answer1 = dr["Friends"].ToString();
                }
                dr.Close(); myconn.Close();
                answer = answer1.Split(',');
                int j = 0;
                for (int i = 0; i < answer.Length; i++)
                    if (answer[i] != "")
                        if (GetUserName(int.Parse(answer[i])) != null)
                        {
                            if (i != 0)
                                friendsListChecked += "," + GetUserName(int.Parse(answer[i]));
                            else friendsListChecked = GetUserName(int.Parse(answer[i]));
                        }

            }
            catch (Exception e)
            {
                if (dr != null) { dr.Close(); myconn.Close(); }
                friendsListChecked = "there has been a problem";
            }
            if (friendsListChecked == null || friendsListChecked == "")
                friendsListChecked = "No Friends Yet";
            return friendsListChecked;
        }
        public string GetFriendsRquestsUserNameList(string userName)
        {
            string[] answer = null;
            string answer1 = null;
            string friendsListChecked = "";
            string question = "SELECT FriendsRequest FROM ClientsTable WHERE UserName='" + userName + "'";
            cmd = new SqlCommand(question, myconn);
            answer1 = GetFriendsRequested(userName);
            answer = answer1.Split(',');
            int j = 0;
            for (int i = 0; i < answer.Length; i++)
                if (answer[i] != "")
                    if (GetUserName(int.Parse(answer[i])) != null)
                    {
                        if (friendsListChecked == "")
                            friendsListChecked = GetUserName(int.Parse(answer[i]));
                        else friendsListChecked += "," + GetUserName(int.Parse(answer[i]));
                    }
            if (friendsListChecked == null || friendsListChecked == "")
                friendsListChecked = "No Friends Requests";
            return friendsListChecked;
        }


//A friend update action - Updating the member list to the user 1.
        public string UpdateAFriend(string userName, string friendUserName)
        {
            string friends;
            if (IsFriends(userName, friendUserName))
                return "Already Friends";
            if (GetId(friendUserName) == -1)
                return "User Does not exicte";
            int[] check = GetFriendsList(userName);
            if (check[0] != -1)
            {
                friends = GetFriends(userName);
                friends += "," + GetId(friendUserName);

            }
            else friends = GetId(friendUserName).ToString();
            string sql = "UPDATE ClientsTable SET Friends ='" + friends + "' WHERE Id =" + GetId(userName);
            cmd = new SqlCommand(sql, myconn);
            try
            {
                myconn.Open();
                cmd.ExecuteReader();
                myconn.Close();
            }
            catch { if (dr != null) { dr.Close(); myconn.Close(); } return "Unexpected Problem"; }
            return "Succeded";

        }
        //Updating friends for user 2
        public string UpdateFriends(string user1, string user2)
        {
            int id1 = GetId(user1);
            int id2 = GetId(user2);
            if (id1 == -1)
                return user1 + ",Does not exists";
            if (id2 == -1)
                return user2 + ",Does not exists";
            string answer1 = UpdateAFriend(user1, user2);
            string answer2 = UpdateAFriend(user2, user1);
            if (answer1 == "Succeded" && answer2 == "Succeded")
                return answer1;
            else
            {
                DeleteFriend(user1, user2);
                DeleteFriend(user2, user1);
                return user1 + "," + answer1 + "." + user2 + "," + answer2;
            }
        }
        public string RequestFriend(string user, string friendUserName)
        {
            int id = GetId(friendUserName);
            if (id != -1)
            {
                if (user != friendUserName)
                {
                    if (!(IsFriends(user, friendUserName)))
                    {
                        string newFriendsList = GetFriendsRequested(friendUserName);
                        if (newFriendsList != "")
                            newFriendsList += "," + GetId(user);
                        else newFriendsList = GetId(user).ToString();
                        string sql = "UPDATE ClientsTable SET FriendsRequest ='" + newFriendsList + "' WHERE Id =" + GetId(friendUserName);
                        cmd = new SqlCommand(sql, myconn);
                        try
                        {
                            myconn.Open();
                            cmd.ExecuteReader();
                            myconn.Close();
                        }
                        catch { if (dr != null) { dr.Close(); myconn.Close(); } return "Unexpected Problem"; }
                        return "Succeded";
                    }
                    else return "Already Friends";

                }
                else return "You can't be friend with yourself";
            }
            else return "User Does not exists";
        }

        public bool IsFriends(string user1, string user2)
        {
            int idFriend = GetId(user2);
            if (idFriend != -1 && GetId(user1) != -1)
            {
                int[] check = GetFriendsList(user1);
                for (int i = 0; i < check.Length; i++)
                    if (check[i] == idFriend)
                        return true;
            }
            return false;
        }

        //delete one friend from the userList
        public bool DeleteFriend(string userName, string userFriend)
        {
            int idFriend = GetId(userFriend);
            if (idFriend != -1 && GetId(userName) != -1)
            {
                int[] friends = GetFriendsList(userName);
                string newFriendsList = "";
                for (int i = 0; i < friends.Length; i++)
                    if (friends[i] != idFriend)
                        if (i + 1 != friends.Length)
                            newFriendsList += friends[i] + ",";
                        else newFriendsList += friends[i];
                string sql = "UPDATE ClientsTable SET Friends ='" + newFriendsList + "' WHERE Id =" + GetId(userName);
                cmd = new SqlCommand(sql, myconn);
                try
                {
                    myconn.Open();
                    cmd.ExecuteReader();
                    myconn.Close();
                }
                catch { if (dr != null) { dr.Close(); myconn.Close(); } return false; }
                return true;
            }
            return false;

        }
        public bool DeleteFriendRequest(string userName, string friendUserName)
        {
            int idFriend = GetId(friendUserName);
            if (idFriend != -1)
            {
                string friends = GetFriendsRequested(userName);
                string[] friendsRequestList = friends.Split(',');
                string newFriendsList = "";
                for (int i = 0; i < friendsRequestList.Length; i++)
                    if (int.Parse(friendsRequestList[i]) != idFriend)
                        if (newFriendsList == "")
                            newFriendsList += friendsRequestList[i];
                        else newFriendsList += "," + friendsRequestList[i];

                string sql = "UPDATE ClientsTable SET FriendsRequest ='" + newFriendsList + "' WHERE Id =" + GetId(userName);
                cmd = new SqlCommand(sql, myconn);
                try
                {
                    myconn.Open();
                    cmd.ExecuteReader();
                    myconn.Close();
                }
                catch { if (dr != null) { dr.Close(); myconn.Close(); } return false; }
                return true;
            }
            else return false;

        }
        public string DeleteFriendship(string userName, string userFriend)
        {
            if (GetId(userFriend) != -1)
                if (GetId(userName) != -1)
                {
                    if (DeleteFriend(userFriend, userName))
                        if (DeleteFriend(userName, userFriend))
                            return "Deleted";
                        else return "There has been a problem";
                }
                else return userName + ",Does not exists";
            else return userFriend + ",Does not exists";
            return "There has been a problem";

        }

        //useraccept: the one who accepted the request
        public string AcceptFriend(string userAccept, string user2)
        {
            if (GetId(userAccept) != -1)
                if (GetId(user2) != -1)
                {
                    string answer = UpdateFriends(userAccept, user2);
                    if (answer == "Succeded")
                        if (DeleteFriendRequest(userAccept, user2))
                            return "Accept";
                        else return "There has been a problem";
                }
                else return user2 + ",Does not exists";
            else return userAccept + ",Does not exists";
            return "There has been a problem";

        }
    }
}
