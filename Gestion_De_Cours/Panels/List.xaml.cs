using Gestion_De_Cours.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for List.xaml
    /// </summary>
    public partial class List : Window
    {
        DataTable dt =  new DataTable();
        string quer = "";
        string GID = "";
        string CID = "";
        string YearID = "";
        int totalChecked = 0;
        int AttendID;
        public List(string query,string GroupID,int AID = -1)
        {
            InitializeComponent();
            quer = query;
            GID = GroupID;
            AttendID = AID;
            CID = Connexion.GetClassID(GID).ToString();
            YearID = Connexion.GetString("Select CYear from Class Where ID = " + CID);
            Connexion.FillDT(ref dt, query);
            DG.DataContext = dt.DefaultView;
            StudentAmmount.Text = totalChecked.ToString();
            DateEntry.SelectedDate = DateTime.Today;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string Condition = "1 > 0 ";
                if (TBSearch.Text != "")
                {

                    if (TBSearch.Text.Last() == '$')
                    {
                        Condition += "And barCode ='" + TBSearch.Text + "' ";
                    }
                    else
                    {
                        Condition += "And (NameSearch Like '%" + Commun.ReplaceArabicName(Regex.Replace(TBSearch.Text, @"\s+", " ")) + "%' OR RNameSearch Like '%" + Commun.ReplaceArabicName(Regex.Replace(TBSearch.Text, @"\s+", " ")) + "%' ";
                        Condition += "OR Name Like '%" + Commun.ReplaceArabicName(Regex.Replace(TBSearch.Text, @"\s+", " ")) + "%' OR RName Like '%" + Commun.ReplaceArabicName(Regex.Replace(TBSearch.Text, @"\s+", " ")) + "%') ";
                    }

                }
                dt.DefaultView.RowFilter = Condition;
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            try
            {
                int countforstudent = 0;
                if(DateEntry.Text == "")
                {
                    MessageBox.Show("Please enter the entry date");
                    return;
                }
                string date = DateEntry.Text.Replace("/", "-");
                DataTable dtdates = new DataTable();
                Connexion.FillDT(ref dtdates, $"SELECT id,[date] FROM Attendance WHERE CONVERT(date, [Date], 105) >= CONVERT(date, '{date}', 105) and GroupID= {GID}");
                int InsertOldSessions = 0;
                if (dtdates.Rows.Count >= 1)// just check message once for all  
                {
                    string message = string.Format((string)this.Resources["SessionsAfterSelectedDate"].ToString(), date);
                    if (MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.None, Commun.GetAllignMessageBox()) == MessageBoxResult.Yes)
                    {
                        InsertOldSessions = 1;
                    }
                }
                //here also check studentsif they were already inserted  in this group before 
                foreach (DataRow row in dt.Rows)
                {
                    if (row["IsChecked"] != DBNull.Value && row["IsChecked"].ToString() == "True")
                    {
                        string StudentID = row["ID"].ToString();
                        Commun.AddStudentToClass(StudentID, GID, date, InsertOldSessions);
                        countforstudent++;
                    }
                }
                MessageBox.Show(countforstudent + " Students are inserted");
                this.Close();
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void Button_Clic_addStudent(object sender, RoutedEventArgs e)
        {
            try
            {
                StudentAdd SAdd = new StudentAdd("Add", "-1", YearID);
                string sid = "";
                if (SAdd.ShowDialog() == true)
                {
                    sid = SAdd.ResponseText;
                    string PhoneNumber = Connexion.GetString("Select PhoneNumber from Students Where ID = " + sid);
                    string Parentnumber = Connexion.GetString("Select ParentNumber from Students Where ID = " + sid);
                    string Subjects = Connexion.GetString("Select dbo.GetStudentSubjects(ID) as Subjects from Students Where ID = " + sid);
                    string Name = Connexion.GetString("Select FirstName + ' '+ lastname from Students Where ID = " + sid);
                    string adress = Connexion.GetString("Select Adress from Students Where ID = " + sid);
                    string Level = Connexion.GetString("Select Level from Students Where ID = " + sid);
                    string NameSearch = Connexion.GetString("Select dbo.ReplaceArabicName(Students.FirstName) + ' ' + dbo.ReplaceArabicName(Students.LastName) from Students Where ID = " + sid);
                    string RNameSearch = Connexion.GetString("Select  dbo.ReplaceArabicName(Students.LastName) + ' ' + dbo.ReplaceArabicName(Students.FirstName) from Students Where ID = " + sid);
                    string FName = Connexion.GetString("Select FirstName from Students Where ID = " + sid);
                    string Lname = Connexion.GetString("Select LastName from Students Where ID = " + sid);
                    string IsChecked = "True";
                    string BarCode = Connexion.GetString("Select BarCode from Students Where ID = " + sid);
                    string Gender = Connexion.GetString("Select Gender from Students Where ID = " + sid);
                    string Name1 = Connexion.GetString("Select lastname + ' '+ FirstName from Students Where ID = " + sid);
                    string RName = Connexion.GetString("Select FirstName + ' '+ lastname from Students Where ID = " + sid);
                    DataRow newRow = dt.NewRow();

                    // Populate the DataRow with values
                    newRow["ID"] = sid;
                    newRow["PhoneNumber"] = PhoneNumber;
                    newRow["ParentNumber"] = Parentnumber;
                    newRow["Subjects"] = Subjects;
                    newRow["Name"] = Name;
                    newRow["adress"] = adress;
                    newRow["LevelID"] = Level;
                    newRow["NameSearch"] = NameSearch;
                    newRow["RNameSearch"] = RNameSearch;
                    newRow["FirstName"] = FName;
                    newRow["LastName"] = Lname;
                    newRow["IsChecked"] = IsChecked;
                    newRow["BarCode"] = BarCode;
                    newRow["Gender"] = Gender;
                    newRow["Name1"] = Name1;
                    newRow["RName"] = RName; // Adding "Name1" as FullName
                    dt.Rows.InsertAt(newRow, 0);
                    totalChecked++;
                    StudentAmmount.Text = totalChecked.ToString();
                }
                else
                {
                    MessageBox.Show("No Student Was Added");
                    return;
                }
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void TBSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {

            
            if (e.Key == Key.Enter)
            {
                foreach (DataRowView rowView in dt.DefaultView)
                {
                    DataRow row = rowView.Row;
                    row["IsChecked"] = true;
                    totalChecked++;
                }
                
                DG.DataContext = dt.DefaultView;
                
                DG.Items.Refresh();
                TBSearch.Text = "";
                StudentAmmount.Text = totalChecked.ToString();
            }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleButton toggleButton = sender as ToggleButton;

                if (toggleButton != null)
                {
                    // Get the DataGridRow containing the ToggleButton
                    DataGridRow row = DataGridRow.GetRowContainingElement(toggleButton);

                    if (row != null)
                    {
                        // Get the data item (row's data) bound to the DataGridRow
                        DataRowView rowView = row.Item as DataRowView;
                        if (rowView != null)
                        {
                            // Access the underlying DataRow
                            DataRow rowData = rowView.Row;
                            if (rowData["IsChecked"].ToString() == "False")
                            {
                                rowData["IsChecked"] = "True";
                                totalChecked++;
                            }
                            else
                            {
                                rowData["IsChecked"] = "False";
                                totalChecked--;
                            }
                            // Modify values in the DataRow

                            // Optionally refresh the DataGrid to show the changes
                            DG.Items.Refresh();
                            StudentAmmount.Text = totalChecked.ToString();
                        }
                    }
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void DG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)DG.SelectedItem;
                if (row != null)
                {
                    StudentAdd s = new StudentAdd("Show", row["ID"].ToString());
                    s.Show();
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }
    }
}
