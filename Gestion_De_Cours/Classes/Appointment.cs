using DevExpress.XtraScheduler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;

public class Appointment
{
    public static Appointment Create()
    {
        return new Appointment();
    }

    internal static Appointment Create(int id, DateTime startTime, DateTime endTime,
        int roomId, string notes, string location, int categoryId, string cName,
        int statusid, int dayweek, int GroupID, bool reaccurence,
        string descriptio, int AID, int ExtraID = 0)
    {
        Appointment apt = Appointment.Create();
        apt.roomId = roomId;
        apt.Notes = notes;
        apt.Location = location;
        apt.CategoryId = categoryId;
        apt.CName = cName;
        apt.Type = reaccurence ? 1 : 0;
        apt.StatusId = statusid;
        apt.Id = id;
        apt.GroupID = GroupID;
        apt.Description = descriptio;
        apt.AttendanceID = AID;
        apt.ReoccurrenceExceptionFinished = false;
        apt.StartTime = startTime;
        apt.EndTime = endTime;
        if (reaccurence)
        {
            if (reaccurence)
            {
                RecurrenceInfo r = new RecurrenceInfo();

                r.Type = RecurrenceType.Weekly;
                r.Start = startTime;
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
                r.WeekDays = (DevExpress.XtraScheduler.WeekDays)(WeekDays)dayweek;
                r.OccurrenceCount = 15;
                r.Periodicity = 1;
                r.Range = RecurrenceRange.OccurrenceCount;
                apt.RecurrenceInfo = r.ToXml();
            }
        }   
        else
        {
            apt.StartTime = startTime;
            apt.EndTime = endTime;
            apt.ExtraID = ExtraID;
        }
        return apt;
    }

    // Helper class for recurrence pattern
    private class RecurrencePattern
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public WeekDays WeekDays { get; set; }
        public int OccurrenceCount { get; set; }
    }



    // WeekDays enum (defined since we can't use WinForms version)
    [Flags]
    public enum WeekDays
    {
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64
    }

    // Existing properties remain unchanged
    protected Appointment() { }
    public virtual int Id { get; set; }
    public virtual bool AllDay { get; set; }
    public virtual DateTime StartTime { get; set; }
    public virtual DateTime EndTime { get; set; }
    public virtual string CName { get; set; }
    public virtual string Notes { get; set; }
    public virtual string Subject { get; set; }
    public virtual int StatusId { get; set; }
    public virtual string labelID { get; set; }
    public virtual int CategoryId { get; set; }
    public virtual int Type { get; set; }
    public virtual string Location { get; set; }
    public virtual string RecurrenceInfo { get; set; }
    public virtual string Description { get; set; }
    public virtual string ReminderInfo { get; set; }
    public virtual int? roomId { get; set; }
    public virtual int GroupID { get; set; }
    public virtual int AttendanceID { get; set; }
    public virtual int ExtraID { get; set; }
    public virtual int TimeID { get; set; }
    public virtual bool ReoccurrenceExceptionFinished { get; set; }
}