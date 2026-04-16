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
using Gestion_De_Cours.Panels;
using Gestion_De_Cours.UControl;
using Gestion_De_Cours.Classes;
using System.Data;
using System.Windows.Controls.Primitives;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Gestion_De_Cours.Properties;
using System.IO;
using Microsoft.Win32;
using FastReport;
using OfficeOpenXml;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Gestion_De_Cours.UControl
{
    /// <summary>
    /// Interaction logic for ShowTableU.xaml
    /// </summary>
    public partial class ShowTableU : UserControl
    {
        string TypeUC;
        DataTable dtMain = new DataTable();
        DataTable dtSubjects = new DataTable();
        string query = "";
        OpenFileDialog open = new OpenFileDialog();
        DataTable dtStudentGroup = new DataTable();
        DataTable dtStudentclass = new DataTable();
        DataSet DSStudent = new DataSet();


        public ShowTableU(string Type , string condition = "")
        {
            try
            {
                SqlConnection con = Connexion.Connect();
                TypeUC = Type;
                int lang = Connexion.Language();
                InitializeComponent();
                CBGender.SelectedIndex = 0 ;
                DGUC.Height = Commun.ScreenHeight - 225; 
                SetLang();
                if (lang == 1)
                {
                    this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
                }
                DataTable dtLevels = new DataTable();

                Connexion.FillDT(ref dtLevels, "Select * from levels");

                DataRow newRow = dtLevels.NewRow();
                newRow["ID"] = 0;
                newRow["Level"] = this.Resources["Levels--"].ToString();
                dtLevels.Rows.InsertAt(newRow, 0);
                CBLevel.ItemsSource = dtLevels.DefaultView;
                CBLevel.SelectedIndex = 0;
                DataTable dtyears = new DataTable();
                dtyears.Columns.Add("ID", typeof(int));
                dtyears.Columns.Add("Year", typeof(string));
                dtyears.Rows.Add(0, this.Resources["Year--"]);
                CBYear.ItemsSource = dtyears.DefaultView;
                CBYear.SelectedIndex = 0;
                DataTable dtspec = new DataTable();
                dtspec.Columns.Add("ID", typeof(int));
                dtspec.Columns.Add("Speciality", typeof(string));
                dtspec.Rows.Add(0, this.Resources["Speciality--"]);
                CBSpec.ItemsSource = dtspec.DefaultView;
                CBSpec.SelectedIndex = 0;
                if (TypeUC == "Student")
                {
                  
                    SPStudentTotal.Visibility = Visibility.Visible;
                    row1.Height = new GridLength(25);

                    dtStudentclass.TableName = "Class";
                    Connexion.FillCB(ref CBStudentLevel, "Select * from Levels");

                    Connexion.FillDT(ref dtSubjects, "WITH CTE AS ( SELECT RTRIM(LOWER(Subject)) AS Text, ID, 0 IsChecked, ROW_NUMBER() OVER(PARTITION BY RTRIM(LOWER(Subject)) ORDER BY ID) AS RowNum FROM Subjects ) SELECT Text, ID, IsChecked FROM CTE WHERE RowNum = 1; ");
                    DataRow newRowSubject = dtSubjects.NewRow();
                    newRowSubject["Text"] = "Select All";
                    newRowSubject["IsChecked"] = 0; // 1 for true, 0 for false, adjust as needed
                    newRowSubject["ID"] = 0;
                    // Insert the new row at the beginning of the DataTable
                    dtSubjects.Rows.InsertAt(newRowSubject, 0);
                    CustomComboBox.ItemsSource = dtSubjects.DefaultView;
                    Methods.AddcolumnLV(ref DGUC, "ID", "Sort", "Student");
                    Methods.AddcolumnLV(ref DGUC, "BarCode", "BarCode", "Student");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["SName"].ToString(), "Name", "Student");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["PhoneNumber"].ToString(), "PhoneNumber", "Student");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["ParentNumber"].ToString(), "ParentNumber", "Student");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["inscFees"].ToString(), "Insc", "Student");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Gender"].ToString(), "Genderr", "Student");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["BirthDate"].ToString(), "Birthdate", "Student");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Level"].ToString(), "Level", "Student");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Year"].ToString(), "Year", "Student");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Speciality"].ToString(), "Spec", "Student");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Subject"].ToString(), "Subjects", "Student");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Adress"].ToString(), "Adress", "Student");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["DateOfRegister"].ToString(), "Register", "Student");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Note"].ToString(), "Note", "Student");
                    ContextMenu contextMenu = new ContextMenu();
                    foreach (GridViewColumn column in (((GridView)DGUC.View).Columns))
                    {
                        string bindingPath = "";
                        if (column.DisplayMemberBinding is Binding binding && binding.Path != null)
                        {
                            bindingPath = binding.Path.Path;
                        }
                        if (Methods.CheckConditionColumn("Student" , bindingPath) )
                        {
                            MenuItem menuItem = new MenuItem
                            {
                                Header = column.Header,
                                IsCheckable = true,
                                IsChecked = Methods.CheckIfVisible("Student", bindingPath)
                            };

                            // Add a Click event handler to toggle column visibility
                            menuItem.Click += (sender, e) =>
                            {
                               
                                if (menuItem.IsChecked)
                                {
                                    //column.Visibility = Visibility.Visible;
                                    Methods.SetVisibileColumn(bindingPath, "Student", 1);
                                    
                                }
                                else
                                {
                                    //column.Visibility = Visibility.Collapsed;
                                    Methods.SetVisibileColumn(bindingPath, "Student", 0);
                                }
                            };
                            contextMenu.Items.Add(menuItem);
                        }
                    }
                    // Attach the context menu to the DataGrid
                    DGUC.ContextMenu = contextMenu;
                    query = "Select row_number() OVER (ORDER BY Students.ID) Sort ,dbo.GetStudentSubjects(Students.ID) as Subjects,case when Students.LastName is NULL then students.FirstName " +
                        "When Students.Firstname is NULL then Students.LastName " +
                        "when Students.Firstname is not Null and Students.LastName is not null then Students.LastName + ' ' + Students.FirstName end as Name ,Students.Adress,Students.Level as LevelID ,dbo.ReplaceArabicName(Students.FirstName) + ' ' + dbo.ReplaceArabicName(Students.LastName) as NameSearch ,dbo.ReplaceArabicName(Students.LastName) + ' ' + dbo.ReplaceArabicName(Students.FirstName) as RNameSearch ,  Students.FirstName as FirstName,Students.LastName as LastName , Students.BarCode as BarCode , " +
                        "Case When Students.PayedInsc = 0 then N'" + this.Resources["Paid"].ToString() + "'  " +
                        "when Students.PayedInsc = 1 then N'" + this.Resources["Didntpay"].ToString() + "' " +
                        "when Students.PayedInsc = 2 then N'" + this.Resources["DoesntPay"].ToString() + "' " +
                        "When Students.PayedInsc = 3 then N'" + this.Resources["Refund"].ToString() + "'  end as Insc,students.PayedInsc as PayedInsc,Levels.Level as Level,  Students.PhoneNumber , Students.ParentNumber ,Students.ID as ID, Students.School , Students.Note , Students.Register , Students.Birthdate,Years.ID as YearID, Students.Gender as Gender,Students.FirstName + ' ' + Students.LastName as RName , Years.Year as Year ,Case When Students.Gender = 1 Then 'Female' When Students.Gender = 0  Then 'Male' END  as Genderr , Specialities.Speciality as Spec  ,Students.Speciality as SpecID from Students join Levels on Levels.ID = Students.Level  Join Years On Students.Year = Years.ID  Left Join Specialities On Students.Speciality = Specialities.ID Where Status = 1 order by Students.ID desc";
                    DGUC.Visibility = Visibility.Visible;
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "StudentV") != 1)
                    {
                        DGUC.IsEnabled = false;
                    }
                }
                else if (TypeUC == "Teacher")
                {
                    DGUC.Height = Commun.ScreenHeight - 210;
                    Methods.AddcolumnLV(ref DGUC, this.Resources["TName"].ToString(), "Name");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["PhoneNumber"].ToString(), "TPhoneNumber");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Gender"].ToString(), "Genderr");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["BirthDate"].ToString(), "TBirthDate");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Adress"].ToString(), "TAdress");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Email"].ToString(), "Email");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["CCP"].ToString(), "TCCP");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["DateOfRegister"].ToString(), "TRegister");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Note"].ToString(), "TNote");
                    query = "Select TBirthDate,TAdress,Email,TCCP,TRegister,ID,TNote,TPhoneNumber,TLastName + ' ' + TFirstName as Name ,TFirstName + ' ' + TLastName as RName ,Tgender as Gender, Case When TGender  = 1 Then N'"+ this.Resources["Female"].ToString() +"' When TGender = 0 Then N'"+ this.Resources["Male"].ToString() + "' End as Genderr  from Teacher Where Status = 1";
                    Methods.ChangeGridHeight(ref row2, "40");
                    CBLevel.Visibility = Visibility.Collapsed;
                    CBYear.Visibility = Visibility.Collapsed;
                    CBSpec.Visibility = Visibility.Collapsed;
                    CBClass.Visibility = Visibility.Collapsed;

                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "TPayV") == 1)
                    {
                        // Add this code after initializing DGUC and its context menu, inside the constructor (after DGUC is set up):

                        ContextMenu rowContextMenu = new ContextMenu();

                        // Create the Payment menu item with icon
                        MenuItem paymentMenuItem = new MenuItem
                        {
                            Header = "Payment",
                            Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/Payment512.png")),
                                Width = 16,
                                Height = 16
                            }
                        };

                        // Handle click event for Payment
                        paymentMenuItem.Click += BtnPay_Click;

                        rowContextMenu.Items.Add(paymentMenuItem);

                        // Assign the context menu to DGUC rows
                        DGUC.ContextMenu = rowContextMenu;

                        // Optionally, you can show the context menu only when a row is selected:
                        DGUC.MouseRightButtonUp += (s, e) =>
                        {
                            if (DGUC.SelectedItem != null)
                            {
                                DGUC.ContextMenu.IsOpen = true;
                            }
                        };
                    }
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "TeacherV") != 1)
                    {
                        DGUC.IsEnabled = false;
                    }
                }
                else if (TypeUC == "Class")
                {
                    BtnIMG1.Visibility = Visibility.Collapsed;
                    BtnIMG2.Visibility = Visibility.Collapsed;
                    DGUC.Height = Commun.ScreenHeight - 210;
                    CBSpec.Visibility = Visibility.Collapsed;
                    SPG1.Visibility = Visibility.Visible;
                 
                    Methods.AddcolumnDGSetterBackGround(ref DGUC, this.Resources["ClassName"].ToString(), "CName" , "EmptyAttend" , "1" , Brushes.Red) ;
                    Methods.AddcolumnLV(ref DGUC, this.Resources["TName"].ToString(), "TName");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Year"].ToString(), "Year");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Subject"].ToString(), "Subject");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Price"].ToString(), "CPrice");
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "TPayV") == 1)
                    {
                        Methods.AddcolumnLV(ref DGUC, this.Resources["TPaymentMethod"].ToString(), "TPaymentMethod");
                        Methods.AddcolumnLV(ref DGUC, this.Resources["TPayment"].ToString(), "TPayment");
                    }
                    query = "SELECT Class.ID,dbo.CheckEmptyAttendance(Class.ID) as EmptyAttend,  Class.CName,Class.MultipleGroups as MultipleGroups,Teacher.TGender , Class.CSubject, Years.Year,Subjects.Subject,CLevel as LevelID , CYear as YearID , Class.CPrice,Teacher.TGender as Gender, case When  Class.TPaymentMethod = 1 then N'" + this.Resources["PerStudent"].ToString() + "' When Class.TPaymentMethod = 2 then N'" + this.Resources["PerPeriod"] + "' End as TPaymentMethod, Class.TID, Class.TPayment,  Teacher.TFirstName + ' ' + Teacher.TLastName as TName , Teacher.TLastName + ' ' + Teacher.TFirstName as RTName FROM Class JOIN Teacher ON Class.TID = Teacher.ID Join Years on Years.ID = Class.Cyear Join Subjects On Subjects.ID = Class.CSubject ";
                  
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "ClassV") != 1)
                    {
                        DGUC.IsEnabled = false;
                    }
                }
                else if (TypeUC == "Worker")
                {
                    Methods.AddcolumnLV(ref DGUC, this.Resources["WName"].ToString(), "Name");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["PhoneNumber"].ToString(), "Number");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Gender"].ToString(), "Genderr");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["BirthDate"].ToString(), "BirthDate");
                    //Methods.AddcolumnDG(ref DGUC, "Salary", "Salary");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Adress"].ToString(), "Adress");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["DateOfRegister"].ToString(), "Date_of_register");
                    query = "Select *, FirstName + ' ' + LastName as Name , LastName + ' ' + FirstName as RName , Case When Gender  = 1 Then N'" + this.Resources["Female"].ToString() + "' When Gender = 0 Then N'" + this.Resources["Male"].ToString() + "' End as Genderr  from Workers Where Status = 1 ";
                    Methods.ChangeGridHeight(ref row2, "40");
                    Methods.ChangeGridHeight(ref row3, "0");
                    DGUC.MinHeight = 600;
                   
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "WorkerV") != 1)
                    {
                        DGUC.IsEnabled = false;
                    }
                }
                else if (TypeUC == "Group")
                {
                    BtnIMG1.Visibility = Visibility.Collapsed;
                    BtnIMG2.Visibility = Visibility.Collapsed;

                    DGUC.Height = Commun.ScreenHeight - 210;
                    CBSpec.Visibility = Visibility.Collapsed;
                    Methods.AddcolumnDGSetterBackGround(ref DGUC, this.Resources["GroupName"].ToString(), "GroupName", "EmptyAttend", "1", Brushes.Red);
                    Methods.AddcolumnLV(ref DGUC, this.Resources["TName"].ToString(), "TName");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Level"].ToString(), "Level");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Year"].ToString(), "Year");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Subject"].ToString(), "Subject");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["GenderGroup"].ToString(), "GroupGender");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["StudentAmount"].ToString(), "SAmmount");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["StudiedSes"].ToString(), "Sessions");
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "TPayV") == 1)
                    {
                        Methods.AddcolumnLV(ref DGUC, this.Resources["UnpaidSes"].ToString(), "TSessions");
                    }
                   
                  //  DGUC.Columns[1].DisplayIndex = DGUC.Columns.Count - 1;
                  //  DGUC.Columns[2].DisplayIndex = DGUC.Columns.Count - 1;
                   // DGUC.Columns[3].DisplayIndex = DGUC.Columns.Count - 1;
                    SPG1.Visibility = Visibility.Visible;
                   
                    query = "Select " +
                        "Case When Class.MultipleGroups = 'Single' Then" +
                        " Class.CName " +
                        "When Class.MultipleGroups = 'Multiple' Then Class.CName + ' ' + Groups.GroupName " +
                        "End as GroupName, " +
                        "Case when Groups.TSessions = '0' then  0 end  as TSessions, " +
                        "Class.CSubject,dbo.CheckEmptyAttendanceGroup(Groups.GroupID) as EmptyAttend , " +
                        "Groups.GroupGender as Gender," +
                        "Class.MultipleGroups," +
                        "Class.CYear," +
                        "Class.TID as TID ," +
                        "Teacher.TGender," +
                        "Groups.GroupID as GID," +
                        "Class.ID as ID ," +
                        "Teacher.TFirstName + ' ' + Teacher.TLastName as TName ," +
                        "Years.ID as YearID , " +
                        "Teacher.TLastName + ' ' + Teacher.TFirstName as RTName ," +
                        "Case When Groups.GroupGender = 0 then 'Male' " +
                        "When Groups.GroupGender = 1 then 'Female' " +
                        "When Groups.GroupGender = 2 then 'Mix' end as GroupGender ," +
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
                        "Join Levels On Levels.ID = Class.CLevel ";
              
                    CBGender.Items.Add("Mix");
                    /*if (Connexion.GetInt(Connexion.WorkerID, "Users", "GroupV") != 1)
                    {
                        DGUC.IsEnabled = false;
                    }*/

                }
                else if (TypeUC == "SPayment")
                {
                    Methods.AddcolumnLV(ref DGUC, this.Resources["SName"].ToString(), "Name");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["PhoneNumber"].ToString(), "Number");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["ParentNumber"].ToString(), "PNumber");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Price"].ToString(), "Price");
                    if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 1)
                    {
                        query =
                         "Select " +
                         "Students.FirstName + ' ' + Students.LastName as Name ," +
                         "Students.LastName + ' ' + Students.FirstName as RName , " +
                         "Students.PhoneNumber as Number , " +
                         "Students.ParentNumber as PNumber ," +
                         "Students.ID as ID , " +
                         "dbo.ReplaceArabicName(Students.FirstName) + ' ' + dbo.ReplaceArabicName(Students.LastName) as NameSearch ,dbo.ReplaceArabicName(Students.LastName) + ' ' + dbo.ReplaceArabicName(Students.FirstName) as RNameSearch ," +
                         " dbo.SumMonthlyDebt(Students.ID) as Price " +
                         "from Students " ;
                    }
                    else
                    {
                        Methods.AddcolumnLV(ref DGUC, this.Resources["ClassName"].ToString(), "CName");
                       
                        Methods.AddcolumnLV(ref DGUC, this.Resources["Sessions"].ToString(), "Sessions");

                        query =
                         "Select " +
                         "Students.FirstName + ' ' + Students.LastName as Name ," +
                         "Students.LastName + ' ' + Students.FirstName as RName , " +
                         "Students.PhoneNumber as Number , " +
                         "Students.ParentNumber as PNumber ," +
                         "Class.CSubject , " +
                         "Students.ID as ID , " +
                         "dbo.ReplaceArabicName(Students.FirstName) + ' ' + dbo.ReplaceArabicName(Students.LastName) as NameSearch ,dbo.ReplaceArabicName(Students.LastName) + ' ' + dbo.ReplaceArabicName(Students.FirstName) as RNameSearch ," +
                         "Class.CName as CName , " +
                         "Class.ID as CID  , " +
                         "dbo.CalculatePrice(Students.ID ,Groups.GroupID, Groups.TSessions, 'Su') - dbo.CalculatePrice(Students.ID ,Groups.GroupID,  Groups.TSessions, 'S') as Price  ," +
                         "  Class_Student.Session + dbo.CalculateSesPayed(Students.ID, Groups.GroupID) - Groups.Sessions as Sessions " +
                         "from Students " +
                         "join Class_Student on Students.ID = Class_Student.StudentID  " +
                         "join Groups on groups.groupID = Class_Student.GroupID " +
                         "join class on class.ID = Groups.CLassID  ";
                        CBClass.Visibility = Visibility.Visible;
                   
                        Connexion.FillCB(ref CBClass, "Select * from Class");
                    }
                    CustomComboBox.Visibility = Visibility.Collapsed;
                    Date.Visibility = Visibility.Collapsed;
                    LBdate.Visibility = Visibility.Collapsed;
                    CBGender.Visibility = Visibility.Collapsed;
               
                 
                    CBSpec.Visibility = Visibility.Collapsed;
                   
                    DGUC.ItemsSource = dtMain.DefaultView;
                    CBLevel.Visibility = Visibility.Collapsed;
                    CBYear.Visibility = Visibility.Collapsed;
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "SPayV") != 1)
                    {
                        DGUC.IsEnabled = false;
                    }
                }
                else if ( TypeUC == "History")
                {
                    LBdate.Visibility = Visibility.Visible;
                    Date.Visibility = Visibility.Visible;
                    LBTypeAction.Visibility = Visibility.Visible; 
                    LBType.Visibility = Visibility.Visible;
                    CBTypeAction.Visibility = Visibility.Visible;
                    CBType.Visibility = Visibility.Visible;
                    SPImage.Visibility = Visibility.Collapsed;
                    TBSearch.Width = 200;
                    CBGender.Visibility = Visibility.Collapsed;
                    Methods.ChangeGridHeight(ref row2, "40");
                    Methods.ChangeGridHeight(ref row3, "40");
                    Methods.ChangeGridHeight(ref row4, "0");
                    
                    DGUC.MinHeight = 650;
                    Methods.AddcolumnLV(ref DGUC, this.Resources["WName"].ToString(), "WName");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Type"].ToString(), "Typee");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["ActionType"].ToString(), "ActionType");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Name"].ToString(), "Name");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["CName"].ToString(), "CName");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Date"].ToString(), "Date");
                    query = "Select * from  GetHistory()";


                }
                else if (TypeUC == "Particulier")
                {
                    BtnIMG1.Visibility = Visibility.Collapsed;
                    BtnIMG2.Visibility = Visibility.Collapsed;
                 
                    CBSpec.Visibility = Visibility.Collapsed;
                    SPG1.Visibility = Visibility.Visible;
               
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Name"].ToString(), "Name");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["TName"].ToString(), "TName");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["TPaymentMethod"].ToString(), "Method");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["TPayment"].ToString(), "TPrice");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["SNames"].ToString(), "SNames");
                    query = "Select Particulier.ID as ID ,  Teacher.TFirstName + ' ' + Teacher.TLastName as TName ,  Teacher.TLastName + ' ' + Teacher.TFirstName as RTName ,Particulier.TPrice , Particulier.Name as Name,Teacher.TGender as Gender , Teacher.ID as TID ,  case When  Particulier.Method = 1 then N'" + this.Resources["PerPeriod"].ToString() + "' When Particulier.Method = 2 then N'" + this.Resources["PerStudent"] + "' End as Method , dbo.getstudentsParticulier(Particulier.ID) as SNames from Particulier left join Particulier_Student on Particulier.ID = Particulier_Student.PID  left join Particulier_Time on Particulier_Time.PID = Particulier.ID left join Teacher on Teacher.ID = Particulier.TID ";
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "ClassV") != 1)
                    {
                        DGUC.IsEnabled = false;
                    }
                }
                else if (TypeUC == "Formation")
                {
                    BtnIMG1.Visibility = Visibility.Collapsed;
                    BtnIMG2.Visibility = Visibility.Collapsed;
                    DGUC.Height = Commun.ScreenHeight - 210;
                    CBSpec.Visibility = Visibility.Collapsed;
                    SPG1.Visibility = Visibility.Visible;
                    Methods.AddcolumnLV(ref DGUC, this.Resources["FormationName"].ToString(), "FName");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["TotalHours"].ToString(), "THours");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["RemainingHours"].ToString(), "RHours");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["Price"].ToString(), "Price");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["TName"].ToString(), "TName");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["TPaymentMethod"].ToString(), "Method");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["TSalary"].ToString(), "Salary");
                    //Methods.AddcolumnDG(ref DGUC, this.Resources["Time"].ToString(), "Time");
                    Methods.AddcolumnLV(ref DGUC, this.Resources["StudentAmount"].ToString(), "SAmmount");
                    Methods.AddcolumnLV(ref DGUC, "Status", "Status");
                    query = "Select " +
                        "Formation.ID as ID ," +
                        "Formation.Name as FName , " +
                        "Formation.Hours as THours , " +
                        "dbo.FormationRemainingHours(Formation.ID) as RHours," +
                        "Formation.Price as Price , " +
                        "Formation.TID , " +
                        "Teacher.TFirstName +  ' ' + Teacher.TLastName as TName , " +
                        "Case " +
                        "When Formation.TPaymentMethod = 0 " +
                        "then N'" + this.Resources["PerHour"].ToString() + "'  " +
                        "When Formation.TPaymentMethod = 1  " +
                        "Then N'" + this.Resources["PerStudent"].ToString() + "' " +
                        "When Formation.TPaymentMethod = 2 " +
                        "Then N'" + this.Resources["PerPeriod"].ToString() + "' " +
                        "End as Method , " +
                        "Formation.TPrice as Salary , " +
                        "dbo.FormationTotalStudents(Formation.ID) as SAmmount , " +
                        "1 as Status From Formation Join Teacher on Teacher.ID = Formation.TID  " ;
                }
                if (DGUC.IsEnabled)
                {
                    Connexion.FillDT(ref dtMain, query);
                    DGUC.ItemsSource = dtMain.DefaultView;
                    StudentAmmount.Text = dtMain.DefaultView.Count.ToString();
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            DataRowView rowview = (DataRowView)checkBox.DataContext;
            if(rowview["ID"].ToString() == "0")
            {
                foreach (DataRow row in dtSubjects.Rows)
                {
                    if (row["ID"].ToString() != "0")
                    {
                        row["IsChecked"] = 1;
                    }
                }
            }
            Search();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            DataRowView rowview = (DataRowView)checkBox.DataContext;
            if (rowview["ID"].ToString() == "0")
            {
                foreach (DataRow row in dtSubjects.Rows)
                {
                    if (row["ID"].ToString() != "0")
                    {
                        row["IsChecked"] = 0;
                    }
                }
            }
            Search();
        }

        private void DataGridColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var columnHeader = sender as DataGridColumnHeader;
            if (columnHeader != null)
            {
                DGUC.SelectedItem = null;
            }
        }
        public void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TypeUC == "Student")
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
                else if (TypeUC == "Teacher")
                {
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "TeacherA") == 1)
                    {
                        var AddT = new TeacherAdd("Add", "");
                        AddT.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Sorry , You Dont have privilage to do this action");
                    }
                }
                else if (TypeUC == "Class" || TypeUC == "Group")
                {
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "ClassA") == 1)
                    {
                        string Multiple;
                        if (MessageBox.Show("Does The Class Have Multiple Groups?",
                         "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            Multiple = "Multiple";
                        }
                        else
                        {
                            Multiple = "Single";
                        }
                        var AddC = new ClassAdd("Add", "",Multiple);
                        AddC.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Sorry , You Dont have privilage to do this action");
                    }
                }
                else if (TypeUC == "Particulier")
                {
                    Particulier P = new Particulier(-1);
                    P.Show();
                }
                else if (TypeUC == "Worker")
                {
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "WorkerA") == 1)
                    {
                        var AddW = new WorkerAdd("Add", "");
                        AddW.ShowDialog();
                    }
                }
                else if (TypeUC == "Payment")
                {

                }
                else if (TypeUC == "SPayment")
                {
                }
                else if(TypeUC == "Formation")
                {
                    AddFormation Addform = new AddFormation("Add" , "");
                    Addform.ShowDialog();
                }
                Connexion.FillDT(ref dtMain, query);
                DGUC.ItemsSource = dtMain.DefaultView;
                Search();
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }


        }

        private void ListviewUC_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string ID = "";
                DataRowView rowview = DGUC.SelectedItem as DataRowView;
                if (rowview == null)
                {
                    return;
                }
                DataRow row = rowview.Row;
                ID = row["ID"].ToString();
                if (TypeUC == "Teacher")
                {
                    var AddW = new TeacherAdd("Show", ID);
                    AddW.Closing += (senderr, ee) =>
                    {
                        Connexion.FillDT(ref dtMain, query);
                        DGUC.ItemsSource = dtMain.DefaultView;
                        Search();
                    };
                    AddW.Show();
                }
                else if (TypeUC == "Student")
                {
                    var AddW = new StudentAdd("Show", ID);
                    AddW.Closing += (senderr, ee) =>
                    {
                        Connexion.FillDT(ref dtMain, query);
                        DGUC.ItemsSource = dtMain.DefaultView;
                        Search();
                    };
                    AddW.Show();
                }
                else if (TypeUC == "Class")
                {
                    string TID = row["TID"].ToString();
                    string Multi = row["MultipleGroups"].ToString();

                    var AddW = new ClassAdd("Show", ID,Multi);
                    AddW.Closing += (senderr, ee) =>
                     {
                         Connexion.FillDT(ref dtMain, query);
                         DGUC.ItemsSource = dtMain.DefaultView;
                         Search();
                     };
                    AddW.Show();

                }
                else if (TypeUC == "Worker")
                {
                    var AddW = new WorkerAdd("Show", ID);
                    AddW.Closing += (senderr, ee) =>
                    {
                        Connexion.FillDT(ref dtMain, query);
                        DGUC.ItemsSource = dtMain.DefaultView;
                        Search();
                    };
                    AddW.Show();
                }
                else if (TypeUC == "Group")
                {
                    string TID = row["TID"].ToString();
                    string Multi = row["MultipleGroups"].ToString();
                    var AddW = new ClassAdd("Show", ID,Multi);
                    AddW.Closing += (senderr, ee) =>
                    {
                        Connexion.FillDT(ref dtMain, query);
                        DGUC.ItemsSource = dtMain.DefaultView;
                        Search();
                    };
                    AddW.Show();

                }
                else if (TypeUC == "Particulier")
                {
                    var AddP = new Particulier(int.Parse(ID));

                    AddP.Show();

                }
                else if (TypeUC == "Formation")
                {
                    var ShowFormation = new AddFormation("Show", ID);
                    ShowFormation.Show();
                }
                else if (TypeUC == "SPayment")
                {
                    var AddW = new StudentAdd("Show", ID);
                    AddW.Closing += (senderr, ee) =>
                    {
                        Connexion.FillDT(ref dtMain, query);
                        DGUC.ItemsSource = dtMain.DefaultView;
                        Search();
                    };
                    AddW.Show();
                }

            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void BtnGroupSee_Click(object sender, RoutedEventArgs e)
        {
            DataRowView rowview = DGUC.SelectedItem as DataRowView;
            DataRow row = rowview.Row;
            string ID = row["GID"].ToString();
            if(row["Sessions"].ToString() == "0")
            {
                MessageBox.Show("There are no sessions studied in this group yet");
                return; 
            }
            var AttendanceGroup = new GroupAttendance(ID);
            AttendanceGroup.Show();

        }

        private void CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Search();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Search();
        }
        private void Search()
        {
            try
            {
                string Condition = "1 > 0 ";
                foreach (DataRow row in dtSubjects.Rows)
                {
                    if (row["IsChecked"].ToString() == "1")
                    {
                        Condition += "AND Subjects Like '%" + row["Text"].ToString() + "%'";
                    }
                }
                if (TBSearch.Text != "")
                {
                    if (TypeUC == "Student")
                    {
                        if (TBSearch.Text.Last() == '$')
                        {
                            Condition += "And barCode ='" + TBSearch.Text + "' ";
                        }
                        else
                        {
                            Condition += "And (NameSearch Like '%" + Commun.ReplaceArabicName(Regex.Replace(TBSearch.Text, @"\s+", " ")) + "%' OR RNameSearch Like '%" + Commun.ReplaceArabicName(Regex.Replace(TBSearch.Text, @"\s+", " ")) + "%' ";
                            Condition += "OR Name Like '%" + Commun.ReplaceArabicName(Regex.Replace(TBSearch.Text, @"\s+", " ")) + "%' OR RName Like '%" + Commun.ReplaceArabicName(Regex.Replace(TBSearch.Text, @"\s+", " ")) + "%' or BarCode Like '%"+ TBSearch.Text + "%') ";
                        }
                    }
                    else if (TypeUC == "Teacher")
                    {
                        Condition += "And (Name Like '%" + Regex.Replace(TBSearch.Text, @"\s+", " ") + "%' OR RName Like '%" + Regex.Replace(TBSearch.Text, @"\s+", " ") + "%' )";
                    }
                    else if (TypeUC == "Class")
                    {
                        Condition += "And (TName Like '%" + Regex.Replace(TBSearch.Text, @"\s+", " ") + "%' OR RTName Like '%" + Regex.Replace(TBSearch.Text, @"\s+", " ") + "%') OR CName Like '%" + Regex.Replace(TBSearch.Text, @"\s+", " ") + "%'  ";
                    }
                    else if (TypeUC == "Group")
                    {
                        Condition += "And (TName Like '%" + Regex.Replace(TBSearch.Text, @"\s+", " ") + "%' OR RTName Like '%" + Regex.Replace(TBSearch.Text, @"\s+", " ") + "%') OR GroupName Like '%" + Regex.Replace(TBSearch.Text, @"\s+", " ") + "%' ";
                    }
                    else if (TypeUC == "SPayment")
                    {
                        Condition += "And (NameSearch Like '%" + Commun.ReplaceArabicName(Regex.Replace(TBSearch.Text, @"\s+", " ")) + "%' OR RNameSearch Like '%" + Commun.ReplaceArabicName(Regex.Replace(TBSearch.Text, @"\s+", " ")) + "%' ";
                        Condition += "OR Name Like '%" + Commun.ReplaceArabicName(Regex.Replace(TBSearch.Text, @"\s+", " ")) + "%' OR RName Like '%" + Commun.ReplaceArabicName(Regex.Replace(TBSearch.Text, @"\s+", " ")) + "%') ";
                    }
                    else if (TypeUC == "Worker")
                    {
                        Condition += "And (Name Like '%" + Regex.Replace(TBSearch.Text, @"\s+", " ") + "%' OR RName Like '%" + Regex.Replace(TBSearch.Text, @"\s+", " ") + "%') ";
                    }
                    else if (TypeUC == "History")
                    {
                        Condition += "And (WName Like '%" + Regex.Replace(TBSearch.Text, @"\s+", " ") + "%' OR WRName Like '%" + Regex.Replace(TBSearch.Text, @"\s+", " ") + "%') ";
                    }
                }
                if (CBGender.SelectedIndex != 0)
                {
                    int f = CBGender.SelectedIndex - 1;
                    Condition += "And Gender ='" + f + "' ";
                }
                if (CBLevel.SelectedIndex != -1 && CBLevel.SelectedIndex != 0)
                {
                    DataRowView rowview = CBLevel.SelectedItem as DataRowView;
                    Condition += "And LevelID ='" + rowview["ID"].ToString() + "' ";
                }
                if (CBYear.SelectedIndex != -1 && CBYear.SelectedIndex != 0)
                {
                    DataRowView rowview = CBYear.SelectedItem as DataRowView;
                    Condition += "And YearID ='" + rowview["ID"].ToString() + "' ";
                }
                if (CBSpec.SelectedIndex != -1 && CBSpec.SelectedIndex != 0)
                {
                    DataRowView rowview2 = CBSpec.SelectedItem as DataRowView;
                    Condition += "And SpecID ='" + rowview2["ID"].ToString() + "' ";
                }
                if (CBClass.SelectedIndex != -1 )
                {
                    DataRowView rowview3 = CBClass.SelectedItem as DataRowView;
                    Condition += "And CID ='" + rowview3["ID"].ToString() + "' ";
                }
               
                if (Date.Text != "")
                {
                    DateTime dtfrom = DateTime.Parse(Date.Text);
                    string from = dtfrom.ToString("yyyy-MM-dd");
                    Condition += "AND DateSearch >= #" + from + "# ";
                    Condition += "AND DateSearch <= #" + from + "# ";
                }
                if (CBType.SelectedIndex != -1)
                {
                    int index = CBType.SelectedIndex;
                    if (CBType.SelectedIndex >= 4)
                    {
                        index++;
                    }
                    if (CBType.SelectedIndex >= 6)
                    {
                        index++;
                    }
                    Condition += " And Type = " + index;
                }
                if (CBTypeAction.SelectedIndex != -1)
                {
                    Condition += " And TypeAct = " + CBTypeAction.SelectedIndex;
                }
                dtMain.DefaultView.RowFilter = Condition;
                StudentAmmount.Text = dtMain.DefaultView.Count.ToString();
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void DGUC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView rowview = DGUC.SelectedItem as DataRowView;
                if (rowview != null)
                {
                    DataRow row = rowview.Row;
                    if (TypeUC == "Student")
                    {
                        string Name = row["Name"].ToString();
                        string number = row["ParentNumber"].ToString() + "/" + row["PhoneNumber"].ToString();

                        string year = row["Year"].ToString();
                        TBName.Text = Name;
                        TBNumber.Text = number;
                        TBYear.Text = year;
                        string ID = row["ID"].ToString();
                        if (System.IO.File.Exists(Connexion.GetImagesFile() + "\\S" + ID + ".jpg"))
                        {
                            string ShowPic = Connexion.GetImagesFile() + "\\S" + ID + ".jpg";
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(ShowPic);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            Pic.Source = bitmap;
                        }
                        else
                        {
                            if (row["Gender"].ToString() == "1")
                            {
                                BitmapImage bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\Women.png");
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                Pic.Source = bitmap;
                            }
                            else if (row["Gender"].ToString() == "0")
                            {
                                BitmapImage bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\man.png");
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                Pic.Source = bitmap;
                            }
                        }
                    }
                    else if (TypeUC == "Teacher")
                    {
                        string Name = row["Name"].ToString();
                        string number = row["TPhoneNumber"].ToString();
                        TBName.Text = Name;
                        TBNumber.Text = number;
                        string ID = row["ID"].ToString();
                        if (System.IO.File.Exists(Connexion.GetImagesFile() + "\\T" + ID + ".jpg"))
                        {
                            string ShowPic = Connexion.GetImagesFile() + "\\T" + ID + ".jpg";
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(ShowPic);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            Pic.Source = bitmap;
                        }
                        else
                        {
                            if (row["Gender"].ToString() == "1")
                            {
                                BitmapImage bitmap = new BitmapImage();

                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\man.png");
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                Pic.Source = bitmap;
                            }
                            else if (row["Gender"].ToString() == "0")
                            {
                                BitmapImage bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\man.png");
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                Pic.Source = bitmap;
                            }
                        }
                    }
                    else if (TypeUC == "Class")
                    {
                        string Name = row["CName"].ToString();
                        string TName = row["TName"].ToString();
                        string Year = row["Year"].ToString();
                        string ID = row["TID"].ToString();
                        TBName.Text = Name;
                        TBNumber.Text = TName;
                        TBYear.Text = Year;
                        if (System.IO.File.Exists(Connexion.GetImagesFile() + "\\T" + ID + ".jpg"))
                        {
                            string ShowPic = Connexion.GetImagesFile() + "\\T" + ID + ".jpg";
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(ShowPic);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            Pic.Source = bitmap;
                        }
                        else
                        {
                            if (row["TGender"].ToString() == "1")
                            {
                                BitmapImage bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\Women.png");
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                Pic.Source = bitmap;
                            }
                            else if (row["TGender"].ToString() == "0")
                            {
                                BitmapImage bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\man.png");
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                Pic.Source = bitmap;
                            }
                        }
                    }
                    else if (TypeUC == "Group")
                    {
                        string Name = row["GroupName"].ToString();
                        string TName = row["TName"].ToString();
                        string Year = row["Year"].ToString();
                        TBName.Text = Name;
                        TBNumber.Text = TName;
                        TBYear.Text = Year;
                        string ID = row["TID"].ToString();
                        if (System.IO.File.Exists(Connexion.GetImagesFile() + "\\T" + ID + ".jpg"))
                        {
                            string ShowPic = Connexion.GetImagesFile() + "\\T" + ID + ".jpg";
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(ShowPic);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            Pic.Source = bitmap;
                        }
                        else
                        {
                            if (row["TGender"].ToString() == "1")
                            {
                                BitmapImage bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\Women.png");
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                Pic.Source = bitmap;
                            }
                            else if (row["TGender"].ToString() == "0")
                            {
                                BitmapImage bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\man.png");
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                Pic.Source = bitmap;
                            }
                        }

                    }
                    else if (TypeUC == "SPayment")
                    {
                        string ID = row["ID"].ToString();
                        string Number = Connexion.GetString(ID, "Students", "PhoneNumber");
                        Number += "/" + Connexion.GetString(ID, "Students", "ParentNumber");
                        string Gender = Connexion.GetString(ID, "Students", "Gender");
                        string Name = row["Name"].ToString();
                        TBName.Text = Name;
                        TBNumber.Text = Number;
                        if (System.IO.File.Exists(Connexion.GetImagesFile() + "\\" + ID + ".jpg"))
                        {
                            string ShowPic = Connexion.GetImagesFile() + "\\" + ID + ".jpg";
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(ShowPic);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            Pic.Source = bitmap;
                        }
                        else
                        {
                            if (Gender == "1")
                            {
                                BitmapImage bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\Women.png");
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                Pic.Source = bitmap;
                            }
                            else if (Gender == "0")
                            {
                                BitmapImage bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\man.png");
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                Pic.Source = bitmap;
                            }
                        }

                    }
                    else if (TypeUC == "Particulier")
                    {

                    }
                    else if (TypeUC == "Worker")
                    {
                        string Name = row["Name"].ToString();
                        string number = row["Number"].ToString();
                        TBName.Text = Name;
                        TBNumber.Text = number;
                        string ID = row["ID"].ToString();
                        Methods.InsertPicwithGender(ref Pic, Connexion.GetImagesFile() + "\\W" + ID + ".jpg", row["Gender"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }
        private void CBLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CBSpec.SelectedIndex = 0;
            DataRowView row = (DataRowView)CBLevel.SelectedItem;
            if (row != null && row["ID"].ToString() != "0" )
            {
                DataTable dtyear = new DataTable();
                Connexion.FillDT(ref dtyear, "Select * from Years Where LevelID = " + row["ID"].ToString());
                DataRow newRow = dtyear.NewRow();
                newRow["ID"] = 0;
                newRow["Year"] = this.Resources["Year--"].ToString();
                dtyear.Rows.InsertAt(newRow, 0);
                CBYear.ItemsSource = dtyear.DefaultView;
                CBYear.SelectedIndex = 0;
            }
            Search();
        }

        private void CBYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CBSpec.SelectedIndex = 0;
            DataRowView row = (DataRowView)CBLevel.SelectedItem;
            DataRowView row2 = (DataRowView)CBYear.SelectedItem;
            if (row != null && row2 != null && row2["ID"].ToString() != "0")
            {
                if (row["IsSpeciality"].ToString() == "1")
                {
                    DataTable dtSpec = new DataTable();

                    Connexion.FillDT(ref dtSpec, "Select * from Specialities Where YearID = " + row2["ID"].ToString());
                    DataRow newRow = dtSpec.NewRow();
                    newRow["ID"] = 0;
                    newRow["Speciality"] = this.Resources["Speciality--"].ToString();
                    dtSpec.Rows.InsertAt(newRow, 0);
                    CBSpec.ItemsSource = dtSpec.DefaultView;
                    CBSpec.SelectedIndex = 0;
                }
                else
                {
                    CBSpec.ItemsSource = null;
                }
            }
            Search();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void BtnPay_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)DGUC.SelectedItem;
            SelectAllTeacherPaymentGroup TPG = new SelectAllTeacherPaymentGroup(row["ID"].ToString());
            TPG.Show();
        }

        public void SetLang()
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

        private void Button_Click_Delete(object sender, RoutedEventArgs e)
        {
            if (DGUC.SelectedItems == null)
            {
                MessageBox.Show(this.Resources["MessageBoxNoDeleteSelections"].ToString());
            }
            string id = "";
            if (TypeUC == "Student")
            {
                if (Connexion.GetInt(Connexion.WorkerID, "Users", "StudentD") == 1)
                {

                    foreach (DataRowView row in DGUC.SelectedItems)
                    {
                        id = row["ID"].ToString();
                        if (Connexion.IFNULL("Select * from Class_Student Where studentID = " + id))
                        {
                            Connexion.Insert("Delete from Students  Where ID = " + id);
                            Connexion.InsertHistory(1, id, 0);
                        }
                        else
                        {
                            if (MessageBox.Show("Are you sure you want to delete this Student?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                Connexion.Insert("Delete from Students  Where ID = " + id);
                                Connexion.Insert("Delete from Class_Student  Where StudentID = " + id);
                                Connexion.Insert("Delete from Attendance_Student  Where StudentID = " + id);
                                Connexion.Insert("Delete from Attendance_Extra_Students  Where SID = " + id);
                                Connexion.Insert("Delete from Discounts  Where StudentID = " + id);
                                Connexion.Insert("Delete from StudentPayment  Where SID = " + id);
                                Connexion.Insert("Delete from Monthly_Payment  Where SID = " + id);

                            }

                        }
                    }
                }
                else
                {
                    MessageBox.Show("Sorry, You dont have the privalage to do this action");
                    return;
                }
            }
            else if (TypeUC == "Teacher")
            {
                if (Connexion.GetInt(Connexion.WorkerID, "Users", "TeacherD") == 1)
                {
                    foreach (DataRowView row in DGUC.SelectedItems)
                    {
                        id = row["ID"].ToString();
                        if (Connexion.IFNULL("Select * from Class Where TID = " + id))
                        {
                            Connexion.Insert("Update Teacher   Set Status = -1 Where ID = " + id);
                            Connexion.InsertHistory(1, id, 1);
                        }
                        else
                        {
                            MessageBox.Show("Cant delete this teacher , already existing classes with this teacher");
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Sorry, You dont have the privalage to do this action");
                    return;
                }
            }
            else if (TypeUC == "Worker")
            {
                if (Connexion.GetInt(Connexion.WorkerID, "Users", "WorkerD") == 1)
                {

                    foreach (DataRowView row in DGUC.SelectedItems)
                    {
                        id = row["ID"].ToString();
                        Connexion.Insert("Update  Workers Set Status = 0 Where ID = " + id);
                        Connexion.InsertHistory(1, id, 2);
                    }
                }
                else
                {
                    MessageBox.Show("Sorry, You dont have the privalage to do this action");
                    return;
                }
            }
            else if (TypeUC == "Class")
            {
                if (Connexion.GetInt(Connexion.WorkerID, "Users", "ClassD") == 1)
                {
                    if (MessageBox.Show("Are you sure you want to delete this class?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (MessageBox.Show("Are you sure you want to delete this class? it cannot be retrieved", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            foreach (DataRowView row in DGUC.SelectedItems)
                            {
                                id = row["ID"].ToString();
                                Connexion.Insert("Delete from  Class Where ID = " + id);
                                Connexion.Insert("Delete from  Class_Student Where ClassID = " + id);
                                Connexion.Insert("Delete from  groups Where CLASSID = " + id);
                               // Connexion.InsertHistory(1, id, 3);
                                
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Sorry, You dont have the privalage to do this action");
                    return;
                }
            }
            MessageBox.Show("Deleted succesfully");

            Connexion.FillDT(ref dtMain, query);
            DGUC.ItemsSource = dtMain.DefaultView;
            Search();
          
        }

        private void Button_Click_Import(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TypeUC == "Student")
                {
                    TwoOptionsButtons customMessageBox = new TwoOptionsButtons("Do you want to Import or Export?", "Import", "Export");
                    customMessageBox.ShowDialog();
                    DataTable Unregistered = new DataTable();
                    Unregistered.Columns.Add("");
                    // Access the result after the dialog is closed
                    int result = customMessageBox.Result;
                    int errorcount = 0;
                    if (result == 2)
                    {

                        Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
                        Microsoft.Office.Interop.Excel.Workbook workbook = excelApp.Workbooks.Add(System.Reflection.Missing.Value);
                        Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];

                        // Create headers for the columns you want to export
                        string[] columnNames = { "FirstName","LastName", "PhoneNumber", "ParentNumber", "Genderr", "BirthDate", "Level", "Year", "Spec", "Adress", "Register", "Note", "Insc" };

                        int colIndex = 1;
                        foreach (var columnName in columnNames)
                        {
                            worksheet.Cells[1, colIndex] = columnName;
                            colIndex++;
                        }

                        int rowIndex = 2;

                        // Loop through the filtered rows in the DataTable and export the data
                        DataTable dtExcel = new DataTable();
                        Connexion.FillDT(ref dtExcel, "Select   Students.FirstName as FirstName,Students.LastName as LastName , Students.BarCode as BarCode , " +
                        "Case When Students.PayedInsc = 0 then N'" + this.Resources["Paid"].ToString() + "'  " +
                        "when Students.PayedInsc = 1 then N'" + this.Resources["Didntpay"].ToString() + "' " +
                        "when Students.PayedInsc = 2 then N'" + this.Resources["DoesntPay"].ToString() + "' " +
                        "When Students.PayedInsc = 3 then N'" + this.Resources["Refund"].ToString() + "'  end as Insc,students.PayedInsc as PayedInsc,Levels.Level as Level,  Students.PhoneNumber , Students.ParentNumber ,Students.ID as ID, Students.School ,students.Adress ,  Students.Note , Students.Register , Students.Birthdate,Years.ID as YearID, Students.Gender as Gender,Students.FirstName + ' ' + Students.LastName as RName , Years.Year as Year ,Case When Students.Gender = 1 Then 'Female' When Students.Gender = 0  Then 'Male' END  as Genderr , Specialities.Speciality as Spec  ,Students.Speciality as SpecID from Students join Levels on Levels.ID = Students.Level  Join Years On Students.Year = Years.ID  Left Join Specialities On Students.Speciality = Specialities.ID Where Status = 1 order by Students.ID desc");

                        foreach (DataRowView rowView in dtExcel.DefaultView)
                        {
                            colIndex = 1;
                            foreach (var columnName in columnNames)
                            {
                                worksheet.Cells[rowIndex, colIndex] = rowView.Row[columnName].ToString();
                                colIndex++;
                            }
                            rowIndex++;
                        }

                        Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                        saveFileDialog.Filter = "Excel Files|*.xlsx";

                        bool? resultfordialog = saveFileDialog.ShowDialog();

                        if (resultfordialog == true)
                        {
                            string filePath = saveFileDialog.FileName;


                            // Save the Excel workbook to the selected file path
                            workbook.SaveAs(filePath);

                            // Close Excel and release resources
                            workbook.Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
                            excelApp.Quit();
                            Marshal.ReleaseComObject(workbook);
                            Marshal.ReleaseComObject(excelApp);
                        }

                    }
                    else if (result == 1)
                    {
                        Microsoft.Office.Interop.Excel.Application xlapp;
                        Microsoft.Office.Interop.Excel.Workbook xlworkbook;
                        Microsoft.Office.Interop.Excel.Worksheet xlWorksheet;
                        Microsoft.Office.Interop.Excel.Range xlrange;
                        int xlrow;
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
                            int Spec2Philo =  Connexion.GetInt("Philo", "Specialities", "ID", "Speciality", "YearID", AS2ID.ToString());
                            //3AS Speciality
                            int AS3ID = Connexion.GetInt("3AS", "Years", "ID", "Year");
                            int Spec3Sc = Connexion.GetInt("Scientifique", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                            int Spec3M = Connexion.GetInt("mathéleme", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                            int Spec3Philo = Connexion.GetInt("Philo", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                            int Spec3Langue = Connexion.GetInt("Langue", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                            int Spec3MT = Connexion.GetInt("Math-Technique", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                            int Spec3GE = Connexion.GetInt("Gestion", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                            int spec3Philo = Connexion.GetInt("philosophie", "Specialities", "ID", "Speciality", "YearID", AS3ID.ToString());
                            string ClassID = "";
                         
                            for (xlrow = 2; xlrow < 1000; xlrow++)
                            {
                                int YearID = 0;
                                int SpecID = 0;
                                int levelID = 0;
                                string Index = "";
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
                                if (Convert.ToString(xlWorksheet.Cells[xlrow,5].Value) != null)
                                {
                                    PNumber = Convert.ToString(xlWorksheet.Cells[xlrow,5].Value).Replace("'", "''");
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
                                    else if (Speciality == "gestion")
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
                                    else if (Speciality == "gestion")
                                    {
                                        SpecID = Spec3GE;
                                    }
                                    else if(Speciality == "Philo")
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
                                        SID =  Connexion.GetInt("Insert into Students" +
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
                                           "N'" + levelID + "' , 1,N'" + Index +  "$')");
                                    }
                                    else
                                    {
                                        SID =  Connexion.GetInt("Insert into Students" +
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
                                    if(ClassID != "")
                                    {
                                        if(Convert.ToString(xlWorksheet.Cells[xlrow, 14].Value) != null)
                                        {
                                            if (!Connexion.IFNULL("Select GroupID from Groups " +
                                                "WHere ClassID = " + ClassID + " " +
                                                "and GroupName = " + Convert.ToString(xlWorksheet.Cells[xlrow, 14].Value)))
                                            {
                                                string GID = Connexion.GetString("Select GroupID from Groups" +
                                                    " WHere ClassID = " + ClassID + " " +
                                                    "and GroupName = " + Convert.ToString(xlWorksheet.Cells[xlrow, 14].Value));
                                                Connexion.Insert("Insert into Class_Student" +
                                                    " Values ('" + SID + "','" + ClassID + "' ,  '" + GID + "'," + 0 + ",NULL,0,0 )");
                                                if (Convert.ToString(xlWorksheet.Cells[xlrow, 16].Value) != null && Convert.ToString(xlWorksheet.Cells[xlrow, 17].Value) != null)
                                                {
                                                    string[] values = Convert.ToString
                                                        (xlWorksheet.Cells[xlrow, 16].Value).Split(',');
                                                    string[] valuesDate = Convert.ToString
                                                       (xlWorksheet.Cells[xlrow, 17].Value).Split(',');
                                                    int f = 0;
                                                    // Loop through each value
                                                    foreach (string value in values)
                                                    {
                                                        string ID = Connexion.GetInt("Insert into " +
                                                       "StudentPayment output inserted.ID Values " +
                                                       "(" + SID + "," +
                                                       "1," +
                                                       "'" + ClassID + "'," +
                                                       "'" + value + "'," +
                                                       "''," +
                                                       "'" + valuesDate[f] + "',0)").ToString();
                                                        f++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    errorcount = 0;
                                }
                                else
                                {
                                    errorcount++;
                                        Unregistered.Rows.Add(xlrow);
                                    if(errorcount ==10)
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
                            DGUC.ItemsSource = null;
                            dtMain.Clear();
                            Connexion.FillDG(ref DGUC, query);
                            Connexion.FillDT(ref dtMain, query);
                            Search();
                            MessageBox.Show(this.Resources["InsertedSucc"].ToString());
                        }
                    }

                }
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnAttendance_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)DGUC.SelectedItem;
            if (row == null)
            {
                return;
            }
            string GroupID = row["GID"].ToString();
            bool Exist = Connexion.IFNULL("Select ID from Attendance Where GroupID = '" + row["GID"].ToString() + "' And Date = '" + DateTime.Today.ToString("d").Replace("/", "-") + "'");
            if (Exist)
            {
                int result = Connexion.GetInt(GroupID, "Groups", "Sessions", "GroupID") + 1;
                int ClassID = Connexion.GetClassID(GroupID);
                DataTable dtTimes = new DataTable();
                Connexion.FillDT(ref dtTimes, "Select * from Class_Time Where GID ='" + GroupID + "'");
                string TimeID = "";
                bool Found = false;
                for (int i = 0; i < dtTimes.Rows.Count; i++)
                {
                    DateTime date = DateTime.Today;
                    string DayID = dtTimes.Rows[i]["Day"].ToString();
                    string Today = date.DayOfWeek.ToString();
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
                if (Found)
                {
                    Connexion.Insert("Update Groups set Sessions = Sessions + 1 , TSessions = TSessions + 1 Where GroupID = " + GroupID);
                    int IDRoom = Connexion.GetInt("Select IDRoom from Class_Time where ID =" + TimeID);
                    string TimeStart = Connexion.GetString("Select TimeStart from Class_Time where ID =" + TimeID);
                    string TimeEnd = Connexion.GetString("Select TimeEnd from Class_Time where ID =" + TimeID);
                    int AID = Connexion.GetInt("Insert into Attendance(GroupID,Date,Session,GTID) OUTPUT Inserted.ID  Values(" + GroupID + ",'" + DateTime.Today.ToString("d").Replace("/", "-") + "','" + result + "' , '" + IDRoom + "','"+ TimeStart +"','" + TimeEnd + "')");
                    Connexion.InsertHistory(0, AID.ToString(), 5);
                    var AddS = new AttendanceAdd(AID.ToString(), "Add" , "1");
                    AddS.Show();
                }
                else
                {
                    MessageBox.Show("There isnt a Group With This Time Frame ");
                }
            }
            else
            {
                int AID = Connexion.GetInt(GroupID, "Attendance", "ID", "GroupID", "Date", DateTime.Today.ToString("d").Replace("/", "-"));
                int ClassID = Connexion.GetClassID(GroupID);
                var AddS = new AttendanceAdd(AID.ToString(), "Show" , "1");
                AddS.Show();
            }
        }

        private void Button_Click_PrintStudentInfo(object sender, RoutedEventArgs e)
        {
            if (DGUC.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please Select a Student(s)");
                return; 
            }
            if (TypeUC == "Student")
            {
                Report r = new Report();
                int count = 0;
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                DataTable dtEcoleinfo = new DataTable();
                foreach (DataRowView row in DGUC.SelectedItems)
                {
                    if (Connexion.Language() == 0) // eng
                    {
                        r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\StudentInfo.frx");
                    }
                    else // ar
                    {
                        r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\StudentInfoAR.frx");
                    }
                    ds.Clear();
                    dt.Clear();
                    dt.Columns.Clear();
                    dtEcoleinfo.Columns.Clear();
                    dtEcoleinfo.Clear();
                    ds.Tables.Clear();
                    dtEcoleinfo.TableName = "EcoleInfo";
                    Connexion.FillDT(ref dtEcoleinfo, "Select * from EcoleSetting");
                    dtEcoleinfo.Columns.Add("Logo");
                    dtEcoleinfo.Rows[0]["Logo"] = Connexion.GetImagesFile() + @"\EcoleLogo.jpg";
                    dt.Columns.Add("FirstName");
                    dt.Columns.Add("LastName");
                    dt.Columns.Add("PhoneNumber");
                    dt.Columns.Add("ParentNumber");
                    dt.Columns.Add("Adress");
                    dt.Columns.Add("SchoolName");
                    dt.Columns.Add("Note");
                    dt.Columns.Add("BarCode");
                    dt.Columns.Add("Gender");
                    dt.Columns.Add("BirthDate");
                    dt.Columns.Add("InscFees");
                    dt.Columns.Add("Level");
                    dt.Columns.Add("Year");
                    dt.Columns.Add("Speciality");
                    dt.Rows.Add();
                    dt.Rows[0][0] = row["FirstName"].ToString();
                    dt.Rows[0][1] = row["LastName"].ToString();
                    dt.Rows[0][2] = row["PhoneNumber"].ToString();
                    dt.Rows[0][3] = row["ParentNumber"].ToString();
                    dt.Rows[0][4] = row["Adress"].ToString();
                    dt.Rows[0][5] = row["School"].ToString();
                    dt.Rows[0][6] = row["Note"].ToString();
                    dt.Rows[0][7] = row["BarCode"].ToString();
                    dt.Rows[0][8] = row["Genderr"].ToString();
                    dt.Rows[0][9] = row["BirthDate"].ToString();
                    dt.Rows[0][10] = row["Insc"].ToString();
                    dt.Rows[0][11] = row["Level"].ToString();
                    dt.Rows[0][12] = row["Year"].ToString();
                    dt.Rows[0][13] = row["Spec"].ToString();
                    dt.TableName = "Student";
                    ds.Tables.Add(dtEcoleinfo);
                    ds.Tables.Add(dt);
                    r.RegisterData(ds);
                    r.GetDataSource("EcoleInfo").Enabled = true;
                    r.GetDataSource("Student").Enabled = true;

                    if (Commun.FastReportEdit == 0)
                    {
                        r.Design();
                        return;
                    }
                    if (count == 0)
                    {
                        r.Prepare();
                    }
                    else
                    {
                        r.Prepare(true);
                    }
                    count++;
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
        }

        private void Button_Click_PrintClassPayments(object sender, RoutedEventArgs e)
        {
            if (DGUC.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please Select a Student(s)");
                return;
            }
            if (TypeUC == "Student")
            {
                Report r = new Report();
                
                int count = 0;
                DataSet ds = new DataSet();
                DataTable dtStudent = new DataTable();
                DataTable dtEcoleinfo = new DataTable();
                DataTable dtInfo = new DataTable();
                foreach (DataRowView row in DGUC.SelectedItems)
                {
                    if (Connexion.Language() == 0) // eng
                    {
                        r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\ClassInfoStudent.frx");
                    }
                    else // ar
                    {
                        r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\ClassInfoStudentAR.frx");
                    }
                    ds.Clear();
                    dtInfo.Clear();
                    dtInfo.Columns.Clear();
                    dtStudent.Clear();
                    dtStudent.Columns.Clear();
                    dtEcoleinfo.Columns.Clear();
                    dtEcoleinfo.Clear();
                    ds.Tables.Clear();
                    dtEcoleinfo.TableName = "EcoleInfo";
                    Connexion.FillDT(ref dtEcoleinfo, "Select * from EcoleSetting");
                    dtEcoleinfo.Columns.Add("Logo");
                    dtEcoleinfo.Rows[0]["Logo"] = Connexion.GetImagesFile() + @"\EcoleLogo.jpg";
                    string ID = row["ID"].ToString();
                    dtStudent.Columns.Add("name");
                    dtStudent.Rows.Add();
                    dtStudent.Rows[0][0] = row["Name"].ToString();
                    Connexion.FillDT(ref dtInfo, "SELECT " +
                        "Class_Student.ClassID," +
                        "Class_Student.StudentID, " +
                        "Class_Student.Session + 1 as StartSes  , " +
                        "Class_Student.EndSession as EndSes, " +
                        "Case when Class.MultipleGroups = 'Multiple' " +
                        "then Groups.GroupName ENd as GName , " +
                        "Class.CName as CName ," +
                        "Class.CSubject ," +
                        "groups.GroupID ," +
                        "dbo.CalculatePrice(Class_Student.StudentID ,Groups.GroupID, Groups.TSessions, 'Su') - dbo.CalculatePrice(Class_Student.StudentID ,Groups.GroupID,  Groups.TSessions, 'S') as Price , " +
                        " " +
                        "Subjects.Subject " +
                        "FROM Class  " +
                        "left Join Groups on Groups.ClassID = Class.ID  " +
                        "right JOIN Class_Student " +
                        "ON (Class_Student.ClassID = Class.ID " +
                        "and Class_Student.GroupID = Groups.GroupID) " +
                        "left JOIN Subjects ON Subjects.ID = Class.CSubject " +
                        "Where Class_Student.StudentID  = '" + ID + "'");
                    dtInfo.TableName = "Info";
                    dtStudent.TableName = "Student";
                    dtEcoleinfo.TableName = "EcoleInfo";
                    ds.Tables.Add(dtEcoleinfo);
                    ds.Tables.Add(dtStudent);
                    ds.Tables.Add(dtInfo);
                    r.RegisterData(ds);
                    r.GetDataSource("EcoleInfo").Enabled = true;
                    r.GetDataSource("Student").Enabled = true;
                    r.GetDataSource("Info").Enabled = true;
                    if (Commun.FastReportEdit == 0)
                    {
                        r.Design();
                        return;
                    }
                    if (count == 0)
                    {
                        r.Prepare();
                    }
                    else
                    {
                        r.Prepare(true);
                    }
                    count++;
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
        }

        private void Button_Click_PrintHistory(object sender, RoutedEventArgs e)
        {
            if (DGUC.SelectedItems.Count != 1)
            {
                MessageBox.Show("Please Select just one student");
                return;
            }
            if (TypeUC == "Student")
            {
               
                foreach(DataRowView row in DGUC.SelectedItems)
                {

                    FastReports.PrintHistoryStudent(row["ID"].ToString());
                }
               
            }
            else if (TypeUC == "Class" || TypeUC == "Group")
            {
                Report r = new Report();
                int count = 0;
                DataSet ds = new DataSet();
                DataTable DataAttend = new DataTable();
                DataTable dtEcoleinfo = new DataTable();
                DataTable dtattendInfo = new DataTable();
                string AID;
                DataTable group = new DataTable();
                foreach (DataRowView row in DGUC.SelectedItems)
                {
                    if (Connexion.Language() == 0) // eng
                    {
                        r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\AttendanceFastReport.frx");
                    }
                    else // ar
                    {
                        r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\AttendanceFastReportAR.frx");
                    }
                    ds.Clear();
                    dtattendInfo.Clear();
                    DataAttend.Clear();
                    group.Clear();
                    group.Columns.Clear();
                    DataAttend.Columns.Clear();
                    dtattendInfo.Clear();
                    dtEcoleinfo.Columns.Clear();
                    dtEcoleinfo.Clear();
                    dtEcoleinfo.TableName = "EcoleInfo";
                    Connexion.FillDT(ref dtEcoleinfo, "Select * from EcoleSetting");
                    dtEcoleinfo.Columns.Add("Logo");
                    dtEcoleinfo.Rows[0]["Logo"] = Connexion.GetImagesFile() + @"\EcoleLogo.jpg";
                    if (TypeUC == "Class")
                    {
                        AID = Connexion.GetString("Select top 1 Attendance.ID from Attendance Join Groups on Groups.GroupID = Attendance.GroupID Where Groups.CLassID = " + row["ID"].ToString() + " ORDER BY Attendance.ID DESC");
                    }
                    else
                    {
                        AID = Connexion.GetString("Select top 1 Attendance.ID from Attendance  Where Attendance.GroupID = " + row["GID"].ToString() + " ORDER BY Attendance.ID DESC");
                    }
                    Connexion.FillDT(ref DataAttend, "Select Students.FirstName + ' ' + Students.LastName as Name, " +
                       "       Students.Gender as Gender ," +
                       "        Class_Student.Session + dbo.CalculateSesPayed(Students.ID, Groups.GroupID) - Groups.Sessions as Sessions , "
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
                       "Where Attendance.ID = " + AID + "And Class_Student.ClassID = " + row["ID"].ToString());
                    Connexion.FillDT(ref dtattendInfo, "Select * from Attendance Where ID = " + AID);
                    DataAttend.TableName = "DataAttend";
                    dtattendInfo.TableName = "DataAttendInfo";
                    ds.Tables.Add(dtattendInfo);
                    ds.Tables.Add(DataAttend);
                    DataTable DataEcole = new DataTable();
                    DataEcole.TableName = "DataEcole";
                    Connexion.FillDT(ref DataEcole, "Select NameFR,NameAR,N'" + Connexion.GetImagesFile() + "\\EcoleLogo.jpg'  as Logo , Number ,Number2,Adress from EcoleSetting");
                    ds.Tables.Add(DataEcole);
                    group.TableName = "DataGroup";
                    string GID = Connexion.GetInt("Select Attendance.GroupID from Attendance Where ID =  " + AID).ToString();
                    Connexion.FillDT(ref group, "Select  " +
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
                    ds.Tables.Add(group);
                    r.RegisterData(ds);
                    r.GetDataSource("DataAttend").Enabled = true;
                    r.GetDataSource("DataEcole").Enabled = true;
                    r.GetDataSource("DataGroup").Enabled = true;
                    r.GetDataSource("dataAttendInfo").Enabled = true;
                    if (Commun.FastReportEdit == 0)
                    {
                        r.Design();
                        return;
                    }
                    if (count == 0)
                    {
                        r.Prepare();
                    }
                    else
                    {
                        r.Prepare(true);
                    }
                    count++;
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
        }

        private void Button_Click_PrintBarCode(object sender, RoutedEventArgs e)
        {
            try
            {


                if (DGUC.SelectedItems == null)
                {
                    MessageBox.Show("Please Select a Student(s)");
                    return;
                }
                if (TypeUC == "Student")
                {
                    string path;
                    if (File.Exists(@"C:\ProgramData\EcoleSetting\Mouathfile.txt"))
                    {
                        path = @"C:\Users\Home\Desktop\C# Projects\Gestion_De_Cours\FastReport";
                    }
                    else
                    {
                        path = @"C:\ProgramData\EcoleSetting\EcolePrint";
                    }
                    Report r = new Report();
                    path += @"\BarCodeStudentsAllAR.frx";
                    if (Connexion.Language() == 0) // eng
                    {

                        r.Load(path);
                    }
                    else // ar
                    {
                        r.Load(path);
                    }

                    DataTable dtStudents = new DataTable();
                    dtStudents.Columns.Add("FirstName");
                    dtStudents.Columns.Add("LastName");
                    dtStudents.Columns.Add("BarCode");
                    dtStudents.Columns.Add("Note");
                    dtStudents.Columns.Add("Level");
                    foreach (DataRowView row in DGUC.SelectedItems)
                    {
                        DataRow newRow = dtStudents.NewRow();
                        newRow["FirstName"] = row["FirstName"].ToString();
                        newRow["LastName"] = row["LastName"].ToString();
                        newRow["BarCode"] = row["BarCode"].ToString();
                        newRow["Note"] = row["Note"].ToString();
                        newRow["Level"] = row["Year"].ToString() + " " + row["Spec"].ToString();
                        dtStudents.Rows.Add(newRow);
                    }
                    dtStudents.TableName = "Students";
                    DataSet ds = new DataSet();
                    ds.Tables.Add(dtStudents);
                    DataTable dtEcole = new DataTable();
                    dtEcole.TableName = "Ecole";
                    Connexion.FillDT(ref dtEcole, "Select * from EcoleSetting");
                    ds.Tables.Add(dtEcole);

                    r.RegisterData(ds);
                    r.GetDataSource("Students").Enabled = true;
                    r.GetDataSource("Ecole").Enabled = true;
                    PictureObject picture = r.FindObject("Picture1") as PictureObject;

                    if (picture != null)
                    {
                        // Set the new image path
                        picture.Image = System.Drawing.Image.FromFile(@"C:\ProgramData\EcoleSetting\EcolePhotos\EcoleLogo.jpg");

                        // Optionally, you can set other properties, e.g., image size, if needed
                        // picture.Stretch = true; // Example property to stretch the image
                    }
                    r.Save(path);
                    if (Commun.FastReportEdit != 1)
                    {
                        r.Design();
                    }
                    else
                    {
                        r.Show();
                    }
                }
                else if (TypeUC == "Class" || TypeUC == "Group")
                {
                    Report r = new Report();
                    if (Connexion.Language() == 0) // eng
                    {
                        r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\BarCodeStudentsAllAR.frx");
                    }
                    else // ar
                    {
                        r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\BarCodeStudentsAllAR.frx");
                    }
                    DataSet ds = new DataSet();
                    DataTable dtSN1 = new DataTable();
                    dtSN1.TableName = "SN1";
                    dtSN1.Columns.Add("Name1", typeof(string));
                    dtSN1.Columns.Add("BarCode1", typeof(string));
                    dtSN1.Columns.Add("Name2", typeof(string));
                    dtSN1.Columns.Add("BarCode2", typeof(string));
                    DataTable dt2 = new DataTable();
                    int counter = 0;
                    foreach (DataRowView row in DGUC.SelectedItems)
                    {
                        if (TypeUC == "Class")
                        {
                            Connexion.FillDT(ref dt2, "Select Students.FirstName + ' ' + Students.LastName as Name , " +
                   "Students.BarCode  from Students Join Class_Student on Class_Student.StudentID = Students.ID Where ClassID = " + row["ID"].ToString());
                        }
                        else
                        {
                            Connexion.FillDT(ref dt2, "Select Students.FirstName + ' ' + Students.LastName as Name , " +
                   "Students.BarCode  from Students Join Class_Student on Class_Student.StudentID = Students.ID Where GroupID =" + row["GID"].ToString());
                        }
                        for (int i = 0; i < dt2.Rows.Count; i++)
                        {
                            if (counter % 2 == 0)
                            {
                                dtSN1.Rows.Add();
                                dtSN1.Rows[counter / 2][0] = dt2.Rows[i]["Name"].ToString();
                                dtSN1.Rows[counter / 2][1] = dt2.Rows[i]["BarCode"].ToString();
                            }
                            else if (counter % 2 == 1)
                            {
                                dtSN1.Rows[counter / 2][2] = dt2.Rows[i]["Name"].ToString();
                                dtSN1.Rows[counter / 2][3] = dt2.Rows[i]["BarCode"].ToString();
                            }
                            counter++;
                        }
                    }
                    ds.Tables.Add(dtSN1);
                    r.RegisterData(ds);
                    r.GetDataSource("SN1").Enabled = true;
                    if (Commun.FastReportEdit != 1)
                    {
                        r.Design();
                    }
                    else
                    {
                        r.Show();
                    }
                }
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Search();
        }

        private void CustomComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            row1.Height = new GridLength(110);
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            row1.Height = new GridLength(25);
        }

        private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            if(TBStudentFName.Text != "" && TBStudentLName.Text != "" && TBStudentNumber1.Text !="" && CBStudentInsc.SelectedIndex != -1 && CBStudentYear.SelectedIndex != -1)
            {
                DataRowView rowLevel = (DataRowView)CBStudentLevel.SelectedItem;
                DataRowView rowYear = (DataRowView)CBStudentYear.SelectedItem;
                DataRowView rowSpec = (DataRowView)CBStudentSpec.SelectedItem;
                string SpecID = "NULL";
                if(rowLevel["IsSpeciality"].ToString() == "1")
                {
                    if(CBStudentSpec.SelectedIndex ==  -1)
                    {
                        MessageBox.Show("");
                    }
                    else
                    {
                        SpecID = rowSpec["ID"].ToString();
                    }
                }
              ;
              if (!Commun.CheckName(TBStudentFName.Text, TBStudentLName.Text, string.Format((string)this.Resources["MessageBoxSameName"].ToString(), ( TBStudentFName.Text + ' ' + TBStudentLName.Text))))
              {
                    return;
              }
                string SID = Connexion.GetString("Insert into Students OUTPUT Inserted.ID  Values(" +
                    "N'" + TBStudentFName.Text + "'," +
                    "N'" + TBStudentLName.Text + "'," +
                    "N'" + TBStudentNumber1.Text + "'," +
                    "''," +
                    "" + CBStudentGender.SelectedIndex + "," +
                    "N'" + DPStudent.Text + "','',''," +
                    "" + rowYear["ID"].ToString() + "," +
                    "" + SpecID + " ," +
                    "''," +
                    "N'" + DateTime.Now.ToString("MM / dd / yyyy HH: mm:ss") + "'," +
                    "1," +
                    "" + rowLevel["ID"].ToString() + " ," +
                    "" + CBStudentInsc.SelectedIndex + " " +
                    ", NULL )") ;
                Connexion.InsertHistory(0, SID, 0);
                if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 1)
                {
                    Connexion.Insert("Update Students Set MonthlyPayment = 0 Where ID = " + SID);
                }
                Connexion.Insert("Update students set Barcode = convert(varchar(50),ID ) + '$' where ID = " + SID);
                if (CBStudentInsc.SelectedIndex == 0)
                {
                    int Insc = Connexion.GetInt("Select InscFees from Levels where ID = " + rowLevel["ID"].ToString());
                    int CashID = Connexion.GetInt("Insert into " +
                        "CashRegisterExtra output inserted.ID values (" + Insc + ",N'Insc Fees for student " + TBStudentFName.Text + ' ' + TBStudentLName.Text + " '," + Commun.IDCR + "," + Connexion.WorkerID + ",convert(varchar, getdate(), 8) )");
                    Connexion.InsertHistory(0, CashID.ToString(), 9);
                }
                foreach(DataRow GroupsRow in dtStudentGroup.Rows)
                {
                    if(GroupsRow["IsChecked"].ToString() == "1")
                    {
                        if (!Commun.CheckSeatsClass(GroupsRow["GroupID"].ToString(), this.Resources["WarningSeatsMax"].ToString()))
                        {
                            continue;
                        }
                        int ses = Connexion.GetInt(GroupsRow["GroupID"].ToString(), "Groups", "Sessions", "GroupID");
                        Connexion.Insert("Insert into Class_Student Values ('" + SID + "','" + GroupsRow["ClassID"].ToString() + "', " + GroupsRow["GroupID"].ToString() + "," + ses + ", null,0,0 )");
                    }
                }
                if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 1)
                {
                    int price = Connexion.GetInt("Select MonthlyPayment From Students Where ID = " + SID);

                    string Subjects = Connexion.GetString("Select dbo.GetStudentSubjects(" + SID + ")");
                    Backup newmonthprice = new Backup("This Student now studies in(" + Subjects + ")", 1, price.ToString());
                    if (newmonthprice.ShowDialog() == true)
                    {
                        Connexion.Insert("Update Students Set MonthlyPayment = " + newmonthprice.ResponseText + " Where ID =" + SID);
                    }

                }
              
                Commun.CheckDiscountAddClass(SID, this.Resources , 0 , -1);
                Connexion.FillDT(ref dtMain, query);
                DGUC.ItemsSource = dtMain.DefaultView;
                Search();
                MessageBox.Show(this.Resources["InsertedSucc"].ToString());
                TBStudentFName.Focus();
                TBStudentFName.Text = "";
                TBStudentLName.Text = "";
                CBStudentGender.SelectedIndex = -1;
                DPStudent.Text = "";
                CBStudentInsc.SelectedIndex = -1;
                CBStudentSpec.SelectedIndex = -1;
                CBStudentYear.SelectedIndex = -1;
                CBStudentLevel.SelectedIndex = -1;
                TBStudentNumber1.Text = "";
            }
            else
            {
                MessageBox.Show(this.Resources["MesssageBoxFillStudents"].ToString());
            }
           
        }

        private void CBStudentLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CBStudentSpec.SelectedIndex = -1;
            CBStudentYear.SelectedIndex = -1;
            DSStudent = new DataSet();
            dtStudentclass = new DataTable();
            dtStudentGroup = new DataTable();
            treeView.ItemsSource = null; 
            DataRowView row = (DataRowView)CBStudentLevel.SelectedItem;
            if (row != null)
            {
                Connexion.FillCB(ref CBStudentYear, "Select * from Years Where LevelID = " + row["ID"].ToString());
            }
        }

        private void CBStudentYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CBStudentSpec.SelectedIndex = -1;
            DataRowView row = (DataRowView)CBStudentYear.SelectedItem;
            DataRowView row2 = (DataRowView)CBStudentLevel.SelectedItem;

            if (row != null && row2 != null)
            {
                string IDYear = row["ID"].ToString();
                SqlConnection con = Connexion.Connect();
                if (row2["IsSpeciality"].ToString() == "0")
                {
                    dtStudentclass = new DataTable();
                    dtStudentGroup = new DataTable();
                    dtStudentclass.TableName = "Class";
                    Connexion.FillDT(ref dtStudentclass, "Select ID as ClassID ,Class.CName as ClassName from Class Where CYear = "+ IDYear);
                    Connexion.FillDT(ref dtStudentGroup, "Select Groups.GroupID as GroupID , Class.ID as ClassID ,0 as IsChecked, GroupName from Groups Join Class on  Groups.ClassID = Class.ID where Class.CYear = " + IDYear);

                    DSStudent = new DataSet();
                    DSStudent.Tables.Add(dtStudentclass);
                    DSStudent.Tables.Add(dtStudentGroup);
                    DataRelation relation = new DataRelation("ClassToGroup", dtStudentclass.Columns["ClassID"], dtStudentGroup.Columns["ClassID"]);
                    DSStudent.Relations.Add(relation);
                    treeView.ItemsSource = DSStudent.Tables["Class"].DefaultView;

                }
                else
                {
                    Connexion.FillCB(ref CBStudentSpec, "Select * from Specialities Where YearID = " + IDYear);
                }
            }
        }
        private void CheckBox_TreeView_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            DataRowView dataRow = (DataRowView)checkBox.DataContext;
            DataRow row = dataRow.Row;
            int classID = (int)row["ClassID"];
            // Set the IsChecked column value to 1 when the checkbox is checked
            row["IsChecked"] = 1;
            foreach (DataRow groupRow in dtStudentGroup.Rows)
            {
                if ((int)groupRow["ClassID"] == classID && groupRow != row)
                {
                    groupRow["IsChecked"] = 0;
                }
            }
        }

        private void CheckBox_TreeView_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            DataRowView dataRow = (DataRowView)checkBox.DataContext;
            DataRow row = dataRow.Row;

            // Set the IsChecked column value to 0 when the checkbox is unchecked
            row["IsChecked"] = 0;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CBStudentClass.SelectedIndex = -1;
        }

        private void CBStudentSpec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CBStudentYear.SelectedItem;
            DataRowView row2 = (DataRowView)CBStudentSpec.SelectedItem;
          
            treeView.ItemsSource = null;
            if (row != null && row2 != null)
            {
                string IDYear = row["ID"].ToString();
                SqlConnection con = Connexion.Connect();
                dtStudentclass = new DataTable();
                dtStudentclass.TableName = "Class";
                dtStudentGroup = new DataTable();
                Connexion.FillDT(ref dtStudentclass, "Select Class.ID as ClassID ,Class.CName as ClassName from Class  Join Class_Speciality on Class_Speciality.ID = Class.ID  Where   Class_Speciality.SpecID = " + row2["ID"].ToString() +" and CYear = " + IDYear);
                Connexion.FillDT(ref dtStudentGroup, "Select Groups.GroupID as GroupID , Class.ID as ClassID ,0 as IsChecked, GroupName from Groups Join Class on  Groups.ClassID = Class.ID  Join Class_Speciality on Class_Speciality.ID = Class.ID   where Class_Speciality.SpecID = " + row2["ID"].ToString() + " and  Class.CYear = " + IDYear);
                DSStudent = new DataSet();
                DSStudent.Tables.Add(dtStudentclass);
                DSStudent.Tables.Add(dtStudentGroup);
                DataRelation relation = new DataRelation("ClassToGroup", dtStudentclass.Columns["ClassID"], dtStudentGroup.Columns["ClassID"]);
                DSStudent.Relations.Add(relation);
                treeView.ItemsSource = DSStudent.Tables["Class"].DefaultView;
            }
        }

        private void TBStudentFName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Left)
                {
                    TBStudentLName.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    TBStudentLName.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                TBStudentLName.Focus();
            }
        }

        private void TBStudentLName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Left)
                {
                    TBStudentNumber1.Focus();
                    return;
                }
                else if (e.Key == Key.Right)
                {
                    TBStudentFName.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    TBStudentNumber1.Focus();
                    return;
                }
                else if (e.Key == Key.Left)
                {
                    TBStudentFName.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                TBStudentNumber1.Focus();
            }
        }

        private void TBStudentNumber1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Left)
                {
                    DPStudent.Focus();
                    return;
                }
                else if (e.Key == Key.Right)
                {
                    TBStudentFName.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    DPStudent.Focus();
                    return;
                }
                else if (e.Key == Key.Left)
                {
                    TBStudentFName.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                DPStudent.Focus();
            }
        }

        private void DPStudent_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Left)
                {
                    CBStudentGender.Focus();
                    return;
                }
                else if (e.Key == Key.Right)
                {
                    TBStudentNumber1.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    CBStudentGender.Focus();
                    return;
                }
                else if (e.Key == Key.Left)
                {
                    TBStudentNumber1.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                CBStudentGender.Focus();
            }
        }

        private void CBStudentGender_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Left)
                {
                    CBStudentLevel.Focus();
                    return;
                }
                else if (e.Key == Key.Right)
                {
                    DPStudent.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    CBStudentLevel.Focus();
                    return;
                }
                else if (e.Key == Key.Left)
                {
                    DPStudent.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                CBStudentLevel.Focus();
            }
        }

        private void CBStudentLevel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Left)
                {
                    CBStudentYear.Focus();
                    return;
                }
                else if (e.Key == Key.Right)
                {
                    CBStudentGender.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    CBStudentYear.Focus();
                    return;
                }
                else if (e.Key == Key.Left)
                {
                    CBStudentGender.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                CBStudentYear.Focus();
            }
        }

        private void CBStudentYear_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Left)
                {
                    CBStudentSpec.Focus();
                    return;
                }
                else if (e.Key == Key.Right)
                {
                    CBStudentLevel.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    CBStudentSpec.Focus();
                    return;
                }
                else if (e.Key == Key.Left)
                {
                    CBStudentLevel.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                CBStudentSpec.Focus();
            }
        }

        private void CBStudentSpec_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Left)
                {
                    CBStudentInsc.Focus();
                    return;
                }
                else if (e.Key == Key.Right)
                {
                    CBStudentYear.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    CBStudentInsc.Focus();
                    return;
                }
                else if (e.Key == Key.Left)
                {
                    CBStudentYear.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                CBStudentInsc.Focus();
            }
        }

        private void CBStudentInsc_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Left)
                {
                    CBStudentClass.IsDropDownOpen = true;
                    return;
                }
                else if (e.Key == Key.Right)
                {
                    CBStudentSpec.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    CBStudentClass.IsDropDownOpen = true;
                    return;
                }
                else if (e.Key == Key.Left)
                {
                    CBStudentSpec.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                CBStudentClass.IsDropDownOpen = true;
            }
        }

        private void CBStudentClass_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Connexion.Language() == 1)
            {
                if (e.Key == Key.Left)
                {
                    BtnAddStudent.Focus();
                    return;
                }
                else if (e.Key == Key.Right)
                {
                    CBStudentInsc.Focus();
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    BtnAddStudent.Focus();
                    return;
                }
                else if (e.Key == Key.Left)
                {
                    CBStudentInsc.Focus();
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                BtnAddStudent.Focus();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5 && TypeUC == "Students")
            {
                BtnAddStudent.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void BtnAttendancemonthly_Click(object sender, RoutedEventArgs e)
        {
            Report r = new Report();
            r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\AttendanceMonthly.frx");

            DataRowView row = (DataRowView)DGUC.SelectedItem;
            if (row == null)
            {
                return;
            }
            DataSet ds = new DataSet();
            DataTable dtStudentInfo = new DataTable();
            Connexion.FillDT(ref dtStudentInfo,
                        "Select Students.FirstName + ' ' + Students.LastName as StudentName ,'' as S1 , '' as S2 , '' as S3 ,'' as S4 from Class_Student Join Students on CLass_Student.StudentID = Students.ID where GroupID =  " + row["GID"].ToString());
            dtStudentInfo.TableName = "StudentInfo";
            DataTable dtGeneralInfo = new DataTable();
            dtGeneralInfo.TableName = "GeneralInfo";
            Connexion.FillDT(ref dtGeneralInfo, "Select Teacher.TLastName + ' ' + teacher.TFirstName as TeacherName , case When Class.MultipleGroups = 'Single' then Class.CName else Class.CName + ' ' + groups.GroupName end as GroupName , dbo.GetStudentsAmmount(Groups.GroupID) as TotalStudents From Groups Join Class on Class.ID = Groups.ClassID join Teacher on Teacher.ID = Class.TID Where Groups.GroupID = " + row["GID"].ToString());
            ds.Tables.Add(dtGeneralInfo);
            ds.Tables.Add(dtStudentInfo);
            r.RegisterData(ds);
            r.GetDataSource("GeneralInfo").Enabled = true;
            r.GetDataSource("StudentInfo").Enabled = true;

            if (Commun.FastReportEdit == 0)
            {
                r.Design();
                return;
            }
            else
            {
                r.Show();
            }
        }
    }
}
