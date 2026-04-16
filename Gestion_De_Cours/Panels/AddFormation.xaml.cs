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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for AddFormation.xaml
    /// </summary>
    public partial class AddFormation : Window
    {
        string FID = "";
        string Type = "";
        DataTable dtstudents = new DataTable();
        public AddFormation(string type, string IDformation)
        {
            try
            {


                int lang = Connexion.Language();
                Type = type;
                InitializeComponent();


                this.Height = 250;
                this.Width = 600;
                Connexion.FillCB(ref TComboBox, "Select ID,TFirstName + ' ' + TLastName As Name ,TGender,'" + Connexion.GetImagesFile() + "\\" + "T' + Convert(Varchar(50),ID)  + '.jpg' as Picture From Teacher where status = 1");
                Connexion.FillDTItem("Rooms", ref CRoom);

                SetLang();
                if (lang == 1)
                {
                    this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
                }
                if (type == "Show")
                {
                    FID = IDformation;

                    Connexion.FillDT(ref dtstudents, "SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students Except SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students join Formation_Student On Formation_Student.SID = Students.ID WHere Formation_Student.FID = " + FID);
                    CBStudent.ItemsSource = dtstudents.DefaultView;
                    TBName.Text = Connexion.GetString(FID, "Formation", "Name");
                    TBHours.Text = Connexion.GetString(FID, "Formation", "Hours");
                    TComboBox.SelectedValue = Connexion.GetInt(FID, "Formation", "TID");
                    CBTPaymentMethod.SelectedIndex = Connexion.GetInt(FID, "Formation", "TPaymentMethod");
                    TBPrice.Text = Connexion.GetString(FID, "Formation", "Price");
                    TBTPrice.Text = Connexion.GetString(FID, "Formation", "TPrice");
                    Connexion.FillCB(ref CBAttendanceTime, "SELECT *,Case " +
                    "  When Class_Time.Day = 0 Then N'" + Properties.Resources.Sunday + "' + TimeStart" +
                    "  When Class_Time.Day = 1 Then N'" + Properties.Resources.Monday + "' + TimeStart" +
                    "  When Class_Time.Day = 2 Then N'" + Properties.Resources.Tuesday + "' + TimeStart" +
                    "  When Class_Time.Day = 3 Then N'" + Properties.Resources.Wednesday + "' + TimeStart" +
                    "  When Class_Time.Day = 4 Then N'" + Properties.Resources.Thursday + "' + TimeStart" +
                    "  When Class_Time.Day = 5 Then N'" + Properties.Resources.Friday + "' + TimeStart" +
                    "  When Class_Time.Day = 6 Then N'" + Properties.Resources.Saturday + "' + TimeStart " +
                    "  End As Time  " +
                    "FROM Class_Time " +
                    "WHERE Class_time.Type = 2 and  CLass_Time.FID=" + FID);
                    Connexion.FillDG(ref DGAddTime, "Select Class_Time.FID as FID, Class_Time.ID as ID ,Class_Time.Day as DayID , " +
                   " Case  When Class_Time.Day = 0 Then N'" + this.Resources["Sunday"].ToString() + "' " +
                   "  When Class_Time.Day = 1 Then N'" + this.Resources["Monday"].ToString() + "' " +
                   "  When Class_Time.Day = 2 Then N'" + this.Resources["Tuesday"].ToString() + "'" +
                   "  When Class_Time.Day = 3 Then N'" + this.Resources["Wednesday"].ToString() + "'" +
                   "  When Class_Time.Day = 4 Then N'" + this.Resources["Thursday"].ToString() + "' " +
                   "  When Class_Time.Day = 5 Then N'" + this.Resources["Friday"].ToString() + "'" +
                   "  When Class_Time.Day = 6 Then N'" + this.Resources["Saturday"].ToString() + "' " +
                    " End  as Day , " +
                    "Class_Time.TimeStart as TimeStart, " +
                    "Class_Time.TimeEnd As TimeEnd ," +
                    "Rooms.Room As Room , " +
                    "Formation.Name as Name , " +
                    "Formation.ID , " +
                    "Rooms.ID as IDRoom " +
                    "From Class_Time " +
                    "Join Rooms On Class_Time.IDRoom = Rooms.ID " +
                    "JOIN Formation ON Formation.ID = Class_Time.FID " +
                    "Where Class_time.Type = 2  and Class_Time.FID = " + FID);

                    Connexion.FillDG(ref DGAttendance, "Select Class_Time.TimeStart, Formation_Attendance.ID as ID, dbo.ConvertMintoHour(Formation_Attendance.Duration) as Duration , Formation_Attendance.Date , dbo.GetStudentsAmmountAttendanceFormation(Formation_Attendance.ID) as TotalStudent from Formation_Attendance Join Class_Time on Formation_Attendance.TimeID = Class_Time.ID Where Formation_Attendance.FID = " + FID);


                    Connexion.FillDG(ref DGStudent, "Select students.FirstName , Students.LastName , Students.ID as ID ,dbo.CalculateRemainingPayment(Formation_Student.FID, Students.ID) as SuPrice, Formation_Student.Note as Note ,Formation_Student.Hours as Hours  , Formation_Student.StartHour as StartHour  from Formation_Student join Students on Formation_Student.SID = Students.ID Where Formation_Student.FID = " + FID);
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (Tab.SelectedIndex == 0)
            {
                this.Height = 250;
                this.Width = 600;
            }
            else
            {
                this.Height = 400;
                this.Width = 650;
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

        //Formation Info Panel
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {


                TextBox textBox = sender as TextBox;
                Regex regex = new Regex("[^0-9]+");
                if (regex.IsMatch(e.Text))
                {
                    e.Handled = regex.IsMatch(e.Text);

                }
                else
                {


                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                DataRowView row = (DataRowView)TComboBox.SelectedItem;

                string TID = row["ID"].ToString(); //getting Teacher ID 

                FID = Connexion.GetString("Insert into Formation output inserted.ID  Values(N'" + TBName.Text + "','" + TID + "','" + TBHours.Text + "','" + CBTPaymentMethod.SelectedIndex + "','" + TBTPrice.Text + "','" + TBPrice.Text + "')");
                Connexion.InsertHistory(0,FID,12);
                Type = "Show";
                Connexion.FillDT(ref dtstudents, "SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students  LEFT JOIN formation_Student fs ON students.ID = fs.SID WHERE Students.ID NOT IN (  SELECT SID FROM formation_Student WHERE FID = " + FID + " ) And status = 1");
                CBStudent.ItemsSource = dtstudents.DefaultView;
                TabTime.Focus();
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }


        //Time Panel 
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
                    dt.Columns.Add("ID", typeof(int));
                    dt.Columns.Add("Type", typeof(int));

                    for (int i = 6; i < 24; i++)
                    {
                        DataRow dr = dt.NewRow();
                        string hour = "";
                        if (i < 10)
                        {
                            dr[0] = "0" + i.ToString();
                            hour = "0" + i.ToString();
                        }
                        else
                        {
                            dr[0] = i.ToString();
                            hour = "0" + i.ToString();
                        }
                        int[] r = Connexion.CheckTimeHour(hour, CDay.SelectedIndex, int.Parse(rowRoom["ID"].ToString()));
                        dr[1] = r[0];
                        dr[2] = r[1];
                        dr[3] = r[2];
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
                              "and day =  " + CDay.SelectedIndex + " " +
                              "and CONVERT(INT,SUBSTRING(TimeEnd,0,3)) = " + row["Hour"].ToString() + " " +
                              "and Groups.ClassID = '" + Connexion.GetClassID(row["ID"].ToString()) + "'";
                        }
                        else if (row["Type"].ToString() == "2")
                        {
                            query = "Select  CONVERT(INT,SUBSTRING(TimeEnd,4,2)) From Class_Time  join Formation on Formation.ID = Class_Time.FID " +
                            "Where IDroom = " + rowroom["ID"].ToString() + " " +
                            "and day =  " + CDay.SelectedIndex + " " +
                            "and CONVERT(INT,SUBSTRING(TimeEnd,0,3)) = " + row["Hour"].ToString() + " " +
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

                if (row != null && row2 != null && rowRoom != null)
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
                        MessageBox.Show(this.Resources["TimeTaken"].ToString() + " " + groupName);
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
                                "and day =  " + CDay.SelectedIndex + " " +
                                "and CONVERT(INT,SUBSTRING(TimeStart,0,3)) = " + i;

                            DataTable dttime = new DataTable();
                            Connexion.FillDT(ref dttime, Command);
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
                            "and day =  " + CDay.SelectedIndex + " " +
                            "and CONVERT(INT,SUBSTRING(TimeStart,0,3)) = " + row["Hour"].ToString() + " " +
                            "and GID = '" + row["ID"].ToString() + "'";
                        }
                        else if (row["Type"].ToString() == "2")
                        {
                            query = "Select  CONVERT(INT,SUBSTRING(TimeStart,4,2)) From Class_Time " +
                            "Where IDroom = " + rowroom["ID"].ToString() + " " +
                            "and day =  " + CDay.SelectedIndex + " " +
                            "and CONVERT(INT,SUBSTRING(TimeStart,0,3)) = " + row["Hour"].ToString() + " " +
                            "and FID = '" + row["ID"].ToString() + "'";
                        }
                        SqlCommand CommandID = new SqlCommand(query,
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
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void CDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CRoom.SelectedIndex = -1;
            HFrom.SelectedIndex = -1;
            HTo.SelectedIndex = -1;
            MTo.SelectedIndex = -1;
            MFrom.SelectedIndex = -1;
        }

        private void Button_Click_AddTime(object sender, RoutedEventArgs e)
        {
            try
            {


                DataRowView rowHFrom = (DataRowView)HFrom.SelectedValue;
                DataRowView rowMFrom = (DataRowView)MFrom.SelectedValue;
                DataRowView rowHTo = (DataRowView)HTo.SelectedValue;
                DataRowView RowMTo = (DataRowView)MTo.SelectedValue;
                DataRowView rowRoom = (DataRowView)CRoom.SelectedItem;
                string RID = rowRoom["ID"].ToString();
                string Start = rowHFrom["Hour"].ToString() + ":" + rowMFrom["Hour"].ToString();
                string End = rowHTo["Hour"].ToString() + ":" + RowMTo["Hour"].ToString();
                Connexion.Insert("Insert into Class_Time Values (NULL,'" + RID + "','" + CDay.SelectedIndex + "','" + Start + "','" + End + "',2 , " + FID + ")");
                DateTime currentDate = DateTime.Now;
                if (CDay.SelectedIndex == (int)currentDate.DayOfWeek)
                {
                    DateTime today = DateTime.Today;

                    // Format the date as "dd-MM-yyyy"
                    string formattedDate = today.ToString("dd-MM-yyyy");
                    if (Connexion.IFNULL("Select ID from Formation_Attendance where Formation_Attendance.Date = '" + formattedDate + "'	and FID = " + FID))
                    {
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
                Connexion.FillCB(ref CBAttendanceTime, "SELECT *,Case " +
                     "  When Class_Time.Day = 0 Then N'" + Properties.Resources.Sunday + "' + TimeStart" +
                     "  When Class_Time.Day = 1 Then N'" + Properties.Resources.Monday + "' + TimeStart" +
                     "  When Class_Time.Day = 2 Then N'" + Properties.Resources.Tuesday + "' + TimeStart" +
                     "  When Class_Time.Day = 3 Then N'" + Properties.Resources.Wednesday + "' + TimeStart" +
                     "  When Class_Time.Day = 4 Then N'" + Properties.Resources.Thursday + "' + TimeStart" +
                     "  When Class_Time.Day = 5 Then N'" + Properties.Resources.Friday + "' + TimeStart" +
                     "  When Class_Time.Day = 6 Then N'" + Properties.Resources.Saturday + "' + TimeStart " +
                     "  End As Time  " +
                     "FROM Class_Time " +
                     "WHERE Class_time.Type = 2 and  CLass_Time.FID=" + FID);

                Connexion.FillDG(ref DGAddTime, "Select Class_Time.FID as FID, Class_Time.ID as ID ,Class_Time.Day as DayID , " +
                    " Case  When Class_Time.Day = 0 Then N'" + this.Resources["Sunday"].ToString() + "' " +
                    "  When Class_Time.Day = 1 Then N'" + this.Resources["Monday"].ToString() + "' " +
                    "  When Class_Time.Day = 2 Then N'" + this.Resources["Tuesday"].ToString() + "'" +
                    "  When Class_Time.Day = 3 Then N'" + this.Resources["Wednesday"].ToString() + "'" +
                    "  When Class_Time.Day = 4 Then N'" + this.Resources["Thursday"].ToString() + "' " +
                    "  When Class_Time.Day = 5 Then N'" + this.Resources["Friday"].ToString() + "'" +
                    "  When Class_Time.Day = 6 Then N'" + this.Resources["Saturday"].ToString() + "' " +
                     " End  as Day , " +
                     "Class_Time.TimeStart as TimeStart, " +
                     "Class_Time.TimeEnd As TimeEnd ," +
                     "Rooms.Room As Room , " +
                     "Formation.Name as Name , " +
                     "Formation.ID , " +
                     "Rooms.ID as IDRoom " +
                     "From Class_Time " +
                     "Join Rooms On Class_Time.IDRoom = Rooms.ID " +
                     "JOIN Formation ON Formation.ID = Class_Time.FID " +
                     "Where Class_time.Type = 2  and Class_Time.FID = " + FID);
                MTo.SelectedIndex = -1;
                HTo.SelectedIndex = -1;
                MFrom.SelectedIndex = -1;
                HFrom.SelectedIndex = -1;
                CDay.SelectedIndex = -1;
                CRoom.SelectedIndex = -1;
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }

        }
        //Students
        private void Button_Click_AddStudent(object sender, RoutedEventArgs e)
        {
            try
            {


                DataRowView row = (DataRowView)CBStudent.SelectedItem;
                if (row != null && FID != "")
                {
                    int Hours = Connexion.GetInt("Select Hours from Formation Where ID = " + FID);
                    int Remaining = Connexion.GetInt("Select dbo.FormationRemainingHours(" + FID + ")");
                    int StartHour = Hours - Remaining;
                    Connexion.Insert("Insert into Formation_Student Values(" + row["ID"].ToString() + "," + FID + "," + TBPrice.Text + ",'',"+ Hours + " ,"+ StartHour+" )");
                    dtstudents.Rows.Remove(row.Row);

                    // CBStudent.ItemsSource = dtstudents.DefaultView;
                    Connexion.FillDG(ref DGStudent, "Select students.FirstName , Students.LastName , Students.ID as ID ,dbo.CalculateRemainingPayment(Formation_Student.FID, Students.id) as SuPrice, Formation_Student.Note as Note , Formation_Student.Hours as Hours , Formation_Student.StartHour as StartHour from Formation_Student join Students on Formation_Student.SID=Students.ID Where Formation_Student.FID = " + FID);
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnDeleteTime_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                string ID = ((DataRowView)DGAddTime.SelectedItem).Row["ID"].ToString();
                Connexion.Insert("Delete From Class_time Where ID = '" + ID + "'");
                Connexion.FillDG(ref DGAddTime, "Select Class_Time.FID as FID, Class_Time.ID as ID ,Class_Time.Day as DayID , " +
                   " Case  When Class_Time.Day = 0 Then N'" + this.Resources["Sunday"].ToString() + "' " +
                   "  When Class_Time.Day = 1 Then N'" + this.Resources["Monday"].ToString() + "' " +
                   "  When Class_Time.Day = 2 Then N'" + this.Resources["Tuesday"].ToString() + "'" +
                   "  When Class_Time.Day = 3 Then N'" + this.Resources["Wednesday"].ToString() + "'" +
                   "  When Class_Time.Day = 4 Then N'" + this.Resources["Thursday"].ToString() + "' " +
                   "  When Class_Time.Day = 5 Then N'" + this.Resources["Friday"].ToString() + "'" +
                   "  When Class_Time.Day = 6 Then N'" + this.Resources["Saturday"].ToString() + "' " +
                    " End  as Day , " +
                    "Class_Time.TimeStart as TimeStart, " +
                    "Class_Time.TimeEnd As TimeEnd ," +
                    "Rooms.Room As Room , " +
                    "Formation.Name as Name , " +
                    "Formation.ID , " +
                    "Rooms.ID as IDRoom " +
                    "From Class_Time " +
                    "Join Rooms On Class_Time.IDRoom = Rooms.ID " +
                    "JOIN Formation ON Formation.ID = Class_Time.FID " +
                    "Where Class_time.Type = 2  and Class_Time.FID = " + FID);
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }
        private void CBTypePayemnt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {


                if (CBTypePayment.SelectedIndex == 0)
                {
                    CBStudentPayment.Visibility = Visibility.Collapsed;
                    LBStudentPayment.Visibility = Visibility.Collapsed;
                    TextBoxType.Visibility = Visibility.Visible;
                    LabelType.Visibility = Visibility.Visible;
                    DGPaymentTeacher.Visibility = Visibility.Visible;
                    DGPaymentStudent.Visibility = Visibility.Collapsed;
                    int typepayment = Connexion.GetInt("Select TPaymentMethod from Formation Where ID =" + FID);
                    if (typepayment == 0)//Hour
                    {
                        DatagridColumnType.Text = this.Resources["Hours"].ToString();
                        LabelType.Content = this.Resources["HoursLB"].ToString();
                        int HoursStudied = Connexion.GetInt("Select case when SuM(Duration) is null then 0 else Sum(Duration) end as f  from Formation_Attendance Where FID = " + FID) / 60;
                        int HoursUnpaid = -Connexion.GetInt("Select Case When SUM(Type) is null then 0 else Sum(Type) end as f  from Formation_Payments Where FID = " + FID) + HoursStudied;
                        int Total = HoursUnpaid * Connexion.GetInt("Select TPrice from Formation Where ID =" + FID);

                        if (Total < 0)
                        {
                            Total = 0;
                            HoursUnpaid = 0;
                        }
                        TextBoxType.Text = HoursUnpaid.ToString();
                        TBPricePayment.Text = Total.ToString();

                    }
                    else if (typepayment == 1)//Student
                    {
                        TextBoxType.Visibility = Visibility.Collapsed;
                        LabelType.Visibility = Visibility.Collapsed;

                    }
                    else if (typepayment == 2)//Session
                    {
                        LabelType.Content = this.Resources["SessionsLB"].ToString();
                        DatagridColumnType.Text = this.Resources["Session"].ToString();
                        int SessionsUnpaid = Connexion.GetInt("Select Count(*) from Formation_Attendance Where FID= " + FID) - Connexion.GetInt("Select case when Sum(Type) is null then 0 else Sum(Type) end as f from Formation_Payments Where FID =" + FID);
                        TextBoxType.Text = SessionsUnpaid.ToString();
                        int Total = SessionsUnpaid * Connexion.GetInt("Select TPrice from Formation Where ID =" + FID);
                        if (Total < 0)
                        {
                            Total = 0;
                        }
                        TBPricePayment.Text = Total.ToString();
                    }
                    Connexion.FillDG(ref DGPaymentTeacher, "Select ID , Price  ,Date  ,Type , Case WHen Deleted = 0 then '' when Deleted = 1 then N'" + this.Resources["Deleted"].ToString() + "' end as Note from  Formation_Payments  where FID = " + FID);
                }
                else
                {
                    DGPaymentTeacher.Visibility = Visibility.Collapsed;
                    DGPaymentStudent.Visibility = Visibility.Visible;
                    CBStudentPayment.Visibility = Visibility.Visible;
                    LBStudentPayment.Visibility = Visibility.Visible;
                    TextBoxType.Visibility = Visibility.Collapsed;
                    LabelType.Visibility = Visibility.Collapsed;
                    TBPricePayment.Text = "";
                    Connexion.FillCB(ref CBStudentPayment, "SELECT Students.ID as SID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students join Formation_Student on Formation_Student.SID = Students.ID  Where  FID = " + FID);
                    Connexion.FillDG(ref DGPaymentStudent, "Select StudentPayment.ID as ID , StudentPayment.SID ,'" + this.Resources["StudentPayment"].ToString() + "' as Type , Students.FirstName + ' ' + Students.LastName as Name , Price , Date , StudentPayment.Note as Note  From StudentPayment Join Students on Students.ID = StudentPayment.SID where StudentPayment.Type = 3 and CID = " + FID + " ");

                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void PaymentBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (TBPricePayment.Text == "")
                {
                    return;
                }
                if (int.Parse(TBPricePayment.Text) == 0)
                {
                    return;
                }
                if (CBTypePayment.SelectedIndex == 0)//Teacher
                {
                    Connexion.Insert("Insert into Formation_Payments values(" + FID + "," + TBPricePayment.Text + ",'" + DateTime.Now.ToString("dd-MM-yyyy HH:mm") + "'," + TextBoxType.Text + ",0)");
                    Connexion.FillDG(ref DGPaymentTeacher, "Select ID , Price  ,Date  ,Type , Case WHen Deleted = 0 then '' when Deleted = 1 then N'" + this.Resources["Deleted"].ToString() + "' end as Note  from Formation_Payments  where FID = " + FID);
                }
                else //Student
                {
                    DataRowView row = (DataRowView)CBStudentPayment.SelectedItem;
                    if (row != null)
                    {

                        string PaymentiD = Connexion.GetString("Insert into StudentPayment OUTPUT Inserted.ID Values ( " + row["SID"].ToString() + ", 3 ," + FID + " , " + TBPricePayment.Text + ",'' ,'" + DateTime.Now.ToString("dd-MM-yyyy HH:mm") + " ', 0 )");
                        Connexion.InsertHistory(0,PaymentiD , 13);
                        Connexion.FillDG(ref DGPaymentStudent, "Select StudentPayment.ID as ID , SID ,'" + this.Resources["StudentPayment"].ToString() + "' as Type , Students.FirstName + ' ' + Students.LastName as Name , Price , Date From StudentPayment Join Students on Students.ID = StudentPayment.SID where StudentPayment.Type = 3 and CID = " + FID + " and SID = " + row["SID"].ToString());
                    }
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void Button_Click_AttendanceCreate(object sender, RoutedEventArgs e)
        {
            try
            {


                DataRowView row = (DataRowView)CBAttendanceTime.SelectedItem;
                if (row != null)
                {
                    if (MessageBox.Show("Do you want To Create this Attendance?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        string AID = Connexion.GetString("Insert into Formation_Attendance output Inserted.ID values (" + FID + ",'" + DateTime.Today.ToString("d").Replace("/", "-") + "'," + row["ID"].ToString() + ",'',Null)");
                        var AddS = new AttendanceAdd(AID, "Add", "2");
                        AddS.Show();
                        Connexion.InsertHistory(0, AID, 14);
                        Connexion.FillDG(ref DGAttendance, "Select Class_Time.TimeStart, Formation_Attendance.ID as ID, dbo.ConvertMintoHour(Formation_Attendance.Duration) as Duration , Formation_Attendance.Date , dbo.GetStudentsAmmountAttendanceFormation(Formation_Attendance.ID) as TotalStudent from Formation_Attendance Join Class_Time on Formation_Attendance.TimeID = Class_Time.ID Where Formation_Attendance.FID = " + FID);
                    }
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnDeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                DataRowView row = (DataRowView)DGStudent.SelectedItem;
                if (row != null)
                {
                    if (MessageBox.Show("Are you Sure u want to Delete the student from this Class?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        string SID = row["ID"].ToString();
                        Connexion.Insert("Delete from Formation_Student Where SID = " + SID + " and FID = " + FID);
                    }
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }

        }

        private void DGStudent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void CBStudentPayment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {


                DataRowView row = (DataRowView)CBStudentPayment.SelectedItem;
                if (row != null)
                {
                    TBPricePayment.Text = Connexion.GetString("SELECT dbo.CalculateRemainingPayment(" + FID + ", " + row["SID"].ToString() + "); ");
                    Connexion.FillDG(ref DGPaymentStudent, "Select StudentPayment.ID as ID , SID ,'" + this.Resources["StudentPayment"].ToString() + "' as Type , Students.FirstName + ' ' + Students.LastName as Name , Price , Date From StudentPayment Join Students on Students.ID = StudentPayment.SID where StudentPayment.Type = 3 and CID = " + FID + " and SID = " + row["SID"].ToString());
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void CBAttendanceTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BtnDeleteAttendance_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnPrintPayment_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDeletePayment_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (CBTypePayment.SelectedIndex == 0)
                {
                    DataRowView row = (DataRowView)DGPaymentStudent.SelectedItem;
                    if (row != null)
                    {
                        if (MessageBox.Show("Are you Sure u want to Delete this Payment?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            Connexion.Insert("Update StudentPayment Set Deleted = 1 Where ID =" + row["ID"].ToString());
                            Connexion.FillDG(ref DGPaymentStudent, "Select StudentPayment.ID as ID , SID ,'" + this.Resources["StudentPayment"].ToString() + "' as Type , Students.FirstName + ' ' + Students.LastName as Name , Price , Date From StudentPayment Join Students on Students.ID = StudentPayment.SID where StudentPayment.Type = 3 and CID = " + FID);
                        }
                    }
                }
                else
                {
                    DataRowView row = (DataRowView)DGPaymentTeacher.SelectedItem;
                    if (row != null)
                    {
                        if (MessageBox.Show("Are you Sure u want to Delete this Payment?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            Connexion.Insert("Update StudentPayment Set Deleted = 1 Where ID =" + row["ID"].ToString());
                            Connexion.FillDG(ref DGPaymentStudent, "Select StudentPayment.ID , SID ,'" + this.Resources["StudentPayment"].ToString() + "' as Type , Students.FirstName + ' ' + Students.LastName as Name , Price , Date From StudentPayment Join Students on Students.ID = StudentPayment.SID where StudentPayment.Type = 3 and CID = " + FID);
                        }
                    }

                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }

        }

        private void DGStudent_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
                        int SID = (int)rowView["ID"];
                        if(columnIndex ==3)
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

                                   
                                    Connexion.Insert("Update Formation_Student Set Hours ="+editedValue +" where SID =" + SID + " and FID =" + FID);
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
                        else if (columnIndex == 4) // Assuming "RemainingPayment" column is at index 3
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
                        else if (columnIndex == 5) // Assuming "Note" column is at index 4
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

        private void BtnShowStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                DataRowView row = (DataRowView)DGStudent.SelectedItem;
                if (row != null)
                {
                    string SID = row["ID"].ToString();
                    var AddW = new StudentAdd("Show", SID);
                    AddW.Show();
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnShowAttendance_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                DataRowView row = (DataRowView)DGAttendance.SelectedItem;
                if (row != null)
                {

                    var AddS = new AttendanceAdd(row["ID"].ToString(), "Show", "2");
                    AddS.Show();
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnPrintAttend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBoxType_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {


                if (TextBoxType.Text == "")
                {
                    return;
                }
                int typepayment = Connexion.GetInt("Select TPaymentMethod from Formation Where ID =" + FID);
                if (typepayment == 0)//Hour
                {
                    int Total = int.Parse(TextBoxType.Text) * Connexion.GetInt("Select TPrice from Formation Where ID =" + FID);
                    if (Total < 0)
                    {
                        Total = 0;
                    }
                    TBPricePayment.Text = Total.ToString();

                }
                else if (typepayment == 1)//Student
                {

                }
                else if (typepayment == 2)//Session
                {

                    int Total = int.Parse(TextBoxType.Text) * Connexion.GetInt("Select TPrice from Formation Where ID =" + FID);
                    if (Total < 0)
                    {
                        Total = 0;
                    }
                    TBPricePayment.Text = Total.ToString();
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }

        }
    }
}
