using Newtonsoft.Json.Linq;
using System;

namespace NeoSpider
{
    internal class Transaction
    {
        public string TxId { get; set; }
        public DateTime Time { get; set; }
        public string Asset { get; set; }
        public decimal Amount { get; set; }
        public string AddressFrom { get; set; }
        public string AddressTo { get; set; }

        public static Transaction FromJson(JToken json)
        {
            var asset = json["asset"].ToName();
            if (asset == null) return null;
            return new Transaction() {
                TxId = (string)json["txid"],
                Time = json["time"].ToDateTime(),
                Asset = asset,
                Amount = (decimal)json["amount"],
                AddressFrom = (string)json["address_from"],
                AddressTo = (string)json["address_to"],
            };
        }
    }
}