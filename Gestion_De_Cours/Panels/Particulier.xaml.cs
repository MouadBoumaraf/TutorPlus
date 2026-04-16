using Gestion_De_Cours.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
    /// Interaction logic for Particulier.xaml
    /// </summary>
    public partial class Particulier : Window
    {
        int pid = -1;
        bool TriggerSelection = true; 
      
        public Particulier( int id )
        {
            InitializeComponent();
            SetLang();
            Connexion.FillCB(ref TComboBox, "Select ID,TFirstName + ' ' + TLastName As Name ,TGender,'" + Connexion.GetImagesFile() + "\\" + "T' + Convert(Varchar(50),ID)  + '.jpg' as Picture From Teacher where status = 1");
            Connexion.FillCB(ref CBStudent, "SELECT * ,(FirstName + ' ' + LastName) as Name  , '" + Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "//MyPhotos\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture From Students  Where Status = 1");

            DGTime.PreviewMouseLeftButtonDown += DataGrid_PreviewMouseLeftButtonDown;

            if (id != -1)
            {
                pid = id;
                Connexion.FillDTItem("Rooms", ref CRoom);
                TComboBox.SelectedValue = Connexion.GetInt(pid.ToString(), "Particulier", "TID");
                CBStudent.SelectedValue = Connexion.GetInt("Select SID from Particulier_Student Where PID = " + pid);
                TBName.Text = Connexion.GetString(pid.ToString(), "Particulier", "Name");
                CPrice.Text = Connexion.GetString(pid.ToString(), "Particulier", "Price");
                TPayment.Text = Connexion.GetString(pid.ToString(), "Particulier", "TPrice");
                TPaymentMethod.Text = Connexion.GetString(pid.ToString(), "Particulier", "Method");
                Connexion.FillDG(ref DGTime, "Select * from Particulier_Time where PID = " + pid);
                Connexion.FillDG(ref DGPay, "Select ID , CID as PID , Date , Price ,'Student' as Type " +
                    "from StudentPayment Where Type = 2 and CID = " + pid + " union Select ID,PID,Total as Price,Date , 'Teacher' as Type from Particulier_TPay Where PID = " + pid);
            }
        }
        private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the clicked cell
            var cellInfo = DGTime.CurrentCell;

            if (cellInfo != null)
            {
                // Enable editing for the clicked cell
                DataGridCell cell = GetCell(cellInfo.Item, cellInfo.Column);
                if (cell != null)
                {
                    cell.IsEditing = true;
                }
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var editedItem = e.Row.Item as DataRowView; // Replace YourItemType with the actual type of your data items
                var editedColumn = e.Column.DisplayIndex; // Get the header (column name) of the edited column 
                string TimeStart = Connexion.GetString("Select TimeStart From Particulier_Time Where ID" + editedItem["ID"].ToString());
                string TimeEnd = Connexion.GetString("Select TimeEND From Particulier_Time Where ID" + editedItem["ID"].ToString());
                double hoursDifference = GetTime(TimeStart, TimeEnd);
                // Update the corresponding property in the database record
                if (editedColumn == 2)
                {
                    var editedValue = (e.EditingElement as TextBox).Text;
                    if (editedValue == "")
                    {
                        return;
                    }
                    if (Connexion.GetString("Select Status From Particulier_Time Where ID" + editedItem["ID"].ToString()) == "1")
                    {
                        double NewHoursDifference = GetTime(editedValue, TimeEnd);
                        NewHoursDifference -= hoursDifference;
                        AddPaymentChangeTime(hoursDifference, "+");
                    }
                    Connexion.Insert("Update Particulier_Time Set TimeStart = '" + editedValue + "' Where ID = " + editedItem["ID"].ToString());
                }
                else if (editedColumn == 3)
                {
                    var editedValue = (e.EditingElement as TextBox).Text;
                    if (Connexion.GetString("Select Status From Particulier_Time Where ID" + editedItem["ID"].ToString()) == "1")
                    {
                        double NewHoursDifference = GetTime(editedValue, TimeEnd);
                        NewHoursDifference -= hoursDifference;
                        AddPaymentChangeTime(hoursDifference, "+");
                    }
                    if (editedValue == "")
                    {
                        return;
                    }
                    Connexion.Insert("Update Particulier_Time Set TimeENd = '" + editedValue + "' Where ID = " + editedItem["ID"].ToString());
                }
            }
        }

        private DataGridCell GetCell(object dataItem, DataGridColumn column)
        {
            if (dataItem == null || column == null) return null;

            var cellContent = column.GetCellContent(dataItem);
            if (cellContent == null) return null;

            return (DataGridCell)cellContent.Parent;
        }
        private  void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Tab.SelectedIndex == 0)
            {
                this.Height = 280;
                this.Width = 650;
            }
            else if (Tab.SelectedIndex == 1)
            {
                TriggerSelection = false;
                this.Height = 400;
                this.Width = 900;
                Thread backgroundThread = new Thread(PerformBackgroundTask);
                backgroundThread.Start();
              
            }
            else if (Tab.SelectedIndex == 2)
            {
                this.Height = 450;
                this.Width = 600;
            }
        }

        private void PerformBackgroundTask()
        {
            // Simulate some work
            Thread.Sleep(1000);

            // Use Dispatcher to update UI from the background thread
            Dispatcher.Invoke(() =>
            {
                TriggerSelection = true;
            });
        }

        private void CBStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
                    dt.Columns.Add("GID", typeof(int));
                    if(Date.Text == "")
                    {
                        MessageBox.Show("Please fill in the date");
                        return; 
                    }
                    DateTime selectedDate = Date.SelectedDate ?? DateTime.Now;
                    int dayIndex = (int)selectedDate.DayOfWeek;

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
                        int[] r = Connexion.CheckTimeHour(dr[0].ToString(), dayIndex, int.Parse(rowRoom["ID"].ToString()));
                        dr[1] = r[0];
                        dr[2] = r[1];
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
                if (Date.Text == "")
                {
                    MessageBox.Show("Please fill in the date");
                    return;
                }
                DateTime selectedDate = Date.SelectedDate ?? DateTime.Now;
                int dayIndex = (int)selectedDate.DayOfWeek;
                DataRowView row = (DataRowView)HFrom.SelectedItem;
                if (row != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Hour", typeof(string));
                    dt.Columns.Add("Status", typeof(int));
                    dt.Columns.Add("GID", typeof(int));

                    if (row["Status"].ToString() == "2")
                    {
                        string groupName = Connexion.GetString(row["GID"].ToString(), "Groups", "GroupName", "GroupID");
                        MessageBox.Show(this.Resources["TimeTaken"].ToString() + " " + groupName);
                        HFrom.SelectedIndex = -1;
                    }
                    else if (row["Status"].ToString() == "1")
                    {
                        DataRowView rowroom = (DataRowView)CRoom.SelectedItem;
                        SqlConnection con = Connexion.Connect();
                        SqlCommand CommandID = new SqlCommand("Select  CONVERT(INT,SUBSTRING(TimeEnd,4,2)) From Class_Time " +
                            "Where IDroom = " + rowroom["ID"].ToString() + " " +
                            "and day =  " + dayIndex + " " +
                            "and CONVERT(INT,SUBSTRING(TimeEnd,0,3)) = " + row["Hour"].ToString() + " " +
                            "and GID = '" + row["GID"].ToString() + "'",
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
                                dr[2] = int.Parse(row["GID"].ToString());
                            }
                            else if (result > i)
                            {
                                dr[1] = 2;
                                dr[2] = int.Parse(row["GID"].ToString());
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
                string gid = "0";
                DataRowView row2 = (DataRowView)HFrom.SelectedItem;
                if (Date.Text == "")
                {
                    MessageBox.Show("Please fill in the date");
                    return;
                }
                DateTime selectedDate = Date.SelectedDate ?? DateTime.Now;
                int dayIndex = (int)selectedDate.DayOfWeek;

                if (row != null && row2 != null && rowRoom != null)
                {
                    if (row["Status"].ToString() == "2")
                    {
                        string groupName = Connexion.GetString(row["GID"].ToString(), "Groups", "GroupName", "GroupID");
                        MessageBox.Show(this.Resources["TimeTaken"].ToString() + " " + groupName);
                        HFrom.SelectedIndex = -1;
                        return;
                    }
                    else if (row["Status"].ToString() == "1")
                    {
                        string groupName = Connexion.GetString(row["GID"].ToString(), "Groups", "GroupName", "GroupID");
                        if (MessageBox.Show(this.Resources["TimeEndNowPT1"].ToString() + " " + groupName + this.Resources["TimeEndNowPT2"].ToString(), this.Resources["Warning"].ToString(), MessageBoxButton.YesNo) == MessageBoxResult.No)
                        {
                            HFrom.SelectedIndex = -1;
                            return;
                        }
                    }
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Hour", typeof(string));
                    dt.Columns.Add("Status", typeof(int));
                    dt.Columns.Add("GID", typeof(int));

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
                        if (found == 0)
                        {
                            string Command = "Select  * From Class_Time " +
                                "Where IDroom = " + rowRoom["ID"].ToString() + " " +
                                "and day =  " +  dayIndex + " " +
                                "and CONVERT(INT,SUBSTRING(TimeStart,0,3)) = " + i;
                            DataTable dttime = new DataTable();
                            Connexion.FillDT(ref dttime, Command);
                            if (dttime.Rows.Count > 0)
                            {
                                found = 2;
                                gid = dttime.Rows[0][1].ToString();
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
                            dr[2] = gid;
                        }
                        if (found == 2)
                        {
                            found = 1;
                            dr[1] = 1;
                            dr[2] = gid;
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
                if (Date.Text == "")
                {
                    MessageBox.Show("Please fill in the date");
                    return;
                }
                DateTime selectedDate = Date.SelectedDate ?? DateTime.Now;
                int dayIndex = (int)selectedDate.DayOfWeek;
                if (row != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Hour", typeof(string));
                    dt.Columns.Add("Status", typeof(int));
                    dt.Columns.Add("GID", typeof(int));

                    if (row["Status"].ToString() == "2")
                    {
                        string groupName = Connexion.GetString(row["GID"].ToString(), "Groups", "GroupName", "GroupID");
                        MessageBox.Show(this.Resources["TimeTaken"].ToString() + " " + groupName); //edit resource
                        HTo.SelectedIndex = -1;
                    }
                    else if (row["Status"].ToString() == "1")
                    {
                        DataRowView rowroom = (DataRowView)CRoom.SelectedItem;
                        SqlConnection con = Connexion.Connect();
                        SqlCommand CommandID = new SqlCommand("Select  CONVERT(INT,SUBSTRING(TimeStart,4,2)) From Class_Time " +
                            "Where IDroom = " + rowroom["ID"].ToString() + " " +
                            "and day =  " + dayIndex + " " +
                            "and CONVERT(INT,SUBSTRING(TimeStart,0,3)) = " + row["Hour"].ToString() + " " +
                            "and GID = '" + row["GID"].ToString() + "'",
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
                                dr[2] = int.Parse(row["GID"].ToString());
                            }
                            else if (result < i)
                            {
                                dr[1] = 2;
                                dr[2] = int.Parse(row["GID"].ToString());
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
                        string groupName = Connexion.GetString(row["GID"].ToString(), "Groups", "GroupName", "GroupID");
                        MessageBox.Show(this.Resources["TimeTaken"].ToString() + " " + groupName);
                        MTo.SelectedIndex = -1;
                        return;
                    }
                    else if (row["Status"].ToString() == "1")
                    {
                        string groupName = Connexion.GetString(row["GID"].ToString(), "Groups", "GroupName", "GroupID");
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
        private void CPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TPayment_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (CBStudent.SelectedIndex != -1 && TComboBox.SelectedIndex != -1 && TBName.Text != "" && CPrice.Text != "" && TPayment.Text != "" && TPaymentMethod.SelectedIndex != -1)
                {
                    DataRowView row = (DataRowView)TComboBox.SelectedItem;
                    DataRowView rowstudent = (DataRowView)CBStudent.SelectedItem;
                    pid = Connexion.GetInt("Insert into Particulier  output inserted.ID Values(" + row["ID"].ToString() + " ,'" + TBName.Text + "','" + CPrice.Text + "' , '" + TPayment.Text + "','" + TPaymentMethod.SelectedIndex + "')");
                    Connexion.Insert("Insert into Particulier_Student values (" + pid + " , " + rowstudent["ID"].ToString() + " , 0 , 0 )");
                    Connexion.FillDTItem("Rooms", ref CRoom);
                    MessageBox.Show(this.Resources["InsertedSucc"].ToString());
                    Time.IsSelected = true;
                }
                else
                {
                    MessageBox.Show("Please Fill in all the information");
                }
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if(Date.Text != "" && MTo.SelectedIndex != -1)
                {
                    DataRowView rowHFrom = (DataRowView)HFrom.SelectedValue;
                    DataRowView rowMFrom = (DataRowView)MFrom.SelectedValue;
                    DataRowView rowHTo = (DataRowView)HTo.SelectedValue;
                    DataRowView RowMTo = (DataRowView)MTo.SelectedValue;
                    DataRowView rowRoom = (DataRowView)CRoom.SelectedItem;
                    string RID = rowRoom["ID"].ToString();
                    string Start = rowHFrom["Hour"].ToString() + ":" + rowMFrom["Hour"].ToString();
                    string End = rowHTo["Hour"].ToString() + ":" + RowMTo["Hour"].ToString();
                    Connexion.Insert("Insert into Particulier_time values(" + pid + " , '" + Date.Text + "'," + RID+ " ,'" + Start + "','" + End + "',2)");
                    MessageBox.Show("Inserted Succesfully");//0 not studied yet 1 studied 2 cancelled 3 weekly
                    TriggerSelection = false;
                    Connexion.FillDG(ref DGTime, "Select * from Particulier_Time where PID = " + pid);
                    TriggerSelection = true;
                }
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }
        
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TriggerSelection)
            {
                ComboBox comboBox = (ComboBox)sender;

                // Find the parent DataGridRow
                DataGridRow dataGridRow = FindVisualParent<DataGridRow>(comboBox);

                if (dataGridRow != null)
                {
                    // Get the DataContext of the DataGridRow
                    DataRowView dataRowView = dataGridRow.DataContext as DataRowView;

                    if (dataRowView != null)
                    {
                        int OldIndex = Connexion.GetInt("Select Status from Particulier_Time Where ID = " + dataRowView["ID"].ToString());
                        Connexion.Insert("Update Particulier_Time Set Status = " + comboBox.SelectedIndex + " Where ID = " + dataRowView["ID"].ToString());
                        Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("Updated Succesfully")));
                        string TimeStart = Connexion.GetString("Select TimeStart From Particulier_Time Where ID" + dataRowView["ID"].ToString());
                        string TimeEnd = Connexion.GetString("Select TimeEND From Particulier_Time Where ID" + dataRowView["ID"].ToString());
                        double hoursDifference = GetTime(TimeStart, TimeEnd);
                        if (comboBox.SelectedIndex == 1)
                        {
                            AddPaymentChangeTime(hoursDifference, "+");
                        }
                        else if (comboBox.SelectedIndex != 1 && OldIndex == 1)
                        {
                            AddPaymentChangeTime(hoursDifference, "-");
                        }
                    }
                    
                }
            }
        }
        


        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            if (parentObject is T parent)
                return parent;

            return FindVisualParent<T>(parentObject);
        }

        private void ComboBox_SelectionChanged_PaymentStudent(object sender, SelectionChangedEventArgs e)
        {
            DataRowView rowstudent = (DataRowView)CBStudent.SelectedItem;
            string sid = rowstudent["ID"].ToString();
            TBPricePay.Text = Connexion.GetString("Select TotalPay from Particulier_Student Where PID = " + pid + " and SID = " + sid);
            Connexion.FillDG(ref DGPay, "Select ID , CID as PID , Date , Price ,'Student' as Type " +
             "from StudentPayment Where Type = 2 and CID = " + pid);


        }

        private void Button_Click_Payment(object sender, RoutedEventArgs e)
        {
            if (TBPricePay.Text != "" && CBStudentPay.SelectedIndex != -1)
            {

                string dt = DateTime.Now.ToString("dd-MM-yyyy HH:mm");
                DataRowView rowstudent = (DataRowView)CBStudent.SelectedItem;
                string sid = rowstudent["ID"].ToString();
                Connexion.Insert("Insert into StudentPayment Values (" + sid + " , 2 , " + pid + " , " + TBPricePay.Text + " , '', '" + dt + "',0");
                double percentage = Connexion.GetInt(pid.ToString(), "Particulier", "TPrice") / Connexion.GetInt(pid.ToString(), "Particulier", "Price");
                Connexion.Insert("Update Particulier_Student set TotalPayTeacher = TotalPayTeacher + " + int.Parse(TBPricePay.Text) * percentage + " , TotalPay = TotalPay - " + TBPricePay.Text + " Where PID = " + pid + " and SID = " + sid);

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
            this.Resources.MergedDictionaries.Add(ResourceDic);
        }

        private double GetTime(string TimeStart , string TimeEnd)
        {
            TimeSpan startTime = TimeSpan.ParseExact(TimeStart, "HH:mm", null);
            TimeSpan endTime = TimeSpan.ParseExact(TimeEnd, "HH:mm", null);
            TimeSpan timeDifference = endTime - startTime;
            double hoursDifference = timeDifference.Hours;
            int minutesDifference = timeDifference.Minutes;
            if (minutesDifference == 15)
            {
                hoursDifference += 0.25;

            }
            else if (minutesDifference == 30)
            {
                hoursDifference += 0.5;
            }
            else if (minutesDifference == 45)
            {
                hoursDifference += 0.75;
            }
            return hoursDifference; 
        }
        private void AddPaymentChangeTime(double ChangeTime , string Sign)
        {
            int price = Connexion.GetInt("Select Price from Particulier");
            int TPrice = Connexion.GetInt("Select TPrice from Particulier");
            double AddedPrice = ChangeTime * price;
            double AddedTPrice = ChangeTime * TPrice;
            if (Sign == "+")
            {
                Connexion.Insert("Update Particulier_Student Set TotalPay = TotalPay + " + AddedPrice + " , TotalPayTeacher = TotalPayTeacher + " + AddedTPrice + " Where PID = " + pid);
            }
            else if (Sign == "-")
            {
                Connexion.Insert("Update Particulier_Student Set TotalPay = TotalPay - " + AddedPrice + " , TotalPayTeacher = TotalPayTeacher - " + AddedTPrice + " Where PID = " + pid);
            }
        }
    }
}
