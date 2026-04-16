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
    /// Interaction logic for Attendance_Programmed.xaml
    /// </summary>
    
    public partial class Attendance_Programmed : Window
    {
        string GID = "";
        public string returnID { get; set; }
        public Attendance_Programmed(string date)
        {
            InitializeComponent();
            SetLang();
            Connexion.FillCB(ref CBclass, "Select * from Class");
            Connexion.FillCB(ref CRoom, "Select * from Rooms");
            Date.Text = date;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                DataRowView rowRoom = (DataRowView)CRoom.SelectedItem;
                DataRowView rowHFrom = (DataRowView)HFrom.SelectedItem;
                DataRowView rowMFrom = (DataRowView)MFrom.SelectedItem;
                DataRowView rowHTo = (DataRowView)HTo.SelectedItem;
                DataRowView RowMTo = (DataRowView)MTo.SelectedItem;
                if (RowMTo == null)
                {
                    return;
                }
                string RID = rowRoom["ID"].ToString();
                string Start = rowHFrom["Hour"].ToString() + ":" + rowMFrom["Hour"].ToString();
                string End = rowHTo["Hour"].ToString() + ":" + RowMTo["Hour"].ToString();
                this.returnID =  Connexion.GetString("Insert into Attendance_Prog Output Inserted.ID Values (" + GID+ " , " + RID + " ,'" + Date.Text.Replace("/", "-") + "','" + Start + "','" + End + "',N'" + Note.Text + "',0)"   );
                this.Close();
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void CBclass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CBclass.SelectedItem;
            CRoom.SelectedIndex = -1;
            if (row != null)
            {
                if (row["MultipleGroups"].ToString() == "Multiple")
                {
                   
                    SPGroup.Visibility = Visibility.Visible;
                    Connexion.FillCB(ref CBGroup, "Select * from groups Where ClassID = " + row["ID"].ToString() + "");
                }
                else
                {
                    SPGroup.Visibility = Visibility.Collapsed;
                    GID = Connexion.GetString("Select GroupID from Groups Where CLassID = " + row["ID"].ToString());
                }
            }
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
                    dt.Columns.Add("ID", typeof(int));
                    dt.Columns.Add("Type", typeof(int));

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
                        DayOfWeek dayOfWeekname = Date.SelectedDate.Value.DayOfWeek;
                        int dayOfWeek = (int)dayOfWeekname;

                        // Cast the enumeration to an integer
                        int dayOfWeekValue = (int)dayOfWeek;

                        int[] r = Connexion.CheckTimeHour(dr[0].ToString(), dayOfWeekValue, int.Parse(rowRoom["ID"].ToString()), GID);
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

        private void CBGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView RowGroup = (DataRowView)CBGroup.SelectedItem;
                if(RowGroup == null)
                {
                    return;
                }
                GID = RowGroup["GroupID"].ToString();
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
                if(GID == "")
                {
                    return;
                }

                MFrom.SelectedIndex = -1;
                MTo.SelectedIndex = -1;
                HTo.SelectedIndex = -1;

                DayOfWeek dayOfWeekname = Date.SelectedDate.Value.DayOfWeek;
                int dayOfWeek = (int)dayOfWeekname;
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
                              "and day =  " + dayOfWeek + " " +
                              "and GID != " + GID + " and CONVERT(INT,SUBSTRING(TimeEnd,0,3)) = " + row["Hour"].ToString() + " " +
                              "and Groups.ClassID = '" + Connexion.GetClassID(row["ID"].ToString()) + "'";
                        }
                        else if (row["Type"].ToString() == "2")
                        {
                            query = "Select  CONVERT(INT,SUBSTRING(TimeEnd,4,2)) From Class_Time  join Formation on Formation.ID = Class_Time.FID " +
                            "Where IDroom = " + rowroom["ID"].ToString() + " " +
                            "and day =  " + dayOfWeek + " " +
                            "and GID != " + GID + " and CONVERT(INT,SUBSTRING(TimeEnd,0,3)) = " + row["Hour"].ToString() + " " +
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
                DayOfWeek dayOfWeekname = Date.SelectedDate.Value.DayOfWeek;
                int dayofWeek = (int)dayOfWeekname;
                if (row != null && row2 != null && rowRoom != null)
                {
                    
                    if (row["Status"].ToString() == "1" || row["Status"].ToString() == "2") 
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
                        if (MessageBox.Show("The Chosen Time Period is taken By The Group :"+ groupName
                            +"  do you want to override it?" , this.Resources["Warning"].ToString(), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            //here first add to verify from appointments and if already an attendance is programmed change its status to 1 
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
                                "and GID != " + GID + " and day =  " + dayofWeek + " " +
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
                DayOfWeek dayOfWeekname = Date.SelectedDate.Value.DayOfWeek;
                int dayOfWeek = (int)dayOfWeekname;

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
                            "and day =  " + dayOfWeek + " " +
                            "and GID != " + GID + " and CONVERT(INT,SUBSTRING(TimeStart,0,3)) = " + row["Hour"].ToString() + " " +
                            "and GID = '" + row["ID"].ToString() + "'";
                        }
                        else if (row["Type"].ToString() == "2")
                        {
                            query = "Select  CONVERT(INT,SUBSTRING(TimeStart,4,2)) From Class_Time " +
                            "Where IDroom = " + rowroom["ID"].ToString() + " " +
                            "and day =  " + dayOfWeek + " " +
                            "and GID != " + GID + " and CONVERT(INT,SUBSTRING(TimeStart,0,3)) = " + row["Hour"].ToString() + " " +
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
    }
}
