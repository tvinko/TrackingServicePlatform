using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using TrackingService.Helpers;
using TrackingService.Models;

namespace TrackingService.Services.Db
{
    public class TrackingServiceAccountUnit : ITrackingServiceAccountUnit
    {
        const string ACCOUNT_NOT_FOUND = "Account not found";
        const string ACCOUNT_ACTIVE = "Account active";
        const string ACCOUNT_DISABLED = "Account disabled";

        private readonly TrackingServiceDbContext _context;

        public TrackingServiceAccountUnit(TrackingServiceDbContext context)
        {
            this._context = context;
        }

        public string GetConnectionString()
        {
            return this._context.Database.GetDbConnection().ConnectionString;
        }

        /// <summary>
        /// Validates account and can have three states:
        /// 1) Exists in database and is active
        /// 2) Exists in database and is deactivated
        /// 3) Doesn't exists in database
        /// 
        /// </summary>
        /// <param name="accountStatusData">In/Out structure that provides account id and stores account status</param>
        public void CheckAccountStatus(AccountStatusData accountStatusData)
        {
            try
            {
                using (var command = this._context.Database.GetDbConnection().CreateCommand())
                {
                    if (command.Connection.State == ConnectionState.Closed)
                        command.Connection.Open();

                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "sp_IsAccountActive";

                    command.Parameters.Add(new SqlParameter("@AccountID", accountStatusData.AccountID));

                    var result = command.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                    {
                        accountStatusData.HttpCode = HTTP_CODES.SUCCESS_ACCOUNT_NOT_FOUND;
                        accountStatusData.HttpMessage = ACCOUNT_NOT_FOUND;
                    }
                    else if ((bool)result)
                    {
                        accountStatusData.HttpCode = HTTP_CODES.SUCCESS_ACCOUNT_ACTIVE;
                        accountStatusData.HttpMessage = ACCOUNT_ACTIVE;
                    }
                    else
                    {
                        accountStatusData.HttpCode = HTTP_CODES.SUCCESS_ACCOUNT_DISABLED;
                        accountStatusData.HttpMessage = ACCOUNT_DISABLED;
                    }
                }
            }
            catch (Exception ex)
            {
                accountStatusData.HttpCode = HTTP_CODES.ERROR_DB_QUERY;
                accountStatusData.HttpMessage = ex.Message;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }
    }
}
