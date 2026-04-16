using Gestion_De_Cours.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for ChangeGroup.xaml
    /// </summary>
    public partial class ChangeGroup : Window
    {
        string CID = "";
        string GID = "";
        public string AID { get; set; } 
        string TypeAdd = "";
        public ChangeGroup(string ID,   string Type, ref DataTable dt ) // 1 for Classic 
        {
            InitializeComponent();
            SetLang();
            TypeAdd = Type;
            if (Type == "1")
            {
                if(dt== null)
                {
                    LBAttendance.Visibility = Visibility.Collapsed;
                    CBAttendance.Visibility = Visibility.Collapsed;
                    this.Width = 900;
                    GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + ID);
                    AID = ID;
                    CID = Connexion.GetClassID(GID).ToString();
                    DataTable dt2 = new DataTable();
                    Connexion.FillDT(ref dt2, $"Select * from Groups Where ClassID = {CID} AND GroupID != {GID}");
                    DataRow extraRow = dt2.NewRow();
                    extraRow["GroupID"] = -1;
                    extraRow["GroupName"] = "New Student";
                    dt2.Rows.Add(extraRow);
                    CBGroup.ItemsSource = dt2.DefaultView;
                }
                else
                {
                    LBAttendance.Visibility = Visibility.Visible;
                    CBAttendance.Visibility = Visibility.Visible;
                    this.Width = 1150;
                    CBAttendance.ItemsSource = dt.DefaultView;
                }
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

        private void CBGroup_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            try
            {
                var combo = sender as DevExpress.Xpf.Editors.ComboBoxEdit;
                if (combo?.SelectedItem is DataRowView row)
                {
                    if(row == null)
                    {
                        return;
                    }
                    string GroupChange = row["GroupID"].ToString();

                    if (TypeAdd == "1")
                    {
                        if (GroupChange != "-1")
                        {
                            string query = "Select Students.FirstName + ' ' + Students.LastName  as Name ,Students.ID ,Students.Gender as Gender,'" + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "//MyPhotos\\" + "S' + Convert(Varchar(50),Students.ID)  + '.jpg' as Picture  " +
                               "from Students join Class_Student On Class_Student.StudentID = Students.ID " +
                               "join Groups On Groups.GroupID = Class_Student.GroupID " +
                               "Where Class_Student.ClassID = " + CID + " " +
                               "and groups.GroupID = " + GroupChange + " and Class_Student.Stopped = '0' " +
                               "Except  Select Students.FirstName + ' ' + Students.LastName as Name ,Students.Gender as Gender,Students.ID , '" + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "//MyPhotos\\" + "S' + Convert(Varchar(50),Students.ID)  + '.jpg' as Picture " +
                               "from Students join Class_Student On Class_Student.StudentID = Students.ID " +
                               "join Attendance_Student on Attendance_Student.StudentID = Students.ID " +
                               "Where Attendance_Student.ID =" + AID;
                            Connexion.FillCB(ref CBStudent, query);
                            SPTypeChange.Visibility = Visibility.Visible;
                            DateGroupChange.Visibility = Visibility.Visible;
                            FromLB.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            SPTypeChange.Visibility = Visibility.Collapsed;
                            DateGroupChange.Visibility = Visibility.Collapsed;
                            FromLB.Visibility = Visibility.Collapsed;
                            int yearid = Connexion.GetInt("Select CYear From Class Where ID = " + CID);
                            string query = Commun.GetStudentsNotInGroupQuery(GID);
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
        }
        private void btn_InsertGroupChange(object sender, RoutedEventArgs e)
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
                string DateAttendance = Connexion.GetString("Select Date from Attendance Where ID= " + AID);
                if (SGID != "-1")//group change and not inserting new student 
                {
                    if (TypeAdd == "1")
                    {
                        string StudentID = rowStudent["ID"].ToString();
                        if (OneTime.IsChecked == true)
                        {
                            string DateFrom = DateGroupChange.Text;
                            if (DateFrom == "")
                            {
                                MessageBox.Show("Please Add from session date ");
                                return;
                            }
                            if (MessageBox.Show("Do you want To Add This Student?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                if(!Commun.ChangeGroupOneTime(StudentID, GID, AID, DateFrom))
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            Commun.ChangeGroupForever(StudentID, GID, DateAttendance);
                        }
                    }
                }
                else
                {
                    string StudentID = rowStudent["ID"].ToString();
                    if (StudentID == "-1")
                    {
                       
                        StudentAdd SAdd = new StudentAdd("Add", "-1", Connexion.GetString("Select CYear from Class Where ID = " + CID));
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
                    Commun.AddStudentToClass(StudentID, GID, DateAttendance);
                }
                this.DialogResult = true;
                this.Close();

            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void OneTime_Checked(object sender, RoutedEventArgs e)
        {
            DateGroupChange.Visibility = Visibility.Visible;
            FromLB.Visibility = Visibility.Visible;
        }

        private void OneTime_Unchecked(object sender, RoutedEventArgs e)
        {
            DateGroupChange.Visibility = Visibility.Collapsed;
            FromLB.Visibility = Visibility.Collapsed ;
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void CBAttendance_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            try
            {
                var combo = sender as DevExpress.Xpf.Editors.ComboBoxEdit;
                if (combo?.SelectedItem is DataRowView row)
                {
                    GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + row["Name"].ToString());
                    AID = row["Name"].ToString(); ;
                    CID = Connexion.GetClassID(GID).ToString();
                    DataTable dt2 = new DataTable();
                    Connexion.FillDT(ref dt2, $"Select * from Groups Where ClassID = {CID} AND GroupID != {GID}");
                    DataRow extraRow = dt2.NewRow();
                    extraRow["GroupID"] = -1;
                    extraRow["GroupName"] = "New Student";
                    dt2.Rows.Add(extraRow);
                    CBGroup.ItemsSource = dt2.DefaultView;
                    CBStudent.ItemsSource = null;
                }
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }
    }
}
