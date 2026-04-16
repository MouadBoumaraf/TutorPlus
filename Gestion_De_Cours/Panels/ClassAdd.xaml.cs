using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using Gestion_De_Cours.Classes;
using Gestion_De_Cours.Properties;
using System.IO;
using FastReport;
using System.Reflection;

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for ClassAdd.xaml
    /// </summary>
    public partial class ClassAdd : Window
    {
        string Type;
        string ClassID;
        string TeacherSID;
        string S;
        Boolean isSpec;
        DataTable dtGroups = new DataTable();
        private bool isComboBoxDisabled = false;
        string queryAttendance = @"
            SELECT 
                'NRML' AS Type,  
                Class.CName + ' ' + Groups.GroupName AS GroupName, 
                Attendance.ID AS AttendanceID, 
                dbo.CheckEmptyAttendanceAttend(Attendance.ID) AS AttendEmpty, 
                Attendance.TimeStart AS Time, 
                CONVERT(NVARCHAR(50), Attendance.Session) AS Session,
                Attendance.Date as Date,
                CONVERT(DATE, Attendance.Date, 105) AS DateOrder, 
                dbo.TotalStudentsAttendance(Attendance.ID) AS TotalStudent  
            FROM 
                Attendance 
            JOIN 
                Groups ON Attendance.GroupID = Groups.GroupID 
            JOIN 
                Class ON Groups.ClassID = Class.ID 
            WHERE 
                1 = 1 
                AND {FilterCondition} 
            UNION 

            SELECT 
                'EXTRA' AS Type, 
                Class.CName AS GroupName, 
                AE.ID AS AttendanceID,  
                0 AS AttendEmpty, 
                AE.TimeStart AS Time, 
                N'حصة إضافية' AS Session, 
                AE.Date as Date,
                CONVERT(DATE,AE.Date, 105) AS DateOrder, 
                (SELECT COUNT(*) FROM Attendance_Extra_Students WHERE ID = AE.ID) AS TotalStudent 
            FROM 
                Attendance_Extra AE 
            JOIN 
                Class ON AE.CID = Class.ID 
            WHERE 
                AE.CID = {CID}  
           ORDER BY DateOrder DESC;
            ";
        public ClassAdd(string TypeUC, string ID, string Setting)
        {
            try
            {
                int lang = Connexion.Language();
                InitializeComponent();
                SetLang();
                if (lang == 1)
                {
                    this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
                }

                S = Setting;
                if (Setting == "Single")
                {
                    SPGender.Visibility = Visibility.Visible;
                    Groups.Visibility = Visibility.Collapsed;
                    SPGroupTime.Visibility = Visibility.Collapsed;
                    SPGroupStudent.Visibility = Visibility.Collapsed;
                    SPGroupAttendance.Visibility = Visibility.Collapsed;
                    DGCGroupS.Visibility = Visibility.Collapsed;
                    DGCGroupT.Visibility = Visibility.Collapsed;
                    DGCGroupA.Visibility = Visibility.Collapsed;
                }
                Connexion.FillCB(ref CLevel, "Select * from Levels");
                Connexion.FillDTItem("Rooms", ref CRoom);
                Type = TypeUC;
                DataTable DTTeacher = new DataTable();
                Connexion.FillDT(ref DTTeacher, "Select ID,TFirstName + ' ' + TLastName As Name ,TGender,'" + Connexion.GetImagesFile() + "\\" + "T' + Convert(Varchar(50),ID)  + '.jpg' as Picture From Teacher where status = 1");
                DataRow newRow = DTTeacher.NewRow();

                // Assign values to the new row
                newRow["ID"] = -1;
                newRow["Name"] = "New Teacher";
                newRow["TGender"] = -1;
                newRow["Picture"] = "";
                DTTeacher.Rows.Add(newRow);
                TComboBox.ItemsSource = DTTeacher.DefaultView;
                if (Type == "Show")
                {
                    ClassID = ID;
                    TeacherSID = Connexion.GetString("Select TID from Class Where iD = " + ID);
                    Connexion.FillDT(ref dtGroups, "Select * from Groups Where ClassID  = " + ClassID);
                    CLevel.SelectedValue = Connexion.GetInt(ClassID, "Class", "CLevel");
                    CYear.SelectedValue = Connexion.GetInt(ClassID, "Class", "CYear");
                    TComboBox.SelectedValue = Connexion.GetInt(ClassID, "Class", "TID");
                    SubjectC.SelectedValue = Connexion.GetInt(ClassID, "Class", "CSubject");
                    CPrice.Text = Connexion.GetString(ClassID, "Class", "CPrice");
                    TPaymentMethod.SelectedIndex = Connexion.GetInt(ClassID, "Class", "TPaymentMethod");
                    TPayment.Text = Connexion.GetString(ClassID, "Class", "TPayment");
                    Connexion.FillDataGrid(ClassID, ref DGSpec, "Class_Speciality");
                    Connexion.FillDG(ref DGSpec, "Select Class_Speciality.ID , Class_Speciality.SpecID as SpecID , Specialities.Speciality from Class_Speciality JOIN Specialities ON Class_Speciality.SpecID = Specialities.ID WHERE Class_Speciality.ID = " + ClassID);
                    queryAttendance = queryAttendance.Replace("{CID}", ClassID);
                    FillDG("Students",$"WHERE Class_Student.ClassID = {ClassID}");
                    Connexion.FillDG(ref DGAddTime, "Select Class_Time.GID as ID ,Class_Time.Day as DayID ,  Case   " +
                           "  When Class_Time.Day = 0 Then N'" + this.Resources["Sunday"].ToString() + "' " +
                           "  When Class_Time.Day = 1 Then N'" + this.Resources["Monday"].ToString() + "' " +
                           "  When Class_Time.Day = 2 Then N'" + this.Resources["Tuesday"].ToString() + "'" +
                           "  When Class_Time.Day = 3 Then N'" + this.Resources["Wednesday"].ToString() + "'" +
                           "  When Class_Time.Day = 4 Then N'" + this.Resources["Thursday"].ToString() + "' " +
                           "  When Class_Time.Day = 5 Then N'" + this.Resources["Friday"].ToString() + "'" +
                           "  When Class_Time.Day = 6 Then N'" + this.Resources["Saturday"].ToString() + "' " +
                            " End  as Day , Class_Time.TimeStart as TimeStart, Class_Time.TimeEnd As TimeEnd ,Rooms.Room As Room , Groups.GroupName as GroupName , Groups.GroupID , Rooms.ID as IDRoom From Class_Time Join Rooms On Class_Time.IDRoom = Rooms.ID JOIN Groups ON Groups.GroupID = Class_Time.GID Where Class_time.Type = 1 and Groups.ClassID = " + ClassID);

                    Connexion.FillDG(ref DGGroups, "Select GroupID, GroupName, CASE   WHEN GroupGender = 0 THEN N'" + Properties.Resources.Male + "'  WHEN GroupGender = 1 THEN N'" + Properties.Resources.Female + "' When GroupGender = 2 Then N'" + Properties.Resources.Mix + "' END as GroupGenderr, GroupGender From Groups WHERE ClassID = " + ClassID);
                    Connexion.FillCB(ref CBGroups, "Select * from groups Where ClassID = " + ClassID);
                    CBGroups.SelectedIndex = 0;
                    Connexion.FillCB(ref CBGroupsT, "Select * from groups Where ClassID = " + ClassID);
                    Connexion.FillCB(ref CBGroupsA, "Select * FROM groups WHERE ClassID = " + ClassID);
                    
                    string queryAttend = queryAttendance.Replace("{FilterCondition}", "Class.ID = " + ClassID);
                    Connexion.FillDG(ref DGAttendance, queryAttend);
                    if (S == "Single")
                    {
                        int GID = Connexion.GetInt(ClassID, "Groups", "GroupID", "ClassID");
                        DataTable dtTime = new DataTable();

                        Connexion.FillDT(ref dtTime, "SELECT *,Case " +
                     "  When Class_Time.Day = 0 Then N'" + Properties.Resources.Sunday + "' + TimeStart" +
                     "  When Class_Time.Day = 1 Then N'" + Properties.Resources.Monday + "' + TimeStart" +
                     "  When Class_Time.Day = 2 Then N'" + Properties.Resources.Tuesday + "' + TimeStart" +
                     "  When Class_Time.Day = 3 Then N'" + Properties.Resources.Wednesday + "' + TimeStart" +
                     "  When Class_Time.Day = 4 Then N'" + Properties.Resources.Thursday + "' + TimeStart" +
                     "  When Class_Time.Day = 5 Then N'" + Properties.Resources.Friday + "' + TimeStart" +
                     "  When Class_Time.Day = 6 Then N'" + Properties.Resources.Saturday + "' + TimeStart " +
                     "  End As Time  FROM Class_Time WHERE Class_time.Type = 1 and  CLass_Time.GID=" + GID);
                        DataRow newRowT = dtTime.NewRow();
                        newRowT["Time"] = this.Resources["ExtraSession"];
                        newRowT["ID"] = -1;
                        dtTime.Rows.Add(newRowT);
                        CBGroupT.ItemsSource = dtTime.DefaultView;
                        GenderC.SelectedIndex = Connexion.GetInt(GID.ToString(), "Groups", "GroupGender", "GroupID");
                    }
                    CName.Text = Connexion.GetString(ClassID, "Class", "CName");
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "ClassM") != 1)
                    {
                        CLevel.IsEnabled = false;
                        CYear.IsEnabled = false;
                        SubjectC.IsEnabled = false;
                        CName.IsReadOnly = true;
                        TComboBox.IsEnabled = false;
                        TPayment.IsReadOnly = true;
                        TPaymentMethod.IsEnabled = false;
                        CPrice.IsReadOnly = true;

                    }
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "TPayV") != 1)
                    {
                        TPayment.Text = "";
                        TPaymentMethod.SelectedIndex = -1;
                    }
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "TPayM") != 1)
                    {
                        TPaymentMethod.IsEnabled = false;
                        TPayment.IsEnabled = false;
                    }
                }
                else if (Type == "Add")
                {
                    TabSpec.IsEnabled = false;
                    Groups.IsEnabled = false;
                    GroupTime.IsEnabled = false;
                    Students.IsEnabled = false;
                    Attandence.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FillDG(string type ,string Condition)
        {
            if(type == "Students")
            {
                DataRowView row = (DataRowView)CYear.SelectedItem;
                
                Connexion.FillDG(ref DGStudent,
               "SELECT " +
               "Class_Student.ClassID, " +
               "Class_Student.StudentID, " +
               "Class_Student.GroupID, " +
               "Groups.GroupName, " +
               "Class_Student.StartDate, " +
               "Class_Student.Stopped, " +
               "Students.FirstName, " +
               "Students.LastName  " +
               "FROM Class_Student " +
               "JOIN Students ON Class_Student.StudentID = Students.ID " +
               "JOIN Groups ON Groups.GroupID = Class_Student.GroupID " + Condition ) ;
            }
            else if (type == "Attendance")
            {

                string queryAttend = queryAttendance.Replace("{FilterCondition}", "Class.ID = " + ClassID);
                Connexion.FillDG(ref DGAttendance, queryAttend);
            }
        }
        private void Button_Click_AddTime(object sender, RoutedEventArgs e) // Getting Class Time
        {
            try
            {
                DataRowView rowHFrom = (DataRowView)HFrom.SelectedValue;
                DataRowView rowMFrom = (DataRowView)MFrom.SelectedValue;
                DataRowView rowHTo = (DataRowView)HTo.SelectedValue;
                DataRowView RowMTo = (DataRowView)MTo.SelectedValue;
                bool isrepeated = true;
                if (CDay.SelectedIndex != -1 && rowHFrom != null && rowMFrom != null && rowHTo != null && RowMTo != null)
                {
                    string Start = rowHFrom["Hour"].ToString() + ":" + rowMFrom["Hour"].ToString();
                    string End = rowHTo["Hour"].ToString() + ":" + RowMTo["Hour"].ToString();
                    if (S != "Single")
                    {
                        if (CBGroupsT.SelectedIndex == -1)
                        {
                            MessageBox.Show("Please Fill All Comboboxes");
                            return;
                        }
                    }
                    if (isrepeated)
                    {
                        DataRowView rowRoom = (DataRowView)CRoom.SelectedItem;
                        string RID = rowRoom["ID"].ToString();
                        string GID;
                        if (S == "Multiple")
                        {
                            DataRowView Group = (DataRowView)CBGroupsT.SelectedItem;
                            GID = Group["GroupID"].ToString();
                        }
                        else
                        {
                            GID = Connexion.GetInt(ClassID, "Groups", "GroupID", "ClassID").ToString();
                        }
                        Connexion.Insert("Insert into Class_Time Values ('" + GID + "','" + RID + "','" + CDay.SelectedIndex + "','" + Start + "','" + End + "',1,null)");
                        DateTime currentDate = DateTime.Now;
                        if (CDay.SelectedIndex == (int)currentDate.DayOfWeek)
                        {
                            DateTime today = DateTime.Today;

                            // Format the date as "dd-MM-yyyy"
                            string formattedDate = today.ToString("dd-MM-yyyy");
                            if (Connexion.IFNULL("Select ID from attendance where Attendance.Date = '" + formattedDate + "'	and GroupID = " + GID))
                            {
                                MainWindow mainWindow = null;
                                foreach (Window window in Application.Current.Windows)
                                {
                                    Type type = typeof(MainWindow);
                                    if (window != null && window.DependencyObjectType.Name == type.Name)
                                    {
                                        mainWindow = (MainWindow)window;
                                        if (mainWindow != null)
                                        {
                                            break;
                                        }
                                    }
                                }
                                mainWindow.UpdateAllAttendance();

                            }
                        }

                        Connexion.FillCB(ref CBGroupsA, "Select * FROM groups WHERE ClassID = " + ClassID);
                        if (S == "Single")
                        {
                            DataTable dtTime = new DataTable();

                            Connexion.FillDT(ref dtTime, "SELECT *,Case " +
                         "  When Class_Time.Day = 0 Then N'" + Properties.Resources.Sunday + "' + TimeStart" +
                         "  When Class_Time.Day = 1 Then N'" + Properties.Resources.Monday + "' + TimeStart" +
                         "  When Class_Time.Day = 2 Then N'" + Properties.Resources.Tuesday + "' + TimeStart" +
                         "  When Class_Time.Day = 3 Then N'" + Properties.Resources.Wednesday + "' + TimeStart" +
                         "  When Class_Time.Day = 4 Then N'" + Properties.Resources.Thursday + "' + TimeStart" +
                         "  When Class_Time.Day = 5 Then N'" + Properties.Resources.Friday + "' + TimeStart" +
                         "  When Class_Time.Day = 6 Then N'" + Properties.Resources.Saturday + "' + TimeStart " +
                         "  End As Time  FROM Class_Time WHERE Class_time.Type = 1 and  CLass_Time.GID=" + GID);
                            DataRow newRow = dtTime.NewRow();
                            newRow["Time"] = this.Resources["ExtraSession"];
                            newRow["ID"] = -1; 
                            dtTime.Rows.Add(newRow);
                            CBGroupT.ItemsSource = dtTime.DefaultView;
                        }
                        Connexion.FillDG(ref DGAddTime, "Select Class_Time.GID as GID, Class_Time.ID as ID ,Class_Time.Day as DayID , " +
                           " Case  When Class_Time.Day = 0 Then N'" + this.Resources["Sunday"].ToString() + "' " +
                           "  When Class_Time.Day = 1 Then N'" + this.Resources["Monday"].ToString() + "' " +
                           "  When Class_Time.Day = 2 Then N'" + this.Resources["Tuesday"].ToString() + "'" +
                           "  When Class_Time.Day = 3 Then N'" + this.Resources["Wednesday"].ToString() + "'" +
                           "  When Class_Time.Day = 4 Then N'" + this.Resources["Thursday"].ToString() + "' " +
                           "  When Class_Time.Day = 5 Then N'" + this.Resources["Friday"].ToString() + "'" +
                           "  When Class_Time.Day = 6 Then N'" + this.Resources["Saturday"].ToString() + "' " +
                            " End  as Day , Class_Time.TimeStart as TimeStart, Class_Time.TimeEnd As TimeEnd ,Rooms.Room As Room , Groups.GroupName as GroupName , Groups.GroupID , Rooms.ID as IDRoom From Class_Time Join Rooms On Class_Time.IDRoom = Rooms.ID JOIN Groups ON Groups.GroupID = Class_Time.GID Where Class_time.Type = 1  and Groups.GroupID = " + GID);
                        MTo.SelectedIndex = -1;
                        HTo.SelectedIndex = -1;
                        MFrom.SelectedIndex = -1;
                        HFrom.SelectedIndex = -1;
                        CDay.SelectedIndex = -1;
                        CRoom.SelectedIndex = -1;

                    }
                }
                else
                {
                    MessageBox.Show("Please Fill all the combobox");
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void ClickSpec(object sender, RoutedEventArgs e)
        {
            try
            {
                string speciality;
                bool Istrue = true;
                DataRowView vrow = (DataRowView)CBSpeciality.SelectedItem;
                string r = vrow["ID"].ToString();
                for (int i = 0; i < DGSpec.Items.Count; i++)
                {
                    speciality = ((DataRowView)DGSpec.Items[i]).Row["SpecID"].ToString();
                    if (r == speciality)
                    {
                        Istrue = false;
                        MessageBox.Show("This Register is already inserted");
                    }
                }
                if (Istrue)
                {
                    string Specid;
                    DataRowView row = (DataRowView)CYear.SelectedItem;
                    Connexion.Insert("Insert into Class_Speciality Values ('" + ClassID + "','" + vrow["ID"].ToString() + "')");
                    Connexion.FillDG(ref DGSpec, "Select Class_Speciality.ID , Class_Speciality.SpecID , Specialities.Speciality from Class_Speciality JOIN Specialities ON Class_Speciality.SpecID = Specialities.ID WHERE Class_Speciality.ID = " + ClassID);
                    if (row != null) // combobox fill for students depending on the specialities Added
                    {
                        string yearid = row["ID"].ToString();
                        string query = "SELECT * ,(FirstName + LastName) as Name from Students Where Year = " + yearid;
                        for (int i = 0; i < DGSpec.Items.Count; i++)
                        {
                            Specid = ((DataRowView)DGSpec.Items[i]).Row["SpecID"].ToString();
                            if (i == 0)
                            {
                                query += "AND SPECIALITY = " + Specid;
                            }
                            else
                            {
                                query += "OR SPECIALITY = " + Specid;
                            }
                        }
                        query += "  ORDER BY Name ASC";
                        Connexion.FillCB(ref CBStudent, query);
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }
        private void BtnDeleteTime_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string Day = ((DataRowView)DGAddTime.SelectedItem).Row["DayID"].ToString();
                string TimeStart = ((DataRowView)DGAddTime.SelectedItem).Row["TimeStart"].ToString();
                string TimeEnd = ((DataRowView)DGAddTime.SelectedItem).Row["TimeEnd"].ToString();
                string Room = ((DataRowView)DGAddTime.SelectedItem).Row["IDRoom"].ToString();
                string ID = ((DataRowView)DGAddTime.SelectedItem).Row["ID"].ToString();
                Connexion.Insert("Delete From Class_time Where ID = '" + ID + "'");
                Connexion.FillDG(ref DGAddTime, "Select Class_Time.ID as ID, Class_Time.GID as GID ,Class_Time.Day as DayID, Case When Class_Time.Day = 0 Then N'" + this.Resources["Sunday"].ToString() + "' " +
                           "  When Class_Time.Day = 1 Then N'" + this.Resources["Monday"].ToString() + "' " +
                           "  When Class_Time.Day = 2 Then N'" + this.Resources["Tuesday"].ToString() + "'" +
                           "  When Class_Time.Day = 3 Then N'" + this.Resources["Wednesday"].ToString() + "'" +
                           "  When Class_Time.Day = 4 Then N'" + this.Resources["Thursday"].ToString() + "' " +
                           "  When Class_Time.Day = 5 Then N'" + this.Resources["Friday"].ToString() + "'" +
                           "  When Class_Time.Day = 6 Then N'" + this.Resources["Saturday"].ToString() + "' " +
                            " End  as Day , " +
                         "Class_Time.TimeStart as TimeStart," +
                         "Class_Time.TimeEnd As TimeEnd ," +
                         "Rooms.Room As Room , " +
                         "Groups.GroupName as GroupName , " +
                         "Groups.GroupID , " +
                         "Rooms.ID as IDRoom " +
                         "From Class_Time " +
                         "Join Rooms On Class_Time.IDRoom = Rooms.ID " +
                         "JOIN Groups ON Groups.GroupID = Class_Time.GID " +
                         "Where  Class_time.Type = 1 and Groups.ClassID = " + ClassID);
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }
        private void BtnDeleteSpec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string Specid;
                DataRowView row = (DataRowView)CYear.SelectedItem;
                Specid = ((DataRowView)DGSpec.SelectedItem).Row["SpecID"].ToString();
                Connexion.Insert("Delete From Class_Speciality Where ID = '" + ClassID + "' And SpecID = '" + Specid + "'");
                Connexion.FillDG(ref DGSpec, "Select Class_Speciality.ID , Class_Speciality.SpecID , Specialities.Speciality from Class_Speciality JOIN Specialities ON Class_Speciality.SpecID = Specialities.ID WHERE Class_Speciality.ID = " + ClassID);
                if (row != null) // combobox fill for students depending on the specialities Added
                {
                    if (DGSpec.Items.Count != 0)
                    {
                        string yearid = row["ID"].ToString();
                        string query = "SELECT * ,(FirstName + LastName) as Name from Students  Where Year = " + yearid;
                        for (int i = 0; i < DGSpec.Items.Count; i++)
                        {
                            Specid = ((DataRowView)DGSpec.Items[i]).Row["SpecID"].ToString();
                            if (i == 0)
                            {
                                query += "AND SPECIALITY = " + Specid;
                            }
                            else
                            {
                                query += "OR SPECIALITY = " + Specid;
                            }
                        }
                        query += "  ORDER BY Name ASC";
                        Connexion.FillCB(ref CBStudent, query);
                    }
                    else
                    {
                        CBStudent.ItemsSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) // add student and Groups
        {
            try
            {

                DataRowView vrow;
                DataRow row;
                string ID;
                bool Istrue = true;
                vrow = (DataRowView)CBStudent.SelectedItem;
                if (CBStudent.SelectedIndex == -1)
                {
                    return;
                }
                row = vrow.Row;

                if (row == null)
                {

                    return;
                }
                if (Istrue)
                {
                   
                    ID = row["ID"].ToString();
                    vrow = (DataRowView)CBStudent.SelectedItem;
                    row = vrow.Row;
                    string GID;
                    string SID = row["ID"].ToString();
                    if (S == "Single")
                    {
                        GID = Connexion.GetInt(ClassID, "Groups", "GroupID", "ClassID").ToString();
                    }
                    else
                    {
                        DataRowView Group = (DataRowView)CBGroups.SelectedItem;
                        GID = Group["GroupID"].ToString();
                    }
                    if(!Commun.CheckSeatsClass(GID, this.Resources["WarningSeatsMax"].ToString()))
                    {
                        return;
                    }
                   
                    int ses = Connexion.GetInt(GID, "Groups", "Sessions", "GroupID");
                    if (Connexion.IFNULL("Select * from Class_Student Where StudentID = " + SID + " and ClassID = " + ClassID))
                    {
                        Connexion.Insert("Insert into Class_Student Values ('" + SID + "','" + ClassID + "' ,  '" + GID + "'," + ses + ",NULL,0,0 )");
                        int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                        if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                        {
                            int YID = Connexion.GetInt("Select CYear from Class Where ID = " + ClassID);
                            monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                        }

                        if (monthlypayment == 1)
                        {
                            Methods.InsertStudentClassMonthly(SID, ClassID);
                        }
                        else
                        {
                            Commun.CheckDiscountAddClass(SID, this.Resources, 0, -1);
                        }
                    }
                    else
                    {
                        int OldGroupID = Connexion.GetInt("Select GroupID from Class_Student where StudentID = " + SID + " and ClassID = " + ClassID);
                        if (OldGroupID.ToString() == GID)
                        {
                            int EndSession = Connexion.GetInt("Select EndSession from Class_Student where StudentID = " + SID + " and ClassID = " + ClassID) + 1;
                            int EndGroup = Connexion.GetInt("Select Sessions from Groups Where GroupID = " + GID);
                            for (int f = EndSession; EndSession <= EndGroup; EndSession++)
                            {
                                int AID = Connexion.GetInt("Select ID from Attendance Where GroupID = " + GID + " and Session =  " + f);
                                Connexion.Insert("Insert into Attendance_Student Values (" + AID + " , " + SID + " , 3, 'Group Quit'");
                                Commun.SetStatusAttendance(SID, AID.ToString(), 3);
                            }
                            Connexion.Insert("Update Class_Student Set EndSession = null Where StudentID = " + SID + " and GroupID = " + GID);
                        }
                        else
                        {
                            DataTable dt = new DataTable();
                            Connexion.FillDT(ref dt, "Select * from Attendance join Attendance_Student on Attendance.ID = Attendance_Student.ID where StudentID = " + SID + " and Attendance.GroupID = " + OldGroupID + " order by session");
                            string status;
                            int attendance;
                            int attendanceNew;
                            foreach (DataRow OldAttendancerow in dt.Rows)
                            {
                                status = OldAttendancerow["Status"].ToString();
                                if (status == "0")
                                {
                                    attendance = Connexion.GetInt("Select ID from Attendance Where GroupID = " + OldGroupID + " and Session = " + OldAttendancerow["Session"].ToString());
                                    attendanceNew = Connexion.GetInt("Select ID from Attendance Where GroupID = " + GID + " and Session = " + OldAttendancerow["Session"].ToString());
                                    Connexion.Insert("Delete from Attendance_Student Where ID = " + attendance + " and StudentID = " + SID);
                                    Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status,Note) Values( " + attendanceNew + " ," + SID + " , 0,'')");
                                    Commun.SetStatusAttendance(SID, attendanceNew.ToString(), 0);

                                }
                                else if (status == "1")
                                {
                                    Connexion.Insert("insert into Attendance_Change " +
                                        "values(" + SID + " ," + GID + " , " + OldGroupID + " , " + OldAttendancerow["Session"].ToString() + " )");
                                    attendance = Connexion.GetInt("Select ID from Attendance Where GroupID = " + GID + " and Session = " + OldAttendancerow["Session"].ToString());
                                    Connexion.Insert("Insert into attendance_Student(ID,StudentID,Status,Note) values (" + attendance + " , " + SID + " , 2 ,'')");
                                    Commun.SetStatusAttendance(SID, attendance.ToString(), 2);

                                }
                                else if (status == "2")
                                {
                                    int toGroupID = Connexion.GetInt("Select ToGroupID from Attendance_Change where StudentID = " + SID + "and FromGroupID = " + OldGroupID + " and Session = " + OldAttendancerow["Session"].ToString());
                                    int AttendanceID = Connexion.GetInt("Select ID from Attendance Where GroupID=" + GID + " and Session = " + OldAttendancerow["Session"].ToString());
                                    if (toGroupID.ToString() == GID)
                                    {
                                        Connexion.Insert("delete from attendance_Change Where StudentID = " + SID + " and Session = " + OldAttendancerow["Session"].ToString() + " and fromgroupID = " + OldGroupID);
                                        Connexion.Insert("Insert into attendance_Student(ID,StudentID,Status,Note) values (" + AttendanceID + " ,  " + SID + " ,1,'')");
                                        Commun.SetStatusAttendance(SID, AttendanceID.ToString(), 1);
                                    }
                                    else
                                    {
                                        Connexion.Insert("update attendance_Change set FromGroupID = " + GID + " Where StudentID = " + SID + " and Session = " + OldAttendancerow["Session"].ToString() + " and FromgroupID  = " + OldGroupID);
                                        Connexion.Insert("Insert into attendance_Student(ID,StudentID,Status,Note) values (" + AttendanceID + " ,  " + SID + " ,2,'')");
                                        Commun.SetStatusAttendance(SID, AttendanceID.ToString(), 2);

                                    }
                                }
                                else if (status == "3")
                                {
                                    attendance = Connexion.GetInt("Select ID from Attendance Where GroupID = " + OldGroupID + " and Session = " + OldAttendancerow["Session"].ToString());
                                    attendanceNew = Connexion.GetInt("Select ID from Attendance Where GroupID = " + GID + " and Session = " + OldAttendancerow["Session"].ToString());
                                    Connexion.Insert("Delete from Attendance_Student Where ID = " + attendance + " and StudentID = " + SID);
                                    Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status,Note) Values( " + attendanceNew + " ," + SID + " , 3,'')");
                                    Commun.SetStatusAttendance(SID, attendanceNew.ToString(), 3);
                                }
                            }
                            ses = Connexion.GetInt("Select Session from Class_Student Where StudentID = " + SID + " and ClassID = " + ClassID);
                            int EndSession = Connexion.GetInt("Select EndSession from Class_Student where StudentID = " + SID + " and ClassID = " + ClassID) + 1;
                            int EndGroup = Connexion.GetInt("Select Sessions from Groups Where GroupID = " + GID);
                            for (int f = EndSession; EndSession <= EndGroup; EndSession++)
                            {
                                int AID = Connexion.GetInt("Select ID from Attendance Where GroupID = " + GID + " and Session =  " + f);
                                Connexion.Insert("Insert into Attendance_Student(ID,studentID,Status,Note) Values (" + AID + " , " + SID + " , 3, 'Group Quit'");
                                Commun.SetStatusAttendance(SID, AID.ToString(), 3);
                            }
                            Connexion.Insert("Delete from Class_Student Where StudentID = " + SID + " and ClassID = " + ClassID);

                            Connexion.Insert("Insert into Class_Student Values ('" + SID + "','" + ClassID + "' ,  '" + GID + "'," + ses + ",NULL,0,0 )");
                        }
                    }
                    // Connexion.InsertHistory(0, SID, GID ,6 );
                    DataRowView rowCY = (DataRowView)CYear.SelectedItem;
                    string yearid = rowCY["ID"].ToString();
                    string Specid;
                    string query = "SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students  Where Year = " + yearid;
                    for (int i = 0; i < DGSpec.Items.Count; i++)
                    {
                        Specid = ((DataRowView)DGSpec.Items[i]).Row["SpecID"].ToString();
                        if (i == 0)
                        {
                            query += "AND SPECIALITY = " + Specid;
                        }
                        else
                        {
                            query += "OR SPECIALITY = " + Specid;
                        }
                    }
                    string gender;
                    if (S == "Multiple")
                    {
                        DataRowView row2 = (DataRowView)CBGroups.SelectedItem;
                        gender = row2["GroupGender"].ToString();
                    }
                    else
                    {
                        gender = GenderC.SelectedIndex.ToString();
                    }
                    if (gender == "1" || gender == "0")
                    {
                        query += "AND Students.Gender =" + gender;
                    }
                    query += " Except SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students Join Class_Student On Class_Student.StudentID = Students.ID  Where Class_Student.ClassID = " + ClassID + "  ORDER BY Name ASC";
                    Connexion.FillCB(ref CBStudent, query);
                    string groupIDquery = " And Class_Student.GroupID = ";
                    if (S == "Single")
                    {
                        groupIDquery += Connexion.GetString($"Select GroupID from Groups Where ClassID = {ClassID}");
                    }
                    else
                    {
                        DataRowView rowGroup = (DataRowView)CBGroups.SelectedItem;
                        if (rowGroup == null)
                        {
                            groupIDquery = "";
                        }
                        else
                        {
                            groupIDquery += rowGroup["GroupID"].ToString();
                        }
                    }
                    FillDG("Students", $"WHERE Class_Student.ClassID = '" + ClassID + "'" + groupIDquery);
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }


        private void CYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)CYear.SelectedItem;
                if (row != null)
                {
                    string yearid = row["ID"].ToString();
                    Connexion.FillDTItem("Subjects Where YearID = " + yearid, ref SubjectC);
                    if (S == "Single")
                    {
                        Connexion.FillCB(ref CBStudent, "SELECT * ,(FirstName + LastName) as Name from Students  Where Year = " + yearid + "  ORDER BY Name ASC");
                    }
                    Connexion.FillCB(ref CBSpeciality, "Select * from Specialities Where YearID = '" + yearid + "'");
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }
        private void BtnDeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to delete this group?",
                         "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string GID = ((DataRowView)DGGroups.SelectedItem).Row["GroupID"].ToString();
                    Connexion.Insert("Delete from Groups Where GroupID=" + GID);
                    Connexion.Insert("delete from Class_Time Where GID = " + GID);
                    Connexion.Insert("Delete From Class_Student Where GroupID=" + GID);
                    Connexion.FillDG(ref DGGroups, "Select GroupID, GroupName, CASE   WHEN GroupGender = 0 THEN N'" + Properties.Resources.Male + "'  WHEN GroupGender = 1 THEN N'" + Properties.Resources.Female + "' When GroupGender = 2 Then N'" + Properties.Resources.Mix + "' END as GroupGenderr, GroupGender From Groups WHERE ClassID = " + ClassID);
                    Connexion.FillDG(ref DGAddTime, "Select Class_Time.ID as ID , Class_Time.GID as GID,Class_Time.Day as DayID,  Case   When Class_Time.Day = 0 Then N'" + this.Resources["Sunday"].ToString() + "' " +
                               "  When Class_Time.Day = 1 Then N'" + this.Resources["Monday"].ToString() + "' " +
                               "  When Class_Time.Day = 2 Then N'" + this.Resources["Tuesday"].ToString() + "'" +
                               "  When Class_Time.Day = 3 Then N'" + this.Resources["Wednesday"].ToString() + "'" +
                               "  When Class_Time.Day = 4 Then N'" + this.Resources["Thursday"].ToString() + "' " +
                               "  When Class_Time.Day = 5 Then N'" + this.Resources["Friday"].ToString() + "'" +
                               "  When Class_Time.Day = 6 Then N'" + this.Resources["Saturday"].ToString() + "' " +
                                " End  as Day , " +
                             "Class_Time.TimeStart as TimeStart," +
                             "Class_Time.TimeEnd As TimeEnd ," +
                             "Rooms.Room As Room , " +
                             "Groups.GroupName as GroupName , " +
                             "Groups.GroupID , " +
                             "Rooms.ID as IDRoom " +
                             "From Class_Time " +
                             "Join Rooms On Class_Time.IDRoom = Rooms.ID " +
                             "JOIN Groups ON Groups.GroupID = Class_Time.GID " +
                             "Where Class_time.Type = 1 and Groups.ClassID = " + ClassID);
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click_AddGroup(object sender, RoutedEventArgs e)
        {
            try
            {
                int Gender = -1;
                if (Male.IsChecked == true)
                {
                    Gender = 0;
                }
                else if (Female.IsChecked == true)
                {
                    Gender = 1;
                }
                else if (Mix.IsChecked == true)
                {
                    Gender = 2;
                }
                else
                {
                    MessageBox.Show("Please Choose A gender for the group ");
                    return;
                }
                if (TBGN.Text == "")
                {
                    MessageBox.Show("Please Insert Group Name");
                    return;
                }
                Class.Group G = new Class.Group();
                G.GroupName = TBGN.Text;
                G.GroupGender = Gender;
                Connexion.Insert("Insert into Groups(ClassID,GroupName,GroupGender,Sessions , TSessions) VALUES('" + ClassID + "',N'" + G.GroupName + "','" + G.GroupGender + "',0,0)");
                Connexion.FillDT(ref dtGroups, "Select * from Groups Where ClassID  = " + ClassID);
                string groupid = Connexion.GetID("Groups");
                //  Connexion.InsertHistory(0, groupid, 4);
                Connexion.FillCB(ref CBGroupsT, "Select GroupID, GroupName, CASE   WHEN GroupGender = 0 THEN N'" + Properties.Resources.Male + "'  WHEN GroupGender = 1 THEN N'" + Properties.Resources.Female + "' When GroupGender = 2 Then N'" + Properties.Resources.Mix + "' END as GroupGender From Groups WHERE ClassID = " + ClassID);
                Connexion.FillCB(ref CBGroups, "Select GroupID, GroupName,  CASE   WHEN GroupGender = 0 THEN N'" + Properties.Resources.Male + "'  WHEN GroupGender = 1 THEN N'" + Properties.Resources.Female + "' When GroupGender = 2 Then N'" + Properties.Resources.Mix + "' END as GroupGenderr , GroupGender From Groups WHERE ClassID = " + ClassID);
                Connexion.FillCB(ref CBGroupsA, "Select GroupID, GroupName, CASE   WHEN GroupGender = 0 THEN N'" + Properties.Resources.Male + "'  WHEN GroupGender = 1 THEN N'" + Properties.Resources.Female + "' When GroupGender = 2 Then N'" + Properties.Resources.Mix + "' END as GroupGenderr , GroupGender From Groups WHERE ClassID = " + ClassID);
                Connexion.FillDG(ref DGGroups, "Select GroupID, GroupName, CASE   WHEN GroupGender = 0 THEN N'" + Properties.Resources.Male + "'  WHEN GroupGender = 1 THEN N'" + Properties.Resources.Female + "' When GroupGender = 2 Then N'" + Properties.Resources.Mix + "' END as GroupGenderr , GroupGender From Groups WHERE ClassID = " + ClassID);
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Buttun_Click_Next1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TComboBox.SelectedIndex != -1 && CYear.SelectedIndex != -1 && SubjectC.SelectedIndex != -1 && TPaymentMethod.SelectedIndex != -1 && TPayment.Text != "" && CPrice.Text != "")
                {
                    DataRowView row = (DataRowView)TComboBox.SelectedItem;
                    DataRowView rowSubject = (DataRowView)SubjectC.SelectedItem;
                    DataRowView rowYear = (DataRowView)CYear.SelectedItem;
                    DataRowView Rowlevel = (DataRowView)CLevel.SelectedItem;

                    string TID = row["ID"].ToString(); //getting Teacher ID 

                    if (Type == "Add") // adding class
                    {
                        if (TPaymentMethod.SelectedIndex == 2 && int.Parse(TPayment.Text) > int.Parse(CPrice.Text))
                        {
                            if (MessageBox.Show("the Teacher payment is higher than student price are you sure they are not mixed up?", "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                            {
                                return;
                            }
                        }
                        ClassID = Connexion.GetInt("Insert into Class( CName,CYear,CLevel, CSubject, TID, CPrice, TPaymentMethod, TPayment,  DateOFRegister, MultipleGroups) output inserted.ID values(N'" + CName.Text + "'," +
                             "'" + rowYear["ID"].ToString() + "'," +
                             "'" + Rowlevel["ID"].ToString() + "'," +
                             "'" + rowSubject["ID"].ToString() + "'," +
                             "'" + TID + "'," +
                             "'" + CPrice.Text + "'," +
                             "" + TPaymentMethod.SelectedIndex + "," +
                             "'" + TPayment.Text + "'," +
                             " getdate(),'" + S + "')").ToString();
                        queryAttendance = queryAttendance.Replace("{CID}", ClassID);
                        if (S == "Single")
                        {
                            Connexion.Insert("Insert into Groups Values(" + ClassID + ",N'" + CName.Text + "', " + GenderC.SelectedIndex + ",0,0)");
                        }
                        Connexion.InsertHistory(0, ClassID, 3);
                        Groups.IsEnabled = true;
                        TabSpec.IsEnabled = true;
                        Students.IsEnabled = true;
                        Attandence.IsEnabled = true;
                        GroupTime.IsEnabled = true;

                        if (isSpec)
                        {
                            TabSpec.IsEnabled = true;
                            TabSpec.Focus();
                        }
                        else
                        {
                            if (S == "Single")
                            {
                                GroupTime.Focus();
                            }
                            else
                            {
                                Groups.Focus();
                            }
                        }
                        MessageBox.Show("Inserted Succesfuly");
                    }
                    else
                    {
                        Connexion.Insert("Update Class Set CName = N'" + CName.Text + "' , CYear = " + rowYear["ID"].ToString() + " , CLevel = " + Rowlevel["ID"].ToString() + " , TID = " + TID + " , CSubject = " + rowSubject["ID"].ToString() + " , TPaymentMethod = " + TPaymentMethod.SelectedIndex + " , CPrice = " + CPrice.Text + " , TPayment = " + TPayment.Text + "   where ID = " + ClassID);
                        MessageBox.Show("Updated Succesfully");
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click_Attendance(object sender, RoutedEventArgs e)
        {
            try
            {
                if(DateAttend.Text == "")
                {
                    MessageBox.Show("Please Select a date");
                    return;
                }
                if (MessageBox.Show("Do you want To Create this Attendance?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string GroupID;
                    if (S == "Single")
                    {
                        GroupID = Connexion.GetInt(ClassID, "Groups", "GroupID", "ClassID").ToString();
                    }
                    else
                    {
                        DataRowView row = (DataRowView)CBGroupsA.SelectedItem;
                        if (row == null)
                        {
                            return;
                        }
                        GroupID = row["GroupID"].ToString();

                    }
                    DataRowView rowTime = (DataRowView)CBGroupT.SelectedItem;
                    if (rowTime == null)
                    {
                        return;
                    }
                    if (rowTime["ID"].ToString() != "-1")
                    {
                        SqlConnection con = Connexion.Connect();

                        Connexion.Insert("Update Groups set Sessions = Sessions + 1 , TSessions = TSessions + 1 Where GroupID = " + GroupID);

                        SqlCommand CommandID = new SqlCommand("Select Sessions From Groups Where GroupID = '" + GroupID + "'", con);
                        CommandID.ExecuteNonQuery();
                        Int32 result = Convert.ToInt32(CommandID.ExecuteScalar());

                        int IDRoom = Connexion.GetInt("Select IDRoom from Class_Time where ID =" + rowTime["ID"].ToString());
                        string TimeStart = Connexion.GetString("Select TimeStart from Class_Time where ID =" + rowTime["ID"].ToString());
                        string TimeEnd = Connexion.GetString("Select TimeEnd from Class_Time where ID =" + rowTime["ID"].ToString());
                        string AID = Connexion.GetInt("Insert into Attendance (GroupID,Date,Session,RoomID,TimeStart,TimeEND) OUTPUT Inserted.ID  Values(" + GroupID + ",'" + DateAttend.Text.Replace("/", "-") + "','" + result + "' , '" + IDRoom + "','" + TimeStart + "','" + TimeEnd + "' )").ToString();
                        Connexion.InsertHistory(0, AID, 5);
                        var AddS = new AttendanceAdd(AID, "Add", "1");
                        AddS.ShowDialog();
                    }
                    else
                    {
                        string AID = Connexion.GetInt("Insert into Attendance_Extra (CID,Date,Paid) OUTPUT Inserted.ID  Values(" + ClassID + ",'" + DateTime.Today.ToString("dd/MM/yyyy").Replace("/", "-") + "',0)").ToString();
                        var AddS = new AttendanceAdd(AID, "Add", "3");
                        AddS.ShowDialog(); 
                    }
                    string queryAttend = queryAttendance.Replace("{FilterCondition}", "Class.ID = " + ClassID);
                    Connexion.FillDG(ref DGAttendance, queryAttend);
                   
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void CBGroupsA_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (S == "Multiple")
                {
                    DataRowView row = (DataRowView)CBGroupsA.SelectedItem;
                    if (row == null)
                    {
                        return;
                    }
                    string GroupID = row["GroupID"].ToString();
                    DataTable dtTime = new DataTable();

                    Connexion.FillDT(ref dtTime, "SELECT *,Case " +
                 "  When Class_Time.Day = 0 Then N'" + Properties.Resources.Sunday + "' + TimeStart" +
                 "  When Class_Time.Day = 1 Then N'" + Properties.Resources.Monday + "' + TimeStart" +
                 "  When Class_Time.Day = 2 Then N'" + Properties.Resources.Tuesday + "' + TimeStart" +
                 "  When Class_Time.Day = 3 Then N'" + Properties.Resources.Wednesday + "' + TimeStart" +
                 "  When Class_Time.Day = 4 Then N'" + Properties.Resources.Thursday + "' + TimeStart" +
                 "  When Class_Time.Day = 5 Then N'" + Properties.Resources.Friday + "' + TimeStart" +
                 "  When Class_Time.Day = 6 Then N'" + Properties.Resources.Saturday + "' + TimeStart " +
                 "  End As Time  FROM Class_Time WHERE Class_time.Type = 1 and  CLass_Time.GID=" + GroupID);
                    DataRow newRow = dtTime.NewRow();
                    newRow["Time"] = this.Resources["ExtraSession"];
                    newRow["ID"] = -1;
                    dtTime.Rows.Add(newRow);
                    CBGroupT.ItemsSource = dtTime.DefaultView;
                    Connexion.FillDG(ref DGAttendance, queryAttendance.Replace("{FilterCondition}", "Groups.GroupID = " + GroupID));

                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void CBGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (S != "d") // condition doesnt matter still didnt manage when single or multiple groups
                {
                    CBStudent.SelectedIndex = -1;
                    DataRowView row = (DataRowView)CYear.SelectedItem;
                    string yearid = row["ID"].ToString();
                    string Specid;
                        /* string query = "SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students  Where  Students.Status = 1 and Year = " + yearid;
                    for (int i = 0; i < DGSpec.Items.Count; i++)
                    {
                        Specid = ((DataRowView)DGSpec.Items[i]).Row["SpecID"].ToString();
                        if (i == 0)
                        {
                            query += "AND SPECIALITY = " + Specid;
                        }
                        else
                        {
                            query += "OR SPECIALITY = " + Specid;
                        }
                    }
                    DataRowView rowGroup = (DataRowView)CBGroups.SelectedItem;
                    if (rowGroup == null)
                    {
                        return;
                    }
                    string gender = rowGroup["GroupGender"].ToString();
                    if (gender == "1" || gender == "0")
                    {
                        query += "AND Students.Gender =" + gender;
                    }
                    query += " Except SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students Join Class_Student On Class_Student.StudentID = Students.ID   Where Students.Status = 1 and  Class_Student.ClassID = " + ClassID + " and Class_Student.EndSession is null ORDER BY Name ASC";
                    Connexion.FillCB(ref CBStudent, query);*/
                }
                string groupIDquery = " And Class_Student.GroupID = ";
                if (S == "Single")
                {
                    groupIDquery += Connexion.GetString($"Select GroupID from Groups Where ClassID = {ClassID}");
                }
                else
                {
                    DataRowView rowGroup = (DataRowView)CBGroups.SelectedItem;
                    if (rowGroup == null)
                    {
                        groupIDquery = "";
                    }
                    else
                    {
                        groupIDquery += rowGroup["GroupID"].ToString();
                    }
                }
                FillDG("Students", $"WHERE Class_Student.ClassID = '" + ClassID + "'" + groupIDquery);
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void btnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)DGAttendance.SelectedItem;
                if(row["Type"].ToString() == "EXTRA")
                {
                    string AID = row["AttendanceID"].ToString();
                    var AddS = new AttendanceAdd(AID, "Show", "3");
                    AddS.ShowDialog();
                }
                else if ( row["Type"].ToString() == "NRML")
                {
                    string AID = row["AttendanceID"].ToString();
                    var AddS = new AttendanceAdd(AID, "Show", "1");
                    AddS.ShowDialog();

                }
                string query;
                DataRowView row2 = (DataRowView)CBGroupsA.SelectedItem;
                if (row2 == null)
                {
                    string queryAttend = queryAttendance.Replace("{FilterCondition}", "Class.ID = " + ClassID);
                    Connexion.FillDG(ref DGAttendance, queryAttend);
                }
                else
                {
                    string RowGroupID = row2["GroupID"].ToString();
                    string queryAttend = queryAttendance.Replace("{FilterCondition}", "Groups.GroupID = " + RowGroupID);
                    Connexion.FillDG(ref DGAttendance, queryAttend);

                }

                /* AddS.Closing += (senderr, ee) =>
                 {
                     string query; 
                     string GroupID;
                     if (S == "Single")
                     {
                         GroupID = Connexion.GetInt(ClassID, "Groups", "GroupID", "ClassID").ToString();
                         query = "Select " +
                                  "Class.CName + ' ' + Groups.GroupName  as GroupName ," +
                                  "Attendance.ID as AttendanceID , " +
                                  "Class_Time.TimeStart as Time ," +
                                  "Attendance.Session as Session, " +
                                  "Attendance.Date From Attendance " +
                                  "Join Groups ON Attendance.GroupID = Groups.GroupID " +
                                  "Join Class_Time ON  Groups.GroupID = Class_Time.GID  " +
                                  "Join Class on Groups.CLassID = Class.ID where Groups.GroupID = " + GroupID;
                     }
                     else
                     {
                         DataRowView row2 = (DataRowView)CBGroupsA.SelectedItem;
                         if(row2 == null)
                         {
                             query = "Select Class.CName + ' ' + Groups.GroupName  as GroupName , Attendance.ID as AttendanceID , Class_Time.TimeStart as Time ,       Attendance.Session as Session,  Class_Time.GID, class_Time.ID , Attendance.Date From Attendance Join Groups ON Attendance.GroupID = Groups.GroupID Join Class_Time ON  Groups.GroupID = Class_Time.GID Join Class on Groups.CLassID = Class.ID Where Groups.ClassID =  " + ClassID + " Order By CONVERT(DATETIME, Attendance.Date, 103) Desc "; 
                         }
                         else
                         {
                             GroupID = row["GroupID"].ToString();
                             query = "Select " +
                                "Class.CName + ' ' + Groups.GroupName  as GroupName ," +
                                  "Attendance.ID as AttendanceID , " +
                                   "Class_Time.TimeStart as Time ," +
                                  "Attendance.Session as Session, " +
                                  "Attendance.Date From Attendance " +
                                  "Join Groups ON Attendance.GroupID = Groups.GroupID " +
                                  "Join Class_Time ON  Groups.GroupID = Class_Time.GID  " +
                                   "Join Class on Groups.CLassID = Class.ID where Groups.GroupID = " + GroupID; 
                         }
                     }
                     Connexion.FillDG(ref DGAttendance,query );
                 };*/

            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void TPayment_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void CPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }



        private void CLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)CLevel.SelectedItem;
                if (row != null)
                {
                    if (row["IsSpeciality"].ToString() == "1")
                    {
                        isSpec = true;
                        SPSpec.Visibility = Visibility.Visible;
                        DGSpec.Visibility = Visibility.Visible;
                        TabSpec.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        isSpec = false;
                        SPSpec.Visibility = Visibility.Collapsed;
                        DGSpec.Visibility = Visibility.Collapsed;
                        TabSpec.Visibility = Visibility.Collapsed;
                    }
                    Connexion.FillCB(ref CYear, "Select * from Years Where LevelID = " + row["ID"].ToString());
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void CBGroupsT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                DataRowView row = (DataRowView)CBGroupsT.SelectedItem;
                if (row != null)
                {
                    CDay.SelectedIndex = -1;
                    Connexion.FillDG(ref DGAddTime, "Select Class_Time.ID as ID , Class_Time.GID as GID , Class_Time.Day as DayID ,   Case  When Class_Time.Day = 0 Then N'" + this.Resources["Sunday"].ToString() + "' " +
                           "  When Class_Time.Day = 1 Then N'" + this.Resources["Monday"].ToString() + "' " +
                           "  When Class_Time.Day = 2 Then N'" + this.Resources["Tuesday"].ToString() + "'" +
                           "  When Class_Time.Day = 3 Then N'" + this.Resources["Wednesday"].ToString() + "'" +
                           "  When Class_Time.Day = 4 Then N'" + this.Resources["Thursday"].ToString() + "' " +
                           "  When Class_Time.Day = 5 Then N'" + this.Resources["Friday"].ToString() + "'" +
                           "  When Class_Time.Day = 6 Then N'" + this.Resources["Saturday"].ToString() + "' " +
                            " End  as Day  , " +
                         "Class_Time.TimeStart as TimeStart," +
                         "Class_Time.TimeEnd As TimeEnd ," +
                         "Rooms.Room As Room , " +
                         "Groups.GroupName as GroupName , " +
                         "Groups.GroupID , " +
                         "Rooms.ID as IDRoom " +
                         "From Class_Time " +
                         "Join Rooms On Class_Time.IDRoom = Rooms.ID " +
                         "JOIN Groups ON Groups.GroupID = Class_Time.GID " +
                         "Where Class_time.Type = 1  and Groups.GroupID = " + row["GroupID"].ToString());
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void SetLang()
        {
            ResourceDictionary ResourceDic = new ResourceDictionary();
            if (Connexion.Language() == 1)
            {
                ResourceDic.Source = new Uri("../Dictionary\\AR.xaml", UriKind.Relative);
                this.FlowDirection = FlowDirection.RightToLeft;
            }
            else if (Connexion.Language() == 0)
            {
                ResourceDic.Source = new Uri("../Dictionary\\EN.xaml", UriKind.Relative);
                this.FlowDirection = FlowDirection.LeftToRight;
            }
            else if (Connexion.Language() == 2)
            {
                ResourceDic.Source = new Uri("../Dictionary\\FR.xaml", UriKind.Relative);
                this.FlowDirection = FlowDirection.LeftToRight;
            }
            this.Resources.MergedDictionaries.Add(ResourceDic);
        }

        private void CRoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView rowRoom = (DataRowView)CRoom.SelectedItem;
                if (rowRoom != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Hour", typeof(string));
                    dt.Columns.Add("Status", typeof(int));
                    dt.Columns.Add("ID", typeof(int));
                    dt.Columns.Add("Type", typeof(int));

                    for (int i = 6; i < 24; i++)
                    {
                        DataRow dr = dt.NewRow();
                        if (i < 10)
                        {
                            dr[0] = "0" + i.ToString();
                        }
                        else
                        {
                            dr[0] = i.ToString();
                        }
                        int[] r = Connexion.CheckTimeHour(dr[0].ToString(), CDay.SelectedIndex, int.Parse(rowRoom["ID"].ToString()));
                        dr[1] = r[0];
                        dr[2] = r[1];
                        dr[3] = r[2];
                        dt.Rows.Add(dr);
                    }
                    HFrom.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void HFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                MFrom.SelectedIndex = -1;
                MTo.SelectedIndex = -1;
                HTo.SelectedIndex = -1;
                DataRowView row = (DataRowView)HFrom.SelectedItem;
                if (row != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Hour", typeof(string));
                    dt.Columns.Add("Status", typeof(int));
                    dt.Columns.Add("ID", typeof(int));
                    dt.Columns.Add("Type", typeof(int));
                    if (row["Status"].ToString() == "2")
                    {
                        string groupName = "";
                        if (row["Type"].ToString() == "1")
                        {
                            groupName = Connexion.GetString(row["ID"].ToString(), "Groups", "GroupName", "GroupID");
                        }
                        else if (row["Type"].ToString() == "2")
                        {
                            groupName = Connexion.GetString(row["ID"].ToString(), "Formation", "Name", "ID");
                        }
                        MessageBox.Show(this.Resources["TimeTaken"].ToString() + " " + groupName);
                        HFrom.SelectedIndex = -1;
                    }
                    else if (row["Status"].ToString() == "1")
                    {
                        DataRowView rowroom = (DataRowView)CRoom.SelectedItem;
                        SqlConnection con = Connexion.Connect();
                        string query = "";
                        if (row["Type"].ToString() == "1")
                        {
                            query = "Select  CONVERT(INT,SUBSTRING(TimeEnd,4,2)) From Class_Time  join Groups on Groups.GroupID = Class_Time.GID " +
                              "Where IDroom = " + rowroom["ID"].ToString() + " " +
                              "and day =  " + CDay.SelectedIndex + " " +
                              "and CONVERT(INT,SUBSTRING(TimeEnd,0,3)) = " + row["Hour"].ToString() + " " +
                              "and Groups.ClassID = '" + Connexion.GetClassID(row["ID"].ToString()) + "'";
                        }
                        else if (row["Type"].ToString() == "2")
                        {
                            query = "Select  CONVERT(INT,SUBSTRING(TimeEnd,4,2)) From Class_Time  join Formation on Formation.ID = Class_Time.FID " +
                            "Where IDroom = " + rowroom["ID"].ToString() + " " +
                            "and day =  " + CDay.SelectedIndex + " " +
                            "and CONVERT(INT,SUBSTRING(TimeEnd,0,3)) = " + row["Hour"].ToString() + " " +
                            "and Class_Time.FID = '" + row["ID"].ToString() + "'";


                        }
                        SqlCommand CommandID = new SqlCommand(query, con);
                        int result = int.Parse(CommandID.ExecuteScalar().ToString());
                        for (int i = 0; i < 60; i += 15)
                        {
                            DataRow dr = dt.NewRow();
                            if (i == 0)
                            {
                                dr[0] = "00";
                            }
                            else
                            {
                                dr[0] = i.ToString();
                            }
                            if (i == result)
                            {
                                dr[1] = 1;
                                dr[2] = int.Parse(row["ID"].ToString());
                                dr[3] = row["Type"].ToString();
                            }
                            else if (result > i)
                            {
                                dr[1] = 2;
                                dr[2] = int.Parse(row["ID"].ToString());
                                dr[3] = row["Type"].ToString();
                            }
                            else
                            {
                                dr[1] = 0;
                                dr[2] = -1;
                            }
                            dt.Rows.Add(dr);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 60; i += 15)
                        {
                            DataRow dr = dt.NewRow();
                            if (i == 0)
                            {
                                dr[0] = "00";
                            }
                            else
                            {
                                dr[0] = i.ToString();
                            }
                            dr[1] = 0;
                            dr[2] = -1;
                            dt.Rows.Add(dr);
                        }
                    }
                    MFrom.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void MFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                MTo.SelectedIndex = -1;
                HTo.SelectedIndex = -1;
                DataRowView row = (DataRowView)MFrom.SelectedItem;
                DataRowView rowRoom = (DataRowView)CRoom.SelectedItem;
                int found = 0;
                string id = "0";
                DataRowView row2 = (DataRowView)HFrom.SelectedItem;

                if (row != null && row2 != null && rowRoom != null)
                {
                    if (row["Status"].ToString() == "2")
                    {
                        string groupName = "";
                        if (row["Type"].ToString() == "1")
                        {
                            groupName = Connexion.GetString(row["ID"].ToString(), "Groups", "GroupName", "GroupID");
                        }
                        else
                        {
                            groupName = Connexion.GetString(row["ID"].ToString(), "Formation", "Name", "ID");
                        }
                        MessageBox.Show(this.Resources["TimeTaken"].ToString() + " " + groupName);
                        HFrom.SelectedIndex = -1;
                        return;
                    }
                    else if (row["Status"].ToString() == "1")
                    {
                        string groupName = "";
                        if (row["Type"].ToString() == "1")
                        {
                            groupName = Connexion.GetString(row["ID"].ToString(), "Groups", "GroupName", "GroupID");
                        }
                        else
                        {
                            groupName = Connexion.GetString(row["ID"].ToString(), "Formation", "Name", "ID");
                        }
                        if (MessageBox.Show(this.Resources["TimeEndNowPT1"].ToString() + " " + groupName + this.Resources["TimeEndNowPT2"].ToString(), this.Resources["Warning"].ToString(), MessageBoxButton.YesNo) == MessageBoxResult.No)
                        {
                            HFrom.SelectedIndex = -1;
                            return;
                        }
                    }
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Hour", typeof(string));
                    dt.Columns.Add("Status", typeof(int));
                    dt.Columns.Add("ID", typeof(int));
                    dt.Columns.Add("Type", typeof(string));

                    for (int i = int.Parse(row2["Hour"].ToString()); i < 24; i++)
                    {
                        DataRow dr = dt.NewRow();
                        if (i < 10)
                        {
                            dr[0] = "0" + i;
                        }
                        else
                        {
                            dr[0] = i.ToString();
                        }
                        string typeclasstime = "";
                        if (found == 0)
                        {

                            string Command = "Select  * From Class_Time " +
                                "Where IDroom = " + rowRoom["ID"].ToString() + " " +
                                "and day =  " + CDay.SelectedIndex + " " +
                                "and CONVERT(INT,SUBSTRING(TimeStart,0,3)) = " + i;

                            DataTable dttime = new DataTable();
                            Connexion.FillDT(ref dttime, Command);

                            if (dttime.Rows.Count > 0)
                            {
                                typeclasstime = dttime.Rows[0]["Type"].ToString();
                                found = 2;
                                if (typeclasstime == "1")
                                {
                                    id = dttime.Rows[0]["GID"].ToString();
                                }
                                else if (typeclasstime == "2")
                                {
                                    id = dttime.Rows[0]["FID"].ToString();
                                }

                            }
                            else
                            {
                                dr[1] = 0;
                                dr[2] = -1;
                            }
                        }
                        if (found == 1)
                        {
                            dr[1] = 2;
                            dr[2] = id;
                            dr[3] = typeclasstime;
                        }
                        if (found == 2)
                        {
                            found = 1;
                            dr[1] = 1;
                            dr[2] = id;
                            dr[3] = typeclasstime;
                        }
                        dt.Rows.Add(dr);
                    }
                    HTo.ItemsSource = dt.DefaultView;

                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void HTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                MTo.SelectedIndex = -1;
                DataRowView row = (DataRowView)HTo.SelectedItem;
                if (row != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Hour", typeof(string));
                    dt.Columns.Add("Status", typeof(int));
                    dt.Columns.Add("ID", typeof(int));
                    dt.Columns.Add("type", typeof(int));

                    if (row["Status"].ToString() == "2")
                    {
                        string groupName = "";
                        if (row["Type"].ToString() == "1")
                        {
                            groupName = Connexion.GetString(row["ID"].ToString(), "Groups", "GroupName", "GroupID");
                        }
                        else if (row["Type"].ToString() == "2")
                        {
                            groupName = Connexion.GetString(row["ID"].ToString(), "Formation", "Name", "ID");
                        }
                        MessageBox.Show(this.Resources["TimeTaken"].ToString() + " " + groupName); //edit resource
                        HTo.SelectedIndex = -1;
                    }
                    else if (row["Status"].ToString() == "1")
                    {
                        DataRowView rowroom = (DataRowView)CRoom.SelectedItem;
                        SqlConnection con = Connexion.Connect();
                        string query = "";
                        if (row["Type"].ToString() == "1")
                        {
                            query = "Select  CONVERT(INT,SUBSTRING(TimeStart,4,2)) From Class_Time " +
                            "Where IDroom = " + rowroom["ID"].ToString() + " " +
                            "and day =  " + CDay.SelectedIndex + " " +
                            "and CONVERT(INT,SUBSTRING(TimeStart,0,3)) = " + row["Hour"].ToString() + " " +
                            "and GID = '" + row["ID"].ToString() + "'";
                        }
                        else if (row["Type"].ToString() == "2")
                        {
                            query = "Select  CONVERT(INT,SUBSTRING(TimeStart,4,2)) From Class_Time " +
                            "Where IDroom = " + rowroom["ID"].ToString() + " " +
                            "and day =  " + CDay.SelectedIndex + " " +
                            "and CONVERT(INT,SUBSTRING(TimeStart,0,3)) = " + row["Hour"].ToString() + " " +
                            "and FID = '" + row["ID"].ToString() + "'";
                        }
                        SqlCommand CommandID = new SqlCommand(query,
                            con);
                        CommandID.ExecuteNonQuery();
                        int result = int.Parse(CommandID.ExecuteScalar().ToString());
                        for (int i = 0; i < 60; i += 15)
                        {
                            DataRow dr = dt.NewRow();
                            if (i == 0)
                            {
                                dr[0] = "00";
                            }
                            else
                            {
                                dr[0] = i.ToString();
                            }
                            if (i == result)
                            {
                                dr[1] = 1;
                                dr[2] = int.Parse(row["ID"].ToString());
                                dr[3] = int.Parse(row["Type"].ToString());
                            }
                            else if (result < i)
                            {
                                dr[1] = 2;
                                dr[2] = int.Parse(row["ID"].ToString());
                                dr[3] = int.Parse(row["Type"].ToString());
                            }
                            else
                            {
                                dr[1] = 0;
                                dr[2] = -1;
                                dr[3] = -1;
                            }
                            dt.Rows.Add(dr);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 60; i += 15)
                        {
                            DataRow dr = dt.NewRow();
                            if (i == 0)
                            {
                                dr[0] = "00";
                            }
                            else
                            {
                                dr[0] = i.ToString();
                            }
                            dr[1] = 0;
                            dr[2] = -1;
                            dr[3] = -1;
                            dt.Rows.Add(dr);
                        }
                    }
                    MTo.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void MTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                DataRowView row = (DataRowView)MTo.SelectedItem;
                if (row != null)
                {
                    if (row["Status"].ToString() == "2")
                    {
                        string groupName = "";
                        if (row["Type"].ToString() == "1")
                        {
                            groupName = Connexion.GetString(row["ID"].ToString(), "Groups", "GroupName", "GroupID");
                        }
                        else if (row["Type"].ToString() == "2")
                        {
                            groupName = Connexion.GetString(row["ID"].ToString(), "Formation", "Name", "ID");
                        }
                        MessageBox.Show(this.Resources["TimeTaken"].ToString() + " " + groupName);
                        MTo.SelectedIndex = -1;
                        return;
                    }
                    else if (row["Status"].ToString() == "1")
                    {
                        string groupName = "";
                        if (row["Type"].ToString() == "1")
                        {
                            groupName = Connexion.GetString(row["ID"].ToString(), "Groups", "GroupName", "GroupID");
                        }
                        else if (row["Type"].ToString() == "2")
                        {
                            groupName = Connexion.GetString(row["ID"].ToString(), "Formation", "Name", "ID");
                        }
                        if (MessageBox.Show(this.Resources["TimeEndNowPT1"].ToString() + " " + groupName + this.Resources["TimeEndNowPT2"].ToString(), this.Resources["Warning"].ToString(), MessageBoxButton.YesNo) == MessageBoxResult.No)
                        {
                            MTo.SelectedIndex = -1;
                            return;
                        }
                    }
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void CDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CRoom.SelectedIndex = -1;
            HFrom.SelectedIndex = -1;
            HTo.SelectedIndex = -1;
            MTo.SelectedIndex = -1;
            MFrom.SelectedIndex = -1;
        }


        private void BtnBarCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)DGGroups.SelectedItem;
                if (row == null)
                {
                    return;
                }
                Methods.PrintBarcode("Select Students.FirstName as FirstName  , Students.LastName as LastName , " +
                   "Students.BarCode , Years.Year as Year, Specialities.Speciality as Spec , Students.Note as Note ,Students.BarCode  from Students Join Class_Student on Class_Student.StudentID = Students.ID join Levels on Levels.ID = Students.Level  Join Years On Students.Year = Years.ID  Left Join Specialities On Students.Speciality = Specialities.ID join Class on Class_Student.ClassID = Class.ID   Where GroupID = " + row["GroupID"].ToString());
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnDeleteAttendance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)DGAttendance.SelectedItem;
                if (MessageBox.Show("Are you Sure u want to Delete this Attendance?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {

                    string AID = row["AttendanceID"].ToString();
                    Commun.DeleteAttendance(AID);
                    string query;
                    DataRowView row2 = (DataRowView)CBGroupsA.SelectedItem;
                    if (row2 == null)
                    {

                        string queryAttend = queryAttendance.Replace("{FilterCondition}", "Class.ID = " + ClassID);
                        Connexion.FillDG(ref DGAttendance, queryAttend);
                    }
                    else
                    {
                        string RowGroupID = row2["GroupID"].ToString();
                        string queryAttend = queryAttendance.Replace("{FilterCondition}", "Groups.GroupID = " + RowGroupID);
                        Connexion.FillDG(ref DGAttendance, queryAttend);

                    }

                    MessageBox.Show("Deleted Succesfully");
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void CBStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DGStudent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataRowView row = (DataRowView)DGStudent.SelectedItem;
            if (row != null)
            {
                string SID = row["StudentID"].ToString();
                var AddW = new StudentAdd("Show", SID);
                AddW.Show();
            }
        }

        private void BtnRemoveStudent_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you Sure u want to Delete the student from this Class?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                string CID = ((DataRowView)DGStudent.SelectedItem).Row["ClassID"].ToString();
                string GID = ((DataRowView)DGStudent.SelectedItem).Row["GroupID"].ToString();
                string SID = ((DataRowView)DGStudent.SelectedItem).Row["StudentID"].ToString();
                Connexion.Insert("Delete from Class_Student Where GroupID = " + GID + " and StudentID = " + SID);
                DataTable dtAttendance = new DataTable();
                Connexion.FillDT(ref dtAttendance, "Select ID from Attendance Where GroupID = " + GID);
                for (int i = 0; i < dtAttendance.Rows.Count; i++)
                {
                    Connexion.Insert("Delete from Attendance_Student Where StudentID = " + SID + " and ID = " + dtAttendance.Rows[i]["ID"].ToString());
                }
                string groupIDquery = " And Class_Student.GroupID = ";
                if (S == "Single")
                {
                    groupIDquery += Connexion.GetString($"Select GroupID from Groups Where ClassID = {ClassID}");
                }
                else
                {
                    DataRowView rowGroup = (DataRowView)CBGroups.SelectedItem;
                    if (rowGroup == null)
                    {
                        groupIDquery = "";
                    }
                    else
                    {
                        groupIDquery += rowGroup["GroupID"].ToString();
                    }
                }
                FillDG("Students", $"WHERE Class_Student.ClassID = '" + ClassID + "'" + groupIDquery);
                FillDG("Attendance", "");
                MessageBox.Show("Deleted Succesfully");
            }
        }


   
  
        private void BtnPrintAttend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)DGAttendance.SelectedItem;
                string AID = row["AttendanceID"].ToString();
                if(row["Type"].ToString() == "EXTRA")
                {
                    FastReports.PrintAttendance(AID,"3");
                }
                else
                {
                    FastReports.PrintAttendance(AID);
                }
                
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }
        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                // Assuming you have a DataTable named "DtGroups" with columns "GroupID" and "GroupName"
                comboBox.ItemsSource = dtGroups.DefaultView;
                DataRowView rowView = (DataRowView)comboBox.DataContext;
                int groupID = (int)rowView["GroupID"];
                comboBox.SelectedItem = groupID;
            }
        }

        private void CmbGroupName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox comboBox = (ComboBox)sender;
                if (comboBox.SelectedItem != null && DGStudent.SelectedItem != null)
                {
                    // Assuming that the ComboBox's DataContext is a DataRowView.
                    DataRowView rowView = (DataRowView)comboBox.DataContext;
                    int studentID = (int)rowView["StudentID"];

                    // Retrieve the selected GroupID.
                    int selectedGroupID = (int)comboBox.SelectedValue;
                    if (MessageBox.Show("Are you Sure u want to Change the Group  for this student?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        int startSes = Connexion.GetInt("Select Session from Class_Student Where StudentID = " + studentID + " and ClassID  = " + ClassID);
                        int EndSes;
                        int Gid = Connexion.GetGroupID(studentID.ToString(), ClassID.ToString());
                        if (Connexion.IFNULLVar("Select EndSession from Class_Student Where StudentID = " + studentID + " and ClassID  = " + ClassID))
                        {
                            EndSes = Connexion.GetInt("Select Sessions from Groups Where GroupID = " + Gid);
                            if (EndSes > Connexion.GetInt("Select Sessions from Groups Where GroupID = " + selectedGroupID))
                            {
                                EndSes = Connexion.GetInt("Select Sessions from Groups Where GroupID = " + selectedGroupID);
                                for (int f = Connexion.GetInt("Select Sessions from Groups Where GroupID = " + selectedGroupID) + 1; f <= EndSes; f++)
                                {
                                    int AttendID = Connexion.GetInt("Select ID from Attendance Where GroupID = " + Gid + " and Session =" + f);
                                    int stat = Connexion.GetInt("Select Status from Attendance_Student Where ID =" + AttendID + " and StudentID =" + studentID);
                                    if (stat == 0)
                                    {
                                        Connexion.Insert("Delete from Attendance_Student Where ID =" + AttendID + " and StudentID =" + studentID);
                                    }
                                    else if (stat == 1)
                                    {
                                        Connexion.Insert("Insert Into Attendance_Change Values (" + studentID + "," + selectedGroupID + ", " + Gid + "," + f + ")");
                                    }

                                }
                            }
                            else if (EndSes < Connexion.GetInt("Select Sessions from Groups Where GroupID = " + selectedGroupID))
                            {
                                for (int f = EndSes + 1; f <= Connexion.GetInt("Select Sessions from Groups Where GroupID = " + selectedGroupID); f++)
                                {
                                    int AttendID = Connexion.GetInt("Select ID from Attendance Where GroupID = " + selectedGroupID + " and Session =" + f);
                                    if (Connexion.IFNULL("Select * from Attendance_Student Where ID =" + AttendID + " and StudentID =" + studentID))
                                    {
                                        Connexion.Insert("Insert into attendance_Student(ID,StudentID,Status,Note) Values (" + AttendID + "," + studentID + ",null,'')");
                                        Commun.SetStatusAttendance(studentID.ToString(), AttendID.ToString(), 1);
                                    }
                                    else
                                    {
                                        Connexion.Insert("Delete from Attendance_Change Where ToGroupID = " + selectedGroupID + " and Session =" + f + " and StudentID =" + studentID);// if the student went and studied to the destined groupchange then delete it 
                                    }

                                }
                            }
                        }
                        else
                        {
                            EndSes = Connexion.GetInt("Select EndSession from Class_Student Where StudentID = " + studentID + " and ClassID  = " + ClassID);
                        }
                        for (int i = startSes; i <= EndSes; i++)
                        {
                            int AID = Connexion.GetInt("Select ID from Attendance Where GroupID = " + Gid + " and session = " + i);
                            int AIDNew = Connexion.GetInt("Select ID from Attendance Where GroupID = " + selectedGroupID + " and session = " + i);
                            int Status = Connexion.GetInt("Select Status from Attendance_Student Where StudentID = " + studentID + " and ID =" + AID);
                            if (Status == 0)
                            {
                                Connexion.Insert("Delete From Attendance_Student Where StudentID = " + studentID + " and ID =" + AID);
                                Connexion.Insert("Insert Into Attendance_Student(ID,StudentID,Status,Note) Values (" + AIDNew + "," + studentID + ",0,'')");
                                Commun.SetStatusAttendance(studentID.ToString(), AIDNew.ToString(), 0);

                            }
                            else if (Status == 1)
                            {
                                Connexion.Insert("Insert Into Attendance_Student(ID,StudentID,Status,Note) Values (" + AIDNew + "," + studentID + ",2,'')");
                                Connexion.Insert("Insert Into Attendance_Change  Values (" + studentID + "," + selectedGroupID + "," + Gid + " , " + i + ")");
                                Commun.SetStatusAttendance(studentID.ToString(), AIDNew.ToString(), 2);
                            }
                            else if (Status == 2)
                            {
                                int ToGroupID = Connexion.GetInt("Select ToGroupID From Attendance_Change Where StudentID = " + studentID + " and FromGroupID = " + Gid + " and Session = " + i);
                                if (ToGroupID == selectedGroupID)
                                {
                                    Connexion.Insert("Delete  From Attendance_Change Where StudentID = " + studentID + " and FromGroupID = " + Gid + " and Session = " + i + " and ToGroupID = " + ToGroupID);
                                    Connexion.Insert("delete from  Attendance_Student Where StudentID = " + studentID + " and ID =" + AID);

                                }
                                else
                                {
                                    Connexion.Insert("delete from  Attendance_Student Where StudentID = " + studentID + " and ID =" + AID);
                                    Connexion.Insert("Insert Into Attendance_Student(ID,StudentID,Status,Note) Values (" + AIDNew + "," + studentID + ",2,'')");
                                    Commun.SetStatusAttendance(studentID.ToString(), AIDNew.ToString(), 2);
                                    Connexion.Insert("Update  Attendance_Change Set FromGroupID =" + selectedGroupID + " Where ToGroupID = " + ToGroupID + " and StudentID = " + studentID + " and Session =" + i);

                                }
                            }
                            else if (Status == 3)
                            {
                                Connexion.Insert("delete from  Attendance_Student Where StudentID = " + studentID + " and ID =" + AID);
                                Connexion.Insert("Insert Into Attendance_Student(ID,StudentID,Status,Note) Values (" + AIDNew + "," + studentID + ",3,'')");
                                Commun.SetStatusAttendance(studentID.ToString(), AIDNew.ToString(), 3);
                                Connexion.Insert("Update  justif  Set AID =" + AIDNew + " Where SID = " + studentID + " and AID = " + AID + " and Session =" + i);
                            }

                        }

                        Connexion.Insert("Update Class_Student Set GroupID =" + selectedGroupID + " Where GroupID =" + Gid + " and StudentID =" + studentID);
                        MessageBox.Show("Changed Groups Succesfully");
                    }
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void DGGroups_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                TextBox textBox = e.EditingElement as TextBox;
                if (textBox != null)
                {
                    // Get the DataRowView associated with the edited row
                    DataRowView rowView = e.Row.Item as DataRowView;
                    if (rowView != null)
                    {
                        // Assuming "GroupID" and "GroupName" are column names in your DataTable
                        int groupID = (int)rowView["GroupID"];
                        string newGroupName = textBox.Text;
                        Connexion.Insert("Update Groups Set GroupName = N'" + newGroupName + "'  Where GroupID =" + groupID);
                    }


                }
            }
        }

        private void CBGroupT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click_ImportStudents(object sender, RoutedEventArgs e)
        {
            try
            {
                if (S != "d") // condition doesnt matter still didnt manage when single or multiple groups
                {
                    CBStudent.SelectedIndex = -1;
                    DataRowView row = (DataRowView)CYear.SelectedItem;
                    string yearid = row["ID"].ToString();
                    string Specid;
                    string query = "SELECT Students.ID  , Students.Parentnumber , Students.PhoneNumber ,dbo.GetStudentSubjects(Students.ID) as Subjects , case when Students.LastName is NULL then students.FirstName " +
                            "When Students.Firstname is NULL then Students.LastName " +
                            "when Students.Firstname is not Null and Students.LastName is not null then Students.LastName + ' ' + Students.FirstName end as Name ,Students.Adress,Students.Level as LevelID ,dbo.ReplaceArabicName(Students.FirstName) + ' ' + dbo.ReplaceArabicName(Students.LastName) as NameSearch ,dbo.ReplaceArabicName(Students.LastName) + ' ' + dbo.ReplaceArabicName(Students.FirstName) as RNameSearch ,  Students.FirstName as FirstName,Students.LastName as LastName , 'False' as IsChecked, Students.BarCode as BarCode,Students.Gender,(FirstName + ' ' + LastName) as Name  ,Students.FirstName + ' ' + Students.LastName as RName from Students  Where  Students.Status = 1 and Year = " + yearid;
                    for (int i = 0; i < DGSpec.Items.Count; i++)
                    {
                        Specid = ((DataRowView)DGSpec.Items[i]).Row["SpecID"].ToString();
                        if (i == 0)
                        {
                            query += "AND SPECIALITY = " + Specid;
                        }
                        else
                        {
                            query += "OR SPECIALITY = " + Specid;
                        }
                    }
                    string gender;
                    if (S == "Multiple")
                    {
                        DataRowView row2 = (DataRowView)CBGroups.SelectedItem;
                        if(row2 == null)
                        {
                            MessageBox.Show("Please Select a group");
                            return;
                        }
                        gender = row2["GroupGender"].ToString();
                    }
                    else
                    {
                        gender = GenderC.SelectedIndex.ToString();
                    }
                    query += " Except SELECT Students.ID  , Students.Parentnumber , Students.PhoneNumber ,dbo.GetStudentSubjects(Students.ID) as Subjects , case when Students.LastName is NULL then students.FirstName " +
                            "When Students.Firstname is NULL then Students.LastName " +
                            "when Students.Firstname is not Null and Students.LastName is not null then Students.LastName + ' ' + Students.FirstName end as Name ,Students.Adress,Students.Level as LevelID ,dbo.ReplaceArabicName(Students.FirstName) + ' ' + dbo.ReplaceArabicName(Students.LastName) as NameSearch ,dbo.ReplaceArabicName(Students.LastName) + ' ' + dbo.ReplaceArabicName(Students.FirstName) as RNameSearch ,  Students.FirstName as FirstName,Students.LastName as LastName , 'False' as IsChecked, Students.BarCode as BarCode,Students.Gender,(FirstName + ' ' + LastName) as Name  , Students.FirstName + ' ' + Students.LastName as RName  from Students Join Class_Student On Class_Student.StudentID = Students.ID   Where Students.Status = 1 and  Class_Student.ClassID = " + ClassID + " and Class_Student.EndSession is null ORDER BY ID ASC";
                    string GID = "";
                    if (S == "Single")
                    {
                        GID = Connexion.GetInt(ClassID, "Groups", "GroupID", "ClassID").ToString();
                    }
                    else
                    {
                        DataRowView Group = (DataRowView)CBGroups.SelectedItem;
                        GID = Group["GroupID"].ToString();
                    }
                    Panels.List l = new Panels.List(query, GID);
                    l.ShowDialog();
                    string groupIDquery = " And Class_Student.GroupID = ";
                    if (S == "Single")
                    {
                        groupIDquery += Connexion.GetString($"Select GroupID from Groups Where ClassID = {ClassID}");
                    }
                    else
                    {
                        DataRowView rowGroup = (DataRowView)CBGroups.SelectedItem;
                        if (rowGroup == null)
                        {
                            groupIDquery = "";
                        }
                        else
                        {
                            groupIDquery += rowGroup["GroupID"].ToString();
                        }
                    }
                    FillDG("Students", $"WHERE Class_Student.ClassID = '" + ClassID + "'" + groupIDquery);
                }
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void DGStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)TComboBox.SelectedItem;
            if (row == null)
            {
                return;
            }
            string ID = row["ID"].ToString();
            if (ID == "-1")
            {
                TeacherAdd TADD = new TeacherAdd("Add", "-1");
                TADD.ShowDialog();
                string TID = TADD.ResponseText;
                if (TID == "-1")
                {
                    MessageBox.Show("No Teacher Was Added");
                    TComboBox.SelectedIndex = -1;
                    return;
                }
                DataTable DTTeacher = new DataTable();
                Connexion.FillDT(ref DTTeacher, "Select ID,TFirstName + ' ' + TLastName As Name ,TGender,'" + Connexion.GetImagesFile() + "\\" + "T' + Convert(Varchar(50),ID)  + '.jpg' as Picture From Teacher where status = 1");
                DataRow newRow = DTTeacher.NewRow();

                // Assign values to the new row
                newRow["ID"] = -1;
                newRow["Name"] = "New Teacher";
                newRow["TGender"] = -1;
                newRow["Picture"] = "";
                DTTeacher.Rows.Add(newRow);
                TComboBox.ItemsSource = DTTeacher.DefaultView;
                TComboBox.SelectedValue = TID;
            }
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void DGGroups_CellEditEnding_1(object sender, DataGridCellEditEndingEventArgs e)
        {

        }

        private void DGStudent_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ChangeGroup_Click(object sender, RoutedEventArgs e)
        {
            if (DGStudent.SelectedItems.Count > 0)
            {
                DataTable dtlist = new DataTable();
                Connexion.FillDT(ref dtlist, "Select *,GroupID as ID from Groups Where ClassID = " + ClassID);
                Panels.DataGridWithSearch searchWindow = new Panels.DataGridWithSearch("Groups", dtlist);
                bool? result = searchWindow.ShowDialog();
                int selectedGroupID;
                if (result == true)
                {
                    var selectedValue = searchWindow.SelectedValue;
                     selectedGroupID = int.Parse(selectedValue.ToString());
                }
                else
                {
                    return;
                }
                if (DGStudent.ItemsSource is DataView dataView)
                {

                    // Loop through all selected items
                    foreach (var selectedItem in DGStudent.SelectedItems)
                    {
                        if (selectedItem is DataRowView dataRowView)
                        {
                            int studentID = (int)dataRowView["StudentID"];

                            // Retrieve the selected GroupID.
                            int CurrentGID = (int)dataRowView["GroupID"];
                            if (selectedGroupID != CurrentGID)
                            {
                                Commun.ChangeGroupForever(studentID.ToString(), selectedGroupID.ToString(), DateTime.Today.ToString("dd-MM-yyyy"));
                                /* int startSes = Connexion.GetInt("Select Session from Class_Student Where StudentID = " + studentID + " and ClassID  = " + ClassID);
                                int EndSes;
                                int Gid = Connexion.GetGroupID(studentID.ToString(), ClassID.ToString());
                                if (Connexion.IFNULLVar("Select EndSession from Class_Student Where StudentID = " + studentID + " and ClassID  = " + ClassID))
                                {
                                    EndSes = Connexion.GetInt("Select Sessions from Groups Where GroupID = " + Gid);
                                    if (EndSes > Connexion.GetInt("Select Sessions from Groups Where GroupID = " + selectedGroupID))
                                    {
                                        EndSes = Connexion.GetInt("Select Sessions from Groups Where GroupID = " + selectedGroupID);
                                        for (int f = Connexion.GetInt("Select Sessions from Groups Where GroupID = " + selectedGroupID) + 1; f <= EndSes; f++)
                                        {
                                            int AttendID = Connexion.GetInt("Select ID from Attendance Where GroupID = " + Gid + " and Session =" + f);
                                            int stat = Connexion.GetInt("Select case when Status is null then 0 else Status end as f  from Attendance_Student Where ID =" + AttendID + " and StudentID =" + studentID);
                                            if (stat == 0)
                                            {
                                                Connexion.Insert("Delete from Attendance_Student Where ID =" + AttendID + " and StudentID =" + studentID);
                                            }
                                            else if (stat == 1)
                                            {
                                                Connexion.Insert("Insert Into Attendance_Change Values (" + studentID + "," + selectedGroupID + ", " + Gid + "," + f + ")");
                                            }

                                        }
                                    }
                                    else if (EndSes < Connexion.GetInt("Select Sessions from Groups Where GroupID = " + selectedGroupID))
                                    {
                                        for (int f = EndSes + 1; f <= Connexion.GetInt("Select Sessions from Groups Where GroupID = " + selectedGroupID); f++)
                                        {
                                            int AttendID = Connexion.GetInt("Select ID from Attendance Where GroupID = " + selectedGroupID + " and Session =" + f);
                                            if (Connexion.IFNULL("Select * from Attendance_Student Where ID =" + AttendID + " and StudentID =" + studentID))
                                            {
                                                Connexion.Insert("Insert into attendance_Student(ID,StudentID,status,Note) Values (" + AttendID + "," + studentID + ",null,'')");
                                            }
                                            else
                                            {
                                                Connexion.Insert("Delete from Attendance_Change Where ToGroupID = " + selectedGroupID + " and Session =" + f + " and StudentID =" + studentID);// if the student went and studied to the destined groupchange then delete it 
                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    EndSes = Connexion.GetInt("Select EndSession from Class_Student Where StudentID = " + studentID + " and ClassID  = " + ClassID);
                                }
                                for (int i = startSes; i <= EndSes; i++)
                                {
                                    int AID = Connexion.GetInt("Select ID from Attendance Where GroupID = " + Gid + " and session = " + i);
                                    int AIDNew = Connexion.GetInt("Select ID from Attendance Where GroupID = " + selectedGroupID + " and session = " + i);
                                    int Status = Connexion.GetInt("Select  Case When  Status  is null then 0 else Status end as f from Attendance_Student Where StudentID = " + studentID + " and ID =" + AID);
                                    if (Status == 0)
                                    {
                                        Connexion.Insert("Delete From Attendance_Student Where StudentID = " + studentID + " and ID =" + AID);
                                        Connexion.Insert("Insert Into Attendance_Student(ID,StudentID,Status,Note) Values (" + AIDNew + "," + studentID + ",0,'')");
                                        Commun.SetStatusAttendance(studentID.ToString(), AIDNew.ToString(), 0);
                                    }
                                    else if (Status == 1)
                                    {
                                        Connexion.Insert("Insert Into Attendance_Student(ID,StudentID,Status,Note) Values (" + AIDNew + "," + studentID + ",2,'')");
                                        Commun.SetStatusAttendance(studentID.ToString(), AIDNew.ToString(), 2);
                                        Connexion.Insert("Insert Into Attendance_Change  Values (" + studentID + "," + selectedGroupID + "," + Gid + " , " + i + ")");
                                    }
                                    else if (Status == 2)
                                    {
                                        int ToGroupID = Connexion.GetInt("Select ToGroupID From Attendance_Change Where StudentID = " + studentID + " and FromGroupID = " + Gid + " and Session = " + i);
                                        if (ToGroupID == selectedGroupID)
                                        {
                                            Connexion.Insert("Delete  From Attendance_Change Where StudentID = " + studentID + " and FromGroupID = " + Gid + " and Session = " + i + " and ToGroupID = " + ToGroupID);
                                            Connexion.Insert("delete from  Attendance_Student Where StudentID = " + studentID + " and ID =" + AID);

                                        }
                                        else
                                        {
                                            Connexion.Insert("delete from  Attendance_Student Where StudentID = " + studentID + " and ID =" + AID);
                                            Connexion.Insert("Insert Into Attendance_Student(ID,StudentID,Status,Note) Values (" + AIDNew + "," + studentID + ",2,'')");
                                            Commun.SetStatusAttendance(studentID.ToString(), AIDNew.ToString(), 2);

                                            Connexion.Insert("Update  Attendance_Change Set FromGroupID =" + selectedGroupID + " Where ToGroupID = " + ToGroupID + " and StudentID = " + studentID + " and Session =" + i);

                                        }
                                    }
                                    else if (Status == 3)
                                    {
                                        Connexion.Insert("delete from  Attendance_Student Where StudentID = " + studentID + " and ID =" + AID);
                                        Connexion.Insert("Insert Into Attendance_Student(ID,StudentID,Status,Note) Values (" + AIDNew + "," + studentID + ",3,'')");
                                        Commun.SetStatusAttendance(studentID.ToString(), AIDNew.ToString(), 3);
                                        Connexion.Insert("Update  justif  Set AID =" + AIDNew + " Where SID = " + studentID + " and AID = " + AID + " and Session =" + i);
                                    }

                                }

                                Connexion.Insert("Update Class_Student Set GroupID =" + selectedGroupID + " Where GroupID =" + Gid + " and StudentID =" + studentID);
                                MessageBox.Show("Changed Groups Succesfully");*/
                            }
                        }
                    }

                }
                string groupIDquery = " And Class_Student.GroupID = ";
                if (S == "Single")
                {
                    groupIDquery += Connexion.GetString($"Select GroupID from Groups Where ClassID = {ClassID}");
                }
                else
                {
                    DataRowView rowGroup = (DataRowView)CBGroups.SelectedItem;
                    if (rowGroup == null)
                    {
                        groupIDquery = "";
                    }
                    else
                    {
                        groupIDquery += rowGroup["GroupID"].ToString();
                    }
                }
                FillDG("Students", $"WHERE Class_Student.ClassID = '" + ClassID + "'" + groupIDquery);
                FillDG("Attendance", "");
            }
        }

        private void ChangeStartSes_Click(object sender, RoutedEventArgs e)
        {
            string GID = "";
            if (CBGroups.IsVisible == true)
            {
                if (CBGroups.SelectedIndex != -1)
                {
                    DataRowView Group = (DataRowView)CBGroups.SelectedItem;
                    GID = Group["GroupID"].ToString();
                }
                else
                {
                    return;
                }
            }
            else
            {
                GID = Connexion.GetString("Select GroupID from Groups Where ClassID = " + ClassID);
            }
            if (DGStudent.ItemsSource is DataView)
            {
                // Loop through all selected items
                foreach (var selectedItem in DGStudent.SelectedItems)
                {
                    if (selectedItem is DataRowView row)
                    {
                        Commun.ChangeStartSesStudent(row["StudentID"].ToString(), row["GroupID"].ToString());
                    }
                }
            }
            string groupIDquery = " And Class_Student.GroupID = " ;
            if (S == "Single")
            {
                groupIDquery += Connexion.GetString($"Select GroupID from Groups Where ClassID = {ClassID}");
            }
            else
            {
                DataRowView rowGroup = (DataRowView)CBGroups.SelectedItem;
                if (rowGroup == null)
                {
                    groupIDquery ="";
                }
                else
                {
                    groupIDquery += rowGroup["GroupID"].ToString();
                }
            }
            FillDG("Students",$"WHERE Class_Student.ClassID = '" + ClassID + "'" + groupIDquery);
            FillDG("Attendance", "");
        }

        private void ChangeEndSes_Click(object sender, RoutedEventArgs e)
        {
            string GID = "";
            if(CBGroups.IsVisible == true)
            {
                if (CBGroups.SelectedIndex != -1)
                {
                    DataRowView Group = (DataRowView)CBGroups.SelectedItem;
                    GID = Group["GroupID"].ToString();
                }
                else
                {
                    return;
                }
            }
            else
            {
                GID = Connexion.GetString("Select GroupID from Groups Where ClassID = " + ClassID);
            }
           /* DataTable dtlist = new DataTable();
            Connexion.FillDT(ref dtlist, "Select Date,Session   as Ses from Attendance Where GroupID = " + GID + " ORDER BY CONVERT(DATE, Date, 105) DESC ");
            DataRow newRow = dtlist.NewRow();
            // Set values for the new row
            newRow["Date"] = "Back to Studying";  // Example: Set current date
            newRow["Ses"] = "-1";         // Example: Set 'Ses' column value
            dtlist.Rows.Add(newRow);
            DataRow newRow2 = dtlist.NewRow();
            newRow2["Date"] = "Remove Completly";  // Example: Set current date
            newRow2["Ses"] = "-2";                              // Example: Set 'Ses' column value
            dtlist.Rows.Add(newRow2);
            Panels.DataGridWithSearch dgsearch = new Panels.DataGridWithSearch("Sessions", dtlist);
            bool? result = dgsearch.ShowDialog();
            string NewEndSession;
            if (result == true)
            {
                var selectedValue = dgsearch.SelectedValue;
                NewEndSession = selectedValue.ToString();
            }
            else
            {
                return;
            }*/
            if (DGStudent.ItemsSource is DataView dataView)
            {

                // Loop through all selected items
                foreach (var selectedItem in DGStudent.SelectedItems)
                {
                    if (selectedItem is DataRowView row)
                    {
                        Commun.LeaveGroup(row["studentID"].ToString(), row["GroupID"].ToString());
                      /*  int oldEndSession;
                        if (Connexion.IFNULLVar("Select EndSession from Class_Student Where StudentID = " + row["StudentID"].ToString() + " and GroupID = " + row["GroupID"].ToString()))
                        {
                            oldEndSession = Connexion.GetInt("Select Sessions from Groups Where GroupID = " + row["GroupID"].ToString());
                        }
                        else
                        {
                            oldEndSession = Connexion.GetInt("Select EndSession from Class_Student Where StudentID = " + row["StudentID"].ToString() + " and GroupID = " + row["GroupID"].ToString());
                        }
                        if (NewEndSession > 0)
                        {
                            if (NewEndSession > oldEndSession)
                            {
                                for (int i = oldEndSession; i < NewEndSession; i++)
                                {
                                    int ses = i + 1;
                                    int aid = Connexion.GetInt("Select ID from Attendance Where Session = " + ses + " and GroupID = " + row["GroupID"].ToString());
                                    Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status,Note) values(" + aid + "," + row["StudentID"].ToString() + ",1,null)");
                                    Commun.SetStatusAttendance(row["StudentID"].ToString(), aid.ToString(), 1);
                                }
                            }
                            else
                            {
                                for (int i = oldEndSession; NewEndSession < i; i--)
                                {
                                    int ses = i;
                                    int aid = Connexion.GetInt("Select ID from Attendance Where Session = " + ses + " and GroupID = " + row["GroupID"].ToString());
                                    Connexion.Insert("Delete From Attendance_Student where StudentID = " + row["StudentID"].ToString() + " and ID = " + aid);

                                }
                            }
                            Connexion.Insert("Update Class_Student Set EndSession = " + NewEndSession + " Where StudentID = " + row["StudentID"].ToString() + " and GroupID = " + row["GroupID"].ToString());
                        }
                        else if (NewEndSession == -1 )
                        {
                            int EndSession = Connexion.GetInt("Select case when  EndSession is null then -1 else EndSession End as F from Class_Student Where StudentID = " + row["StudentID"].ToString() + " and GroupID = " + row["GroupID"].ToString());
                            if (EndSession == -1)
                            {
                                continue;
                            }
                            int EndSessionForGroup = Connexion.GetInt("Select Sessions from Groups Where GroupID = " + row["GroupID"].ToString());
                            for (int i = EndSession; i < EndSessionForGroup; i++)
                            {
                                int ses = i + 1;
                                int aid = Connexion.GetInt("Select ID from Attendance Where Session = " + ses + " and GroupID = " + row["GroupID"].ToString());
                                Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status,Note) values(" + aid + "," + row["StudentID"].ToString() + ",3,null)");
                                Commun.SetStatusAttendance(row["StudentID"].ToString(), aid.ToString(), 3);
                                int CID = Connexion.GetClassID(row["GroupID"].ToString());
                                Connexion.Insert("Insert Into Justif Values  (" + row["StudentID"].ToString() + " , " + CID + " , N'" + this.Resources["JustifStoppedGroup"].ToString() + "', " + ses + " ," + aid + " )");
                                Connexion.Insert("Update Class_Student Set EndSession = null  Where StudentID = " + row["StudentID"].ToString() + " and GroupID = " + row["GroupID"].ToString());
                            }
                        }
                        else if (NewEndSession == -2)
                        {
                            Connexion.Insert("Delete from Class_Student Where GroupID = " + GID + " and StudentID = " + row["StudentID"].ToString());
                            DataTable dtAttendance = new DataTable();
                            Connexion.FillDT(ref dtAttendance, "Select ID from Attendance Where GroupID = " + GID);
                            for (int i = 0; i < dtAttendance.Rows.Count; i++)
                            {
                                Connexion.Insert("Delete from Attendance_Student Where StudentID = " + row["StudentID"].ToString() + " and ID = " + dtAttendance.Rows[i]["ID"].ToString());
                            }
                        }*/
                    }
                }
            }
            string groupIDquery = " And Class_Student.GroupID = ";
            if (S == "Single")
            {
                groupIDquery += Connexion.GetString($"Select GroupID from Groups Where ClassID = {ClassID}");
            }
            else
            {
                DataRowView rowGroup = (DataRowView)CBGroups.SelectedItem;
                if (rowGroup == null)
                {
                    groupIDquery = "";
                }
                else
                {
                    groupIDquery += rowGroup["GroupID"].ToString();
                }
            }
            FillDG("Students", $"WHERE Class_Student.ClassID = '" + ClassID + "'" + groupIDquery);
            FillDG("Attendance", "");

        }

        private void AddStudentdoubleclck_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                CBStudent.SelectedIndex = -1;
                DataRowView row = (DataRowView)CYear.SelectedItem;
                string groupID = "";
                if (S == "Single")
                {
                    groupID = Connexion.GetString($"Select GroupID from Groups Where ClassID = {ClassID}");
                }
                else
                {
                    DataRowView rowGroup = (DataRowView)CBGroups.SelectedItem;
                    if (rowGroup == null)
                    {
                        return;
                    }
                    groupID = rowGroup["GroupID"].ToString();
                }
                string query = Commun.GetStudentsNotInGroupQuery(groupID);
                Panels.List l = new Panels.List(query, groupID);
                l.ShowDialog();
                string groupIDquery = " And Class_Student.GroupID = ";
                if (S == "Single")
                {
                    groupIDquery += Connexion.GetString($"Select GroupID from Groups Where ClassID = {ClassID}");
                }
                else
                {
                    DataRowView rowGroup = (DataRowView)CBGroups.SelectedItem;
                    if (rowGroup == null)
                    {
                        groupIDquery = "";
                    }
                    else
                    {
                        groupIDquery += rowGroup["GroupID"].ToString();
                    }
                }
                FillDG("Students", $"WHERE Class_Student.ClassID = '" + ClassID + "'" + groupIDquery);
                FillDG("Attendance", "");
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }
    }
}

