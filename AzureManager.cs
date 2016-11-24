using BankingBot.DataModels;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BankingBot
{
    public class AzureManager
    {
        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<AccountsTable> accountsTable;


        private AzureManager()
        {
            this.client = new MobileServiceClient("http://contosodigitaldb.azurewebsites.net");
            this.accountsTable = this.client.GetTable<AccountsTable>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task<List<AccountsTable>> GetBalance()
        {
            return await this.accountsTable.ToListAsync();
        }
    }
}