using Gestion_De_Cours.Classes;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gestion_De_Cours.UControl;
using Gestion_De_Cours.Panels;
using System.IO;
using ZonaTools.XPlorerBar;
using Gestion_De_Cours.Properties;
using FastReport;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Gestion_De_Cours
{
    /// <summarys>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string UName = "SStudent";
        bool checkclose = true;
        private DispatcherTimer timer;
        private UControl.AllAttendance  allA ;
        string datetoday;

        string Patch = Directory.GetCurrentDirectory() + @"\Patches\Patch.txt";
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                Commun.ScreenWIdth = screenWidth;
                Commun.ScreenHeight = screenHeight;
                timer = new DispatcherTimer();
                Commun.SetMW(this);
                SStudent.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                ShowTableU Studentsmenu = new ShowTableU("Student");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Studentsmenu);
             //  UpdateAllAttendance();
                string path = @"C:\ProgramData\EcoleSetting\Mouathfile.txt";
                Commun.setResourceDic(Connexion.Language());
                FastReports.setResourceDic(Connexion.Language());
                // Set the interval to 5 minutes
                timer.Interval = TimeSpan.FromMinutes(5);
                timer.Tick += BackUp5Minutes;
                // Start the timer
                timer.Start();
              
                Methods.ReadPatches(Patch);
                int count = 0; 
                
                bool cashregist= Connexion.IFNULL("Select * from Cashregister where Date = '" + DateTime.Today.ToString("dd-MM-yyyy") + "'");
               
                if (cashregist)
                {
                    Commun.IDCR = Connexion.GetInt("insert into CashRegister Output Inserted.ID Values ('" + DateTime.Today.ToString("dd-MM-yyyy") + "',null )");
                }
                else
                {
                    Commun.IDCR = Connexion.GetInt("Select ID from CashRegister Where Date = '" + DateTime.Today.ToString("dd-MM-yyyy") + "'");
                }
                int lang = Connexion.Language();
                if (Connexion.GetInt(Connexion.WorkerID, "Users", "CashRegister") != 1)
                {
                    CashRegister.IsEnabled = false;
                }
                if (Connexion.GetInt(Connexion.WorkerID, "Users", "EcoleInfo") != 1)
                {
                    SettingsXP.IsEnabled = false;
                }
                
                if (lang == 1)
                {
                    this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
                    Arabic.IsChecked = true;
                }
                else if (lang == 0)
                {
                    English.IsChecked = true;
                }
                else if(lang == 2)
                {
                    French.IsChecked = true;
                }
                datetoday = DateTime.Now.ToString("dd-MM-yyyy");
                // Pre-create control asynchronously but still on UI thread
                this.Loaded += async (s, e) =>
                {
                    allA = new UControl.AllAttendance();

                    // Run filldata in background
                    await Task.Run(() =>
                    {
                        allA.filldata(datetoday);
                    });

                };
            }
            catch (Exception e)
            {
                Methods.ExceptionHandle(e);
            }
        }
    
        public void UpdateAllAttendance()
        {
            allA = new AllAttendance();
        }
        private void TimeButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void BackUp5Minutes(object sender, EventArgs e)
        {
            try
            {
                Connexion.CreateBackUpNoRestore("BACKUP__" + Connexion.DB + "__" + DateTime.Now.ToString("yyyy_MM_dd__HH_mm"));
            }
            catch(Exception er )
            {

            }
            // This method will be executed every 5 minutes
        }


        private void Settingsbutton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Payment_Click(object sender, RoutedEventArgs e)
        {

        }

        private void XPlorerItem_Click(object sender, RoutedEventArgs e)
        {
            string t = ((XPlorerItem)e.OriginalSource).Name;
            if (UName == "SStudent")
            {
                SStudent.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");
            }
            else if (UName == "SPayment")
            {
                SPayment.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");
            }
            else if (UName == "STeacher")
            {
                STeacher.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");
            }
            else if (UName == "SClass")
            {
                SClass.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");
            }
            else if (UName == "SWorker")
            {
                SWorker.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");
            }
            else if (UName == "Settings")
            {
                Settings.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");

            }
            else if (UName == "SGroup")
            {
                SGroup.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");
            }
            else if (UName == "History")
            {
                History.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");

            }
            else if (UName == "CashRegister")
            {
                CashRegister.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");


            }
            else if (UName == "ShortCuts")
            {
                ShortCuts.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");
            }
            else if (UName == "PClass")
            {
                PClass.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");

            }
            else if (UName == "AttendanceAll")
            {
                AttendanceAll.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");
            }
            else if (UName == "Scheduler")
            {
                Scheduler.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");
            }
            else if (UName == "FClass")
            {
                FClass.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#56662d");
            }
            UName = t;

            if (t == "SStudent")
            {
                SStudent.Foreground = (SolidColorBrush) new BrushConverter().ConvertFrom("#e6095e");
                ShowTableU Studentsmenu = new ShowTableU("Student");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Studentsmenu);
            }
            else if (t == "SPayment")
            {
                SPayment.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                ShowTableU Studentsmenu = new ShowTableU("SPayment");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Studentsmenu);
            }
            else if (t == "STeacher")
            {
                 STeacher.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                 ShowTableU Teachersmenu = new ShowTableU("Teacher");
                 Coontrol.Children.Clear();
                 Coontrol.Children.Add(Teachersmenu);
            }
            else if (t == "SClass")
            {
                SClass.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                ShowTableU Classesmenu = new ShowTableU("Class");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Classesmenu);
            }
            else if (t == "SWorker")
            {
                SWorker.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                ShowTableU Workersmenu = new ShowTableU("Worker");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Workersmenu);
            }
            else if (t == "Settings")
            {
                Settings.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                SettingEcole SE = new SettingEcole();
                Coontrol.Children.Clear();
                Coontrol.Children.Add(SE);
            }
            else if (t == "SGroup")
            {
                SGroup.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                ShowTableU Groups = new ShowTableU("Group");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Groups);
            }
            else if (t == "History")
            {
                History.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                ShowTableU His = new ShowTableU("History");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(His);
            }
            else if (t == "AttendanceAll")
            {
                bool f = allA.FINISH;
                while (!allA.FINISH)
                {
                    Task.Delay(100); // check every 100ms, UI stays responsive
                }
                if (!allA.ALREADYOPENED)
                {
                    allA.DPTodayDate.Text = datetoday;
                    allA.fill(datetoday);
                }
                AttendanceAll.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(allA);
            }
            else if (t == "Scheduler")
            {
                Schedulerr sched = new Schedulerr();
                Scheduler.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(sched);
            }
            else if (t == "CashRegister")
            {
                CashRegister.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                string IDCR = Connexion.GetInt("Select ID from CashRegister Where Date = '" + DateTime.Today.ToString("dd-MM-yyyy") + "'").ToString();
                if (Connexion.IFNULLVar("Select StartAmmount from CashRegister Where   ID =" + IDCR))
                {
                    var dialog = new Backup("Enter Today's Starting Ammount", 1);
                    if (dialog.ShowDialog() == true)
                    {
                        int ID = Connexion.GetInt("update CashRegister set startammount = " +
                   " " + dialog.ResponseText + " where id = " + IDCR);
                        Connexion.InsertHistory(0, ID.ToString(), 9);
                    }
                    else
                    {
                        return;
                    }
                }

                CashRegister Cash = new CashRegister();
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Cash);

            }
            else if (t == "ShortCuts")
            {
                ShortCuts.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                ShortCuts shortcuts = new ShortCuts();
                Coontrol.Children.Clear();
                Coontrol.Children.Add(shortcuts);

            }
            else if (t == "PClass")
            {
                PClass.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                ShowTableU Workersmenu = new ShowTableU("Particulier");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Workersmenu);
                //Coontrol.Children.Clear();
                //Coontrol.Children.Add(P);
            }
            else if (t== "FClass")
            {
                FClass.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#e6095e");
                ShowTableU Workersmenu = new ShowTableU("Formation");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Workersmenu);
            }
        }

        private void Discounts_Click(object sender, RoutedEventArgs e)
        {
            Discount discount = new Discount();
            discount.Show();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var absent = new Justif();
            absent.Show();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
        }

        private void English_Checked(object sender, RoutedEventArgs e)
        {
            Arabic.IsChecked = false;
            French.IsChecked = false; 
            Connexion.Insert("Update EcoleSetting Set language = 0");
            SetLang();
        }

        private void Arabic_Checked(object sender, RoutedEventArgs e)
        {
            English.IsChecked = false;
            French.IsChecked = false;

            Connexion.Insert("Update EcoleSetting Set language = 1");
            SetLang();
        }
        private void SetLang()
        {
            ResourceDictionary ResourceDic = new ResourceDictionary();
            int lang = Connexion.Language();
            if (lang == 1)
            {
                ResourceDic.Source = new Uri("Dictionary\\AR.xaml", UriKind.Relative);
                this.FlowDirection = FlowDirection.RightToLeft;
            }
            else if(lang == 0)
            {
                ResourceDic.Source = new Uri("Dictionary\\EN.xaml", UriKind.Relative);
                this.FlowDirection = FlowDirection.LeftToRight;
            }
            else if (lang == 2)
            {
                ResourceDic.Source = new Uri("Dictionary\\FR.xaml", UriKind.Relative);
                this.FlowDirection = FlowDirection.LeftToRight;
            }
            Commun.setResourceDic(lang);
            FastReports.setResourceDic(lang);
            this.Resources.MergedDictionaries.Add(ResourceDic);
            if (UName == "SStudent")
            {
                ShowTableU Studentsmenu = new ShowTableU("Student");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Studentsmenu); 

            }
            else if (UName == "SPayment")
            {
                ShowTableU Studentsmenu = new ShowTableU("SPayment");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Studentsmenu);
            }
            else if (UName == "STeacher")
            {
                ShowTableU Teachersmenu = new ShowTableU("Teacher");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Teachersmenu);
            }
            else if (UName == "SClass")
            {
                ShowTableU Classesmenu = new ShowTableU("Class");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Classesmenu);
            }
            else if (UName == "SWorker")
            {
                ShowTableU Workersmenu = new ShowTableU("Worker");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Workersmenu);
            }
            else if (UName == "Settings")
            {
                SettingEcole SE = new SettingEcole();
                Coontrol.Children.Clear();
                Coontrol.Children.Add(SE);
            }
            else if (UName == "SGroup")
            {
                ShowTableU Groups = new ShowTableU("Group");
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Groups);
            }
            else if(UName == "ShortCuts")
            {
                ShortCuts Groups = new ShortCuts();
                Coontrol.Children.Clear();
                Coontrol.Children.Add(Groups);
            }

        }

        private void FastReport_Checked(object sender, RoutedEventArgs e)
        {
            Commun.FastReportEdit = 0; 
        }

        private void FastReport_Unchecked(object sender, RoutedEventArgs e)
        {
            Commun.FastReportEdit = 1;
        }

        private void BarCode_Click(object sender, RoutedEventArgs e)
        {
            var scanningCodeBar = new ScanningCodeBar();
            scanningCodeBar.Show();
        }

        private void MenuItem_Click_GenerateAllBarcode(object sender, RoutedEventArgs e)
        {
            Methods.PrintBarcode("select   FirstName + ' ' + LastName as Name , " +
               "BarCode " +
               "from Students ORDER BY Name ASC");
        }

        private void MenuItem_Click_StudentPayment(object sender, RoutedEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                string path = @"C:\ProgramData\EcoleSetting\" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
                if (File.Exists(path))
                {
                    System.Diagnostics.Process.Start("notepad.exe", path);
                }
                else
                {
                    MessageBox.Show("No Exceptions exist today");
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.T)
            {
                MessageBox.Show("5.0.0.18");
            }
            else if (e.Key == Key.F1)
            {
                if (Connexion.GetInt(Connexion.WorkerID, "Users", "StudentA") == 1)
                {
                    var AddS = new StudentAdd("Add", "");
                    AddS.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Sorry , You Dont have privilage to do this action");
                }
            }
            else if (e.Key == Key.F2)
            {
                if (Connexion.GetInt(Connexion.WorkerID, "Users", "StudentA") == 1)
                {
                    EmptyPage Empty = new EmptyPage("StudentPayment", "", "");
                    Empty.Show();
                }
                else
                {
                    MessageBox.Show("Sorry , You Dont have privilage to do this action");
                }
            }
        }

        private void MenuItem_Click_TeacherPayment(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_Disconnect(object sender, RoutedEventArgs e)
        {
            for (int intCounter = App.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
            {
                if (App.Current.Windows[intCounter].Name != this.Name)
                {
                    App.Current.Windows[intCounter].Close();
                }
            }
            Login l = new Login();
            l.Show();
            this.Close();
        }
        private void myWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void PrintAttendance_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                Panels.DatePicker dp = new Panels.DatePicker();
                dp.ShowDialog();
                string date = dp.ResponseDate.Text.Replace("/", "-");
                //DateTime.Today.ToString("d").Replace("/", "-")
                DataTable dtAttendance = new DataTable();
                Connexion.FillDT(ref dtAttendance, "Select Attendance.ID as AID , Attendance.GroupID as GID from Attendance Where  Date = '" + date + "'" );
                Report r = new Report();
                DataSet ds = new DataSet();
                DataTable DataAttend = new DataTable();
                DataTable DataEcole = new DataTable();
                DataTable dtGroup = new DataTable();
                if(dtAttendance.Rows.Count == 0)
                {
                    MessageBox.Show("No Attendance in this date");
                }

                for (int i = 0; i < dtAttendance.Rows.Count; i++)
                {
                    string AID = dtAttendance.Rows[i]["AID"].ToString();
                    string GID = dtAttendance.Rows[i]["GID"].ToString();
                    string CID = Connexion.GetClassID(GID).ToString();
                    ds.Clear();
                    DataAttend.Clear();
                    DataAttend.Columns.Clear();
                    DataEcole.Columns.Clear();
                    DataEcole.Clear();
                    dtGroup.Columns.Clear();
                    dtGroup.Clear();
                    ds.Tables.Clear();
                    string path = @"C:\ProgramData\EcoleSetting\Mouathfile.txt";
                    if (Connexion.Language() == 0)
                    {
                        if (File.Exists(path))
                        {

                            r.Load(@"C:\Users\Home\Desktop\C# Projects\Gestion_De_Cours\FastReport\AttendanceFastReport.frx");
                        }
                        else
                        {
                            r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\AttendanceFastReport.frx");
                        }
                    }
                    else
                    {
                        if (File.Exists(path))
                        {
                            r.Load(@"C:\Users\Home\Desktop\C# Projects\Gestion_De_Cours\FastReport\AttendanceFastReportAR.frx");
                        }
                        else
                        {
                            r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\AttendanceFastReportAR.frx");
                        }
                    }
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
                    DataEcole.TableName = "DataEcole";
                    Connexion.FillDT(ref DataEcole, "Select NameFR,NameAR,N'" + Connexion.GetImagesFile() + "\\EcoleLogo.jpg'  as Logo , Number ,Number2,Adress from EcoleSetting");
                    ds.Tables.Add(DataEcole);
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
                        return;
                    }
                    else
                    {
                        if (i == 0)
                        {
                            r.Prepare();
                        }
                        else
                        {
                            r.Prepare(true);
                        }
                    }
                }
                if (Commun.FastReportEdit == 0)
                {
                    r.Design();
                }
                else
                {
                    r.ShowPrepared();
                }
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er); 
            }
        }

        private void DiscountsStandard_Click(object sender, RoutedEventArgs e)
        {
            var discountStandard = new DiscountStandard();
            discountStandard.Show();
        }

        private void French_Checked(object sender, RoutedEventArgs e)
        {
            English.IsChecked = false;
            Arabic.IsChecked = false;

            Connexion.Insert("Update EcoleSetting Set language = 2");
            SetLang();
        }

        private void ImpStudents_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application xlapp;
            Microsoft.Office.Interop.Excel.Workbook xlworkbook;
            Microsoft.Office.Interop.Excel.Worksheet xlWorksheet;
            Microsoft.Office.Interop.Excel.Range xlrange;
            int xlrow;
            DataTable Unregistered = new DataTable();
            Unregistered.Columns.Add("");
            OpenFileDialog open = new OpenFileDialog();
            int errorcount = 0;
            open.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            open.ShowDialog();
            string filename = open.FileName;
            if (filename != "")
            {
                xlapp = new Microsoft.Office.Interop.Excel.Application();
                xlworkbook = xlapp.Workbooks.Open(filename);
                xlWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)xlworkbook.Sheets["Students"];
                xlrange = xlWorksheet.UsedRange;
                //Levels
                xlapp = new Microsoft.Office.Interop.Excel.Application();
                xlworkbook = xlapp.Workbooks.Open(filename);
                xlWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)xlworkbook.Sheets["Students"];
                xlrange = xlWorksheet.UsedRange;
                //Levels
                int PremID = Connexion.GetInt("Premaire", "Levels", "ID", "Level");
                int CemID = Connexion.GetInt("Cem", "Levels", "ID", "Level");
                int LyceeID = Connexion.GetInt("Lycée", "Levels", "ID", "Level");
                //Premaire
                int AP1ID = Connexion.GetInt("1AP", "Years", "ID", "Year");
                int AP2ID = Connexion.GetInt("2AP", "Years", "ID", "Year");
                int AP3ID = Connexion.GetInt("3AP", "Years", "ID", "Year");
                int AP4ID = Connexion.GetInt("4AP", "Years", "ID", "Year");
                int AP5ID = Connexion.GetInt("5AP", "Years", "ID", "Year");
                //Cem
                int AM1ID = Connexion.GetInt("1AM", "Years", "ID", "Year");
                int AM2ID = Connexion.GetInt("2AM", "Years", "ID", "Year");
                int AM3ID = Connexion.GetInt("3AM", "Years", "ID", "Year");
                int AM4ID = Connexion.GetInt("4AM", "Years", "ID", "Year");
                //1AS Speciality
                int AS1ID = Connexion.GetInt("1AS", "Years", "ID", "Year");
                int Spec1Sc = Connexion.GetInt("Scientifique", "Specialities", "ID", "Speciality", "YearID", AS1ID.ToString());
                int Spec1Let = Connexion.GetInt("Lettres", "Specialities", "ID", "Speciality", "YearID", AS1ID.ToString());
                //2AS Speciality
                int AS2ID = Connexion.GetInt("2AS", "Years", "ID", "Year");
                int Spec2Sc = Connexion.GetInt("Scientifique", "Specialities", "ID", "Speciality", "YearID", AS2ID.ToString());
                int Spec2M = Connexion.GetInt("mathéleme", "Specialities", "ID", "Speciality", "YearID", AS2ID.ToString());
                int Spec2MT = Connexion.GetInt("Math-Technique", "Specialities", "ID", "Speciality", "YearID", AS2ID.ToString());
                int Spec2GE = Connexion.GetInt("Gestion", "Specialities", "ID", "Speciality", "YearID", AS2ID.ToString());
                int Spec2Langue = Connexion.GetInt("Langue", "Specialities", "ID", "Speciality", "YearID", AS2ID.ToString());
                int Spec2Philo = Connexion.GetInt("Philo", "Specialities", "ID", "Speciality", "YearID", AS2ID.ToString());
                //3AS Speciality
                int AS3ID = Connexion.GetInt("3AS", "Years", "ID", "Year");
                int Spec3Sc = Connexion.GetInt("Scientifique", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                int Spec3M = Connexion.GetInt("mathéleme", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                int Spec3Philo = Connexion.GetInt("Philo", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                int Spec3Langue = Connexion.GetInt("Langue", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                int Spec3MT = Connexion.GetInt("Math-Technique", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                int Spec3GE = Connexion.GetInt("Gestion", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                int spec3Philo = Connexion.GetInt("philosophie", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                for (xlrow = 2; xlrow < 1000; xlrow++)
                {
                    string Index = "";

                    int YearID = 0;
                    int SpecID = 0;
                    int levelID = 0;
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 1].Value) != null)
                    {
                        Index = Convert.ToString(xlWorksheet.Cells[xlrow, 1].Value).Replace("'", "''");
                    }
                    string firstname = "";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 2].Value) != null)
                    {
                        firstname = Convert.ToString(xlWorksheet.Cells[xlrow, 2].Value).Replace("'", "''");
                    }
                    string lastname = "";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 3].Value) != null)
                    {
                        lastname = Convert.ToString(xlWorksheet.Cells[xlrow, 3].Value).Replace("'", "''");
                    }
                    string Number = "";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 4].Value) != null)
                    {
                        Number = Convert.ToString(xlWorksheet.Cells[xlrow, 4].Value).Replace("'", "''");
                    }
                    string PNumber = "";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 5].Value) != null)
                    {
                        PNumber = Convert.ToString(xlWorksheet.Cells[xlrow, 5].Value).Replace("'", "''");
                    }
                    string adress = "";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 6].Value) != null)
                    {
                        adress = Convert.ToString(xlWorksheet.Cells[xlrow, 6].Value).Replace("'", "''");
                    }
                    string School = "";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 7].Value) != null)
                    {
                        School = Convert.ToString(xlWorksheet.Cells[xlrow, 7].Value).Replace("'", "''");
                    }
                    string Gender = "";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 8].Value) != null)
                    {
                        Gender = Convert.ToString(xlWorksheet.Cells[xlrow, 8].Value).Replace("'", "''");
                    }

                    string Register = "";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 9].Value) != null)
                    {
                        Register = Convert.ToString(xlWorksheet.Cells[xlrow, 9].Value).Replace("'", "''");
                    }

                    // string Gender = Convert.ToString(xlWorksheet.Cells[xlrow, 10].Value);

                    string Bday = "";
                    string note = "";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 10].Value) != null)
                    {
                        note = Convert.ToString(xlWorksheet.Cells[xlrow, 10].Value).Replace("'", "''");
                    }

                    // 8   9 year 10 Speciality  11 Subjects 
                    string Speciality = "";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 11].Value) != null)
                    {
                        Speciality = Convert.ToString(xlWorksheet.Cells[xlrow, 11].Value).Replace("'", "''");
                    }
                    string Year = "";
                    // 9 is classes
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 12].Value) != null)
                    {
                        Year = Convert.ToString(xlWorksheet.Cells[xlrow, 12].Value).Replace("'", "''");
                    }

                    string InscFees = "0";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 13].Value) != null)
                    {
                        if (Convert.ToString(xlWorksheet.Cells[xlrow, 13].Value) == "P")
                        {
                            InscFees = "0";
                        }
                        else if (Convert.ToString(xlWorksheet.Cells[xlrow, 13].Value) == "NP")
                        {
                            InscFees = "2";
                        }
                        else if (Convert.ToString(xlWorksheet.Cells[xlrow, 13].Value) == "DP")
                        {
                            InscFees = "3";
                        }
                        else if (Convert.ToString(xlWorksheet.Cells[xlrow, 13].Value) == "RP")
                        {
                            InscFees = "4";
                        }
                    }

                    if (Year == "1AP")
                    {
                        YearID = AP1ID;
                        levelID = PremID;
                    }
                    else if (Year == "2AP")
                    {
                        YearID = AP2ID;
                        levelID = PremID;
                    }
                    else if (Year == "3AP")
                    {
                        YearID = AP3ID;
                        levelID = PremID;
                    }
                    else if (Year == "4AP")
                    {
                        YearID = AP4ID;
                        levelID = PremID;
                    }
                    else if (Year == "5AP")
                    {
                        YearID = AP5ID;
                        levelID = PremID;
                    }
                    else if (Year == "1AM")
                    {
                        YearID = AM1ID;
                        levelID = CemID;
                    }
                    else if (Year == "2AM")
                    {
                        YearID = AM2ID;
                        levelID = CemID;
                    }
                    else if (Year == "3AM")
                    {
                        YearID = AM3ID;
                        levelID = CemID;
                    }
                    else if (Year == "4AM")
                    {
                        YearID = AM4ID;
                        levelID = CemID;
                    }
                    else if (Year == "1AS")//
                    {
                        if (Speciality == "Scientifique")
                        {
                            SpecID = Spec1Sc;
                        }
                        else if (Speciality == "Langue")
                        {
                            SpecID = Spec1Let;
                        }
                        YearID = AS1ID;
                        levelID = LyceeID;
                    }
                    else if (Year == "2AS")
                    {
                        YearID = AS2ID;
                        if (Speciality == "Scientifique")
                        {
                            SpecID = Spec2Sc;
                        }
                        else if (Speciality == "Langue")
                        {
                            SpecID = Spec2Langue;
                        }
                        else if (Speciality == "mathéleme")
                        {
                            SpecID = Spec2M;
                        }
                        else if (Speciality == "Math-Technique")
                        {
                            SpecID = Spec2MT;
                        }
                        else if (Speciality == "Gestion")
                        {
                            SpecID = Spec2GE;
                        }
                        else if (Speciality == "Philo")
                        {
                            SpecID = Spec2Philo;
                        }
                        levelID = LyceeID;
                    }
                    else if (Year == "3AS")
                    {
                        YearID = AS3ID;
                        if (Speciality == "Scientifique")
                        {
                            SpecID = Spec3Sc;
                        }
                        else if (Speciality == "Langue")
                        {
                            SpecID = Spec3Langue;
                        }
                        else if (Speciality == "mathéleme")
                        {
                            SpecID = Spec3M;
                        }
                        else if (Speciality == "Math-Technique")
                        {
                            SpecID = Spec3MT;
                        }
                        else if (Speciality == "Gestion")
                        {
                            SpecID = Spec3GE;
                        }
                        else if (Speciality == "Philo")
                        {
                            SpecID = Spec3Philo;
                        }
                        levelID = LyceeID;
                    }
                    int SID = -1;
                    if (YearID != 0)
                    {
                        if (SpecID == 0)
                        {
                            SID = Connexion.GetInt("Insert into Students" +
                                "(FirstName, " +
                                "LastName  ,  " +
                               "BirthDate , " +
                               "Adress, " +
                               "ParentNumber , " +
                               "PhoneNumber, " +
                               "note, " +
                               "Year, " +
                               "School ," +
                               "Register ," +
                               "PayedInsc ," +
                               "Gender," +
                               "Level,status,BarCode )  OUTPUT Inserted.ID  values(" +
                               "N'" + firstname + "'," +
                               "N'" + lastname + "' ," +
                               "N'" + Bday + "'," +
                               "N'" + adress + "'," +
                               "N'" + PNumber + "'," +
                               "N'" + Number + "'," +
                               "N'" + note + "'," +
                               "N'" + YearID + "'," +
                               "N'" + School + "'," +
                               "N'" + Register + "'," +
                               "N'" + InscFees + "'," +
                               "N'" + Gender + "'," +
                               "N'" + levelID + "' , 1,N'" + Index + "')");
                        }
                        else
                        {
                            SID = Connexion.GetInt("Insert into Students" +
                               "(FirstName," +
                               "lastname , " +
                              "BirthDate , " +
                              "Adress, " +
                              "ParentNumber , " +
                              "PhoneNumber, " +
                              "note, " +
                              "Year, " +
                              "School ," +
                              "Register ," +
                              "PayedInsc ," +
                              "Gender , " +
                              "Speciality," +
                              "Level,status,BarCode  )  OUTPUT Inserted.ID  values(" +
                              "N'" + firstname + "'," +
                              "N'" + lastname + "'," +
                              "N'" + Bday + "'," +
                              "N'" + adress + "'," +
                              "N'" + PNumber + "'," +
                              "N'" + Number + "'," +
                              "N'" + note + "'," +
                              "N'" + YearID + "'," +
                              "N'" + School + "'," +
                              "N'" + Register + "'," +
                              "N'" + InscFees + "'," +
                              "N'" + Gender + "'," +
                              "N'" + SpecID + "'," +
                              "N'" + levelID + "' , 1 ,N'" + Index + "$')");
                        }

                        errorcount = 0;
                    }
                    else
                    {
                        errorcount++;
                        Unregistered.Rows.Add(xlrow);
                        if (errorcount == 10)
                        {
                            break;
                        }

                    }
                }
                try
                {
                    using (StreamWriter sw = new StreamWriter(@"C:\ProgramData\EcoleSetting\ExcelNotEnter.txt", false, Encoding.UTF8))
                    {
                        // Loop through each row in the DataTable
                        foreach (DataRow row in Unregistered.Rows)
                        {
                            // Customize this part based on the structure of your DataTable
                            // You may need to access specific columns
                            string dataToSave = string.Join(", ", row.ItemArray);

                            // Write the data to the file
                            sw.WriteLine(dataToSave);
                        }
                    }

                    MessageBox.Show("Data saved to Notepad successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving data: {ex.Message}");
                }
            }
        }

        private void ImpClass_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application xlapp;
            Microsoft.Office.Interop.Excel.Workbook xlworkbook;
            Microsoft.Office.Interop.Excel.Worksheet xlWorksheet;
            Microsoft.Office.Interop.Excel.Range xlrange;
            int xlrow;
            OpenFileDialog open = new OpenFileDialog();

            open.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            open.ShowDialog();
            string filename = open.FileName;
            if (filename != "")
            {
                xlapp = new Microsoft.Office.Interop.Excel.Application();
                xlworkbook = xlapp.Workbooks.Open(filename);
                xlWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)xlworkbook.Sheets["Students"];
                xlrange = xlWorksheet.UsedRange;
                //Levels

                for (xlrow = 2; xlrow < 1000; xlrow++)
                {
                    string GID = "";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 1].Value) != null)
                    {
                        if (Connexion.IFNULL("Select * from Groups Where GroupID =" + GID))
                        {
                            MessageBox.Show("Group not found  = " + GID);
                            continue;
                        }
                        GID = Convert.ToString(xlWorksheet.Cells[xlrow, 1].Value);
                     //   Connexion.GetInt("Select ID from Class_Time where GID = " + GID)
                   //     Connexion.InsertAttendance("Select")
                    }
                }
            }
        }

        private void ImpAttendance_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application xlapp;
            Microsoft.Office.Interop.Excel.Workbook xlworkbook;
            Microsoft.Office.Interop.Excel.Worksheet xlWorksheet;
            Microsoft.Office.Interop.Excel.Range xlrange;
            int xlrow;
            OpenFileDialog open = new OpenFileDialog();

            open.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            open.ShowDialog();
            string filename = open.FileName;
            if (filename != "")
            {
                xlapp = new Microsoft.Office.Interop.Excel.Application();
                xlworkbook = xlapp.Workbooks.Open(filename);
                xlWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)xlworkbook.Sheets["Class_Students"];
                xlrange = xlWorksheet.UsedRange;
                //Levels

                for (xlrow = 2; xlrow < 1000; xlrow++)
                {
                    string ID = "";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 1].Value) != null)
                    {
                        ID = Convert.ToString(xlWorksheet.Cells[xlrow, 1].Value).Replace("'", "''");
                    }
                    string GID = "";
                    string Date = "";
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 4].Value) != null)
                    {
                        GID = Convert.ToString(xlWorksheet.Cells[xlrow, 4].Value).Replace("'", "''");
                        Date = Convert.ToString(xlWorksheet.Cells[xlrow, 5].Value).Replace("/", "-");
                        int Session = Connexion.GetInt("SELECT TOP 1 Session FROM Attendance WHERE date <= '" + Date + "' and  GroupID = " + GID + "  ORDER BY  date DESC; ") - 1;
                        if (Connexion.IFNULL("Select * from Class_Student Where StudentID = " + ID + " and groupID = " + GID))
                        {
                            Connexion.Insert("Insert into Class_Student Values ('" + ID + "','" + Connexion.GetClassID(GID) + "' ,  '" + GID + "'," + Session + ",NULL ,0,0)");
                            for (int i = Session; i < Session; i++)
                            {
                                int ses = i + 1;
                                int aid = Connexion.GetInt("Select ID from Attendance Where Session = " + ses + " and GroupID = " +GID);
                                Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status,Note) values(" + aid + "," + ID + ",1,null)");
                                Commun.SetStatusAttendance(ID, aid.ToString(), 1);

                            }
                        }
                        else
                        {
                            for (int i = 0; i < Session; i++)
                            {
                                int ses = i + 1;
                                int aid = Connexion.GetInt("Select ID from Attendance Where Session = " + ses + " and GroupID = " + GID);
                                Connexion.Insert("Delete From Attendance_Student where StudentID = " + ID + " and ID = " + aid);

                            }
                            Connexion.Insert("Update Class_Student Set Session = " + Session + " Where StudentID = " + ID + " and GroupID = " + GID);
                        }
                    }
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 6].Value) != null)
                    {
                        GID = Convert.ToString(xlWorksheet.Cells[xlrow, 6].Value).Replace("'", "''");
                        Date = Convert.ToString(xlWorksheet.Cells[xlrow, 7].Value).Replace("/", "-");
                        int Session = Connexion.GetInt("SELECT TOP 1 Session FROM Attendance WHERE CONVERT(DATE, date, 105) <= '" + Date + "' and  GroupID = " + GID + "  ORDER BY CONVERT(DATE, date, 105) DESC; ") - 1;
                        if (Connexion.IFNULL("Select * from Class_Student Where StudentID = " + ID + " and groupID = " + GID))
                        {
                            Connexion.Insert("Insert into Class_Student Values Values ('" + ID + "','" + Connexion.GetClassID(GID) + "' ,  '" + GID + "'," + Session + ",NULL ,0,0)");
                            for (int i = Session; i < Session; i++)
                            {
                                int ses = i + 1;
                                int aid = Connexion.GetInt("Select ID from Attendance Where Session = " + ses + " and GroupID = " + GID);
                                Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status,Note) values(" + aid + "," + ID + ",1,null)");
                                Commun.SetStatusAttendance(ID, aid.ToString(), 1);

                            }
                        }
                        else
                        {
                            for (int i = 0; i < Session; i++)
                            {
                                int ses = i + 1;
                                int aid = Connexion.GetInt("Select ID from Attendance Where Session = " + ses + " and GroupID = " + GID);
                                Connexion.Insert("Delete From Attendance_Student where StudentID = " + ID + " and ID = " + aid);

                            }
                            Connexion.Insert("Update Class_Student Set Session = " + Session + " Where StudentID = " + ID + " and GroupID = " + GID);
                        }
                    }
                    if (Convert.ToString(xlWorksheet.Cells[xlrow, 7].Value) != null)
                    {
                        GID = Convert.ToString(xlWorksheet.Cells[xlrow, 7].Value).Replace("'", "''");
                        Date = Convert.ToString(xlWorksheet.Cells[xlrow, 8].Value).Replace("/", "-");
                        int Session = Connexion.GetInt("SELECT TOP 1 Session FROM Attendance WHERE CONVERT(DATE, date, 105) <= '" + Date + "' and  GroupID = " + GID + "  ORDER BY CONVERT(DATE, date, 105) DESC; ") - 1;
                        if (Connexion.IFNULL("Select * from Class_Student Where StudentID = " + ID + " and groupID = " + GID))
                        {
                            Connexion.Insert("Insert into Class_Student Values Values ('" + ID + "','" + Connexion.GetClassID(GID) + "' ,  '" + GID + "'," + Session + ",NULL,0,0)");
                            for (int i = Session; i < Session; i++)
                            {
                                int ses = i + 1;
                                int aid = Connexion.GetInt("Select ID from Attendance Where Session = " + ses + " and GroupID = " + GID);
                                Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status,Note) values(" + aid + "," + ID + ",1,null)");
                                Commun.SetStatusAttendance(ID, aid.ToString(), 1);

                            }
                        }
                        else
                        {
                            for (int i = 0; i < Session; i++)
                            {
                                int ses = i + 1;
                                int aid = Connexion.GetInt("Select ID from Attendance Where Session = " + ses + " and GroupID = " + GID);
                                Connexion.Insert("Delete From Attendance_Student where StudentID = " + ID + " and ID = " + aid);

                            }
                            Connexion.Insert("Update Class_Student Set Session = " + Session + " Where StudentID = " + ID + " and GroupID = " + GID);
                        }
                    }



                }
            }
        }

        private void ImpPayment_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
