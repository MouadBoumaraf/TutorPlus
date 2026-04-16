using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Gestion_De_Cours.Classes;
using DevExpress.Xpf.Scheduler;
using Gestion_De_Cours.Panels;
using DevExpress.Xpf.Scheduling;
using DevExpress.Xpf.Core;
using DevExpress.XtraScheduler;
using System.Globalization;

using DevExpress.Mvvm;

namespace Gestion_De_Cours.UControl
{
    /// <summary>
    /// Interaction logic for Schedulerr.xaml
    /// </summary>
    public partial class Schedulerr : UserControl
    {
        public virtual ObservableCollection<Class.Room> Rooms { get; set; }
        public virtual ObservableCollection<Class.SessionsStatus> Statuses { get; set; }
        public virtual ObservableCollection<Class.YearColor> YearColor { get; set; }
        public static ObservableCollection<Appointment> Appointments { get; set; }

        DataTable dtReaccurent;
        public static string[] AppointmentTypes = { "Primaire", "Cem", "Lycee", "FAQ" };
        public static Color[] AppointmentColorTypes = {Color.FromRgb(255, 183, 185),  Color.FromRgb(171, 196, 255),
    Color.FromRgb(184, 242, 185) , Color.FromRgb(236, 88, 0)};

        public static string[] PaymentStates = { "Active", "Removed" };
        public static Brush[] PaymentBrushStates =
    {
    new LinearGradientBrush(
        new GradientStopCollection
        {
            new GradientStop(Colors.Green, 0.0),
            new GradientStop(Colors.Green, 0.9),  // 90% green
            new GradientStop(Colors.LightGreen, 1.0) // Slight gradient at the end
        },
        0.0), // Angle of gradient

    new LinearGradientBrush(
        new GradientStopCollection
        {
            new GradientStop(Colors.Red, 0.0),
            new GradientStop(Colors.Red, 0.9),  // 90% red
            new GradientStop(Colors.Pink, 1.0) // Slight gradient at the end
        },
        0.0) // Angle of gradient
};

        private void CreateRooms()
        {
            Rooms = new ObservableCollection<Class.Room>();
            DataTable dtrooms = new DataTable();
            Connexion.FillDT(ref dtrooms, "Select * from Rooms ");
            foreach (DataRow dr in dtrooms.Rows)
            {
                Rooms.Add(Class.Room.Create(Id: int.Parse(dr["ID"].ToString()), Name: dr["Room"].ToString()));
            }
        }

        ObservableCollection<Class.YearColor> CreateLabels()
        {
            ObservableCollection<Class.YearColor> result = new ObservableCollection<Class.YearColor>();
            DataTable dtLevels = new DataTable();
            Connexion.FillDT(ref dtLevels, "Select * from Levels ");
            int count = AppointmentTypes.Length;
            int i = 0;
            foreach (DataRow dr in dtLevels.Rows)
            {
                Class.YearColor label = Class.YearColor.Create();
                label.Id = int.Parse(dr["ID"].ToString());
                label.Color = AppointmentColorTypes[i];
                label.Caption = dr["Level"].ToString();
                result.Add(label);
                i++;
            }
            return result;
        }
        ObservableCollection<Class.SessionsStatus> CreateStatuses()
        {
            ObservableCollection<Class.SessionsStatus> result = new ObservableCollection<Class.SessionsStatus>();
            int count = PaymentStates.Length;
            for (int i = 0; i < count; i++)
            {
                Class.SessionsStatus paymentState = Class.SessionsStatus.Create();
                paymentState.Id = i;
                paymentState.Brush = PaymentBrushStates[i];
                paymentState.Caption = PaymentStates[i];
                result.Add(paymentState);

            }
            return result;
        }

        public Schedulerr()
        {

            try
            {
                InitializeComponent();
                int lang = Connexion.Language();
            //    this.Loaded += Schedulerr_Loaded; // Attach the Loaded event handler
                Statuses = CreateStatuses();
                YearColor = CreateLabels();
                scheduler.Width = Commun.ScreenWIdth - 220;
                scheduler.Height = Commun.ScreenHeight - 40;
                DataContext = this;
                SetLang();
                if (lang == 1)
                {
                    this.FontFamily = new FontFamily("Segoe UI");
                    // this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
                }
                CreateRooms();
                Appointments = new ObservableCollection<Appointment>();
                DataTable dtAttendanceCreated = new DataTable();
                Connexion.FillDT(ref dtAttendanceCreated, "Select " +
                    "Attendance.GroupID as GID ," +
                    "Attendance.TimeStart as TimeStart," +
                    "Attendance.TimeEND as TimeEND , attendance.ID as AID ,  " +
                    "Attendance.RoomID as IDRoom ,Attendance.Date as Date ," +
                    "Class.CLevel as CLevel , " +
                    " DATEPART(WEEKDAY, CONVERT(DATE, Date, 105)) - 1 AS Day ," +
                    "Attendance.GroupID as GroupID , " +
                    "Groups.ClassID as CID  , " +
                    "Case When Class.MultipleGroups = 'Multiple' then class.CName + ' ' + Groups.GroupName " +
                    "When MultipleGroups = 'Single' then Class.CName End as Name  from Attendance " +
                    "Join Groups on Groups.GroupID = Attendance.GroupID  " +
                    "Join Class on Class.ID = Groups.ClassID  where date = '20-07-2025' "
                    );
                foreach (DataRow dr in dtAttendanceCreated.Rows)
                {
                    Appointments.Add(CreateOneTimeAppointment(dr));
                }
                DataTable dtProgrammed = new DataTable();
                Connexion.FillDT(ref dtProgrammed, "Select * , DATEPART(WEEKDAY, CONVERT(DATE, Date, 105)) - 1 AS Day from Attendance_Prog ");
                foreach (DataRow dr in dtProgrammed.Rows)
                {
                    CreateOneTimeAppointment(dr);
                }
                dtReaccurent = new DataTable();
                Connexion.FillDT(ref dtReaccurent, "Select" +
                 " GroupID as GID ," + "CLassID as CID  ," +
                 "Class_Time.TimeStart," +
                 "Class_Time.TimeEnd , " +
                 "Class_Time.IDRoom ," +
                 "Class_Time.ID as TimeID , " +
                 "Class.CLevel  , " +
                 "Class_Time.Day as Day , " +
                 "Groups.GroupID as GroupID , " +
                 "Case when MultipleGroups = 'Multiple' then Class.CName + ' ' + Groups.GroupName " +
                 "When MultipleGroups = 'Single' then Class.CName " +
                 "End as Name " +
                 "from Class_Time  " +
                 "join  Groups on Groups.GroupID = Class_Time.GID " +
                 "join Class on Class.ID = Groups.ClassID where gid = '98' ");

                foreach (DataRow dr in dtReaccurent.Rows)
                {

                    Appointment ap = CreateRecurringAppointment(dr);
                    List<DateTime> exceptionsdates = FindRecurringConflicts(ap);
                    foreach (DateTime exceptionDate in exceptionsdates)
                    {

                    }                            // Add the exception to the recurrence pattern 
                    Appointments.Add(ap);
                }
                
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        public List<DateTime> FindRecurringConflicts(Appointment recurringAppt)
        {
            List<DateTime> conflictingDates = new List<DateTime>();

            // Parse recurrence info
            RecurrenceInfo rInfo = new RecurrenceInfo();
            rInfo.FromXml(recurringAppt.RecurrenceInfo);

            // Determine recurrence end conditions
            int occurrenceCount = rInfo.Range == RecurrenceRange.OccurrenceCount
                ? rInfo.OccurrenceCount
                : int.MaxValue;

            DateTime recurrenceEnd = rInfo.Range == RecurrenceRange.EndByDate
                ? rInfo.End
                : DateTime.MaxValue;

            DateTime safeEndDate = recurringAppt.StartTime.AddYears(1); // Default to 1 year if no end

            // Generate occurrences and check for conflicts
            int count = 0;
            DateTime currentDate = recurringAppt.StartTime.Date;
            while (count < occurrenceCount &&
                   currentDate <= recurrenceEnd &&
                   currentDate <= safeEndDate)
            {
                // Use appointment's actual start/end times with current date
                DateTime occurrenceStart = currentDate.Add(recurringAppt.StartTime.TimeOfDay);
                DateTime occurrenceEnd = currentDate.Add(recurringAppt.EndTime.TimeOfDay);

                // Check for conflicts
                bool hasConflict = Appointments.Any(existing =>
                    // Exclude self for edits
                    existing.roomId == recurringAppt.roomId &&
                    existing.StartTime < occurrenceEnd &&
                    existing.EndTime > occurrenceStart
                );

                if (hasConflict)
                {
                    conflictingDates.Add(currentDate);
                }

                // Move to next week (consider recurrence interval)
                currentDate = currentDate.AddDays(7);
                count++;
            }
            return conflictingDates;

        }
        public static Appointment CreateOneTimeAppointment(DataRow row)
        {
            string dateStr = GetValue<string>(row, "date"); // Your date column name
            string startTimeStr = GetValue<string>(row, "TimeStart");
            string endTimeStr = GetValue<string>(row, "TimeEND");

            // Parse date and time components
            DateTime date = DateTime.ParseExact(dateStr, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            TimeSpan startTime = TimeSpan.ParseExact(startTimeStr, "hh\\:mm", CultureInfo.InvariantCulture);
            TimeSpan endTime = TimeSpan.ParseExact(endTimeStr, "hh\\:mm", CultureInfo.InvariantCulture);

            // Combine date and time
            DateTime start = date.Add(startTime);
            DateTime end = date.Add(endTime);

            // Extract other parameters
            int id = GetValue<int>(row, "id");
            int roomId = GetValue<int>(row, "IDRoom");
            string notes = GetValue<string>(row, "notes") ?? string.Empty;
            string location = GetValue<string>(row, "location") ?? string.Empty;
            int categoryId = GetValue<int>(row, "CLevel");
            string cName = GetValue<string>(row, "Name") ?? string.Empty;
            int statusid = GetValue<int>(row, "statusid");
            int groupID = GetValue<int>(row, "GroupID");
            string descriptio = GetValue<string>(row, "descriptio") ?? string.Empty;
            int attendanceID = GetValue<int>(row, "AID");
            int extraID = GetValue<int>(row, "ExtraID");

            // Create one-time appointment (reaccurence: false)
            return Appointment.Create(
                id: id,
                startTime: start,
                endTime: end,
                roomId: roomId,
                notes: notes,
                location: location,
                categoryId: categoryId,
                cName: cName,
                statusid: statusid,
                dayweek: 0, // Ignored for non-recurring
                GroupID: groupID,
                reaccurence: false,
                descriptio: descriptio,
                AID: attendanceID,
                ExtraID: extraID
            );
        }

        // Creates a recurring appointment from a DataRow and checks for conflicts
        public static Appointment CreateRecurringAppointment(DataRow row)
        {
            // Extract parameters from DataRow with validation
            int id = GetValue<int>(row, "id");
         
            string startTimeStr = GetValue<string>(row, "TimeStart");
            string endTimeStr = GetValue<string>(row, "TimeEND");
            int dayweek = GetValue<int>(row, "Day"); // Recurrence day (0=Sunday, etc.)
            // Parse date and time components
            DayOfWeek targetDay = (DayOfWeek)dayweek;
            DateTime date = DateTime.Today;

            // Calculate days until the next target day (including today)
            int daysUntilTarget = ((int)targetDay - (int)date.DayOfWeek + 7) % 7;

            date = date.AddDays(daysUntilTarget);
            TimeSpan startTime = TimeSpan.ParseExact(startTimeStr, "hh\\:mm", CultureInfo.InvariantCulture);
            TimeSpan endTime = TimeSpan.ParseExact(endTimeStr, "hh\\:mm", CultureInfo.InvariantCulture);

            // Combine date and time
            DateTime start = date.Add(startTime);
            DateTime end = date.Add(endTime);

            int roomId = GetValue<int>(row, "IDRoom");
            string notes = GetValue<string>(row, "notes") ?? string.Empty;
            string location = GetValue<string>(row, "location") ?? string.Empty;
            int categoryId = GetValue<int>(row, "CLevel");
            string cName = GetValue<string>(row, "Name") ?? string.Empty;
            int statusid = GetValue<int>(row, "statusid");
           
            int groupID = GetValue<int>(row, "GroupID");
            string Description = GetValue<string>(row, "Description") ?? string.Empty;
            int attendanceID = GetValue<int>(row, "AID");
            int extraID = GetValue<int>(row, "ExtraID");
            // Create recurring appointment (reaccurence: true)
            Appointment recurringAppt = Appointment.Create(
                id: id,
                startTime: start,
                endTime: end,
                roomId: roomId,
                notes: notes,
                location: location,
                categoryId: categoryId,
                cName: cName,
                statusid: statusid,
                dayweek: dayweek,
                GroupID: groupID,
                reaccurence: true,
                descriptio: Description,
                AID: attendanceID,
                ExtraID: extraID
            );
            if (recurringAppt.Type == 1)
            {
                
            }
            return recurringAppt;
        }

      
        private static T GetValue<T>(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName)
                || row[columnName] == DBNull.Value)
            {
                return default(T);
            }
            return (T)Convert.ChangeType(row[columnName], typeof(T));
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
        private void Schedulerr_Loaded(object sender, RoutedEventArgs e)
        {
          /*  foreach (DataRow dr in dtReaccurent.Rows)
            {
                int occurrences = 4;

                int targetDayNumber = int.Parse(dr["day"].ToString()); // Example: 4 represents Thursday

                List<DateTime> nextDays = GetNextDays(targetDayNumber, occurrences);
                foreach (DateTime exceptionDate in nextDays)
                {


                    TimeSpan HourStart = TimeSpan.Parse(dr["TimeStart"].ToString());
                    TimeSpan HourEnd = TimeSpan.Parse(dr["TimeEnd"].ToString());
                    DateTime TimeStart = exceptionDate.Add(HourStart);
                    DateTime TimeEnd = exceptionDate.Add(HourEnd);
                    DateTimeRange range = new DateTimeRange(TimeStart, TimeEnd);
                    //  DateTimeRange range2 = new DateTimeRange(DateTime.MinValue, DateTime.MaxValue);
                    List<AppointmentItem> appointmentsList = scheduler.GetAppointments(range);
                    //List<AppointmentItem> appointmentsList2 = scheduler.GetAppointments(range2);
                    AppointmentItem check = null;
                    foreach (AppointmentItem appointmentloop in appointmentsList)
                    {
                        if (appointmentloop.CustomFields["GroupID"].ToString() == dr["GroupID"].ToString())
                        {
                            check = appointmentloop;
                        }
                    }
                    foreach (AppointmentItem appointmentloop in appointmentsList)
                    {

                        if (appointmentloop.ResourceId.Equals(check.ResourceId) && appointmentloop.StatusId.ToString() == "0" && appointmentloop.CustomFields["GroupID"].ToString() != check.CustomFields["GroupID"].ToString())
                        {
                            scheduler.RemoveAppointment(check);

                        }
                        if (appointmentloop.CustomFields["GroupID"].ToString() == check.CustomFields["GroupID"].ToString() && appointmentloop.StatusId.ToString() == "1")
                        {
                            scheduler.RemoveAppointment(check);
                        }
                    }
                }
            }*/
        }

        private void SchedulerControl1_AppointmentInserted(object sender, AppointmentEventArgs e)
        {
            
        }
        static List<DateTime> GetNextDays(int targetDayNumber, int occurrences)
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime today = DateTime.Today;
            int todayNumber = (int)today.DayOfWeek;
            int daysUntilTarget = ((targetDayNumber - todayNumber + 7) % 7);
            DateTime nextDate = today.AddDays(daysUntilTarget == 0 ? 7 : daysUntilTarget);

            for (int i = 0; i < occurrences; i++)
            {
                dates.Add(nextDate);
                nextDate = nextDate.AddDays(7);
            }

            return dates;
        }
        private void scheduler_AppointmentEdited(object sender, AppointmentEditedEventArgs e)
        {
            /*
            var editedAppointment = e.Appointments.FirstOrDefault();
            if (editedAppointment != null)
            {
                DateTimeRange range = new DateTimeRange(editedAppointment.Start, editedAppointment.End);
                List<AppointmentItem> appointmentsList = scheduler.GetAppointments(range);
                Appointment check;
                int existapp = 0;
                foreach (AppointmentItem appointmentloop in appointmentsList)
                {
                    if (appointmentloop.ResourceId.Equals(editedAppointment.ResourceId) && appointmentloop != editedAppointment && appointmentloop.StatusId.ToString() == "0")
                    {

                        TwoOptionsButtons two = new TwoOptionsButtons("A Session is already occupied in this time frame", "Remove old session", "Remove dragged session");
                        two.ShowDialog();
                        int result = two.Result;
                        if (result == 1)
                        {
                            appointmentloop.StatusId = 1;
                            if (editedAppointment.Type == AppointmentType.ChangedOccurrence)
                            {
                                AppointmentItem newNormalApt = scheduler.CopyAppointment(editedAppointment);
                                scheduler.RemoveAppointment(editedAppointment);
                                newNormalApt.CustomFields["ExtraID"] = Connexion.GetString("Insert into Attendance_Prog Output Inserted.ID Values (" + editedAppointment.CustomFields["GroupID"].ToString() + " , " + editedAppointment.ResourceId + " ,'" + editedAppointment.Start.ToString("dd-MM-yyyy") + "','" + editedAppointment.Start.ToString("HH:mm") + "','" + editedAppointment.End.ToString("HH:mm") + "',N'" + editedAppointment.Description + "',0)");
                                newNormalApt.CustomFields["GroupID"] = 0;
                                newNormalApt.StatusId = 0;
                                newNormalApt.Type = AppointmentType.Normal;
                                scheduler.AddAppointment(newNormalApt);
                                string TimeID = editedAppointment.CustomFields["TimeID"].ToString();
                                Connexion.Insert("Insert into Attendance_Prog Values (" +
                                           editedAppointment.CustomFields["GroupID"].ToString() + "," + Connexion.GetString("Select IDRoom from Class_Time Where ID = " + TimeID) + ",'" + editedAppointment.Start.ToString("dd-MM-yyyy") + "','" + Connexion.GetString("Select TimeStart from Class_Time Where ID = " + TimeID) + "','" + Connexion.GetString("Select TimeEnd From Class_Time Where ID = " + TimeID) + "','',1)");
                            }
                            else if (editedAppointment.CustomFields["ExtraID"].ToString() != "0")
                            {
                                Connexion.Insert("Update Attendance_Prog set Status = 0 where ID = " + editedAppointment.CustomFields["ExtraID"].ToString());

                            }
                            else if (editedAppointment.CustomFields["AttendanceID"].ToString() != "")
                            {
                                MessageBoxResult messageboxresult = MessageBox.Show("An appointment is already created in this TimeStamp, Do you want to delete it ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (messageboxresult == MessageBoxResult.Yes)
                                {
                                    // User clicked Yes
                                    // Perform the action for Yes
                                }
                                else if (messageboxresult == MessageBoxResult.No)
                                {
                                    // User clicked No
                                    // Perform the action for No
                                }
                            }
                            //if appointment is reaccurent delete  (then skip to planned attendance and create one ) if its created attendance then make it not be able to be deleted or ask if they want to delete this attendance    
                        }
                        else if (result == 2)
                        {
                            existapp = 1;


                        }
                    }
                }
                if (editedAppointment.Type == AppointmentType.ChangedOccurrence)
                {
                    if (existapp == 0)
                    {

                        var newNormalApt = scheduler.CopyAppointment(editedAppointment);
                        string TimeID = editedAppointment.CustomFields["TimeID"].ToString();
                        Connexion.Insert("Insert into Attendance_Prog Values (" +
                                   editedAppointment.CustomFields["GroupID"].ToString() + "," + Connexion.GetString("Select IDRoom from Class_Time Where ID = " + TimeID) + ",'" + editedAppointment.Start.ToString("dd-MM-yyyy") + "','" + Connexion.GetString("Select TimeStart from Class_Time Where ID = " + TimeID) + "','" + Connexion.GetString("Select TimeEnd From Class_Time Where ID = " + TimeID) + "','',1)");
                        newNormalApt.Type = AppointmentType.Normal;
                        string startTime = editedAppointment.Start.ToString("HH:mm");

                        string Endtime = editedAppointment.End.ToString("HH:mm");
                        Connexion.GetString("Insert into Attendance_Prog Output Inserted.ID Values (" + newNormalApt.CustomFields["GroupID"].ToString() + " , " + newNormalApt.ResourceId + " ,'" + newNormalApt.Start.ToString("dd-MM-yyyy") + "','" + startTime + "','" + Endtime + "',N'" + newNormalApt.Description + "',0)");

                        // scheduler.AppointmentItems.Remove(editedAppointment);
                    }
                    else
                    {
                        scheduler.RemoveAppointment(editedAppointment);
                        int dayOfWeek = ((int)editedAppointment.Start.DayOfWeek) % 7;
                        int TimeID = Connexion.GetInt("Select ID from Class_Time Where GID = " + editedAppointment.CustomFields["GroupID"].ToString() + " and day =" + dayOfWeek);

                        DataTable dtReaccurent = new DataTable();
                        Connexion.FillDT(ref dtReaccurent, "Select" +
                          " GroupID as GID ," + "CLassID as CID  ," +
                          "Class_Time.TimeStart," +
                          "Class_Time.TimeEnd , " +
                          "Class_Time.IDRoom ," +
                          "Class_Time.ID as TimeID , " +
                          "Class.CLevel  , " +
                          "Class_Time.Day as Day , " +
                          "Groups.GroupID as GroupID , " +
                          "Case when MultipleGroups = 'Multiple' then Class.CName + ' ' + Groups.GroupName " +
                          "When MultipleGroups = 'Single' then Class.CName " +
                          "End as Name " +
                          "from Class_Time  " +
                          "join  Groups on Groups.GroupID = Class_Time.GID " +
                          "join Class on Class.ID = Groups.ClassID where Class_Time.ID = " + TimeID);
                        Class.Appointment apt =
                   Class.Appointment.Create(
                   int.Parse(dtReaccurent.Rows[0]["CID"].ToString()),
                   DateTime.ParseExact(dtReaccurent.Rows[0]["TimeStart"].ToString(), "HH:mm", CultureInfo.InvariantCulture),
                   DateTime.ParseExact(dtReaccurent.Rows[0]["TimeEnd"].ToString(), "HH:mm", CultureInfo.InvariantCulture),
                   int.Parse(dtReaccurent.Rows[0]["IDRoom"].ToString()),
                   "", "",
                   int.Parse(dtReaccurent.Rows[0]["CLevel"].ToString()),
                   dtReaccurent.Rows[0]["Name"].ToString(),
                   0,
                   int.Parse(dtReaccurent.Rows[0]["Day"].ToString()),
                   int.Parse(dtReaccurent.Rows[0]["GroupID"].ToString()), false, "", 0);

                        Appointments.Add(apt);
                    }


                }
                else if (editedAppointment.CustomFields["ExtraID"].ToString() != "0")
                {
                    if (existapp == 0)
                    {
                        string startTimeString = editedAppointment.Start.ToString("HH:mm", CultureInfo.InvariantCulture);
                        string Endtime = editedAppointment.End.ToString("HH:mm", CultureInfo.InvariantCulture);


                        Connexion.Insert("Update Attendance_Prog Set TimeStart = '" + startTimeString + "' , TimeEnd = '" + Endtime + "', RoomID = '" + editedAppointment.ResourceId + "' where ID=" + editedAppointment.CustomFields["ExtraID"].ToString());
                    }
                    else
                    {
                        DataTable dtattend = new DataTable();
                        Connexion.FillDT(ref dtattend, "Select * , DATEPART(WEEKDAY, CONVERT(DATE, Date, 105)) - 1 AS Day from Attendance_Prog where id = " + editedAppointment.CustomFields["ExtraID"].ToString());
                        DateTime date = DateTime.ParseExact(dtattend.Rows[0]["Date"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);

                        // Parsing the start time from dr["TimeStart"]
                        DateTime startTime = DateTime.ParseExact(dtattend.Rows[0]["TimeStart"].ToString(), "HH:mm", CultureInfo.InvariantCulture);

                        // Parsing the end time from dr["TimeEnd"]
                        DateTime endTime = DateTime.ParseExact(dtattend.Rows[0]["TimeEnd"].ToString(), "HH:mm", CultureInfo.InvariantCulture);
                        // Combining the date with the time
                        DateTime startDateTime = date.Date.Add(startTime.TimeOfDay);
                        DateTime endDateTime = date.Date.Add(endTime.TimeOfDay);
                        int CID = Connexion.GetClassID(dtattend.Rows[0]["GID"].ToString());
                        int CLevel = Connexion.GetInt("Select CLevel from Class Where ID = " + CID);
                        string name = Connexion.GetString("Select Case when Class.MultipleGroups = 'Single' then Class.CName when Class.MultipleGroups = 'Multiple' then Class.CName + ' ' + Groups.GroupName end from Class Join Groups on Class.ID = Groups.ClassID Where GroupID = " + dtattend.Rows[0]["GID"].ToString());

                        Class.Appointment apt =
                            Class.Appointment.Create(
                            CID,
                           startDateTime, endDateTime,
                            int.Parse(dtattend.Rows[0]["RoomID"].ToString()),
                            dtattend.Rows[0]["Note"].ToString(), dtattend.Rows[0]["Note"].ToString(),
                            CLevel,
                           name,
                            int.Parse(dtattend.Rows[0]["Status"].ToString()),
                            int.Parse(dtattend.Rows[0]["Day"].ToString()),
                            int.Parse(dtattend.Rows[0]["GID"].ToString()), false, dtattend.Rows[0]["Note"].ToString(), 0, int.Parse(dtattend.Rows[0]["ID"].ToString()));
                        Appointments.Add(apt);
                        scheduler.RemoveAppointment(editedAppointment);
                    }
                }
                else
                {
                    if (existapp == 0)
                    {

                        string startTime = editedAppointment.Start.ToString("HH:mm", CultureInfo.InvariantCulture);
                        string Endtime = editedAppointment.End.ToString("HH:mm", CultureInfo.InvariantCulture);

                        Connexion.Insert("Update Attendance Set TimeStart = '" + startTime + "' , TimeEnd = '" + Endtime + "' , RoomID = '" + editedAppointment.ResourceId + "'  where ID = " + editedAppointment.CustomFields["AttendanceID"].ToString());
                    }
                    else
                    {
                        string AID = editedAppointment.CustomFields["AttendanceID"].ToString();
                        DataTable dtapp = new DataTable();
                        Connexion.FillDT(ref dtapp, "Select " +
                        "Attendance.GroupID as GID ," +
                        "Attendance.TimeStart as TimeStart," +
                        "Attendance.TimeEND as TimeEND , attendance.ID as AID ,  " +
                        "Attendance.RoomID as IDRoom ,Attendance.Date as Date ," +

                        "Class.CLevel as CLevel , " +
                        " DATEPART(WEEKDAY, CONVERT(DATE, Date, 105)) - 1 AS Day ," +
                        "Attendance.GroupID as GroupID , " +
                        "Groups.ClassID as CID  , " +
                        "Case When Class.MultipleGroups = 'Multiple' then class.CName + ' ' + Groups.GroupName " +
                        "When MultipleGroups = 'Single' then Class.CName End as Name  from Attendance " +
                        "Join Groups on Groups.GroupID = Attendance.GroupID  " +
                        "Join Class on Class.ID = Groups.ClassID where Attendance.ID =" + AID);
                        DateTime date = DateTime.ParseExact(dtapp.Rows[0]["Date"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);

                        // Parsing the start time from dr["TimeStart"]
                        DateTime startTime = DateTime.ParseExact(dtapp.Rows[0]["TimeStart"].ToString(), "HH:mm", CultureInfo.InvariantCulture);

                        // Parsing the end time from dr["TimeEnd"]
                        DateTime endTime = DateTime.ParseExact(dtapp.Rows[0]["TimeEnd"].ToString(), "HH:mm", CultureInfo.InvariantCulture);

                        // Combining the date with the time
                        DateTime startDateTime = date.Date.Add(startTime.TimeOfDay);
                        DateTime endDateTime = date.Date.Add(endTime.TimeOfDay);

                        Class.Appointment apt =
                            Class.Appointment.Create(
                            int.Parse(dtapp.Rows[0]["CID"].ToString()),
                           startDateTime, endDateTime,
                            int.Parse(dtapp.Rows[0]["IDRoom"].ToString()),
                            "", "",
                            int.Parse(dtapp.Rows[0]["CLevel"].ToString()),
                            dtapp.Rows[0]["Name"].ToString(),
                            0,
                            int.Parse(dtapp.Rows[0]["Day"].ToString()),
                            int.Parse(dtapp.Rows[0]["GroupID"].ToString()), false, "", int.Parse(dtapp.Rows[0]["AID"].ToString()));

                        Appointments.Add(apt);
                        scheduler.RemoveAppointment(editedAppointment);


                    }

                }

            }*/
        }

        private void scheduler_EditOccurrenceWindowShowing(object sender, EditOccurrenceWindowShowingEventArgs e)
        {

            e.Handled = true;
            e.Cancel = true;
        }

        private void scheduler_AppointmentEditing(object sender, AppointmentEditingEventArgs e)
        {
            //    e.Cancel = true;
        }

        private void scheduler_AppointmentWindowShowing(object sender, AppointmentWindowShowingEventArgs e)
        {

            TwoOptionsButtons two = new TwoOptionsButtons("What Do you want to open ?", "Class Info", "Session Info");
            two.ShowDialog();
            int result = two.Result;
            AppointmentItem selectedApt;
            if (result == 1)
            {
                if (this.scheduler.SelectedAppointments.Count == 1)
                {
                    selectedApt = this.scheduler.SelectedAppointments[0];

                    string groupid = selectedApt.CustomFields["GroupID"].ToString();
                    string id = Connexion.GetClassID(groupid).ToString();
                    string Multi = Connexion.GetString("Select MultipleGroups from Class where ID = " + id);
                    ClassAdd cadd = new ClassAdd("Show", id, Multi);
                    cadd.Show();
                }
            }
            else if (result == 2)
            {

                if (this.scheduler.SelectedAppointments.Count == 1)
                {
                    string AID = "";
                    DateTime selectedDate = scheduler.SelectedInterval.Start;
                    selectedApt = this.scheduler.SelectedAppointments[0];
                    if (selectedApt.StatusId.ToString() == "1")
                    {
                        MessageBox.Show("This Session is cancelled ");
                        return;
                    }
                    if (selectedApt.Type == AppointmentType.Occurrence) //type is for reaccurent sessions 
                    {
                        string groupid = selectedApt.CustomFields["GroupID"].ToString();

                        DateTime startDate = selectedApt.Start;

                        // Format the date as dd/MM/yyyy
                        string formattedDate = startDate.ToString("dd-MM-yyyy");

                        Connexion.Insert("Update Groups set Sessions = Sessions + 1 , TSessions = TSessions + 1 Where GroupID = " + groupid);

                        int result2 = Connexion.GetInt("Select Sessions From Groups Where GroupID = '" + groupid + "'");
                        int IDRoom = int.Parse(selectedApt.ResourceId.ToString());
                        string TimeStart = startDate.ToString("HH:mm");
                        string TimeEnd = selectedApt.End.ToString("HH:mm");
                        AID = Connexion.GetInt("Insert into Attendance (GroupID,Date,Session,RoomID,TimeStart,TimeEND) OUTPUT Inserted.ID  Values(" + groupid + ",'" + formattedDate + "','" + result2 + "' , '" + IDRoom + "','" + TimeStart + "','" + TimeEnd + "' )").ToString();
                        var AddS = new AttendanceAdd(AID, "Add", "1");
                        AddS.ShowDialog();


                    }
                    else
                    {
                        AID = selectedApt.CustomFields["AttendanceID"].ToString();
                        if (AID != "0") //attendance that is created 
                        {
                            AttendanceAdd attendancepanel = new AttendanceAdd(AID, "Show", "1");
                            attendancepanel.ShowDialog();
                        }
                        else // attendance that is planned 
                        {
                            string APID = selectedApt.CustomFields["ExtraID"].ToString();
                            string groupid = Connexion.GetString("Select GID from Attendance_Prog where ID =" + APID);
                            Connexion.Insert("Update Groups set Sessions = Sessions + 1 , TSessions = TSessions + 1 Where GroupID = " + groupid);
                            string formattedDate = Connexion.GetString("Select Date from Attendance_Prog where ID = " + APID);
                            string IDRoom = Connexion.GetString("Select RoomID  from Attendance_Prog where ID = " + APID);
                            string TimeStart = Connexion.GetString("Select TimeStart  from Attendance_Prog where ID = " + APID);
                            string TimeEnd = Connexion.GetString("Select TimeEnd  from Attendance_Prog where ID = " + APID);

                            int result2 = Connexion.GetInt("Select Sessions From Groups Where GroupID = '" + groupid + "'");
                            string AttendanceID = Connexion.GetInt("Insert into Attendance (GroupID,Date,Session,RoomID,TimeStart,TimeEND) OUTPUT Inserted.ID  Values(" + groupid + ",'" + formattedDate + "','" + result2 + "' , '" + IDRoom + "','" + TimeStart + "','" + TimeEnd + "' )").ToString();
                            Connexion.Insert("Delete from Attendance_Prog Where ID =" + APID);
                            selectedApt.CustomFields["ExtraID"] = "";
                            var AddS = new AttendanceAdd(AttendanceID, "Add", "1");// modify the attendance and delete the old one 
                            AddS.ShowDialog();
                            AID = AttendanceID;

                        }
                    }
                    DateTime date = DateTime.ParseExact(Connexion.GetString("Select Date from Attendance Where ID = " + AID), "dd-MM-yyyy", CultureInfo.InvariantCulture);

                    // Parsing the start time from dr["TimeStart"]

                    TimeSpan startTime = TimeSpan.Parse(Connexion.GetString("Select TimeStart From Attendance Where ID = " + AID));
                    // Parsing the end time from dr["TimeEnd"]
                    TimeSpan endTime = TimeSpan.Parse(Connexion.GetString("Select TimeEnd From Attendance Where ID = " + AID));

                    // Combining the date with the time
                    DateTime startDateTime = date.Add(startTime);
                    DateTime endDateTime = date.Add(endTime);

                    selectedApt.Start = startDateTime;
                    selectedApt.End = endDateTime;
                    selectedApt.CustomFields["roomId"] = Connexion.GetString("Select RoomID from Attendance Where ID = " + AID);
                    selectedApt.Description = Connexion.GetString("Select Note from Attendance Where ID = " + AID);
                    scheduler.RefreshData();

                }
            }

            e.Handled = true;
            e.Cancel = true;
        }//?????

        private void BarButtonItem_NewAppointment(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            var selectedInterval = scheduler.SelectedInterval;
            // Get the start date of the selected interval
            string selectedStartDate = selectedInterval.Start.ToString("dd/MM/yyyy");

            Panels.Attendance_Programmed AttP = new Attendance_Programmed(selectedStartDate);
            AttP.ShowDialog();
            string ProgramID = AttP.returnID;
            if (ProgramID == null)
            {
                return;
            }
            DataTable dt = new DataTable();
            Connexion.FillDT(ref dt, "Select * , DATEPART(WEEKDAY, CONVERT(DATE, Date, 105)) - 1 AS Day from Attendance_Prog where ID = " + ProgramID);
            DateTime date = DateTime.ParseExact(dt.Rows[0]["Date"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);

            // Parsing the start time from dr["TimeStart"]
            DateTime startTime = DateTime.ParseExact(dt.Rows[0]["TimeStart"].ToString(), "HH:mm", CultureInfo.InvariantCulture);

            // Parsing the end time from dr["TimeEnd"]
            DateTime endTime = DateTime.ParseExact(dt.Rows[0]["TimeEnd"].ToString(), "HH:mm", CultureInfo.InvariantCulture);
            // Combining the date with the time
            DateTime startDateTime = date.Date.Add(startTime.TimeOfDay);
            DateTime endDateTime = date.Date.Add(endTime.TimeOfDay);
            int CID = Connexion.GetClassID(dt.Rows[0]["GID"].ToString());
            int CLevel = Connexion.GetInt("Select CLevel from Class Where ID = " + CID);
            string name = Connexion.GetString("Select Case when Class.MultipleGroups = 'Single' then Class.CName when Class.MultipleGroups = 'Multiple' then Class.CName + ' ' + Groups.GroupName end from Class Join Groups on Class.ID = Groups.ClassID Where GroupID = " + dt.Rows[0]["GID"].ToString());

            Class.Appointment apt =
                Class.Appointment.Create(
                CID,
               startDateTime, endDateTime,
                int.Parse(dt.Rows[0]["RoomID"].ToString()),
                dt.Rows[0]["Note"].ToString(), dt.Rows[0]["Note"].ToString(),
                CLevel,
               name,
                0,
                int.Parse(dt.Rows[0]["Day"].ToString()),
                int.Parse(dt.Rows[0]["GID"].ToString()), false, dt.Rows[0]["Note"].ToString(), int.Parse(ProgramID));
          //  Appointments.Add(apt);
        }

        private void ActivateSes_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            
        }

        private void DisActivateSes_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            /*var selectedAppointments = scheduler.SelectedAppointments;
            foreach (Appointment appointment in selectedAppointments)
            {
                appointment.StatusId = 1;

            }*/



        }

        private void DeleteSes_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {

        }

        private void scheduler_AppointmentAdded(object sender, AppointmentAddedEventArgs e)
        {

            /*   foreach (var appointment in e.Appointments)
               {
                   if (bool.Parse(appointment.CustomFields["ReoccurrenceExceptionFinished"].ToString()) == true  ) // Example condition
                   {

                   }
               }
            */
        }
    }
}
