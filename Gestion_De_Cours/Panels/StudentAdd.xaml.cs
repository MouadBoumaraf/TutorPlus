using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using Microsoft.Win32;
using Gestion_De_Cours.Classes;
using Gestion_De_Cours.UControl;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;
using FastReport;
using WIA;

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for StudentAdd.xaml
    /// </summary>
    public partial class StudentAdd : Window
    {
        OpenFileDialog open = new OpenFileDialog();
        string Type;
        string SID = "0";
        static int Fees ;
        DataTable dtAttendance = new DataTable();
        string Condition = "1 > 0 ";
        string path = "";
        string IDforDialog = "";
        bool finishinitializing = false;
        public StudentAdd(string TypeUC, string ID , string YearIDFix = "")
        {
            try
            {

                InitializeComponent();
                finishinitializing = true; 
                int lang = Connexion.Language();
                IDforDialog = ID;
                SetLang();
               
                if(lang == 1)
                {
                    this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
                }
                SqlConnection con = Connexion.Connect();
                SpecCB.Visibility = Visibility.Collapsed;
                Speclabel.Visibility = Visibility.Collapsed;
                DateClassReg.SelectedDate = DateTime.Today;
                Connexion.FillCB(ref LevelCB, "Select * from Levels");
                Type = TypeUC;
                if (TypeUC == "Show")
                {
                    int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                    if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                    {
                        int YID2 = Connexion.GetInt("Select Year from Students Where id = " + ID);
                        monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID2);
                    }
                    if (monthlypayment == 1)
                    {
                        DGCPrice.Visibility = Visibility.Collapsed;
                    }
                    SID = ID;
                    CBItemRefund.Visibility = Visibility.Visible; 
                    //Filling Comboboxes 
                    Connexion.FillCB(ref CBAttendanceClass, "Select " +
                        "Class.ID as ID ," +
                        "Class.CName as CName " +
                        "from Class " +
                        "Join Class_Student on Class_Student.ClassID = Class.ID " +
                        "Where Class_Student.StudentID = " + SID);
                    int max = Connexion.GetInt("select " +
                        "case when max(Groups.Sessions) is null then '0' " +
                        "else max(groups.Sessions) end as f " +
                        "from groups " +
                        "Join Class_Student " +
                        "on Class_Student.GroupID = Groups.GroupID " +
                        "Where Class_Student.StudentID = " + SID);
                    for (int i = 1; i <= max; i++)
                    {
                        CBAttendanceSession.Items.Add(i);
                    }
                    StudentPayment paymentU = new StudentPayment("1", SID, "");
                    ControlP.Children.Add(paymentU);
                    FirstName.Text = Connexion.GetString(ID, "Students", "FirstName");
                    LastName.Text = Connexion.GetString(ID, "Students", "LastName");
                    PhoneNumber.Text = Connexion.GetString(ID, "Students", "PhoneNumber");
                    ParentNumber.Text = Connexion.GetString(ID, "Students", "ParentNumber");
                    Adress.Text = Connexion.GetString(ID, "Students", "Adress");
                    Note.Text = Connexion.GetString(ID, "Students", "Note");
                    Gender.SelectedIndex = Connexion.GetInt(ID, "Students", "Gender");
                    string gender = Gender.Text;
                    LevelCB.SelectedValue = Connexion.GetInt(ID, "Students", "Level");
                    int ID2 = Connexion.GetInt(ID, "Students", "Year");
                    YearCB.SelectedValue = ID2.ToString();
                    if(SpecCB.Visibility == Visibility.Visible)
                    {
                        SpecCB.SelectedValue = Connexion.GetInt(ID, "Students", "Speciality");
                    }
                    InscFees.SelectedIndex = Connexion.GetInt(ID, "Students", "PayedInsc");
                    DateRegister.Text = Connexion.GetString(ID, "Students", "Register");
                    School.Text = Connexion.GetString(ID, "Students", "School");
                    TBBarCode.Text = Connexion.GetString("Select Barcode from Students Where ID = " + ID);
                    TBBarCode.Text = TBBarCode.Text.Remove(TBBarCode.Text.Length - 1);
                    DataTable dtSubjects = new DataTable();

                  
                    SqlCommand connYearSelected = new SqlCommand("Select * From Years WHERE ID = " + ID2, con);
                    DataTable dt2 = new DataTable();
                    SqlDataAdapter da2 = new SqlDataAdapter(connYearSelected);
                    da2.Fill(dt2);
                    YearCB.SelectedItem = dt2;
                    object d2d2 = YearCB.SelectedItem;
                    string BirthDate = Connexion.GetString(ID, "Students", "Birthdate");
                    Date.Text = BirthDate;
                   
                    Connexion.FillDT(ref dtAttendance, "Select 'Class' as Type,attendance.ID as AID , Class.ID as CID," +
                        "Class.CName , Groups.GroupName as GName,Attendance.TimeStart as Time , Attendance.Date , " +
                        "Case When Attendance_Student.Status = 1 then N'" + this.Resources["Present"].ToString() + "' When Attendance_Student.status = 0 then N'" + 
                        this.Resources["Absent"].ToString() + "' When Attendance_Student.Status = 3 then N'"+ this.Resources["Justified"] + "' End as Status , Attendance_Student.Status as Stat ,  Attendance.Session from Attendance " +
                       "left join Attendance_Student on Attendance_Student.ID = Attendance.ID join Groups on Groups.GroupID = Attendance.GroupID " +
                       "join Class on groups.ClassID = Class.ID " +
                     
                        " Where Attendance_Student.Status != 2 and  Attendance_Student.StudentID = " + SID + " " +
                        " Union Select 'Formation' as Type,Formation_Attendance.ID as AID ,Formation.ID as CID,Formation.Name as CName , ''as GName , Class_Time.TimeStart as Time , Formation_Attendance.Date , " +"Case When Formation_Attendance_Student.Status =1 then N'"+ this.Resources["Present"].ToString() + "'When Formation_Attendance_Student.Status = 0 then N'" + this.Resources["Absent"].ToString() + "' end as Status ,Formation_Attendance_Student.Status as Stat ,'' as Session from Formation_Attendance Left Join Formation_Attendance_Student on Formation_Attendance_Student.AID = Formation_Attendance.ID Join Formation on Formation.ID = Formation_Attendance.FID Join Class_Time on Class_time.FID= Formation.ID  Where Class_time.type = 2 and Formation_Attendance_Student.SID =" +SID);
                    DGAttendance.DataContext = dtAttendance.DefaultView;
                    if (System.IO.File.Exists(Connexion.GetImagesFile() + "\\S" + ID + ".jpg"))
                    {
                        string ShowPic = Connexion.GetImagesFile() + "\\S" + ID + ".jp" +
                            "" +
                            "g";
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(ShowPic);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        SPicture.Source = bitmap;
                        path = ShowPic;
                    }
                    else
                    {
                        if (Gender.SelectedIndex == 1)
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\Women.png");
                            bitmap.EndInit();
                            SPicture.Source = bitmap;
                        }
                        else if (Gender.SelectedIndex == 0)
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\man.png");
                            bitmap.EndInit();
                            SPicture.Source = bitmap;
                        }
                    }
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "StudentM") != 1)
                    {
                        FirstName.IsReadOnly = true;
                        LastName.IsReadOnly = true;
                        PhoneNumber.IsReadOnly = true;
                        ParentNumber.IsReadOnly = true;
                        Adress.IsReadOnly = true;
                        School.IsReadOnly = true;
                        Note.IsReadOnly = true;
                        InscFees.IsEnabled = false;
                        Gender.IsEnabled = false;
                        Date.IsEnabled = false;
                        LevelCB.IsEnabled = false;
                        YearCB.IsEnabled = false;
                        SpecCB.IsEnabled = false;
                    }
                }
               
                if(YearIDFix != "")
                {
                    string LID = Connexion.GetString("Select LevelID from Years Where ID = " + YearIDFix);
                    LevelCB.SelectedValue = LID;
                    YearCB.SelectedValue = YearIDFix;
                    YearCB.IsEnabled = false;
                    LevelCB.IsEnabled = false;
                }
                  ContextMenu contextMenu = new ContextMenu();
                // Create Discount Menu Item
                MenuItem discountMenuItem = new MenuItem();
                discountMenuItem.Header = this.Resources["Discount"];
                discountMenuItem.Click += DiscountMenuItem_Click;
                // Set the icon for Discount
                Image discountImage = new Image();
                discountImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\DiscountContent.png")); // Use your image path here
                discountImage.Width = 16;
                discountImage.Height = 16;
                discountMenuItem.Icon = discountImage;

                // Create Leave Group Menu Item
                MenuItem leaveGroupMenuItem = new MenuItem();
                leaveGroupMenuItem.Header = this.Resources["LeaveGroup"];
                leaveGroupMenuItem.Click += LeaveMenuItem_Click;

                // Set the icon for Leave Group
                Image leaveGroupImage = new Image();
                leaveGroupImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\LeaveContent.png")); // Use your image path here
                leaveGroupImage.Width = 16;
                leaveGroupImage.Height = 16;
                leaveGroupMenuItem.Icon = leaveGroupImage;

                MenuItem EnterGroupMenuItem = new MenuItem();
                EnterGroupMenuItem.Header = this.Resources["ChangeStartSes"];
                EnterGroupMenuItem.Click += LeaveMenuItem_Click;

                // Set the icon for Enter Group
                Image EnterGroupImage = new Image();
                EnterGroupImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\EnterContent.png")); // Use your image path here
                EnterGroupImage.Width = 16;
                EnterGroupImage.Height = 16;
                EnterGroupMenuItem.Icon = EnterGroupImage;
                // Create Change Payment Menu Item
                MenuItem changePaymentMenuItem = new MenuItem();
                changePaymentMenuItem.Header = this.Resources["Olddebt"];
                changePaymentMenuItem.Click += ChangePaymentItem_Click;

                // Set the icon for Change Payment
                Image changePaymentImage = new Image();
                changePaymentImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\debt16.png")); // Use your image path here
                changePaymentImage.Width = 16;
                changePaymentImage.Height = 16;
                changePaymentMenuItem.Icon = changePaymentImage;

                // Add MenuItems to ContextMenu
                contextMenu.Items.Add(EnterGroupMenuItem);
                contextMenu.Items.Add(leaveGroupMenuItem);
                contextMenu.Items.Add(discountMenuItem);
                contextMenu.Items.Add(changePaymentMenuItem);

                // Attach ContextMenu to DataGrid
                DGClass.ContextMenu = contextMenu;

                // Optionally, you can add a MouseRightButtonUp event to trigger the ContextMenu
                DGClass.MouseRightButtonUp += DataGrid_MouseRightButtonUp;
                CBTypeClass.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }
        private void FillDG(string type , string condition )
        {
            if(type == "Class")
            {
                Connexion.FillDG(ref DGClass, "SELECT " +
                "Class_Student.ClassID," +
                "Class_Student.StudentID, " +
                "Class_Student.StartDate as StartDate , " +
                "Class_Student.Stopped as Stopped," +
                "Class_Student.StartPrice as Debt ,  " +
                "Case when Class.MultipleGroups = 'Multiple' " +
                "then Groups.GroupName ENd as GroupName , " +
                "Class.CName as ClassName ," +
                "Class.CSubject ," +
                "groups.GroupID ," +
                "dbo.CalcPriceSum(class_Student.StudentID,Class.ID) as Price " +
                ",Subjects.Subject " +
                "FROM Class  " +
                "left Join Groups on Groups.ClassID = Class.ID  " +
                "right JOIN Class_Student " +
                "ON (Class_Student.ClassID = Class.ID " +
                "and Class_Student.GroupID = Groups.GroupID) " +
                "left JOIN Subjects ON Subjects.ID = Class.CSubject " + condition);
            }
        }
        private void DiscountMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataRowView rowS = (DataRowView)DGClass.SelectedItem;
            if(rowS == null)
            {
                return;
            }
            string CID = ((DataRowView)DGClass.SelectedItem).Row["ClassID"].ToString();
            string GID = ((DataRowView)DGClass.SelectedItem).Row["GroupID"].ToString();
            Panels.Discount discpanel = new Panels.Discount(SID, CID);
            discpanel.ShowDialog();
            FillDG("Class","Where Class_Student.StudentID  = '" + SID + "'");

        }
        private void ChangeStartSes_Click(object sender, RoutedEventArgs e)
        {
            DataRowView rowS = (DataRowView)DGClass.SelectedItem;
            if (rowS == null)
            {
                return;
            }
            string GID = rowS["GroupID"].ToString();
            Commun.ChangeStartSesStudent(SID, GID);
            FillDG("Class", "Where Class_Student.StudentID  = '" + SID + "'");
        }
        private void LeaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataRowView rowS = (DataRowView)DGClass.SelectedItem;
            if(rowS == null)
            {
                return;
            }
            string GID = rowS["GroupID"].ToString();
            Commun.LeaveGroup(SID, GID);
            FillDG("Class","Where Class_Student.StudentID  = '" + SID + "'");
        }
        private void ChangePaymentItem_Click(object sender, RoutedEventArgs e)
        {
            DataRowView rowS = (DataRowView)DGClass.SelectedItem;
            if (rowS == null)
            {
                return;
            }
            string GID = rowS["GroupID"].ToString();
            Commun.ChangeInitialPrice(SID, GID);
            FillDG("Class","Where Class_Student.StudentID  = '" + SID + "'");
        }
        private void DataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

            DGClass.ContextMenu.IsOpen = true;
        }
        public string ResponseText
        {
            get { return SID; }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)YearCB.SelectedItem;
                DataRowView row3 = (DataRowView)LevelCB.SelectedItem;
                if (FirstName.Text != "" || LastName.Text != "" || YearCB.SelectedIndex != -1 || InscFees.SelectedIndex != -1 || row != null || row3 != null )
                {
                    LBFN.Foreground = Brushes.Black;
                    LBLN.Foreground = Brushes.Black;
                    LBLevel.Foreground = Brushes.Black;
                    LBYear.Foreground = Brushes.Black;
                    LBInsc.Foreground = Brushes.Black;
                    string DateofRegister = "";
                    if (DateRegister.Text != "")
                    {
                        DateofRegister = DateRegister.Text;
                    }
                    else
                    {
                        DateofRegister = DateTime.Now.ToString("MM/dd/yyyy");
                    }
                    if (Type == "Add")
                    {
                        string Birthdate = Date.Text;
                        
                        string SelectedYear = row["ID"].ToString();
                        string selectedLevel = row3["ID"].ToString();
                        string command;
                        string command2;
                        string SelectedSpec;
                        if (!Commun.CheckName(FirstName.Text, LastName.Text, string.Format((string)this.Resources["MessageBoxSameName"].ToString(), (FirstName.Text + ' ' + LastName.Text))))
                        {
                            return; 
                        }
                        if (SpecCB.Visibility != Visibility.Collapsed)
                        {
                            if (SpecCB.SelectedIndex == -1)
                            {
                                MessageBox.Show("Please Choose a Speciality");
                                return;
                            }
                            else
                            {
                                DataRowView row2 = (DataRowView)SpecCB.SelectedItem;
                                if(row2 == null)
                                {
                                    MessageBox.Show("Please choose a speciality");
                                    return;
                                }
                                SelectedSpec = row2["ID"].ToString();
                                command = "Insert into Students " +
                               "(FirstName ," +
                               " LastName , " +
                               "PhoneNumber , " +
                               "ParentNumber, " +
                               "Adress , " +
                               "Note , " +
                               "Gender , " +
                               "Year , " +
                               "Register , " +
                               "Speciality , " +
                               "Birthdate , " +
                               "Status ,PayedInsc," +
                               "School , " +
                               "Level ) OUTPUT Inserted.ID values  " +
                               "(N'" + FirstName.Text.Replace("'", "''") + "',N'" +
                               "" + LastName.Text.Replace("'", "''") + "',N'" +
                               "" + PhoneNumber.Text.Replace("'", "''") + "',N'" +
                               "" + ParentNumber.Text.Replace("'", "''") + "',N'" +
                               "" + Adress.Text.Replace("'", "''") + "',N'" +
                               "" + Note.Text.Replace("'", "''") + "',N'" +
                               "" + Gender.SelectedIndex + "','" +
                               "" + SelectedYear + "'," +
                               " '" + DateofRegister + "','" +
                               "" + SelectedSpec + "','" +
                               "" + Birthdate + "'," +
                               "'1'," + InscFees.SelectedIndex + ",N'" +
                               "" + School.Text.Replace("'", "''") + "'," +
                               "" + row3["ID"].ToString() + ")";
                            }
                        }
                        else
                        {
                            command = "Insert into Students " +
                                "(FirstName , " +
                                "LastName , " +
                                "PhoneNumber , " +
                                "ParentNumber, " +
                                "Adress , " +
                                "Note , " +
                                "Gender , " +
                                "Year , " +
                                "Register  , " +
                                "Birthdate , " +
                                "Status," +
                                "PayedInsc," +
                                "School," +
                                "Level ) OUTPUT Inserted.ID values (N'" +
                                "" + FirstName.Text.Replace("'", "''") + "',N'" +
                                "" + LastName.Text.Replace("'", "''") + "',N'" +
                                "" + PhoneNumber.Text.Replace("'", "''") + "',N'" +
                                "" + ParentNumber.Text.Replace("'", "''") + "',N'" +
                                "" + Adress.Text.Replace("'", "''") + "',N'" +
                                "" + Note.Text.Replace("'", "''") + "','" +
                                "" + Gender.SelectedIndex + "','" +
                                "" + SelectedYear + "'," + 
                                "'" + DateofRegister + "','" + Birthdate + "'" +
                                ",'1'," + InscFees.SelectedIndex + " , N'" + School.Text.Replace("'", "''") + "'," +
                                "" + row3["ID"].ToString() + " )";

                        }
                        SID = Connexion.GetInt(command).ToString();
                        int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                        if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                        {
                            int YID2 = Connexion.GetInt("Select Year from Students Where id = " + SID);
                            monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID2);
                        }
                        if (monthlypayment == 1)
                        {
                            Connexion.Insert("Update Students Set MonthlyPayment = 0 Where ID = " +SID);
                            DGCPrice.Visibility = Visibility.Collapsed;
                        }
                        Connexion.InsertHistory(0, SID, 0);
                        string barcode = SID + "$";
                        if (TBBarCode.Text != "")
                        {
                            barcode = TBBarCode.Text + "$";
                        }
                       
                        int numberbarcode = int.Parse(SID);
                        while(!Connexion.IFNULL("Select * from Students Where Barcode = '" + barcode + "' and ID != " + SID))
                        {
                            numberbarcode++;
                            barcode = numberbarcode + "$";
                        }
                        Connexion.Insert("Update students set Barcode = '"+ barcode+"' where ID = " + SID);
                        if(InscFees.SelectedIndex == 0)
                        {
                            int Insc = Connexion.GetInt("Select InscFees from Levels where ID = " +  selectedLevel );
                            int CashID = Connexion.GetInt("Insert into " +
                                "CashRegisterExtra output inserted.ID values (" + Insc + ",N'Insc Fees for student " + FirstName.Text + ' ' + LastName.Text + " '," + Commun.IDCR + "," + Connexion.WorkerID + ",convert(varchar, getdate(), 8) )");
                            Connexion.InsertHistory(0, CashID.ToString(), 9);
                        }
                        if (path != "")
                        {
                            System.IO.File.Copy(path, Connexion.GetImagesFile() + "\\S" + SID + ".jpg");
                        }

                        MessageBox.Show("Inserted Successfully");
                       
                        if (IDforDialog == "-1")
                        {
                            DialogResult = true;
                        }
                        Type = "Show";
                        StudentPayment paymentU = new StudentPayment("1", SID, "");
                        ControlP.Children.Add(paymentU);
                        AddC.IsSelected = true;
                        if (SpecCB.Visibility == Visibility.Collapsed)
                        {
                            Connexion.FillCB(ref SubjectC, "Select * from Subjects Where YearID = " + row["ID"].ToString());
                        }
                        else
                        {
                            DataRowView row2 = (DataRowView)SpecCB.SelectedItem;
                            Connexion.FillCB(ref SubjectC, "Select Subjects.ID ,Subjects.YearID,Subjects.Subject from Subjects join SubjectSpec on SubjectSpec.SubjectID = Subjects.ID  Where Subjects.YearID = " + row["ID"].ToString() + " And SubjectSpec.SpecialityID = " + row2["ID"].ToString());
                        }
                    }
                    else if (Type == "Show")
                    {
                        string Birthdate = Date.Text;
                      
                        string SelectedYear = row["ID"].ToString();
                        string command;

                        int OldInsc = Connexion.GetInt("Select PayedInsc from Students Where ID = " + SID);
                        if (SpecCB.Visibility != Visibility.Collapsed)
                        {
                            DataRowView row2 = (DataRowView)SpecCB.SelectedItem;
                            string SelectedSpec = row2["ID"].ToString();

                            command = "Update Students Set " +
                           "FirstName =  N'" + FirstName.Text.Replace("'", "''") + "'," +
                           "LastName = N'" + LastName.Text.Replace("'", "''") + "'," +
                           "PhoneNumber = N'" + PhoneNumber.Text.Replace("'", "''") + "'," +
                           "ParentNumber = N'" + ParentNumber.Text.Replace("'", "''") + "', " +
                           "Adress = N'" + Adress.Text.Replace("'", "''") + "', " +
                           "Note = N'" + Note.Text.Replace("'", "''") + "', " +
                           "Gender = N'" + Gender.SelectedIndex + "', " +
                           "Year = N'" + SelectedYear + "'," +
                           "BirthDate = N'" + Birthdate + "'," +
                           "School = N'" + School.Text.Replace("'", "''") + "'," +
                           "Speciality = N'" + SelectedSpec + "'," +
                           "Level = " + row3["ID"].ToString() + " , " +
                           "PayedInsc = " + InscFees.SelectedIndex + "  , " +
                           "Register = '" + DateofRegister + 
                           "' Where ID = " + SID;
                        }
                        else
                        {
                            command = "Update Students Set " +
                           "FirstName =  N'" + FirstName.Text.Replace("'", "''") + "'," +
                           "LastName = N'" + LastName.Text.Replace("'", "''") + "', " +
                           "PhoneNumber = N'" + PhoneNumber.Text.Replace("'", "''") + "'," +
                           "ParentNumber = N'" + ParentNumber.Text.Replace("'", "''") + "', " +
                           "Adress = N'" + Adress.Text.Replace("'", "''") + "', " +
                           "Note = N'" + Note.Text.Replace("'", "''") + "', " +
                           "Gender = '" + Gender.SelectedIndex + "', " +
                           "Year = N'" + SelectedYear + "'," +
                           "BirthDate = N'" + Birthdate + "'," +
                           "School = N'" + School.Text.Replace("'", "''") + "'," +
                           "Level  = " + row3["ID"].ToString() + " ," +
                           "PayedInsc = " + InscFees.SelectedIndex + " ," +
                           "Register = '" + DateofRegister + "' Where ID = " + SID;

                        }
                        Connexion.Insert(command);
                        Connexion.InsertHistory(2, SID, 0);
                        if (TBBarCode.Text + "$" != Connexion.GetString("Select Barcode from Students Where ID = " + SID))
                        {
                            string barcode = TBBarCode.Text + "$";
                            int parseint = 0; 
                            bool f = int.TryParse(TBBarCode.Text, out parseint);
                            if (f)
                            {
                                while (!Connexion.IFNULL("Select * from Students Where Barcode = '" + barcode + "' and ID != " + SID))
                                {
                                    parseint++;
                                    barcode = parseint + "$";
                                }
                            }
                            else
                            {
                                 if(!Connexion.IFNULL("Select * from Students Where Barcode = '" + barcode + "' and ID != " + SID))
                                {
                                    string namebarcode = Connexion.GetString("Select Firstname + ' ' + LastName from Students Where BArcode =  '" + barcode + "' and ID != " + SID);
                                    MessageBox.Show("Student (" + namebarcode + ") has this barcode");
                                    return;
                                }
                            }

                            Connexion.Insert("Update students set Barcode = '" + barcode + "' where ID = " + SID);
                        }
                        if (OldInsc != 0)
                        {
                            if (InscFees.SelectedIndex == 0)
                            {
                                int Insc = Connexion.GetInt("Select InscFees from Levels where ID = " + row3["ID"].ToString());
                                int CashID = Connexion.GetInt("Insert into " +
                                    "CashRegisterExtra output inserted.ID values (" + Insc + ",N'Insc Fees for student " + FirstName.Text + ' ' + LastName.Text + " '," + Commun.IDCR + "," + Connexion.WorkerID + ",convert(varchar, getdate(), 8) )");
                                Connexion.InsertHistory(0, CashID.ToString(), 9);
                            }
                        }
                        else if (OldInsc == 0)
                        {
                            if (InscFees.SelectedIndex == 3)
                            {
                                int Insc = -Connexion.GetInt("Select InscFees from Levels where ID = " + row3["ID"].ToString());
                                int CashID = Connexion.GetInt("Insert into " +
                                    "CashRegisterExtra output inserted.ID values (" + Insc + ",N'Refund Insc Fees for student " + FirstName.Text + ' ' + LastName.Text + " '," + Commun.IDCR + "," + Connexion.WorkerID + ",convert(varchar, getdate(), 8) )");
                            }
                        }

                        if (path != "" && path != Connexion.GetImagesFile() + "\\S" + SID + ".jpg" )
                        {
                            if (System.IO.File.Exists(Connexion.GetImagesFile() + "\\S" + SID + ".jpg"))
                            {
                                System.IO.File.Delete(Connexion.GetImagesFile() + "\\S" + SID + ".jpg");
                            }
                            System.IO.File.Copy(path,
                                Connexion.GetImagesFile() + "\\S" + SID + ".jpg");
                        }
                       
                        MessageBox.Show("Updated Successfully");
                        AddC.IsSelected = true;
                    }
                }
                else
                {
                    if(FirstName.Text == "")
                    {
                        LBFN.Foreground = Brushes.Red;
                    }
                    if (LastName.Text == "")
                    {
                        LBLN.Foreground = Brushes.Red;
                    }
                    if (YearCB.SelectedIndex != -1)
                    {
                        LBYear.Foreground = Brushes.Red;
                    }
                    if (InscFees.SelectedIndex != -1)
                    {
                        LBInsc.Foreground = Brushes.Red;
                    }
                    if (LevelCB.SelectedIndex != -1)
                    {
                        LBYear.Foreground = Brushes.Red;
                        LBLevel.Foreground = Brushes.Red;
                    }

                    MessageBox.Show("Please Fill Out the nessasary information");
                }
            }
            catch(Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
           
        }
        private void FirstName_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref LastName);
        }
        private void LastName_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref PhoneNumber , ref FirstName);

        }
        private void PhoneNumber_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref ParentNumber , ref LastName);
        }
        private void ParentNumber_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref Adress , ref PhoneNumber );
        }
        private void School_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref Note, ref Adress);
        }
        private void Adress_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref School , ref ParentNumber);
        }
        private void Note_KeyDown(object sender, KeyEventArgs e)
        {
         
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                Date.Focus();
            }
            else if (e.Key == Key.Up)
            {
                School.Focus();
            }

        }
        private void BDay_KeyDown(object sender, KeyEventArgs e)
        {
            if(Connexion.Language() == 1)
            {
                if(e.Key == Key.Left)
                {
                    DateRegister.Focus();
                    return; 
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    DateRegister.Focus();
                    return; 
                }
            }
            if (e.Key == Key.Enter )
            {
                DateRegister.Focus();
            }
            else if (e.Key == Key.Up)
            {
                Note.Focus();
            }
            else if ( e.Key == Key.Down)
            {
                Gender.Focus(); 
            }

        }
        private void Register_KeyDown(object sender, KeyEventArgs e)
        {
            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Right)
                {
                    Date.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Left)
                {
                    Date.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                Gender.Focus();
            }
            else if (e.Key == Key.Up)
            {
                Note.Focus();
            }
            else if (e.Key == Key.Down)
            {
                InscFees.Focus();
            }

        }
        private void Gender_KeyDown(object sender, KeyEventArgs e)
        {
            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Left)
                {
                    InscFees.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    InscFees.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                InscFees.Focus();
            }
        }
        private void InscFees_KeyDown(object sender, KeyEventArgs e)
        {
            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Right)
                {
                    Gender.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Left)
                {
                    Gender.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                LevelCB.Focus();
            }
        }
        private void Level_KeyDown(object sender, KeyEventArgs e)
        {

            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Left)
                {
                    YearCB.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    YearCB.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                YearCB.Focus();
            }
          
        }
        private void Year_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpecCB.SelectedIndex = -1;
            DataRowView row = (DataRowView)YearCB.SelectedItem;
            DataRowView row2 = (DataRowView)LevelCB.SelectedItem; 
            if (row != null && row2 != null)
            {
                string IDYear = row["ID"].ToString();
                SqlConnection con = Connexion.Connect();
                if (row2["IsSpeciality"].ToString() == "0")
                {
                }
                else
                {
                    Connexion.FillCB(ref SpecCB, "Select * from Specialities Where YearID = " + IDYear);
                }
            }
        }
        private void Insert_Pic(object sender, RoutedEventArgs e)
        {
            open.Filter = "Image Files(*.jpg; *.jpeg; *.bmp )|*.jpg; *.jpeg; *.bmp";
            if (open.ShowDialog() == true)
            {

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(open.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                SPicture.Source = bitmap;
                path = open.FileName; 
            }
        }
        private void BtnDeleteClass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you Sure u want to Remove the student from this Class?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    string CID = ((DataRowView)DGClass.SelectedItem).Row["ClassID"].ToString();
                    string GID = ((DataRowView)DGClass.SelectedItem).Row["GroupID"].ToString();
                    int ses = Connexion.GetInt(GID, "Groups", "Sessions", "GroupID");
                    int BegSes = Connexion.GetInt("Select Session from Class_Student Where GroupID = " + GID + " and StudentID = " + SID);
                    if (ses == BegSes)
                    {
                        Connexion.Insert("Delete from Class_Student Where GroupID = " + GID + " and StudentID = " + SID);
                    }
                    else
                    {
                        Connexion.Insert("Update  Class_Student" +
                            " Set EndSession = " + ses + " " +
                            "Where StudentID = '" + SID + "' " +
                            "and GroupID = '" + GID + "'");
                    }

                    FillDG("Class","Where Class_Student.StudentID  = '" + SID + "'");
                    MessageBox.Show("Removed Succesfully");
                }
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }
        private void SubjectC_SelectionChanged(object sender, SelectionChangedEventArgs e) // fill class combobox depending subject chosen
        {
            DataRowView row = (DataRowView)SubjectC.SelectedItem;
            if (row != null)
            {
                SPGroup.Visibility = Visibility.Collapsed;

                Connexion.FillCB(ref CBclass,
                  "Select Class.CName ,Class.MultipleGroups ,Class.ID from Class Where Class.CSubject = " + row["ID"].ToString() + " " +
                  "Except  Select Class.CName , Class.MultipleGroups,Class.ID from Class_Student left join Class on Class_Student.ClassID = Class.ID  Where Class.CSubject = " + row["ID"].ToString() + " And Class_Student.StudentID = " + SID);
            }

        }
        private void SubmitClass_Click_2(object sender, RoutedEventArgs e)//not finished
        {
            try
            {
                DataRowView vrow;
                DataRow row;
                vrow = (DataRowView)CBclass.SelectedItem;
                DataRowView rowSubject = (DataRowView)SubjectC.SelectedItem;
                if (vrow == null)
                {
                    MessageBox.Show("Please Fill all Information");
                    return;
                }
                if (CBTypeClass.SelectedIndex == 0)
                {
                    vrow = (DataRowView)CBclass.SelectedItem;
                    row = vrow.Row;
                    if (row == null)
                    {
                        return;
                    }
                    DataRowView vrow2;
                    DataRow row2;
                    vrow2 = (DataRowView)SubjectC.SelectedItem;
                    row2 = vrow2.Row;
                    string ClassID = row["ID"].ToString();
                    string GroupID = "";
                    if (row["MultipleGroups"].ToString() == "Multiple")
                    {
                        DataRowView rowGroup = (DataRowView)CBGroup.SelectedItem;
                        if (rowGroup != null)
                        {
                            GroupID = rowGroup["GroupID"].ToString();
                        }
                    }
                    else
                    {
                        GroupID = Connexion.GetInt(ClassID, "Groups", "GroupID", "ClassID").ToString();
                    }
                    Commun.AddStudentToClass(SID,GroupID, DateClassReg.SelectedDate?.ToString("dd-MM-yyyy"));
                    SubjectC.SelectedIndex = -1;
                    CBclass.SelectedIndex = -1;
                    CBGroup.SelectedIndex = -1;
                    SPGroup.Visibility = Visibility.Collapsed;
                    int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                    if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                    {
                        int YID2 = Connexion.GetInt("Select Year from Students Where id = " + SID);
                        monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID2);
                    }
                    if (monthlypayment == 1)
                    {
                        Methods.InsertStudentClassMonthly(SID, ClassID);
                    }
                    else
                    {
                        Commun.CheckDiscountAddClass(SID, this.Resources, 0, -1);
                    }
                    FillDG("Class","Where Class_Student.StudentID  = '" + SID + "'");
                    ControlP.Children.Clear();
                    StudentPayment paymentU = new StudentPayment("1", SID, "");
                    ControlP.Children.Add(paymentU);
                    SubjectC.SelectedIndex = -1;
                    CBclass.SelectedIndex = -1;
                    CBGroup.SelectedIndex = -1;
                    SPGroup.Visibility = Visibility.Collapsed;
                }
                else
                {
                    string FID = vrow["ID"].ToString();
                    int Hours = Connexion.GetInt("Select Hours from Formation Where ID = " + FID);
                    int Remaining = Connexion.GetInt("Select dbo.FormationRemainingHours(" + FID + ")");
                    int price = Connexion.GetInt("Select Price from Formation Where ID =" + FID);
                    int StartHour = Hours - Remaining;
                    Connexion.Insert("Insert into Formation_Student Values(" + SID + "," + FID + "," + price + ",''," + Hours + " ," + StartHour + " )");
                    CBTypeClass.SelectedIndex = -1;
                    CBTypeClass.SelectedIndex = 1;
                    Connexion.FillDG(ref DGFormation, "Select Formation.Name , Formation.ID as ID ,dbo.CalculateRemainingPayment(Formation_Student.FID," + SID + ") as SuPrice, Formation_Student.Note as Note , Formation_Student.Hours as Hours , Formation_Student.StartHour as StartHour from Formation_Student join Formation on Formation_Student.FID = Formation.ID  join Students on Formation_Student.SID=Students.ID Where Formation_Student.SID = " + SID);
                }

            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void SpecCB_SelectionChanged(object sender, SelectionChangedEventArgs e)//choosing a speciality making the SubjectCb Filled
        {
            DataRowView row = (DataRowView)SpecCB.SelectedItem;
            if (row != null)
            {
                string YearID = row["YearID"].ToString();
                string SpecID = row["ID"].ToString();

                SqlConnection con = Connexion.Connect();

                SqlCommand command = new SqlCommand("Select * From Subjects JOIN SubjectSpec ON Subjects.ID = SubjectSpec.SubjectiD Where Subjects.YearID = " + YearID + " And SubjectSpec.SpecialityID = " + SpecID, con);
                DataTable dt1 = new DataTable();
                SqlDataAdapter da1 = new SqlDataAdapter(command);
                da1.Fill(dt1);
                SubjectC.ItemsSource = dt1.DefaultView;
            }
        }

        private void LevelCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpecCB.SelectedIndex = -1;
            YearCB.SelectedIndex = -1;
            DataRowView row = (DataRowView)LevelCB.SelectedItem;
            if(row != null)
            {
                if (row["IsSpeciality"].ToString() == "0")
                {
                    SpecCB.Visibility = Visibility.Collapsed;
                    Speclabel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SpecCB.Visibility = Visibility.Visible;
                    Speclabel.Visibility = Visibility.Visible;
                }
                Connexion.FillCB(ref YearCB, "Select * from Years Where LevelID = " + row["ID"].ToString());
            }
        }

        private void CBclass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CBclass.SelectedItem;
          
            if (row != null)
            {
                if (CBTypeClass.SelectedIndex==0)
                {
                    if (row["MultipleGroups"].ToString() == "Multiple")
                    {
                        int gender = Connexion.GetInt(SID, "Students", "Gender");
                        SPGroup.Visibility = Visibility.Visible;
                        Connexion.FillCB(ref CBGroup, "Select * from groups Where ClassID = " + row["ID"].ToString() + " And (GroupGender = 2 or GroupGender  =  " + gender + " )");
                    }
                    else
                    {
                        SPGroup.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void CBGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        

        private void PhoneNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Regex regex = new Regex("[^0-9]+");
            //e.Handled = regex.IsMatch(e.Text);
        }

        private void ParentNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Regex regex = new Regex("[^0-9]+");
            //e.Handled = regex.IsMatch(e.Text);
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

        private void AttendanceSearch(object sender, SelectionChangedEventArgs e)
        {
            if (finishinitializing)
            {
                Search();
            }
        }
        private void Search()
        {

             Condition = "1 > 0 ";
            string selectedDay = (DaySearchAttend.SelectedItem as ComboBoxItem)?.Content.ToString();
            string selectedMonth = (MonthSearchAttend.SelectedItem as ComboBoxItem)?.Content.ToString();
            string selectedYear = (YearSearchAttend.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (DaySearchAttend.SelectedIndex != 0)
            {
                // Matches 'dd' at the beginning of the date string
                Condition += $"AND date LIKE '{selectedDay}-%'";
            }

            if (MonthSearchAttend.SelectedIndex != 0)
            {
                // Matches '-MM-' in the middle
                Condition += $" AND date LIKE '%-{selectedMonth}-%'";
            }

            if (YearSearchAttend.SelectedIndex != 0)
            {
                // Matches year at the end
                Condition += $" AND date LIKE '%-{selectedYear}'";
            }

            if (CBAttendanceSession.SelectedIndex != -1)
            {
                int index = CBAttendanceSession.SelectedIndex + 1;
                Condition += "And Session = '" + index + "' ";
            }
            if(CBAttendanceClass.SelectedIndex != -1)
            {
                DataRowView row = (DataRowView)CBAttendanceClass.SelectedItem;
                string CID = row["ID"].ToString();
                Condition += "And CID = '" + CID + "'"; 
            }
            if(CBType.SelectedIndex != -1)
            {
                Condition += "And Stat = " + CBType.SelectedIndex;
            }
            dtAttendance.DefaultView.RowFilter = Condition;
        }

      
        private void DGAttendance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            
            DataGridRow DGrow = sender as DataGridRow;
            DataRowView drv = DGrow.DataContext as DataRowView;
            if(drv["Type"].ToString() == "Class")
            {
                var AddS = new AttendanceAdd(drv["AID"].ToString(), "Show", "1");
                AddS.Show();
            }
            else
            {

            }
        }

  

        private void Button_Click_PrintMultiple(object sender, RoutedEventArgs e)
        {
            DataRow[] filterd_result = dtAttendance.Select(Condition);
            if(filterd_result.Length > 0)
            {
                FastReports.PrintHistoryStudent(SID, filterd_result);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Search();
        }

        private void Button_Click_Crop(object sender, RoutedEventArgs e)
        {
            string f = Commun.Crop(path, ref SPicture);
            if(f != "-1")
            {
                path = f; 
            }
        }

        private void scan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var deviceManager = new DeviceManager();

                // Create an empty variable to store the scanner instance
                DeviceInfo firstScannerAvailable = null;

                // Loop through the list of devices to choose the first available
                for (int i = 1; i <= deviceManager.DeviceInfos.Count; i++)
                {
                    // Skip the device if it's not a scanner
                    if (deviceManager.DeviceInfos[i].Type != WiaDeviceType.ScannerDeviceType)
                    {
                        continue;
                    }

                    firstScannerAvailable = deviceManager.DeviceInfos[i];

                    break;
                }

                // Connect to the first available scanner
                var device = firstScannerAvailable.Connect();

                // Select the scanner
                var scannerItem = device.Items[1];

                // Retrieve a image in JPEG format and store it into a variable
                var imageFile = (ImageFile)scannerItem.Transfer(FormatID.wiaFormatJPEG);
                path = @"C:\ProgramData\EcoleSetting\EcolePhotos\Scan"+ SID + ".jpg";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                imageFile.SaveFile(path);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                SPicture.Source = bitmap;

            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void PhoneNumber_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Note_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void School_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void FirstName_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }

        private void CBTypeClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SubjectC.SelectedIndex = -1;
            CBclass.SelectedIndex = -1;
            if (CBTypeClass.SelectedIndex == 0)
            {
                int ID2 = Connexion.GetInt(SID, "Students", "Year");
                if (SpecCB.Visibility == Visibility.Collapsed)
                {
                    Connexion.FillCB(ref SubjectC, "Select * from Subjects Where YearID = " + ID2);
                }
                else
                {
                    int SpecID = Connexion.GetInt(SID, "Students", "Speciality");
                    SpecCB.SelectedValue = SpecID.ToString();
                    DataRowView row2 = (DataRowView)SpecCB.SelectedItem;
                    Connexion.FillCB(ref SubjectC, "Select " +
                        "Subjects.ID ," +
                        "Subjects.YearID," +
                        "Subjects.Subject " +
                        "from Subjects " +
                        "join SubjectSpec on SubjectSpec.SubjectID = Subjects.ID  " +
                        "Where Subjects.YearID = " + ID2 + " " +
                        "And SubjectSpec.SpecialityID = " + SpecID);
                }
                SubjectC.Visibility = Visibility.Visible;
                DGClass.Visibility = Visibility.Visible;
                DGFormation.Visibility = Visibility.Collapsed;
                LBSubject.Visibility = Visibility.Visible;
                 FillDG("Class","Where Class_Student.StudentID  = '" + SID + "'");
            }
            else if (CBTypeClass.SelectedIndex == 1 )
            {
                DGClass.Visibility = Visibility.Collapsed;
                DGFormation.Visibility = Visibility.Visible;
                SubjectC.Visibility = Visibility.Collapsed;
                LBSubject.Visibility = Visibility.Collapsed;
                Connexion.FillCB(ref CBclass, "Select Formation.Name as CName , Formation.ID From Formation Except Select Formation.Name as CName, Formation.ID from Formation Left Join Formation_Student on Formation_Student.FID = Formation.ID Where Formation_Student.SID = " + SID);
                Connexion.FillDG(ref DGFormation, "Select Formation.Name , Formation.ID as ID ,dbo.CalculateRemainingPayment(Formation_Student.FID," + SID + ") as SuPrice, Formation_Student.Note as Note , Formation_Student.Hours as Hours , Formation_Student.StartHour as StartHour from Formation_Student join Students on Formation_Student.SID=Students.ID join Formation on Formation_Student.FID = Formation.ID Where Formation_Student.SID = " + SID);
            }
        }

        private void DGFormation_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {


                if (e.EditAction == DataGridEditAction.Commit)
                {
                    if (e.Column is DataGridTextColumn textColumn)
                    {
                        int columnIndex = e.Column.DisplayIndex;
                        DataRowView rowView = e.Row.Item as DataRowView;
                        if (rowView == null)
                        {
                            return;
                        }
                        int FID = (int)rowView["ID"];
                        if (columnIndex == 2)
                        {
                            TextBox textBox = e.EditingElement as TextBox;
                            int remaining = Connexion.GetInt("Select Hours from Formation_Student Where SID = " + SID + " and FID = " + FID);
                            if (MessageBox.Show("Are you sure you want to Modify  the Hours for this Student?",
                             "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                // The "RemainingPayment" column has been edited
                                string editedValue = textBox.Text;
                                if (int.TryParse(editedValue, out int intValue))
                                {


                                    Connexion.Insert("Update Formation_Student Set Hours =" + editedValue + " where SID =" + SID + " and FID =" + FID);
                                    MessageBox.Show("Updated Succesfully");
                                }
                                else
                                {
                                    MessageBox.Show("Please enter a correct ammount");
                                    textBox.Text = remaining.ToString();
                                    return;
                                }
                            }
                            else
                            {
                                textBox.Text = remaining.ToString();
                                return;
                            }
                        }
                        else if (columnIndex == 3) // Assuming "RemainingPayment" column is at index 3
                        {
                            TextBox textBox = e.EditingElement as TextBox;
                            int remaining = Connexion.GetInt("Select dbo.CalculateRemainingPayment(" + FID + ", " + SID + ")");
                            if (MessageBox.Show("Are you sure you want to Modify  the remaining payment for this Student?",
                             "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                // The "RemainingPayment" column has been edited
                                string editedValue = textBox.Text;
                                if (int.TryParse(editedValue, out int intValue))
                                {

                                    int difference = remaining - intValue;
                                    Connexion.Insert("Update Formation_Student Set SupposedPrice = SupposedPrice - " + difference + " where SID =" + SID + " and FID =" + FID);
                                    MessageBox.Show("Updated Succesfully");
                                }
                                else
                                {
                                    MessageBox.Show("Please enter a correct ammount");
                                    textBox.Text = remaining.ToString();
                                    return;
                                }
                            }
                            else
                            {
                                textBox.Text = remaining.ToString();
                                return;
                            }

                        }
                        else if (columnIndex == 4) // Assuming "Note" column is at index 4
                        {

                            TextBox textBox = e.EditingElement as TextBox;
                            string editedValue = textBox.Text;
                            Connexion.Insert("Update Formation_Student Set Note =N'" + editedValue + "' where SID= " + SID + " and FID = " + FID);
                        }
                    }
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void btndeletefromclass_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                string CID = ((DataRowView)DGClass.SelectedItem).Row["ClassID"].ToString();
                string GID = ((DataRowView)DGClass.SelectedItem).Row["GroupID"].ToString();

                Connexion.Insert("Delete from Class_Student Where GroupID = " + GID + " and StudentID = " + SID);
                DataTable dtAttendance = new DataTable();
                Connexion.FillDT(ref dtAttendance, "Select ID from Attendance Where GroupID = " + GID);
                for (int i = 0; i < dtAttendance.Rows.Count; i++)
                {
                    Connexion.Insert("Delete from Attendance_Student Where StudentID = " + SID + " and ID = " + dtAttendance.Rows[i]["ID"].ToString());
                }

                FillDG("Class","Where Class_Student.StudentID  = '" + SID + "'");

                MessageBox.Show("Removed Succesfully");


            }
            catch (Exception er)
            {

            }

        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            CBType.SelectedIndex = -1;
            DaySearchAttend.SelectedIndex = 0;
            MonthSearchAttend.SelectedIndex = 0;
            YearSearchAttend.SelectedIndex = 0;
            CBAttendanceClass.SelectedIndex = -1;
            CBAttendanceSession.SelectedIndex = -1;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AddC.IsSelected || AddP.IsSelected)
            {
                this.Width = 700;
            }
            else
            {
                this.Width = 600;
            }
        }

        private void DGClass_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataRowView rowS = (DataRowView)DGClass.SelectedItem;
            if (rowS == null)
            {
                return;
            }
            string CID = rowS["ClassID"].ToString();
            string Multi = Connexion.GetString("Select MultipleGroups from Class Where ID = " + CID);
            var AddW = new ClassAdd("Show", CID, Multi);
            AddW.ShowDialog();
            FillDG("Class", "Where Class_Student.StudentID  = '" + SID + "'");
        }
    }
}


