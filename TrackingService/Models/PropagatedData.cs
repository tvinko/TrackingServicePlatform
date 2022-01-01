using System;

namespace TrackingService.Models
{
    public class PropagatedData
    {
        public int AccountID { get; set; }
        public string Data { get; set; }
        public DateTime TimeStamp { get; set; }

        public PropagatedData(int AccountID, string Data)
        {
            this.AccountID = AccountID;
            this.TimeStamp = DateTime.Now;
            this.Data = Data;
        }
    }
}
