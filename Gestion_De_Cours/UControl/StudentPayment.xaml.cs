using System;
using System.Collections.Generic;
using System.Data;
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
using System.Data.SqlClient;
using Gestion_De_Cours.Classes;
using Gestion_De_Cours.Properties;
using FastReport;
using System.IO;
using System.Globalization;

namespace Gestion_De_Cours.UControl
{
    /// <summary>
    /// Interaction logic for StudentPayment.xaml
    /// </summary>
    /// 
    public partial class StudentPayment : UserControl
    {
        string PID;
        string Ty;
        string CLassIDType3;
        int focus = 0 ;
        DataTable dtmonthes = new DataTable();
        //1 Student in Student Info
        public StudentPayment(string Type , string ID , string CID)
        {
            try
            {
                int lang = Connexion.Language();
                InitializeComponent();
                SetLang();
                if (lang == 1)
                {
                    this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
                }
                Ty = Type;
                if (Type == "1")
                {
                    this.Width = 650; 
                    MainGrid.Width = 650;
                    this.Height = 520;
                    MainGrid.Height = 520;
                    PID = ID;
                    StudentLabel.Visibility = Visibility.Collapsed;
                    CBStudent.Visibility = Visibility.Collapsed;
                    DateTime today = DateTime.Now;

                    int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                    if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                    {
                        int YID = Connexion.GetInt("Select Year from Students Where id = " + PID);
                        monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                    }
                    if (monthlypayment == 1)
                    {
                        DGCCName.Visibility = Visibility.Collapsed;
                        DGCMonth.Visibility = Visibility.Visible;
                        DGCYear.Visibility = Visibility.Visible;
                        Row0.Height = new GridLength(40);
                        SPMonthly.Visibility = Visibility.Visible;
                        CNLabel.Visibility = Visibility.Collapsed;
                        CNcombobox.Visibility = Visibility.Collapsed;
                        PricemonthlyTB.Text = Connexion.GetString("Select MonthlyPayment from Students Where ID = " + PID);
                       

                        Connexion.FillDT(ref dtmonthes, "WITH DistinctMonths AS " +
                            "(SELECT " +
                            "DISTINCT DATENAME(MONTH, CONVERT(DATE, a.Date, 103)) AS AttendanceMonth," +
                            " YEAR(CONVERT(DATE, a.Date, 103)) AS AttendanceYear, " +
                            "MONTH(CONVERT(DATE, a.Date, 103)) AS MonthNumber " +
                            "FROM   Attendance a " +
                            "INNER JOIN attendance_Student ast ON a.ID = ast.ID  " +
                            "WHERE ast.StudentID ='"+ PID+"' ) " +
                            "SELECT   " +
                            "AttendanceMonth + ' ' + CAST(AttendanceYear AS VARCHAR(4)) AS AttendanceMonthYear," +
                            "AttendanceMonth,  " +
                            "AttendanceYear " +
                            "FROM DistinctMonths " +
                            "ORDER BY  AttendanceYear ASC,  MonthNumber ASC;");
                        DateTime nextMonth;
                        string dateTimeFormat = "dd-MM-yyyy";

                        // Create a custom CultureInfo for consistent date format
                        CultureInfo customCulture = new CultureInfo(CultureInfo.InvariantCulture.Name);
                        customCulture.DateTimeFormat.ShortDatePattern = dateTimeFormat;
                        customCulture.DateTimeFormat.LongDatePattern = dateTimeFormat;

                        if (dtmonthes.Rows.Count != 0)
                        {
                            DataRow lastRow = dtmonthes.Rows[dtmonthes.Rows.Count - 1];
                            string monthName = lastRow["AttendanceMonth"].ToString();
                            int yearValue = int.Parse(lastRow["AttendanceYear"].ToString());

                            int monthNumber;

                            try
                            {
                                // Try parsing with InvariantCulture (assuming monthName is in English)
                                monthNumber = DateTime.ParseExact(monthName, "MMMM", CultureInfo.InvariantCulture).Month;
                            }
                            catch (FormatException)
                            {
                                try
                                {
                                    // If parsing with InvariantCulture fails, try with client's local culture (French, etc.)
                                    monthNumber = DateTime.ParseExact(monthName, "MMMM", CultureInfo.CurrentCulture).Month;
                                }
                                catch (FormatException)
                                {
                                    Console.WriteLine("Unable to parse month name. Ensure the month name is valid.");
                                    throw;  // Re-throw or handle the error
                                }
                            }

                            DateTime lastAttendanceMonth = new DateTime(yearValue, monthNumber, 1);
                            nextMonth = lastAttendanceMonth;
                        }
                        else
                        {
                            nextMonth = DateTime.Today;
                        }
                        if (Connexion.IFNULLVar("Select MonthlyPayment From Students Where ID =" + ID))
                        {
                            int price = Connexion.GetInt("Select Case when Sum(CPrice) is null then 0 else Sum(CPrice) end  from Class_Student Join Class on Class_Student.ClassID = Class.ID where StudentID = " + ID);
                            Connexion.Insert("update Students Set monthlyPayment = " + price + " Where ID = " + ID);
                        }
                        
                        // Add the next months to your DataGrid
                        for (int i = 1 ; i < 4; i++) // Assuming you want to add the next 3 months
                        {
                            DataRow newRow = dtmonthes.NewRow();

                            // Set the value of the "AttendanceMonth" column for the new row
                            newRow["AttendanceMonth"] = nextMonth.AddMonths(i).ToString("MMMM");
                            newRow["AttendanceYear"] = nextMonth.AddMonths(i).ToString("yyyy");
                            newRow["AttendanceMonthYear"] = newRow["AttendanceMonth"].ToString() + ' ' + newRow["AttendanceYear"].ToString();

                            // Add the new row to the DataTable
                            dtmonthes.Rows.Add(newRow);
                        }

                        dtmonthes.Columns.Add("MonthNumber", typeof(int));
                        dtmonthes.Columns.Add("ID", typeof(int));
                        dtmonthes.Columns.Add("Status", typeof(int));
                        // Now iterate through all rows and set the value of the "MonthNumber" column
                        foreach (DataRow row in dtmonthes.Rows)
                        {
                            string attendanceMonth = row["AttendanceMonth"].ToString();
                            string attendanceYear = row["AttendanceYear"].ToString();
                            try
                            {
                                // Attempt to parse assuming `attendanceMonth` is in English
                                row["MonthNumber"] = DateTime.ParseExact(attendanceMonth, "MMMM", CultureInfo.InvariantCulture).Month;
                            }
                            catch (FormatException)
                            {
                                try
                                {
                                    // Fallback: Attempt parsing using the current system culture (e.g., French)
                                    row["MonthNumber"] = DateTime.ParseExact(attendanceMonth, "MMMM", CultureInfo.CurrentCulture).Month;
                                }
                                catch (FormatException)
                                {
                                    Console.WriteLine($"Unable to parse the month name: {attendanceMonth}. Please verify its format and language.");
                                    throw;  // Handle or re-throw the error as needed
                                }
                            }
                            int PaymentID;
                            int status;
                            if (Connexion.IFNULL("Select ID from Monthly_Payment WHere SID = " + ID + " and  Month = " + row["MonthNumber"].ToString() + " and Year = " + attendanceYear) )
                            {
                                 PaymentID =  Connexion.GetInt("Insert into Monthly_Payment " +
                                     "OUTPUT INSERTED.ID " +
                                     "Values (" +ID + ","+ row["MonthNumber"].ToString() + ",0,"+ Connexion.GetInt("Select MonthlyPayment From Students Where ID ="+ ID) + "," + attendanceYear +")");
                                status = 0;
                            }
                            else
                            {
                                PaymentID = Connexion.GetInt("Select ID " +
                                    "from Monthly_Payment WHere SID = " + ID + " " +
                                    "and  Month = " + row["MonthNumber"].ToString() + " and Year = " + attendanceYear);
                                status = Connexion.GetInt("Select Status" +
                                    " from Monthly_Payment WHere SID = " + ID + " " +
                                    "and  ID = " + PaymentID);
                            }
                            row["ID"] = PaymentID;
                            row["Status"] = status;
                        }
                        CBMonths.ItemsSource = dtmonthes.DefaultView;
                        Connexion.FillDG(ref DGName, "Select " +
                           "StudentPayment.ID as PayID,StudentPayment.Date as Date ,  Students.ID as SID  , StudentPayment.Note as Note , " +
                           "StudentPayment.Price as Price , Students.FirstName + ' ' + Students.LastName as Name , " +
                           "Monthly_Payment.Month as Month ,Monthly_Payment.ID as MonthID ," +
                           "Case When Month = 12 then N'" + this.Resources["Dec"].ToString() + "' " +
                           "When Month = 1 Then N'" + this.Resources["Jan"].ToString()+"' " +
                           "When Month = 2 then N'" + this.Resources["Feb"].ToString() + "' "+
                           "When Month = 3 then N'" + this.Resources["Mar"].ToString() + "' " +
                           "When Month = 4 then N'" + this.Resources["Apr"].ToString() + "' " +
                           "When Month = 5 then N'" + this.Resources["May"].ToString() + "' " +
                           "When Month = 6 then N'" + this.Resources["Jun"].ToString() + "' " +
                           "When Month = 7 then N'" + this.Resources["Jul"].ToString() + "' " +
                           "When Month = 8 then N'" + this.Resources["Aug"].ToString() + "' " +
                           "When Month = 9 then N'" + this.Resources["Sep"].ToString() + "' " +
                           "When Month = 10 then N'" + this.Resources["Oct"].ToString() + "' " +
                           "When Month = 11 then N'" + this.Resources["Nov"].ToString() + "' " +
                           " End as MonthName, Monthly_Payment.Year as Year  " +
                           "from StudentPayment Join Students On Students.ID = StudentPayment.SID " +
                           "Join Monthly_Payment ON Monthly_Payment.ID = StudentPayment.CID Where StudentPayment.Type = 4 and StudentPayment.Deleted = 0 and Students.ID =" + PID);
                        Keyboard.ClearFocus();
                    }
                    else
                    {
                        PricemonthlyTB.Visibility = Visibility.Collapsed;
                        Connexion.FillDG(ref DGName, "Select " +
                            "StudentPayment.ID as PayID,StudentPayment.Date as Date ,  Students.ID as SID  , StudentPayment.Note as Note , " +
                            "StudentPayment.Price as Price , Students.FirstName + ' ' + Students.LastName as Name , " +
                            "Class.CName as CName  , class.ID as CID  " +
                            "from StudentPayment Join Students On Students.ID = StudentPayment.SID " +
                            "Join Class ON Class.ID = StudentPayment.CID Where StudentPayment.Type = 1 and StudentPayment.Deleted = 0 and Students.ID =" + PID);
                        Connexion.FillCB(ref CNcombobox, "Select  Distinct(Class.ID) as  ClassID,Class_Student.StudentID as SID,CName as Name From Class   left outer Join Class_Student On Class_Student.ClassID = Class.ID Where Class_Student.StudentID = " + PID);
                    }
                }
                else if(Type == "0")
                {
                   // Connexion.FillCB(ref CNcombobox, "Select  Distinct(Class.ID) as  ClassID,Class_Student.StudentID as SID,CName as Name From Class   left outer Join Class_Student On Class_Student.ClassID = Class.ID" );

                   string query = "SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students where Status = 1  order by name asc ";
                    Connexion.FillCB(ref CBStudent, query );
                    Connexion.FillDG(ref DGName,
                  "Select StudentPayment.ID as PayID,StudentPayment.Date as Date , " +
                  "StudentPayment.Note as Note , Students.ID as SID  ,  " +
                  "StudentPayment.Price as Price , " +
                  "Students.FirstName + ' ' + Students.LastName as Name , " +
                  "Class.CName as CName  , class.ID as CID " +
                  "from StudentPayment " +
                  "Join Students On Students.ID = StudentPayment.SID " +
                  "Join Class ON Class.ID = StudentPayment.CID" +
                  " Where StudentPayment.Type = 1 and StudentPayment.Deleted = 0  ");
                }
                else if (Type == "2")
                {
                    Connexion.FillCB(ref CNcombobox, "Select  Distinct(Class.ID) as  ClassID,Class_Student.StudentID as SID,CName as Name From Class   left outer Join Class_Student On Class_Student.ClassID = Class.ID");

                    string query = "SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students where Status = 1 ";
                    Connexion.FillCB(ref CBStudent, query);
                    CBStudent.SelectedValue = ID;
                    Connexion.FillCB(ref CNcombobox, "Select  Distinct(Class.ID) as  ClassID,Class_Student.StudentID as SID,CName as Name From Class   left outer Join Class_Student On Class_Student.ClassID = Class.ID Where Class_Student.StudentID = " + PID);
                    CNcombobox.EditValue = CID; 
                }
                else if (Type == "3")
                {
                    CNcombobox.Visibility = Visibility.Collapsed;
                    CNLabel.Visibility = Visibility.Collapsed;
                    CLassIDType3 = CID;
                    Connexion.FillDG(ref DGName, "Select StudentPayment.ID as PayID,StudentPayment.Date as Date , StudentPayment.Note as Note , StudentPayment.Price as Price , Students.FirstName + ' ' + Students.LastName as Name , Class.CName as CName ,  Students.ID as SID  , class.ID as CID  from StudentPayment Join Students On Students.ID = StudentPayment.SID Join Class ON Class.ID = StudentPayment.CID Where StudentPayment.Deleted = 0  and Type = 1 And Class.ID = " + CID);
                    string query = "SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students join Class_Student on Class_Student.StudentID= Students.ID  where Status = 1  and Class_student.ClassID = " + CID + "  order by name asc ";
                    Connexion.FillCB(ref CBStudent, query);


                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            focus = 0;
            // Set focus to the window itself to prevent the first element from receiving focus
            FocusManager.SetFocusedElement(this, this);
            focus = 1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (Connexion.GetInt(Connexion.WorkerID, "Users", "SPayA") != 1)
                {
                    MessageBox.Show("Sorry,you dont have the privalage to do this action");
                    return;
                }
                string dt = "";
                if(PriceTB.Text == "")
                {
                    return;
                }
                SqlConnection con = Connexion.Connect();
                /*  if(DatePayment.Text == "")
                  {
                      dt = DateTime.Now.ToString("dd-MM-yyyy HH:mm");
                  }
                  else
                  {
                 dt = DatePayment.Text;
                  }*/
                //  dt = DateTime.Now.ToString("dd-MM-yyyy HH:mm");
                dt = Date.Text.Replace("/","-");
                if (dt == "")
                {
                    return;
                }
                    int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                {
                    int YID = Connexion.GetInt("Select Year from Students Where id = " + PID);
                    monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                }
                DataRowView row = (DataRowView)CNcombobox.SelectedItem;
                if ( row != null || Ty == "3")
                {
                    string cid;
                    if (Ty == "3")
                    {
                        cid = CLassIDType3;
                    }
                    else
                    {
                        
                        cid = row["ClassID"].ToString();
                    }
                    string SID = PID;
                    //get groupid 
                    string query = "select Groups.GroupID from Groups join Class_Student on Groups.GroupID = Class_Student.GroupID Where Class_Student.StudentID = " + PID + " And Class_Student.ClassID = " + cid;
                    SqlCommand com = new SqlCommand(query, con);
                    com.ExecuteNonQuery();
                    string GID = com.ExecuteScalar().ToString();
                    string ID = Connexion.GetInt("Insert into " +
                        "StudentPayment output inserted.ID Values " +
                        "(" + PID + "," +
                        "1," +
                        "'" + cid + "'," +
                        "'" + PriceTB.Text + "'," +
                        "''," +
                        "'" + dt + "',0)").ToString();
                    Connexion.InsertHistory(0, ID, 8);
                    Connexion.FillDG(ref DGName, "Select StudentPayment.ID as PayID,StudentPayment.Date as Date , StudentPayment.Note as Note , StudentPayment.Price as Price , Students.FirstName + ' ' + Students.LastName as Name , Class.CName as CName ,  Students.ID as SID  , class.ID as CID  from StudentPayment Join Students On Students.ID = StudentPayment.SID Join Class ON Class.ID = StudentPayment.CID Where Students.ID =" + PID + " and StudentPayment.Deleted = 0  and Type = 1 And Class.ID = " + cid);
                }
                else if (monthlypayment == 1)
                {
                    DataRowView rowmonth = (DataRowView)CBMonths.SelectedItem;
                    if (rowmonth != null)
                    {
                        int SuTotal = Connexion.GetInt("Select SuTotal from Monthly_Payment Where ID =" + rowmonth["ID"].ToString());
                        int Sum = Connexion.GetInt("Select Case When Sum(Price) is null then 0 else Sum(Price) end as f from StudentPayment Where Type = 4 and CID = " + rowmonth["ID"].ToString());
                        Sum += int.Parse(PriceTB.Text);
                        int status  ;
                        MessageBoxResult message;
                        if ( SuTotal > Sum)
                        {
                            message = MessageBox.Show("الدفع المضاف أقل  من السعر المفترض. هل تريد تجاوز السعر المفترض ليطابق؟", "Confirmation", MessageBoxButton.YesNo);
                            if (message == MessageBoxResult.Yes)
                            {
                                Connexion.Insert("Update Monthly_Payment Set SuTotal =" + Sum + " where ID =" + rowmonth["ID"].ToString());
                                Connexion.Insert("Update Monthly_Payment Set status = 1 where ID =" + rowmonth["ID"].ToString());
                                status = 1;
                            }
                            else if (message == MessageBoxResult.No)
                            {
                                Connexion.Insert("Update Monthly_Payment Set status = 2 where ID =" + rowmonth["ID"].ToString());
                                status = 2;
                            }
                            else
                            {
                                return;
                            }
                        }
                        else if (SuTotal < Sum)
                        {
                             message = MessageBox.Show("الدفع المضاف أكبر من السعر المفترض. هل تريد تجاوز السعر المفترض ليطابق؟", "Confirmation", MessageBoxButton.YesNo);
                            if (message == MessageBoxResult.Yes)
                            {
                                Connexion.Insert("Update Monthly_Payment Set SuTotal =" + Sum + " where ID =" +      rowmonth["ID"].ToString());
                                Connexion.Insert("Update Monthly_Payment Set status = 1 where ID =" + rowmonth["ID"].ToString());
                                status = 1;


                            }
                            else if (message == MessageBoxResult.No)
                            {
                                Connexion.Insert("Update Monthly_Payment Set status = 2 where ID =" + rowmonth["ID"].ToString());
                                status = 2;
                            }
                            else
                            {
                                return;
                            }

                        }
                        else
                        {
                            Connexion.Insert("Update Monthly_Payment Set status = 1 where ID =" + rowmonth["ID"].ToString());
                            status = 1;
                        }
                        string paymentID = Connexion.GetString("Insert into " +
                                "StudentPayment output inserted.ID Values " +
                                "(" + PID + "," +
                                 "4," +
                                 "'" + rowmonth["ID"].ToString() + "'," +
                                  "'" + PriceTB.Text + "'," +
                                 "''," +
                                 "'" + dt + "',0)").ToString();
                        Connexion.InsertHistory(0, paymentID, 8);
                        DataRowView selectedRow = CBMonths.SelectedItem as DataRowView;

                        // Modify the value status of the selected item
                        selectedRow["Status"] = status;

                        // Refresh the ComboBox to reflect the changes
                        CBMonths.Items.Refresh();
                        Connexion.FillDG(ref DGName, "Select " +
                           "StudentPayment.ID as PayID,StudentPayment.Date as Date ,  Students.ID as SID  , StudentPayment.Note as Note , " +
                           "StudentPayment.Price as Price , Students.FirstName + ' ' + Students.LastName as Name , " +
                           "Monthly_Payment.Month as Month ,Monthly_Payment.ID as MonthID," +
                           "Case When Month = 12 then N'" + this.Resources["Dec"].ToString() + "' " +
                           "When Month = 1 Then N'" + this.Resources["Jan"].ToString() + "' " +
                           "When Month = 2 then N'" + this.Resources["Feb"].ToString() + "' " +
                           "When Month = 3 then N'" + this.Resources["Mar"].ToString() + "' " +
                           "When Month = 4 then N'" + this.Resources["Apr"].ToString() + "' " +
                           "When Month = 5 then N'" + this.Resources["May"].ToString() + "' " +
                           "When Month = 6 then N'" + this.Resources["Jun"].ToString() + "' " +
                           "When Month = 7 then N'" + this.Resources["Jul"].ToString() + "' " +
                           "When Month = 8 then N'" + this.Resources["Aug"].ToString() + "' " +
                           "When Month = 9 then N'" + this.Resources["Sep"].ToString() + "' " +
                           "When Month = 10 then N'" + this.Resources["Oct"].ToString() + "' " +
                           "When Month = 11 then N'" + this.Resources["Nov"].ToString() + "' " +
                           " End as MonthName, Monthly_Payment.Year as Year  " +
                           "from StudentPayment Join Students On Students.ID = StudentPayment.SID " +
                           "Join Monthly_Payment ON Monthly_Payment.ID = StudentPayment.CID Where StudentPayment.Type = 4 and StudentPayment.Deleted = 0 and Students.ID =" + PID+ " and Monthly_Payment.ID = " + rowmonth["ID"].ToString());
                    }
                    else
                    {
                        return;
                    }
                }
                MessageBox.Show("Inserted Successfully");
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (System.Windows.MessageBox.Show(this.Resources["ConfirmationDeletePayment"].ToString(), "Delete Confirmation", System.Windows.MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                    if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                    {
                        int YID = Connexion.GetInt("Select Year from Students Where id = " + PID);
                        monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                    }
                    if (monthlypayment == 1)
                    {
                        string MonthID = ((DataRowView)DGName.SelectedItem).Row["MonthID"].ToString();
                        int Sum = Connexion.GetInt("Select Case When Sum(Price) is null then 0 else Sum(Price) end as f from StudentPayment Where Type = 4 and CID = " + MonthID);
                        int PriceDeleted = int.Parse(((DataRowView)DGName.SelectedItem).Row["Price"].ToString());
                        if (MonthID != null)
                        {
                            DataRowView row = (DataRowView)CBMonths.SelectedItem;
                            int status ;
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
                            Connexion.Insert("Delete from StudentPayment Where ID = " + ((DataRowView)DGName.SelectedItem).Row["PayID"].ToString());
                           
                            if (row != null)
                            {
                                row["Status"] = status;
                                Connexion.FillDG(ref DGName, "Select " +
                                "StudentPayment.ID as PayID,StudentPayment.Date as Date ,  Students.ID as SID  , StudentPayment.Note as Note , " +
                                "StudentPayment.Price as Price , Students.FirstName + ' ' + Students.LastName as Name , " +
                                "Monthly_Payment.Month as Month , Monthly_Payment.ID as MonthID ," +
                                "Case When Month = 12 then N'" + this.Resources["Dec"].ToString() + "' " +
                                "When Month = 1 Then N'" + this.Resources["Jan"].ToString() + "' " +
                                "When Month = 2 then N'" + this.Resources["Feb"].ToString() + "' " +
                                "When Month = 3 then N'" + this.Resources["Mar"].ToString() + "' " +
                                "When Month = 4 then N'" + this.Resources["Apr"].ToString() + "' " +
                                "When Month = 5 then N'" + this.Resources["May"].ToString() + "' " +
                                "When Month = 6 then N'" + this.Resources["Jun"].ToString() + "' " +
                                "When Month = 7 then N'" + this.Resources["Jul"].ToString() + "' " +
                                "When Month = 8 then N'" + this.Resources["Aug"].ToString() + "' " +
                                "When Month = 9 then N'" + this.Resources["Sep"].ToString() + "' " +
                                "When Month = 10 then N'" + this.Resources["Oct"].ToString() + "' " +
                                "When Month = 11 then N'" + this.Resources["Nov"].ToString() + "' " +
                                " End as MonthName, Monthly_Payment.Year as Year  " +
                                "from StudentPayment Join Students On Students.ID = StudentPayment.SID " +
                                "Join Monthly_Payment ON Monthly_Payment.ID = StudentPayment.CID Where StudentPayment.Type = 4 and StudentPayment.Deleted = 0 and Students.ID =" + PID + " and Monthly_Payment.ID = " + row["ID"].ToString());
                            }
                            else
                            {
                                DataRow[] rowsToUpdate = dtmonthes.Select("ID = " + MonthID);

                                if (rowsToUpdate.Length > 0)
                                {
                                    // Update the specific column in the row
                                    rowsToUpdate[0]["Status"] = status;

                                    // Optionally, if you are using data binding, you may need to accept changes or refresh the view
                                    dtmonthes.AcceptChanges();
                                }
                                Connexion.FillDG(ref DGName, "Select " +
                                "StudentPayment.ID as PayID,StudentPayment.Date as Date ,  Students.ID as SID  , StudentPayment.Note as Note , " +
                                "StudentPayment.Price as Price , Students.FirstName + ' ' + Students.LastName as Name , " +
                                "Monthly_Payment.Month as Month , Monthly_Payment.ID as MonthID ," +
                                "Case When Month = 12 then N'" + this.Resources["Dec"].ToString() + "' " +
                                "When Month = 1 Then N'" + this.Resources["Jan"].ToString() + "' " +
                                "When Month = 2 then N'" + this.Resources["Feb"].ToString() + "' " +
                                "When Month = 3 then N'" + this.Resources["Mar"].ToString() + "' " +
                                "When Month = 4 then N'" + this.Resources["Apr"].ToString() + "' " +
                                "When Month = 5 then N'" + this.Resources["May"].ToString() + "' " +
                                "When Month = 6 then N'" + this.Resources["Jun"].ToString() + "' " +
                                "When Month = 7 then N'" + this.Resources["Jul"].ToString() + "' " +
                                "When Month = 8 then N'" + this.Resources["Aug"].ToString() + "' " +
                                "When Month = 9 then N'" + this.Resources["Sep"].ToString() + "' " +
                                "When Month = 10 then N'" + this.Resources["Oct"].ToString() + "' " +
                                "When Month = 11 then N'" + this.Resources["Nov"].ToString() + "' " +
                                " End as MonthName, Monthly_Payment.Year as Year  " +
                                "from StudentPayment Join Students On Students.ID = StudentPayment.SID " +
                                "Join Monthly_Payment ON Monthly_Payment.ID = StudentPayment.CID Where StudentPayment.Type = 4 and StudentPayment.Deleted = 0 and Students.ID =" + PID);
                            }

                            MessageBox.Show("Deleted Succesfully");


                        }
                       
                    }
                    else
                    {
                        string PayID = ((DataRowView)DGName.SelectedItem).Row["PayID"].ToString();
                        int Price = int.Parse(((DataRowView)DGName.SelectedItem).Row["Price"].ToString());
                        Price = -Price;
                        string CName = ((DataRowView)DGName.SelectedItem).Row["CName"].ToString();
                        Connexion.Insert("Delete from  StudentPayment where ID = " + PayID);
                        string studentname = Connexion.GetString("Select Students.FirstName + ' ' + Students.LastName from Students Where ID = " + PID);
                     
                        Connexion.InsertHistory(1, PayID, 8);
                        MessageBox.Show("Deleted Succesfully");
                        string query = "Select StudentPayment.ID as PayID,StudentPayment.Date as Date , StudentPayment.Note as Note , StudentPayment.Price as Price , Students.FirstName + ' ' + Students.LastName as Name , Class.CName as CName  ,Students.ID as SID , class.ID as CID  from StudentPayment Join Students On Students.ID = StudentPayment.SID Join Class ON Class.ID = StudentPayment.CID Where type = 1 and StudentPayment.Deleted = 0   ";
                        if (PID != null)
                        {
                            query += "and Students.ID = " + PID;
                        }
                        Connexion.FillDG(ref DGName, query);
                    }
                }
            }
            catch(Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click_DeleteMultiple(object sender, RoutedEventArgs e)
        {

            if (System.Windows.MessageBox.Show(this.Resources["ConfirmationDeletePayment"].ToString(), "Delete Confirmation", System.Windows.MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (DGName.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please Select a row(s) to be deleted ");
                    return;
                }
                int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                {
                    int YID = Connexion.GetInt("Select Year from Students Where id = " + PID);
                    monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                }
                string query = ""; 
                for (int i = 0; i < DGName.SelectedItems.Count; i++)
                {
                    var selectedRow = DGName.SelectedItems[i] as DataRowView;

                    if (selectedRow != null)
                    {

                        if (monthlypayment == 1)
                        {
                            string MonthID = selectedRow["MonthID"].ToString();
                            int Sum = Connexion.GetInt("Select Case When Sum(Price) is null then 0 else Sum(Price) end as f from StudentPayment Where Type = 4 and CID = " + MonthID);
                            int PriceDeleted = int.Parse(selectedRow["Price"].ToString());
                            if (MonthID != null)
                            {
                                DataRowView row = (DataRowView)CBMonths.SelectedItem;
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
                                Connexion.Insert("Delete from StudentPayment Where ID = " + (selectedRow["PayID"].ToString()));

                                if (row != null)
                                {
                                    row["Status"] = status;
                                    query = "Select " +
                                      "StudentPayment.ID as PayID,StudentPayment.Date as Date ,  Students.ID as SID  , StudentPayment.Note as Note , " +
                                      "StudentPayment.Price as Price , Students.FirstName + ' ' + Students.LastName as Name , " +
                                      "Monthly_Payment.Month as Month , Monthly_Payment.ID as MonthID ," +
                                      "Case When Month = 12 then N'" + this.Resources["Dec"].ToString() + "' " +
                                      "When Month = 1 Then N'" + this.Resources["Jan"].ToString() + "' " +
                                      "When Month = 2 then N'" + this.Resources["Feb"].ToString() + "' " +
                                      "When Month = 3 then N'" + this.Resources["Mar"].ToString() + "' " +
                                      "When Month = 4 then N'" + this.Resources["Apr"].ToString() + "' " +
                                      "When Month = 5 then N'" + this.Resources["May"].ToString() + "' " +
                                      "When Month = 6 then N'" + this.Resources["Jun"].ToString() + "' " +
                                      "When Month = 7 then N'" + this.Resources["Jul"].ToString() + "' " +
                                      "When Month = 8 then N'" + this.Resources["Aug"].ToString() + "' " +
                                      "When Month = 9 then N'" + this.Resources["Sep"].ToString() + "' " +
                                      "When Month = 10 then N'" + this.Resources["Oct"].ToString() + "' " +
                                      "When Month = 11 then N'" + this.Resources["Nov"].ToString() + "' " +
                                      " End as MonthName, Monthly_Payment.Year as Year  " +
                                      "from StudentPayment Join Students On Students.ID = StudentPayment.SID " +
                                      "Join Monthly_Payment ON Monthly_Payment.ID = StudentPayment.CID Where StudentPayment.Type = 4 and StudentPayment.Deleted = 0 and Students.ID =" + PID + " and Monthly_Payment.ID = " + row["ID"].ToString();
                                }
                                else
                                {
                                    DataRow[] rowsToUpdate = dtmonthes.Select("ID = " + MonthID);

                                    if (rowsToUpdate.Length > 0)
                                    {
                                        // Update the specific column in the row
                                        rowsToUpdate[0]["Status"] = status;

                                        // Optionally, if you are using data binding, you may need to accept changes or refresh the view
                                        dtmonthes.AcceptChanges();
                                    }
                                    query = "Select " +
                                    "StudentPayment.ID as PayID,StudentPayment.Date as Date ,  Students.ID as SID  , StudentPayment.Note as Note , " +
                                    "StudentPayment.Price as Price , Students.FirstName + ' ' + Students.LastName as Name , " +
                                    "Monthly_Payment.Month as Month , Monthly_Payment.ID as MonthID ," +
                                    "Case When Month = 12 then N'" + this.Resources["Dec"].ToString() + "' " +
                                    "When Month = 1 Then N'" + this.Resources["Jan"].ToString() + "' " +
                                    "When Month = 2 then N'" + this.Resources["Feb"].ToString() + "' " +
                                    "When Month = 3 then N'" + this.Resources["Mar"].ToString() + "' " +
                                    "When Month = 4 then N'" + this.Resources["Apr"].ToString() + "' " +
                                    "When Month = 5 then N'" + this.Resources["May"].ToString() + "' " +
                                    "When Month = 6 then N'" + this.Resources["Jun"].ToString() + "' " +
                                    "When Month = 7 then N'" + this.Resources["Jul"].ToString() + "' " +
                                    "When Month = 8 then N'" + this.Resources["Aug"].ToString() + "' " +
                                    "When Month = 9 then N'" + this.Resources["Sep"].ToString() + "' " +
                                    "When Month = 10 then N'" + this.Resources["Oct"].ToString() + "' " +
                                    "When Month = 11 then N'" + this.Resources["Nov"].ToString() + "' " +
                                    " End as MonthName, Monthly_Payment.Year as Year  " +
                                    "from StudentPayment Join Students On Students.ID = StudentPayment.SID " +
                                    "Join Monthly_Payment ON Monthly_Payment.ID = StudentPayment.CID Where StudentPayment.Type = 4 and StudentPayment.Deleted = 0 and Students.ID =" + PID;
                                }




                            }

                        }
                        else
                        {
                            string PayID = (selectedRow["PayID"].ToString());
                            int Price = int.Parse((selectedRow["Price"].ToString()));
                            Price = -Price;
                            string CName = (selectedRow["CName"].ToString());
                            Connexion.Insert("Delete from  StudentPayment where ID = " + PayID);
                            string studentname = Connexion.GetString("Select Students.FirstName + ' ' + Students.LastName from Students Where ID = " + PID);
                            Connexion.InsertHistory(1, PayID, 8);
                            query = "Select StudentPayment.ID as PayID,StudentPayment.Date as Date , StudentPayment.Note as Note , StudentPayment.Price as Price , Students.FirstName + ' ' + Students.LastName as Name , Class.CName as CName  ,Students.ID as SID , class.ID as CID  from StudentPayment Join Students On Students.ID = StudentPayment.SID Join Class ON Class.ID = StudentPayment.CID Where type = 1 and StudentPayment.Deleted = 0   ";
                            if (PID != null)
                            {
                                query += "and Students.ID = " + PID;
                            }
                        }
                    }
                }
                Connexion.FillDG(ref DGName, query);
                MessageBox.Show("Deleted Succesfully");
                
            }
        }

        private void CNCombobox_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {

            try
            {

                DataRowView row = (DataRowView)CNcombobox.SelectedItem;
                if (row == null)
                {
                    return;
                }
                string ClassID = row["ClassID"].ToString();
                int GID = Connexion.GetInt("Select GroupID from Class_Student where StudentID = " + PID + " and ClassID = " + ClassID);
                int TSessions = Connexion.GetInt("Select TSessions from groups where groupid = " + GID);
                int price ;
                if (Connexion.GetInt("Select CalcPrice from EcoleSetting") == 1)
                {
                    price = Connexion.GetInt("Select dbo.CalcPriceSum("+PID+","+ClassID+")");
                }
                else
                {
                    price = Connexion.GetInt("Select dbo.GettotalPayStudent(Students.ID , Groups.ClassID) - dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'Su')");
                }
                if (price > 0)
                {
                    PriceTB.Text = price.ToString();
                }
                else
                {
                    PriceTB.Text = Connexion.GetPrice(ClassID, PID).ToString();
                }
                Connexion.FillDG(ref DGName, "Select StudentPayment.ID as PayID,StudentPayment.Date as Date , StudentPayment.Note as Note , StudentPayment.Price as Price , Students.FirstName + ' ' + Students.LastName as Name , Class.CName as CName ,  Students.ID as SID  , class.ID as CID  from StudentPayment Join Students On Students.ID = StudentPayment.SID Join Class ON Class.ID = StudentPayment.CID Where Students.ID =" + PID + " and StudentPayment.Deleted = 0  and Type = 1 And Class.ID = " + row["ClassID"].ToString());


            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void StudentName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {


                DataRowView row = (DataRowView)CBStudent.SelectedItem;
                if (row != null)
                {
                    PID = row["ID"].ToString();

                    if (Ty != "3")
                    {
                        Connexion.FillCB(ref CNcombobox, "Select  Distinct(Class.ID) as  ClassID,Class_Student.StudentID as SID,CName as Name From Class   left outer Join Class_Student On Class_Student.ClassID = Class.ID Where Class_Student.StudentID = " + PID);
                        Connexion.FillDG(ref DGName, "Select " +
                           "StudentPayment.ID as PayID,StudentPayment.Date as Date , StudentPayment.Note as Note , " +
                           "StudentPayment.Price as Price ,Students.ID as SID  , Students.FirstName + ' ' + Students.LastName as Name , " +
                           "Class.CName as CName , class.ID as CID " +
                           "from StudentPayment Join Students On Students.ID = StudentPayment.SID " +
                           "Join Class ON Class.ID = StudentPayment.CID Where StudentPayment.Deleted = 0 and  Students.ID =" + PID);
                    }
                    else
                    {
                        Connexion.FillDG(ref DGName, "Select " +
                           "StudentPayment.ID as PayID,StudentPayment.Date as Date , StudentPayment.Note as Note , " +
                           "StudentPayment.Price as Price ,Students.ID as SID  , Students.FirstName + ' ' + Students.LastName as Name , " +
                           "Class.CName as CName , class.ID as CID " +
                           "from StudentPayment Join Students On Students.ID = StudentPayment.SID " +
                           "Join Class ON Class.ID = StudentPayment.CID Where StudentPayment.Deleted = 0 and StudentPayment.CID = " + CLassIDType3 + " and Students.ID =" + PID);
                    }
                }
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
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

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                FastReports.PrintPaymentStudent(ref DGName, PID, "Student");

            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void Button_Click_PrintMultiple(object sender, RoutedEventArgs e)
        {
            try
            {
                FastReports.PrintPaymentStudent(ref DGName, PID, "Student");
            }
            catch (Exception r)
            {
                Methods.ExceptionHandle(r);
            }
        }

  

        private void CBMonths_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)CBMonths.SelectedItem;
                if (row != null)
                {
                    int SuTotal = Connexion.GetInt("Select SuTotal from Monthly_Payment Where ID = " + row["ID"].ToString());
                    PricemonthlyTB.Text = SuTotal.ToString();
                    if(row["Status"].ToString() == "0")
                    {
                        PriceTB.Text = SuTotal.ToString(); 

                    }
                    else if (row["Status"].ToString() == "1")
                    {
                        PriceTB.Text = "0";
                    }
                    else if ( row["Status"].ToString() == "2")
                    {
                        PriceTB.Text = (SuTotal - Connexion.GetInt("Select Sum(Price) from StudentPayment Where Type = 4 and CID = " + row["ID"].ToString() + " and Deleted = 0")).ToString();
                    }
                    Connexion.FillDG(ref DGName, "Select " +
                           "StudentPayment.ID as PayID,StudentPayment.Date as Date ,  Students.ID as SID  , StudentPayment.Note as Note , " +
                           "StudentPayment.Price as Price , Students.FirstName + ' ' + Students.LastName as Name , " +
                           "Monthly_Payment.Month as Month , Monthly_Payment.ID as MonthID ," +
                           "Case When Month = 12 then N'" + this.Resources["Dec"].ToString() + "' " +
                           "When Month = 1 Then N'" + this.Resources["Jan"].ToString() + "' " +
                           "When Month = 2 then N'" + this.Resources["Feb"].ToString() + "' " +
                           "When Month = 3 then N'" + this.Resources["Mar"].ToString() + "' " +
                           "When Month = 4 then N'" + this.Resources["Apr"].ToString() + "' " +
                           "When Month = 5 then N'" + this.Resources["May"].ToString() + "' " +
                           "When Month = 6 then N'" + this.Resources["Jun"].ToString() + "' " +
                           "When Month = 7 then N'" + this.Resources["Jul"].ToString() + "' " +
                           "When Month = 8 then N'" + this.Resources["Aug"].ToString() + "' " +
                           "When Month = 9 then N'" + this.Resources["Sep"].ToString() + "' " +
                           "When Month = 10 then N'" + this.Resources["Oct"].ToString() + "' " +
                           "When Month = 11 then N'" + this.Resources["Nov"].ToString() + "' " +
                           " End as MonthName, Monthly_Payment.Year as Year  " +
                           "from StudentPayment Join Students On Students.ID = StudentPayment.SID " +
                           "Join Monthly_Payment ON Monthly_Payment.ID = StudentPayment.CID Where StudentPayment.Type = 4 and StudentPayment.Deleted = 0 and Students.ID =" + PID + " and Monthly_Payment.ID = " + row["ID"].ToString());
                }
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void PricemonthlyTB_LostFocus(object sender, RoutedEventArgs e)
        {
            if(PricemonthlyTB.Visibility == Visibility.Collapsed)
            {
                return;
            }
            if(focus == 0)
            {
                return;
            }
            string newValue = PricemonthlyTB.Text;
            if(CBMonths.SelectedIndex == -1) 
            {
                if (Connexion.GetString("Select MonthlyPayment From Students Where ID= " + PID) != newValue)
                {

                    Connexion.Insert("Update Students Set MonthlyPayment = " + newValue + " Where ID =" + PID);
                    DataTable dttable = new DataTable();
                    Connexion.FillDT(ref dttable, "SELECT * " +
                        "FROM[dbo].[Monthly_Payment]  " +
                        "WHERE SID =" + PID + " and ([Year] > YEAR(GETDATE()) " +
                        "OR([Year] = YEAR(GETDATE()) AND[Month] >= MONTH(GETDATE())));");

                    int status;
                    foreach (DataRow rowSuprice in dttable.Rows)
                    {
                        int sumprice = Connexion.GetInt("Select " +
                            "case WHen Sum(Price) is null then 0 " +
                            "else Sum(Price) end as f " +
                            "from studentPayment " +
                            "Where Type = 4 and deleted = 0 " +
                            "and CID = " + rowSuprice["ID"].ToString());
                        if (sumprice >= int.Parse(newValue))
                        {
                            status = 1;
                        }
                        else if (sumprice == 0)
                        {
                            status = 0;
                        }
                        else
                        {
                            status = 2;
                        }
                        Connexion.Insert("Update Monthly_payment " +
                            "Set SuTotal =" + newValue + " , " +
                            "Status =" + status + " " +
                            "where ID = " + rowSuprice["ID"].ToString());
                    }
                    MessageBox.Show("Updated Succesfully");

                }
            }
            else
            {
                DataRowView row = (DataRowView)CBMonths.SelectedItem;
                if (Connexion.GetString("Select SuTotal From Monthly_Payment Where ID= " + row["ID"].ToString()) != newValue)
                {
                    if (MessageBox.Show("?إذا كنت تريد تغيير دفعة الشهر الحالي فقط، اضغط لا. إذا كنت تريد لجميع الأشهر، اضغط نعم", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        Connexion.Insert("Update Monthly_Payment Set SuTotal = " + newValue + " Where ID =" + row["ID"].ToString());
                        int sumprice = Connexion.GetInt("Select case WHen Sum(Price) is null then 0 else Sum(Price) end as f from studentPayment Where Type = 4 and deleted = 0 and CID = " + row["ID"].ToString());
                        if (sumprice  >= int.Parse(newValue))
                        {
                            Connexion.Insert("Update Monthly_Payment Set Status = 1 Where ID =" + row["ID"].ToString());
                        }
                        else if( sumprice == 0)
                        {
                            Connexion.Insert("Update Monthly_Payment Set Status = 0 Where ID =" + row["ID"].ToString());
                        }
                        else
                        {
                            Connexion.Insert("Update Monthly_Payment Set Status = 2 Where ID =" + row["ID"].ToString());
                        }
                        MessageBox.Show("Updated Succesfully");
                    }
                    else
                    {
                        Connexion.Insert("Update Students Set MonthlyPayment = " + newValue + " Where ID =" + PID);
                        DataTable dttable = new DataTable();
                        Connexion.FillDT(ref dttable, "SELECT * " +
                            "FROM[dbo].[Monthly_Payment]  " +
                            "WHERE SID =" + PID + " and ([Year] > YEAR(GETDATE()) " +
                            "OR([Year] = YEAR(GETDATE()) AND[Month] >= MONTH(GETDATE())));");

                        int status;
                        foreach (DataRow rowSuprice in dttable.Rows)
                        {
                            int sumprice = Connexion.GetInt("Select " +
                                "case WHen Sum(Price) is null then 0 " +
                                "else Sum(Price) end as f " +
                                "from studentPayment " +
                                "Where Type = 4 and deleted = 0 " +
                                "and CID = " + rowSuprice["ID"].ToString());
                            if (sumprice >= int.Parse(newValue))
                            {
                                status = 1;
                            }
                            else if (sumprice == 0)
                            {
                                status = 0;
                            }
                            else
                            {
                                status = 2;
                            }
                            Connexion.Insert("Update Monthly_payment " +
                                "Set SuTotal =" + newValue + " , " +
                                "Status =" + status + " " +
                                "where ID = " + rowSuprice["ID"].ToString());
                        }
                        MessageBox.Show("Updated Succesfully");

                    }
                }
            }
        }

        private void PricemonthlyTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Check if the input is a valid number
            if (!IsNumber(e.Text))
            {
                e.Handled = true; // Reject the input
            }
        }

        private bool IsNumber(string input)
        {
            return input.All(char.IsDigit);
        }

        private void DatePayment_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

    }
}
