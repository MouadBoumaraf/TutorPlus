
using DevExpress.Mvvm.POCO;
using DevExpress.Utils.Filtering.Internal;
using DevExpress.Xpf.Scheduling;
using DevExpress.XtraScheduler;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gestion_De_Cours.Classes
{
    public class Class
    {

        public class WFTime
        {
            public string Day { get; set; }
            public string TimeStart { get; set; }
            public string TimeEnd { get; set; }

        }

        public class Appointment
        {
            public static Appointment Create()
            {
                return new Appointment();
            }
            internal static Appointment Create(int id ,DateTime startTime, DateTime endTime,
            int roomId, string notes, string location, int categoryId, string cName  ,int statusid  , int dayweek , int GroupID , bool reaccurence , string descriptio , int AID , int ExtraID =0)
            {
                Appointment apt = Appointment.Create();   
                apt.StartTime = startTime;
                apt.EndTime = endTime;
                apt.roomId = roomId;
                apt.Notes = notes;
                apt.Location = location;
                apt.CategoryId = categoryId;
                apt.CName = cName;
                apt.Type = 0;
                apt.StatusId = statusid;
                apt.Id = id;
                apt.GroupID = GroupID;
                apt.Description = descriptio;
                apt.AttendanceID = AID;
                apt.ReoccurrenceExceptionFinished = false;
                
                if (reaccurence)
                {
                    RecurrenceInfo r = new RecurrenceInfo();
                    r.Type = RecurrenceType.Weekly;
                    r.Start = startTime;
                    r.End = endTime;
                    r.Month = startTime.Month;
                    r.DayNumber = startTime.Day;
                    
                    apt.Type = 1;
                    if (dayweek == 0)
                    {
                        dayweek = 1;
                    }
                    else if (dayweek == 1)
                    {
                        dayweek = 2;
                    }
                    else if (dayweek == 2)
                    {
                        dayweek = 4;
                    }
                    else if (dayweek == 3)
                    {
                        dayweek = 8;
                    }
                    else if (dayweek == 4)
                    {
                        dayweek = 16;
                    }
                    else if (dayweek == 5)
                    {
                        dayweek = 32;

                    }
                    else if (dayweek == 6)
                    {
                        dayweek = 64;
                    }
                    r.WeekDays = (WeekDays)dayweek;
                    r.OccurrenceCount = 15;
                    r.Periodicity = 1;
                    r.Range = RecurrenceRange.OccurrenceCount;
                    apt.RecurrenceInfo = r.ToXml();
                    apt.ReoccurrenceExceptionFinished = false;
                    apt.TimeID = ExtraID;
                    DateTime firstRecurrence = startTime.AddDays(7); // Weekly, so add 7 days

      

                }
                else
                {
                    apt.ExtraID = ExtraID;
                }
                return apt;

            }
            protected Appointment() { }
            public virtual int Id { get; set; }
            public virtual bool AllDay { get; set; }
            public virtual DateTime StartTime { get; set; }
            public virtual DateTime EndTime { get; set; }
            public virtual string CName { get; set; }
            public virtual string Notes { get; set; }
            public virtual string Subject { get; set; }
            public virtual int StatusId { get; set; } // delayed, stopped , ect

            public virtual string labelID { get; set; }
            public virtual int CategoryId { get; set; }// lycee cem faq ... 
            public virtual int Type { get; set; }
            public virtual string Location { get; set; }
            public virtual string RecurrenceInfo { get; set; }



            public virtual string Description { get; set; }

            public virtual string ReminderInfo { get; set; }
            public virtual int? roomId { get; set; }

            public virtual int GroupID { get; set; }

            public virtual int AttendanceID { get; set; }

            public virtual int ExtraID { get; set;  }
            public virtual int TimeID { get; set; }

            public virtual bool ReoccurrenceExceptionFinished { get; set; }
        }
        public class Room
        {
            public static Room Create(int Id, string Name)
            {

                return new Room { Id = Id, Name = Name };
            }
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
        }

        public class YearColor
        {
            public static YearColor Create()
            {
                return ViewModelSource.Create(() => new YearColor());
            }
            protected YearColor() { }
            public virtual int Id { get; set; }
            public virtual string Caption { get; set; }
            public virtual Color Color { get; set; }
        }
        public class SessionsStatus
        {
            public static SessionsStatus Create()
            {
                return ViewModelSource.Create(() => new SessionsStatus());
            }

            protected SessionsStatus() { }
            public virtual int Id { get; set; }
            public virtual string Caption { get; set; }
            public virtual Brush Brush { get; set; }
        }
        public class Spec
        {
            public string Speciality { get; set; }
            public string ID { get; set;  }
        }

        public class ClassTime
        {
            public string GroupID { get; set; }
            public string GroupName { get; set; }
            public string Day { get; set; }
            public string TimeStart { get; set; }
            public string TimeEnd { get; set; }
            public string Frequency { get; set; }
            public string Room { get; set; }

        }

        public class Classes
        {
            public int id { get; set; }
            public string TFName { get; set; }
            public string TLName { get; set; }
        }

        public class student
        {
            public string StudentID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string GroupID { get; set; }
            public string GroupName { get; set; }
        }

        public class ClassStudent
        {
            public string ClassName { get; set; }
            public string Subject { get; set; }
            public string id { get; set; }
        }
        public class TeacherSub
        {
            public string IDspec { get; set; }
            public string IDsub { get; set; }
            public string IDyear { get; set; }
            public string Speciality { get; set; }
            public string Subject { get; set;  }
            public string Year { get; set; }
        }
        public class Group
        {
            public string ClassID { get; set; }
            public string GroupID { get; set; }
            public string GroupName { get; set; }
            public int GroupGender { get; set; }
        }

    }
}
