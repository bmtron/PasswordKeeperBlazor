using System;
using AccountsModel;
using PasswordKeeperBlazor.DataObjects;

namespace PasswordKeeperBlazor.Data
{
	public class AccountsService
	{
        AccountsModel.AccountsModel db;

        public AccountsService()
		{
            db = new AccountsModel.AccountsModel();

        }
        public List<AccountLogin> GetAccounts()
        {
            var x = db.AccountLogins.ToList();

            return x;
        }

        public List<AccountLogin> GetAccountsByMasterId(int masterId)
        {
            return db.AccountLogins.Where(al => al.MasterAccount.MasterId == masterId).ToList();
        }

        public int GetMasterId(string aspUserId)
        {
            var masterId = db.MasterAccounts.Single(ma => ma.AspUserID == aspUserId);
            return masterId.MasterId;
        }
    }
}

