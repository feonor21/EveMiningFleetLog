using System;

namespace EveMiningFleet.Entities.Tables
{
    public class AlerteMessage
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime End { get; set; }

    }
}
