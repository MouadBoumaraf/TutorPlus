using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Gestion_De_Cours.UControl;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Data;
using System.IO;
using System.Windows.Media;
using Gestion_De_Cours.Panels;
using FastReport;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Gestion_De_Cours.Classes
{
    public class Methods
    {
        public static void AddColumn(ref ListView myGridView, string finding, string Header, int width)
        {
            GridView gv = new GridView();
            gv = (GridView)myGridView.View;
            GridViewColumn col = new GridViewColumn();
            col.Width = width;
            col.DisplayMemberBinding = new Binding(finding);
            col.Header = Header;
            gv.Columns.Add(col);
            myGridView.View = gv;
        }

    
        public static void FillDGSesStuTPaymentOld(ref DataGrid DG , int IDTPay, ref DataTable dt)
        {
            dt = new DataTable();
            string Query = "SELECT " +
                " Sessions AS SessionNumber, COUNT(SID) AS StudentsCount, SUM(TPrice) AS TotalPrice " +
                "FROM TPaymentStudent " +
                "where ID = "+ IDTPay+" " +
                "GROUP BY Sessions " +
                "ORDER BY SessionNumber; ";
            Connexion.FillDT(ref dt,Query);
            DG.ItemsSource = dt.DefaultView;
        }
        public static void FillDGSesStuTPayment(ref DataGrid DG, int StartSes, int EndSes, string GID, ref DataTable dt)
        {
            dt = new DataTable();
            StartSes++;
            string query = "SELECT " +
                    "TotalSessions, " +
                    "COUNT(StudentID) AS TotalStudents " +
                    "FROM ( " +
                    "SELECT " +
                    "AStu.StudentID, " +
                    "COUNT(DISTINCT A.Session) AS TotalSessions " +
                    "FROM Attendance A " +
                    "JOIN Attendance_Student AStu ON A.ID = AStu.ID " +
                    "JOIN Class_Student CS ON CS.StudentID = AStu.StudentID " +
                    "WHERE AStu.Status IN (0, 1,2) " +
                    "AND A.Session BETWEEN " + StartSes + " AND " + EndSes + " " +
                    "AND A.GroupID = " + GID + " " +
                    "AND CS.GroupID = " + GID + " " +
                    "GROUP BY AStu.StudentID " +
                    ") AS SubQuery " +
                    "GROUP BY TotalSessions " +
                    "ORDER BY TotalSessions;";

            Connexion.FillDT(ref dt, query);
            dt.Columns.Add("Sum", typeof(int));

            // Fill all rows with a specific value
            foreach (DataRow row in dt.Rows)
            {
                string Ses = row["TotalSessions"].ToString();
                int sum = Connexion.GetInt(
                "SELECT SUM(CalculatedPrice) " +
                "FROM ( " +
                "SELECT AStu.StudentID, " +
                "COUNT(DISTINCT A.Session) AS TotalSessionsStudied, " +
                "dbo.TPAYCALCTPRICESTU(AStu.StudentID, " + GID + ", " + EndSes + ") AS CalculatedPrice " +
                "FROM Attendance A " +
                "JOIN Attendance_Student AStu ON A.ID = AStu.ID " +
                "JOIN Class_Student CS ON CS.StudentID = AStu.StudentID " +
                "WHERE AStu.Status IN (0, 1,2) " +
                "AND A.Session BETWEEN " + StartSes + " AND " + EndSes + " " +
                "AND A.GroupID = " + GID + " " +
                "AND CS.GroupID = " + GID + " " + // Ensure students belong to the specified GroupID
                "GROUP BY AStu.StudentID " +
                ") AS SubQuery " +
                "WHERE SubQuery.TotalSessionsStudied = " + Ses + ";");
                row["Sum"] = sum;
            }
            var rowToModify = dt.AsEnumerable()
                            .FirstOrDefault(row => row.Field<int>("TotalSessions") == 1);

            if (rowToModify != null)
            {

                // Modify the columns (for example, changing the "other_column" value)
                rowToModify["TotalStudents"] = int.Parse(rowToModify["TotalStudents"].ToString()) + Connexion.GetInt("SELECT COUNT(*) FROM Attendance_StudentsOneSes AS ASO JOIN Attendance AS A ON ASO.AID = A.ID " +
                "WHERE A.GroupID = " + GID + " AND A.Session BETWEEN " + StartSes + " AND " + EndSes);
                rowToModify["Sum"] = int.Parse(rowToModify["Sum"].ToString()) + Connexion.GetInt("Select dbo.GetTotalOneSesStudents(" + StartSes + "," + EndSes + "," + GID + ")");

            }
            else
            {
                DataRow newrow = dt.NewRow();
                newrow["TotalSessions"] = 1;
                newrow["TotalStudents"] = Connexion.GetInt("SELECT COUNT(*) FROM Attendance_StudentsOneSes AS ASO JOIN Attendance AS A ON ASO.AID = A.ID " +
                "WHERE A.GroupID = " + GID + " AND A.Session BETWEEN " + StartSes + " AND " + EndSes);
                newrow["Sum"] = Connexion.GetInt("Select dbo.GetTotalOneSesStudents(" + StartSes + "," + EndSes + "," + GID + ")");
                dt.Rows.InsertAt(newrow,0);

            }
            DG.ItemsSource = dt.DefaultView;

        }

        public static void FillDGSesStuTPayment( int StartSes, int EndSes, string GID, ref DataTable dt)
        {
            dt = new DataTable();
            StartSes++;
            string query = "SELECT " +
                    "TotalSessions, " +
                    "COUNT(StudentID) AS TotalStudents " +
                    "FROM ( " +
                    "SELECT " +
                    "AStu.StudentID, " +
                    "COUNT(DISTINCT A.Session) AS TotalSessions " +
                    "FROM Attendance A " +
                    "JOIN Attendance_Student AStu ON A.ID = AStu.ID " +
                    "JOIN Class_Student CS ON CS.StudentID = AStu.StudentID " +
                    "WHERE AStu.Status IN (0, 1,2) " +
                    "AND A.Session BETWEEN " + StartSes + " AND " + EndSes + " " +
                    "AND A.GroupID = " + GID + " " +
                    "AND CS.GroupID = " + GID + " " +
                    "GROUP BY AStu.StudentID " +
                    ") AS SubQuery " +
                    "GROUP BY TotalSessions " +
                    "ORDER BY TotalSessions;";
            Connexion.FillDT(ref dt, query);
            dt.Columns.Add("Sum", typeof(int));

            // Fill all rows with a specific value
            foreach (DataRow row in dt.Rows)
            {
                string Ses = row["TotalSessions"].ToString();
                int sum = Connexion.GetInt(
                "SELECT SUM(CalculatedPrice) " +
                "FROM ( " +
                "SELECT AStu.StudentID, " +
                "COUNT(DISTINCT A.Session) AS TotalSessionsStudied, " +
                "dbo.TPAYCALCTPRICESTU(AStu.StudentID, " + GID + ", " + EndSes + ") AS CalculatedPrice " +
                "FROM Attendance A " +
                "JOIN Attendance_Student AStu ON A.ID = AStu.ID " +
                "JOIN Class_Student CS ON CS.StudentID = AStu.StudentID " +
                "WHERE AStu.Status IN (0, 1,2) " +
                "AND A.Session BETWEEN " + StartSes + " AND " + EndSes + " " +
                "AND A.GroupID = " + GID + " " +
                "AND CS.GroupID = " + GID + " " + // Ensure students belong to the specified GroupID
                "GROUP BY AStu.StudentID " +
                ") AS SubQuery " +
                "WHERE SubQuery.TotalSessionsStudied = " + Ses + ";");
                row["Sum"] = sum;
            }

        }
        public static void InsertStudentClassMonthly(string SID, string ClassID)
        {
            int price = Connexion.GetInt("Select Case When MonthlyPayment is null then 0 else MonthlyPayment end as f From Students Where ID = " + SID);
            if (price == 0)
            {
                price = Connexion.GetInt("Select CPrice from Class Where id  = " + ClassID);
                Connexion.Insert("Update Students Set MonthlyPayment = " + price + " Where ID = " + SID);
            }
            else
            {
                string Subjects = Connexion.GetString("Select dbo.GetStudentSubjects(" + SID + ")");
                Backup newmonthprice = new Backup("This Student now studies in(" + Subjects + ")", 1, price.ToString());
                if (newmonthprice.ShowDialog() == true)
                {
                    price = int.Parse(newmonthprice.ResponseText);
                }
                else
                {
                    price = Connexion.GetInt("Select Sum(CPrice) from Class  Join Class_Student On Class_Student.ClassID = Class.ID where StudentID = " + SID);
                }
                Connexion.Insert("Update Students Set MonthlyPayment = " + price + " Where ID =" + SID);
            }
            DataTable dtmonthes = new DataTable();
            Connexion.FillDT(ref dtmonthes, "WITH DistinctMonths AS " +
                        "(SELECT " +
                        "DISTINCT DATENAME(MONTH, CONVERT(DATE, a.Date, 103)) AS AttendanceMonth," +
                        " YEAR(CONVERT(DATE, a.Date, 103)) AS AttendanceYear, " +
                        "MONTH(CONVERT(DATE, a.Date, 103)) AS MonthNumber " +
                        "FROM   Attendance a " +
                        "INNER JOIN attendance_Student ast ON a.ID = ast.ID  " +
                        "WHERE ast.StudentID ='" + SID + "' ) " +
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

            // Add the next months to your DataGrid
            for (int i = 1; i < 4; i++) // Assuming you want to add the next 3 months
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
              
                if (Connexion.IFNULL("Select ID from Monthly_Payment WHere SID = " + SID + " and  Month = " + row["MonthNumber"].ToString() + " and Year = " + attendanceYear))
                {
                    PaymentID = Connexion.GetInt("Insert into Monthly_Payment " +
                        "OUTPUT INSERTED.ID " +
                        "Values (" + SID + "," + row["MonthNumber"].ToString() + ",0," + Connexion.GetInt("Select MonthlyPayment From Students Where ID =" + SID) + "," + attendanceYear + ")");
               
                }
            }
                int status;
            DataTable dttable = new DataTable();
            Connexion.FillDT(ref dttable, "SELECT * " +
                       "FROM [dbo].[Monthly_Payment]  " +
                       "WHERE SID =" + SID + " and ([Year] > YEAR(GETDATE()) " +
                       "OR([Year] = YEAR(GETDATE()) AND[Month] >= MONTH(GETDATE())));");

            foreach (DataRow rowSuprice in dttable.Rows)
            {
                int sumprice = Connexion.GetInt("Select " +
                    "case WHen Sum(Price) is null then 0 " +
                    "else Sum(Price) end as f " +
                    "from studentPayment " +
                    "Where Type = 4 and deleted = 0 " +
                    "and CID = " + rowSuprice["ID"].ToString());
                if (sumprice >= price)
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
                    "Set SuTotal =" + price + " , " +
                    "Status =" + status + " " +
                    "where ID = " + rowSuprice["ID"].ToString());
            }
        }
        public static void ReadPatches(string Path)
        {
            StreamReader file = new StreamReader(Path);
            string f = "";
            while ((f = file.ReadLine()) != "!!!!")
            {
                string query = f;
                while ((f = file.ReadLine()) != "@@@@@")
                {
                    query += f  + " "+ Environment.NewLine;
                }
                Connexion.Insert(query);
            }
        }

        public static void ChangeGridHeight(ref RowDefinition row , string height)
        {
            GridLengthConverter myGridLengthConverter = new GridLengthConverter();
            GridLength gl1 = (GridLength)myGridLengthConverter.ConvertFromString(height);
            row.Height = gl1;
        }
        public static void ExceptionHandle(Exception ex)
        {

           

                MessageBox.Show("يُوجد خلل في البرنامج،", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                string path = @"C:\ProgramData\EcoleSetting\" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
                if (File.Exists(path))
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine(ex.ToString());
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(ex.ToString());
                        sw.Close();
                    }
                }
          

        }

        public static int GetGroupsTotal(ref DataGrid DG)
        {
            int Total = 0;
            foreach (DataRowView dr in DG.ItemsSource)
            {
                if (dr["TotalP"] != null && dr["TotalP"].ToString() != "")
                {
                    Total += int.Parse(dr["TotalP"].ToString());
                }
            }
            return Total; 
        }
        public static void FillDtAttendanceGroupPayment(string GID , ref DataTable dt)
        {
            DataTable dtAttendanceDates = new DataTable();
           
                int count = 1;
                Connexion.FillDT(ref dtAttendanceDates, "Select '(' + cast(attendance.Session AS varchar(50)) + ')' + Attendance.Date as Date  from Attendance Where GroupID = " + GID + " ORDER BY convert(datetime, Attendance.Date, 103)");
                string query = "Select * From (Select Students.ID , '(' + cast(attendance.Session AS varchar(50)) + ')' + Attendance.Date as date , '' as stat " +
                     "from Attendance " +
                     "left join Attendance_Student on Attendance_Student.ID = Attendance.ID  " +
                     "Join Students on Students.ID = Attendance_Student.StudentID " +
                     "left join Attendance_Change " +
                     "on ( Attendance_Change.FromGroupID = Attendance.GroupID  and Attendance.Session = Attendance_Change.Session) " +
                     "left join Groups as G on Attendance_Change.ToGroupID = G.GroupID " +
                     "Join Groups on Groups.GroupID = Attendance.GroupID  " +
                     "Join Class_Student on (Class_Student.StudentID = Students.ID and Class_Student.GroupID = Groups.GroupID) " +
                     "where Attendance.GroupID = " + GID + "  ) as T  " +
                     "Pivot(max(stat) FOR Date IN (";

                foreach (DataRow row in dtAttendanceDates.Rows)
                {
                    if (count == 1)
                    {
                        query += "[" + row["Date"].ToString() + "]";
                    }
                    else
                    {
                        query += ",[" + row["Date"].ToString() + "]";
                    }
                    count++;
                }
                query += ")) as F order by f.ID asc";
                Connexion.FillDT(ref dt, query);
            

        }
       
        public static void FillDTAttendance(int ses, string GID, ref DataTable dt2, int SessionG)
        {
            
            DataTable dt = new DataTable();
            Connexion.FillDT(ref dt, "Select '(' + cast(attendance.Session AS varchar(50)) + ')' + Attendance.Date as Date  from Attendance Where Session >" + SessionG + " And GroupID = " + GID + "   ORDER BY convert(datetime, Attendance.Date, 103)");
            int count = 1;
            string Absent = "Absent";
            string Present = "Present";
            string Justified = "Justified";
            if (Connexion.Language() == 1)
            {
                Absent = "غائب";
                Present = "حاضر";
                Justified = "مبرر";
            }
            string query = "Select * From (Select Students.FirstName + ' ' + Students.LastName as Name," +
                 "Students.Gender as Gender," +
                 "Students.ID as ID , " +
                 "Case when Attendance_Student.Status = 0 Then N'"+ Absent + "' " +
                 "When Attendance_Student.Status = 1 Then N'"+ Present + "' " +
                 "When Attendance_Student.Status = 2 Then " +
                 "G.GroupName + N'" +
                 "(' + dbo.GetDateStudentChange(Attendance_Change.ToGroupID,Attendance_Change.Session,Attendance_Change.StudentID) + ')' " +
                 "When Attendance_Student.Status = 3 Then N'"+ Justified+"' End as stat," +
                 "'(' + cast(attendance.Session AS varchar(50)) + ')' + Attendance.Date as Date, " +
                 "Case When dbo.calculatesesPayed(Students.ID , Attendance.GroupID ) -dbo.calculatesesPayedT(Students.ID , Attendance.GroupID ) >=  " + ses + " then " + ses +
                 "When dbo.calculatesesPayed(Students.ID , Attendance.GroupID ) -dbo.calculatesesPayedT(Students.ID , Attendance.GroupID ) <  " + ses + " then dbo.calculatesesPayed(Students.ID , Attendance.GroupID ) -dbo.calculatesesPayedT(Students.ID , Attendance.GroupID ) end as Tses  ," +
                 "dbo.CalculatePrice(Students.ID ,Attendance.GroupID, " + ses + ", 'S') as SPrice" +
                 ",dbo.CalculatePrice(Students.ID, Attendance.GroupID, " + ses + ", 'T') as TPrice" +
                  ",dbo.CalculatePrice(Students.ID ,Attendance.GroupID, " + ses + ", 'Su') as SuPrice " +
                 "from Attendance " +
                 "left join Attendance_Student on Attendance_Student.ID = Attendance.ID  " +
                 "Join Students on Students.ID = Attendance_Student.StudentID " +
                 "left join Attendance_Change " +
                 "on ( Attendance_Change.FromGroupID = Attendance.GroupID  and Attendance.Session = Attendance_Change.Session) " +
                 "left join Groups as G on Attendance_Change.ToGroupID = G.GroupID " +
                 "Join Groups on Groups.GroupID = Attendance.GroupID  " +
                 "Join Class_Student on (Class_Student.StudentID = Students.ID and Class_Student.GroupID = Groups.GroupID) " +
                 "where Attendance.GroupID = " + GID + "  ) as T  " +
                 "Pivot(max(stat) FOR Date IN (";

            foreach (DataRow row in dt.Rows)
            {
                if (count <= ses)
                {
                    if (count == 1)
                    {
                        query += "[" + row["Date"].ToString() + "]";
                    }
                    else
                    {
                        query += ",[" + row["Date"].ToString() + "]";
                    }
                    count++;
                }
            }
            query += ")) as F order by f.ID asc";

            Connexion.FillDT(ref dt2, query);
            int sumTPrice = 0, sumStudentpay = 0;
            DataRow newRow = dt2.NewRow();

            foreach (DataColumn column in dt2.Columns)
            {
                if (column.ColumnName == "Name")
                {
                    newRow[column.ColumnName] = "طلاب إضافيين";
                }
                else if (column.ColumnName == "ID")
                {
                    newRow[column.ColumnName] = "-1";
                }
                else
                {
                    string pattern = @"\((.*?)\)";

                    // Create a Regex object and try to match the pattern
                    Regex regex = new Regex(pattern);

                    // Find the match
                    Match match = regex.Match(column.ColumnName);

                    // If a match is found, return the captured group (text between parentheses)
                    if (match.Success)
                    {
                        string Ses = match.Groups[1].Value;
                        string AID = Connexion.GetString("Select ID from Attendance Where Session = " + Ses + " and GroupID = " + GID);
                        string Total = Connexion.GetString("Select count(*) from Attendance_StudentsOneSes Where AID = " + AID);
                        newRow[column.ColumnName] = Total;
                        sumTPrice += Connexion.GetInt("Select case when Sum(TPrice) is null then 0 else Sum(tprice) end as f  from attendance_StudentsOneSes Where AID=" + AID);
                        sumStudentpay += Connexion.GetInt("Select case when Sum(price) is null then 0 else Sum(price) end as f  from attendance_StudentsOneSes Where AID=" + AID);

                    }
                }

            }
            newRow["SPrice"] = sumStudentpay;
            newRow["SuPrice"] = sumStudentpay;
            newRow["TPrice"] = sumTPrice;
            dt2.Rows.Add(newRow);
        }
        public static void FillDGAttendance(ref DataGrid DG , int ses , string GID ,ref DataTable dt2 )
        {
            int f = DG.Columns.Count();
            for (int i = 1; i < f; i++)
            {
                DG.Columns.RemoveAt(1);
            }
            int EndSes = ses + Connexion.GetInt("Select Sessions - TSessions from Groups where GroupID = " + GID);
            int SessionG = Connexion.GetSessions(GID);
            DataTable dt = new DataTable();
            Connexion.FillDT(ref dt, "Select '(' + cast(attendance.Session AS varchar(50)) + ')' + Attendance.Date as Date  from Attendance Where Session > " + SessionG + " And GroupID = " + GID + "   ORDER BY convert(datetime, Attendance.Date, 103)");
            int count = 1;
            string Absent = "Absent";
            string Present = "Present";
            string Justified = "Justified";
            if(Connexion.Language() == 1)
            {
                Absent = "غائب";
                Present = "حاضر";
                Justified = "مبرر";
            }
          
            string query = "Select * From (Select Students.FirstName + ' ' + Students.LastName as Name," +
                "Students.Gender as Gender," +
                "Students.ID as ID , " +
                "Case when Attendance_Student.Status = 0 Then N'"+ Absent+"' " +
                "When Attendance_Student.Status = 1 Then N'"+ Present + "' " +
                "When Attendance_Student.Status = 2 Then " +
                "G.GroupName + '" +
                "(' + dbo.GetDateStudentChange(Attendance_Change.ToGroupID,Attendance_Change.Session,Attendance_Change.StudentID) + ')' " +
                "When Attendance_Student.Status = 3 Then N'" + Justified + "' End as stat," +
                "'(' + cast(attendance.Session AS varchar(50)) + ')' + Attendance.Date as Date, " +
                "dbo.CalculateCount(Students.ID , Attendance.GroupID, "+ ses + ") as TSes , " +
                "dbo.TPayPrice(Students.ID ,"+Connexion.GetClassID(GID) +", " + EndSes + ", 'Paid') as SPrice" +
                ",dbo.TPayPrice(Students.ID ," + Connexion.GetClassID(GID) + ", " + EndSes + ", 'T') as TPrice" +
                 ",dbo.TPayPrice(Students.ID ," + Connexion.GetClassID(GID) + ", " + EndSes + ", 'SU') as SuPrice " +
                "from Attendance " +
                "left join Attendance_Student on Attendance_Student.ID = Attendance.ID  " +
                "Join Students on Students.ID = Attendance_Student.StudentID " +
                "left join Attendance_Change " +
                "on ( Attendance_Change.FromGroupID = Attendance.GroupID  and Attendance.Session = Attendance_Change.Session) " +
                "left join Groups as G on Attendance_Change.ToGroupID = G.GroupID " +
                "Join Groups on Groups.GroupID = Attendance.GroupID  " +
                "Join Class_Student on (Class_Student.StudentID = Students.ID and Class_Student.GroupID = Groups.GroupID) " +
                "where Attendance.GroupID = " + GID + "  ) as T  " +
                "Pivot(max(stat) FOR Date IN (";

            foreach (DataRow row in dt.Rows)
            {
                if (count <= ses)
                {
                    Methods.AddcolumnDG(ref DG, row["Date"].ToString(), row["Date"].ToString());
                    if (count == 1)
                    {
                        query += "[" + row["Date"].ToString() + "]";
                    }
                    else
                    {
                        query += ",[" + row["Date"].ToString() + "]";
                    }
                    count++;
                }
            }
            query += ")) as F order by f.ID asc";
            string price = "Price";
            string outof = "Out of";
            string teacherPayment = "Teacher Payment"; 
            if(Connexion.Language() == 1)
            {
                price = "سعر";
                outof = "السعر المفترض";
                teacherPayment = "دفع  الأستاذ";
            }
            Methods.AddcolumnDG(ref DG, price, "SPrice");
            Methods.AddcolumnDG(ref DG, outof, "SuPrice");
            Methods.AddcolumnDG(ref DG, teacherPayment ,  "TPrice");
            Connexion.FillDT(ref dt2, query);
            int sumTPrice = 0, sumStudentpay = 0 ;
            DataRow newRow = dt2.NewRow();

            foreach (DataColumn column in dt2.Columns)
            {
                if (column.ColumnName == "Name")
                {
                    newRow[column.ColumnName] = "طلاب إضافيين";
                }
                else if (column.ColumnName == "ID")
                {
                    newRow[column.ColumnName] = "-1";
                }
                else
                {
                    string pattern = @"\((.*?)\)";

                    // Create a Regex object and try to match the pattern
                    Regex regex = new Regex(pattern);

                    // Find the match
                    Match match = regex.Match(column.ColumnName);

                    // If a match is found, return the captured group (text between parentheses)
                    if (match.Success)
                    {
                        string Ses = match.Groups[1].Value;
                        string AID = Connexion.GetString("Select ID from Attendance Where Session = " + Ses + " and GroupID = " + GID);
                        string Total = Connexion.GetString("Select count(*) from Attendance_StudentsOneSes Where AID = " + AID);
                        newRow[column.ColumnName] = Total;
                        sumTPrice += Connexion.GetInt("Select case when Sum(TPrice) is null then 0 else Sum(tprice) end as f  from attendance_StudentsOneSes Where AID=" + AID);
                        sumStudentpay += Connexion.GetInt("Select case when Sum(price) is null then 0 else Sum(price) end as f  from attendance_StudentsOneSes Where AID=" + AID);

                    }
                }

            }
            newRow["SPrice"] = sumStudentpay;
            newRow["SuPrice"] = sumStudentpay;
            newRow["TPrice"] = sumTPrice;
            dt2.Rows.Add(newRow);
            DG.ItemsSource = dt2.DefaultView;
        }

        public static void FillDGAttendanceOld(int PaymentID,  ref DataTable dt , ref DataGrid DG)
        {
            DataTable dtDates = new DataTable();
            int StartSes = Connexion.GetInt("Select FromSes From Tpayment Where ID = " + PaymentID) + 1;
            int ENDSes = StartSes + Connexion.GetInt("Select Ses from TPayment Where ID = " + PaymentID);
            int GID = Connexion.GetInt("Select GID From TPayment Where ID = " + PaymentID);
            Connexion.FillDT(ref dtDates, "Select '(' + cast(attendance.Session AS varchar(50)) + ')' + Attendance.Date as Date  from Attendance Where Session >= " + StartSes  + " And Session < "+ ENDSes +" and GroupID = " + GID + "   ORDER BY convert(datetime, Attendance.Date, 103)");
            string Absent = "Absent";
            string Present = "Present";
            string Justified = "Justified";
            if (Connexion.Language() == 1)
            {
                Absent = "غائب";
                Present = "حاضر";
                Justified = "مبرر";
            }
            string query = "Select * From (Select Students.FirstName + ' ' + Students.LastName as Name," +
                "Students.Gender as Gender," +
                "Students.ID as ID , " +
                "Case when Attendance_Student.Status = 0 Then N'" + Absent + "' " +
                "When Attendance_Student.Status = 1 Then N'" + Present + "' " +
                "When Attendance_Student.Status = 2 Then " +
                "G.GroupName + '" +
                "(' + dbo.GetDateStudentChange(Attendance_Change.ToGroupID,Attendance_Change.Session,Attendance_Change.StudentID) + ')' " +
                "When Attendance_Student.Status = 3 Then N'" + Justified + "' End as stat , " +
                "'(' + cast(attendance.Session AS varchar(50)) + ')' + Attendance.Date as Date " +
                "from Attendance " +
                "left join Attendance_Student on Attendance_Student.ID = Attendance.ID  " +
                "Join Students on Students.ID = Attendance_Student.StudentID " +
                "left join Attendance_Change " +
                "on ( Attendance_Change.FromGroupID = Attendance.GroupID  and Attendance.Session = Attendance_Change.Session) " +
                "left join Groups as G on Attendance_Change.ToGroupID = G.GroupID " +
                "Join Groups on Groups.GroupID = Attendance.GroupID  " +
                "Join Class_Student on (Class_Student.StudentID = Students.ID and Class_Student.GroupID = Groups.GroupID) " +
                "where Attendance.GroupID = " + GID + "  ) as T  " +
                "Pivot(max(stat) FOR Date IN (";
            int count = 1;
            foreach (DataRow row in dtDates.Rows)
            {
                Methods.AddcolumnDG(ref DG, row["Date"].ToString(), row["Date"].ToString());
                if (count == 1)
                {
                    query += "[" + row["Date"].ToString() + "]";
                }
                else
                {
                    query += ",[" + row["Date"].ToString() + "]";
                }
                count++;

            }
            query += ")) as F order by f.ID asc";
        
            Connexion.FillDT(ref dt, query);
            dt.Columns.Add("SuTotal");
            Methods.AddcolumnDG(ref DG,"Total" ,"SuTotal");
            foreach(DataRow rowdt in dt.Rows)
            {
                string SID = rowdt["ID"].ToString();
                rowdt["SuTotal"] =  Connexion.GetInt("Select TPrice from TPaymentStudent Where ID = " + PaymentID + " and SID = " + SID);
            }
            DG.ItemsSource = dt.DefaultView;
        }
        public static void FillDGAttendance( int ses, string GID, ref DataTable dt2 , ref DataTable dt , ref DataGrid DG)
        {
            int SessionG = Connexion.GetSessions(GID);

            int count = 1;
            string query = "Select * From (Select Students.FirstName + ' ' + Students.LastName as Name," +
              "Students.Gender as Gender," +
              "Students.ID as ID , " +
             "Case when Attendance_Student.Status = 0 Then 'Absent' " +
             "When Attendance_Student.Status = 1 Then 'Present' " +
             "When Attendance_Student.Status = 2 Then " +
             "G.GroupName + '" +
             "'(' + dbo.GetDateStudentChange(Attendance_Change.ToGroupID,Attendance_Change.Session,Attendance_Change.StudentID) + ')' End as stat," +
             "Attendance.Date, " +
            "Case When dbo.calculatesesPayed(Students.ID , Attendance.GroupID ) -dbo.calculatesesPayedT(Students.ID , Attendance.GroupID ) >=  " + ses + " then " + ses +
            "When dbo.calculatesesPayed(Students.ID , Attendance.GroupID ) -dbo.calculatesesPayedT(Students.ID , Attendance.GroupID ) <  " + ses + " then dbo.calculatesesPayed(Students.ID , Attendance.GroupID ) -dbo.calculatesesPayedT(Students.ID , Attendance.GroupID ) end as Tses  ," +
            "dbo.CalculatePrice(Students.ID ,Attendance.GroupID, " + ses + ", 'S') as SPrice" +
            ",dbo.CalculatePrice(Students.ID, Attendance.GroupID, " + ses + ", 'T') as TPrice" +
            ",dbo.CalculatePrice(Students.ID ,Attendance.GroupID, " + ses + ", 'Su') as SuPrice " +
            "from Attendance " +
            "left join Attendance_Student on Attendance_Student.ID = Attendance.ID  " +
            "Join Students on Students.ID = Attendance_Student.StudentID " +
            "left join Attendance_Change " +
            "on ( Attendance_Change.FromGroupID = Attendance.GroupID  and Attendance.Session = Attendance_Change.Session) " +
            "left join Groups as G on Attendance_Change.ToGroupID = G.GroupID " +
            "Join Groups on Groups.GroupID = Attendance.GroupID  " +
            "Join Class_Student on (Class_Student.StudentID = Students.ID and Class_Student.GroupID = Groups.GroupID) " +
            "where Attendance.GroupID = " + GID + "  ) as T  " +
            "Pivot(max(stat) FOR Date IN (";
            foreach (DataRow row in dt.Rows)
            {
                Methods.AddcolumnDG(ref DG, row["Date"].ToString(), row["Date"].ToString());
                if (count == 1)
                {
                    query += "[" + row["Date"].ToString() + "]";
                }
                else
                {
                    query += ",[" + row["Date"].ToString() + "]";
                }
                count++;

            }
            query += ")) as F order by f.ID asc";          
            Connexion.FillDT(ref dt2, query);
        }

        public static void AddColumnDT( ref DataTable dt  , string Type , string Name)
        {
            DataColumn columnName;
            columnName = new DataColumn();
            columnName.DataType = System.Type.GetType(Type);
            columnName.ColumnName = Name;
            dt.Columns.Add(columnName);
        }
        public static void AddcolumnDG(ref DataGrid DG, string Header, string binding  )
        {
            DataGridTextColumn textColumn = new DataGridTextColumn();
            textColumn.Header = Header;
            textColumn.Binding = new Binding(binding);
            DG.Columns.Add(textColumn);
        }
        public static void AddcolumnLV(ref ListView LV, string Header, string binding)
        {
            // Ensure the ListView uses a GridView
            GridView gridView = LV.View as GridView;
            if (gridView == null)
            {
                gridView = new GridView();
                LV.View = gridView;
            }

            // Create and add a GridViewColumn
            GridViewColumn column = new GridViewColumn
            {
                Header = Header,
                DisplayMemberBinding = new Binding(binding)
            };
            gridView.Columns.Add(column);
        }
        public static void AddcolumnDGSetterBackGround(ref DataGrid DG, string Header, string binding , string bindingTrigger , string triggervalue , SolidColorBrush settercolor)
        {
            DataGridTextColumn textColumn = new DataGridTextColumn();
            textColumn.Header = Header;
            textColumn.Binding = new Binding(binding);
            DG.Columns.Add(textColumn);

            // Create a DataTrigger to change the text color for the specific column
            DataTrigger dataTrigger = new DataTrigger();
            dataTrigger.Binding = new Binding(bindingTrigger); // Bind to the EmptyAttend property
            dataTrigger.Value = triggervalue; // Value to trigger on

            Setter setter = new Setter();
            setter.Property = TextBlock.ForegroundProperty; // Set the text (foreground) color
            setter.Value = settercolor; // Set the text color to red

            dataTrigger.Setters.Add(setter);
            textColumn.CellStyle = new System.Windows.Style(typeof(DataGridCell));
            textColumn.CellStyle.Triggers.Add(dataTrigger);
       

        }

        public static void AddcolumnDGSetterBackGround(ref ListView LV, string Header, string binding, string bindingTrigger, string triggervalue, SolidColorBrush settercolor)
        {
            // Ensure the ListView uses a GridView
            GridView gridView = LV.View as GridView;
            if (gridView == null)
            {
                gridView = new GridView();
                LV.View = gridView;
            }

            // Create and add a GridViewColumn
            GridViewColumn column = new GridViewColumn
            {
                Header = Header
            };

            // Create a DataTemplate for cell style with trigger
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetBinding(TextBlock.TextProperty, new Binding(binding));
            factory.SetValue(TextBlock.ForegroundProperty, Brushes.Black);

            template.VisualTree = factory;

            // Add DataTrigger for foreground color
            var trigger = new DataTrigger
            {
                Binding = new Binding(bindingTrigger),
                Value = triggervalue
            };
            trigger.Setters.Add(new Setter(TextBlock.ForegroundProperty, settercolor));

            template.Triggers.Add(trigger);

            column.CellTemplate = template;

            gridView.Columns.Add(column);
        }

        public static void AddcolumnDG(ref DataGrid DG, string Header, string binding , string type)
        {
            DataGridTextColumn textColumn = new DataGridTextColumn();
            textColumn.Header = Header;
            textColumn.Binding = new Binding(binding);
            DG.Columns.Add(textColumn);
            DataGridColumn lastColumn = DG.Columns[DG.Columns.Count - 1];
            if (type == "Student")
            {
                if (binding == "Sort")
                {
                    if (Connexion.GetInt("Select Case When FSID is null then 1 else FSID end as f from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed;
                    }
                }
                if (binding == "BarCode")
                {
                    if (Connexion.GetInt("Select Case When FSBarCode is null then 1 else FSBarCode end as f from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed;
                    }
                }
                else if (binding == "Name")
                {
                    if (Connexion.GetInt("Select Case When FSName is null then 1 else FSName end as f from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed; 
                    }
                }
                else if(binding == "PhoneNumber")
                {
                    if (Connexion.GetInt("Select Case When FSPhone is null then 1 else FSPhone end as f from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed;
                    }
                }
                else if (binding == "ParentNumber")
                {
                    if (Connexion.GetInt("Select Case When FSParentNumber is null then 1 else FSParentNumber end as f  from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed;
                    }
                }
                else if (binding == "Genderr")
                {
                    if (Connexion.GetInt("Select Case When FSGender is null then 1 else FSGender end as f from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed;
                    }
                }
                else if (binding == "Birthdate")
                {
                    if (Connexion.GetInt("Select Case When FSBirthDate is null then 1 else FSBirthDate end as f from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed;
                    }
                }
                else if (binding == "Level")
                {
                    if (Connexion.GetInt("Select Case When FSLevel is null then 1 else FSLevel end as f from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed;
                    }
                }
                else if (binding == "Year")
                {
                    if (Connexion.GetInt("Select Case When FSYear is null then 1 else FSYear end as f from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed;
                    }
                }
                else if (binding == "Spec")
                {
                    if (Connexion.GetInt("Select Case When FSSpeciality is null then 1 else FSSpeciality end as f from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed;
                    }
                }
                else if (binding == "Adress")
                {
                    if (Connexion.GetInt("Select Case When FSAdress is null then 1 else FSAdress end as f from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed;
                    }
                }
                else if (binding == "Register")
                {
                    if (Connexion.GetInt("Select Case When FSRegister is null then 1 else FSRegister end as f from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed;
                    }
                }
                else if (binding == "Note")
                {
                    if (Connexion.GetInt("Select Case When FSNote is null then 1 else FSNote end as f from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed;
                    }
                }
                else if (binding == "Insc")
                {
                    if (Connexion.GetInt("Select Case When FSInsc is null then 1 else FSInsc end as f from EcoleSetting") != 1)
                    {
                        lastColumn.Visibility = Visibility.Collapsed;
                    }
                }

            }
           
        }

        public static void AddcolumnLV(ref ListView LV, string Header, string binding, string type)
        {
            // Ensure the ListView uses a GridView
            GridView gridView = LV.View as GridView;
            if (gridView == null)
            {
                gridView = new GridView();
                LV.View = gridView;
            }

            // Check visibility for "Student" type columns
            bool isVisible = true;
            if (type == "Student")
            {
                if (binding == "Sort")
                    isVisible = Connexion.GetInt("Select Case When FSID is null then 1 else FSID end as f from EcoleSetting") == 1;
                else if (binding == "BarCode")
                    isVisible = Connexion.GetInt("Select Case When FSBarCode is null then 1 else FSBarCode end as f from EcoleSetting") == 1;
                else if (binding == "Name")
                    isVisible = Connexion.GetInt("Select Case When FSName is null then 1 else FSName end as f from EcoleSetting") == 1;
                else if (binding == "PhoneNumber")
                    isVisible = Connexion.GetInt("Select Case When FSPhone is null then 1 else FSPhone end as f from EcoleSetting") == 1;
                else if (binding == "ParentNumber")
                    isVisible = Connexion.GetInt("Select Case When FSParentNumber is null then 1 else FSParentNumber end as f  from EcoleSetting") == 1;
                else if (binding == "Genderr")
                    isVisible = Connexion.GetInt("Select Case When FSGender is null then 1 else FSGender end as f from EcoleSetting") == 1;
                else if (binding == "Birthdate")
                    isVisible = Connexion.GetInt("Select Case When FSBirthDate is null then 1 else FSBirthDate end as f from EcoleSetting") == 1;
                else if (binding == "Level")
                    isVisible = Connexion.GetInt("Select Case When FSLevel is null then 1 else FSLevel end as f from EcoleSetting") == 1;
                else if (binding == "Year")
                    isVisible = Connexion.GetInt("Select Case When FSYear is null then 1 else FSYear end as f from EcoleSetting") == 1;
                else if (binding == "Spec")
                    isVisible = Connexion.GetInt("Select Case When FSSpeciality is null then 1 else FSSpeciality end as f from EcoleSetting") == 1;
                else if (binding == "Adress")
                    isVisible = Connexion.GetInt("Select Case When FSAdress is null then 1 else FSAdress end as f from EcoleSetting") == 1;
                else if (binding == "Register")
                    isVisible = Connexion.GetInt("Select Case When FSRegister is null then 1 else FSRegister end as f from EcoleSetting") == 1;
                else if (binding == "Note")
                    isVisible = Connexion.GetInt("Select Case When FSNote is null then 1 else FSNote end as f from EcoleSetting") == 1;
                else if (binding == "Insc")
                    isVisible = Connexion.GetInt("Select Case When FSInsc is null then 1 else FSInsc end as f from EcoleSetting") == 1;
            }

            if (isVisible)
            {
                GridViewColumn column = new GridViewColumn
                {
                    Header = Header,
                    DisplayMemberBinding = new Binding(binding)
                };
                gridView.Columns.Add(column);
            }
        }
        public static bool CheckIfVisible(string type , string binding)
        {
            bool result = true; 
            if (type == "Student")
            {
                if (binding == "Sort")
                {
                    if (Connexion.GetInt("Select Case When FSID is null then 1 else FSID end as f from EcoleSetting") != 1)
                    {
                        result = false; 
                    }
                }
                if (binding == "BarCode")
                {
                    if (Connexion.GetInt("Select Case When FSBarCode is null then 1 else FSBarCode end as f from EcoleSetting") != 1)
                    {
                        result = false;
                    }
                }
                else if (binding == "Name")
                {
                    if (Connexion.GetInt("Select Case When FSName is null then 1 else FSName end as f from EcoleSetting") != 1)
                    {
                        result = false;
                    }
                }
                else if (binding == "PhoneNumber")
                {
                    if (Connexion.GetInt("Select Case When FSPhone is null then 1 else FSPhone end as f from EcoleSetting") != 1)
                    {
                        result = false;
                    }
                }
                else if (binding == "ParentNumber")
                {
                    if (Connexion.GetInt("Select Case When FSParentNumber is null then 1 else FSParentNumber end as f  from EcoleSetting") != 1)
                    {
                        result = false;
                    }
                }
                else if (binding == "Genderr")
                {
                    if (Connexion.GetInt("Select Case When FSGender is null then 1 else FSGender end as f from EcoleSetting") != 1)
                    {
                        result = false;
                    }
                }
                else if (binding == "Birthdate")
                {
                    if (Connexion.GetInt("Select Case When FSBirthDate is null then 1 else FSBirthDate end as f from EcoleSetting") != 1)
                    {
                        result = false;
                    }
                }
                else if (binding == "Level")
                {
                    if (Connexion.GetInt("Select Case When FSLevel is null then 1 else FSLevel end as f from EcoleSetting") != 1)
                    {
                        result = false;
                    }
                }
                else if (binding == "Year")
                {
                    if (Connexion.GetInt("Select Case When FSYear is null then 1 else FSYear end as f from EcoleSetting") != 1)
                    {
                        result = false;
                    }
                }
                else if (binding == "Spec")
                {
                    if (Connexion.GetInt("Select Case When FSSpeciality is null then 1 else FSSpeciality end as f from EcoleSetting") != 1)
                    {
                        result = false;
                    }
                }
                else if (binding == "Adress")
                {
                    if (Connexion.GetInt("Select Case When FSAdress is null then 1 else FSAdress end as f from EcoleSetting") != 1)
                    {
                        result = false;
                    }
                }
                else if (binding == "Register")
                {
                    if (Connexion.GetInt("Select Case When FSRegister is null then 1 else FSRegister end as f from EcoleSetting") != 1)
                    {
                        result = false;
                    }
                }
                else if (binding == "Note")
                {
                    if (Connexion.GetInt("Select Case When FSNote is null then 1 else FSNote end as f from EcoleSetting") != 1)
                    {
                        result = false;
                    }
                }
                else if (binding == "Insc")
                {
                    if (Connexion.GetInt("Select Case When FSInsc is null then 1 else FSInsc end as f from EcoleSetting") != 1)
                    {
                        result = false;
                    }
                }

            }
            return result; 
        }

        public static void SetVisibileColumn(string name , string type , int visibility)
        {
            if (type == "Student")
            {
                if (name == "Sort")
                {
                    Connexion.Insert("Update EcoleSetting Set FSID = " + visibility);
                }
                else if (name == "Name")
                {
                    Connexion.Insert("Update EcoleSetting Set FSName = " + visibility);
                    
                }
                else if (name == "PhoneNumber")
                {
                    Connexion.Insert("Update EcoleSetting Set FSPhone = " + visibility);

                }
                else if (name == "ParentNumber")
                {
                    Connexion.Insert("Update EcoleSetting Set FSParentNumber = " + visibility);
                  
                }
                else if (name == "Genderr")
                {
                    Connexion.Insert("Update EcoleSetting Set FSGender = " + visibility);
                  
                }
                else if (name == "Birthdate")
                {
                    Connexion.Insert("Update EcoleSetting Set FSBirthDate = " + visibility);
                   
                }
                else if (name == "BarCode")
                {
                    Connexion.Insert("Update EcoleSetting Set FSBarCode = " + visibility);
                }
                else if (name == "Level")
                {
                    Connexion.Insert("Update EcoleSetting Set FSLevel = " + visibility);
                   
                }
                else if (name == "Year")
                {
                    Connexion.Insert("Update EcoleSetting Set FSYear = " + visibility);
                  
                }
                else if (name == "Spec")
                {
                    Connexion.Insert("Update EcoleSetting Set FSSpeciality = " + visibility);
                 
                }
                else if (name == "Adress")
                {
                    Connexion.Insert("Update EcoleSetting Set FSAdress = " + visibility);
                  
                }
                else if (name == "Register")
                {
                    Connexion.Insert("Update EcoleSetting Set FSRegister = " + visibility);
                 
                }
                else if (name == "Note")
                {
                    Connexion.Insert("Update EcoleSetting Set FSNote = " + visibility);
                }
                else if (name == "Insc")
                {
                    Connexion.Insert("Update EcoleSetting Set FSInsc = " + visibility);
                }
            }
        }

        public static bool CheckConditionColumn(string type , string binding)
        {
            bool result = false; 
            if (type == "Student")
            {
                if (binding == "Sort")
                {
                    result = true; 
                }
                if (binding == "BarCode")
                {
                    result = true;
                }
                else if (binding == "Name")
                {
                    result = true;
                }
                else if (binding == "PhoneNumber")
                {
                    result = true;
                }
                else if (binding == "ParentNumber")
                {
                    result = true;
                }
                else if (binding == "Genderr")
                {
                    result = true;
                }
                else if (binding == "Birthdate")
                {
                    result = true;
                }
                else if (binding == "Level")
                {
                    result = true;
                }
                else if (binding == "Year")
                {
                    result = true;
                }
                else if (binding == "Spec")
                {
                    result = true;
                }
                else if (binding == "Adress")
                {
                    result = true;
                }
                else if (binding == "Subjects")
                {
                    result = true;
                }
                else if (binding == "Register")
                {
                    result = true;
                }
                else if (binding == "Note")
                {
                    result = true;
                }
                else if (binding == "Insc")
                {
                    result = true;
                }
            }
            return result; 
        }
        public static void FillDTAttendanceGroup(string GID, ref DataTable dt, ref DataGrid DG, ref DataTable dtTextcolumn, ref DataTable DtMonthes, ResourceDictionary resource)
        {
            DataTable dtAttendanceDates = new DataTable();
            int count = 1;
            Connexion.FillDT(ref dtAttendanceDates, "Select '(' + cast(attendance.Session AS varchar(50)) + ')Z' + Attendance.Date as Date  , Attendance.ID as ID from Attendance Where GroupID = " + GID + " ORDER BY convert(datetime, Attendance.Date, 103)");
            string A = "A";
            string P = "P";
            string J = "J";
            if (Connexion.Language() == 1)
            {
                A = "غ";
                P = "ح";
                J = "م";
            }
            string query = "Select * From (Select Students.FirstName + ' ' + Students.LastName as Name,Students.LastName + ' ' + Students.FirstName as RName, Students.BarCode as Barcode,Students.PhoneNumber as PhoneNumber , Students.ParentNumber as ParentNumber ," +
                 "Students.Gender as Gender," +
                 "Students.ID as ID , " +
                 "Case when Attendance_Student.Status = 0 Then N'" + A + "' " +
                 "When Attendance_Student.Status = 1 Then N'" + P + "' " +
                 "When Attendance_Student.Status = 2 Then G.GroupName " +
                 "When Attendance_Student.Status = 3 Then N'" + J + "' " +
                 " End as stat," +
                 "'(' + cast(attendance.Session AS varchar(50)) + ')Z' + Attendance.Date " +
                 " as date from Attendance " +
                 "left join Attendance_Student on Attendance_Student.ID = Attendance.ID  " +
                 "Join Students on Students.ID = Attendance_Student.StudentID " +
                 "left join Attendance_Change " +
                 "on ( Attendance_Change.FromGroupID = Attendance.GroupID  and Attendance.Session = Attendance_Change.Session) " +
                 "left join Groups as G on Attendance_Change.ToGroupID = G.GroupID " +
                 "Join Groups on Groups.GroupID = Attendance.GroupID  " +
                 "Join Class_Student on (Class_Student.StudentID = Students.ID and Class_Student.GroupID = Groups.GroupID) " +
                 "where Attendance.GroupID = " + GID + "  ) as T  " +
                 "Pivot(max(stat) FOR Date IN (";

            foreach (DataRow row in dtAttendanceDates.Rows)
            {
                Methods.AddcolumnDGAttendance(ref DG, row["Date"].ToString(), count, row["ID"].ToString(), ref dtTextcolumn, ref DtMonthes);
                if (count == 1)
                {
                    query += "[" + row["Date"].ToString() + "]";
                }
                else
                {
                    query += ",[" + row["Date"].ToString() + "]";
                }
                count++;
            }
            query += ")) as F order by f.ID asc";
            Connexion.FillDT(ref dt, query);

            DataRow newRow = dt.NewRow();
            int sum = 0;
            foreach (DataColumn column in dt.Columns)
            {
                if (column.ColumnName == "Name")
                {
                    newRow[column.ColumnName] = resource["ExtraStudents"].ToString();
                }
                else if (column.ColumnName == "ID")
                {
                    newRow[column.ColumnName] = "-1";
                }
                else
                {
                    string pattern = @"\((.*?)\)";

                    // Create a Regex object and try to match the pattern
                    Regex regex = new Regex(pattern);

                    // Find the match
                    Match match = regex.Match(column.ColumnName);

                    // If a match is found, return the captured group (text between parentheses)
                    if (match.Success)
                    {
                        string Ses =  match.Groups[1].Value;
                        string AID =Connexion.GetString("Select ID from Attendance Where Session = " + Ses + " and GroupID = " + GID);
                        string Total = Connexion.GetString("Select count(*) from Attendance_StudentsOneSes Where AID = " + AID);
                        newRow[column.ColumnName] = Total;
                        sum += Connexion.GetInt("Select case when Sum(TPrice) is null then 0 else Sum(tprice) end as f  from attendance_StudentsOneSes Where AID=" + AID);

                    }
                }

            }
            dt.Rows.Add(newRow);
        }
        public static void AddcolumnDGAttendance(ref DataGrid DG, string Header , int Num , string AID , ref DataTable dtTextColumn , ref DataTable dtMonthes)
        {
            DataGridTextColumn textColumn = new DataGridTextColumn();
            textColumn.Header = Header.Replace('Z', '\n'); ;
          
            textColumn.Binding = new Binding(Header);
            int indexZ = Header.IndexOf('Z');
            DateTime date;
            if (indexZ != -1 && indexZ + 1 < Header.Length)
            {
                // Extract the numeric part before 'Z'
                string numericPart = Header.Substring(1, indexZ - 1);

                // Extract the date part after 'Z'
                string datePart = Header.Substring(indexZ + 1);
                if (DateTime.TryParseExact(datePart, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out date))
                {
                    // Get the month
                    int month = date.Month;

                    // Get the month name
                    string monthName = date.ToString("MMM");

                    DataRow[] matchingRows = dtMonthes.Select($"MonthId = {month}");

                    if (matchingRows.Length > 0)
                    {
                        DataRow row = matchingRows[0];
                        row["Amount"] = Convert.ToInt32(row["Amount"]) + 1;
                    }
                    else
                    {
                        int monthIndex = DG.Columns.Count;
                        dtMonthes.Rows.Add(month, monthName, 1 , monthIndex);
                    }
                    textColumn.Header = date.Day.ToString("00");

                }
            }

            System.Windows.Style style = new System.Windows.Style();
            style.TargetType = typeof(DataGridCell);

            // Trigger for value "1"
            DataTrigger trigger1 = new DataTrigger();
            trigger1.Value = "1";
            trigger1.Binding = new Binding(Num.ToString());
            Setter setter1 = new Setter();
            setter1.Property = DataGridCell.BackgroundProperty;
            Color color1 = (Color)ColorConverter.ConvertFromString("#a6be85");
            Brush brush1 = new SolidColorBrush(color1);
            setter1.Value = brush1;
            trigger1.Setters.Add(setter1);

            // Trigger for value "0"
            DataTrigger trigger0 = new DataTrigger();
            trigger0.Value = "0";
            trigger0.Binding = new Binding(Num.ToString());
            Setter setter0 = new Setter();
            setter0.Property = DataGridCell.BackgroundProperty;
            Color color0 = (Color)ColorConverter.ConvertFromString("#d68b80");
            Brush brush0 = new SolidColorBrush(color0);
            setter0.Value = brush0;
            trigger0.Setters.Add(setter0);

            // Trigger for value "2"
            DataTrigger trigger2 = new DataTrigger();
            trigger2.Value = "2";
            trigger2.Binding = new Binding(Num.ToString());
            Setter setter2 = new Setter();
            setter2.Property = DataGridCell.BackgroundProperty;
            Color color2 = (Color)ColorConverter.ConvertFromString("#FFFF94"); // Light Yellow
            Brush brush2 = new SolidColorBrush(color2);
            setter2.Value = brush2;
            trigger2.Setters.Add(setter2);

            // Add triggers to the style
            style.Triggers.Add(trigger2); // Add trigger for value "2" first, so it takes precedence
            style.Triggers.Add(trigger1);
            style.Triggers.Add(trigger0);

            // Assign the style to the cell
            textColumn.CellStyle = style;

            // Add the column to the DataGrid
            textColumn.Width = 26;
            DG.Columns.Add(textColumn);

            // Add data to the DataTable
            dtTextColumn.Rows.Add(AID, Num);
        }
        public static void AddTime(ref DataGrid DG, string HF, string MF, string HT, string MT, string D)
        {
            string Start = Convert.ToString(HF) + ":" + Convert.ToString(MF);
            string End = Convert.ToString(HT) + ":" + Convert.ToString(MT);
            DG.Items.Add(new Class.WFTime { Day = D, TimeStart = Start, TimeEnd = End });

        }
        
        public static void insertPic(ref Image IM , string path )
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();
            
            IM.Source = bitmap;

        }

        public static void PrintBarcode(string query)
        {
            string path;
            if (File.Exists(@"C:\ProgramData\EcoleSetting\Mouathfile.txt"))
            {
                path = @"C:\Users\Home\Desktop\C# Projects\Gestion_De_Cours\FastReport";
            }
            else
            {
                path = @"C:\ProgramData\EcoleSetting\EcolePrint";
            }
            Report r = new Report();
            path += @"\BarCodeStudentsAllAR.frx";
            if (Connexion.Language() == 0) // eng
            {

                r.Load(path);
            }
            else // ar
            {
                r.Load(path);
            }

            DataTable dtStudents = new DataTable();
            dtStudents.Columns.Add("FirstName");
            dtStudents.Columns.Add("LastName");
            dtStudents.Columns.Add("BarCode");
            dtStudents.Columns.Add("Note");
            dtStudents.Columns.Add("Level");
            DataTable dt = new DataTable();
            Connexion.FillDT(ref dt, query);
            foreach (DataRow row in dt.Rows)
            {
                DataRow newRow = dtStudents.NewRow();
                newRow["FirstName"] = row["FirstName"].ToString();
                newRow["LastName"] = row["LastName"].ToString();
                newRow["BarCode"] = row["BarCode"].ToString();
                newRow["Note"] = row["Note"].ToString();
                newRow["Level"] = row["Year"].ToString() + " " + row["Spec"].ToString();
                dtStudents.Rows.Add(newRow);
            }
            dtStudents.TableName = "Students";
            DataSet ds = new DataSet();
            ds.Tables.Add(dtStudents);
            DataTable dtEcole = new DataTable();
            dtEcole.TableName = "Ecole";
            Connexion.FillDT(ref dtEcole, "Select * from EcoleSetting");
            ds.Tables.Add(dtEcole);

            r.RegisterData(ds);
            r.GetDataSource("Students").Enabled = true;
            r.GetDataSource("Ecole").Enabled = true;
            PictureObject picture = r.FindObject("Picture1") as PictureObject;

            if (picture != null)
            {
                // Set the new image path
                picture.Image = System.Drawing.Image.FromFile(@"C:\ProgramData\EcoleSetting\EcolePhotos\EcoleLogo.jpg");

                // Optionally, you can set other properties, e.g., image size, if needed
                // picture.Stretch = true; // Example property to stretch the image
            }
            r.Save(path);
            if (Commun.FastReportEdit != 1)
            {
                r.Design();
            }
            else
            {
                r.Show();
            }
        }

        public static void InsertPicwithGender(ref Image Pic ,string path , string gender)
        {
            if (System.IO.File.Exists(path))
            {
                string ShowPic = path;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(ShowPic);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                Pic.Source = bitmap;
            }
            else
            {
                if (gender == "1")
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\Women.png");
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    Pic.Source = bitmap;
                }
                else if (gender == "0")
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\man.png");
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    Pic.Source = bitmap;
                }
            }
        }

        public static void EnterText(KeyEventArgs e, ref TextBox TB)
        {
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                TB.Focus();
            }

        }
        public static void EnterText(KeyEventArgs e, ref TextBox TB , ref TextBox TBUp)
        {
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                TB.Focus();
            }
            else if (e.Key == Key.Up)
            {
                TBUp.Focus();
            }

        }

        public static void DeleteRow(ref DataGrid DG )
        {
            DG.Items.Remove(DG.SelectedItem);


        }
    }
}
