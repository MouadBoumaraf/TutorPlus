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
using Gestion_De_Cours.Classes; 

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for ScanningCodeBar.xaml
    /// </summary>
    public partial class ScanningCodeBar : Window
    {
        string GID = "";
        string AID = "";
        string SID = "";
        public ScanningCodeBar()
        {
            InitializeComponent();
            SetLang();
            CodebarTxt.Focus(); 
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                AID = "";
                SPPresent.Visibility = Visibility.Collapsed;
                FName.Text = "";
                LName.Text = "";
                Level.Text = "";
                Year.Text = "";
                Speciality.Text = "";
                BirthDate.Text = "";
                Pic.Source = null;
                SID = "";
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
                        DataTable dt = new DataTable();
                        Connexion.FillDT(ref dt, "Select * From Students Where BarCode ='" + CodebarTxt.Text + "'");
                        FName.Text = dt.Rows[0]["FirstName"].ToString();
                        LName.Text = dt.Rows[0]["LastName"].ToString();
                        Level.Text = Connexion.GetString(dt.Rows[0]["Level"].ToString(), "Levels", "Level");
                        Year.Text = Connexion.GetString(dt.Rows[0]["Year"].ToString(), "Years", "Year");
                        if (dt.Rows[0]["Speciality"].ToString() != "")
                        {
                            Speciality.Text = Connexion.GetString(dt.Rows[0]["Speciality"].ToString(), "Specialities", "Speciality");
                        }
                        BirthDate.Text = dt.Rows[0]["Birthdate"].ToString();
                        SID = dt.Rows[0]["ID"].ToString();
                        Methods.InsertPicwithGender(ref Pic, Connexion.GetImagesFile() + "\\S" + SID + ".jpg", dt.Rows[0]["Gender"].ToString());
                        DataTable dtgroups = new DataTable();
                        Connexion.FillDT(ref dtgroups, "" +
                            "Select Groups.GroupID , groups.GroupName as GroupName " +
                            "from Groups " +
                            "Join Class_Student " +
                            "On Groups.GroupID = Class_Student.GroupID " +
                            "Where StudentID = " + SID);
                        for (int i = 0; i < dtgroups.Rows.Count; i++)
                        {
                            DataTable dtattendance = new DataTable();
                            Connexion.FillDT(ref dtattendance, "Select * from Attendance Where GroupID = " + dtgroups.Rows[i]["GroupID"].ToString() + " order by Attendance.Session");
                            //check if empty dataattendance
                            string Datetoday = DateTime.Now.ToShortDateString().Replace("/", "-");
                            if (Datetoday == dtattendance.Rows[0]["Date"].ToString())
                            {
                                string GTimeBeg = Connexion.GetString(dtattendance.Rows[0]["GTID"].ToString(), "Class_Time", "TimeStart");
                                string GTimeEnd = Connexion.GetString(dtattendance.Rows[0]["GTID"].ToString(), "Class_Time", "TimeEnd");
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
                                    GID = dtattendance.Rows[0]["GroupID"].ToString();
                                    AID = dtattendance.Rows[0]["ID"].ToString();
                                    int CID = Connexion.GetClassID(GID);
                                    string GroupName = Connexion.GetString(CID.ToString(), "Class", "CName");
                                    GroupName += " " + dtgroups.Rows[i]["GroupName"].ToString();
                                    LBPresent.Content = "Present in '" + GroupName + "'?";
                                    SPPresent.Visibility = Visibility.Visible;
                                    return;
                                }
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

        private void Present_Click(object sender, RoutedEventArgs e)
        {
            Connexion.Insert(
                "Update Attendance_Student" +
                " Set Status =  1 " +
                "Where ID = '" + AID + "' " +
                "And StudentID = " + SID );
            MessageBox.Show("Done");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (SID != "")
            {
                var AddW = new StudentAdd("Show", SID);
                AddW.Show();
            }
        }
        private void SetLang()
        {
            ResourceDictionary ResourceDic = new ResourceDictionary();
            if (Connexion.Language() == 1)
            {
                ResourceDic.Source = new Uri("../Dictionary\\AR.xaml", UriKind.Relative);
                this.FlowDirection = FlowDirection.RightToLeft;

                this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");

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
    }
}
