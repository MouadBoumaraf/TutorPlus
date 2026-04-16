using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
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
using Gestion_De_Cours.Classes;
using Gestion_De_Cours.UControl;

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for GroupAttendance.xaml
    /// </summary>
    public partial class GroupAttendance : Window
    {
        string GID;
        DataTable dtTextColumns = new DataTable();
        DataTable dtMonthes = new DataTable();
        DataTable dt = new DataTable();
        string CID;
        public GroupAttendance(string GroupID)
        {
            try
            {
                InitializeComponent();
                dtTextColumns.Columns.Add("ID");
                dtTextColumns.Columns.Add("Index");
                SetLang();
                dtMonthes.Columns.Add("MonthId", typeof(int));
                dtMonthes.Columns.Add("MonthName", typeof(string));
                dtMonthes.Columns.Add("Amount", typeof(int));
                dtMonthes.Columns.Add("StartIndex", typeof(int));
                GID = GroupID;
                CID = Connexion.GetClassID(GID).ToString();
                Methods.FillDTAttendanceGroup(GroupID, ref dt, ref DGAttendance, ref dtTextColumns, ref dtMonthes, this.Resources);
                DataTable dt2 = new DataTable();
                Methods.FillDtAttendanceGroupPayment(GroupID, ref dt2);
                int dtStartColumn = dt.Columns.Count - 1;
                int rowcount = 1;
                foreach (DataRow row in dtMonthes.Rows)
                {
                    CheckBox checkBoxAll = new CheckBox();
                    switch (rowcount)
                    {
                        case 1:
                            LB1.Visibility = Visibility.Visible;
                            LB1.Content = row["MonthName"].ToString();
                            LB1.Width = int.Parse(row["Amount"].ToString()) * 26;
                            checkBoxAll.Name = "Check" + row["MonthName"].ToString();
                            checkBoxAll.Width = 190;
                            checkBoxAll.IsChecked = true;
                            checkBoxAll.Content = this.Resources[row["MonthName"].ToString()];
                            checkBoxAll.Checked += CheckBox_Checked;
                            checkBoxAll.Unchecked += CheckBox_Unchecked;
                            // Add the CheckBox to the ComboBox
                            CBMonthes.Items.Add(checkBoxAll);
                            break;
                        case 2:
                            LB2.Visibility = Visibility.Visible;
                            LB2.Content = row["MonthName"].ToString();
                            LB2.Width = int.Parse(row["Amount"].ToString()) * 26;

                            checkBoxAll.Name = "Check" + row["MonthName"].ToString();
                            checkBoxAll.Width = 190;
                            checkBoxAll.IsChecked = true;
                            checkBoxAll.Content = this.Resources[row["MonthName"].ToString()];
                            checkBoxAll.Checked += CheckBox_Checked;
                            checkBoxAll.Unchecked += CheckBox_Unchecked;

                            // Add the CheckBox to the ComboBox
                            CBMonthes.Items.Add(checkBoxAll);
                            break;
                        case 3:
                            LB3.Visibility = Visibility.Visible;
                            LB3.Content = row["MonthName"].ToString();
                            LB3.Width = int.Parse(row["Amount"].ToString()) * 26;
                            checkBoxAll.Name = "Check" + row["MonthName"].ToString();
                            checkBoxAll.Width = 190;
                            checkBoxAll.IsChecked = true;
                            checkBoxAll.Content = this.Resources[row["MonthName"].ToString()];
                            checkBoxAll.Checked += CheckBox_Checked;
                            checkBoxAll.Unchecked += CheckBox_Unchecked;
                            // Add the CheckBox to the ComboBox
                            CBMonthes.Items.Add(checkBoxAll);
                            break;
                        case 4:
                            LB4.Visibility = Visibility.Visible;
                            LB4.Content = row["MonthName"].ToString();
                            LB4.Width = int.Parse(row["Amount"].ToString()) * 26;
                            checkBoxAll.Name = "Check" + row["MonthName"].ToString();
                            checkBoxAll.Width = 190;
                            checkBoxAll.IsChecked = true;
                            checkBoxAll.Content = this.Resources[row["MonthName"].ToString()];
                            checkBoxAll.Checked += CheckBox_Checked;
                            checkBoxAll.Unchecked += CheckBox_Unchecked;
                            // Add the CheckBox to the ComboBox
                            CBMonthes.Items.Add(checkBoxAll);
                            break;
                        case 5:
                            LB5.Visibility = Visibility.Visible;
                            LB5.Content = row["MonthName"].ToString();
                            LB5.Width = int.Parse(row["Amount"].ToString()) * 26;
                            checkBoxAll.Name = "Check" + row["MonthName"].ToString();
                            checkBoxAll.Width = 190;
                            checkBoxAll.IsChecked = true;
                            checkBoxAll.Content = this.Resources[row["MonthName"].ToString()];
                            checkBoxAll.Checked += CheckBox_Checked;
                            checkBoxAll.Unchecked += CheckBox_Unchecked;
                            // Add the CheckBox to the ComboBox
                            CBMonthes.Items.Add(checkBoxAll);
                            break;
                        case 6:
                            LB6.Visibility = Visibility.Visible;
                            LB6.Content = row["MonthName"].ToString();
                            LB6.Width = int.Parse(row["Amount"].ToString()) * 26;
                            checkBoxAll.Name = "Check" + row["MonthName"].ToString();
                            checkBoxAll.Width = 190;
                            checkBoxAll.IsChecked = true;
                            checkBoxAll.Content = this.Resources[row["MonthName"].ToString()];
                            checkBoxAll.Checked += CheckBox_Checked;
                            checkBoxAll.Unchecked += CheckBox_Unchecked;
                            // Add the CheckBox to the ComboBox
                            CBMonthes.Items.Add(checkBoxAll);
                            break;
                        case 7:
                            LB7.Visibility = Visibility.Visible;
                            LB7.Content = row["MonthName"].ToString();
                            LB7.Width = int.Parse(row["Amount"].ToString()) * 26;
                            checkBoxAll.Name = "Check" + row["MonthName"].ToString();
                            checkBoxAll.Width = 190;
                            checkBoxAll.IsChecked = true;
                            checkBoxAll.Content = this.Resources[row["MonthName"].ToString()];
                            checkBoxAll.Checked += CheckBox_Checked;
                            checkBoxAll.Unchecked += CheckBox_Unchecked;
                            // Add the CheckBox to the ComboBox
                            CBMonthes.Items.Add(checkBoxAll);
                            break;
                        case 8:
                            LB8.Visibility = Visibility.Visible;
                            LB8.Content = row["MonthName"].ToString();
                            LB8.Width = int.Parse(row["Amount"].ToString()) * 26;
                            checkBoxAll.Name = "Check" + row["MonthName"].ToString();
                            checkBoxAll.Width = 190;
                            checkBoxAll.IsChecked = true;
                            checkBoxAll.Content = this.Resources[row["MonthName"].ToString()];
                            checkBoxAll.Checked += CheckBox_Checked;
                            checkBoxAll.Unchecked += CheckBox_Unchecked;
                            // Add the CheckBox to the ComboBox
                            CBMonthes.Items.Add(checkBoxAll);
                            break;
                        case 9:
                            LB9.Visibility = Visibility.Visible;
                            LB9.Content = row["MonthName"].ToString();
                            LB9.Width = int.Parse(row["Amount"].ToString()) * 26;
                            checkBoxAll.Name = "Check" + row["MonthName"].ToString();
                            checkBoxAll.Width = 190;
                            checkBoxAll.IsChecked = true;
                            checkBoxAll.Content = this.Resources[row["MonthName"].ToString()];
                            checkBoxAll.Checked += CheckBox_Checked;
                            checkBoxAll.Unchecked += CheckBox_Unchecked;
                            // Add the CheckBox to the ComboBox
                            CBMonthes.Items.Add(checkBoxAll);
                            break;
                        case 10:
                            LB10.Visibility = Visibility.Visible;
                            LB10.Content = row["MonthName"].ToString();
                            LB10.Width = int.Parse(row["Amount"].ToString()) * 26;
                            checkBoxAll.Name = "Check" + row["MonthName"].ToString();
                            checkBoxAll.Width = 190;
                            checkBoxAll.IsChecked = true;
                            checkBoxAll.Content = this.Resources[row["MonthName"].ToString()];
                            checkBoxAll.Checked += CheckBox_Checked;
                            checkBoxAll.Unchecked += CheckBox_Unchecked;
                            // Add the CheckBox to the ComboBox
                            CBMonthes.Items.Add(checkBoxAll);
                            break;
                        case 11:
                            LB11.Visibility = Visibility.Visible;
                            LB11.Content = row["MonthName"].ToString();
                            LB11.Width = int.Parse(row["Amount"].ToString()) * 26;
                            checkBoxAll.Name = "Check" + row["MonthName"].ToString();
                            checkBoxAll.Width = 190;
                            checkBoxAll.IsChecked = true;
                            checkBoxAll.Content = this.Resources[row["MonthName"].ToString()];
                            checkBoxAll.Checked += CheckBox_Checked;
                            checkBoxAll.Unchecked += CheckBox_Unchecked;
                            // Add the CheckBox to the ComboBox
                            CBMonthes.Items.Add(checkBoxAll);
                            break;
                        case 12:
                            LB12.Visibility = Visibility.Visible;
                            LB12.Content = row["MonthName"].ToString();
                            LB12.Width = int.Parse(row["Amount"].ToString()) * 26;
                            checkBoxAll.Name = "Check" + row["MonthName"].ToString();
                            checkBoxAll.Width = 190;
                            checkBoxAll.IsChecked = true;
                            checkBoxAll.Content = this.Resources[row["MonthName"].ToString()];
                            checkBoxAll.Checked += CheckBox_Checked;
                            checkBoxAll.Unchecked += CheckBox_Unchecked;
                            // Add the CheckBox to the ComboBox
                            CBMonthes.Items.Add(checkBoxAll);
                            break;
                    }
                    rowcount++;
                }
                for (int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
                {
                    DataRow row = dt.Rows[rowIndex];
                    string SID = row["ID"].ToString();
                    if (SID == "-1")
                    {
                        continue;
                    }
                    int total = Connexion.GetInt("Select dbo.CalcPriceSum(" + SID + "," + CID + ")");
                    // Loop through columns starting from the last column
                    for (int i = dt.Columns.Count - 1; i >= 0; i--)
                    {

                        // Access the column by index
                        DataColumn column = dt.Columns[i];

                        // Access the cell value
                        string pattern = @"\((.*?)\)";

                        // Create a Regex object and try to match the pattern
                        Regex regex = new Regex(pattern);

                        // Find the match
                        Match match = regex.Match(column.ColumnName);

                        // If a match is found, return the captured group (text between parentheses)
                        if (!match.Success)
                        {
                            continue;
                        }
                        string cellValue = row[column].ToString();
                        if (cellValue == "")
                        {
                            continue;
                        }
                        if (cellValue != "A" && cellValue != "P" && cellValue != "غ" && cellValue != "ح")
                        {
                            continue;
                        }
                        if (total >= 0)
                        {
                            dt2.Rows[rowIndex][i - 6] = "1";
                        }
                        else
                        {
                            dt2.Rows[rowIndex][i - 6] = "0";
                            string Ses = match.Groups[1].Value;
                            string AID = Connexion.GetString("Select ID from Attendance Where Session = " + Ses + " and GroupID = " + GID);
                            total += Connexion.GetInt("Select Price from Attendance_Student Where ID = " + AID + " and StudentID = " + SID);
                        }

                    }
                }
                for (int Columns = 1; Columns < dt2.Columns.Count; Columns++)
                {
                    dt.Columns.Add(Columns.ToString());
                    for (int Rows = 0; Rows < dt2.Rows.Count; Rows++)
                    {
                        dt.Rows[Rows][dtStartColumn + Columns] = dt2.Rows[Rows][Columns];
                    }
                }
                DGAttendance.ItemsSource = dt.DefaultView;
                DGAttendance.Height = Commun.ScreenHeight - 250;

                ContextMenu contextMenu = new ContextMenu();


                MenuItem menuItem = new MenuItem
                {
                    Header = this.Resources["PhoneNumber"].ToString(),
                    IsCheckable = true,
                    IsChecked = false
                };

                // Add a Click event handler to toggle column visibility
                menuItem.Click += (sender, e) =>
                {
                    double leftMargin = SPMargin.Margin.Left;

                    if (menuItem.IsChecked)
                    {
                        DGCPhone.Visibility = Visibility.Visible;
                   
                        SPMargin.Margin = new Thickness(DGCPhone.Width.Value + leftMargin, 20, 0, 0);
                    }
                    else
                    {
                        DGCPhone.Visibility = Visibility.Collapsed;
                        SPMargin.Margin = new Thickness(-DGCPhone.Width.Value + leftMargin, 20, 0, 0);
                    }
                };

                contextMenu.Items.Add(menuItem);



                // Attach the context menu to the DataGrid
                DGAttendance.ContextMenu = contextMenu;

            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var columnHeader = sender as DataGridColumnHeader;
            if (columnHeader != null)
            {
                int Index = columnHeader.DisplayIndex -1 ;
                for(int i = 0; i< dtTextColumns.Rows.Count; i++)
                {
                    if (int.Parse(dtTextColumns.Rows[i][1].ToString()) == Index)
                    {
                        //string AID = Connexion.GetInt(Index.ToString(), "Attendance", "ID", "Session", "GroupID", GID).ToString();
                        var AddS = new AttendanceAdd(dtTextColumns.Rows[i][0].ToString(), "Show" , "1");
                        AddS.Show();
                        return; 
                    }
                    else
                    {

                    }
                }
            }
        }

        private void DGAttendance_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DGAttendance.SelectedCells.Count > 0)
            {
                var cellInfo = DGAttendance.SelectedCells[0];
                var row = DGAttendance.ItemContainerGenerator.ItemFromContainer(
                    DGAttendance.ItemContainerGenerator.ContainerFromItem(cellInfo.Item)) as DataRowView;

                if (row == null)
                {
                    return;
                }
                string SID = row["ID"].ToString();
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

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {

        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {

        }

        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void AttendanceAdd_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you Sure you want to create a new attendance ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                int AID = Commun.InsertAttendance(GID, DateTime.Today.ToString().Replace("/", "-"));
                AttendanceAdd AttendancePage = new AttendanceAdd(AID.ToString(), "Add" , "1");
                AttendancePage.ShowDialog();
            }

        }

        private void PaymentButton_Click(object sender, RoutedEventArgs e)
        {
            EmptyPage Payment = new EmptyPage("StudentPayment3", "", Connexion.GetClassID(GID).ToString());
            Payment.ShowDialog();
        }

        private void StudentAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnAbsent_Click(object sender, RoutedEventArgs e)
        {
            var selectedCells = DGAttendance.SelectedCells;

            foreach (var selectedCell in selectedCells)
            {
                // Get the row index and column index of the selected cell
             
                int columnIndex = selectedCell.Column.DisplayIndex;
                DataGridRow dataGridRow = Commun.FindVisualParent<DataGridRow>(selectedCell.Item as DependencyObject);
                int selectedRowIndex = DGAttendance.Items.IndexOf(selectedCell.Item);

                if (dataGridRow != null)
                {
                    int rowIndex = dataGridRow.GetIndex();

                    // Get the corresponding DataRowView for the selected row
                    DataRowView row = DGAttendance.Items[rowIndex] as DataRowView;
                    string SID = row["ID"].ToString();
                    string AID = "";
                    for (int i = 0; i < dtTextColumns.Rows.Count; i++)
                    {
                        if (int.Parse(dtTextColumns.Rows[i][1].ToString()) == columnIndex)
                        {
                            //string AID = Connexion.GetInt(Index.ToString(), "Attendance", "ID", "Session", "GroupID", GID).ToString();
                            AID = dtTextColumns.Rows[i][0].ToString();
                            break;
                        }
                    }
                    if(AID == "")
                    {
                        return;
                    }
                    Connexion.Insert("Update Class_Students Set Status = 0 Where ID = " + AID + " and StudentID  = " + SID);
                }
            }
        }

        private void BtnPresent_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                // Get the name of the CheckBox
                string checkBoxName = checkBox.Name;
                if (checkBoxName == "CheckBoxAll")
                {
                    foreach (var item in CBMonthes.Items)
                    {
                        if (item is CheckBox checkBox2 && checkBox2 != CheckBoxAll)
                        {
                            checkBox2.IsChecked = true;
                        }
                    }
                }
                else
                {
                    string monthName = checkBoxName.Substring("Check".Length);
                    DataRow[] matchingRows = dtMonthes.Select($"MonthName = '{monthName}'");
                    int rowIndex = dtMonthes.Rows.IndexOf(matchingRows[0])+ 1;
                    int startIndex = Convert.ToInt32(matchingRows[0]["StartIndex"].ToString()) ;
                    int amount = Convert.ToInt32(matchingRows[0]["Amount"].ToString()) + startIndex;
                    string labelName = "LB" + rowIndex;

                    // Find the Label control with the specified name
                    Label label = FindName(labelName) as Label;

                    // If the Label control is found, set its visibility to Collapsed
                    if (label != null)
                    {
                        label.Visibility = Visibility.Visible;
                    }
                    // Hide columns starting from the startIndex
                    for (int i = startIndex; i < amount; i++)
                    {
                        DGAttendance.Columns[i].Visibility = Visibility.Visible;

                    }
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                // Get the name of the CheckBox
                string checkBoxName = checkBox.Name;
                if (checkBoxName == "CheckBoxAll")
                {
                    foreach (var item in CBMonthes.Items)
                    {
                        if (item is CheckBox checkBox2 && checkBox2 != CheckBoxAll)
                        {
                            checkBox2.IsChecked = false;
                        }
                    }
                }
                else
                {
                    string monthName = checkBoxName.Substring("Check".Length);
                    DataRow[] matchingRows = dtMonthes.Select($"MonthName = '{monthName}'");
                    int rowIndex = dtMonthes.Rows.IndexOf(matchingRows[0]) +1;
                    int startIndex = Convert.ToInt32(matchingRows[0]["StartIndex"].ToString());
                    int amount = Convert.ToInt32(matchingRows[0]["Amount"].ToString()) + startIndex;
                    string labelName = "LB" + rowIndex;

                    // Find the Label control with the specified name
                    Label label = FindName(labelName) as Label;

                    // If the Label control is found, set its visibility to Collapsed
                    if (label != null)
                    {
                        label.Visibility = Visibility.Collapsed;
                    }
                    // Hide columns starting from the startIndex
                    for (int i = startIndex ; i < amount ; i++)
                    {
                        DGAttendance.Columns[i].Visibility = Visibility.Collapsed;
                        
                    }
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TBSearch.Text == "")
            {
               
                string Condition = "1 > 0 ";
                dt.DefaultView.RowFilter = Condition;
                return;
            }
            if (TBSearch.Text.Last() == '$')
            {
                if (Connexion.IFNULL("Select * from Students Where BarCode = '" + TBSearch.Text + "'"))
                {
                    MessageBox.Show("Barcode Wrong");
                    TBSearch.Text = "";
                }
                else
                {

                    string SID = Connexion.GetInt(TBSearch.Text, "Students", "ID", "BarCode").ToString();
                    if (Connexion.IFNULL("Select * from Class_Student Where StudentID = " + SID + " and GroupID = " + GID))
                    {
                        MessageBox.Show("This Student isn't registered in this group");
                        return;
                    }
                    string Condition = "1 > 0 ";
                    //   Condition += "And (Name Like '%" + Regex.Replace(CodebarTxt.Text, @"\s+", " ") + "%' OR RName Like '%" + Regex.Replace(CodebarTxt.Text, @"\s+", " ") + "%') ";
                    string searchText = Regex.Replace(TBSearch.Text, @"\s+", " ").Replace("'", "''");

                    // Construct the filter expression
                    Condition += $" and (BarCode ='{searchText}' ) ";

                    // Apply the filter to the DataView
                    dt.DefaultView.RowFilter = Condition;
                    return;



                }

            }
            string text = TBSearch.Text;
            bool isNumeric = int.TryParse(text, out _);

            if (isNumeric)
            {

            }
            else
            {

                string Condition = "1 > 0 ";
                //   Condition += "And (Name Like '%" + Regex.Replace(CodebarTxt.Text, @"\s+", " ") + "%' OR RName Like '%" + Regex.Replace(CodebarTxt.Text, @"\s+", " ") + "%') ";
                string searchText = Regex.Replace(TBSearch.Text, @"\s+", " ").Replace("'", "''");

                // Construct the filter expression
                Condition += $" and (Name LIKE '%{searchText}%' OR RName LIKE '%{searchText}%') ";

                // Apply the filter to the DataView
                dt.DefaultView.RowFilter = Condition;

            }
        }
    }
}
