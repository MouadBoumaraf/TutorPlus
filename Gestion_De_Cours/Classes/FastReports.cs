using FastReport;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gestion_De_Cours.Classes
{
    class FastReports
    {
        public static ResourceDictionary ResourceDic = new ResourceDictionary();

        public static void setResourceDic(int i)
        {
            switch (i)
            {
                case 0:
                    ResourceDic.Source = new Uri("Dictionary\\EN.xaml", UriKind.Relative);
                    break;
                case 1:
                    ResourceDic.Source = new Uri("Dictionary\\AR.xaml", UriKind.Relative);
                    break;
                case 2:
                    ResourceDic.Source = new Uri("Dictionary\\FR.xaml", UriKind.Relative);
                    break;
                default:
                    ResourceDic.Source = new Uri("Dictionary\\EN.xaml", UriKind.Relative); // Fallback to English
                    break;
            }
        }

        public static void PrintHistoryStudent(string SID, DataRow[] filterd_result = null)
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
            if (Connexion.Language() != 1)
            {
                r.Load(path + @"\AttendanceHistoryStudentEN.frx");
            }
            else
            {
                r.Load(path+ @"\AttendanceHistoryStudentAR.frx");
            }
            DataSet ds = new DataSet();
            DataTable dtEcoleinfo = new DataTable();
            Connexion.FillDT(ref dtEcoleinfo, "Select * from EcoleSetting");
            dtEcoleinfo.Columns.Add("Logo");
            dtEcoleinfo.Rows[0]["Logo"] = Connexion.GetImagesFile() + @"\EcoleLogo.jpg";
            DataTable dtStudent = new DataTable();
            dtStudent.Columns.Add("name");
            dtStudent.Rows.Add();
            dtStudent.Rows[0][0] = Connexion.GetString("Select FirstName + ' ' + LastName from students Where ID = " + SID);
            DataTable dtInfo = new DataTable("Info");
            if (filterd_result != null)
            {
                dtInfo = filterd_result[0].Table.Clone();
                foreach (DataRow row in filterd_result)
                {
                    dtInfo.ImportRow(row);
                }
            }
            else
            {
                Connexion.FillDT(ref dtInfo, "Select 'Class' as Type,attendance.ID as AID , Class.ID as CID," +
                    "Class.CName , Groups.GroupName as GName,Attendance.TimeStart as Time , Attendance.Date , " +
                    "Case When Attendance_Student.Status = 1 then N'" +ResourceDic["Present"].ToString() + "' When Attendance_Student.status = 0 then N'" +
                    ResourceDic["Absent"].ToString() + "' When Attendance_Student.Status = 3 then N'" + ResourceDic["Justified"] + "' End as Status , Attendance_Student.Status as Stat ,  Attendance.Session from Attendance " +
                   "left join Attendance_Student on Attendance_Student.ID = Attendance.ID join Groups on Groups.GroupID = Attendance.GroupID " +
                   "join Class on groups.ClassID = Class.ID " +

                    " Where Attendance_Student.Status != 2 and  Attendance_Student.StudentID = " + SID + " " +
                    " Union Select 'Formation' as Type,Formation_Attendance.ID as AID ,Formation.ID as CID,Formation.Name as CName , ''as GName , Class_Time.TimeStart as Time , Formation_Attendance.Date , " + "Case When Formation_Attendance_Student.Status =1 then N'" + ResourceDic["Present"].ToString() + "'When Formation_Attendance_Student.Status = 0 then N'" + ResourceDic["Absent"].ToString() + "' end as Status ,Formation_Attendance_Student.Status as Stat ,'' as Session from Formation_Attendance Left Join Formation_Attendance_Student on Formation_Attendance_Student.AID = Formation_Attendance.ID Join Formation on Formation.ID = Formation_Attendance.FID Join Class_Time on Class_time.FID= Formation.ID  Where Class_time.type = 2 and Formation_Attendance_Student.SID =" + SID);
            }
            PictureObject picture1 = r.FindObject("EcoleLogo") as PictureObject;
            if (picture1 != null)
            {
                picture1.ImageLocation = dtEcoleinfo.Rows[0]["Logo"].ToString() ; 
            }
            dtInfo.TableName = "Info";
            dtEcoleinfo.TableName = "EcoleInfo";
            dtStudent.TableName = "Student";
            ds.Tables.Add(dtInfo);
            ds.Tables.Add(dtStudent);
            ds.Tables.Add(dtEcoleinfo);
            r.RegisterData(ds);
            r.GetDataSource("Info").Enabled = true;
            r.GetDataSource("Student").Enabled = true;
            r.GetDataSource("EcoleInfo").Enabled = true;
            if (Commun.FastReportEdit != 1)
            {
                r.Design();
            }
            else
            {
                r.Show();
            }
        }
        public static void PrintAttendance(string AID, string type = "1")
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
                    r.Load(path + @"\AttendanceFastReport.frx");
                }
                else
                {
                    r.Load(path + @"\AttendanceFastReportAR.frx");
                }
                DataSet ds = new DataSet();
                DataTable DataAttend = new DataTable();
                DataTable dtGroup = new DataTable();
                string querySession = "";
                if (type == "1")
                {
                    string GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + AID);
                    string CID = Connexion.GetClassID(GID).ToString();
                    int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                    if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                    {
                        int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                        monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                    }

                    if (monthlypayment == 1)
                    {
                        string dateAttendanceString = Connexion.GetString(AID, "Attendance", "Date");
                        DateTime dateAttendance = DateTime.Parse(dateAttendanceString);
                        int monthNumber = dateAttendance.Month;
                        int year = dateAttendance.Year;
                        querySession = "dbo.CalculateMonthlyPaymentRemaining(Students.ID ) ";
                    }
                    else
                    {
                        if (Connexion.GetInt("Select CalcPrice from EcoleSetting") == 1)
                        {
                            querySession = "dbo.CalcPriceSum(Students.ID,Groups.ClassID)";
                        }
                        else
                        {
                            querySession = "dbo.GettotalPayStudent(Students.ID , Groups.ClassID) - dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'Su')";
                        }
                    }
                    Connexion.FillDT(ref DataAttend,
                         "Select Students.FirstName + ' ' + Students.LastName as Name,Students.Note ,students.BarCode ," +
                       "       Students.Gender as Gender ," +
                       "        " + querySession + " as Sessions, "
                      + "       Students.ID as ID ," +
                       "     Case when Attendance_Student.Status = 0 Then N'" + ResourceDic["Absent"].ToString() + "' " +
                          "When Attendance_Student.Status = 1 Then N'" + ResourceDic["Present"].ToString() + "' " +
                         "When Attendance_Student.Status = 2 Then N'" + ResourceDic["GroupChange"].ToString() +"' ELSE '' End as status, " +
                       "       Case When Attendance_Student.Status = 0 THEN 1 " +
                       "       When Attendance_Student.Status = 1 THEN 0 END as StatusOP ," +
                       "       Case When Attendance_Student.Status = 2 Then 1 " +
                       "       When Attendance_Student.Status !=2 Then 0 END As StatusCH ," +
                       "       Case When Attendance_Student.Status = 3 Then 1 " +
                       "       When Attendance_Student.Status !=3  Then 0 END as StatusJustif " +
                       "from Attendance_Student" +
                       "       Join Attendance ON Attendance_Student.ID = Attendance.ID " +
                       "       Join Students ON Students.ID = Attendance_Student.StudentID " +
                       "       Join Class_Student ON Class_Student.StudentID = Students.ID " +
                       "Join Groups on Groups.GroupID = Class_student.GroupID " +
                       "Where Attendance.ID = " + AID + " And Class_Student.ClassID = " + CID);

                    Connexion.FillDT(ref dtGroup, "Select  " +
                      "Case When Class.MultipleGroups = 'Single' Then" +
                      " Class.CName " +
                      "When Class.MultipleGroups = 'Multiple' Then Class.CName + ' ' + Groups.GroupName " +
                      "End as GroupName," +
                      "Class.CSubject," +
                      "Groups.GroupGender as Gender," +
                      "Class.CYear," +
                      "Teacher.TFirstName + ' ' + Teacher.TLastName as TName ," +
                      "Teacher.TLastName + ' ' + Teacher.TFirstName as RTName ," +
                      "Years.Year as Year , " +
                      "Subjects.Subject as Subject , " +
                      "dbo.GetStudentsAmmount(Attendance.GroupID) as  SAmmount,  " +
                      "Groups.Sessions , " +
                      "Groups.TSessions ," +
                      "Levels.Level , " +
                      "Class.CLevel as LevelID  ," +
                      "Attendance.TimeStart as TimeStart , " +
                      "Attendance.TimeEnd as TimeEnd , " +
                      "Attendance.Date as Date  , " +
                      "Rooms.Room as Room " +
                      "From Groups  " +
                      "Join Class On Class.ID = Groups.ClassID " +
                      "Join Years on Class.CYear = Years.ID " +
                      "Join Subjects on Class.CSubject = Subjects.ID " +
                      "join Teacher on Teacher.ID = Class.TID " +
                      "Join Levels On Levels.ID = Class.CLevel " +
                      "join Attendance on Attendance.GroupID = Groups.GroupID  " +
                      "Join Rooms on Rooms.ID = Attendance.RoomID WHere  Attendance.ID = " + AID + " and Groups.GroupID =" + GID);

                }
                else
                {

                    string CID = Connexion.GetString("Select CID from Attendance_Extra where ID = " + AID);
                    Connexion.FillDT(ref DataAttend,
                        "Select Students.FirstName + ' ' + Students.LastName as Name,Students.Note ,students.BarCode ," +
                      "       Students.Gender as Gender ," +
                      "       Attendance_Extra_Students.Price as Sessions, "
                     + "       Students.ID as ID ," +
                      "     Case when Attendance_Extra_Students.Status = 0 Then N'" + ResourceDic["Absent"].ToString() + "' " +
                         "When Attendance_Extra_Students.Status = 1 Then N'" + ResourceDic["Present"].ToString() + "' " +
                       " ELSE '' End as status, " +
                      "       Case When Attendance_Extra_Students.Status = 0 THEN 1 " +
                      "       When Attendance_Extra_Students.Status = 1 THEN 0 END as StatusOP ," +
                      "       Case When Attendance_Extra_Students.Status = 2 Then 1 " +
                      "       When Attendance_Extra_Students.Status !=2 Then 0 END As StatusCH ," +
                      "       Case When Attendance_Extra_Students.Status = 3 Then 1 " +
                      "       When Attendance_Extra_Students.Status !=3  Then 0 END as StatusJustif " +
                      "from Attendance_Extra_Students" +
                      "       Join Students ON Students.ID = Attendance_Extra_Students.SID " +
                      "Where Attendance_Extra_Students.ID = " + AID);
                    Connexion.FillDT(ref dtGroup, "Select  " +
                      "" +
                      " Class.CName  as GroupName," +
                      "Class.CSubject," +

                      "Class.CYear," +
                      "Teacher.TFirstName + ' ' + Teacher.TLastName as TName ," +
                      "Teacher.TLastName + ' ' + Teacher.TFirstName as RTName ," +
                      "Years.Year as Year , " +
                      "Subjects.Subject as Subject , " +
                      "Levels.Level , " +
                      "Class.CLevel as LevelID  ," +
                      "Attendance_Extra.TimeStart as TimeStart , " +
                      "Attendance_Extra.TimeEnd as TimeENd , " +
                      "Attendance_Extra.Date as Date  ," +
                      "Rooms.Room as Room " +
                      "From Class  " +
                      "Join Years on Class.CYear = Years.ID " +
                      "Join Subjects on Class.CSubject = Subjects.ID " +
                      "join Teacher on Teacher.ID = Class.TID " +
                      "Join Levels On Levels.ID = Class.CLevel" +
                      " join Attendance_Extra on Attendance_Extra.CID = Class.ID   " +
                      "Join Rooms on Rooms.ID = Attendance_Extra.RoomID  WHere Attendance_Extra.ID = " + AID + " and Class.ID =" + CID);

                }
                DataAttend.TableName = "DataAttend";
                ds.Tables.Add(DataAttend);
                DataTable DataEcole = new DataTable();
                DataEcole.TableName = "DataEcole";
                Connexion.FillDT(ref DataEcole, "Select NameFR,NameAR,N'" + Connexion.GetImagesFile() + "\\EcoleLogo.jpg'  as Logo , Number ,Number2,Adress from EcoleSetting");
                ds.Tables.Add(DataEcole);

                dtGroup.TableName = "DataGroup";
                ds.Tables.Add(dtGroup);
                r.RegisterData(ds);
                r.GetDataSource("DataAttend").Enabled = true;
                r.GetDataSource("DataEcole").Enabled = true;
                r.GetDataSource("DataGroup").Enabled = true;
                if (Commun.FastReportEdit != 1)
                {
                    r.Design();
                }
                else
                {
                    r.Show();
                }
            }
            catch (Exception e)
            {
                Methods.ExceptionHandle(e);
            }
        }

        public static void PrintMethod1(DataTable dt, string TID, string date)
        {
            Report r = new Report();
            string pathmouath = @"C:\ProgramData\EcoleSetting\Mouathfile.txt";
            if (File.Exists(pathmouath))
            {
                pathmouath = @"C:\Users\Home\Desktop\C# Projects\Gestion_De_Cours\FastReport";
            }
            else
            {
                pathmouath = @"C:\ProgramData\EcoleSetting\EcolePrint";
            }


            int count = 0;
            DataTable DtGroups1 = new DataTable();
            DtGroups1.Columns.Add("GroupName");
            DtGroups1.Columns.Add("Total");
            DtGroups1.Columns.Add("StartDate");
            DtGroups1.Columns.Add("EndDate");
            DataTable DtGroups2 = new DataTable();
            DtGroups2.Columns.Add("GroupName");
            DtGroups2.Columns.Add("Total");
            DtGroups2.Columns.Add("StartDate");
            DtGroups2.Columns.Add("EndDate");
            DataTable DtGroups3 = new DataTable();
            DtGroups3.Columns.Add("GroupName");
            DtGroups3.Columns.Add("Total");
            DtGroups3.Columns.Add("StartDate");
            DtGroups3.Columns.Add("EndDate");
            DataTable DtGroups4 = new DataTable();
            DtGroups4.Columns.Add("GroupName");
            DtGroups4.Columns.Add("Total");
            DtGroups4.Columns.Add("StartDate");
            DtGroups4.Columns.Add("EndDate");
            DataTable dtGeneral = new DataTable();
            dtGeneral.Columns.Add("TName");

            dtGeneral.Columns.Add("Date");
            DataTable DtStudents1 = new DataTable();
            DataTable DtStudents2 = new DataTable();
            DataTable DtStudents3 = new DataTable();
            DataTable DtStudents4 = new DataTable();
            DataSet ds;
            if (Connexion.Language() == 0)//en
            {
                r.Load(pathmouath + @"\TeacherPaymentPerStudentModule2EN.frx");
            }
            else if (Connexion.Language() == 1)
            {
                r.Load(pathmouath + @"\TeacherPaymentPerStudentModule2AR.frx");

            }
            else if (Connexion.Language() == 2)
            {
                r.Load(pathmouath + @"\TeacherPaymentPerStudentModule2EN.frx");
            }
            foreach (DataRow row in dt.Rows)
            { 
                int ses = int.Parse(row["Sessions"].ToString());
                if (ses == 0)
                {
                    continue;
                }
                int TotalPaidSes = int.Parse(row["Sessions"].ToString());
                int StartSes = Connexion.GetInt("Select Sessions - TSessions from Groups Where GroupID = " + row["GID"].ToString());
                int EndSes = StartSes + TotalPaidSes;
                if (count % 4 == 0)
                {
                    dtGeneral.Rows.Add(new object[] { Connexion.GetString("Select TLastName + ' ' + TFirstName from Teacher where ID = " + TID), date });
                    dtGeneral.TableName = "DataGeneral";
                    DtGroups1.Rows.Add(new Object[] { row["GroupName"].ToString(), row["TotalP"].ToString(), row["FromSesDate"].ToString(), row["ToSesDate"].ToString() });

                    Methods.FillDGSesStuTPayment(StartSes, EndSes, row["GID"].ToString(), ref DtStudents1);
                }
                else if (count % 4 == 1)
                {
                    DtGroups2.Rows.Add(new Object[] { row["GroupName"].ToString(), row["TotalP"].ToString(), row["FromSesDate"].ToString(), row["ToSesDate"].ToString() });
                    Methods.FillDGSesStuTPayment(StartSes, EndSes, row["GID"].ToString(), ref DtStudents2);
                }
                else if (count % 4 == 2)
                {
                    DtGroups3.Rows.Add(new Object[] { row["GroupName"].ToString(), row["TotalP"].ToString(), row["FromSesDate"].ToString(), row["ToSesDate"].ToString() });
                    Methods.FillDGSesStuTPayment(StartSes, EndSes, row["GID"].ToString(), ref DtStudents3);
                }
                else if (count % 4 == 3)
                {
                    DtGroups4.Rows.Add(new Object[] { row["GroupName"].ToString(), row["TotalP"].ToString(), row["FromSesDate"].ToString(), row["ToSesDate"].ToString() });
                    Methods.FillDGSesStuTPayment(StartSes, EndSes, row["GID"].ToString(), ref DtStudents4);
                }
                count++;
                if (count % 4 == 0)
                {
                    SubreportObject Sub4loop = r.FindObject("Sub4") as SubreportObject;
                    Sub4loop.Visible = true;
                    SubreportObject Sub3loop = r.FindObject("Sub3") as SubreportObject;
                    Sub3loop.Visible = true;
                    SubreportObject Sub2loop = r.FindObject("Sub2") as SubreportObject;
                    Sub2loop.Visible = true;
                    SubreportObject Sub1loop = r.FindObject("Sub1") as SubreportObject;
                    Sub1loop.Visible = true;
                    ds = new DataSet();
                    ds.Tables.Add(dtGeneral.Copy());
                    DtGroups4.TableName = "GroupInfo4";
                    DtGroups3.TableName = "GroupInfo3";
                    DtGroups2.TableName = "GroupInfo2";
                    DtGroups1.TableName = "GroupInfo1";
                    ds.Tables.Add(DtGroups1.Copy());
                    ds.Tables.Add(DtGroups2.Copy());
                    ds.Tables.Add(DtGroups3.Copy());
                    ds.Tables.Add(DtGroups4.Copy());
                    DtStudents1.TableName = "DataStudents1";
                    DtStudents2.TableName = "DataStudents2";
                    DtStudents3.TableName = "DataStudents3";
                    DtStudents4.TableName = "DataStudents4";
                    ds.Tables.Add(DtStudents1.Copy());
                    ds.Tables.Add(DtStudents2.Copy());
                    ds.Tables.Add(DtStudents3.Copy());
                    ds.Tables.Add(DtStudents4.Copy());
                    r.RegisterData(ds);
                    r.GetDataSource("GroupInfo1").Enabled = true;
                    r.GetDataSource("GroupInfo2").Enabled = true;
                    r.GetDataSource("GroupInfo3").Enabled = true;
                    r.GetDataSource("GroupInfo4").Enabled = true;
                    r.GetDataSource("DataStudents1").Enabled = true;
                    r.GetDataSource("DataStudents2").Enabled = true;
                    r.GetDataSource("DataStudents3").Enabled = true;
                    r.GetDataSource("DataStudents4").Enabled = true;
                    r.GetDataSource("DataGeneral").Enabled = true;
                    r.Prepare(true);

                }
            }
            SubreportObject Sub4 = r.FindObject("Sub4") as SubreportObject;
            Sub4.Visible = true;
            SubreportObject Sub3 = r.FindObject("Sub3") as SubreportObject;
            Sub3.Visible = true;
            SubreportObject Sub2 = r.FindObject("Sub2") as SubreportObject;
            Sub2.Visible = true;
            SubreportObject Sub1 = r.FindObject("Sub1") as SubreportObject;
            Sub1.Visible = true;
            
            if (count %4== 3)
            {
                Sub4.Visible = false;
                foreach (var obj in Sub4.AllObjects)
                {
                    if (obj is DataBand dataBand)
                    {
                        dataBand.DataSource = null;
                    }
                }
                ds = new DataSet();
                ds.Tables.Add(dtGeneral.Copy());
                DtGroups3.TableName = "GroupInfo3";
                DtGroups2.TableName = "GroupInfo2";
                DtGroups1.TableName = "GroupInfo1";
                ds.Tables.Add(DtGroups1.Copy());
                ds.Tables.Add(DtGroups2.Copy());
                ds.Tables.Add(DtGroups3.Copy());
                DtStudents1.TableName = "DataStudents1";
                DtStudents2.TableName = "DataStudents2";
                DtStudents3.TableName = "DataStudents3";
                ds.Tables.Add(DtStudents1.Copy());
                ds.Tables.Add(DtStudents2.Copy());
                ds.Tables.Add(DtStudents3.Copy());
                r.RegisterData(ds);
                r.GetDataSource("GroupInfo1").Enabled = true;
                r.GetDataSource("GroupInfo2").Enabled = true;
                r.GetDataSource("GroupInfo3").Enabled = true;
                r.GetDataSource("DataStudents1").Enabled = true;
                r.GetDataSource("DataStudents2").Enabled = true;
                r.GetDataSource("DataStudents3").Enabled = true;
                r.GetDataSource("DataGeneral").Enabled = true;
                r.Prepare(true);
            }
            else if (count %4 == 2)
            {

                Sub4.Visible = false;
                foreach (var obj in Sub4.AllObjects)
                {
                    if (obj is DataBand dataBand)
                    {
                        dataBand.DataSource = null;
                    }
                }
                Sub3.Visible = false;
                foreach (var obj in Sub3.AllObjects)
                {
                    if (obj is DataBand dataBand)
                    {
                        dataBand.DataSource = null;
                    }
                }
                ds = new DataSet();
                ds.Tables.Add(dtGeneral.Copy());
                DtGroups2.TableName = "GroupInfo2";
                DtGroups1.TableName = "GroupInfo1";
                ds.Tables.Add(DtGroups1.Copy());
                ds.Tables.Add(DtGroups2.Copy());
                DtStudents1.TableName = "DataStudents1";
                DtStudents2.TableName = "DataStudents2";
                ds.Tables.Add(DtStudents1.Copy());
                ds.Tables.Add(DtStudents2.Copy());
                r.RegisterData(ds);
                r.GetDataSource("GroupInfo1").Enabled = true;
                r.GetDataSource("GroupInfo2").Enabled = true;
                r.GetDataSource("DataStudents1").Enabled = true;
                r.GetDataSource("DataStudents2").Enabled = true;
                r.GetDataSource("DataGeneral").Enabled = true;
                r.Prepare(true);
            }
            else if (count%4 == 1)
            {

                Sub4.Visible = false;
                foreach (var obj in Sub4.AllObjects)
                {
                    if (obj is DataBand dataBand)
                    {
                        dataBand.DataSource = null;
                    }
                }

                Sub3.Visible = false;
                foreach (var obj in Sub3.AllObjects)
                {
                    if (obj is DataBand dataBand)
                    {
                        dataBand.DataSource = null;
                    }
                }

                Sub2.Visible = false;
                foreach (var obj in Sub2.AllObjects)
                {
                    if (obj is DataBand dataBand)
                    {
                        dataBand.DataSource = null;
                    }
                }
                ds = new DataSet();
                ds.Tables.Add(dtGeneral.Copy());
                DtGroups1.TableName = "GroupInfo1";
                ds.Tables.Add(DtGroups1.Copy());
                DtStudents1.TableName = "DataStudents1";
                ds.Tables.Add(DtStudents1);
                r.RegisterData(ds);
                r.GetDataSource("GroupInfo1").Enabled = true;
                r.GetDataSource("DataStudents1").Enabled = true;
                r.GetDataSource("DataGeneral").Enabled = true;
                r.Prepare(true);
            }
            if (Commun.FastReportEdit == 0)
            {
                r.Design();
            }
            else
            {
                if (count < 4)
                {
                    r.Show();
                }
                else
                {
                    r.ShowPrepared();
                }
            }

        }

        public static void PrintMethod1(DataRowView row, string TID, string date)
        {
            Report r = new Report();
            string pathmouath = @"C:\ProgramData\EcoleSetting\Mouathfile.txt";
            if (File.Exists(pathmouath))
            {
                pathmouath = @"C:\Users\Home\Desktop\C# Projects\Gestion_De_Cours\FastReport";
            }
            else
            {
                pathmouath = @"C:\ProgramData\EcoleSetting\EcolePrint";
            }


            int count = 0;
            DataTable DtGroups1 = new DataTable();
            DtGroups1.Columns.Add("GroupName");
            DtGroups1.Columns.Add("Total");
            DtGroups1.Columns.Add("StartDate");
            DtGroups1.Columns.Add("EndDate");
            DataTable DtGroups2 = new DataTable();
            DtGroups2.Columns.Add("GroupName");
            DtGroups2.Columns.Add("Total");
            DtGroups2.Columns.Add("StartDate");
            DtGroups2.Columns.Add("EndDate");
            DataTable DtGroups3 = new DataTable();
            DtGroups3.Columns.Add("GroupName");
            DtGroups3.Columns.Add("Total");
            DtGroups3.Columns.Add("StartDate");
            DtGroups3.Columns.Add("EndDate");
            DataTable DtGroups4 = new DataTable();
            DtGroups4.Columns.Add("GroupName");
            DtGroups4.Columns.Add("Total");
            DtGroups4.Columns.Add("StartDate");
            DtGroups4.Columns.Add("EndDate");
            DataTable dtGeneral = new DataTable();
            dtGeneral.Columns.Add("TName");

            dtGeneral.Columns.Add("Date");
            DataTable DtStudents1 = new DataTable();
            DataTable DtStudents2 = new DataTable();
            DataTable DtStudents3 = new DataTable();
            DataTable DtStudents4 = new DataTable();
            DataSet ds;
            dtGeneral.Rows.Add(new object[] { Connexion.GetString("Select TLastName + ' ' + TFirstName from Teacher where ID = " + TID), date });
            dtGeneral.TableName = "DataGeneral";


            if (Connexion.Language() == 0)//en
            {
                r.Load(pathmouath + @"\TeacherPaymentPerStudentModule2EN.frx");
            }
            else if (Connexion.Language() == 1)
            {
                r.Load(pathmouath + @"\TeacherPaymentPerStudentModule2AR.frx");

               
            }
            else if (Connexion.Language() == 2)
            {
                r.Load(pathmouath + @"\TeacherPaymentPerStudentModule2EN.frx");

            }

            int ses = int.Parse(row["Sessions"].ToString());
            if (ses == 0)
            {
                return;
            }
            int TotalPaidSes = int.Parse(row["Sessions"].ToString());
            int StartSes = Connexion.GetInt("Select Sessions - TSessions from Groups Where GroupID = " + row["GID"].ToString());
            int EndSes = StartSes + TotalPaidSes;

            DtGroups1.Rows.Add(new Object[] { row["GroupName"].ToString(), row["TotalP"].ToString(), row["FromSesDate"].ToString(), row["ToSesDate"].ToString() });

            Methods.FillDGSesStuTPayment(StartSes, EndSes, row["GID"].ToString(), ref DtStudents1);


            SubreportObject Sub4 = r.FindObject("Sub4") as SubreportObject;
            Sub4.Visible = false;
            foreach (var obj in Sub4.AllObjects)
            {
                if (obj is DataBand dataBand)
                {
                    dataBand.DataSource = null;
                }
            }
            SubreportObject Sub3 = r.FindObject("Sub3") as SubreportObject;
            Sub3.Visible = false;
            foreach (var obj in Sub3.AllObjects)
            {
                if (obj is DataBand dataBand)
                {
                    dataBand.DataSource = null;
                }
            }
            SubreportObject Sub2 = r.FindObject("Sub2") as SubreportObject;
            Sub2.Visible = false;
            foreach (var obj in Sub2.AllObjects)
            {
                if (obj is DataBand dataBand)
                {
                    dataBand.DataSource = null;
                }
            }
            ds = new DataSet();
            ds.Tables.Add(dtGeneral.Copy());
            DtGroups1.TableName = "GroupInfo1";
            ds.Tables.Add(DtGroups1.Copy());
            DtStudents1.TableName = "DataStudents1";
            ds.Tables.Add(DtStudents1);
            r.RegisterData(ds);
            r.GetDataSource("GroupInfo1").Enabled = true;
            r.GetDataSource("DataStudents1").Enabled = true;
            r.GetDataSource("DataGeneral").Enabled = true;

            if (Commun.FastReportEdit == 0)
            {
                r.Design();
            }
            else
            {

                r.Show();

            }

        }

        public static void PrintPaymentStudent(ref DataGrid DG , string SID, string Type)
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

                path += @"\PaymentStudentEN.frx";
                r.Load(path);
                TextObject TBName = r.FindObject("TBName") as TextObject;
                if (TBName != null)
                {
                    TBName.Text = "Student Name :";
                }
                TextObject TBYear = r.FindObject("TBYear") as TextObject;
                if (TBYear != null)
                {
                    TBYear.Text = "Year :";
                }
                TextObject TBMonthSubject = r.FindObject("TBMonthSubject") as TextObject;
                if (TBMonthSubject != null)
                {
                    if (Type == "ExtraStu")
                    {
                        TBMonthSubject.Text = "Subject";
                    }
                    else
                    {
                        int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                        if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                        {
                            int YID2 = Connexion.GetInt("Select Year from Students Where id = " + SID);
                            monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID2);
                        }
                        if (monthlypayment == 1)
                        {
                            TBMonthSubject.Text = "Month";
                        }
                        else
                        {
                            TBMonthSubject.Text = "Subject";
                        }
                    }
                }
                TextObject TBPrice = r.FindObject("TBPrice") as TextObject;
                if (TBPrice != null)
                {
                    TBPrice.Text = "Ammount";
                }
                TextObject TBStudentPayment = r.FindObject("TBStudentPayment") as TextObject;
                if (TBStudentPayment != null)
                {
                    TBStudentPayment.Text = "Student Payment";
                }
                TextObject TBDisClaimer = r.FindObject("TBDisClaimer") as TextObject;
                if (TBDisClaimer != null)
                {
                    TBDisClaimer.Text = @"Please keep this receipt safe. The school is not responsible for lost or misplaced receipts";
                }

            }
            else if (Connexion.Language() == 2)
            {
                path += @"\PaymentStudentEN.frx";
                r.Load(path);
                TextObject TBName = r.FindObject("TBName") as TextObject;
                if (TBName != null)
                {
                    TBName.Text = "Nom :";
                }
                TextObject TBYear = r.FindObject("TBYear") as TextObject;
                if (TBYear != null)
                {
                    TBYear.Text = "Année :";
                }
                TextObject TBMonthSubject = r.FindObject("TBMonthSubject") as TextObject;
                if (TBMonthSubject != null)
                {
                    if(Type == "ExtraStu")
                    {
                        TBMonthSubject.Text = "Matière";
                    }
                    else
                    {
                        int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                        if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                        {
                            int YID2 = Connexion.GetInt("Select Year from Students Where id = " + SID);
                            monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID2);
                        }
                        if (monthlypayment == 1)
                        {
                            TBMonthSubject.Text = "Mois";
                        }
                        else
                        {
                            TBMonthSubject.Text = "Matière";
                        }
                    }
                }
                TextObject TBPrice = r.FindObject("TBPrice") as TextObject;
                if (TBPrice != null)
                {
                    TBPrice.Text = "Montant";
                }
                TextObject TBStudentPayment = r.FindObject("TBStudentPayment") as TextObject;
                if (TBStudentPayment != null)
                {
                    TBStudentPayment.Text = "Paiement de l'étudiant";
                }
                TextObject TBDisClaimer = r.FindObject("TBDisClaimer") as TextObject;
                if (TBDisClaimer != null)
                {
                    TBDisClaimer.Text = @"Veuillez conserver ce reçu en lieu sûr. L’école n’est pas responsable des reçus perdus ou égarés.";
                }

            }
            else
            {
                path += @"\PaymentStudentAR.frx";
                r.Load(path);
                TextObject TBMonthSubject = r.FindObject("TBMonthSubject") as TextObject;
                if (TBMonthSubject != null)
                {
                    if (Type == "ExtraStu")
                    {
                        TBMonthSubject.Text = "المادة";
                    }
                    else
                    {
                        int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                        if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                        {
                            int YID2 = Connexion.GetInt("Select Year from Students Where id = " + SID);
                            monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID2);
                        }
                        if (monthlypayment == 1)
                        {
                            TBMonthSubject.Text = "الشهر";
                        }
                        else
                        {
                            TBMonthSubject.Text = "المادة";
                        }
                    }
                }
            }
            PictureObject picture = r.FindObject("Picture2") as PictureObject;

            if (picture != null)
            {
                // Set the new image path
                picture.Image = System.Drawing.Image.FromFile(@"C:\ProgramData\EcoleSetting\EcolePhotos\EcoleLogo.jpg");

                // Optionally, you can set other properties, e.g., image size, if needed
                // picture.Stretch = true; // Example property to stretch the image
            }
            r.Save(path);
            if (DG.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please Select a row(s) to be deleted ");
                return;
            }
            DataSet ds = new DataSet();
            DataTable dtEcoleinfo = new DataTable();
            dtEcoleinfo.TableName = "EcoleInfo";
            Connexion.FillDT(ref dtEcoleinfo, "Select * from EcoleSetting");
            dtEcoleinfo.Columns.Add("EcoleLogo");
            dtEcoleinfo.Rows[0]["EcoleLogo"] = Connexion.GetImagesFile() + @"\EcoleLogo.jpg";
            DataTable dtPayment = new DataTable();
            string monthlypayment3 = dtEcoleinfo.Rows[0]["PaymentMonth"].ToString();
            
            if(Type == "ExtraStu")
            {

                dtPayment.Columns.Add("Name");
                dtPayment.Columns.Add("Ammount");
                dtPayment.Columns.Add("CName");
                dtPayment.Columns.Add("Date");
                dtPayment.Columns.Add("Note");
                dtPayment.TableName = "Payment";
                TextObject TBDataMonthSub = r.FindObject("TBDataMonthSub") as TextObject;
                TBDataMonthSub.Text = "[Payment.CName]";
            }
            else
            {
                if (dtEcoleinfo.Rows[0]["PaymentMonth"].ToString() == "2")
                {
                    int YID3 = Connexion.GetInt("Select Year from Students Where id = " + SID);
                    monthlypayment3 = Connexion.GetString("Select Monthly from years where id = " + YID3);
                }
                if (monthlypayment3 == "1")
                {
                    dtPayment.Columns.Add("Name");
                    dtPayment.Columns.Add("Ammount");
                    dtPayment.Columns.Add("Month");
                    dtPayment.Columns.Add("Date");
                    dtPayment.Columns.Add("Note");

                    dtPayment.TableName = "Payment";
                }
                else
                {
                    dtPayment.Columns.Add("Name");
                    dtPayment.Columns.Add("Ammount");
                    dtPayment.Columns.Add("CName");
                    dtPayment.Columns.Add("Date");
                    dtPayment.Columns.Add("Note");
                    dtPayment.TableName = "Payment";
                }
                TextObject TBDataMonthSub = r.FindObject("TBDataMonthSub") as TextObject;
                if (TBDataMonthSub != null)
                {

                    if (monthlypayment3 == "1")
                    {
                        TBDataMonthSub.Text = "[Payment.Month]";
                    }
                    else
                    {
                        TBDataMonthSub.Text = "[Payment.CName]";
                    }
                }
            }
            
            DataTable dtStudentInfo = new DataTable();
            dtStudentInfo.Columns.Add("Name");
            dtStudentInfo.Columns.Add("TotalPrice");
            dtStudentInfo.Columns.Add("Year");
            dtStudentInfo.Columns.Add("BarCode");
            ds.Tables.Add(dtEcoleinfo);
            dtStudentInfo.Rows.Add();
            dtStudentInfo.TableName = "StudentInfo";
            int TotalPrice = 0;
          
            for(int i = 0; i < DG.SelectedItems.Count; i++)
            {
                DataRowView row = (DataRowView)DG.SelectedItems[i];
                dtPayment.Rows.Add();
                dtPayment.Rows[i][0] = row["Name"].ToString();
                if (i == 0)
                {
                    if(Type == "ExtraStu")
                    {
                        string AID = Connexion.GetString("Select AID from Attendance_StudentsOneSes where ID = " + row["ID"].ToString());
                        string GID = Connexion.GetString("Select GroupID from Attendance where ID = " + AID);
                        int CID = Connexion.GetClassID(GID);
                        dtStudentInfo.Rows[0][0] = row["Name"].ToString();
                        dtStudentInfo.Rows[0][2] = Connexion.GetString("Select Years.Year from Class join Years on Class.CYear = Years.ID where Class.ID = " + CID);
                    }
                    else
                    {
                        dtStudentInfo.Rows[0][0] = row["Name"].ToString();
                        dtStudentInfo.Rows[0][2] = Connexion.GetString("Select Levels.Level from Students join Levels on Students.Level = Levels.ID where Students.ID = " + row["SID"].ToString());
                        dtStudentInfo.Rows[0][3] = Connexion.GetString("Select Barcode from Students  where Students.ID = " + row["SID"].ToString());
                    }
                }
                dtPayment.Rows[i][1] = row["Price"].ToString();
                TotalPrice += int.Parse(row["Price"].ToString());

                if (Type == "ExtraStu")
                {
                    string AID = Connexion.GetString("Select AID from Attendance_StudentsOneSes where ID = " + row["ID"].ToString());
                    string GID = Connexion.GetString("Select GroupID from Attendance where ID = " + AID);
                    dtPayment.Rows[i][2] = Connexion.GetString("Select Case When MultipleGroups = 'Multiple' then Class.CName + ' ' + Groups.GroupName else Class.CName end as f from Class Join Groups on Groups.ClassID = Class.ID where Groups.GroupID = " + GID);
                    dtPayment.Rows[i][3] = Connexion.GetString("Select Date from Attendance Where ID = " + AID);
                }
                else
                {
                    if (monthlypayment3 == "1")
                    {
                        dtPayment.Rows[i][2] = row["MonthName"].ToString();
                    }
                    else
                    {
                        dtPayment.Rows[i][2] = row["CName"].ToString();
                    }
                    dtPayment.Rows[i][3] = row["Date"].ToString();
                    dtPayment.Rows[i][4] = row["Note"].ToString();
                }
                dtPayment.TableName = "Payment";
            }
            dtStudentInfo.Rows[0][1] = TotalPrice;
            ds.Tables.Add(dtStudentInfo);
            ds.Tables.Add(dtPayment);
            r.RegisterData(ds);
            r.GetDataSource("Payment").Enabled = true;
            r.GetDataSource("EcoleInfo").Enabled = true;
            r.GetDataSource("StudentInfo").Enabled = true;
            if (Commun.FastReportEdit == 0)
            {
                r.Design();
            }
            else
            {
                r.Show();
            }
        }
    }
}
