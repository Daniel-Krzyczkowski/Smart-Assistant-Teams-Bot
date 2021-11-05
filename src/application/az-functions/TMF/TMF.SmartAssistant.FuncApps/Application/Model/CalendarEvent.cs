using System;

namespace TMF.SmartAssistant.FuncApps.Application.Model
{
    internal class CalendarEvent
    {
        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Subject { get; set; }
        public string OwnerId { get; set; }
    }
}
