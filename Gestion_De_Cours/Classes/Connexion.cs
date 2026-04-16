
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.IO;
using DevExpress.XtraEditors;
namespace Gestion_De_Cours.Classes
{
    public class Connexion
    {
        public static string DB = "master";
        public static string DS = "PC-HOME\\SQLDEMO";
        public static string UserName = "";
        public static string password = "";
        public static string WorkerID = "";
        public static SqlConnection connecting ;
        public static void SetDB(string DBNew)
        {
            DB = DBNew;
        }

        public static void CreateBackup(string name)
        {
            Insert(@"BACKUP DATABASE "+ DB + @" TO DISK = 'C:\ProgramData\EcoleSetting\DataBases\" + DB + @"OLD.bak' WITH COPY_ONLY, FORMAT, INIT; 
             RESTORE DATABASE " + name + @" FROM DISK = 'C:\ProgramData\EcoleSetting\DataBases\" + DB + @"OLD.bak' WITH MOVE 'DBBackup' TO 'C:\ProgramData\EcoleSetting\DataBases\"+ name + @"NEW.bak', MOVE 'DBBackup_log' TO 'C:\ProgramData\EcoleSetting\DataBases\"+ name + @"NEW.ldf', REPLACE; ALTER DATABASE " + name + " SET MULTI_USER; ");
        }

        public static void CreateBackUpNoRestore(string name)
        {
            Insert(@"BACKUP DATABASE " + DB + @" TO DISK = 'C:\ProgramData\EcoleSetting\DataBases\" + name + @"OLD.bak' WITH COPY_ONLY, FORMAT, INIT; ");
        }

        public static string GetLastDateGroup(string GID)
        {
            return GetString($@"
                    SELECT ISNULL(
                        CONVERT(varchar(10),
                            (SELECT MAX(CONVERT(date, Date, 105))
                             FROM Attendance
                             WHERE GroupID = {GID}), 105
                        ),
                        '0'
                    ) AS LastDate
                ");

        }
        public static int GetPastAbsence(string AID , string SID)// returns  -1 for the student was never present in this group to remove him/her 
                                                                 // returns 0 for not the students's 3rd absence
                                                                 //returns id for last present 
        {
            int absentcount = 1;
            int presentfoundID = 0;
            int Sessionforattendance = Connexion.GetInt("Select Session from Attendance Where ID = " + AID) - 1;
            int GID = Connexion.GetInt("Select GroupID from Attendance Where ID = " + AID);
            while (true)
            {
                if (Sessionforattendance == 0)
                {
                    break;
                }
                int IDPastSessions = Connexion.GetInt("Select ID from Attendance Where GroupID  = " + GID + " and Session =  " + Sessionforattendance);
                if (Connexion.IFNULL("Select Status from Attendance_Student  Where ID =" + IDPastSessions + " and StudentID = " + SID))
                {
                    break;
                }
                int oldStatus = Connexion.GetInt("Select case when  Status is null then 0 else status end as f  from Attendance_Student  Where ID =" + IDPastSessions + " and StudentID = " + SID);
                if (oldStatus == 0 && absentcount != 3 )
                {
                    absentcount++;
                    
                }
                if(absentcount  == 3)
                {
                    if(oldStatus == 1)
                    {
                        return IDPastSessions; 
                    }
                    if (Sessionforattendance == 1)
                    {
                        presentfoundID = -1;
                        break;
                    }
                }
                Sessionforattendance--;
            }
            if(absentcount == 3)
            {
                return -1;
            }
            return presentfoundID;
        }
        public static  string GetTotalStuOutof(string AID)
        {
            int TotalStudents = GetInt("Select Count(*) from Attendance_Student where ID =  " + AID);
            int extrastudents = GetInt("Select Count(*) from Attendance_StudentsOneSes where AID =" + AID);

            int  presentStudents = GetInt("Select Count(*) from Attendance_Student where Status = 1 and ID =  " + AID);
            TotalStudents += extrastudents;
            presentStudents += extrastudents;
            return presentStudents + "/" + TotalStudents;
        }
        public static void RestoreDataBase(string name)
        {

            Insert(@"RESTORE DATABASE " + name + @" FROM DISK = 'C:\ProgramData\EcoleSetting\DataBases\" + name + @"OLD.bak' WITH MOVE 'DBBackup' TO 'C:\ProgramData\EcoleSetting\DataBases\" + name + @"NEW.bak', MOVE 'DBBackup_log' TO 'C:\ProgramData\EcoleSetting\DataBases\" + name + @"NEW.ldf', REPLACE; ALTER DATABASE " + name + " SET MULTI_USER;");
        }

        public static void RestoreMainDataBase(string name,SqlConnection Connect )
        {
            string filepath = Directory.GetCurrentDirectory() + @"\Patches\DataBaseEmpty.bak";
            string text = @"RESTORE DATABASE " + name + @" FROM DISK = '"+ filepath + @"' WITH MOVE 'DBBackup' TO 'C:\ProgramData\EcoleSetting\DataBases\" + name + @"NEW.bak', MOVE 'DBBackup_log' TO 'C:\ProgramData\EcoleSetting\DataBases\" + name + @"NEW.ldf', REPLACE; ALTER DATABASE " + name + " SET MULTI_USER;";
         
            SqlCommand Command = new SqlCommand(text, Connect);
            Command.ExecuteNonQuery();
        }
        public static int GetClassID(string GID)
        {
            SqlConnection con = Connect();
            SqlCommand CommandID = new SqlCommand("Select ClassID from Groups Where GroupID = " + GID, con);
            CommandID.ExecuteNonQuery();
            Int32 result = Convert.ToInt32(CommandID.ExecuteScalar());
            con.Close();
            return result; 

        }
        public static int GetGroupID(string SID , string CID)
        {
            SqlConnection con = Connect();
            SqlCommand CommandID = new SqlCommand($"SELECT TOP 1 GroupID FROM Class_Student WHERE ClassID = {CID} AND StudentID = {SID} ORDER BY CASE WHEN Stopped = '0' THEN 0 ELSE 1 END", con);
            CommandID.ExecuteNonQuery();
            Int32 result = Convert.ToInt32(CommandID.ExecuteScalar());
            con.Close();
            return result;
        }

        public static int GetTeacherID(string CID)
        {
            SqlConnection con = Connect();
            SqlCommand CommandID = new SqlCommand("Select TID from Class Where  ID = " + CID, con);
            CommandID.ExecuteNonQuery();
            Int32 result = Convert.ToInt32(CommandID.ExecuteScalar());
            con.Close();
            return result;
        }

        public static void InsertHistory(int type , string id1 , int type2)
        {
            Insert("Insert into " +
                "History(Date,Type,WorkerID,ID1,Type2) " +
                "values(N'" + DateTime.Now.ToString("dd / MM / yyyy HH: mm:ss") + "',N'"
                + type + "','"
                + WorkerID + "','"
                + id1 + "','"
                + type2 + "')");
        }
        public static int CalculatePrice(string SID , string GID , int SessionsComboBox, string type)
        {
            int SesGroupGotPayed = GetInt(GID, "Groups", "Sessions", "GroupID") - GetInt(GID, "Groups", "TSessions", "GroupID");
            int SesShouldbePayed = SesGroupGotPayed; 
            SesShouldbePayed += SessionsComboBox;
            SesShouldbePayed -= CalculateSesPayedT(SID, GID);
            //SesShould be Payed is what the teacher Got paid + what it want to be paid 
            //- what the student already paid 
            //Example : teacher got paid 4 sessions + wanna be paid 4 = 8 
            // but the student paid only 2 sessions so it will be 6 sessions paid
            //and if the student already paid full price it will be the normal price of 4
            int SesPayedStudent = CalculateSesPayedS(SID, GetClassID(GID).ToString());
            SesPayedStudent -= CalculateSesPayedT(SID, GID);

            //Payed is the total student Payment - What he already paid 
            DataTable dtDiscounts = new DataTable();
            string stringforqueryses = "";
            string stringforPrice = "";
            string stringforDiscount = "";
            int Price = 0;
            if (type == "S" || type == "Su")
            {
                stringforPrice = "CPrice";
                stringforqueryses = "StudentSes";
                stringforDiscount = "CPrice";
            }
            else if (type == "T")
            {
                stringforPrice = "TPayment";
                stringforqueryses = "TeacherSes";
                stringforDiscount = "TPrice";
            }
            Price = Connexion.GetInt(GetClassID(GID).ToString(), "Class", stringforPrice) / 4;
            Connexion.FillDT(ref dtDiscounts,
                "Select * from Discounts " +
                "Where StudentID = " + SID + "" +
                "AND ClassID = " + GetClassID(GID).ToString()  +
                "and Done IS  Null " +
                "Or Done > " + SesGroupGotPayed + " " +
                "ORDER BY " + stringforqueryses + " asc ");
            int count = Math.Min(SesPayedStudent, SesShouldbePayed);
            if(type == "Su")
            {
                count = SesShouldbePayed; 
            }
            int SPayed =  0 ;
            for (int f = 0; f < dtDiscounts.Rows.Count; f++)
            {
                if(dtDiscounts.Rows[f]["Done"] == DBNull.Value)
                {
                    int countforthis = int.Parse(dtDiscounts.Rows[f][stringforqueryses].ToString());
                    if (countforthis >= SesGroupGotPayed && countforthis < SesGroupGotPayed + SessionsComboBox)
                    {
                        // first condition is checking if the discount started after last teacher payment 
                        //Second condition is checking if the count is after this payment ex : we are paying the teacher in 0-4 session and the discounts start in 6th
                        Price = int.Parse(dtDiscounts.Rows[f][stringforDiscount].ToString()) ;
                        Price /= 4;
                        if(count <= (SessionsComboBox + SesGroupGotPayed - countforthis))
                        {
                            return SPayed += Price * count;
                        }
                        else
                        {
                            SPayed += Price * (SessionsComboBox + SesGroupGotPayed - countforthis);
                            count -= (SessionsComboBox + SesGroupGotPayed - countforthis);
                        }
                    }
                    else
                    {
                        SPayed += Price * count;
                        return SPayed;
                    }
                }
                else
                {
                    int done = int.Parse(dtDiscounts.Rows[f]["Done"].ToString());
                    done -= SesGroupGotPayed;
                    if(count - done > 0)
                    {
                        SPayed += Price * done;
                        count -= done; 
                    }
                    else
                    {
                        SPayed += Price * count;
                        return SPayed;
                    }
                }
            }
            SPayed += (Connexion.GetInt(GetClassID(GID).ToString(), "Class", stringforPrice) / 4) * count;
            return SPayed;

        }
        public static int CalculateSesPayedS(string SID , string GID)
        {
            return Connexion.GetInt("Select DBO.CalculateSesPayed(" + SID + " , " + GID + ")");
        }
        public static int CalculateSesPayedT(string SID, string GID)
        {
            return Connexion.GetInt("Select DBO.CalculateSesPayedT(" + SID + " , " + GID + ")");
        }
        public static int CalculateSesPayedTEST(string SID, string CID, string Type)
        {
            //Calculating the Sessions payed 
            // if type is T then calculate the sessions that the teacher got payed by that
            //student 
            //if type is S then Calculate the sessions that student payed  
            string row ="" ; 
            string row2  ="";
            int TotalPay = 0;
            if (Type == "T")
            {
                string GID = CID;
                CID = GetClassID(GID).ToString();
                row = "TPrice";
                row2 = "TeacherSes";
                TotalPay = GettotalPayTeacher(SID, GID , "TPrice");
            }
            else if (Type == "S")
            {
                row = "CPrice";
                row2 = "StudentSes";
                TotalPay = GettotalPayStudent(SID, CID);
            }
            DataTable dtDiscounts = new DataTable();
            FillDT(ref dtDiscounts, "Select " +
                "StudentSes,CPrice,TPrice,TeacherSes " +
                "from Discounts " +
                "Where StudentID = " + SID + " " +
                "AND ClassID = " + CID + "  " +
                "ORDER BY StudentSes asc");
            int price = 0;
            if (Type == "S")
            {
                price = GetInt(CID, "Class", "CPrice") / 4;
            }
            else if (Type == "T")
            {
                price = GetInt(CID, "Class", "TPayment") / 4;
            }
            int ses = 0;
            int total = 0;
            for (int f = 0; f < dtDiscounts.Rows.Count; f++)
            {

                if (TotalPay > (int.Parse(dtDiscounts.Rows[f][row2].ToString()) - ses) * price)
                {
                    total += (int.Parse(dtDiscounts.Rows[f][row2].ToString()) - ses); 
                    TotalPay -= (int.Parse(dtDiscounts.Rows[f][row2].ToString()) - ses) * price;
                }
                else
                {
                    return total = TotalPay / price;
                }
                price = int.Parse(dtDiscounts.Rows[f][row].ToString()) / 4;
                ses = int.Parse(dtDiscounts.Rows[f][row2].ToString());
            }
            return total + (TotalPay / price);

        }
        public static int GettotalPayTeacher(string SID , string GID , string Type)
        {
            SqlConnection con = Connexion.Connect();
            DataTable dt = new DataTable();
            string TID = GetTeacherID(GetClassID(GID).ToString()).ToString();
            FillDT(ref dt, "Select ID from TPayment Where TID = " + TID + " and GID = " + GID);
            int result = 0;
            for (int f = 0; f < dt.Rows.Count; f++)
            {
                SqlCommand CommandID = new SqlCommand("Select  Sum(" + Type + ") From TPaymentStudent Where ID = '" + dt.Rows[f][0].ToString() + "' and SID = " + SID, con);
                CommandID.ExecuteNonQuery();
                result += Convert.ToInt32(CommandID.ExecuteScalar());
            }
            con.Close();
            return result;
        }
        public static int GettotalPayStudent(string SID , string CID )
        {
            SqlConnection con = Connexion.Connect();
            SqlCommand CommandID = new SqlCommand("Select  Sum(Price) From StudentPayment Where ID = '" + SID + "' and type = 1 and CID = " + CID , con);
            CommandID.ExecuteNonQuery();
            Int32 result; 
            if (CommandID.ExecuteScalar() == System.DBNull.Value)
            {
                result = 0;  
            }
            else
            {
                result = Convert.ToInt32(CommandID.ExecuteScalar());
            }
            con.Close();
            return result;
        }
        public static bool TryConnect()
        {
            try
            {
                SqlConnection con = new SqlConnection("Data Source=" + DS +";Initial Catalog=" + DB + ";Persist Security Info=True;User ID=" + UserName + ";Password=" + password +"");
                con.Open();
                return true ;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false; 

            }
        }
        public static int[] CheckTimeHour(string Hour , int Day , int Room)
        {
            SqlConnection con = Connect();
            SqlCommand command = new SqlCommand(
                           "Select *," +
                           "CONVERT(INT,SUBSTRING(TimeStart,0,3)) as SHour," +
                           "CONVERT(INT,SUBSTRING(TimeEnd,0,3)) as EHour " +
                           "from Class_Time  " +
                                 "Where IDRoom = " + Room + " " +
                                 "And   Day = " + Day + " " +
                                 "And CONVERT(INT,SUBSTRING(TimeStart,0,3)) <= " + Hour + " " +
                                 "And CONVERT(INT,SUBSTRING(TimeEnd,0,3)) > " + Hour , con);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            int ID = -1;
            if (dt.Rows.Count > 0)
            {

                if(dt.Rows[0]["Type"].ToString() =="1")
                {
                    ID = int.Parse(dt.Rows[0]["GID"].ToString());
                }
                else
                {
                    ID = int.Parse(dt.Rows[0]["FID"].ToString());
                }
                int[] vs = { 2, ID, int.Parse(dt.Rows[0]["Type"].ToString()) };
                return vs; 
            }
            else
            {
                
                SqlCommand command2 = new SqlCommand(
                          "Select *," +
                          "CONVERT(INT,SUBSTRING(TimeStart,0,3)) as SHour," +
                          "CONVERT(INT,SUBSTRING(TimeEnd,0,3)) as EHour " +
                          "from Class_Time  " +
                                "Where IDRoom = " + Room + " " +
                                "And   Day = " + Day + " " +
                                "And CONVERT(INT,SUBSTRING(TimeStart,0,3)) <= " + Hour + " " +
                                "And CONVERT(INT,SUBSTRING(TimeEnd,0,3)) = " + Hour , con);
                DataTable dt2 = new DataTable();
                SqlDataAdapter da2 = new SqlDataAdapter(command2);
                da2.Fill(dt2);
                if (dt2.Rows.Count > 0)
                {
                    if (dt2.Rows[0]["Type"].ToString() == "1")
                    {
                        ID = int.Parse(dt2.Rows[0]["GID"].ToString());
                    }
                    else
                    {
                        ID = int.Parse(dt2.Rows[0]["FID"].ToString());
                    }
                    int[] vs = { 1, ID,  int.Parse(dt2.Rows[0]["Type"].ToString()) };
                    return vs;
                }
                else
                {
                    int[] vs = { 0 , -1 , -1};
                    return vs;
                }
            }
        }

        public static int[] CheckTimeHour(string Hour, int Day, int Room , string GID)
        {
            SqlConnection con = Connect();
            SqlCommand command = new SqlCommand(
                           "Select *," +
                           "CONVERT(INT,SUBSTRING(TimeStart,0,3)) as SHour," +
                           "CONVERT(INT,SUBSTRING(TimeEnd,0,3)) as EHour " +
                           "from Class_Time  " +
                                 "Where IDRoom = " + Room + " " +
                                 "And GID != " + GID +" and  Day = " + Day + " " +
                                 "And CONVERT(INT,SUBSTRING(TimeStart,0,3)) <= " + Hour + " " +
                                 "And CONVERT(INT,SUBSTRING(TimeEnd,0,3)) > " + Hour, con);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            int ID = -1;
            if (dt.Rows.Count > 0)
            {

                if (dt.Rows[0]["Type"].ToString() == "1")
                {
                    ID = int.Parse(dt.Rows[0]["GID"].ToString());
                }
                else
                {
                    ID = int.Parse(dt.Rows[0]["FID"].ToString());
                }
                int[] vs = { 2, ID, int.Parse(dt.Rows[0]["Type"].ToString()) };
                return vs;
            }
            else
            {

                SqlCommand command2 = new SqlCommand(
                          "Select *," +
                          "CONVERT(INT,SUBSTRING(TimeStart,0,3)) as SHour," +
                          "CONVERT(INT,SUBSTRING(TimeEnd,0,3)) as EHour " +
                          "from Class_Time  " +
                                "Where IDRoom = " + Room + " " +
                                "And GID != "+ GID+ " and  Day = " + Day + " " +
                                "And CONVERT(INT,SUBSTRING(TimeStart,0,3)) <= " + Hour + " " +
                                "And CONVERT(INT,SUBSTRING(TimeEnd,0,3)) = " + Hour, con);
                DataTable dt2 = new DataTable();
                SqlDataAdapter da2 = new SqlDataAdapter(command2);
                da2.Fill(dt2);
                if (dt2.Rows.Count > 0)
                {
                    if (dt2.Rows[0]["Type"].ToString() == "1")
                    {
                        ID = int.Parse(dt2.Rows[0]["GID"].ToString());
                    }
                    else
                    {
                        ID = int.Parse(dt2.Rows[0]["FID"].ToString());
                    }
                    int[] vs = { 1, ID, int.Parse(dt2.Rows[0]["Type"].ToString()) };
                    return vs;
                }
                else
                {
                    int[] vs = { 0, -1, -1 };
                    return vs;
                }
            }
        }
        public static string CheckUser(string UserName, string Password , string f )
        {
            SqlConnection con = Connect();
            SqlCommand cmd = new SqlCommand("select ID from Users where Username=N'" + UserName + "' And Password = N'" + f + Password + "'", con);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            if(dt.Rows.Count == 0)
            {
                return "-1";
            }
            else
            {
                string result = cmd.ExecuteScalar().ToString();
                return result.ToString();
            }
        }
        public static int GetSessions(string GroupID)
        {
            SqlConnection con = Connect();
            int Ses = GetInt(GroupID, "Groups", "Sessions" , "GroupID");
            int TSes = GetInt(GroupID, "Groups", "TSessions" , "GroupID");
            return Ses - TSes;
        }
        public static void FillDT(ref DataTable dt, string query)
        {
            try
            {
                dt.Clear();
                SqlCommand cmd = new SqlCommand(query, connecting);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }
        public static SqlConnection Connect()
        {
            try
            {


                SqlConnection con = new SqlConnection("Data Source=" + DS + ";Initial Catalog=" + DB + ";Persist Security Info=True;User ID=" + UserName + ";Password=" + password + "  ");
                con.Open();
                return con;
            }
            catch (SqlException ex)
            {
                // Log the exception or show a message to the user
                MessageBox.Show("An error occurred while trying to connect to the database: " + ex.Message, "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Optionally rethrow or handle the exception as needed
                throw;
            }
        }
        public static void SetConnect()
        {
            connecting = new SqlConnection("Data Source=" + DS + ";Initial Catalog=" + DB + ";Persist Security Info=True;User ID=" + UserName + ";Password=" + password + "");
            connecting.Open();

        }
        public static void Insert(string Text)
        {

            SqlCommand Command = new SqlCommand(Text, connecting);
            Command.ExecuteNonQuery();
        }
        public static string GetString(string ID, string Table, string Get)
        {
            SqlConnection con = Connexion.Connect();
            SqlCommand CommandID = new SqlCommand("Select case when  " + Get + " is null  then ' ' else "+ Get + " end as f From " + Table + " Where ID = '" + ID + "'", con);
            CommandID.ExecuteNonQuery();
            if(CommandID.ExecuteScalar() == null)
            {
                return " ";
            }
            string result = CommandID.ExecuteScalar().ToString();
            con.Close();
            return result;

        }
        public static string GetString(string query)
        {
            SqlCommand CommandID = new SqlCommand(query, connecting);
            string result = CommandID.ExecuteScalar().ToString();
            return result;

        }
        public static string GetString(string ID, string Table, string Get , string IDName)
        {
            SqlConnection con = Connexion.Connect();
            SqlCommand CommandID = new SqlCommand("Select  " + Get + " From " + Table + " Where "+ IDName+" = '" + ID + "'", con);
            string result = CommandID.ExecuteScalar().ToString();
            con.Close();
            return result;

        }
        public static int GetInt(string query)
        {
            SqlCommand CommandID = new SqlCommand(query, connecting);
            Int32 result = Convert.ToInt32(CommandID.ExecuteScalar());
            return result; 

        }

        public static int? GetIntnl(string query)
        {
            using (SqlCommand CommandID = new SqlCommand(query, connecting))
            {
                object result = CommandID.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    return null;
                }
                return Convert.ToInt32(result);
            }
        }
        public static int GetInt(string ID, string Table, string Get)
        {
            SqlConnection con = Connexion.Connect();
            SqlCommand CommandID = new SqlCommand("Select  " + Get + " From " + Table + " Where ID = '" + ID + "'", con);
            CommandID.ExecuteNonQuery();
            Int32 result = Convert.ToInt32(CommandID.ExecuteScalar());
            con.Close();

            return result;

        }
        public static int GetInt(string ID, string Table, string Get, string IDName)
        {
            SqlConnection con = Connexion.Connect();
            SqlCommand CommandID = new SqlCommand("Select  " + Get + " From " + Table + " Where " + IDName + "= '" + ID + "'", con);
            CommandID.ExecuteNonQuery();
            Int32 result = Convert.ToInt32(CommandID.ExecuteScalar());
            con.Close();

            return result;

        }
        public static int GetInt(string ID, string Table, string Get, string IDName, string IDName2, string ID2)
        {
            SqlConnection con = Connexion.Connect();
            SqlCommand CommandID = new SqlCommand("Select  " + Get + " From " + Table + " Where " + IDName + "= '" + ID + "'And " + IDName2 + "='" + ID2 + "'", con);
            Object Ob = CommandID.ExecuteScalar();
            if (Ob == System.DBNull.Value)
            {
                return -1;
            }
            Int32 result = Convert.ToInt32(CommandID.ExecuteScalar());
            con.Close();

            return result;

        }
        public static string GetID(string Table)
        {
            SqlCommand GetID = new SqlCommand(" SELECT IDENT_CURRENT('" + Table + "') as ID  ", connecting);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(GetID);
            da.Fill(dt);
            return dt.Rows[0]["ID"].ToString();
        }

        public static int InsertTeacher(string FirstName, string LastName, string Phone, int Gender, string Adress, string CCP, string Note, string Birthdate, string Email)
        {

            SqlConnection con = Connexion.Connect();
            SqlCommand TAddCommand = new SqlCommand("Insert into Teacher (TFirstName , TLastName , TPhoneNumber , TGender, TAdress   , TCCP  , TNote , TBirthDate ,Email,TRegister , Status ) OUTPUT Inserted.ID values(N'" + FirstName + "',N'" + LastName + "',N'" + Phone + "',N'" + Gender + "',N'" + Adress + "',N'" + CCP + "',N'" + Note + "',N'" + Birthdate + "',N'" + Email + "','" + DateTime.Now.ToString("MM / dd / yyyy HH: mm:ss") + "' , 1 )", con);
            Int32 result = Convert.ToInt32(TAddCommand.ExecuteScalar());
            return result; 
        }

        public static void insertPic(string Type, string pic, string ID)
        {

            SqlConnection con = Connexion.Connect();
            SqlCommand conn = new SqlCommand("Update " + Type + " Set  Picture = '" + pic + "' Where ID = '" + ID + "'", con);
            conn.ExecuteNonQuery();
        }

        public static void FillDataGrid(string ID, ref DataGrid DG, string TableName)
        {

            SqlConnection con = Connexion.Connect();
            SqlCommand conn = new SqlCommand("Select * From " + TableName + " Where ID = " + ID, con);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(conn);
            da.Fill(dt);
            DG.ItemsSource = dt.DefaultView;
            con.Close();

        }
        public static void FillCBName(string FirstName, string LastName, string Table, ref ComboBox comboBox)
        {
            DataTable dt = new DataTable();
            SqlConnection con = Connexion.Connect();
            SqlCommand TCconn = new SqlCommand("Select *, (" + FirstName + " + ' ' + " + LastName + " )  as Name  from " + Table, con);
            SqlDataAdapter da = new SqlDataAdapter(TCconn);
            da.Fill(dt);
            comboBox.ItemsSource = dt.DefaultView;

        }
        public static void FillDTItem(string Table, ref ComboBox comboBox)
        {
            DataTable dt = new DataTable();
            SqlConnection con = Connexion.Connect();
            SqlCommand command = new SqlCommand("SELECT * FROM " + Table, con);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            comboBox.ItemsSource = dt.DefaultView;
        }
        public static void FillCB(ref ComboBox cb, string text)
        {
            SqlConnection con = Connect();
            SqlCommand conn = new SqlCommand(text, con);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(conn);
            da.Fill(dt);
            cb.ItemsSource = dt.DefaultView;

        }
        public static void FillCB(ref DevExpress.Xpf.Editors.ComboBoxEdit cb, string text)
        {
            SqlConnection con = Connect();
            SqlCommand conn = new SqlCommand(text, con);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(conn);
            da.Fill(dt);
            cb.ItemsSource = dt.DefaultView;

        }
        public static void FillDG(ref DataGrid DG, string text)
        {
            SqlConnection con = Connexion.Connect();
            SqlCommand conn = new SqlCommand(text, con);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(conn);
            da.Fill(dt);
            DG.ItemsSource = dt.DefaultView;
        }
        public static void FillDG(ref ListView DG, string text)
        {
            SqlConnection con = Connexion.Connect();
            SqlCommand conn = new SqlCommand(text, con);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(conn);
            da.Fill(dt);
            DG.ItemsSource = dt.DefaultView;
        }
        public static void FillListView(ref ListView lV, string query)
        {
            SqlConnection con = Connexion.Connect();
            SqlCommand conn = new SqlCommand(query, con);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(conn);
            da.Fill(dt);
            lV.DataContext = dt.DefaultView;
        }
        public static int GetPrice(string ClassID, string StudentID)
        {
            SqlConnection con = Connexion.Connect();

            SqlCommand CommandID = new SqlCommand("Select  CPrice From Discounts Where StudentID = '" + StudentID + "' And done is null and ClassID = " + ClassID, con);
            CommandID.ExecuteNonQuery();
            object result = CommandID.ExecuteScalar();
            if (result == null)
            {
                return Connexion.GetInt(ClassID, "Class", "CPrice");
            }
            else
            {
                return Int32.Parse(result.ToString());
            }
        }
        public static int GetTPrice(string ClassID, string StudentID)
        {
            SqlConnection con = Connexion.Connect();

            SqlCommand CommandID = new SqlCommand("Select  TPrice From Discounts Where StudentID = '" + StudentID + "' And ClassID = " + ClassID, con);
            CommandID.ExecuteNonQuery();
            object result = CommandID.ExecuteScalar();
            if (result == null)
            {
                return Connexion.GetInt(ClassID, "Class", "TPayment");
            }
            else
            {
                return Int32.Parse(result.ToString());
            }
        }
        
        public static string ReturnString(string query)
        {
            SqlConnection con = Connexion.Connect();

            SqlCommand CommandID = new SqlCommand(query, con);
            CommandID.ExecuteNonQuery();
            object result = CommandID.ExecuteScalar();
            return result.ToString();
        }
        public static bool IFNULL(string query)
        {
            SqlConnection con = Connexion.Connect();
            SqlCommand Command = new SqlCommand(query, con);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(Command);
            da.Fill(dt);
            if (dt.Rows.Count >= 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool IFNULLVar(string query)
        {
            SqlConnection con = Connexion.Connect();
            SqlCommand Command = new SqlCommand(query, con);
            Command.ExecuteNonQuery();
            object result = Command.ExecuteScalar();
            if (result == System.DBNull.Value || result == null || result == "" )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static int Language()
        {
            SqlConnection con = Connexion.Connect();
            SqlCommand Command = new SqlCommand("Select Language from EcoleSetting", con);
            Command.ExecuteNonQuery();
            object result = Command.ExecuteScalar();
            bool Exist = Connexion.IFNULL("Select Language from EcoleSetting");
            if (Exist == false && result != System.DBNull.Value)
            {
                return Int32.Parse(result.ToString());
            }
            else
            {
                return 1;
            }
        }
        public static int GetInscFees()
        {
            SqlConnection con = Connexion.Connect();
            SqlCommand Command = new SqlCommand("Select InsFees from EcoleSetting", con);
            Command.ExecuteNonQuery();
            object result = Command.ExecuteScalar();
            bool Exist = Connexion.IFNULLVar("Select InsFees from EcoleSetting");
            if (Exist == false)
            {
                return Int32.Parse(result.ToString());
            }
            else
            {
                return 0;
            }
        }
        public static string GetFile()
        {
            SqlConnection con = Connexion.Connect();
            bool Exist = Connexion.IFNULL("Select ApplicationFile from EcoleSetting");
            if (Exist == false)
            {
                SqlCommand Command = new SqlCommand("Select ApplicationFile from EcoleSetting", con);
                Command.ExecuteNonQuery();
                object result = Command.ExecuteScalar();
                return result.ToString();
            }
            else
            {
                return new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..").ToString();
            }
        }
       public static int GetPayedAmmount(string StudentID , string ClassID)
       {

            return 0; 
       }
       public static string GetImagesFile()
       {
            SqlConnection con =  Connexion.Connect();
            bool Exist = Connexion.IFNULLVar("Select PhotoFile from EcoleSetting");
            if(Exist == false)
            {
                SqlCommand Command = new SqlCommand("Select PhotoFile from EcoleSetting", con);
                Command.ExecuteNonQuery();
                object result = Command.ExecuteScalar();
                return result.ToString();
            }
            else
            {
                return @"C:\ProgramData\EcoleSetting\EcolePhotos";
            }
        }
    }
}
