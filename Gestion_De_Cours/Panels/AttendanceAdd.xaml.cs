using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
using FastReport;
using FastReport.Utils;
using System.IO;
using System.ComponentModel;
using Gestion_De_Cours.UControl;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Globalization;

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for AttendanceAdd.xaml
    /// </summary>
    public partial class AttendanceAdd : Window
    {
        string GID;
        string AID;
        string Typ;
        string CID;
        int SessionA;
        int CType;
        string date;
        string S;
        string queryforexception = "";
        string FID = "";
        string ty2 = "";//1 for normal 2 formation 3 extra session
        int Price;
        int TPrice;
        string querydatagrid = "";
        DataTable dtMain = new DataTable();
        private DispatcherTimer closeTimer;
        private DispatcherTimer inactivityTimer;
        bool showmessagetimechange = true;
        int stop = 0;
        bool reaccurent = true; 

        public AttendanceAdd(string AttendanceID, string Type , string type2 )
        {
            try
            {
                int lang = Connexion.Language();
                InitializeComponent();
                SetLang();
                ty2 = type2; 
                AID = AttendanceID;
               
                if (lang == 1)
                {
                    this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
                }
                if (type2 == "2")
                {
                    FID = Connexion.GetString(AID, "Formation_Attendance", "FID");
                    string TimeID = Connexion.GetString("Select TimeID from Formation_Attendance Where ID = " + AID);
                    ListFormation.Visibility = Visibility.Visible;
                    ListClass.Visibility = Visibility.Collapsed;
                  
                    RowAddStudent.Height = new GridLength(0);
                    TET.Visibility = Visibility.Collapsed;
                    TETLabel.Visibility = Visibility.Collapsed;
                    SPTime.Visibility = Visibility.Visible;
                    this.Title = Connexion.GetString("Select Name from formation where ID = " + FID);
                    this.Width = 550;
                    Note.Visibility = Visibility.Collapsed;
                    NoteLB.Visibility = Visibility.Collapsed;
                    if (Type == "Show")
                    {
                        queryforexception = "Select Formation_Student.SID as ID ,Students.BarCode as Barcode,Students.FirstName + ' ' + Students.LastName as Name , Students.LastName + ' ' + students.FirstName as RName , Students.Gender, Formation_Attendance_Student.Status  , Case " +
                            "When Formation_Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + "' " +
                             "When Formation_Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + "' " +
                             "When Formation_Attendance_Student.Status = 2 Then N'" + this.Resources["GroupChange"].ToString() + "' " +
                             "When Formation_Attendance_Student.Status = 3 Then N'" + this.Resources["Justified"].ToString() + "' END as StatusText , dbo.CalculateRemainingPayment(Formation_Attendance.FID ,Students.ID)  as Sessions  from Formation_Attendance join Formation_Attendance_Student on Formation_Attendance_Student.AID =Formation_Attendance.ID Join Students on Students.ID = Formation_Attendance_Student.SID join Formation_Student on Formation_Student.FID = Formation_Attendance.FID and Formation_Attendance_Student.SID = Formation_Student.SID where Formation_Attendance.ID = " + AID;
                        int duration =Connexion.GetInt("Select Duration from Formation_Attendance Where ID =" + AID);
                        int minute = duration % 60;
                        CBMinute.SelectedIndex = minute / 15;
                        CBHour.SelectedIndex = duration / 60;
                        Date.Text = Connexion.GetString("Select Date from Formation_Attendance WHere ID = " + AID);
                    }
                    else if (Type == "Add")
                    {
                        Date.Text = DateTime.Today.ToString("d");
                     
                        string TimeStart = Connexion.GetString("Select TimeStart from Class_time Where ID = " + TimeID);
                        string TimeEnd =Connexion.GetString("Select TimeEnd from Class_time Where ID = " + TimeID);
                        int HourStart = int.Parse(TimeStart.Substring(0, 2));
                        int MinuteStart = int.Parse(TimeStart.Substring(3, 2));
                        int HourEnd = int.Parse(TimeEnd.Substring(0, 2));
                        int MinuteEnd = int.Parse(TimeEnd.Substring(3, 2));
                        int Minute = 0;
                        int Hour = 0;
                        if(MinuteEnd  - MinuteStart < 0)
                        {
                            MinuteEnd += 60;
                            HourEnd -= 1;
                            
                        }
                        Minute = MinuteEnd - MinuteStart ;
                        Hour = HourEnd - HourStart;
                        Minute /= 15;
                        CBHour.SelectedIndex = Hour;
                        CBMinute.SelectedIndex = Minute;
                        int duration = (Hour * 60) + (Minute * 15);
                        Connexion.Insert("Update Formation_Attendance Set Duration = " + duration + " Where ID= "+ AID);
                        queryforexception = "Select Formation_Student.SID as ID ,Students.Barcode as Barcode,Students.FirstName + ' ' + Students.LastName as Name , Students.LastName + ' ' + students.FirstName as RName , Students.Gender, Formation_Attendance_Student.Status  , Case " +
                            "When Formation_Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + "' " +
                             "When Formation_Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + "' " +
                             "When Formation_Attendance_Student.Status = 2 Then N'" + this.Resources["GroupChange"].ToString() + "' " +
                             "When Formation_Attendance_Student.Status = 3 Then N'" + this.Resources["Justified"].ToString() + "' END as StatusText , dbo.CalculateRemainingPayment(Formation_Attendance.FID ,Students.ID)  as Sessions  from Formation_Attendance join Formation_Attendance_Student on Formation_Attendance_Student.AID =Formation_Attendance.ID Join Students on Students.ID = Formation_Attendance_Student.SID join Formation_Student on Formation_Student.FID = Formation_Attendance.FID and Formation_Attendance_Student.SID = Formation_Student.SID where Formation_Attendance.ID = " + AID;
                        Commun.FillFormationAttendance(AID);
                    }
                    Connexion.FillDT(ref dtMain, queryforexception);
                    ListFormation.DataContext = dtMain.DefaultView;
                }
                else if (type2 == "3")
                {   
                    AID = AttendanceID;
                    CID = Connexion.GetString("Select CID from Attendance_Extra Where ID = " + AID);
                    //ColReason.Visibility = Visibility.Collapsed;
                    //ColJustif.Visibility = Visibility.Collapsed;
                    TET.Visibility = Visibility.Collapsed;
                    TETLabel.Visibility = Visibility.Collapsed;
                    Note.Visibility = Visibility.Collapsed;
                    NoteLB.Visibility = Visibility.Collapsed;
                    btnPresent.Visibility = Visibility.Collapsed;
                    Date.Text = Connexion.GetString("Select Date from Attendance_Extra WHere ID = " + AID);
                    btnDelete.Content = this.Resources["Delete"].ToString();
                    CID = Connexion.GetString("Select CID from Attendance_Extra Where ID = " + AID);
                    Price = Connexion.GetInt("Select CPrice from Class Where ID=  " + CID) /4 ;
                    TPrice = Connexion.GetInt("Select TPayment From Class Where ID = " + CID) / 4;
                    queryforexception = "Select * from Groups Where ClassID = " + CID ;
                   
                    querydatagrid = "Select Students.ID as ID,Students.FirstName + ' ' + Students.LastName as Name , Students.LastName + ' ' + Students.FirstName As RName , Students.Barcode as Barcode , Attendance_Extra_students.Status as Status  ,  Case When Attendance_Extra_students.Status = 0 THEN N'" + this.Resources["Absent"].ToString() +  "' When Attendance_Extra_students.Status = 1 THEN N'" + this.Resources["Present"].ToString() + "' end as StatusText , dbo.CalcPriceSum(Attendance_Extra_Students.SID," + CID + ") as Sessions From Attendance_Extra_Students join Students on Attendance_Extra_Students.SID =  Students.ID Where Attendance_Extra_Students.ID = " + AID;
                    Connexion.FillDT(ref dtMain, querydatagrid);
                    ListClass.DataContext = dtMain.DefaultView;
                    Connexion.FillDTItem("Rooms", ref CRoom);
                    int roomid= Connexion.GetInt("Select case when RoomID is null then -1 else roomID end  from Attendance_Extra Where ID = " + AID);
                    if(roomid != -1)
                    {
                        CRoom.SelectedIndex = roomid;
                        HFrom.SelectedValue = Connexion.GetString("Select case when TimeStart is null then '00:00' else timestart end  from Attendance_Extra where ID = " + AID).Substring(0, 2);
                        MFrom.SelectedValue = Connexion.GetString("Select case when TimeStart is null then '00:00' else timestart end  from Attendance_Extra where ID = " + AID).Substring(3, 2);
                        HTo.SelectedValue = Connexion.GetString("Select  case when TimeEnd is null then '00:00' else TimeEnd end as f  from Attendance_Extra where ID = " + AID).Substring(0, 2);
                        MTo.SelectedValue = Connexion.GetString("Select  case when TimeEnd is null then '00:00' else TimeEnd end as f  from Attendance_Extra where ID = " + AID).Substring(3, 2);
                    }

                }
                else
                {
                    BtnExtraStudents.Visibility = Visibility.Visible; 
                    ContextMenu contextMenu = new ContextMenu();
                    // Create payment Menu Item
                    MenuItem PaymentMenuItem = new MenuItem();
                    PaymentMenuItem.Header = this.Resources["Payment"].ToString();
                    PaymentMenuItem.Click += BtnPayment_Click;

                    // Set the icon for payment
                    Image PaymentImage = new Image();
                    PaymentImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\Payment512Content.png")); // Use your image path here
                    PaymentImage.Width = 16;
                    PaymentImage.Height = 16;
                    PaymentMenuItem.Icon = PaymentImage;

                    // Create payment Menu Item
                    MenuItem JustifMenuItem = new MenuItem();
                    JustifMenuItem.Header = this.Resources["Justification"].ToString();
                    JustifMenuItem.Click += BtnJustif_Click;

                    // Set the icon for justif
                    Image JustifImage = new Image();
                    JustifImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\absent.png")); // Use your image path here
                    JustifImage.Width = 16;
                    JustifImage.Height = 16;
                    JustifMenuItem.Icon = JustifImage;


                    // Create Discount Menu Item
                    MenuItem discountMenuItem = new MenuItem();
                    discountMenuItem.Header = this.Resources["Discount"].ToString();
                    discountMenuItem.Click += DiscountMenuItem_Click;

                    // Set the icon for Discount
                    Image discountImage = new Image();
                    discountImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\DiscountContent.png")); // Use your image path here
                    discountImage.Width = 16;
                    discountImage.Height = 16;
                    discountMenuItem.Icon = discountImage;

                    // Create Leave Group Menu Item
                    MenuItem leaveGroupMenuItem = new MenuItem();
                    leaveGroupMenuItem.Header = this.Resources["LeaveGroup"].ToString();
                    leaveGroupMenuItem.Click += LeaveMenuItem_Click;

                    // Set the icon for Leave Group
                    Image leaveGroupImage = new Image();
                    leaveGroupImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\LeaveContent.png")); // Use your image path here
                    leaveGroupImage.Width = 16;
                    leaveGroupImage.Height = 16;
                    leaveGroupMenuItem.Icon = leaveGroupImage;

                    // Create Change Payment Menu Item
                    MenuItem changePaymentMenuItem = new MenuItem();
                    changePaymentMenuItem.Header = this.Resources["Olddebt"].ToString();
                    changePaymentMenuItem.Click += ChangePaymentItem_Click;

                    // Set the icon for Change Payment
                    Image changePaymentImage = new Image();
                    changePaymentImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\debt.png")); // Use your image path here
                    changePaymentImage.Width = 16;
                    changePaymentImage.Height = 16;
                    changePaymentMenuItem.Icon = changePaymentImage;

                    // Add MenuItems to ContextMenu
                    contextMenu.Items.Add(PaymentMenuItem);
                    contextMenu.Items.Add(JustifMenuItem);
                    contextMenu.Items.Add(discountMenuItem);
                    contextMenu.Items.Add(leaveGroupMenuItem);
                    contextMenu.Items.Add(changePaymentMenuItem);

                    // Attach ContextMenu to DataGrid
                    ListClass.ContextMenu = contextMenu;

                    // Optionally, you can add a MouseRightButtonUp event to trigger the ContextMenu
                    ListClass.MouseRightButtonUp += DataGrid_MouseRightButtonUp;
                    AID = AttendanceID;
                    GID = Connexion.GetInt(AID, "Attendance", "GroupID").ToString();
                    CID = Connexion.GetClassID(GID).ToString();
                    this.Title = Connexion.GetString("Select CName + ' ' + GroupName from Groups Join Class on Class.ID = Groups.CLassID Where Groups.GroupID = " + GID);
                    string Setting = Connexion.GetString(CID, "Class", "MultipleGroups");
                    S = Setting;

                    SqlConnection con = Connexion.Connect();
                    SessionA = Connexion.GetInt(AID, "Attendance", "Session");
                    string ClassID = Connexion.GetString("Select ClassID from Groups Where GroupID = '" + GID + "'");
                    CType = Connexion.GetInt(CID, "Class", "TPaymentMethod");
                    Typ = Type;
                    string querySession = "";
                    int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                    if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                    {
                        int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                        monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                    }
                    if (monthlypayment == 1)
                    {
                        string dateAttendanceString = Connexion.GetString(AID, "Attendance", "Date");
                        DateTime dateAttendance = DateTime.Parse(dateAttendanceString);
                        int monthNumber = dateAttendance.Month;
                        int year = dateAttendance.Year;
                        querySession = "dbo.CalculateMonthlyPaymentRemaining(Students.ID  ) ";
                    }
                    else if( monthlypayment == 0)
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
                    
                    if (Type == "Show")
                    {
                        querydatagrid = Commun.GetQueryDataTable("Attendance",AID);
                        Connexion.FillDT(ref dtMain, querydatagrid);
                        ListClass.DataContext = dtMain.DefaultView;
                        Note.Text = Connexion.GetString(AID, "Attendance", "Note");
                        TET.Text = Connexion.GetString(AID, "Attendance", "TeacherEntrance");
                        Date.Text = Connexion.GetString(AID, "Attendance", "Date");
                    }
                    else if (Type == "Add")
                    {
                        Date.Text = Connexion.GetString("Select Date from Attendance Where ID = " + AID);
                        Commun.FillAttendance(AID);
                        querydatagrid = Commun.GetQueryDataTable("Attendance", AID);

                        Connexion.FillDT(ref dtMain, querydatagrid);
                        ListClass.DataContext = dtMain.DefaultView;

                    }
                    date = Connexion.GetString("Select Date from Attendance Where ID = " + AID);
                    Connexion.FillDTItem("Rooms", ref CRoom);
                    CRoom.SelectedValue = Connexion.GetInt("Select RoomID from Attendance Where ID = " + AID);
                    HFrom.SelectedValue = Connexion.GetString("Select TimeStart from attendance where ID = " + AID).Substring(0, 2);
                    MFrom.SelectedValue = Connexion.GetString("Select TimeStart from attendance where ID = " + AID).Substring(3, 2);
                    HTo.SelectedValue = Connexion.GetString("Select TimeEnd from attendance where ID = " + AID).Substring(0, 2);
                    MTo.SelectedValue = Connexion.GetString("Select TimeEnd from attendance where ID = " + AID).Substring(3, 2);
                    showmessagetimechange = false;
                }
              

            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void DiscountMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataRowView rowS = (DataRowView)ListClass.SelectedItem;
            if (rowS == null)
            {
                return;
            }
            
            Panels.Discount discpanel = new Panels.Discount(rowS["ID"].ToString(), CID);
            discpanel.ShowDialog();

            Connexion.FillDT(ref dtMain, querydatagrid);
            ListClass.DataContext = dtMain.DefaultView;

        }
        private void LeaveMenuItem_Click(object sender, RoutedEventArgs e)
        {

            if(ListClass.SelectedItems.Count > 1)
            {
                MessageBox.Show(this.Resources["SelectOneStudent"].ToString());
                return;
            }
            DataRowView rowS = (DataRowView)ListClass.SelectedItem;
            if(rowS == null)
            {
                MessageBox.Show(this.Resources["SelectOneStudent"].ToString());
                return;
            }
            if (Connexion.IFNULL("Select * from Class_Student Where StudentID = " + rowS["ID"].ToString() + " and GroupID = " + GID + " and (Stopped = '0' Or CONVERT(DATE, Stopped, 105) >= CONVERT(DATE, '" + Date.Text.Replace("/", "-") + "', 105) )"))
            {
                Connexion.Insert("Delete from Attendance_Student Where ID =" + AID + " and StudentID =" + rowS["ID"].ToString());
            }
            else
            {
                Commun.LeaveGroup(rowS["ID"].ToString(), GID);
            }
            Connexion.FillDT(ref dtMain, querydatagrid);
            ListClass.DataContext = dtMain.DefaultView;

        }
        private void ChangePaymentItem_Click(object sender, RoutedEventArgs e)
        {
            if (ListClass.SelectedItems.Count > 1)
            {
                if (MessageBox.Show("هل أنت متأكد أنك تريد تغيير السعر لجميع الطلاب المحددين؟", "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            foreach (DataRowView row in ListClass.SelectedItems)
            {
                Commun.ChangeInitialPrice(row["ID"].ToString(),GID);
            }
            Connexion.FillDT(ref dtMain, querydatagrid);
            ListClass.DataContext = dtMain.DefaultView;


        }
        private void DataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

            ListClass.ContextMenu.IsOpen = true;
        }
  /*      private void CBGroup_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            try
            {
                var combo = sender as DevExpress.Xpf.Editors.ComboBoxEdit;
                if (combo?.SelectedItem is DataRowView row)
                {
                    string GroupChange = row["GroupID"].ToString();

                    if (ty2 == "1")
                    {
                        if (GroupChange != "-1")
                        {
                            string query = "Select Students.FirstName + ' ' + Students.LastName  as Name ,Students.ID ,Students.Gender as Gender,'" + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "//MyPhotos\\" + "S' + Convert(Varchar(50),Students.ID)  + '.jpg' as Picture  from Students join Class_Student On Class_Student.StudentID = Students.ID join Groups On Groups.GroupID = Class_Student.GroupID Where Class_Student.ClassID = " + CID + " and groups.GroupID = " + GroupChange + " Except  Select Students.FirstName + ' ' + Students.LastName as Name ,Students.Gender as Gender,Students.ID , '" + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "//MyPhotos\\" + "S' + Convert(Varchar(50),Students.ID)  + '.jpg' as Picture from Students join Class_Student On Class_Student.StudentID = Students.ID join Attendance_Student on Attendance_Student.StudentID = Students.ID Where Attendance_Student.ID =" + AID;
                            Connexion.FillCB(ref CBStudent, query);
                            SPTypeChange.Visibility = Visibility.Visible;
                            DateGroupChange.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            SPTypeChange.Visibility = Visibility.Collapsed;
                            DateGroupChange.Visibility = Visibility.Collapsed;
                            int yearid = Connexion.GetInt("Select CYear From Class Where ID = " + CID);
                            string query = "SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students  Where  Students.Status = 1 and Year = " + yearid;
                            DataTable dtSpec = new DataTable();
                            Connexion.FillDT(ref dtSpec, "Select SpecID from Class_Speciality Where ID = " + CID);
                            string Specid = "";
                            for (int i = 0; i < dtSpec.Rows.Count; i++)
                            {
                                Specid = dtSpec.Rows[i]["SpecID"].ToString();
                                if (i == 0)
                                    query += "AND SPECIALITY = " + Specid;
                                else
                                    query += "OR SPECIALITY = " + Specid;
                            }
                            string gender = Connexion.GetString("Select GroupGender from Groups Where GroupID =  " + GID);
                            if (gender == "1" || gender == "0")
                            {
                                query += "AND Students.Gender =" + gender;
                            }
                            query += " Except SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students Join Class_Student On Class_Student.StudentID = Students.ID   Where Students.Status = 1 and  Class_Student.ClassID = " + CID + " and Class_Student.Stopped != '0' ORDER BY Name ASC";
                            DataTable dtStudents = new DataTable();
                            Connexion.FillDT(ref dtStudents, query);
                            DataRow extraRow = dtStudents.NewRow();
                            extraRow["ID"] = -1;
                            extraRow["Name"] = "New Student";
                            dtStudents.Rows.InsertAt(extraRow, 0);
                            CBStudent.ItemsSource = dtStudents.DefaultView;
                        }
                    }
                    else if (ty2 == "3")
                    {
                        if (GroupChange != "-1")
                        {
                            string query = "Select Students.FirstName + ' ' + Students.LastName  as Name ,Students.ID ,Students.Gender as Gender,'" + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "//MyPhotos\\" + "S' + Convert(Varchar(50),Students.ID)  + '.jpg' as Picture  from Students join Class_Student On Class_Student.StudentID = Students.ID join Groups On Groups.GroupID = Class_Student.GroupID Where Class_Student.ClassID = " + CID + " and groups.GroupID = " + GroupChange + " and Class_Student.EndSession is null Except Select Students.FirstName + ' ' + Students.LastName  as Name ,Students.ID ,Students.Gender as Gender,'" + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "//MyPhotos\\" + "S' + Convert(Varchar(50),Students.ID)  + '.jpg' as Picture from  Attendance_Extra_Students Join Students on Students.ID = Attendance_Extra_Students.SID  where Attendance_Extra_students.ID =   " + AID;
                            Connexion.FillCB(ref CBStudent, query);
                        }
                        else
                        {
                            int yearid = Connexion.GetInt("Select CYear From Class Where ID = " + CID);
                            string query = "SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),Students.ID)  + '.jpg' as Picture from Students  Where  Students.Status = 1 and Year = " + yearid;
                            DataTable dtSpec = new DataTable();
                            Connexion.FillDT(ref dtSpec, "Select SpecID from Class_Speciality Where ID = " + CID);
                            string Specid = "";
                            for (int i = 0; i < dtSpec.Rows.Count; i++)
                            {
                                Specid = dtSpec.Rows[i]["SpecID"].ToString();
                                if (i == 0)
                                    query += "AND SPECIALITY = " + Specid;
                                else
                                    query += "OR SPECIALITY = " + Specid;
                            }

                            query += " Except SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),Students.ID)  + '.jpg' as Picture from Students Join Attendance_Extra_Students On Attendance_Extra_Students.SID = Students.ID   Where Students.Status = 1 and  Attendance_Extra_Students.ID = " + AID + "  ORDER BY Name ASC";
                            DataTable dtStudents = new DataTable();
                            Connexion.FillDT(ref dtStudents, query);
                            DataRow extraRow = dtStudents.NewRow();
                            extraRow["ID"] = -1;
                            extraRow["Name"] = "New Student";
                            dtStudents.Rows.InsertAt(extraRow, 0);
                            CBStudent.ItemsSource = dtStudents.DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }*/

    /*    private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView rowStudent = (DataRowView)CBStudent.SelectedItem;
                DataRowView rowgroup = (DataRowView)CBGroup.SelectedItem;
                if (rowStudent == null)
                {
                    return;
                }
                string SGID = rowgroup["GroupID"].ToString();
                if (SGID != "-1")//group change and not inserting new student 
                {
                    if (ty2 == "1")
                    {
                        string StudentID = rowStudent["ID"].ToString();
                        if (OneTime.IsChecked == true)
                        {
                            string DateFrom = DateGroupChange.Text;
                            if (DateFrom == "")
                            {
                                return;
                            }
                            if (MessageBox.Show("Do you want To Add This Student?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                Commun.ChangeGroupOneTime(StudentID, GID, AID, DateFrom);
                            }
                        }
                        else
                        {
                            string DateFrom = DateGroupChange.Text;
                            Commun.ChangeGroupForever(StudentID, GID, Date.Text.Replace("/", "-"));
                        }
                    }
                    else if (ty2 == "3")
                    {
                    }
                }
                else
                {
                    string StudentID = rowStudent["ID"].ToString();
                    if (StudentID == "-1")
                    {
                        StudentAdd SAdd = new StudentAdd("Add", "-1");
                        if (SAdd.ShowDialog() == true)
                        {
                            StudentID = SAdd.ResponseText;
                        }
                        else
                        {
                            MessageBox.Show("No Student Was Added");
                            return;
                        }
                    }
                    Commun.AddStudentToClass(StudentID, GID, Date.Text.Replace("/", "-"));
                }
                string querySession = "";
                int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                {
                    int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                    monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                }

                if (monthlypayment == 1)
                {
                    string dateAttendanceString = Connexion.GetString(AID, "Attendance", "Date");
                    DateTime dateAttendance = DateTime.Parse(dateAttendanceString);
                    int monthNumber = dateAttendance.Month;
                    int year = dateAttendance.Year;
                    querySession = "dbo.CalculateMonthlyPaymentRemaining(Students.ID) ";
                }
                else
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

                /* DataRowView rowgroup = (DataRowView)CBGroup.SelectedItem;
 if (rowgroup == null)
 {
     return;
 }

 string SGID = rowgroup["GroupID"].ToString();
 if (SGID != "-1")
 {

     if (ty2 == "1")
     {
         if (MessageBox.Show("Do you want To Add This Student?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
         {

             int sessiongroup = int.Parse(rowgroup["Sessions"].ToString());
             if (sessiongroup < SessionA) //G2->G1 // verify if the group is before or after 
             {
                 DataRowView row = (DataRowView)CBStudent.SelectedItem;
                 string StudentID = row["ID"].ToString();
                 Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status) Values (" + AID + "," + StudentID + ",1)");
                 Commun.SetStatusAttendance(StudentID, AID, 1);
                 Connexion.Insert("Insert into Attendance_Change values (" + StudentID + "," + rowgroup["GroupID"].ToString() + "," + GID + "," + SessionA + ")");
                 //RadioButton btn = sender as RadioButton;
                 //btn.Background = Brushes.Green;
                 //btn.Foreground = Brushes.Green;
             }
             else
             {
                 DataRowView row = (DataRowView)CBStudent.SelectedItem;
                 string StudentID = row["ID"].ToString();

                 int result = Connexion.GetInt("Select ID From Attendance Where GroupID = '" + rowgroup["GroupID"].ToString() + "' And Session = " + SessionA);
                 Commun.SetStatusAttendance(StudentID, result.ToString(), 2);
                 Connexion.Insert("Insert into Attendance_Change values (" + StudentID + "," + rowgroup["GroupID"].ToString() + "," + GID + "," + SessionA + ")");
                 Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status) Values (" + AID + "," + StudentID + " , 1 )");
                 Commun.SetStatusAttendance(StudentID, AID, 1);
             }

         }
         else
         {
             return;
         }
     }
     else if (ty2 == "3")
     {
         DataRowView row = (DataRowView)CBStudent.SelectedItem;
         string SID = row["ID"].ToString();
         string studentName = Connexion.GetString("Select FirstName + ' ' + LastName as Name from Students where ID = " + SID);

         bool exists = dtMain.AsEnumerable().Any(row2 => row2.Field<Int32>("ID").ToString() == SID);

         if (!exists)
         {
             Connexion.Insert("Insert into Attendance_extra_Students values(" + AID + "," + SID + "," + 0 + "," + 0 + ",1)");
             DataRow dr = dtMain.NewRow();
             dr["ID"] = SID;
             dr["Name"] = studentName;
             dr["RName"] = Connexion.GetString("Select lastName + ' ' + FirstName as Name from Students Where ID =" + SID);
             dr["Sessions"] = 0;
             dr["Status"] = 1;
             dr["StatusText"] = this.Resources["Present"].ToString();
             dr["Barcode"] = Connexion.GetString("Select Barcode from Students Where ID = " + SID);
             dtMain.Rows.InsertAt(dr, 0);
         }
         else
         {
             MessageBox.Show("هذا التلميذ مسجل مسبقًا");
         }
     }
 }
 else
 {
     if (ty2 == "1")
     {
         DataRowView row = (DataRowView)CBStudent.SelectedItem;
         if (row == null)
         {
             return;
         }
         string StudentID = row["ID"].ToString();
         if (!Commun.CheckSeatsClass(GID, this.Resources["WarningSeatsMax"].ToString()))
         {
             return;
         }
         if (StudentID == "-1")
         {
             StudentAdd SAdd = new StudentAdd("Add", "-1");
             if (SAdd.ShowDialog() == true)
             {
                 StudentID = SAdd.ResponseText;
             }
             else
             {
                 MessageBox.Show("No Student Was Added");
                 return;
             }
         }
         int SessionNumber = Connexion.GetInt("Select Session from Attendance WHere ID = " + AID) - 1;
         if (Connexion.IFNULL("Select * from Class_Student Where EndSession is null and StudentID = " + StudentID + " and GroupID = " + GID))
         {
             Connexion.Insert("Insert into Class_Student Values (" + StudentID + " , " + CID + " , " + GID + " , " + SessionNumber + " , NULL ,0,0)");
             SessionNumber += 2; // i decremented and incremented the session number because in class student its -1 for the session for example the session 1 means the class_Student starts at 0 
             int LastSessionGroup = Connexion.GetInt("Select Count(*) from Attendance where GroupID = " + GID) + 1;
             if (LastSessionGroup != SessionNumber)
             {
                 string Dates = "";
                 for (int i = SessionNumber; i < LastSessionGroup; i++) //Getting all Dates after this session
                 {
                     Dates += Connexion.GetString("Select Date from Attendance WHere Session =" + i + " and GroupID = " + GID) + " , ";
                 }
                 Dates = Dates.Remove(Dates.Length - 1);
                 Dates = Dates.Remove(Dates.Length - 1);
                 string message = string.Format((string)Resources["MessageSessionsAfterthis"].ToString(), Dates);
                 string Status = "NULL";
                 if (MessageBox.Show(message, "Confirmation",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                 {
                     Status = "1";
                 }
                 else
                 {
                     // i would prefer here just to add him in one session 
                 }
                 for (int f = SessionNumber; f <= LastSessionGroup; f++)
                 {
                     int sessionadd = f - 1;
                     int AIDForOldSes = Connexion.GetInt("Select ID from Attendance Where Session =" + sessionadd + " and GroupID = " + GID);
                     Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status) Values (" + AIDForOldSes + "," + StudentID + " , " + Status + " )");
                     Commun.SetStatusAttendance(StudentID, AIDForOldSes.ToString(), int.Parse(Status));
                 }
             }
             else
             {
                 Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status) Values (" + AID + "," + StudentID + " , 1 )");
                 Commun.SetStatusAttendance(StudentID, AID, 1);
             }
             int monthlypayment2 = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
             if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
             {
                 int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                 monthlypayment2 = Connexion.GetInt("Select Monthly from years where id = " + YID);
             }

             if (monthlypayment2 == 1)
             {
                 Methods.InsertStudentClassMonthly(StudentID, CID);

             }
             else
             {
                 Commun.CheckDiscountAddClass(StudentID, this.Resources, 0, -1);
             }

         }
         else
         {
             MessageBox.Show("Already Inserted ");
         }
     }
     if (ty2 == "3")
     {
         DataRowView row = (DataRowView)CBStudent.SelectedItem;
         if (row == null)
         {
             return;
         }
         string SID = row["ID"].ToString();
         if (SID == "-1")
         {
             StudentAdd SAdd = new StudentAdd("Add", "-1");
             if (SAdd.ShowDialog() == true)
             {
                 SID = SAdd.ResponseText;
             }
             else
             {
                 MessageBox.Show("No Student Was Added");
                 return;
             }
         }
         string studentName = Connexion.GetString("Select FirstName + ' ' + LastName as Name from Students where ID = " + SID);

         string message = $"التلميذ ({studentName}) ليس مسجلاً. هل تريد إضافته؟";
         OptionPanels.TextPopups popup = new OptionPanels.TextPopups();
         bool? dialogResult = popup.ShowDialog();
         DataRow dr = dtMain.NewRow();
         if (dialogResult == true)
         {
             int result = popup.Result;
             Connexion.Insert("Insert into Attendance_extra_Students values(" + AID + "," + SID + "," + result + "," + TPrice + ",1)");

             dr["ID"] = SID;
             dr["Name"] = studentName;
             dr["RName"] = Connexion.GetString("Select lastName + ' ' + FirstName as Name from Students Where ID =" + SID);

             dr["Sessions"] = result;
             dr["Status"] = 1;
             dr["StatusText"] = this.Resources["Present"].ToString();
             dr["Barcode"] = Connexion.GetString("Select Barcode from Students Where ID = " + SID);
         }
         else
         {
             MessageBox.Show("No value was entered or the operation was canceled.");
             return;
         }
         bool exists = dtMain.AsEnumerable().Any(row2 => row2.Field<Int32>("ID").ToString() == SID);

         if (!exists)
         {
             dtMain.Rows.InsertAt(dr, 0);
         }
         else
         {
             MessageBox.Show("هذا التلميذ مسجل مسبقًا");
         }
     }

 }
 if (ty2 == "1")
 {
     string querySession = "";
     int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
     if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
     {
         int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
         monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
     }

     if (monthlypayment == 1)
     {
         string dateAttendanceString = Connexion.GetString(AID, "Attendance", "Date");
         DateTime dateAttendance = DateTime.Parse(dateAttendanceString);
         int monthNumber = dateAttendance.Month;
         int year = dateAttendance.Year;
         querySession = "dbo.CalculateMonthlyPaymentRemaining(Students.ID) ";
     }
     else
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

     queryforexception = "Select Students.FirstName + ' ' + Students.LastName as Name ,Students.Barcode as Barcode,Students.LastName + ' ' + students.FirstName as RName ,  Students.Gender as Gender ," + querySession + " as Sessions  , Students.ID as ID , Attendance_Student.Status as Status ," +
         " Case " +
         "When Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + "' " +
          "When Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + "' " +
          "When Attendance_Student.Status = 2 Then N'" + this.Resources["GroupChange"].ToString() + "' " +
          "When Attendance_Student.Status = 3 Then N'" + this.Resources["Justified"].ToString() + "' END as StatusText , " +
         "case When Attendance_Student.Status = 3 then Justif.Reason end as Reason  " +
         "from Attendance join Attendance_Student on Attendance.ID = Attendance_Student.ID   " +
         "join Students on Students.ID = Attendance_Student.StudentID " +
         "left  join justif on (Justif.AID = Attendance.ID and Justif.SID = Students.ID)  " +
         "join Groups on Groups.GroupID = Attendance.GroupID where Attendance.ID = " + AID;
     Connexion.FillDT(ref dtMain, queryforexception);
     ListClass.DataContext = dtMain.DefaultView; 
 }

            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }*/

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ty2 == "3")
                {
                    DataRowView rowRoom = (DataRowView)CRoom.SelectedItem;
                    DataRowView rowHFrom = (DataRowView)HFrom.SelectedItem;
                    DataRowView rowMFrom = (DataRowView)MFrom.SelectedItem;
                    DataRowView rowHTo = (DataRowView)HTo.SelectedItem;
                    DataRowView RowMTo = (DataRowView)MTo.SelectedItem;
                    if (RowMTo == null)
                    {
                        MessageBox.Show("Please Fill out all the information");
                        return;
                    }
                    string RID = rowRoom["ID"].ToString();
                    string Start = rowHFrom["Hour"].ToString() + ":" + rowMFrom["Hour"].ToString();
                    string End = rowHTo["Hour"].ToString() + ":" + RowMTo["Hour"].ToString();
                    Connexion.Insert("Update Attendance_Extra " +
                         "Set "+
                         "Date = '" + Date.Text.Replace("/", "-") + "'," +
                         "RoomID =  " + RID + ", " +
                         "TimeStart = '" + Start + "' ," +
                         "TimeEND = '" + End + "'" +
                         "Where ID = " + AID);

                }
                else if (ty2 == "1")
                {


                    int f = 1; //0 is the same 1 is not the same 
                    DateTime today = DateTime.Today;

                    // Format the date as "dd-MM-yyyy"
                    string formattedDate = today.ToString("dd-MM-yyyy");
                    if (formattedDate == Connexion.GetString("Select Date from Attendance Where ID = " + AID))
                    {
                        f = 0;
                    }
                    DataRowView rowRoom = (DataRowView)CRoom.SelectedItem;
                    DataRowView rowHFrom = (DataRowView)HFrom.SelectedItem;
                    DataRowView rowMFrom = (DataRowView)MFrom.SelectedItem;
                    DataRowView rowHTo = (DataRowView)HTo.SelectedItem;
                    DataRowView RowMTo = (DataRowView)MTo.SelectedItem;
                    if(RowMTo == null)
                    {
                        MessageBox.Show("Please Fill out all the information");
                        return;
                    }
                    string RID = rowRoom["ID"].ToString();
                    string Start = rowHFrom["Hour"].ToString() + ":" + rowMFrom["Hour"].ToString();
                    string End = rowHTo["Hour"].ToString() + ":" + RowMTo["Hour"].ToString();
                    Connexion.Insert("Update Attendance " +
                        "Set TeacherEntrance='" + TET.Text + "', " +
                        "Date = '" + Date.Text.Replace("/", "-") + "'," +
                        "Note = N'" + Note.Text + "'," +
                        "RoomID =  " + RID + ", " +
                        "TimeStart = '" + Start +  "' ," +
                        "TimeEND = '" + End + "'" +
                        "Where ID = " + AID);
                    Connexion.InsertHistory(2, AID, 5);
                    if (Date.Text.Replace("/", "-") == formattedDate && f == 1)
                    {
                        //Create a new attendance in attendanceAll
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
                MessageBox.Show(this.Resources["InsertedSucc"].ToString());
                this.Close();
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Report r = new Report();
                if (Connexion.Language() == 0)
                {
                    r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\AttendanceFastReport.frx");
                }
                else
                {
                    r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\AttendanceFastReportAR.frx");
                }
                DataSet ds = new DataSet();
                DataTable DataAttend = new DataTable();
                Connexion.FillDT(ref DataAttend,
                         "Select Students.FirstName + ' ' + Students.LastName as Name , Students.Barcode as Barcode," +
                       "       Students.Gender as Gender ," +
                       "        dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'Su') - dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'S') as Sessions, "
                      + "       Students.ID as ID ," +
                       "     Case when Attendance_Student.Status = 0 Then N'"+this.Resources["Absent"].ToString() +"' " +
                          "When Attendance_Student.Status = 1 Then N'" + this.Resources["Present"].ToString() + "' " +
                         "When Attendance_Student.Status = 2 Then " +
                      "G.GroupName + '" +
                      "(' + dbo.GetDateStudentChange(Attendance_Change.ToGroupID,Attendance_Change.Session,Attendance_Change.StudentID) + ')'  ELSE '' End as status, " +
                       "       Case When Attendance_Student.Status = 0 THEN 1 " +
                       "       When Attendance_Student.Status = 1 THEN 0 END as StatusOP ," +
                       "       Case When Attendance_Student.Status = 2 Then 1 " +
                       "       When Attendance_Student.Status !=2 Then 0 END As StatusCH ," +
                       "       Case When Attendance_Student.Status = 3 Then 1 " +
                       "       When Attendance_Student.Status !=3  Then 0 END as StatusJustif " +
                       "from Attendance_Student" +
                       "       Join Attendance ON Attendance_Student.ID = Attendance.ID " +
                       "       Join Students ON Students.ID = Attendance_Student.StudentID " +
                       "       Join Class_Student ON Class_Student.StudentID = Students.ID " +
                       "left join Attendance_Change " +
                 "on ( Attendance_Change.FromGroupID = Attendance.GroupID  and Attendance.Session = Attendance_Change.Session) " +
                 "left join Groups as G on Attendance_Change.ToGroupID = G.GroupID " +
                       "Join Groups on Groups.GroupID = Class_student.GroupID " +
                       "Where Attendance.ID = " + AID + "And Class_Student.ClassID = " + CID);
                DataAttend.TableName = "DataAttend";
                ds.Tables.Add(DataAttend);
                DataTable DataEcole = new DataTable();
                DataEcole.TableName = "DataEcole";
                Connexion.FillDT(ref DataEcole, "Select NameFR,NameAR,N'"+ Connexion.GetImagesFile() + "\\EcoleLogo.jpg'  as Logo , Number ,Number2,Adress from EcoleSetting");
                ds.Tables.Add(DataEcole);
                DataTable dtGroup = new DataTable();
                dtGroup.TableName = "DataGroup";
                Connexion.FillDT(ref dtGroup, "Select  " +
                        "Case When Class.MultipleGroups = 'Single' Then" +
                        " Class.CName " +
                        "When Class.MultipleGroups = 'Multiple' Then Class.CName + ' ' + Groups.GroupName " +
                        "End as GroupName," +
                        "Class.CSubject," +
                        "Groups.GroupGender as Gender, Students.Barcode as Barcode," +
                        "Class.CYear," +
                        "Teacher.TFirstName + ' ' + Teacher.TLastName as TName ," +
                        "Teacher.TLastName + ' ' + Teacher.TFirstName as RTName ," +
                        "Years.Year as Year , " +
                        "Subjects.Subject as Subject , " +
                        "dbo.GetStudentsAmmount(GroupID) as  SAmmount,  " +
                        "Groups.Sessions , " +
                        "Groups.TSessions ," +
                        "Levels.Level , " +
                        "Class.CLevel as LevelID " +
                        "From Groups  " +
                        "Join Class On Class.ID = Groups.ClassID " +
                        "Join Years on Class.CYear = Years.ID " +
                        "Join Subjects on Class.CSubject = Subjects.ID " +
                        "join Teacher on Teacher.ID = Class.TID " +
                        "Join Levels On Levels.ID = Class.CLevel  WHere Groups.GroupID =" + GID);
                ds.Tables.Add(dtGroup);
                r.RegisterData(ds);
                r.GetDataSource("DataAttend").Enabled = true;
                r.GetDataSource("DataEcole").Enabled = true;
                r.GetDataSource("DataGroup").Enabled = true;
                if (Commun.FastReportEdit != 1)
                {
                    r.Design();
                }
                else
                {
                    r.Show();
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string Condition = "1 > 0 ";
                if (CodebarTxt.Text == "")
                {
                    /* queryforexception = "Select Students.FirstName + ' ' + Students.LastName as Name ,Students.LastName + ' ' + students.FirstName as RName ,  Students.Gender as Gender ,dbo.GettotalPayStudent(Students.ID , Groups.ClassID) - dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'Su') as Sessions  , Students.ID as ID , Attendance_Student.Status as Status ," +
                          " Case " +
                          "When Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString()+"' " +
                          "When Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + "' " +
                          "When Attendance_Student.Status = 2 Then N'" + this.Resources["GroupChange"].ToString() + "' " +
                          "When Attendance_Student.Status = 3 Then N'" + this.Resources["Justified"].ToString() + "' END as StatusText , " +
                          "case When Attendance_Student.Status = 3 then Justif.Reason end as Reason  " +
                          "from Attendance join Attendance_Student on Attendance.ID = Attendance_Student.ID   " +
                          "join Students on Students.ID = Attendance_Student.StudentID " +
                          "left  join justif on (Justif.AID = Attendance.ID and Justif.SID = Students.ID)  " +
                          "join Groups on Groups.GroupID = Attendance.GroupID where Attendance.ID = " + AID;
                     Connexion.FillDT(ref dtMain, queryforexception);
                     ListStudents.DataContext = dtMain.DefaultView;*/
                   
                    dtMain.DefaultView.RowFilter = Condition;
                    return;
                }
               
                bool hasNumber = false;

                foreach (char c in CodebarTxt.Text)
                {
                    if (char.IsDigit(c))
                    {
                        hasNumber = true;
                        break; // Exit the loop if a number is found
                    }
                }
                string text = CodebarTxt.Text;
                bool isNumeric = int.TryParse(text, out _);
                char lastChar = text[text.Length - 1];
                if (lastChar == '$')
                {


                    Condition += $" and (BarCode = '{text}') ";

                }
                else if (hasNumber)
                {

                }
                else
                {


                    //   Condition += "And (Name Like '%" + Regex.Replace(CodebarTxt.Text, @"\s+", " ") + "%' OR RName Like '%" + Regex.Replace(CodebarTxt.Text, @"\s+", " ") + "%') ";
                    string searchText = Regex.Replace(CodebarTxt.Text, @"\s+", " ").Replace("'", "''");

                    // Construct the filter expression
                    Condition += $" and (Name LIKE '%{searchText}%' OR RName LIKE '%{searchText}%') ";

                    // Apply the filter to the DataView

                }
                dtMain.DefaultView.RowFilter = Condition;

            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }


        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

        }
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if(ty2 == "1")
                {
                    //HERE
                }
                else
                {
                    if (!Connexion.IFNULL("Select * from Attendance_Student Where Status is null and Attendance_Student.ID = " + AID))
                    {
                        if (MessageBox.Show(this.Resources["ConfirmationCloseattend"].ToString(), "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        {
                            e.Cancel = true;
                        }
                    }
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }

            // Call the base implementation to raise the Closing event
          //  base.OnClosing(e);
        }

        private void ListStudents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(ty2 != "1")
            {
                DataRowView row = (DataRowView)ListFormation.SelectedItem;
                if (row != null)
                {
                    StudentAdd s = new StudentAdd("Show", row["ID"].ToString());
                    s.Show();
                }
            }
            else
            {
                DataRowView row = (DataRowView)ListClass.SelectedItem;
                if (row != null)
                {
                    StudentAdd s = new StudentAdd("Show", row["ID"].ToString());
                    s.Show();
                }
            }
           
        }
        public class ForegroundConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is int intValue)
                {
                    return intValue < 0 ? Brushes.Red : Brushes.Green;
                }
                return Brushes.Black; // Default fallback color
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
        private void BtnPayment_Click(object sender, RoutedEventArgs e)
        {
            if(ty2 != "1")
            {

            }
            else
            {
                DataRowView row = (DataRowView)ListClass.SelectedItem;
                if (ListClass.SelectedItems.Count > 1)
                {
                    MessageBox.Show("Please Select one student only");
                    return;
                }
                if (row != null)
                {
                    int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                    if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                    {
                        int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                        monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                    }

                    if (monthlypayment== 1)
                    {
                        EmptyPage Empty = new EmptyPage("StudentPaymentMonthly", row["ID"].ToString(), "");
                        Empty.Show();
                    }
                    else
                    {
                        EmptyPage Empty = new EmptyPage("StudentPayment2", row["ID"].ToString(), CID);
                        Empty.Show();
                    }
                }
            }
        }

        private void Button_Click_Absent(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ty2 == "2")
                { 
                    foreach (DataRowView rowView in ListFormation.SelectedItems)
                    {
                        string SID = rowView["ID"].ToString();
                        Connexion.Insert("Update Formation_Attendance_Student Set Status = 0 Where SID = '" + SID + "' And AID = '" + AID + "'");
                        Commun.EditRowInDataTable(ref dtMain, SID, "ID", this.Resources["Absent"].ToString(), "StatusText");
                        Commun.EditRowInDataTable(ref dtMain, SID, "ID", "0", "Status");
                        Commun.EditRowInDataTable(ref dtMain, SID, "ID", Connexion.GetString("Select dbo.CalcPriceSum(" + SID + "," + CID + ")"), "Sessions");
                    }
                }
                else if (ty2== "1")
                {

                    foreach (DataRowView rowView in ListClass.SelectedItems)
                    {

                        string SID = rowView["ID"].ToString();
                        
                        Commun.SetStatusAttendanceupg(SID, AID,CID,GID,date, 0 ,ref dtMain);
                    }
                   
                }
                else if(ty2 == "3")
                {
                    string message = "هل تريد حذف هؤلاء التلاميذ من هذه الحصة الإضافية؟";
                    if (MessageBox.Show(message,
                          "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        List<DataRow> rowsToDeleteList = new List<DataRow>();
                        foreach (DataRowView rowView in ListClass.SelectedItems)
                        {
                            Connexion.Insert("Delete from Attendance_Extra_Students Where SID=" + rowView["ID"].ToString() + " and ID = " + AID);
                            DataRow[] rowsToDelete = dtMain.Select("ID = " + rowView["ID"].ToString());
                            rowsToDeleteList.AddRange(rowsToDelete);
                          
                        }
                        foreach (DataRow row in rowsToDeleteList)
                        {
                            dtMain.Rows.Remove(row);
                        }
                        ListClass.ItemsSource = dtMain.DefaultView;
                    }
                }
                
                
               
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click_Present(object sender, RoutedEventArgs e)
        {
            try
            {

                if (ty2 == "2")
                {
                    foreach (DataRowView rowView in ListFormation.SelectedItems)
                    {
                        string SID = rowView["ID"].ToString();
                        Connexion.Insert("Update Formation_Attendance_Student Set Status = 1 Where SID = '" + SID + "' And AID = '" + AID + "'");
                        Commun.EditRowInDataTable(ref dtMain, SID, "ID", this.Resources["Present"].ToString(), "StatusText");
                        Commun.EditRowInDataTable(ref dtMain, SID, "ID", "1", "Status");
                        Commun.EditRowInDataTable(ref dtMain, SID, "ID", Connexion.GetString("Select dbo.CalcPriceSum(" + SID + "," + CID + ")"), "Sessions");
                    }
                }
                else if (ty2 == "3")
                {
                   
                }
                else if (ty2 == "1")
                {
                    foreach (DataRowView rowView in ListClass.SelectedItems)
                    {
                        string SID = rowView["ID"].ToString();

                        Commun.SetStatusAttendanceupg(SID, AID,CID,GID,date, 1, ref dtMain);
                    }
                }
                ListClass.ItemsSource = dtMain.DefaultView;
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
              

/*
                // Check if Enter key is pressed
                if (e.Key == Key.Enter)
                {
                    // Get the selected rows
                    var selectedRows = ListClass.SelectedItems;

                    foreach (DataRowView rowView in ListClass.SelectedItems)
                    {
                        string SID = rowView["ID"].ToString();

                        int oldstatus = Connexion.GetInt("Select case when Status is null then 0 else Status end as sta from Attendance_Student Where StudentID = " + SID + " and ID  = " + AID);
                        if (oldstatus == 2)
                        {
                            if (MessageBox.Show(this.Resources["MessageBoxOverrideGroupChange"].ToString(), "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                int ToGroupID = Connexion.GetInt("Select ToGroupID from Attendance_Change Where FromGroupID = " + GID + " and StudentID = " + SID + " and Session = " + SessionA);
                                int ToAttendanceID = Connexion.GetInt("Select ID from Attendance Where GroupID = " + ToGroupID + " and Session = " + SessionA);
                                Connexion.Insert("Delete from Attendance_Student Where ID=" + ToAttendanceID + " and StudentID = " + SID);
                                Connexion.Insert("Delete From Attendance_Change Where FromGroupID = " + GID + " and StudentID = " + SID + " and Session = " + SessionA);
                            }
                            else
                            {
                                continue; //
                            }
                        }
                        if (oldstatus == 3)
                        {
                            if (MessageBox.Show(this.Resources["MessageBoxOverrideJustif"].ToString(), "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                Connexion.Insert("Delete from justif where SID = " + SID + " and AID = " + AID);
                                Commun.EditRowInDataTable(ref dtMain, SID, "ID", "", "Reason");
                            }
                            else
                            {
                                continue; //
                            }
                        }
                        Connexion.Insert("Update Attendance_Student Set Status = 1 Where StudentID = '" + SID + "' And ID = '" + AID + "'");
                        Commun.EditRowInDataTable(ref dtMain, SID, "ID", this.Resources["Present"].ToString(), "StatusText");
                        Commun.EditRowInDataTable(ref dtMain, SID, "ID", "1", "Status");
                    }

                    e.Handled = true; // Prevent further processing of the Enter key
                }*/
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }
        private void BtnJustif_Click(object sender, RoutedEventArgs e)
        {
            if(stop != 0)
            {
                return;
            }
            if (ty2 == "1")
            {
                DataRowView row = (DataRowView)ListClass.SelectedItem;
                if (ListClass.SelectedItems.Count > 1)
                {
                    MessageBox.Show("Please Select one student only");
                    return;
                }
                if (row != null)
                {
                    string SID = row["ID"].ToString();
                    Commun.SetStatusAttendanceupg(SID, AID,CID,GID,date, 3, ref dtMain);
                }
            }
        }
        private void MyButton_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Check if the pressed key is Enter
            if (e.Key == Key.Enter)
            {
                e.Handled = true; // Prevents the click event from being triggered
            }
        }
        private void CBStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ListStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                try
                {
                    Report r = new Report();
                    if (Connexion.Language() == 0)
                    {
                        r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\AttendanceFastReport.frx");
                    }
                    else
                    {
                        r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\AttendanceFastReportAR.frx");
                    }
                    DataSet ds = new DataSet();
                    DataTable DataAttend = new DataTable();
                   
                    string GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + AID);
                    string CID = Connexion.GetClassID(GID).ToString();
                    Connexion.FillDT(ref DataAttend,
                             "Select Students.FirstName + ' ' + Students.LastName as Name ," +
                           "       Students.Gender as Gender ," +
                           "        dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'Su') - dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'S') as Sessions, "
                          + "       Students.ID as ID ," +
                           "     Case when Attendance_Student.Status = 0 Then N'" + this.Resources["Absent"].ToString() + "' " +
                              "When Attendance_Student.Status = 1 Then N'" + this.Resources["Present"].ToString() + "' " +
                             "When Attendance_Student.Status = 2 Then " +
                          "G.GroupName + '" +
                          "(' + dbo.GetDateStudentChange(Attendance_Change.ToGroupID,Attendance_Change.Session,Attendance_Change.StudentID) + ')' ELSE '' End as status, " +
                           "       Case When Attendance_Student.Status = 0 THEN 1 " +
                           "       When Attendance_Student.Status = 1 THEN 0 END as StatusOP ," +
                           "       Case When Attendance_Student.Status = 2 Then 1 " +
                           "       When Attendance_Student.Status !=2 Then 0 END As StatusCH ," +
                           "       Case When Attendance_Student.Status = 3 Then 1 " +
                           "       When Attendance_Student.Status !=3  Then 0 END as StatusJustif " +
                           "from Attendance_Student" +
                           "       Join Attendance ON Attendance_Student.ID = Attendance.ID " +
                           "       Join Students ON Students.ID = Attendance_Student.StudentID " +
                           "       Join Class_Student ON Class_Student.StudentID = Students.ID " +
                           "left join Attendance_Change " +
                     "on ( Attendance_Change.FromGroupID = Attendance.GroupID  and Attendance.Session = Attendance_Change.Session) " +
                     "left join Groups as G on Attendance_Change.ToGroupID = G.GroupID " +
                           "Join Groups on Groups.GroupID = Class_student.GroupID " +
                           "Where Attendance.ID = " + AID + "And Class_Student.ClassID = " + CID);
                    DataAttend.TableName = "DataAttend";
                    ds.Tables.Add(DataAttend);
                    DataTable DataEcole = new DataTable();
                    DataEcole.TableName = "DataEcole";
                    Connexion.FillDT(ref DataEcole, "Select NameFR,NameAR,N'" + Connexion.GetImagesFile() + "\\EcoleLogo.jpg'  as Logo , Number ,Number2,Adress from EcoleSetting");
                    ds.Tables.Add(DataEcole);
                    DataTable dtGroup = new DataTable();
                    dtGroup.TableName = "DataGroup";
                    Connexion.FillDT(ref dtGroup, "Select  " +
                            "Case When Class.MultipleGroups = 'Single' Then" +
                            " Class.CName " +
                            "When Class.MultipleGroups = 'Multiple' Then Class.CName + ' ' + Groups.GroupName " +
                            "End as GroupName," +
                            "Class.CSubject," +
                            "Groups.GroupGender as Gender," +
                            "Class.CYear," +
                            "Teacher.TFirstName + ' ' + Teacher.TLastName as TName ," +
                            "Teacher.TLastName + ' ' + Teacher.TFirstName as RTName ," +
                            "Years.Year as Year , " +
                            "Subjects.Subject as Subject , " +
                            "dbo.GetStudentsAmmount(GroupID) as  SAmmount,  " +
                            "Groups.Sessions , " +
                            "Groups.TSessions ," +
                            "Levels.Level , " +
                            "Class.CLevel as LevelID " +
                            "From Groups  " +
                            "Join Class On Class.ID = Groups.ClassID " +
                            "Join Years on Class.CYear = Years.ID " +
                            "Join Subjects on Class.CSubject = Subjects.ID " +
                            "join Teacher on Teacher.ID = Class.TID " +
                            "Join Levels On Levels.ID = Class.CLevel  WHere Groups.GroupID =" + GID);
                    ds.Tables.Add(dtGroup);
                    r.RegisterData(ds);
                    r.GetDataSource("DataAttend").Enabled = true;
                    r.GetDataSource("DataEcole").Enabled = true;
                    r.GetDataSource("DataGroup").Enabled = true;
                    if (Commun.FastReportEdit != 1)
                    {
                        r.Design();
                    }
                    else
                    {
                        r.Show();
                    }
                }
                catch (Exception ex)
                {
                    Methods.ExceptionHandle(ex);
                }
            }
        }

        private void CodebarTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                    if (ty2 == "3")
                    {
                        if (Connexion.IFNULL("Select * from Students Where BarCode = '" + CodebarTxt.Text + "'"))
                        {
                            MessageBox.Show("Barcode Wrong");
                            CodebarTxt.Text = "";
                            return;
                        }
                        string SID = Connexion.GetString("Select ID from Students Where Barcode = '" + CodebarTxt.Text + "'");
                        if(!Connexion.IFNULL("Select * from Attendance_Extra_Students Where SID = " + SID + " and ID = " + AID))
                        {
                            MessageBox.Show("هذا التلميذ مسجل مسبقًا");
                            return;
                        }
                        int yearid = Connexion.GetInt("Select CYear From Class Where ID = " + CID);
                        string query = "SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students  Where  Students.Status = 1 and Year = " + yearid;
                        DataTable dtSpec = new DataTable();
                        Connexion.FillDT(ref dtSpec, "Select SpecID from Class_Speciality Where ID = " + CID);
                        string Specid = "";
                        for (int i = 0; i < dtSpec.Rows.Count; i++)
                        {
                            Specid = dtSpec.Rows[i]["SpecID"].ToString();
                            if (i == 0)
                            {
                                query += " AND SPECIALITY = " + Specid;
                            }
                            else
                            {
                                query += " OR SPECIALITY = " + Specid;
                            }
                        }
                        query += " And Students.ID = " + SID;
                        if (Connexion.IFNULL(query))
                        {
                            MessageBox.Show("هذا الطالب ليس لديه نفس السنة أو الشعبة مثل هذا الفوج");
                            CodebarTxt.Text = "";
                            return;
                        }
                        if (Connexion.IFNULL("Select * from Class_Student Where StudentID = " + SID + " and ClassID = " + CID + " and EndSession is null "))
                        {
                            
                            string studentName = Connexion.GetString("Select FirstName + ' ' + LastName as Name from Students where ID = " + SID);
                            string message = $"التلميذ ({studentName}) ليس مسجلاً. هل تريد إضافته؟";
                            OptionPanels.TextPopups popup = new OptionPanels.TextPopups();
                            bool? dialogResult = popup.ShowDialog();

                            if (dialogResult == true)
                            {
                                int result = popup.Result;
                                Connexion.Insert("Insert into Attendance_extra_Students values(" + AID + "," + SID + "," + result + "," + TPrice + ",1)");
                                DataRow dr = dtMain.NewRow();
                                dr["ID"] = SID;
                                dr["Name"] = studentName;
                                dr["RName"] = Connexion.GetString("Select lastName + ' ' + FirstName as Name from Students Where ID =" + SID);

                                dr["Sessions"] = result;
                                dr["Status"] = 1;
                                dr["StatusText"] = this.Resources["Present"].ToString();
                                dr["Barcode"] = Connexion.GetString("Select Barcode from Students Where ID = " + SID);
                                dtMain.Rows.InsertAt(dr, 0);
                            }
                            else
                            {
                                MessageBox.Show("No value was entered or the operation was canceled.");
                                
                            }
                            
                        }
                        else
                        {
                            string studentName = Connexion.GetString("Select FirstName + ' ' + LastName as Name from Students where ID = " + SID);
                            Connexion.Insert("Insert into Attendance_extra_Students values(" + AID + "," + SID + "," + Price + "," + TPrice + ",1)");
                            DataRow dr = dtMain.NewRow();
                            dr["ID"] = SID;
                            dr["Name"] = studentName;
                            dr["RName"] = Connexion.GetString("Select lastName + ' ' + FirstName as Name from Students Where ID =" + SID);
                        
                            dr["Sessions"] = 0;
                            dr["Status"] = 1;
                            dr["StatusText"] = this.Resources["Present"].ToString();
                            dr["Barcode"] = Connexion.GetString("Select Barcode from Students Where ID = " + SID);
                            dtMain.Rows.InsertAt(dr, 0);

                        }
                        CodebarTxt.Text = "";
                        return;
                    }
                    
                   

                    
                    if (CodebarTxt.Text == "")
                    {
                        return;
                    }
                    if (CodebarTxt.Text.Last() == '$')
                    {
                        if (Connexion.IFNULL("Select * from Students Where BarCode = '" + CodebarTxt.Text + "'"))
                        {
                            MessageBox.Show("Barcode Wrong");
                            CodebarTxt.Text = "";
                        }
                        else
                        {

                            string SID = Connexion.GetInt(CodebarTxt.Text, "Students", "ID", "BarCode").ToString();
                            if (Connexion.IFNULL("Select * from Attendance_Student Where StudentID = " + SID + " and ID = " + AID))
                            {
                                MessageBox.Show("This Student isn't registered in this group");
                                return;
                            }

                            string result = Connexion.GetString("Select  case when Status is null then -1 else status end as statu From Attendance_Student Where ID = '" + AID + "'And StudentID = " + SID);
                            if (int.Parse(result.ToString()) != 2 && int.Parse(result.ToString()) != 3)
                            {
                                Commun.SetStatusAttendance(SID, AID, 1);
                            }

                            queryforexception = "Select Students.FirstName + ' ' + Students.LastName as Name, Students.LastName + ' ' + Students.FirstName as RName, Students.Gender as Gender, dbo.GettotalPayStudent(Students.ID, Groups.ClassID) - dbo.CalculatePrice(Students.ID, Groups.GroupID, Groups.TSessions, 'Su') as Sessions, Students.ID as ID, Attendance_Student.Status as Status," +
                               " Case " +
                               "When Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + "' " +
                               "When Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + "' " +
                               "When Attendance_Student.Status = 2 Then N'" + this.Resources["GroupChange"].ToString() + "' " +
                               "When Attendance_Student.Status = 3 Then N'" + this.Resources["Justified"].ToString() + "' END as StatusText, " +
                               "case When Attendance_Student.Status = 3 then Justif.Reason end as Reason  " +
                               "from Attendance join Attendance_Student on Attendance.ID = Attendance_Student.ID   " +
                               "join Students on Students.ID = Attendance_Student.StudentID " +
                               "left  join justif on (Justif.AID = Attendance.ID and Justif.SID = Students.ID)  " +
                               "join Groups on Groups.GroupID = Attendance.GroupID where Attendance.ID = " + AID;

                            Connexion.FillDT(ref dtMain, queryforexception);

                            ListClass.ItemsSource = dtMain.DefaultView;
                            PopupText.Text = Connexion.GetString(SID, "Students", "FirstName") + "  " + Connexion.GetString(SID, "Students", "LastName");
                            MyPopup.IsOpen = true;
                            CodebarTxt.Text = "";
                            CodebarTxt.Focus();
                            return;


                        }
                    }
                    else
                    {

                        string Condition = "1 > 0 ";
                        //   Condition += "And (Name Like '%" + Regex.Replace(CodebarTxt.Text, @"\s+", " ") + "%' OR RName Like '%" + Regex.Replace(CodebarTxt.Text, @"\s+", " ") + "%') ";
                        string searchText = Regex.Replace(CodebarTxt.Text, @"\s+", " ").Replace("'", "''");

                        // Construct the filter expression
                        Condition += $" and (Name LIKE '%{searchText}%' OR RName LIKE '%{searchText}%') ";
                        dtMain.DefaultView.RowFilter = Condition;
                        int rowCount = dtMain.DefaultView.Count;
                        if (rowCount == 0)
                        {
                            return;
                        }
                        if (rowCount > 1)
                        {
                            if (MessageBox.Show("Multiple Students are shown in the search do you want to mark them all as present ?", "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                            {
                                return;
                            }
                        }
                        string SID;
                        string names = "";
                        foreach (DataRowView rowView in dtMain.DefaultView)
                        {
                            DataRow row = rowView.Row;
                            SID = row["ID"].ToString();
                            names += Connexion.GetString(SID, "Students", "FirstName") + "  " + Connexion.GetString(SID, "Students", "LastName") + ",";

                            if (Connexion.IFNULL("Select * from Attendance_Student Where StudentID = " + SID + " and ID = " + AID))
                            {
                                continue;
                            }

                            string result = Connexion.GetString("Select  case when Status is null then -1 else status end as statu From Attendance_Student Where ID = '" + AID + "'And StudentID = " + SID);
                            if (int.Parse(result.ToString()) != 2 && int.Parse(result.ToString()) != 3)
                            {
                                Commun.SetStatusAttendance(SID, AID, 1);
                            }


                        }
                        queryforexception = "Select Students.FirstName + ' ' + Students.LastName as Name, Students.LastName + ' ' + Students.FirstName as RName, Students.Gender as Gender, dbo.GettotalPayStudent(Students.ID, Groups.ClassID) - dbo.CalculatePrice(Students.ID, Groups.GroupID, Groups.TSessions, 'Su') as Sessions, Students.ID as ID, Attendance_Student.Status as Status," +
                         " Case " +
                         "When Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + "' " +
                         "When Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + "' " +
                         "When Attendance_Student.Status = 2 Then N'" + this.Resources["GroupChange"].ToString() + "' " +
                         "When Attendance_Student.Status = 3 Then N'" + this.Resources["Justified"].ToString() + "' END as StatusText, " +
                         "case When Attendance_Student.Status = 3 then Justif.Reason end as Reason  " +
                         "from Attendance join Attendance_Student on Attendance.ID = Attendance_Student.ID   " +
                         "join Students on Students.ID = Attendance_Student.StudentID " +
                         "left  join justif on (Justif.AID = Attendance.ID and Justif.SID = Students.ID)  " +
                         "join Groups on Groups.GroupID = Attendance.GroupID where Attendance.ID = " + AID;

                        Connexion.FillDT(ref dtMain, queryforexception);

                        ListClass.ItemsSource = dtMain.DefaultView;
                        //  MessageBox.Show(Connexion.GetString(SID, "Students", "FirstName") + "  " + Connexion.GetString(SID, "Students", "LastName"));
                        //  string name = Connexion.GetString("Select FirstName +' ' + LastName from Students Where id = " + SID);
                        PopupText.Text = names;
                        MyPopup.IsOpen = true;



                    }
                    CodebarTxt.Text = "";
                    CodebarTxt.Focus();
                    return;

                }
            
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void MyPopup_Opened(object sender, EventArgs e)
        {
            closeTimer = new DispatcherTimer();
            closeTimer.Interval = TimeSpan.FromSeconds(2); // Set the timer interval to 1 second
            closeTimer.Tick += ClosePopup; // Hook up an event handler to run when the timer elapses
            closeTimer.Start(); // Start the timer
        }

        private void ClosePopup(object sender, EventArgs e)
        {
            // This method is called when the timer elapses (after 1 second)
            // Close your popup here
            MyPopup.IsOpen = false;

            // Stop the timer
            closeTimer.Stop();
        }

        private void CRoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                HFrom.SelectedIndex = -1;
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
                        DayOfWeek dayOfWeekname = Date.SelectedDate.Value.DayOfWeek;
                        int dayOfWeek = (int)dayOfWeekname;

                        // Cast the enumeration to an integer
                        int dayOfWeekValue = (int)dayOfWeek;

                        if (ty2 != "3")
                        {
                            int[] r = Connexion.CheckTimeHour(dr[0].ToString(), dayOfWeekValue, int.Parse(rowRoom["ID"].ToString()), GID);
                            dr[1] = r[0];
                            dr[2] = r[1];
                            dr[3] = r[2];
                        }
                        else
                        {
                            int[] r = { 0, -1, -1 };
                            dr[1] = r[0];
                            dr[2] = r[1];
                            dr[3] = r[2];
                        }
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

        private void Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!showmessagetimechange)
            {
                if (reaccurent)
                {
                    reaccurent = false;
                    if (MessageBox.Show(this.Resources["ConfChangeDate"].ToString(), this.Resources["Warning"].ToString(), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (ty2 == "1")
                        {
                            string OldDate = Connexion.GetString("Select Date from Attendance Where ID = " + AID);
                            if (OldDate != Date.Text.Replace("/", "-"))
                            {
                                Commun.ChangeDateAttend(AID, Date.Text.Replace("/", "-"));
                            }
                        }
                    }
                }
                else
                {
                    reaccurent = true;
                }
            }
        }

        private void HFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                MFrom.SelectedIndex = -1;
                MTo.SelectedIndex = -1;
                HTo.SelectedIndex = -1;
           
                DayOfWeek dayOfWeekname = Date.SelectedDate.Value.DayOfWeek;
                int dayOfWeek = (int)dayOfWeekname;
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
                              "and day =  " + dayOfWeek + " " +
                              "and GID != "+ GID+ " and CONVERT(INT,SUBSTRING(TimeEnd,0,3)) = " + row["Hour"].ToString() + " " +
                              "and Groups.ClassID = '" + Connexion.GetClassID(row["ID"].ToString()) + "'";
                        }
                        else if (row["Type"].ToString() == "2")
                        {
                            query = "Select  CONVERT(INT,SUBSTRING(TimeEnd,4,2)) From Class_Time  join Formation on Formation.ID = Class_Time.FID " +
                            "Where IDroom = " + rowroom["ID"].ToString() + " " +
                            "and day =  " + dayOfWeek + " " +
                            "and GID != " + GID +" and CONVERT(INT,SUBSTRING(TimeEnd,0,3)) = " + row["Hour"].ToString() + " " +
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
                DayOfWeek dayOfWeekname = Date.SelectedDate.Value.DayOfWeek;
                int dayofWeek = (int)dayOfWeekname;
                if (row != null && row2 != null && rowRoom != null)
                { 
                    if (!showmessagetimechange)
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
                            if (!showmessagetimechange) MessageBox.Show(this.Resources["TimeTaken"].ToString() + " " + groupName);
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
                                "and GID != " + GID +" and day =  " + dayofWeek + " " +
                                "and CONVERT(INT,SUBSTRING(TimeStart,0,3)) = " + i;

                            DataTable dttime = new DataTable();
                            if(ty2 == "1")
                            {
                                Connexion.FillDT(ref dttime, Command);
                            }

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
                DayOfWeek dayOfWeekname = Date.SelectedDate.Value.DayOfWeek;
                int dayOfWeek = (int)dayOfWeekname;

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
                            "and day =  " + dayOfWeek + " " +
                            "and GID != " + GID + " and CONVERT(INT,SUBSTRING(TimeStart,0,3)) = " + row["Hour"].ToString() + " " +
                            "and GID = '" + row["ID"].ToString() + "'";
                        }
                        else if (row["Type"].ToString() == "2")
                        {
                            query = "Select  CONVERT(INT,SUBSTRING(TimeStart,4,2)) From Class_Time " +
                            "Where IDroom = " + rowroom["ID"].ToString() + " " +
                            "and day =  " + dayOfWeek + " " +
                            "and GID != " + GID  + " and CONVERT(INT,SUBSTRING(TimeStart,0,3)) = " + row["Hour"].ToString() + " " +
                            "and FID = '" + row["ID"].ToString() + "'";
                        }
                        
                        int result = Connexion.GetInt(query);
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
                    if (!showmessagetimechange)
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
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }

        }

        private void Button_Click_ChangeStudents(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = null;
                Panels.ChangeGroup changegroup = new Panels.ChangeGroup(AID, "1",ref dt);
                changegroup.ShowDialog();
                string querySession = "";
                int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                {
                    int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                    monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                }

                if (monthlypayment == 1)
                {
                    string dateAttendanceString = Connexion.GetString(AID, "Attendance", "Date");
                    DateTime dateAttendance = DateTime.Parse(dateAttendanceString);
                    int monthNumber = dateAttendance.Month;
                    int year = dateAttendance.Year;
                    querySession = "dbo.CalculateMonthlyPaymentRemaining(Students.ID) ";
                }
                else
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

                queryforexception = "Select Students.FirstName + ' ' + Students.LastName as Name ,Students.Barcode as Barcode,Students.LastName + ' ' + students.FirstName as RName ,  Students.Gender as Gender ," + querySession + " as Sessions  , Students.ID as ID , Attendance_Student.Status as Status ," +
                    " Case " +
                    "When Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + "' " +
                     "When Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + "' " +
                     "When Attendance_Student.Status = 2 Then N'" + this.Resources["GroupChange"].ToString() + "' " +
                     "When Attendance_Student.Status = 3 Then N'" + this.Resources["Justified"].ToString() + "' END as StatusText , " +
                    "case When Attendance_Student.Status = 3 then Justif.Reason end as Reason  " +
                    "from Attendance join Attendance_Student on Attendance.ID = Attendance_Student.ID   " +
                    "join Students on Students.ID = Attendance_Student.StudentID " +
                    "left  join justif on (Justif.AID = Attendance.ID and Justif.SID = Students.ID)  " +
                    "join Groups on Groups.GroupID = Attendance.GroupID where Attendance.ID = " + AID;
                Connexion.FillDT(ref dtMain, queryforexception);
                ListClass.DataContext = dtMain.DefaultView;
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void ChangePrice_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (ListClass.SelectedItems.Count > 0)
                {
                    if (ty2 != "3")
                    {
                        return;
                    }


                    string message = $"السعر الجديد لهذه الحصة";
                    OptionPanels.TextPopups popup = new OptionPanels.TextPopups();
                    bool? dialogResult = popup.ShowDialog();
                    DataRow dr = dtMain.NewRow();
                    if (dialogResult != true)
                    {
                        return;
                    }
                    int result = popup.Result;
                    // DataTable selectedItemsTable = new DataTable();

                    // Ensure the DataTable structure matches the DataGrid's source structure
                    if (ListClass.ItemsSource is DataView dataView)
                    {

                        // Loop through all selected items
                        foreach (var selectedItem in ListClass.SelectedItems)
                        {
                            if (selectedItem is DataRowView dataRowView)
                            {
                                int studentID = (int)dataRowView["ID"];
                                Connexion.Insert("Update Attendance_Extra_Students Set Price = " + result + " where SID = " + studentID + " and ID = " + AID);
                                Commun.EditRowInDataTable(ref dtMain, studentID.ToString(), "ID", Connexion.GetString("Select dbo.CalcPriceSum(" + studentID.ToString() + "," + CID + ")"), "Sessions");

                            }
                        }
                    }
                }
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnExtraStudents_Click(object sender, RoutedEventArgs e)
        {
            var AddS = new ExtraStudentsAttendView(AID);
            AddS.ShowDialog();
        }
    }
}
