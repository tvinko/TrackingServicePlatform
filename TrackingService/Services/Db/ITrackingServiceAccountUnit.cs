using System;
using TrackingService.Models;

namespace TrackingService.Services.Db
{
    public interface ITrackingServiceAccountUnit : IDisposable
    {
        void CheckAccountStatus(AccountStatusData accountStatusData);
        string GetConnectionString();
    }
}