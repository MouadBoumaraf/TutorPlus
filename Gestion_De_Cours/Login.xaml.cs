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
using System.Windows.Shapes;
using System.Data.SqlClient;
using Gestion_De_Cours.Classes;
using System.IO;
using System.Data;
using System.Windows.Threading;
using System.Security.Principal;
using System.Reflection;
using System.Deployment.Application;
using System.Drawing.Imaging;
using System.Drawing;


namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    /// 
    //HISTORY 
    //0  INSERT STUDENT 
    //1  INSERT TEACHER
    //2  INSERT WORKER 
    //3 INSERT CLASS
    //4 INSERT GROUP
    //5 INSERT ATTENDANCE 
    //6 INSERT STUDENT IN GROUP 
    //7 INSERT PAYMENT TEACHER 
    //8 INSERT STUDENT PAYMENT
    //9 INSERT CashRegisterExtra PAYMENT
    //10 INSERT DISCOUNT 
    //11 INSERT JUSTIFICATION 
    //12 INSERT FORMATION
    //13 INSERT FORMATION PAYMENT
    //14 INSERT FORMATION ATTENDANCE
    //15 Insert Extra_Students
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            try
            {

                string EcoleData = @"C:\ProgramData\EcoleSetting\SqlSettings.txt";
                string EcolePhotos = @"C:\ProgramData\EcoleSetting\EcolePhotos";
                string Patch = Directory.GetCurrentDirectory() + @"\Patches\Patch.txt";
                string EcolePrint = @"C:\ProgramData\EcoleSetting\EcolePrint";
                if(File.Exists(EcolePrint + @"\PaymentStudentAR.frx"))
                {
                    File.Delete(EcolePrint + @"\PaymentStudentAR.frx");
                    File.Delete(EcolePrint + @"\AttendanceFastReportAR.frx");
                    File.Delete(EcolePrint + @"\FastReportCashRegisterAR.frx");
                }

                if (!File.Exists(EcolePrint))
                {
                    Directory.CreateDirectory(EcolePrint);
                }
              
                string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\FastReport");
                // Copy files to the destination folder
                foreach (string file in files)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    string destPath = System.IO.Path.Combine(EcolePrint, fileName);

                    if (!File.Exists(destPath))
                    {
                        // File doesn't exist in the destination, so copy it
                        File.Copy(file, destPath);
                    }
                }


                // string path = @"C:\ProgramData\EcoleSetting\Mouathfile.txt";
                //if (File.Exists(path))
                //{
               
                if (File.Exists(EcoleData))
                {
                    string[] lines = File.ReadAllLines(EcoleData);
                    Connexion.DS = lines[1];
                    Connexion.UserName = lines[2];
                    Connexion.password = lines[3];
                    Connexion.DB = "master";
                    Connexion.SetConnect();
                }
                else
                {
                    DBConfig df = new DBConfig();
                    df.ShowDialog();
                }
                Connexion.FillCB(ref ComboDB, "SELECT name FROM sys.databases where name != 'master' and name!= 'tempdb' and name != 'model' and name!= 'msdb'");

                string path = @"C:\ProgramData\EcoleSetting\Last.txt";
                if (!File.Exists(path))
                {
                    UserName.Text = "Admin";
                    Password.Password = "Admin";

                }
                else
                {
                    string[] linesLast = File.ReadAllLines(path);
                    UserName.Text = linesLast[0];
                    Password.Password = linesLast[1];
                    string databaseName = linesLast[2];
                    DataView view = (DataView)ComboDB.ItemsSource;
                    foreach (DataRowView row in view)
                    {
                        if (row["name"].ToString() == databaseName)
                        {
                            ComboDB.SelectedItem = row;

                            break;
                        }
                    }

                }

                if (ComboDB.SelectedIndex == -1 )
                {
                    ComboDB.SelectedIndex = ComboDB.Items.Count - 1;
                }
               
                DataTable dt = new DataTable();

                /*if (!Directory.Exists(EcolePhotos))
                {
                    MessageBox.Show("Not Created.");
                    Directory.CreateDirectory(EcolePhotos);
                    if (Connexion.GetInt("Select Count(*) from EcoleSetting") == 0)
                    {
                        Connexion.Insert("Insert into EcoleSetting (PhotoFile) Values (N'" + EcolePhotos + "')");
                    }
                    else
                    {
                        Connexion.Insert("Update EcoleSetting Set PhotoFile = N'" + EcolePhotos + "')");
                    }
                }*/
            
            }
            catch(Exception e)
            {
                Methods.ExceptionHandle(e);
                MessageBox.Show(e.ToString());
            }
        }


 
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)ComboDB.SelectedItem;
                if (row == null)
                {
                    return;
                }
                Connexion.SetDB(row["name"].ToString());
                Connexion.SetConnect();

                string Pass = "";
                if (Password.Visibility == Visibility.Visible)
                {
                    Pass = Password.Password.ToLower();
                }
                else
                {
                    Pass = PasswordUnmask.Text.ToLower();
                }

                string WID = Connexion.CheckUser(UserName.Text.ToLower(), Pass, "@");
                if (WID != "-1")
                {
                    var dialog = new Backup("Enter New Password ", 0);
                    if (dialog.ShowDialog() == true)
                    {
                        Connexion.Insert("Update Users Set Password =N'!" + dialog.ResponseText + "' where UserName = N'" + UserName.Text.ToLower() + "' and ID = " + WID);
                    }
                }
                else
                {
                    WID = Connexion.CheckUser(UserName.Text.ToLower(), Pass, "!");
                    if (WID != "-1")
                    {
                        Connexion.WorkerID = WID;
                        string path = @"C:\ProgramData\EcoleSetting\Last.txt";
                       
                        string content = $"{UserName.Text}\n{Pass}\n{row["name"].ToString()}";
                        // Write the content to the file (this will create the file if it doesn't exist)
                        File.WriteAllText(path, content);
                        var main = new MainWindow();
                        main.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة");
                    }
                }

            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Connexion.SetDB(ComboDB.Text);
        }

      


        private void checkBox_showPassword_Checked(object sender, RoutedEventArgs e)
        {
            Password.Visibility = Visibility.Collapsed;
            PasswordUnmask.Visibility = Visibility.Visible;
            PasswordUnmask.Text = Password.Password; 
        }

        private void checkBox_showPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            Password.Visibility = Visibility.Visible;
            PasswordUnmask.Visibility = Visibility.Collapsed;
            Password.Password = PasswordUnmask.Text;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.B)
            {
                var dialog = new Backup("Select DataBase", 2);
                if (dialog.ShowDialog() == true)
                {
                    Connexion.RestoreDataBase(dialog.ResponseText.Substring(0,dialog.ResponseText.Length - 7));
                    Connexion.FillCB(ref ComboDB, "SELECT name,( name + ' '+ convert(varchar(500),create_date) ) as NameSeen  FROM sys.databases where name != 'master' and name!= 'tempdb' and name != 'model' and name!= 'msdb'");
                    MessageBox.Show("تم استعادت قاعدة البيانات  بنجاح");
                }
            }
        }
    }
}
