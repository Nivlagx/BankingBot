using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankingBot.DataModels
{
    public class AccountsTable
    {
        [JsonProperty(PropertyName = "Id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "cheque")]
        public double Cheque { get; set; }

        [JsonProperty(PropertyName = "savings")]
        public double Savings { get; set; }

        [JsonProperty(PropertyName = "credit")]
        public double Credit { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public DateTime Date { get; set; }
    }
}