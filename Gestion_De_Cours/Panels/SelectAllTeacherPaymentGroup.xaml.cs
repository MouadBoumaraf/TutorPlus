using FastReport;
using Gestion_De_Cours.Classes;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for SelectAllTeacherPaymentGroup.xaml
    /// </summary>
    public partial class SelectAllTeacherPaymentGroup : Window
    {
        string TID;
        DataTable dt2 = new DataTable();
        DataTable dt;
        int l = 1;
        public SelectAllTeacherPaymentGroup(string TeacherID)
        {
            try
            {
                l = 0;
                InitializeComponent();
                l = 1;
                SetLang();
                TID = TeacherID;
                string query =
                    "SELECT 'Normal' as TypeClass ,Class.CName + ' ' + Groups.GroupName AS GroupName, " +
                    "       Class.TPaymentMethod AS Type, " +
                    "       Groups.GroupID AS GID, " +
                    "       Groups.TSessions AS Sessions, " +
                    "       Dbo.GetDateGroup(Groups.Sessions - Groups.TSessions + 1, Groups.GroupID) AS FromSesDate, " +
                    "       Dbo.GetDateGroup(Groups.Sessions, Groups.GroupID) AS ToSesDate, " +
                    "       dbo.TotalStudentSessionsGroup(Groups.Sessions - Groups.TSessions + 1, Groups.Sessions, Groups.GroupID) AS TotalSesTeacher, " +
                    "       Dbo.CalcTotalGTPrice(Groups.Sessions, Groups.GroupID) + dbo.GetTotalOneSesStudents(Groups.Sessions - Groups.TSessions + 1, Groups.Sessions, Groups.GroupID) AS TotalP " +
                    "FROM Groups " +
                    "JOIN Class ON Groups.ClassID = Class.ID " +
                    "WHERE Groups.TSessions != 0 AND Class.TID = " + TID + " " +

                    "UNION " +

                    "SELECT 'Extra' as TypeClass, Class.CName + ' ' + N'" + this.Resources["ExtraSessions"].ToString() + "', " +
                    "       '' AS Type, " +
                    "       '' AS GID, " +
                    "       (SELECT COUNT(*) FROM Attendance_Extra WHERE Paid != 1 AND CID = AE.CID)  AS Sessions, " +
                    "       (SELECT TOP 1 Date FROM Attendance_Extra AE2 WHERE AE2.ID = AE.ID AND ISDATE(SUBSTRING(Date, 7, 4) + '-' + SUBSTRING(Date, 4, 2) + '-' + SUBSTRING(Date, 1, 2)) = 1 " +
                    "        ORDER BY CAST(SUBSTRING(Date, 7, 4) + '-' + SUBSTRING(Date, 4, 2) + '-' + SUBSTRING(Date, 1, 2) AS DATETIME) ASC) AS FromSesDate, " +
                    "       (SELECT TOP 1 Date FROM Attendance_Extra AE3 WHERE AE3.ID = AE.ID AND ISDATE(SUBSTRING(Date, 7, 4) + '-' + SUBSTRING(Date, 4, 2) + '-' + SUBSTRING(Date, 1, 2)) = 1 " +
                    "        ORDER BY CAST(SUBSTRING(Date, 7, 4) + '-' + SUBSTRING(Date, 4, 2) + '-' + SUBSTRING(Date, 1, 2) AS DATETIME) DESC) AS ToSesDate, " +
                    "       (SELECT COUNT(*) FROM Attendance_Extra_Students Join Attendance_Extra on Attendance_Extra_Students.ID = Attendance_Extra.ID WHERE Paid != 1 AND Attendance_Extra_Students.TPrice != 0  and  CID = AE.CID) AS TotalSesTeacher, " +
                    "       (SELECT SUM(Attendance_Extra_Students.TPrice) " +
                    "        FROM Attendance_Extra_Students " +
                    "        JOIN Attendance_Extra ON Attendance_Extra.ID = Attendance_Extra_Students.ID " +
                    "        WHERE Attendance_Extra.Paid = 0 AND Attendance_Extra.CID = AE.CID) AS TotalP " +
                    "FROM Attendance_Extra AE " +
                    "JOIN Class ON Class.ID = AE.CID " +
                    "WHERE Class.TID = " + TID + ";";


                Connexion.FillDG(ref DGGroups, query);
                Connexion.FillDT(ref dt2, query);
                string TName = "";
                TName += Connexion.GetString(TID, "Teacher", "TLastName");
                TName += Connexion.GetString(TID, "Teacher", "TFirstName");
                TBlockTName.Text = TName;
                TBTotalGroups.Text = Methods.GetGroupsTotal(ref DGGroups).ToString();
                Date.Text = DateTime.Today.ToString("d");
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }


        private void DGGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (l != 0)
                {
                    l = 0;
                    CBSes.Items.Clear();
                    AttendanceDG.ItemsSource = null;
                    DataRowView row = (DataRowView)DGGroups.SelectedItem;
                    if (row != null)
                    {
                        if (row["TypeClass"].ToString() == "Normal")
                        {
                            int ses = int.Parse(row["Sessions"].ToString());
                            string GID = row["GID"].ToString();
                            int i = Connexion.GetInt(GID, "Groups", "TSessions", "GroupID");
                            for (int f = 1; f <= i; f++)
                            {
                                CBSes.Items.Add(f);
                            }
                            l = 0;
                            CBSes.SelectedIndex = int.Parse(row["Sessions"].ToString()) - 1;
                            if (row["Type"].ToString() == "1")
                            {
                                ColumSName.Visibility = Visibility.Visible;
                                AttendanceDG.AutoGenerateColumns = false;
                                dt = new DataTable();
                                if (PrintMethod1.IsChecked == true)
                                {
                                    int TotalPaidSes = int.Parse(row["Sessions"].ToString());
                                    int StartSes = Connexion.GetInt("Select Sessions - TSessions from Groups Where GroupID = " + GID);
                                    int EndSes = StartSes + TotalPaidSes;
                                    Methods.FillDGSesStuTPayment(ref DGSessions, StartSes, EndSes, GID, ref dt);

                                }
                                else
                                {
                                    Methods.FillDGAttendance(ref AttendanceDG, ses, GID, ref dt);
                                }

                                TBGroupName.Text = row["GroupName"].ToString();
                                TBTotalGroups.Text = Methods.GetGroupsTotal(ref DGGroups).ToString();
                            }
                            else if (row["Type"].ToString() == "2")
                            {
                                return;
                                ColumSName.Visibility = Visibility.Collapsed;
                                int f = AttendanceDG.Columns.Count();
                                for (int ii = 1; ii < f; ii++)
                                {
                                    AttendanceDG.Columns.RemoveAt(1);
                                }

                                int SessionG =
                                    Connexion.GetSessions(row["GID"].ToString());
                                DataTable dtdates = new DataTable();
                                Connexion.FillDT(ref dtdates, "Select " +
                                    "Attendance.Date as Date , " +
                                    "Attendance.TeacherEntrance " +
                                    "from Attendance " +
                                    "Where Session >=" + SessionG + " " +
                                    "And GroupID = " + row["GID"].ToString() + "   " +
                                    "ORDER BY convert(datetime, Attendance.Date, 103)");
                                int count = 0;
                                DataTable dtmain = new DataTable();
                                dtmain.Columns.Add("Dates");
                                dtmain.Rows.Add(new Object[] { "Entrance Time" });
                                dtmain.Rows.Add(new Object[] { "Price" });

                                foreach (DataRow row2 in dtdates.Rows)
                                {
                                    if (count <= ses)
                                    {
                                        dtmain.Columns.Add(row2["Date"].ToString());
                                        dtmain.Rows[0][count + 1] = row2["TeacherEntrance"].ToString();
                                        count++;
                                    }
                                }
                                string cid = Connexion.GetInt(GID, "Groups", "ClassID", "GroupID").ToString();
                                int total = 0;
                                int price = Connexion.GetInt(cid, "CLass", "TPayment");
                                for (int p = 1; p < dtmain.Columns.Count; p++)
                                {
                                    dtmain.Rows[1][p] = price;
                                    total += price;
                                }
                                AttendanceDG.AutoGenerateColumns = true;
                                //string GID = row["GID"].ToString();
                                string CID = Connexion.GetClassID(GID).ToString();
                                AttendanceDG.ItemsSource = dtmain.DefaultView;

                            }
                        }
                        if(row["TypeClass"].ToString() == "Extra")
                        {

                        }
                    }
                }
                else
                {
                    l = 1;
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void CBSes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)DGGroups.SelectedItem;
                if (row != null)
                {
                    AttendanceDG.ItemsSource = null;
                    if (l != 0)
                    {
                        int ses = CBSes.SelectedIndex + 1;
                        int SelectedIndex = DGGroups.SelectedIndex;
                        int SesMax = Connexion.GetInt("Select TSessions from Groups Where GroupID = "+ row["GID"].ToString());
                        int DifSes = SesMax - ses;
                        int Max = Connexion.GetInt("Select TSessions from Groups Where GroupID = " + row["GID"].ToString()) + ses - 1;
                        int distanceFromLast = CBSes.Items.Count - 1 - CBSes.SelectedIndex;
                        string query = "Select Class.CName + ' ' + Groups.GroupName as GroupName ," +
                        "Class.TPaymentMethod as Type  ," +
                        "Groups.GroupID as GID ," +
                        ""+ ses + " as Sessions ," +
                         "Dbo.GetDateGroup(Groups.Sessions - Groups.TSessions + 1 ,Groups.GroupID) as FromSesDate ," +
                        "Dbo.GetDateGroup(Groups.Sessions -  " + DifSes + ",Groups.GroupID ) as ToSesDate," +
                        "dbo.TotalStudentSessionsGroup(Groups.Sessions - Groups.TSessions + 1 ,Groups.Sessions - "+ distanceFromLast + " ,Groups.GroupID) as TotalSesTeacher," +
                        "Dbo.CalcTotalGTPrice( Groups.Sessions - "+ distanceFromLast + " , Groups.GroupID)  + dbo.GetTotalOneSesStudents(Groups.Sessions - Groups.Tsessions + 1 , Groups.Sessions - " + distanceFromLast + ",Groups.GroupID) as  TotalP  " +
                        "from Groups join Class On Groups.ClassID = Class.ID " +
                        "Where Groups.TSessions != 0 and Groups.GroupID = " + row["GID"].ToString();
                        DataTable dtrow = new DataTable();
                        Connexion.FillDT(ref dtrow, query);
                        dt2.Rows[SelectedIndex][0] = dtrow.Rows[0]["GroupName"]; // GroupName
                                                                               
                        dt2.Rows[SelectedIndex]["GID"] = dtrow.Rows[0]["GID"]; //GID
                        dt2.Rows[SelectedIndex]["Sessions"] = dtrow.Rows[0]["Sessions"];
                        dt2.Rows[SelectedIndex]["ToSesDate"] = dtrow.Rows[0]["ToSesDate"];
                        dt2.Rows[SelectedIndex]["TotalSesTeacher"] = dtrow.Rows[0]["TotalSesTeacher"];
                        dt2.Rows[SelectedIndex]["TotalP"] = dtrow.Rows[0]["TotalP"];
                        string GID = row["GID"].ToString();
                        TBGroupName.Text = dtrow.Rows[0][0].ToString();
                        
                        l = 0;
                        DGGroups.ItemsSource = dt2.DefaultView;
                        l = 0;
                        DGGroups.SelectedIndex = SelectedIndex;
                        ColumSName.Visibility = Visibility.Visible;
                        AttendanceDG.AutoGenerateColumns = false;
                        dt = new DataTable();
                        int TotalPaidSes = int.Parse(dtrow.Rows[0]["Sessions"].ToString());
                        int StartSes = Connexion.GetInt("Select Sessions - TSessions from Groups Where GroupID = " + GID);
                        int EndSes = StartSes + TotalPaidSes;
                        if (PrintMethod1.IsChecked == true)
                        {
                          
                            Methods.FillDGSesStuTPayment(ref DGSessions, StartSes, EndSes, GID , ref dt);
                            
                        }
                        else
                        {
                            Methods.FillDGAttendance(ref AttendanceDG, ses, GID, ref dt);
                        }

                       
                        l = 1;
                    }
                    else
                    {
                        l = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void BtnDelGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)DGGroups.SelectedItem;
                if (row != null)
                {
                    int i = DGGroups.SelectedIndex;
                    l = 0;
                    dt2.Rows[i].Delete();
                    dt2.AcceptChanges();
                    l = 0;
                    DGGroups.ItemsSource = dt2.DefaultView;
                    AttendanceDG.ItemsSource = null;
                    TBGroupName.Text = "";
                   
                    CBSes.Items.Clear();
                    TBTotalGroups.Text = Methods.GetGroupsTotal(ref DGGroups).ToString();
                    l = 1;

                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }

        }

        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                for (int o = 0; o < DGGroups.Items.Count; o++)
                {
                    if (dt2.Rows[o]["Sessions"].ToString() == "0")
                    {
                        continue;
                    }
                    string GID = dt2.Rows[o]["GID"].ToString();
                    string CID = Connexion.GetInt(GID, "Groups", "ClassID", "GroupID").ToString();
                    string TPayMethod = Connexion.GetInt(CID, "Class", "TPaymentMethod").ToString();
                    int TID = Connexion.GetInt(CID, "Class", "TID");
                    SqlConnection con = Connexion.Connect();
                    SqlCommand CommandID = new SqlCommand("Select  case WHen Sum(Ses) is null  then 0 When Sum(Ses) is not null then Sum(Ses) end as S From TPayment Where TID = '" + TID + "' and GID = " + GID, con);
                    CommandID.ExecuteNonQuery();
                    Int32 result = Convert.ToInt32(CommandID.ExecuteScalar());
                    con.Close();
                    int ses = int.Parse(dt2.Rows[o]["Sessions"].ToString());
                    int PID = Connexion.GetInt("Insert into TPayment OUTPUT Inserted.ID Values " +
                        "(" + TID + "," +
                              GID + "," +
                               Date.Text.Replace("/", "-") + ",'" +
                              dt2.Rows[o]["FromSesDate"].ToString() + "','" +
                              dt2.Rows[o]["ToSesDate"].ToString() + "','" +
                              result + "' ," + dt2.Rows[o]["Sessions"].ToString() + ", '" + dt2.Rows[o]["TotalP"].ToString() + "' )");
                    Connexion.InsertHistory(0, PID.ToString(), 7);
                    if (TPayMethod == "1")
                    {
                        Methods.FillDGAttendance(ref AttendanceDG, ses, GID, ref dt);
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            string SID = dt.Rows[i]["ID"].ToString();
                            if (dt.Rows[i]["TSes"].ToString() != "0" && SID != "-1")
                            {
                                Connexion.Insert("Insert into TPaymentStudent Values" +
                               " (" + PID + ","
                                    + SID + ","
                                    + dt.Rows[i]["TSes"].ToString() + ","
                                    + dt.Rows[i]["TPrice"].ToString() + ")");
                            }
                            else if (dt.Rows[i]["TSes"].ToString() == "-1")
                            {

                            }
                        }
                    }
                    Connexion.Insert("Update Groups Set TSessions  = Tsessions - " + ses + " WHere GroupID = " + GID);
                }
                MessageBox.Show("Inserted successfully");
                this.Close();
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void Button_Click_PrintAll(object sender, RoutedEventArgs e)
        {
            Report r = new Report();
            int countforreport = 0;
            DataSet ds = new DataSet();
          
            DataTable dtGeneral = new DataTable();
            string path = "";
            string pathmouath = @"C:\ProgramData\EcoleSetting\Mouathfile.txt";
            if (File.Exists(pathmouath))
            {
                path = @"C:\Users\Home\Desktop\C# Projects\Gestion_De_Cours\FastReport";
            }
            else
            {
                path = @"C:\ProgramData\EcoleSetting\EcolePrint";
            }
            if (PrintMethod2.IsChecked == true)
            {


                foreach (DataRow row in dt2.Rows)
                {
                    ds.Clear();
                    ds.Tables.Clear();
                  
                    if (row["Type"].ToString() == "1")
                    {
                        if (Connexion.Language() == 0)//en
                        {

                            r.Load(path + @"\TeacherPaymentPerStudentEN.frx");

                        }
                        else if (Connexion.Language() == 1)//ar
                        {
                            r.Load(path + @"\TeacherPaymentPerStudentAR5.frx");
                        }
                        ds = PrintType2(row, ref r );
                        r.RegisterData(ds);
                        r.GetDataSource("DataGeneral").Enabled = true;
                        r.GetDataSource("dataDates").Enabled = true;
                        r.GetDataSource("Presense").Enabled = true;
                    }
                    else if (row["Type"].ToString() == "2")
                    {
                        return;
                        if (Connexion.Language() == 0)
                        {
                            r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\TeacherPaymentPerSesAR.frx");
                        }
                        else if (Connexion.Language() == 1)
                        {
                            r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\TeacherPaymentPerSesAR.frx");
                        }
                        ds = PrintType1(row);
                        r.RegisterData(ds);
                        r.GetDataSource("Info").Enabled = true;
                        r.GetDataSource("Payment").Enabled = true;
                    }
                    if (countforreport == 0)
                    {
                        r.Prepare();
                    }
                    else
                    {
                        r.Prepare(true);
                    }
                    countforreport++;
                }
            }
            else
            {
                FastReports.PrintMethod1(dt2, TID, Date.Text);
                return;

               
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

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)DGGroups.SelectedItem;
                if (row == null)
                {
                    return;
                }
                Report r = new Report();
                if(PrintMethod1.IsChecked == true)
                {
                    FastReports.PrintMethod1(row, TID, Date.Text);
                    return;
                }
                if (row["Type"].ToString() == "1")
                {
                    string pathmouath = @"C:\ProgramData\EcoleSetting\Mouathfile.txt";
                    string path = "";
                    if (File.Exists(pathmouath))
                    {
                        path = @"C:\Users\Home\Desktop\C# Projects\Gestion_De_Cours\FastReport";
                    }
                    else
                    {
                        path = @"C:\ProgramData\EcoleSetting\EcolePrint";
                    }

                    if (PrintMethod2.IsChecked == true)
                    {
                        if (Connexion.Language() == 1)
                        {
                            r.Load(path + @"\TeacherPaymentPerStudentAR5.frx");


                        }
                        else if (Connexion.Language() == 0)
                        {
                            r.Load(path + @"\TeacherPaymentPerStudentEN5.frx");
                            //r.Load(Directory.GetCurrentDirectory() + @"/FastReport//TeacherPaymentPerStudentEN.frx");
                        }
                        else if (Connexion.Language() == 2)
                        {

                        }
                        DataSet ds = new DataSet();
                        ds = PrintType2(row,ref r );
                        r.RegisterData(ds);
                        r.GetDataSource("DataGeneral").Enabled = true;
                        r.GetDataSource("dataDates").Enabled = true;
                        r.GetDataSource("Presense").Enabled = true;
                    }
                    else
                    {
                        if (Connexion.Language() == 1)
                        {
                            r.Load(path + @"\TeacherPaymentPerStudentModule2AR.frx");



                        }
                        else if (Connexion.Language() == 0)
                        {
                            r.Load(path + @"\TeacherPaymentPerStudentModule2EN.frx");
                            //r.Load(Directory.GetCurrentDirectory() + @"/FastReport//TeacherPaymentPerStudentEN.frx");
                        }
                        else if (Connexion.Language() == 2)
                        {

                        }
                    }


                }
                else if (row["Type"].ToString() == "1")
                {
                   
                 
                    return;
                }
                if (Commun.FastReportEdit == 0)
                {
                    r.Design(); 
                }
                else
                {
                    r.Show();
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }

        }

        private DataSet PrintType1(DataRow row)
        {
            DataSet ds = new DataSet();
            DataTable dtdates = new DataTable();
            int SessionG = Connexion.GetSessions(row["GID"].ToString());
            Connexion.FillDT(ref dtdates, "Select " +
                "Attendance.Date as Date , " +
                "Attendance.TeacherEntrance " +
                "from Attendance " +
                "Where Session >" + SessionG + " " +
                "And GroupID = " + row["GID"].ToString() + "   " +
                "ORDER BY convert(datetime, Attendance.Date, 103)");
            DataTable DtPayment = new DataTable();
            DtPayment.Columns.Add("Date1");
            DtPayment.Columns.Add("Date2");
            DtPayment.Columns.Add("Date3");
            DtPayment.Columns.Add("Date4");
            DtPayment.Columns.Add("Date5");
            DtPayment.Columns.Add("Date6");
            DtPayment.Columns.Add("TEntrance1");
            DtPayment.Columns.Add("TEntrance2");
            DtPayment.Columns.Add("TEntrance3");
            DtPayment.Columns.Add("TEntrance4");
            DtPayment.Columns.Add("TEntrance5");
            DtPayment.Columns.Add("TEntrance6");
            DtPayment.Columns.Add("Price1");
            DtPayment.Columns.Add("Price2");
            DtPayment.Columns.Add("Price3");
            DtPayment.Columns.Add("Price4");
            DtPayment.Columns.Add("Price5");
            DtPayment.Columns.Add("Price6");
            DtPayment.Rows.Add();
            int cid = Connexion.GetClassID(row["GID"].ToString());
            int price = Connexion.GetInt(cid.ToString(), "CLass", "TPayment");
            for (int counter = 0; counter < dtdates.Rows.Count; counter++)
            {
                DtPayment.Rows[0][counter] = dtdates.Rows[counter][0];
                DtPayment.Rows[0][counter + 6] = dtdates.Rows[counter][1];
                DtPayment.Rows[0][counter + 12] = price;
            }
            DtPayment.TableName = "Payment";
            ds.Tables.Add(DtPayment);
            DataTable DtInfo = new DataTable();
            DtInfo.Columns.Add("CName");
            DtInfo.Columns.Add("GName");
            DtInfo.Columns.Add("TName");
            DtInfo.Columns.Add("Date");
            DtInfo.Columns.Add("Total");
            DtInfo.Rows.Add();
            DtInfo.Rows[0][0] = Connexion.GetString(cid.ToString(), "Class", "CName");
            DtInfo.Rows[0][1] = TBGroupName.Text;
            DtInfo.Rows[0][2] = TBlockTName.Text;
            DtInfo.Rows[0][3] = Date.Text;
            DtInfo.Rows[0][4] = price * (CBSes.SelectedIndex + 1);
            DtInfo.TableName = "Info";
            ds.Tables.Add(DtInfo);
            return ds;
        }

        private DataSet PrintType1(DataRowView row)
        {
            DataSet ds = new DataSet();
            DataTable dtdates = new DataTable();
            int SessionG = Connexion.GetSessions(row["GID"].ToString());
            Connexion.FillDT(ref dtdates, "Select " +
                "Attendance.Date as Date , " +
                "Attendance.TeacherEntrance " +
                "from Attendance " +
                "Where Session >" + SessionG + " " +
                "And GroupID = " + row["GID"].ToString() + "   " +
                "ORDER BY convert(datetime, Attendance.Date, 103)");
            DataTable DtPayment = new DataTable();
            DtPayment.Columns.Add("Date1");
            DtPayment.Columns.Add("Date2");
            DtPayment.Columns.Add("Date3");
            DtPayment.Columns.Add("Date4");
            DtPayment.Columns.Add("Date5");
            DtPayment.Columns.Add("Date6");
            DtPayment.Columns.Add("TEntrance1");
            DtPayment.Columns.Add("TEntrance2");
            DtPayment.Columns.Add("TEntrance3");
            DtPayment.Columns.Add("TEntrance4");
            DtPayment.Columns.Add("TEntrance5");
            DtPayment.Columns.Add("TEntrance6");
            DtPayment.Columns.Add("Price1");
            DtPayment.Columns.Add("Price2");
            DtPayment.Columns.Add("Price3");
            DtPayment.Columns.Add("Price4");
            DtPayment.Columns.Add("Price5");
            DtPayment.Columns.Add("Price6");
            DtPayment.Rows.Add();
            int cid = Connexion.GetClassID(row["GID"].ToString());
            int price = Connexion.GetInt(cid.ToString(), "CLass", "TPayment");
            for (int counter = 0; counter < dtdates.Rows.Count; counter++)
            {
                DtPayment.Rows[0][counter] = dtdates.Rows[counter][0];
                DtPayment.Rows[0][counter + 6] = dtdates.Rows[counter][1];
                DtPayment.Rows[0][counter + 12] = price;
            }
            DtPayment.TableName = "Payment";
            ds.Tables.Add(DtPayment);
            DataTable DtInfo = new DataTable();
            DtInfo.Columns.Add("CName");
            DtInfo.Columns.Add("GName");
            DtInfo.Columns.Add("TName");
            DtInfo.Columns.Add("Date");
            DtInfo.Columns.Add("Total");
            DtInfo.Rows.Add();
            DtInfo.Rows[0][0] = Connexion.GetString(cid.ToString(), "Class", "CName");
            DtInfo.Rows[0][1] = TBGroupName.Text;
            DtInfo.Rows[0][2] = TBlockTName.Text;
            DtInfo.Rows[0][3] = Date.Text;
            DtInfo.Rows[0][4] = price * (CBSes.SelectedIndex + 1);
            DtInfo.TableName = "Info";
            ds.Tables.Add(DtInfo);
            return ds;
        }

        private DataSet PrintType2(DataRowView row, ref Report r )
        {
            DataSet ds = new DataSet();
            DataTable dtPresense = new DataTable();

            DataTable dtdatesAll = new DataTable();
            int ses = int.Parse(row["Sessions"].ToString());
            int SessionG = Connexion.GetSessions(row["GID"].ToString());
            Connexion.FillDT(ref dtdatesAll,
                "Select Attendance.Date as Date  from Attendance Where Session > " + SessionG + " And Session <= " + (SessionG + ses).ToString() + " ANd GroupID = " + row["GID"].ToString() + "   ORDER BY convert(datetime, Attendance.Date, 103)");
            int count = 0;
            DataTable dtDates = new DataTable();
            for (int countfordate = 1; countfordate <= ses; countfordate++)
            {
                dtDates.Columns.Add(("Date" + countfordate).ToString());
            }
            int SessionGG = Connexion.GetSessions(row["GID"].ToString());
            Methods.FillDTAttendance(ses, row["GID"].ToString(), ref dtPresense, SessionGG);
            dtDates.Rows.Add();
            foreach (DataRow row2 in dtdatesAll.Rows)
            {
                if (count <= ses)
                {
                    dtDates.Rows[0][count] = row2["Date"].ToString();
                    int f = count + 1;
                    dtPresense.Columns[f + 6].ColumnName = "Date" + f;
                    count++;
                }
            }

            if (ses == 7)
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "[dataDates.Date2]";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.Date2]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Text = "[dataDates.Date3]";
                CDate3.Visible = true;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "[Presense.Date3]";
                Date3.Visible = true;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate4.Text = "[dataDates.Date4]";
                CDate4.Visible = true;
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "[Presense.Date4]";
                Date4.Visible = true;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
                CDate5.Text = "[dataDates.Date5]";
                CDate5.Visible = true;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "[Presense.Date5]";
                Date5.Visible = true;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
                CDate6.Text = "[dataDates.Date6]";
                CDate6.Visible = true;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "[Presense.Date6]";
                Date6.Visible = true;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
                CDate7.Text = "[dataDates.Date7]";
                CDate7.Visible = true;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "[Presense.Date7]";
                Date7.Visible = true;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = true;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = true;
            }
            else if (ses == 6)
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "[dataDates.Date2]";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.Date2]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Text = "[dataDates.Date3]";
                CDate3.Visible = true;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "[Presense.Date3]";
                Date3.Visible = true;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate4.Text = "[dataDates.Date4]";
                CDate4.Visible = true;
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "[Presense.Date4]";
                Date4.Visible = true;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
                CDate5.Text = "[dataDates.Date5]";
                CDate5.Visible = true;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "[Presense.Date5]";
                Date5.Visible = true;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
                CDate6.Text = "[dataDates.Date6]";
                CDate6.Visible = true;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "[Presense.Date6]";
                Date6.Visible = true;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
                CDate7.Text = "سعر الأستاذ";
                CDate7.Visible = true;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "[Presense.TPrice]";
                Date7.Visible = true;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = false;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = false;

            }
            else if (ses == 5)
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "[dataDates.Date2]";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.Date2]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Text = "[dataDates.Date3]";
                CDate3.Visible = true;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "[Presense.Date3]";
                Date3.Visible = true;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate4.Text = "[dataDates.Date4]";
                CDate4.Visible = true;
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "[Presense.Date4]";
                Date4.Visible = true;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
                CDate5.Text = "[dataDates.Date5]";
                CDate5.Visible = true;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "[Presense.Date5]";
                Date5.Visible = true;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
                CDate6.Text = "سعر الأستاذ";
                CDate6.Visible = true;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "[Presense.TPrice]";
                Date6.Visible = true;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
                CDate7.Visible = false;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "";
                Date7.Visible = false;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = false;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = false;
            }
            else if (ses == 4)
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "[dataDates.Date2]";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.Date2]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Text = "[dataDates.Date3]";
                CDate3.Visible = true;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "[Presense.Date3]";
                Date3.Visible = true;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate4.Text = "[dataDates.Date4]";
                CDate4.Visible = true;
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "[Presense.Date4]";
                Date4.Visible = true;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
                CDate5.Text = "سعر الأستاذ";
                CDate5.Visible = true;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "[Presense.TPrice]";
                Date5.Visible = true;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
                CDate6.Visible = false;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "";
                Date6.Visible = false;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
                CDate7.Visible = false;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "";
                Date7.Visible = false;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = false;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = false;
            }
            else if (ses == 3)
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "[dataDates.Date2]";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.Date2]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Text = "[dataDates.Date3]";
                CDate3.Visible = true;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "[Presense.Date3]";
                Date3.Visible = true;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate4.Text = "سعر الأستاذ";
                CDate4.Visible = true;
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "[Presense.TPrice]";
                Date4.Visible = true;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
                CDate5.Visible = false;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "";
                Date5.Visible = false;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
                CDate6.Visible = false;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "";
                Date6.Visible = false;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
                CDate7.Visible = false;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "";
                Date7.Visible = false;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = false;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = false;
            }
            else if (ses == 2)
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "[dataDates.Date2]";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.Date2]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Text = "سعر الأستاذ";
                CDate3.Visible = true;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "[Presense.TPrice]";
                Date3.Visible = true;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate4.Visible = false;
                CDate4.Text = "";
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "";
                Date4.Visible = false;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
                CDate5.Visible = false;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "";
                Date5.Visible = false;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
                CDate6.Visible = false;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "";
                Date6.Visible = false;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
                CDate7.Visible = false;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "";
                Date7.Visible = false;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = false;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = false;
            }
            else
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "سعر الأستاذ";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.TPrice]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Visible = false;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "";
                Date3.Visible = false;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate3.Text = "";
                CDate4.Visible = false;
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "";
                Date4.Visible = false;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
              
                CDate5.Visible = false;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "";
                Date5.Visible = false;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
              
                CDate6.Visible = false;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "";
                Date6.Visible = false;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
               
                CDate7.Visible = false;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "";
                Date7.Visible = false;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = false;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = false;
            }
            dtDates.TableName = "dataDates";
            dtPresense.TableName = "Presense";
            ds.Tables.Add(dtDates);
            ds.Tables.Add(dtPresense);
            DataTable dtGeneral = new DataTable();
            dtGeneral.Columns.Add("TName");
            dtGeneral.Columns.Add("GName");
            dtGeneral.Columns.Add("Date");
            dtGeneral.Columns.Add("FromDate");
            dtGeneral.Columns.Add("ToDate");
            dtGeneral.Columns.Add("TPrice");
        
            string TName = "";

            TName += Connexion.GetString(TID, "Teacher", "TLastName");
            TName += Connexion.GetString(TID, "Teacher", "TFirstName");
            dtGeneral.Rows.Add(new Object[] { TName, row["GroupName"].ToString(), Date.Text, row["FromSesDate"].ToString(), row["ToSesDate"].ToString(), row["TotalP"].ToString() });
            dtGeneral.TableName = "DataGeneral";
            ds.Tables.Add(dtGeneral.Copy());
            return ds;
        }

        private DataSet PrintType2(DataRow row, ref Report r )
        {
            DataSet ds = new DataSet();
            DataTable dtPresense = new DataTable();

            DataTable dtdatesAll = new DataTable();
            int ses = int.Parse(row["Sessions"].ToString());
            int SessionG = Connexion.GetSessions(row["GID"].ToString());
            Connexion.FillDT(ref dtdatesAll,
                "Select Attendance.Date as Date  from Attendance Where Session > " + SessionG + " And Session <= " + (SessionG + ses).ToString() + " ANd GroupID = " + row["GID"].ToString() + "   ORDER BY convert(datetime, Attendance.Date, 103)");
            int count = 0;
            DataTable dtDates = new DataTable();
            for (int countfordate = 1; countfordate <= ses; countfordate++)
            {
                dtDates.Columns.Add(("Date" + countfordate).ToString());
            }
            int SessionGG = Connexion.GetSessions(row["GID"].ToString());
            Methods.FillDTAttendance(ses, row["GID"].ToString(), ref dtPresense, SessionGG);
            dtDates.Rows.Add();
            foreach (DataRow row2 in dtdatesAll.Rows)
            {
                if (count <= ses)
                {
                    dtDates.Rows[0][count] = row2["Date"].ToString();
                    int f = count + 1;
                    dtPresense.Columns[f + 6].ColumnName = "Date" + f;
                    count++;
                }// changethe columns visibility depending on ses 
            }
            if(ses == 7)
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "[dataDates.Date2]";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.Date2]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Text = "[dataDates.Date3]";
                CDate3.Visible = true;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "[Presense.Date3]";
                Date3.Visible = true;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate4.Text = "[dataDates.Date4]";
                CDate4.Visible = true;
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "[Presense.Date4]";
                Date4.Visible = true;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
                CDate5.Text = "[dataDates.Date5]";
                CDate5.Visible = true;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "[Presense.Date5]";
                Date5.Visible = true;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
                CDate6.Text = "[dataDates.Date6]";
                CDate6.Visible = true;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "[Presense.Date6]";
                Date6.Visible = true;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
                CDate7.Text = "[dataDates.Date7]";
                CDate7.Visible = true;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "[Presense.Date7]";
                Date7.Visible = true;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = true;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = true;
            }
            else if (ses == 6)
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "[dataDates.Date2]";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.Date2]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Text = "[dataDates.Date3]";
                CDate3.Visible = true;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "[Presense.Date3]";
                Date3.Visible = true;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate4.Text = "[dataDates.Date4]";
                CDate4.Visible = true;
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "[Presense.Date4]";
                Date4.Visible = true;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
                CDate5.Text = "[dataDates.Date5]";
                CDate5.Visible = true;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "[Presense.Date5]";
                Date5.Visible = true;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
                CDate6.Text = "[dataDates.Date6]";
                CDate6.Visible = true;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "[Presense.Date6]";
                Date6.Visible = true;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
                CDate7.Text = "سعر الأستاذ";
                CDate7.Visible = true;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "[Presense.TPrice]";
                Date7.Visible = true;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = false;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = false;

            }
            else if (ses == 5)
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "[dataDates.Date2]";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.Date2]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Text = "[dataDates.Date3]";
                CDate3.Visible = true;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "[Presense.Date3]";
                Date3.Visible = true;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate4.Text = "[dataDates.Date4]";
                CDate4.Visible = true;
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "[Presense.Date4]";
                Date4.Visible = true;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
                CDate5.Text = "[dataDates.Date5]";
                CDate5.Visible = true;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "[Presense.Date5]";
                Date5.Visible = true;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
                CDate6.Text = "سعر الأستاذ";
                CDate6.Visible = true;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "[Presense.TPrice]";
                Date6.Visible = true;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
                CDate7.Visible = false;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "";
                Date7.Visible = false;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = false;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = false;
            }
            else if (ses == 4)
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "[dataDates.Date2]";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.Date2]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Text = "[dataDates.Date3]";
                CDate3.Visible = true;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "[Presense.Date3]";
                Date3.Visible = true;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate4.Text = "[dataDates.Date4]";
                CDate4.Visible = true;
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "[Presense.Date4]";
                Date4.Visible = true;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
                CDate5.Text = "سعر الأستاذ";
                CDate5.Visible = true;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "[Presense.TPrice]";
                Date5.Visible = true;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
                CDate6.Visible = false;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "";
                Date6.Visible = false;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
                CDate7.Visible = false;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "";
                Date7.Visible = false;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = false;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = false;
            }
            else if (ses == 3)
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "[dataDates.Date2]";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.Date2]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Text = "[dataDates.Date3]";
                CDate3.Visible = true;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "[Presense.Date3]";
                Date3.Visible = true;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate4.Text = "سعر الأستاذ";
                CDate4.Visible = true;
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "[Presense.TPrice]";
                Date4.Visible = true;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
                CDate5.Visible = false;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "";
                Date5.Visible = false;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
                CDate6.Visible = false;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "";
                Date6.Visible = false;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
                CDate7.Visible = false;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "";
                Date7.Visible = false;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = false;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = false;
            }
            else if (ses == 2)
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "[dataDates.Date2]";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.Date2]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Text = "سعر الأستاذ";
                CDate3.Visible = true;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "[Presense.TPrice]";
                Date3.Visible = true;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate4.Visible = false;
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "";
                Date4.Visible = false;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
                CDate5.Visible = false;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "";
                Date5.Visible = false;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
                CDate6.Visible = false;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "";
                Date6.Visible = false;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
                CDate7.Visible = false;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "";
                Date7.Visible = false;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = false;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = false;
            }
            else if(ses == 1)
            {
                TextObject CDate2 = r.FindObject("CDate2") as TextObject;
                CDate2.Text = "سعر الأستاذ";
                CDate2.Visible = true;
                TextObject Date2 = r.FindObject("Date2") as TextObject;
                Date2.Text = "[Presense.TPrice]";
                Date2.Visible = true;
                TextObject CDate3 = r.FindObject("CDate3") as TextObject;
                CDate3.Visible = false;
                TextObject Date3 = r.FindObject("Date3") as TextObject;
                Date3.Text = "";
                Date3.Visible = false;
                TextObject CDate4 = r.FindObject("CDate4") as TextObject;
                CDate4.Visible = false;
                TextObject Date4 = r.FindObject("Date4") as TextObject;
                Date4.Text = "";
                Date4.Visible = false;
                TextObject CDate5 = r.FindObject("CDate5") as TextObject;
                CDate5.Visible = false;
                TextObject Date5 = r.FindObject("Date5") as TextObject;
                Date5.Text = "";
                Date5.Visible = false;
                TextObject CDate6 = r.FindObject("CDate6") as TextObject;
                CDate6.Visible = false;
                TextObject Date6 = r.FindObject("Date6") as TextObject;
                Date6.Text = "";
                Date6.Visible = false;
                TextObject CDate7 = r.FindObject("CDate7") as TextObject;
                CDate7.Visible = false;
                TextObject Date7 = r.FindObject("Date7") as TextObject;
                Date7.Text = "";
                Date7.Visible = false;
                TextObject CTPrice = r.FindObject("CTPrice") as TextObject;
                CTPrice.Visible = false;
                TextObject TPrice = r.FindObject("TPrice") as TextObject;
                TPrice.Text = "[Presense.TPrice]";
                TPrice.Visible = false;
            }
            dtDates.TableName = "dataDates";
            dtPresense.TableName = "Presense";
            ds.Tables.Add(dtDates);
            ds.Tables.Add(dtPresense);
            DataTable dtGeneral = new DataTable();
            dtGeneral.Columns.Add("TName");
            dtGeneral.Columns.Add("GName");
            dtGeneral.Columns.Add("Date");
            dtGeneral.Columns.Add("FromDate");
            dtGeneral.Columns.Add("ToDate");
            dtGeneral.Columns.Add("TPrice");

            string TName = "";

            TName += Connexion.GetString(TID, "Teacher", "TLastName");
            TName += Connexion.GetString(TID, "Teacher", "TFirstName");
            dtGeneral.Rows.Add(new Object[] { TName, row["GroupName"].ToString(), Date.Text, row["FromSesDate"].ToString(), row["ToSesDate"].ToString(), row["TotalP"].ToString() });
            dtGeneral.TableName = "DataGeneral";
            ds.Tables.Add(dtGeneral.Copy());
            return ds;
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

        private void PrintMethod1_Checked(object sender, RoutedEventArgs e)
        {
            if (l == 1)
            {
                AttendanceDG.Visibility = Visibility.Collapsed;
                DGSessions.Visibility = Visibility.Visible;
                DataRowView row = (DataRowView)DGGroups.SelectedItem;
                if (row != null)
                {
                    AttendanceDG.ItemsSource = null;
                    if (l != 0)
                    {
                        int ses = CBSes.SelectedIndex + 1;
                        int SelectedIndex = DGGroups.SelectedIndex;
                        int SesMax = Connexion.GetInt("Select TSessions from Groups Where GroupID = " + row["GID"].ToString());
                        int DifSes = SesMax - ses;
                        int Max = Connexion.GetInt("Select TSessions from Groups Where GroupID = " + row["GID"].ToString()) + ses - 1;
                        
                        dt2.Rows[SelectedIndex][0] = row["GroupName"]; // GroupName

                        dt2.Rows[SelectedIndex]["GID"] = row["GID"]; //GID
                        dt2.Rows[SelectedIndex]["Sessions"] = row["Sessions"];
                        dt2.Rows[SelectedIndex]["ToSesDate"] = row["ToSesDate"];
                        dt2.Rows[SelectedIndex]["TotalSesTeacher"] = row["TotalSesTeacher"];
                        dt2.Rows[SelectedIndex]["TotalP"] = row["TotalP"];
                        string GID = row["GID"].ToString();
                        TBGroupName.Text = row[0].ToString();

                        l = 0;
                        DGGroups.ItemsSource = dt2.DefaultView;
                        l = 0;
                        DGGroups.SelectedIndex = SelectedIndex;
                        ColumSName.Visibility = Visibility.Visible;
                        AttendanceDG.AutoGenerateColumns = false;
                        dt = new DataTable();
                        int TotalPaidSes = int.Parse(row["Sessions"].ToString());
                        int StartSes = Connexion.GetInt("Select Sessions - TSessions from Groups Where GroupID = " + GID);
                        int EndSes = StartSes + TotalPaidSes;
                        if (PrintMethod1.IsChecked == true)
                        {

                            Methods.FillDGSesStuTPayment(ref DGSessions, StartSes, EndSes, GID, ref dt);

                        }
                        else
                        {
                            Methods.FillDGAttendance(ref AttendanceDG, ses, GID, ref dt);
                        }


                        l = 1;
                    }
                    else
                    {
                        l = 1;
                    }
                }
            }
        }

        private void PrintMethod2_Checked(object sender, RoutedEventArgs e)
        {
            if(l== 1)
            {
                AttendanceDG.Visibility = Visibility.Visible;
                DGSessions.Visibility = Visibility.Collapsed;
                DataRowView row = (DataRowView)DGGroups.SelectedItem;
                if (row != null)
                {
                    AttendanceDG.ItemsSource = null;
                    if (l != 0)
                    {
                        int ses = CBSes.SelectedIndex + 1;
                        int SelectedIndex = DGGroups.SelectedIndex;
                        int SesMax = Connexion.GetInt("Select TSessions from Groups Where GroupID = " + row["GID"].ToString());
                        int DifSes = SesMax - ses;
                        int Max = Connexion.GetInt("Select TSessions from Groups Where GroupID = " + row["GID"].ToString()) + ses - 1;
                     
                        dt2.Rows[SelectedIndex][0] = row["GroupName"]; // GroupName

                        dt2.Rows[SelectedIndex]["GID"] = row["GID"]; //GID
                        dt2.Rows[SelectedIndex]["Sessions"] = row["Sessions"];
                        dt2.Rows[SelectedIndex]["ToSesDate"] = row["ToSesDate"];
                        dt2.Rows[SelectedIndex]["TotalSesTeacher"] = row["TotalSesTeacher"];
                        dt2.Rows[SelectedIndex]["TotalP"] = row["TotalP"];
                        string GID = row["GID"].ToString();
                        TBGroupName.Text = row[0].ToString();

                        l = 0;
                        DGGroups.ItemsSource = dt2.DefaultView;
                        l = 0;
                        DGGroups.SelectedIndex = SelectedIndex;
                        ColumSName.Visibility = Visibility.Visible;
                        AttendanceDG.AutoGenerateColumns = false;
                        dt = new DataTable();
                        int TotalPaidSes = int.Parse(row["Sessions"].ToString());
                        int StartSes = Connexion.GetInt("Select Sessions - TSessions from Groups Where GroupID = " + GID);
                        int EndSes = StartSes + TotalPaidSes;
                        if (PrintMethod1.IsChecked == true)
                        {

                            Methods.FillDGSesStuTPayment(ref DGSessions, StartSes, EndSes, GID, ref dt);

                        }
                        else
                        {
                            Methods.FillDGAttendance(ref AttendanceDG, ses, GID, ref dt);
                        }


                        l = 1;
                    }
                    else
                    {
                        l = 1;
                    }
                }
            }
        }
    }
}
