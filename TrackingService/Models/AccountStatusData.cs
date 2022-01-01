using TrackingService.Helpers;

namespace TrackingService.Models
{
    public class AccountStatusData
    {
        public int AccountID { get; set; }
        public HTTP_CODES HttpCode { get; set; }
        public string HttpMessage { get; set; }

        public AccountStatusData(int AccountID)
        {
            this.AccountID = AccountID;
        }
    }
}
