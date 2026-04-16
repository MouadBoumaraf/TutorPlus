using FastReport;
using Gestion_De_Cours.Classes;
using Gestion_De_Cours.Panels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gestion_De_Cours.UControl
{
    /// <summary>
    /// Interaction logic for CashRegister.xaml
    /// </summary>
    public partial class CashRegister : UserControl
    {
        DataTable dt = new DataTable();
        string query = "";
        string IDCR = "";
        string condition = "1 > 0 ";
        public CashRegister()
        {
            try
            {

                InitializeComponent();
                SetLang();
                IDCR = Connexion.GetInt("Select ID from CashRegister Where Date = '" + DateTime.Today.ToString("dd-MM-yyyy") + "'").ToString();
                LVCash.Height = Commun.ScreenHeight - 310;
                TBInitial.Text = Connexion.GetInt("Select StartAmmount from CashRegister Where ID = " + IDCR + " ").ToString();




                query = "Select  *  from (" +
               "Select '1' as Typee ,  " +
               "StudentPayment.ID as ID , " +
               "'Class Payment' as Type , " +
               "Students.FirstName + ' ' + Students.LastName as Name , " +
               "Students.LastName + ' ' + Students.FirstName as RName, " +
               "Class.CName as ClassName ," +
               "Workers.FirstName + ' ' + Workers.LastName as WorkerName ," +
               "Workers.LastName + ' ' + Workers.FirstName as WorkerRName , " +
               "Price as Intake ," +
               "null  as OutTake , " +
               "Replace(LEFT (StudentPayment.Date, 10),'/','-') as DateSearch ," +
               "convert(varchar, Studentpayment.date, 5) as Date " +
               "from StudentPayment  " +
               "Join Class on Class.ID = StudentPayment.CID " +
               "Join Students on StudentPayment.SID = Students.ID " +
               "left Join History on History.ID1 = StudentPayment.ID " +
               "left Join Workers on Workers.ID = History.WorkerID " +
               "Where History.Type2 = 8 and StudentPayment.Type = 1  and StudentPayment.Deleted = 0  " +
               "Union " +
               "Select '0' as Typee , " +
               "StudentPayment.ID as ID , " +
               "'Monthly Payment' as Type , " +
               "Students.FirstName + ' ' + Students.LastName as Name , " +
               "Students.LastName + ' ' + Students.FirstName as RName , " +
              "CONVERT(varchar,Monthly_Payment.Month) + '/' + CONVERT(varchar,Monthly_Payment.Year) as CName ," +
             "Workers.FirstName + ' ' + Workers.LastName as WorkerName ," +
             "Workers.LastName + ' ' + Workers.FirstName as WorkerRName , " +
             "Price as Intake ," +
             "null  as OutTake , " +
             "Replace(LEFT (StudentPayment.Date, 10),'/','-') as DateSearch ," +
             "convert(varchar, Studentpayment.date, 5) as Date " +
             "from StudentPayment  " +
             "Join Monthly_Payment on Monthly_Payment.ID = StudentPayment.CID " +
             "Join Students on StudentPayment.SID = Students.ID " +
             "left Join History on History.ID1 = StudentPayment.ID " +
             "left Join Workers on Workers.ID = History.WorkerID " +
             "Where History.Type2 = 8 and StudentPayment.Type = 4  and StudentPayment.Deleted = 0  Union " +
               "Select '2' as Typee , CashRegisterExtra.ID as ID , " +
               "Name as type  ," +
               "'' as Name ," +
               "'' as RName ," +
               "'' as ClassName ," +
               "Workers.FirstName + ' ' + Workers.LastName as WorkerName  ," +
               "Workers.LastName + ' ' + Workers.FirstName as WorkerRName, " +
               "case When Ammount > 0 then Ammount end as InTake , " +
               "Case When Ammount < 0 then Ammount end as OutTake ," +
               "convert(varchar, CashRegister.Date, 23) as DateSearch," +
               "CashRegister.Date + ' ' + CashRegisterExtra.Time as Date " +
              "from CashRegister " +
              "Join CashRegisterExtra on CashRegister.ID = CashRegisterExtra.IDCR" + " join Workers on Workers.ID = CashRegisterExtra.WID " +
             "Union " +
             "Select '3' as Typee ," +
             "StudentPayment.ID as ID ," +
             "'Formation Payment' as Type ," +
             "Students.FirstName + ' ' + Students.LastName as Name , " +
             "Students.LastName + ' ' + Students.FirstName as RName," +
             "Formation.Name as ClassName ," +
             "Workers.FirstName + ' ' + Workers.LastName as WorkerName  ," +
             "Workers.LastName + ' ' + Workers.FirstName as WorkerRName," +
             "StudentPayment.Price as Intake ," +
             "null  as OutTake , " +
             "Replace(LEFT (StudentPayment.Date, 10),'/','-') as DateSearch ," +
             "convert(varchar, Studentpayment.date, 5) as Date  from StudentPayment " +
             "Join Formation on Formation.ID = StudentPayment.CID " +
             "Join Students on StudentPayment.SID = Students.ID " +
             "left Join History on History.ID1 = StudentPayment.ID " +
             "left Join Workers on Workers.ID = History.WorkerID " +
             "Where History.Type2 = 13 and StudentPayment.Type = 3  and StudentPayment.Deleted = 0 " +
             "union Select '4' as Typee , " +
             "Attendance_StudentsOneSes.ID as ID ," +
             "'Student Payment' as Type   ," +
             "Attendance_StudentsOneSes.Name as Name ," +
             "Attendance_StudentsOneSes.Name as RName , " +
             "Class.CName as ClassName , " +
              "Workers.FirstName + ' ' + Workers.LastName as WorkerName  ," +
             "Workers.LastName + ' ' + Workers.FirstName as WorkerRName , " +
             "Attendance_StudentsOneSes.Price as Intake ," +
             "null as OutTake ," +
             "Attendance.Date as DateSearch , " +
             "convert(varchar, Attendance.date, 5) as Date from  Attendance_StudentsOneSes " +
             "Join Attendance On Attendance_StudentsOneSes.AID = Attendance.ID join Groups on Groups.GroupID = Attendance.GroupID Join Class on Class.ID= Groups.ClassID "+
                "left Join History on History.ID1 = Attendance_StudentsOneSes.ID " +
             "left Join Workers on Workers.ID = History.WorkerID Where History.Type2 = 15 " +
             ") as f";


                Connexion.FillDT(ref dt, query);
                LVCash.ItemsSource = dt.DefaultView;
                DPFrom.Text = DateTime.Today.ToString("dd/MM/yyyy");

            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you Sure you want to This Payment?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach (DataRowView row in LVCash.SelectedItems)
                {
                    if (row["Typee"].ToString() == "1")
                    {
                        if (Connexion.GetInt(Connexion.WorkerID, "Users", "SPayD") != 1)
                        {
                            MessageBox.Show("Sorry you don't have privilage for this action");
                            return;
                        }
                        Connexion.Insert("Delete from StudentPayment Where ID = " + row["ID"].ToString());
                    }
                    else if (row["Typee"].ToString() == "0")
                    {
                        string MonthID = Connexion.GetString("Select CID  from StudentPayment where ID = " + row["ID"].ToString());
                        int Sum = Connexion.GetInt("Select Case When Sum(Price) is null then 0 else Sum(Price) end as f from StudentPayment Where Type = 4 and CID = " + MonthID);
                        int PriceDeleted = int.Parse(row["InTake"].ToString());
                        if (MonthID != null)
                        {

                            int status;
                            if (Sum == PriceDeleted)
                            {
                                status = 0;
                                Connexion.Insert("Update Monthly_Payment Set status = 0 where ID =" + MonthID);
                            }
                            else
                            {
                                status = 2;
                                Connexion.Insert("Update Monthly_Payment Set status = 2 where ID =" + MonthID);
                            }
                            Connexion.Insert("Delete from StudentPayment Where ID = " + row["ID"].ToString());


                        }
                    }
                    else if (row["Typee"].ToString() == "2")
                    {
                        Connexion.Insert("Delete from CashRegisterExtra Where ID = " + row["ID"].ToString());
                    }
                }
                Connexion.FillDT(ref dt, query);
                LVCash.ItemsSource = dt.DefaultView;
                Search();
            }
        }

        private void Selection(object sender, SelectionChangedEventArgs e)
        {
            Search();
        }

        private void Search()
        {
            try
            {

                DateTime dtfrominitial = DateTime.Parse(DPFrom.Text);
                string frominitial = dtfrominitial.ToString("dd-MM-yyyy");
                TBInitial.Text = Connexion.GetInt("Select Case When StartAmmount is null then 0 when StartAmmount is not null then StartAmmount end as f from CashRegister Where Date = '" + frominitial + "'").ToString();
                condition = "1 > 0 ";
                if(Connexion.IFNULL("Select * from CashRegister Where Date = '" + frominitial + "'"))
                {
                    Connexion.GetInt("insert into CashRegister Output Inserted.ID Values ('" + frominitial + "',null )");
                }
                IDCR = Connexion.GetInt("Select ID from CashRegister Where Date = '" + frominitial + "'").ToString();
                if (DPFrom.Text != "")
                {
                    DateTime dtfrom = DateTime.Parse(DPFrom.Text);
                    string from = dtfrom.ToString("dd-MM-yyyy");
                    condition += "AND DateSearch = '" + from + "' ";
                }
              
                if (TBCName.Text != "")
                {
                    condition += "And ClassName Like '%" + Regex.Replace(TBCName.Text, @"\s+", " ") + "%' ";
                }
                if (TBSearchName.Text != "")
                {
                    condition += "And ( (Name Like '%" + Regex.Replace(TBSearchName.Text, @"\s+", " ") + "%' OR RName Like '%" + Regex.Replace(TBSearchName.Text, @"\s+", " ") + "%') ";
                    condition += " Or (WorkerName Like '%" + Regex.Replace(TBSearchName.Text, @"\s+", " ") + "%' OR WorkerRName Like '%" + Regex.Replace(TBSearchName.Text, @"\s+", " ") + "%')) ";
                }

                if (CBInOut.SelectedIndex == 1)
                {
                    condition += "and InTake is not null ";
                }
                else if (CBInOut.SelectedIndex == 2)
                {
                    condition += "And OutTake is not null ";
                }
               
                dt.DefaultView.RowFilter = condition;
                DataTable filteredRows = dt.DefaultView.ToTable();
                double total = 0;
                for (int ii = 0; ii < filteredRows.Rows.Count; ii++)
                {
                    string I = filteredRows.Rows[ii]["Intake"].ToString();
                    string O = filteredRows.Rows[ii]["OutTake"].ToString();
                    if (filteredRows.Rows[ii]["Intake"].ToString() != "")
                    {
                        total += int.Parse(filteredRows.Rows[ii]["InTake"].ToString());
                    }
                    else
                    {
                        total += double.Parse(O);
                    }
                }
                total += int.Parse(TBInitial.Text);
                TBFinal.Text = total.ToString();
                TBDiff.Text = (total - int.Parse(TBInitial.Text)).ToString();
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
               
                int ID = Connexion.GetInt("Insert into CashRegisterExtra output Inserted.ID values " +
                    "( " + TBPrice.Text + ",N'" +
                     TBName.Text + "'," + IDCR + "," + Connexion.WorkerID + " ,  convert(varchar, getdate(), 8) )");
                Connexion.InsertHistory(0, ID.ToString(), 9);
                Connexion.FillDT(ref dt, query);
                Search();
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Search();
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Report r = new Report();
                string path;
                if (File.Exists(@"C:\ProgramData\EcoleSetting\Mouathfile.txt"))
                {
                    path = @"C:\Users\Home\Desktop\C# Projects\Gestion_De_Cours\FastReport";
                }
                else
                {
                    path = @"C:\ProgramData\EcoleSetting\EcolePrint";
                }

                if (Connexion.Language() == 0)
                {
                    r.Load(path + @"\FastReportCashRegister.frx");
                }
                else if (Connexion.Language() == 1)
                {
                    r.Load(path + @"\FastReportCashRegisterAR.frx");
                }
                DataSet ds = new DataSet();
                DataTable dtEcoleinfo = new DataTable();
                dtEcoleinfo.TableName = "EcoleInfo";
                Connexion.FillDT(ref dtEcoleinfo, "Select * from EcoleSetting");
                dtEcoleinfo.Columns.Add("EcoleLogo");
                dtEcoleinfo.Rows[0]["EcoleLogo"] = Connexion.GetImagesFile() + @"\\EcoleLogo.jpg";
                ds.Tables.Add(dtEcoleinfo);
                DataTable dtCashRegister = new DataTable();
                dtCashRegister.Columns.Add("Initial");
                dtCashRegister.Columns.Add("Difference");
                dtCashRegister.Columns.Add("Final");
                dtCashRegister.Columns.Add("FromDate");
                dtCashRegister.Columns.Add("ToDate");
                dtCashRegister.Rows.Add();
                dtCashRegister.Rows[0][0] = TBInitial.Text;
                dtCashRegister.Rows[0][1] = TBDiff.Text;
                dtCashRegister.Rows[0][2] = TBFinal.Text;
                dtCashRegister.Rows[0][3] = DPFrom.Text;
             
                dtCashRegister.TableName = "CashRegister";
                ds.Tables.Add(dtCashRegister);
                DataTable dtInfo = new DataTable("Info");
                if (LVCash.SelectedItems.Count == 0)
                {
                    dtInfo = dt.Clone();
                    DataRow[] filterd_result = dt.Select(condition);
                    foreach (DataRow row in filterd_result)
                    {
                        dtInfo.ImportRow(row);
                    }
                }
                else
                {
                    dtInfo.Columns.Add("WorkerName");
                    dtInfo.Columns.Add("Name");
                    dtInfo.Columns.Add("ClassName");
                    dtInfo.Columns.Add("InTake");
                    dtInfo.Columns.Add("OutTake");
                    dtInfo.Columns.Add("Date");
                    dtInfo.Columns.Add("Type");

                    for (int i = 0; i < LVCash.SelectedItems.Count; i++)
                    {
                        DataRowView row = (DataRowView)LVCash.SelectedItems[i];
                        dtInfo.Rows.Add();  
                        dtInfo.Rows[i][0] = row["WorkerName"].ToString();
                        dtInfo.Rows[i][1] = row["Name"].ToString();
                        dtInfo.Rows[i][2] = row["ClassName"].ToString();
                        dtInfo.Rows[i][3] = row["InTake"].ToString();
                        dtInfo.Rows[i][4] = row["OutTake"].ToString();
                        dtInfo.Rows[i][5] = row["Date"].ToString();
                        dtInfo.Rows[i][6] = row["Type"].ToString();
                    }
                }
                dtInfo.TableName = "Info";
                ds.Tables.Add(dtInfo);
                r.RegisterData(ds);
                r.GetDataSource("Info").Enabled = true;
                r.GetDataSource("CashRegister").Enabled = true;
                r.GetDataSource("EcoleInfo").Enabled = true;
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

        private void BtnDeleteone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)LVCash.SelectedItem;
                if(row == null)
                {
                    return; 
                }
                if (MessageBox.Show("Are you Sure you want to This Payment?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (row["Typee"].ToString() == "1")
                    {
                        if (Connexion.GetInt(Connexion.WorkerID, "Users", "SPayD") != 1)
                        {
                            MessageBox.Show("Sorry you don't have privilage for this action");
                            return; 
                        }
                        Connexion.Insert("Delete from StudentPayment Where ID = " + row["ID"].ToString()); 
                    }
                    else if (row["Typee"].ToString() == "0")
                    {
                        string MonthID = Connexion.GetString("Select CID  from StudentPayment where ID = " + row["ID"].ToString());
                        int Sum = Connexion.GetInt("Select Case When Sum(Price) is null then 0 else Sum(Price) end as f from StudentPayment Where Type = 4 and CID = " + MonthID);
                        int PriceDeleted = int.Parse(row["InTake"].ToString());
                        if (MonthID != null)
                        {
                           
                            int status;
                            if (Sum == PriceDeleted)
                            {
                                status = 0;
                                Connexion.Insert("Update Monthly_Payment Set status = 0 where ID =" + MonthID);
                            }
                            else
                            {
                                status = 2;
                                Connexion.Insert("Update Monthly_Payment Set status = 2 where ID =" + MonthID);
                            }
                            Connexion.Insert("Delete from StudentPayment Where ID = " +row["ID"].ToString());


                        }
                    }
                    else if (row["Typee"].ToString() == "2")
                    {
                        Connexion.Insert("Delete from CashRegisterExtra Where ID = " + row["ID"].ToString());
                    }
                }
                Connexion.FillDT(ref dt, query);
                LVCash.ItemsSource = dt.DefaultView;
                Search();
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void TBInitial_LostFocus(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Are you Sure you want to Change the starting ammount?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DateTime dtfrominitial = DateTime.Parse(DPFrom.Text);
                string frominitial = dtfrominitial.ToString("dd-MM-yyyy");
                Connexion.Insert("Update CashRegister Set StartAmmount = " + TBInitial.Text + " Where Date = '" + frominitial + "'");
            }
            Search();
        }

        private void TBInitial_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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

        private void TBInitial_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
