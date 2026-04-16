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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gestion_De_Cours.Classes;
using Gestion_De_Cours.Panels;

namespace Gestion_De_Cours.UControl
{
    /// <summary>
    /// Interaction logic for TeacherPayment.xaml
    /// </summary>
    public partial class TeacherPayment : UserControl
    {
        string type;
        string GID;
        string TID;
        string query = "Select " +
                    "TPayment.ID as ID,Class.CName ," +
                    "dbo.TPaymentGetStudentsAmmount(TPayment.ID) as Students , " +
                    "Class.TPaymentMethod as Type ,  " +
                    "Groups.GroupName as GName ," +
                    "Groups.GroupID as GID," +
                    "TPayment.FromPeriod , " +
                    "TPayment.Periods as Sessions , " +
                    "TPayment.Date as Date, " +
                    "TPayment.TeacherTotal as TPrice , " +
                    "case when Class.TPaymentMethod = 1 then dbo.SumTPaymentSuPrice(TPayment.ID)  " +
                    "when Class.TPaymentMethod = 0 then TPayment.TeacherTotal " +
                    "end  AS SuPrice , " +
                    "case when Class.TPaymentMethod = 1 " +
                    "then dbo.SumTPaymentSPrice(TPayment.ID) " +
                    "end AS SPrice ," +
                    "case when Class.TPaymentMethod = 1 " +
                    "then dbo.SumTPaymentSuPrice(TPayment.ID) " +
                    "when Class.TPaymentMethod = 0 " +
                    "then TeacherTotal " +
                    "end as SupposedTotal   " +
                    "from TPayment join  " +
                    "Teacher on Teacher.ID = TPayment.TID " +
                    "join Groups on Groups.GroupID = TPayment.GID " +
                    "join Class on Class.ID = Groups.ClassID ";
        public TeacherPayment(string t , string TeacherID , string ID2 )
        {
            try
            {
                int lang = Connexion.Language();
                InitializeComponent();
                SetLang();
                TID = TeacherID;
                query += "Where Teacher.ID = " + TeacherID; 
                if (lang == 1)
                {
                    this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
                }
                type = t;
                GID = ID2;
                Connexion.FillDG(ref DGPay, query);
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            SelectAllTeacherPaymentGroup TPG = new SelectAllTeacherPaymentGroup(TID);
            TPG.Show();
        }

        private void Button_Click_Delete(object sender, RoutedEventArgs e)
        {
            if (DGPay.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please Select a Row(s)");
                return;
            }
            string ID; 
            foreach (DataRowView row in DGPay.SelectedItems)
            {
                ID = row["ID"].ToString();
                Connexion.Insert("update Groups Set TSessions = TSessions + " + row["Sessions"].ToString() + " Where GroupID = " + row["GID"].ToString());
                Connexion.Insert("Delete from TPayment Where ID = " + ID);
                if(row["Type"].ToString() == "1")
                {
                    Connexion.Insert("Delete from TPaymentStudent Where ID = " + ID);
                }
                Connexion.InsertHistory(1,row["GID"].ToString(),7);
            }
            MessageBox.Show("Deleted Succesfully.");
            Connexion.FillDG(ref DGPay, query);
        }

        private void Button_Click_Print(object sender, RoutedEventArgs e)
        {
            if (DGPay.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please Select a Row(s)");
                return;
            }
            DataSet ds = new DataSet();
            string path;
            FastReport.Report r = new FastReport.Report();
            int countforreport = 0 ;
            foreach (DataRowView row in DGPay.SelectedItems)
            {
                ds.Clear();
                ds.Tables.Clear();
                if (row["Type"].ToString() == "1")
                {
                    if (Connexion.Language() == 0)//en
                    {
                        path = @"C:\Users\Home\Desktop\C# Projects\Gestion_De_Cours\FastReport\TeacherPaymentPerStudentEN.frx";
                        r.Load(path);

                    }
                    else if (Connexion.Language() == 1)//ar
                    {
                        path = @"C:\Users\Home\Desktop\C# Projects\Gestion_De_Cours\FastReport\TeacherPaymentPerStudentAR" + row["Sessions"].ToString() + ".frx";
                        r.Load(path);
                    }
                    ds = PrintType1(row);
                    r.RegisterData(ds);
                    r.GetDataSource("DataGeneral").Enabled = true;
                    r.GetDataSource("dataDates").Enabled = true;
                    r.GetDataSource("Presense").Enabled = true;
                }
                else if (row["Type"].ToString() == "2")
                {
                    if (Connexion.Language() == 0)
                    {
                        r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\TeacherPaymentPerSesAR.frx");
                    }
                    else if (Connexion.Language() == 1)
                    {
                        r.Load(@"C:\ProgramData\EcoleSetting\EcolePrint" + @"\TeacherPaymentPerSesAR.frx");
                    }
                    ds = PrintType2(row);
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
            if (Commun.FastReportEdit == 0)
            {
                r.Design();
            }
            else
            {
                r.ShowPrepared();
            }
        }
        private DataSet PrintType2(DataRowView row)
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
            DtInfo.Rows[0][1] = row["GName"].ToString();
            DtInfo.Rows[0][2] = Connexion.GetString("Select Teacher.FirstName + ' ' + Teacher.LastName as Name from Teacher Where ID = " + TID);
            DtInfo.Rows[0][3] = row["Date"].ToString();
            DtInfo.Rows[0][4] = price * int.Parse(row["Sessions"].ToString());
            DtInfo.TableName = "Info";
            ds.Tables.Add(DtInfo);
            return ds;
        }

        private DataSet PrintType1(DataRowView row)
        {
            DataSet ds = new DataSet();
            DataTable dtPresense = new DataTable();

            DataTable dtdatesAll = new DataTable();
            int ses = int.Parse(row["Sessions"].ToString());
            int SessionG = int.Parse(row["Fromperiod"].ToString());
            Connexion.FillDT(ref dtdatesAll,
                "Select Attendance.Date as Date  from Attendance Where Session > " + SessionG + " And Session <= " + (SessionG + ses).ToString() + " ANd GroupID = " + row["GID"].ToString() + "   ORDER BY convert(datetime, Attendance.Date, 103)");
            int count = 0;
            DataTable dtDates = new DataTable();
            for (int countfordate = 1; countfordate <= ses; countfordate++)
            {
                dtDates.Columns.Add(("Date" + countfordate).ToString());
            }
            Methods.FillDTAttendance(ses, row["GID"].ToString(), ref dtPresense ,SessionG);
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
            dtDates.TableName = "dataDates";
            dtPresense.TableName = "Presense";
            ds.Tables.Add(dtDates);
            ds.Tables.Add(dtPresense);
            DataTable dtGeneral = new DataTable();
            dtGeneral.Columns.Add("TName");
            dtGeneral.Columns.Add("GName");
            dtGeneral.Columns.Add("Session");
            dtGeneral.Columns.Add("Date");
            dtGeneral.Columns.Add("TStudents");
            dtGeneral.Columns.Add("SuPrice");
            dtGeneral.Columns.Add("SPrice");
            dtGeneral.Columns.Add("TPrice");
            string TName = "";

            TName += Connexion.GetString(TID, "Teacher", "TLastName");
            TName += Connexion.GetString(TID, "Teacher", "TFirstName");
            dtGeneral.Rows.Add(new Object[] { TName, row["GName"].ToString(), ses, row["Date"].ToString(), row["Students"].ToString(), row["SuPrice"].ToString(), row["SPrice"].ToString(), row["TPrice"].ToString() });
            dtGeneral.TableName = "DataGeneral";
            ds.Tables.Add(dtGeneral);
            return ds;
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

    }
}
