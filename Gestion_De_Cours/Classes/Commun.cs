using Gestion_De_Cours.Panels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gestion_De_Cours.Classes
{
    class Commun
    {
        public static int FastReportEdit = 1;
        public static int IDCR = -1 ; 
        public static bool PrivaEdit = true;
        public static double ScreenWIdth = 0 ;
        public static double ScreenHeight = 0;
        public static UControl.AllAttendance AllA;
        public static MainWindow MW;
        public static ResourceDictionary ResourceDic = new ResourceDictionary();
        public static void SetMW( MainWindow main)
        {
            MW = main;
        }
        public static void setResourceDic(int i)
        {
            switch (i)
            {
                case 0:
                    ResourceDic.Source = new Uri("Dictionary\\EN.xaml", UriKind.Relative);
                    break;
                case 1:
                    ResourceDic.Source = new Uri("Dictionary\\AR.xaml", UriKind.Relative);
                    break;
                case 2:
                    ResourceDic.Source = new Uri("Dictionary\\FR.xaml", UriKind.Relative);
                    break;
                default:
                   ResourceDic.Source = new Uri("Dictionary\\EN.xaml", UriKind.Relative); // Fallback to English
                    break;
            }
        }
        public static void UpdateAllAttendance()
        {
            MW.UpdateAllAttendance();
        }

        public static async void CreateAllA()
        {
            AllA = new UControl.AllAttendance();
        }

        public static bool CheckSeatsClass(string GID , string Message , int addons =  0 )
        {
            int CountSGroups = Connexion.GetInt("Select dbo.GetStudentsAmmount(" + GID + ") ");
            DataTable dtTime = new DataTable();
            Connexion.FillDT(ref dtTime, "Select * from class_time where GID = " + GID);
            foreach (DataRow rowTime in dtTime.Rows)
            {
                int Seats = Connexion.GetInt("Select case when Seats is null then 0 else Seats end as s from Rooms Where ID = " + rowTime["IDRoom"].ToString());
                if(Seats == 0)
                {
                    return true; 
                }
                if (CountSGroups + addons >= Seats)
                {
                    MessageBoxResult result = MessageBox.Show(Message, "Alert", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if(result == MessageBoxResult.No)
                    {
                        return false;
                    }
                    else
                    {
                        return true; 
                    }
                }
            }
            return true; 


        }

        public static bool ChangeGroupOneTime(string SID, string ToGID, string AID, string DateFrom)
        {
            string CID = Connexion.GetClassID(ToGID).ToString();
            int FromGID = Connexion.GetGroupID(SID,CID);
            DateFrom = DateFrom.Replace("/", "-");
            string DateTo = Connexion.GetString("Select Date from Attendance Where ID = " + AID);
            DateTime date1 = DateTime.ParseExact(DateFrom, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime date2 = DateTime.ParseExact(DateTo, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            int? AIDFrom = Connexion.GetIntnl($"Select ID from Attendance Where GroupID = {FromGID} and Date = '{DateFrom}'");
            if (AIDFrom == null)
            {
                if (date1 > date2)
                {
                    if (MessageBox.Show(ResourceDic["NoSesInDate"].ToString(), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.None, Commun.GetAllignMessageBox()) != MessageBoxResult.Yes)
                    {
                        return false;
                    }
                }
                Connexion.Insert($"Insert into Attendance_Change(SID,FromGID,FromDate,ToAID) values({SID},{FromGID},'{DateFrom}',{AID})");
            }
            else
            {
                Commun.SetStatusAttendanceupg(SID, AIDFrom.ToString(),CID, FromGID.ToString(), DateFrom, 2);//setting the old status to absent 
            }
            Commun.InsertStuAttend(SID, AID,CID,ToGID,DateTo,1);
            return true;
        }

        public static string GetStudentsNotInGroupQuery(string GID)
        {
            string CID = Connexion.GetClassID(GID).ToString();
            string YearID = Connexion.GetString($"Select CYear from Class Where ID= {CID}");
            string query = "SELECT Students.ID  , Students.Parentnumber , Students.PhoneNumber ,dbo.GetStudentSubjects(Students.ID) as Subjects , case when Students.LastName is NULL then students.FirstName " +
                           "When Students.Firstname is NULL then Students.LastName " +
                           "when Students.Firstname is not Null and Students.LastName is not null then Students.LastName + ' ' + Students.FirstName end as Name ,Students.Adress,Students.Level as LevelID ,dbo.ReplaceArabicName(Students.FirstName) + ' ' + dbo.ReplaceArabicName(Students.LastName) as NameSearch ,dbo.ReplaceArabicName(Students.LastName) + ' ' + dbo.ReplaceArabicName(Students.FirstName) as RNameSearch ,  Students.FirstName as FirstName,Students.LastName as LastName , 'False' as IsChecked, Students.BarCode as BarCode,Students.Gender,(FirstName + ' ' + LastName) as Name  ,Students.FirstName + ' ' + Students.LastName as RName from Students  Where  Students.Status = 1 and Year = " + YearID;
            DataTable dtspec = new DataTable();
            Connexion.FillDT(ref dtspec, $"Select * from Class_Speciality where ID = {CID}");
            int i = 0;
            foreach (DataRow row in dtspec.Rows )
            {
                if (i == 0)
                {
                    query += "AND SPECIALITY = " + row["SpecID"].ToString();
                }
                else
                {
                    query += "OR SPECIALITY = " + row["SpecID"].ToString();
                }
                i++;
            }
            string gender = Connexion.GetString($"Select GroupGender from Groups Where GroupID = {GID}");
            query += " Except SELECT Students.ID  , Students.Parentnumber , Students.PhoneNumber ,dbo.GetStudentSubjects(Students.ID) as Subjects , case when Students.LastName is NULL then students.FirstName " +
                    "When Students.Firstname is NULL then Students.LastName " +
                    "when Students.Firstname is not Null and Students.LastName is not null then Students.LastName + ' ' + Students.FirstName end as Name ,Students.Adress,Students.Level as LevelID ,dbo.ReplaceArabicName(Students.FirstName) + ' ' + dbo.ReplaceArabicName(Students.LastName) as NameSearch ,dbo.ReplaceArabicName(Students.LastName) + ' ' + dbo.ReplaceArabicName(Students.FirstName) as RNameSearch ,  Students.FirstName as FirstName,Students.LastName as LastName , 'False' as IsChecked, Students.BarCode as BarCode,Students.Gender,(FirstName + ' ' + LastName) as Name  , Students.FirstName + ' ' + Students.LastName as RName  from Students Join Class_Student On Class_Student.StudentID = Students.ID   Where Students.Status = 1 and  Class_Student.ClassID = " + CID + " and Class_Student.Stopped = '0' ORDER BY ID ASC";
            return query;
        }
        public static void AddStudentToClass(string SID , string GID ,string Date,int showmessage = -1)
        {
            string CID = Connexion.GetClassID(GID).ToString();
            if (!Commun.CheckSeatsClass(GID, ResourceDic["WarningSeatsMax"].ToString()))
            {
                return;
            }
            if(!Connexion.IFNULL($"Select * from Class_Student Where  StudentID={SID} and GroupID = {GID} "))
            {
                string OldEndDate = Connexion.GetString($"Select Stopped from Class_Student Where StudentID = {SID} and GroupID = {GID}");
                Connexion.Insert($"Update Class_Student Set Stopped = '0' where StudentID = {SID} and groupID = {GID}");
                DataTable dtAttendances = new DataTable();
                Connexion.FillDT(ref dtAttendances, $"SELECT * FROM Attendance " +
                    $"WHERE GroupID = {GID} " +
                    $"AND CONVERT(DATE, Date, 105) > CONVERT(DATE, '{OldEndDate}', 105) " +
                    $"AND CONVERT(DATE, Date, 105) < CONVERT(DATE, '{Date}', 105) ");
                foreach (DataRow row in dtAttendances.Rows)
                {
                    Commun.InsertStuAttend(SID, row["ID"].ToString(), CID, GID, row["Date"].ToString(), 3, ResourceDic["JustifStoppedGroup"].ToString());
                }
            }
            else
            {
                Connexion.Insert($"Insert Into " +
               $"Class_Student(StudentID,ClassID,GroupID,Stopped,StartDate) " +
               $"Values({SID},{CID} ,{GID},{0},'{Date}')");
            }
            DataTable dtdates = new DataTable();
            Connexion.FillDT(ref dtdates ,$"SELECT id,[date] FROM Attendance WHERE GroupID = {GID} and CONVERT(date, [Date], 105) >= CONVERT(date,'{Date}', 105)");
           //add here the discount check to check if he studies in x condition insert into discount
            if(dtdates.Rows.Count >= 1)
            {
                string message = string.Format((string)ResourceDic["SesAfterToday"].ToString(), Date);
                MessageBoxResult msgresult;
                if (showmessage == -1)
                {
                    msgresult = MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.None, Commun.GetAllignMessageBox());
                }
                else
                {
                    if (showmessage == 1)
                    {
                        msgresult = MessageBoxResult.Yes;
                    }
                    else
                    {
                        msgresult = MessageBoxResult.No;
                    }
                }
                if (msgresult == MessageBoxResult.Yes)
                {
                    foreach(DataRow row in dtdates.Rows)
                    {
                        InsertStuAttend(SID,row["ID"].ToString(),CID,GID,row["Date"].ToString(),1);
                    }
                }
            }
            //here check discounts and check if student was already marked in this class and stopped 
        }
        public static void ChangeGroupForever(string SID, string ToGID,string Date)
        {
            int CID = Connexion.GetClassID(ToGID);
            int GID = Connexion.GetGroupID(SID, CID.ToString());
            if (Connexion.GetString($"Select stopped from Class_Student Where  StudentID={SID} and GroupID = {GID} ") == "0")
            { 
                Connexion.Insert($"Update Class_Student Set Stopped = '{Date}' where StudentID = {SID} and ClassID = {CID} and GroupID = {GID}  and Stopped = '0'");
            }
            Connexion.Insert($"Insert Into " +
               $"Class_Student(StudentID,ClassID,GroupID,Stopped,StartDate) " +
               $"Values({SID},{CID} ,{ToGID},{0},'{Date}')");
          /*   DataTable dtdatesNewgroup = new DataTable();
            DataTable dtdatesOldgroup = new DataTable();
            Connexion.FillDT(ref dtdatesNewgroup, $"SELECT id,[date] FROM Attendance WHERE GroupID = {ToGID} and CONVERT(date, [Date], 105) >= CONVERT(date, {Date}, 105)");
            Connexion.FillDT(ref dtdatesOldgroup, $"SELECT id,[date] FROM Attendance WHERE GroupID = {GID} and CONVERT(date, [Date], 105) >= CONVERT(date, {Date}, 105)");
            foreach(DataRowView row in dtdatesNewgroup.Rows)
            {
                string AID = row["ID"].ToString();
                InsertStuAttend(SID,AID,)
            }  IDK IF I SHOULD DELETE STUDENT FROM OLD ATTENDANCES AND INSERT IN NEW GROUP */ 
        }
        public static void LeaveGroup(string SID , string GID,string NewEndDate = "0")
        {
            string CID = Connexion.GetClassID(GID).ToString();
            if (NewEndDate == "0")
            {
                string StartDate = Connexion.GetString($"Select StartDate from Class_Student Where StudentID = {SID} and GroupID = {GID}");
                DataTable dtlist = new DataTable();
                Connexion.FillDT(ref dtlist,
                    "SELECT " +
                    "CASE " +
                        "WHEN Attendance_Student.Status = 0 THEN N'" + ResourceDic["Absent"].ToString() + "' " +
                        "WHEN Attendance_Student.Status = 1 THEN N'" + ResourceDic["Present"].ToString() + "' " +
                        "WHEN Attendance_Student.Status = 2 THEN N'" + ResourceDic["GroupChange"].ToString() + "' " +
                        "WHEN Attendance_Student.Status = 3 THEN N'" + ResourceDic["Justified"].ToString() + "' " +
                    "END AS StatusText, " +
                    "Attendance.Date, " +
                    "Attendance.Session AS Ses " +
                    "FROM Attendance " +
                    "LEFT JOIN Attendance_Student " +
                        "ON Attendance.ID = Attendance_Student.ID " +
                        "AND Attendance_Student.StudentID = " + SID + " " + // moved here
                    "WHERE CONVERT(DATE, Attendance.Date, 105) >= CONVERT(DATE,'" + StartDate + "', 105) " +
                    "AND Attendance.GroupID = " + GID + " " +
                    "ORDER BY CONVERT(DATE, Attendance.Date, 105) DESC"
                );
                DataRow newRow = dtlist.NewRow();
                // Set values for the new row
                newRow["Date"] = ResourceDic["BackStudyingGroup"].ToString();  // Example: Set current date
                newRow["Ses"] = "-1";         // Example: Set 'Ses' column value
                dtlist.Rows.Add(newRow);
                DataRow newRow2 = dtlist.NewRow();
                newRow2["Date"] = ResourceDic["DeleteCompletly"].ToString();  // Example: Set current date
                newRow2["Ses"] = "-2";
                dtlist.Rows.Add(newRow2);
                Panels.DataGridWithSearch dgsearch = new Panels.DataGridWithSearch("Sessions", dtlist);
                bool? result = dgsearch.ShowDialog();

                if (result == true)
                {
                    var selectedValue = dgsearch.SelectedValue;
                    NewEndDate = selectedValue.ToString();
                }
                else
                {
                    return;
                }
            }
            string OldEndDate = Connexion.GetString("Select Stopped from Class_Student Where StudentID = " + SID + " and GroupID = " + GID);
            if (NewEndDate == "-1")// back to studying 
            {
                Connexion.Insert($"Update Class_Student Set Stopped = '0' where StudentID ={SID} and GroupID = {GID}");
                if (OldEndDate == "0")
                {
                    return;
                }
                string LastDate = Connexion.GetLastDateGroup(GID);
                if (LastDate == "0")
                {
                    return;
                }
                DataTable dtAttendances = new DataTable();
                Connexion.FillDT(ref dtAttendances, $"SELECT * FROM Attendance " +
                    $"WHERE GroupID = {GID} " +
                    $"AND CONVERT(DATE, Date, 105) > CONVERT(DATE, '{OldEndDate}', 105) " +
                    $"AND CONVERT(DATE, Date, 105) < CONVERT(DATE, '{LastDate}', 105) ");

                foreach (DataRow row in dtAttendances.Rows)
                {
                    Commun.InsertStuAttend(SID, row["ID"].ToString() , CID, GID, row["Date"].ToString(), 3, ResourceDic["JustifStoppedGroup"].ToString());
                }
            }
            else if (NewEndDate == "-2")//delete completly
            {
                Connexion.Insert("Delete from Class_Student Where GroupID = " + GID + " and StudentID = " + SID);
                DataTable dtAttendance = new DataTable();
                Connexion.FillDT(ref dtAttendance, "Select ID from Attendance Where GroupID = " + GID);
                for (int i = 0; i < dtAttendance.Rows.Count; i++)
                {
                    Connexion.Insert("Delete from Attendance_Student Where StudentID = " + SID + " and ID = " + dtAttendance.Rows[i]["ID"].ToString());
                }
            }
            else
            {
                string StartDate = Connexion.GetString("Select StartDate from Class_Student Where StudentID = " + SID + " and GroupID = " + GID);
               
                DateTime d1;
                DateTime d2;
                bool conidition = false;

                if (OldEndDate == "0")
                {
                    conidition = true;
                }
                else if(DateTime.ParseExact(OldEndDate, "dd-MM-yyyy", CultureInfo.InvariantCulture) >= DateTime.ParseExact(NewEndDate, "dd-MM-yyyy", CultureInfo.InvariantCulture) )//OldEndDate >= newEndDate
                {
                    conidition = true;
                }
                if (conidition)
                {
                    DataTable dtAttendance = new DataTable();
                    string query  = $"SELECT * FROM Attendance " +
                      $"WHERE GroupID = {GID} " +
                      $"AND CONVERT(DATE, Date, 105) > CONVERT(DATE, '{NewEndDate}', 105) ";
                    Connexion.FillDT(ref dtAttendance, query);
                    foreach(DataRow row in dtAttendance.Rows)
                    {
                        Connexion.Insert("Delete From Attendance_Student where StudentID = " + SID + " and ID = " + row["ID"].ToString());
                    }
                }
                else
                {
                    string message = string.Format((string)ResourceDic["NotRegAddStu"].ToString(), NewEndDate , Connexion.GetString("Select GroupName from Groups Where GroupID = " + GID));
                    if (MessageBox.Show(message,
                           "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                    DataTable dtAttendances = new DataTable();
                    Connexion.FillDT(ref dtAttendances, $"SELECT * FROM Attendance " +
                        $"WHERE GroupID = {GID} " +
                        $"AND CONVERT(DATE, Date, 105) >= CONVERT(DATE, '{OldEndDate}', 105) " +
                        $"AND CONVERT(DATE, Date, 105) <= CONVERT(DATE, '{NewEndDate}', 105) ");
                    foreach (DataRow row in dtAttendances.Rows)
                    {
                        InsertStuAttend(SID, row["ID"].ToString(),CID,GID,row["Date"].ToString(), 1);
                    }
                }
                Connexion.Insert("Update Class_Student Set stopped = '" + NewEndDate + "' Where StudentID = " + SID + " and GroupID = " + GID);
            }
        }

        public static void ChangeStartSesStudent(string SID , string GID, string NewStartDate = "0" )
        {
            string CID = Connexion.GetClassID(GID).ToString();
            string OldStartDate = Connexion.GetString("Select StartDate from Class_Student Where StudentID = " + SID + " and GroupID = " + GID);
            if (NewStartDate == "0")
            {
                DataTable dtlist = new DataTable();
                string EndDate = Connexion.GetString("Select Stopped from Class_Student Where StudentID = " + SID + " and GroupID = " + GID);
                if (EndDate == "0")
                {
                    Connexion.FillDT(ref dtlist, $"Select Date,session as ses  from Attendance Where GroupID = {GID} order by CONVERT(DATE, Attendance.Date, 105) desc ");
                }
                else
                {
                    Connexion.FillDT(ref dtlist, $"Select Date from Attendance Where GroupID = {GID} and CONVERT(DATE, Attendance.Date, 105) <= CONVERT(DATE, '{EndDate}', 105) order by CONVERT(DATE, Attendance.Date, 105) desc ");
                }
                Panels.DataGridWithSearch dgsearch = new Panels.DataGridWithSearch("Sessions", dtlist);
                bool? result = dgsearch.ShowDialog();
                if (result == true)
                {
                    var selectedValue = dgsearch.SelectedValue;
                    NewStartDate = selectedValue.ToString();
                }
                else
                {
                    MessageBox.Show("No Start Session has been selected");
                    return;
                }
            }
            if (NewStartDate == "-2")//delete completly
            {
               
                Connexion.Insert("Delete from Class_Student Where GroupID = " + GID + " and StudentID = " + SID);
                DataTable dtAttendance = new DataTable();
                Connexion.FillDT(ref dtAttendance, "Select ID from Attendance Where GroupID = " + GID);
                for (int i = 0; i < dtAttendance.Rows.Count; i++)
                {
                    Connexion.Insert("Delete from Attendance_Student Where StudentID = " + SID + " and ID = " + dtAttendance.Rows[i]["ID"].ToString());
                }
            }
            else if (NewStartDate != "-1")
            {
                DateTime d1 = DateTime.ParseExact(OldStartDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateTime d2 = DateTime.ParseExact(NewStartDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                if (d1 < d2 )
                {
                    DataTable dtAttendance = new DataTable();
                    string query = $"SELECT * FROM Attendance " +
                      $"WHERE GroupID = {GID} " +
                      $"AND CONVERT(DATE, Date, 105) < CONVERT(DATE, '{NewStartDate}', 105) ";
                    Connexion.FillDT(ref dtAttendance, query);
                    foreach (DataRow row in dtAttendance.Rows)
                    {
                        Connexion.Insert("Delete From Attendance_Student where StudentID = " + SID + " and ID = " + row["ID"].ToString());
                    }
                }
                else
                {
                    DataTable dtAttendances = new DataTable();
                    Connexion.FillDT(ref dtAttendances, $"SELECT * FROM Attendance " +
                        $"WHERE GroupID = {GID} " +
                        $"AND CONVERT(DATE, Date, 105) >= CONVERT(DATE, '{NewStartDate}', 105) " +
                        $"AND CONVERT(DATE, Date, 105) <= CONVERT(DATE, '{OldStartDate}', 105) ");
                    foreach (DataRow row in dtAttendances.Rows)
                    {
                        InsertStuAttend(SID, row["ID"].ToString(),CID,GID,row["Date"].ToString(), 1);
                    }
                }
                Connexion.Insert("Update Class_Student Set startDate = '" + NewStartDate + "'Where StudentID = " + SID + " and GroupID = " + GID);
            }

        }
        public static int ChangeInitialPrice(string SID, string GID, int initialprice = -1)
        {
            string CID = Connexion.GetClassID(GID).ToString();
            int initialOld = Connexion.GetInt($"Select ISNULL(Sum(StartPrice),0) From Class_Student Where StudentID = {SID} and ClassID = {CID}");
            bool? dialogResult = false;
            if (initialprice == -1)
            {
                OptionPanels.TwoTextBoxPage twooption = new OptionPanels.TwoTextBoxPage(ResourceDic["EnterNewPrice"].ToString(), ResourceDic["SPriceLB"].ToString(), "", initialOld.ToString(), "");
                dialogResult = twooption.ShowDialog();
                if (dialogResult == true)
                {
                    string[] result = twooption.Result;
                    int resultint;
                    if (result[0] != "" && int.TryParse(result[0], out resultint))
                    {
                        initialprice = resultint;
                    }
                    else
                    {
                        MessageBox.Show("No ammount was inserted");
                        return -1;
                    }
                }
                else
                {
                    return -1 ;
                }
            }
            Connexion.Insert("Update Class_Student Set StartPrice = 0  Where StudentID = " + SID + " And ClassID = " + CID);
            Connexion.Insert("Update TOP (1) Class_Student Set StartPrice = " + initialprice + " Where StudentID = " + SID + " And ClassID = " + CID);
            return initialprice;
        }
        public static void InsertStuAttend(string SID, string AID , string CID, string GID, string date, int status, string reason = "")
        {
            if(Connexion.IFNULL($"Select ID from Attendance_Student where ID = {AID} and StudentID = {SID}"))
            {
                Connexion.Insert($"Insert into Attendance_Student(ID,StudentID) values ({AID},{SID})");
                int? JID = Connexion.GetIntnl($"Select * from Justif Where SID = {SID} and Date ='{date}' and CID = {CID} and AID =0");
                if(JID !=null)
                {
                    reason = Connexion.GetString("Select Reason from Justif Where ID= " + JID);
                    Connexion.Insert($"Update Justif Set AID ={AID} where ID= {JID}");
                    SetStatusAttendanceupg(SID, AID,CID,GID,date, 3, reason);
                }
            }
            if(status != -1)
            {
                SetStatusAttendanceupg(SID, AID, CID, GID, date, status,reason);
            }
        }
        public static void SetStatusAttendanceupg(string SID, string AID, string CID, string GID, string date, int status, string reason = "")
        {
            DataTable dummy = null;
            SetStatusAttendanceupg(SID, AID,CID,GID,date, status, ref dummy, reason);
        }

        public static void SetStatusAttendanceupg(string SID, string AID, string CID, string GID, string date, int status , ref DataRow[] rows ,string reason = null )
        {
            int Price = 0;
            int TPrice = 0;
            string statusText = "";
            int? OldStatus = Connexion.GetIntnl($"Select Status from Attendance_Student Where ID = {AID} and StudentID = {SID}");
           

            if (reason == null && status == 3)
            {
                var dialog = new Backup(ResourceDic["EnterReasonAbsence"].ToString(), 0);
                dialog.ShowDialog();
                if (dialog.DialogResult == true)
                {
                    reason = dialog.ResponseText;
                }
                else
                {
                    return;
                }
            }
            if (OldStatus == 2)
            {
                if (MessageBox.Show(ResourceDic["MessageBoxOverrideGroupChange"].ToString(), "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    int ToAID = Connexion.GetInt("Select ToAID from Attendance_Change Where FromGID = " + GID + " and SID = " + SID + " and FromDate = '" + date + "'");
                    Connexion.Insert("Delete from Attendance_Student Where ID=" + ToAID + " and StudentID = " + SID);
                    Connexion.Insert("Delete From Attendance_Change Where FromGID = " + GID + " and SID = " + SID + " and FromDate = '" + date + "'");
                }
                else
                {
                    return;
                }
            }
            if (OldStatus == 3)
            {
                if (MessageBox.Show(ResourceDic["MessageBoxOverrideJustif"].ToString(), "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Connexion.Insert("Delete from justif where SID = " + SID + " and AID = " + AID);
                }
                else
                {
                    return ;
                }
            }
            if (status == 2)//GroupChange 
            {
                Price = 0;
                TPrice = 0;
                statusText = ResourceDic["GroupChange"].ToString();
            }
            if (status == 3)
            {
                int? JID = Connexion.GetIntnl($"Select ID from Justif where Date = '{date}' and SID = {SID}  and CID ={CID}");
                if (JID == null)
                {
                    Connexion.GetInt("Insert into justif(SID,Reason,Date,AID,CID) Output Inserted.ID values ('" + SID + "',N'" + reason + " ','" + date + "', '" + AID + "'," + Connexion.GetClassID(GID) + ")");
                }
                Price = 0;
                TPrice = 0;
                statusText = ResourceDic["Justified"].ToString();
            }
            if (status == 0)//Absent
            {
                if (Connexion.GetInt("Select Absent from EcoleSetting") == 1)
                {
                    Price = GetPriceSession(SID, AID, CID, GID, date, "S");
                    TPrice = GetPriceSession(SID, AID, CID, GID, date, "T");
                }
                else
                {
                    Price = 0;
                    TPrice = 0;
                }
                statusText = ResourceDic["Absent"].ToString();
            }
            if (status == 1)//Present
            {
                Price = GetPriceSession(SID, AID, CID, GID, date, "S");
                TPrice = GetPriceSession(SID, AID, CID, GID, date, "T");
                statusText = ResourceDic["Present"].ToString();
            }
            Connexion.Insert($"Update Attendance_Student Set Status = {status} , Price = {Price} , TPrice = {TPrice} where ID = {AID} and StudentID =" + SID);
            foreach (DataRow row in rows)
            {
                row["Status"] = status;
                row["StatusText"] = statusText;
                row["Reason"] = reason;
                row["Status"] = status;
                row["Sessions"] = Connexion.GetString("Select dbo.CalcPriceSum(" + SID + "," + Connexion.GetClassID(GID) + ")");
               
            }
               
        }
        public static void SetStatusAttendanceupg(string SID, string AID,string CID , string GID,string date, int status, ref DataTable dt,string reason =null)
        {
            int Price = 0;
            int TPrice = 0;
            string StatusText = "";
            int? OldStatus = Connexion.GetIntnl($"Select Status from Attendance_Student Where ID = {AID} and StudentID = {SID}");
            if (reason == null && status == 3 )
            {
                var dialog = new Backup(ResourceDic["EnterReasonAbsence"].ToString(), 0);
                dialog.ShowDialog();
                if (dialog.DialogResult == true)
                {
                    reason = dialog.ResponseText;
                }
                else
                {
                    return;
                }
            }
            if (OldStatus == 2)
            {
                if (MessageBox.Show(ResourceDic["MessageBoxOverrideGroupChange"].ToString(), "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    int ToAID = Connexion.GetInt("Select ToAID from Attendance_Change Where FromGID = " + GID + " and SID = " + SID + " and FromDate = '" + date + "'");
                    Connexion.Insert("Delete from Attendance_Student Where ID=" + ToAID + " and StudentID = " + SID);
                    Connexion.Insert("Delete From Attendance_Change Where FromGID = " + GID + " and SID = " + SID + " and FromDate = '" + date + "'");
                }
                else
                {
                    return;
                }
            }
            if (OldStatus == 3)
            {
                if (MessageBox.Show(ResourceDic["MessageBoxOverrideJustif"].ToString(), "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Connexion.Insert("Delete from justif where SID = " + SID + " and AID = " + AID);
                    if (dt != null) Commun.EditRowInDataTable(ref dt, SID, "ID", "", "Reason");
                }
                else
                {
                    return;
                }
            }
            if (status == 2)//GroupChange or Justif
            {
                Price = 0;
                TPrice = 0;
                StatusText = ResourceDic["GroupChange"].ToString();

            }
            if (status == 3)
            {
                if (OldStatus == 1)
                {
                    string message = ResourceDic["TheStudentIsMarkedAs"].ToString() + ResourceDic["Present"].ToString() + ResourceDic["Doyouwantochangethat"].ToString();
                    if (MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                int? JID = Connexion.GetIntnl($"Select ID from Justif where Date = '{date}' and SID = {SID}  and CID ={CID}");
                if (JID == null)
                {
                    Connexion.GetInt("Insert into justif(SID,Reason,Date,AID,CID) Output Inserted.ID values ('" + SID + "',N'" + reason + " ','" + date + "', '" + AID + "'," + Connexion.GetClassID(GID) + ")");
                }
                Price = 0;
                TPrice = 0;
                StatusText = ResourceDic["Justified"].ToString();
            }
            if (status == 0)//Absent
            {
                if (Connexion.GetInt("Select Absent from EcoleSetting") == 1)
                {
                    Price = GetPriceSession(SID, AID,CID,GID,date, "S");
                    TPrice = GetPriceSession(SID, AID, CID, GID, date, "T");
                }
                else
                {
                    Price = 0;
                    TPrice = 0;
                }
                StatusText = ResourceDic["Absent"].ToString();

            }
            if (status == 1)//Present
            {
                Price = GetPriceSession(SID, AID, CID, GID, date, "S");
                TPrice = GetPriceSession(SID, AID, CID, GID, date, "T");
                StatusText = ResourceDic["Present"].ToString();
                
            }
            Connexion.Insert($"Update Attendance_Student Set Status = {status} , Price = {Price} , TPrice = {TPrice} where ID = {AID} and StudentID =" + SID);
            if (dt != null)
            {
                Commun.EditRowInDataTable(ref dt, SID, "ID", StatusText, "StatusText");
                Commun.EditRowInDataTable(ref dt, SID, "ID", status.ToString(), "Status");
                Commun.EditRowInDataTable(ref dt, SID, "ID", Connexion.GetString("Select dbo.CalcPriceSum(" + SID + "," + Connexion.GetClassID(GID) + ")"), "Sessions");
                if(status == 3)
                {
                    Commun.EditRowInDataTable(ref dt, SID, "ID", reason, "Reason");
                }
            }
        }

        public static string GetQueryDataTable(string type, string ID)
        {
            string query = "";

            if (type == "Attendance")
            {
                int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                string querySession = "";
                if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                {
                    string gid = Connexion.GetString("Select GroupID from Attendance Where ID=" + ID);
                    string CID = Connexion.GetClassID(gid).ToString();
                    int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                    monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                }
                if (monthlypayment == 1)
                {
                    string dateAttendanceString = Connexion.GetString(ID, "Attendance", "Date");
                    DateTime dateAttendance = DateTime.Parse(dateAttendanceString);
                    int monthNumber = dateAttendance.Month;
                    int year = dateAttendance.Year;
                    querySession = "dbo.CalculateMonthlyPaymentRemaining(Students.ID  ) ";
                }
                else if (monthlypayment == 0)
                {
                    if (Connexion.GetInt("Select CalcPrice from EcoleSetting") == 1)
                    {
                        querySession = "dbo.CalcPriceSum(Students.ID,Groups.ClassID)";
                    }
                    else
                    {
                        querySession = "dbo.GettotalPayStudent(Students.ID , Groups.ClassID) - dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'Su')";
                    }
                }

                query = @"
                SELECT 
                    SubQuery.Name,
                    SubQuery.RName,
                    SubQuery.Gender,
                    SubQuery.Sessions,
                    SubQuery.ID,
                    SubQuery.Status,
                    SubQuery.StatusText,
                    SubQuery.Reason,
                    CASE 
                        WHEN SubQuery.Sessions < 0 THEN  -1
                        ELSE 1
                    END AS paid
                FROM 
                    (
                        SELECT 
                            Students.FirstName + ' ' + Students.LastName AS Name,
                            Students.LastName + ' ' + Students.FirstName AS RName,
                            Students.Gender AS Gender,
                            " + querySession + @" AS Sessions,
                            Students.ID AS ID,
                            Attendance_Student.Status AS Status,
                            CASE 
                                WHEN Attendance_Student.Status = 0 THEN N'" + ResourceDic["Absent"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 1 THEN N'" + ResourceDic["Present"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 2 THEN N'" + ResourceDic["GroupChange"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 3 THEN N'" + ResourceDic["Justified"].ToString() + @"' 
                            END AS StatusText,
                            CASE 
                                WHEN Attendance_Student.Status = 3 THEN Justif.Reason 
                            END AS Reason
                        FROM 
                            Attendance 
                        JOIN 
                            Attendance_Student ON Attendance.ID = Attendance_Student.ID
                        JOIN 
                            Students ON Students.ID = Attendance_Student.StudentID
                        LEFT JOIN 
                            Justif ON (Justif.AID = Attendance.ID AND Justif.SID = Students.ID)
                        JOIN 
                            Groups ON Groups.GroupID = Attendance.GroupID
                        WHERE 
                            Attendance.ID = " + ID + @"
                    ) AS SubQuery;
                ";
                return query;
            }
            return "";
        }
        public void SetFastReport(int i)
        {
            FastReportEdit = i;
        }
        public static string ReplaceArabicName(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Define the replacement rules
            string[] arabicChars = { "ا", "ء", "ى", "ئ", "آ" };
            string[] replacements = { "أ", "أ", "ي", "ي", "أ" };

            // Replace Arabic characters in the input string
            for (int i = 0; i < arabicChars.Length; i++)
            {
                input = input.Replace(arabicChars[i], replacements[i]);
            }

            return input;
        }


        public  string GetAttendanceQuery(string querySession, int AID, string absent, string present, string groupChange, string justified)
        {
            return $@"
            SELECT 
                SubQuery.Name,
                SubQuery.RName,
                SubQuery.Gender,
                SubQuery.Sessions,
                SubQuery.ID,
                SubQuery.Status,
                SubQuery.StatusText,
                SubQuery.Reason
              
            FROM 
                (
                    SELECT 
                        Students.FirstName + ' ' + Students.LastName AS Name,
                        Students.LastName + ' ' + Students.FirstName AS RName,
                        Students.Gender AS Gender,
                        {querySession} AS Sessions,
                        Students.ID AS ID,
                        Attendance_Student.Status AS Status,
                        CASE 
                            WHEN Attendance_Student.Status = 0 THEN N'{absent}' 
                            WHEN Attendance_Student.Status = 1 THEN N'{present}' 
                            WHEN Attendance_Student.Status = 2 THEN N'{groupChange}' 
                            WHEN Attendance_Student.Status = 3 THEN N'{justified}' 
                        END AS StatusText,
                        CASE 
                            WHEN Attendance_Student.Status = 3 THEN Justif.Reason 
                        END AS Reason
                    FROM 
                        Attendance 
                    JOIN 
                        Attendance_Student ON Attendance.ID = Attendance_Student.ID
                    JOIN 
                        Students ON Students.ID = Attendance_Student.StudentID
                    LEFT JOIN 
                        Justif ON (Justif.AID = Attendance.ID AND Justif.SID = Students.ID)
                    JOIN 
                        Groups ON Groups.GroupID = Attendance.GroupID
                    WHERE 
                        Attendance.ID = {AID}
                ) AS SubQuery;
        ";
        }
        public static string Crop(string path , ref Image im)
        {
            if (path != "")
            {
                WindowEditImage window = new WindowEditImage(path);
                window.ShowDialog();
                if (window.DialogResult == true && window.ResponseText != null)
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(window.ResponseText);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    im.Source = bitmap;
                    return window.ResponseText;
                }
                else
                {
                    return "-1";
                }
            }
            else
            {
                return "-1";
            }
        }
        public static DataTrigger SetTrigger(string value, string binding)
        {
            //Style style = new Style();
            //style.TargetType = type;
            DataTrigger datatrigger = new DataTrigger();
            Binding DataTriggerBinding = new Binding(binding);
            DataTriggerBinding.Mode = BindingMode.Default;
            DataTriggerBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
          //  DataTriggerBinding.Converter = new CalculateIfGreater();
          //  DataTriggerBinding.ConverterParameter = binding; 
            datatrigger.Binding = DataTriggerBinding;
            datatrigger.Value = value;
            Setter DataTriggerSetter = new Setter();
            DataTriggerSetter.Property = DataGridCell.BackgroundProperty;
            DataTriggerSetter.Value = Brushes.Red;
            datatrigger.Setters.Add(DataTriggerSetter);
            //style.Triggers.Add(datatrigger);
            return datatrigger;
        }
        public class CalculateIfGreater : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter , CultureInfo culture)
            {
                if ((int)value >= (int)parameter )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
        public static DataGridCell GetCell(int row, int column, ref DataGrid DG)
        {
            DataGridRow rowContainer = GetRow(row, ref DG);

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null)
                {
                    DG.ScrollIntoView(rowContainer, DG.Columns[column]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }
                return cell;
            }
            return null;
        }
        public static DataGridRow GetRow(int index, ref DataGrid DG)
        {
            DataGridRow row = (DataGridRow)DG.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                DG.UpdateLayout();
                DG.ScrollIntoView(DG.Items[index]);
                row = (DataGridRow)DG.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
        public static MessageBoxOptions  GetAllignMessageBox()
        {
            if(Connexion.Language()== 1)
            {
                return MessageBoxOptions.RightAlign;
            }
            else
            {
                return MessageBoxOptions.None;
            }
        }

        public static void FillFormationAttendance(string AID)
        {
            string FID = Connexion.GetString("Select FID from Formation_Attendance where ID = " + AID);
            string query = "Select SID from Formation_Student where FID = " + FID;
            DataTable dt = new DataTable();
            Connexion.FillDT(ref dt, query);
            for (int count = 0; count < dt.Rows.Count; count++)
            {
                query = "Insert into Formation_Attendance_Student(AID,SID) Values(" + AID + "," + dt.Rows[count]["SID"].ToString() + ")";
                Connexion.Insert(query);
            }

        }
        public static int GetPriceSession(string SID , string AID ,string ClassID , string GroupID, string date, string type)
        {
            string ColPrice = (type == "S") ? "CPrice" : (type == "T") ? "TPrice" : null;
            string ColPriceClass = (type == "S") ? "CPrice" : (type == "T") ? "TPayment" : null;
            string ColDate = (type == "S") ? "StudentDate" : (type == "T") ? "TeacherDate" : null;
            int? Price ;
            Price = Connexion.GetIntnl($@"
                    SELECT {ColPrice} / 4 
                    FROM Discounts 
                    WHERE StudentID = {SID}
                      AND ClassID = {ClassID}
                      AND CONVERT(DATE,{ColDate}, 105) <= CONVERT(DATE,'{date}', 105)
                    And (Done is null or  CONVERT(DATE,Done, 105) > CONVERT(DATE,'{date}', 105) )");
            if(Price == null)
            {
                Price = Convert.ToInt32(Connexion.GetInt($@"
                    SELECT {ColPriceClass} / 4 
                    FROM Class 
                    WHERE ID = {ClassID}"));
            }
            return Convert.ToInt32(Price);
        }
        public static void SetStatusAttendance(string SID , string AID, int status)
        {
            Connexion.Insert("Update Attendance_Student Set Status = "+ status +" Where StudentID = '" + SID + "' And ID = '" + AID + "'");
            if (Connexion.GetInt("Select CalcPrice from EcoleSetting") == 1)
            {
                int Ses =  Connexion.GetInt("Select Session from Attendance Where ID = " +AID);
                if(status == 3)
                {
                    Connexion.Insert("Update Attendance_Student Set Price = 0, TPrice =0  Where StudentID = '" + SID + "' And ID = '" + AID + "'");
                }
                else if (status == 2 )
                {
                    Connexion.Insert("Update Attendance_Student Set Price = 0, TPrice =0  Where StudentID = '" + SID + "' And ID = '" + AID + "'");
                }
                else if (status == 1)
                {
                    Connexion.Insert("Update Attendance_Student Set Price = dbo.GetPriceSession(" + SID + "," + AID + ") , TPrice =dbo.GetTPriceSession(" + SID + "," + AID + ")   Where StudentID = '" + SID + "' And ID = '" + AID + "'");
                }
                else if (status == 0)
                {
                    if(Connexion.GetInt("Select Absent from EcoleSetting") == 1)
                    {
                        Connexion.Insert("Update Attendance_Student Set Price = dbo.GetPriceSession(" + SID + "," + AID + ") , TPrice =dbo.GetTPriceSession(" + SID + "," + AID + ")   Where StudentID = '" + SID + "' And ID = '" + AID + "'");
                 
                    }
                    else
                    {
                        Connexion.Insert("Update Attendance_Student Set Price = 0, TPrice =0  Where StudentID = '" + SID + "' And ID = '" + AID + "'");
                    }
                }


            }
        }
        public static void FillAttendance(string AID)
        {
            string GID = Connexion.GetString("Select GroupID from Attendance Where ID =" + AID);
            int SessionA = Connexion.GetInt("Select Session from Attendance Where ID= " + AID);
            string date = Connexion.GetString("Select Date from Attendance where ID = " + AID);
            string CID = Connexion.GetClassID(GID).ToString();
            DataTable dtinsertattendance_Student = new DataTable();
            string queryforexception = $"select StudentID  from Class_Student  " +
                $"Where CONVERT(date, [StartDate], 105) <= CONVERT(date, '{date}', 105)" +
                $"AND(Stopped = '0' OR  CONVERT(date, [Stopped], 105)  >= CONVERT(date, '{date}', 105) ) and Class_Student.GroupID =  {GID} " +
                $"Except Select SID  from Attendance_Change Where FromGID = {GID} and FromDate = '{date}'";
            Connexion.FillDT(ref dtinsertattendance_Student, queryforexception);
            for (int count = 0; count < dtinsertattendance_Student.Rows.Count; count++)
            {
                Commun.InsertStuAttend(dtinsertattendance_Student.Rows[count]["StudentID"].ToString(), AID, CID, GID,date, -1);
            }
            DataTable dt = new DataTable();
            queryforexception = $"Select * from Attendance_Change Where FromGID = {GID} and FromDate = '{date}'";
            Connexion.FillDT(ref dt, queryforexception);
            foreach (DataRow row in dt.Rows)
            {
                string SID = row["StudentID"].ToString();
                Commun.InsertStuAttend(SID, AID,CID,GID,date, 2);
            }
        }

        public static void DeleteAttendance(string AID)
        {
            string GID = Connexion.GetString($"Select GroupID from Attendance Where ID = {AID}");
            string CID = Connexion.GetClassID(GID).ToString();
            Connexion.Insert("Update Groups Set Sessions = Sessions - 1 , TSessions = TSessions - 1 Where GroupID = " + GID);
            Connexion.Insert("Delete from Attendance Where ID = " + AID);
            Connexion.Insert("Delete From Attendance_Student Where ID = " + AID);
            Connexion.Insert("Delete from Justif Where AID =" + AID);
            DataTable dtAttend = new DataTable();
            Connexion.FillDT(ref dtAttend, "Select * from Attendance_Change Where ToAID = " + AID );
            foreach(DataRow row in dtAttend.Rows)
            {
                string AIDChangeFrom = Connexion.GetString($"Select ID from Attendance Where GroupID = {row["FromGID"].ToString()} and Date ='{row["FromDate"].ToString()}'");
                Commun.SetStatusAttendanceupg(row["SID"].ToString(), AIDChangeFrom,CID ,GID,row["FromDate"].ToString(), 1);

            }

        }
        public static void ChangeDateAttend(string AID,string newdate)
        {
            string GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + AID);
            string OldDate = Connexion.GetString("Select Date from Attendance Where ID = " + AID);
            string CID = Connexion.GetClassID(GID).ToString();
            newdate = newdate.Replace("/", "-");
            string queryforinsert = $"select StudentID  from Class_Student  " +
               $"Where CONVERT(date, [StartDate], 105) <= CONVERT(date, '{newdate}', 105)" +
               $"AND(Stopped = '0' OR  CONVERT(date, [Stopped], 105)  >= CONVERT(date, '{newdate}', 105) ) and Class_Student.GroupID =  {GID} " +
               $"Except Select SID from Attendance_Change Where FromGID = {GID} and FromDate = '{newdate}'";//
            DataTable dtinsertattendance_Student = new DataTable();
            Connexion.FillDT(ref dtinsertattendance_Student, queryforinsert);
            for (int count = 0; count < dtinsertattendance_Student.Rows.Count; count++)
            {
                Commun.InsertStuAttend(dtinsertattendance_Student.Rows[count]["StudentID"].ToString(), AID, CID, GID,newdate, -1);
            }
            string queryfordelete = $"select StudentID   from Class_Student  " +
              $"Where CONVERT(date, [StartDate], 105) <= CONVERT(date, '{OldDate}', 105)" +
              $"AND(Stopped = '0' OR  CONVERT(date, [Stopped], 105)  >= CONVERT(date, '{OldDate}', 105) ) and Class_Student.GroupID =  {GID} " +
              $"Except Select SID from Attendance_Change Where FromGID = {GID} and FromDate = '{OldDate}'";//
            DataTable dtDelete = new DataTable();
            Connexion.FillDT(ref dtDelete, queryfordelete);
            for (int count = 0; count < dtDelete.Rows.Count; count++)
            {
               Connexion.Insert($"Delete from Attendance_Student Where StudentID = {dtDelete.Rows[count]["StudentID"].ToString()} and ID = {AID}");
            }
            //UPDATE : you can here remove the students that has a different starting date and also discounts
            Connexion.Insert("Update Class_Student Set startDate = '" + newdate + "'Where GroupID = " + GID + " and StartDate = '" + OldDate + "'");
            Connexion.Insert($"Update Attendance Set date = '{newdate}' Where ID = {AID}");
        }

        
        public static bool CheckName(string FName , string LName , string message)
        {
            if (!Connexion.IFNULLVar("SELECT FirstName + ' ' + LastName AS MFirstName FROM Students  where dbo.ReplaceArabicName(FirstName) like  dbo.ReplaceArabicName(N'%" + FName + "%') and dbo.ReplaceArabicName(LastName) like  dbo.ReplaceArabicName(N'%" + LName + "%')"))
            {
                string name = Connexion.GetString("SELECT FirstName + ' ' + LastName AS MFirstName FROM Students  where dbo.ReplaceArabicName(FirstName) like  dbo.ReplaceArabicName(N'%" + FName + "%') and dbo.ReplaceArabicName(LastName) like  dbo.ReplaceArabicName(N'%" + LName + "%')");
                if (MessageBox.Show(message,
            "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    return true;
                }
                else
                {
                    return false; 
                }
            }
            else
            {
                return true; 
            }
        }
        public static void CheckDiscountAddClass(string SID , ResourceDictionary resources , int type ,int result )
        {
            DataTable dtType1 = new DataTable();
            int YearID = Connexion.GetInt("Select Year From Students Where ID =" + SID);
            string SpecID = Connexion.GetString("Select Case When Speciality is null then '' else Speciality end as s from students where ID = " + SID);
            DataTable dtDiscounted = new DataTable();
            dtDiscounted.Columns.Add("ClassID");
            dtDiscounted.Columns.Add("TPrice");
            dtDiscounted.Columns.Add("Price");
            dtDiscounted.Columns.Add("Ses");
            dtDiscounted.Columns.Add("Type");
            dtDiscounted.Columns.Add("ID");

            Connexion.FillDT(ref dtType1, "Select * From DiscType1 Where YearID = " + YearID + " and SpecID = " + SpecID);

            foreach (DataRow rowdisc in dtType1.Rows)
            {
                string query = "SELECT cs.StudentID  FROM class_Student cs INNER JOIN class c ON cs.ClassID = c.ID join Students on Students.ID = cs.StudentID ";
                if (SpecID != "")
                {
                    query += " Where Students.SpecID = " + SpecID + " and  ";

                }
                else
                {
                    query += "Where ";
                }
                query += "Students.Speciality = cs.StudentID = " + SID + " and c.CSubject IN (";

                int count = 0;
                if (rowdisc["Subject1"].ToString() != "")
                {
                    query += rowdisc["Subject1"].ToString();
                    count++;
                }
                if (rowdisc["Subject2"].ToString() != "")
                {
                    query += "," + rowdisc["Subject2"].ToString();
                    count++;
                }
                if (rowdisc["Subject3"].ToString() != "")
                {
                    query += "," + rowdisc["Subject3"].ToString();
                    count++;
                }
                if (rowdisc["Subject4"].ToString() != "")
                {
                    query += "," + rowdisc["Subject4"].ToString();
                    count++;
                }
                if (rowdisc["Subject5"].ToString() != "")
                {
                    query += "," + rowdisc["Subject5"].ToString();
                    count++;
                }
                if (rowdisc["Subject6"].ToString() != "")
                {
                    query += "," + rowdisc["Subject6"].ToString();
                    count++;
                }
                query += ") GROUP BY cs.StudentID HAVING COUNT(DISTINCT CASE WHEN c.CSubject = " + rowdisc["Subject1"].ToString() + " THEN 1";
                int count2 = 1;
                if (rowdisc["Subject2"].ToString() != "")
                {
                    count2++;
                    query += " WHEN c.CSubject = " + rowdisc["Subject2"].ToString() + " THEN " + count2;
                }
                if (rowdisc["Subject3"].ToString() != "")
                {
                    count2++;
                    query += " WHEN c.CSubject = " + rowdisc["Subject3"].ToString() + " THEN " + count2;
                }
                if (rowdisc["Subject4"].ToString() != "")
                {
                    count2++;
                    query += " WHEN c.CSubject = " + rowdisc["Subject4"].ToString() + " THEN " + count2;
                }
                if (rowdisc["Subject5"].ToString() != "")
                {
                    count2++;
                    query += " WHEN c.CSubject = " + rowdisc["Subject5"].ToString() + " THEN " + count2;
                }
                if (rowdisc["Subject6"].ToString() != "")
                {
                    count2++;
                    query += " WHEN c.CSubject = " + rowdisc["Subject6"].ToString() + " THEN " + count2;
                }
                query += " else 0 end )  = " + count;
                DataTable dtStudents = new DataTable();
                if (!Connexion.IFNULLVar(query))
                {
                    DataTable dtSubjects = new DataTable();
                    Connexion.FillDT(ref dtSubjects, "Select * from Class_Student Where StudentID = " + SID);
                    int yes = 0;
                    if (type == 1)
                    {
                        yes = 1;
                    }
                    foreach (DataRow rowSubjects in dtSubjects.Rows)
                    {
                        string ClassIDSubjects = rowSubjects["ClassID"].ToString();
                        string GroupIDSubjects = rowSubjects["GroupID"].ToString();
                   
                        string idsubject = Connexion.GetString("Select CSubject from class Where ID = " + ClassIDSubjects);
                        int discNumber = -1;
                        if (rowdisc["SubjectDisc1"].ToString() == idsubject)
                        {
                            discNumber = 1;
                        }
                        if (rowdisc["SubjectDisc2"].ToString() == idsubject)
                        {
                            discNumber = 2;
                        }
                        if (rowdisc["SubjectDisc3"].ToString() == idsubject)
                        {
                            discNumber = 3;
                        }
                        if (rowdisc["SubjectDisc4"].ToString() == idsubject)
                        {
                            discNumber = 4;
                        }
                        if (rowdisc["SubjectDisc5"].ToString() == idsubject)
                        {
                            discNumber = 5;
                        }
                        if (rowdisc["SubjectDisc6"].ToString() == idsubject)
                        {
                            discNumber = 6;
                        }
                        if (discNumber != -1)
                        {
                            if (yes == 0)
                            {
                                string message = string.Format((string)resources["MessageBoxStandardDiscountType1"].ToString(),
                                   Connexion.GetString("Select FirstName + ' '+ LastName from Students Where ID =" + SID), Connexion.GetString(
                                        "Select DBO.GetSubjectsDiscountType1(" + rowdisc["ID"].ToString() + " , " + SID + ")"));
                                if (MessageBox.Show(message, "Confirmation",
                               MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    yes = 1;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (!Connexion.IFNULL(
                                "Select * From Discounts " +
                                "Where StudentID = " + SID + " and ClassID = " + ClassIDSubjects + " and Done is  null "))
                            {
                                int priceNewDiscount = Connexion.GetInt("Select CPrice from Class Where ID = " + ClassIDSubjects)
                                    - int.Parse(rowdisc["price" + discNumber.ToString()].ToString());
                                int priceOldDiscount = Connexion.GetInt("Select CPrice From Discounts " +
                                "Where StudentID = " + SID + " " +
                                "and ClassID = " + ClassIDSubjects + " " +
                                "and Done is  null ");
                                if (priceOldDiscount <= priceNewDiscount)
                                {
                                    continue;
                                }
                            }
                            int CPrice = int.Parse(rowdisc["price" + discNumber.ToString()].ToString());
                            int TPrice = int.Parse(rowdisc["TPrice" + discNumber.ToString()].ToString());
                            var matchingRow = dtDiscounted.AsEnumerable().SingleOrDefault(row => row.Field<string>("ClassID") == ClassIDSubjects && row.Field<int>("Price") < CPrice);// search inside the datatable for discounts that is applied if there is one that already exists but the discount isnt the lowest one apply this

                            if (matchingRow != null)
                            {
                                matchingRow.Delete();
                                dtDiscounted.AcceptChanges(); // Call AcceptChanges to apply the deletion
                            }
                            DataRow drDiscount = dtDiscounted.NewRow();
                            drDiscount[0] = ClassIDSubjects;
                            drDiscount[1] = TPrice;
                            drDiscount[2] = CPrice;
                            int SesSubjects = -1;
                            if(result == -1)
                            {
                                OptionPanels.ThreeButtonPage page = new OptionPanels.ThreeButtonPage(resources["HowApplyDisc?"].ToString(), resources["DiscStartSes"].ToString(), resources["DiscLastPayment"].ToString(), resources["DiscFromNow?"].ToString());
                                page.ShowDialog();
                                result = page.Result;
                                if (result == 1 || result == 2 || result == 3)
                                {
                                    result = 3;
                                }
                            }
                            if (result == 1)
                            {
                                SesSubjects = Connexion.GetInt("Select Session from Class_Student Where GroupID = " + GroupIDSubjects + " and StudentID = " + SID);
                            }
                            else if ( result ==2)
                            {
                                SesSubjects = Connexion.GetInt("Select  Class_Student.Session + dbo.CalculateSesPayed(" + SID + ", " + GroupIDSubjects + ")");
                            }
                            else if ( result == 3)
                            {
                                SesSubjects = Connexion.GetInt("Select Sessions from Groups Where GroupID = " + GroupIDSubjects);
                            }
                            
                            drDiscount[3] = SesSubjects;
                            drDiscount[4] = "1";
                            drDiscount[5] = rowdisc["ID"].ToString();
                            Connexion.Insert("Insert into StudentsInStandardDiscount values(" + SID + "," + ClassIDSubjects + " , " + rowdisc["ID"].ToString() + ", 1 )");
                            dtDiscounted.Rows.Add(drDiscount);
                        }

                    }
                }
            }
            DataTable dtType2 = new DataTable();
            Connexion.FillDT(ref dtType2, "Select * from DiscType2 where YearID = " + YearID + "  order by AmmountSubjects desc");
            int AmmountSubjects = Connexion.GetInt("Select Count(*) from Class_Student Where StudentID = " + SID);
            foreach(DataRow rowdisc2 in dtType2.Rows)
            {
                if (int.Parse(rowdisc2["AmmountSubjects"].ToString()) <= AmmountSubjects)
                {
                    int Priceminus = int.Parse(rowdisc2["Price"].ToString());
                    int TPriceminus = int.Parse(rowdisc2["TPrice"].ToString());
                    if (type != 1)
                    {
                        string messageDisc2 = string.Format((string)resources["MessageBoxStandardDiscountType1"].ToString(), Connexion.GetString("Select FirstName + ' '+ LastName from Students Where ID =" + SID), rowdisc2["AmmountSubjects"].ToString() + resources["Subjects"].ToString());
                        if (MessageBox.Show(messageDisc2, "Confirmation",
                       MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        {

                            break;
                        }
                    }
                    DataTable dtClasses = new DataTable();
                    Connexion.FillDT(ref dtClasses, "Select * from Class_Student Where StudentID =" + SID);

                    foreach(DataRow rowClasses in dtClasses.Rows)
                    {
                        int PriceNew = Connexion.GetInt("Select CPrice from Class Where ID =" + rowClasses["ClassID"].ToString()) - Priceminus;
                        int TPriceNew = Connexion.GetInt("Select TPayment from Class Where ID =" + rowClasses["ClassID"].ToString()) - TPriceminus;
                        if (!Connexion.IFNULL(
                             "Select * From Discounts " +
                             "Where StudentID = " + SID + " and ClassID = " + rowClasses["ClassID"].ToString() + " and Done is  null "))
                        {
                            int priceOldDiscount = Connexion.GetInt("Select CPrice From Discounts " +
                            "Where StudentID = " + SID + " " +
                            "and ClassID = " + rowClasses["ClassID"].ToString() + " " +
                            "and Done is  null ");
                            if (priceOldDiscount <= PriceNew)
                            {
                                continue;
                            }

                        }
                        var matchingRow = dtDiscounted.AsEnumerable().SingleOrDefault(row => row.Field<object>("ClassID").ToString() == rowClasses["ClassID"].ToString() && int.Parse(row.Field<object>("Price").ToString()) > PriceNew);// search inside the datatable for discounts that is applied if there is one that already exists but the discount isnt the lowest one apply this

                        if (matchingRow != null)
                        {
                            matchingRow.Delete();
                            dtDiscounted.AcceptChanges(); // Call AcceptChanges to apply the deletion
                        }
                        DataRow drDiscount = dtDiscounted.NewRow();
                        drDiscount[0] = rowClasses["ClassID"].ToString();
                        drDiscount[1] = TPriceNew;
                        drDiscount[2] = PriceNew;

                              int SesSubjects = -1;
                        if (result == -1)
                        {
                            OptionPanels.ThreeButtonPage page = new OptionPanels.ThreeButtonPage(resources["HowApplyDisc?"].ToString(), resources["DiscStartSes"].ToString(), resources["DiscLastPayment"].ToString(), resources["DiscFromNow?"].ToString());
                            page.ShowDialog();
                            result = page.Result;
                            if (result == 1 || result == 2 || result == 3)
                            {
                                result = 3;
                            }
                        }
                        if (result == 3)
                        {
                            SesSubjects = Connexion.GetInt("Select Session from Class_Student Where GroupID = " + rowClasses["GroupID"].ToString() + " and StudentID = " + SID);
                        }
                        else if (result == 2)
                        {
                            SesSubjects = Connexion.GetInt("Select  Class_Student.Session + dbo.CalculateSesPayed(" + SID + ", " + rowClasses["GroupID"].ToString() + ")");
                        }
                        else if (result == 1)
                        {
                            SesSubjects = Connexion.GetInt("Select Sessions from Groups Where GroupID = " + rowClasses["GroupID"].ToString());
                        }
                        drDiscount[3] = SesSubjects;
                        drDiscount[4] = "2";
                        drDiscount[5] = rowdisc2["ID"].ToString();
                        dtDiscounted.Rows.Add(drDiscount);
                        Connexion.Insert("Insert into StudentsInStandardDiscount values(" + SID + "," + rowClasses["ClassID"].ToString() + " , " + rowdisc2["ID"].ToString() + ", 2 )");
                    }
                    break;
                }
            }
            foreach (DataRow rowApplyDisc in dtDiscounted.Rows)
            {
                if(!Connexion.IFNULL("Select * from discounts where ClassID = " + rowApplyDisc["ClassID"].ToString() + " " +
                    "and Done is null and studentid =" + SID))
                {
                    int CPrice = Connexion.GetInt("Select CPrice from discounts where ClassID = " + rowApplyDisc["ClassID"].ToString() + " " + "and Done is null and studentid =" + SID);
                    if(CPrice < int.Parse(rowApplyDisc["Price"].ToString()))
                    {
                        continue;
                    }
                    else
                    {
                        Connexion.Insert("Update  Discounts Set Done =  " + rowApplyDisc["Ses"].ToString() +
                          " Where StudentID = " + SID + " " +
                          "and ClassID = " + rowApplyDisc["ClassID"].ToString() + " " +
                         "and Done is  null ");
                    }
                }
                Connexion.Insert("Insert into discounts " +
                                     "values(" + SID + " , " + rowApplyDisc["ClassID"].ToString() + " , " +
                                     "" + rowApplyDisc["Price"].ToString() + " , " +
                                     "" + rowApplyDisc["TPrice"].ToString() + " , " +
                                     "N'" + resources["DiscriptionForDiscount"].ToString().Replace("'", "''") + "' , " +
                                     "" + rowApplyDisc["Ses"].ToString() + " , " +
                                     "" + rowApplyDisc["Ses"].ToString() + " , " +
                                     "null ) ");
             
            }
        }

        public static void EditRowInDataTable(ref DataTable dataTable, string ColumnConditionValue , string ColumnConditionName , string ColumnTargetValue , string ColumnTargetName)
        {
            DataRow[] rows = dataTable.Select($"{ColumnConditionName} = '{ColumnConditionValue}'");
            foreach (DataRow row in rows)
            {
                // Edit the values in the target column(s)
                row[ColumnTargetName] = ColumnTargetValue;
            }
        }

        public static int InsertAttendance(string GroupID , string GTID , string date)
        {
            Connexion.Insert("Update Groups set Sessions = Sessions + 1 , TSessions = TSessions + 1 Where GroupID = " + GroupID);
            int result = Connexion.GetInt("Select Sessions from Groups Where GroupID = '" + GroupID + "'");
            int IDRoom = Connexion.GetInt("Select IDRoom from Class_Time where ID =" + GTID);
            string TimeStart = Connexion.GetString("Select TimeStart from Class_Time where ID =" + GTID);
            string TimeEnd = Connexion.GetString("Select TimeEnd from Class_Time where ID =" + GTID);
            int AID = Connexion.GetInt("Insert into Attendance (GroupID,Date,Session,RoomID , TimeStart , TimeEND) OUTPUT Inserted.ID  Values(" + GroupID + ",'" + date + "','" + result + "' , '" + IDRoom + "','" +TimeStart +"','" +TimeEnd +"' )");
            Connexion.InsertHistory(0, AID.ToString(), 5);

            return AID;
        }

        public static int InsertAttendance(string GroupID , string date)
        {
            DataTable dtTimes = new DataTable();
            Connexion.FillDT(ref dtTimes, "Select * from Class_Time Where GID ='" + GroupID + "'");
            string TimeID = "";
            bool Found = false;
            for (int i = 0; i < dtTimes.Rows.Count; i++)
            {
                DateTime datee = DateTime.Parse(date);
                string DayID = dtTimes.Rows[i]["Day"].ToString();
                string Today = datee.DayOfWeek.ToString();
                if (Today == "Sunday")
                {
                    Today = "0";
                }
                else if (Today == "Monday")
                {
                    Today = "1";
                }
                else if (Today == "Tuesday")
                {
                    Today = "2";
                }
                else if (Today == "Wednesday")
                {
                    Today = "3";
                }
                else if (Today == "Thursday")
                {
                    Today = "4";
                }
                else if (Today == "Friday")
                {
                    Today = "5";
                }
                else if (Today == "Saturday")
                {
                    Today = "6";
                }
                if (Today == DayID)
                {
                    string GTimeBeg = Connexion.GetString(GroupID, "Class_Time", "TimeStart", "GID");
                    string GTimeEnd = Connexion.GetString(GroupID, "Class_Time", "TimeEnd", "GID");
                    int Hour = DateTime.Now.Hour;
                    int Minute = DateTime.Now.Minute;
                    TimeSpan GHourBeg = new TimeSpan();
                    GHourBeg.Add(TimeSpan.Parse(GTimeBeg));
                    TimeSpan GHourEnd = new TimeSpan();
                    GHourEnd.Add(TimeSpan.Parse(GTimeEnd));
                    TimeSpan TimeNow = new TimeSpan();
                    TimeNow.Add(TimeSpan.Parse(Hour.ToString() + ":" + Minute.ToString()));
                    if (GHourBeg <= TimeNow && TimeNow <= GHourEnd)
                    {
                        TimeID = dtTimes.Rows[i]["ID"].ToString();
                        Found = true;
                    }
                }
            }
            string GTID = "NULL";
            if (Found)
            {
                GTID = TimeID;
            }
           
            int result = Connexion.GetInt("Select Sessions from Groups Where GroupID = '" + GroupID + "'");
            Connexion.Insert("Update Groups set Sessions = Sessions + 1 , TSessions = TSessions + 1 Where GroupID = " + GroupID);
            int IDRoom = Connexion.GetInt("Select IDRoom from Class_Time where ID =" + GTID);
            string TimeStart = Connexion.GetString("Select TimeStart from Class_Time where ID =" + GTID);
            string TimeEnd = Connexion.GetString("Select TimeEnd from Class_Time where ID =" + GTID);
            int AID = Connexion.GetInt("Insert into Attendance(GroupID,Date,Session,RoomID , TimeStart,TimeEND) OUTPUT Inserted.ID  Values(" + GroupID + ",'" + DateTime.Today.ToString("d").Replace("/", "-") + "','" + result + "' , '" + IDRoom+ "','" + TimeStart+ "','" + TimeEnd + "' )");
            Connexion.InsertHistory(0, AID.ToString(), 5);
            return AID;
        }

        public static  T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T parent)
                {
                    return parent;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }

    }
}
