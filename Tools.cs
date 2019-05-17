using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;

namespace NeoSpider
{
    public static class Tools
    {
        private static readonly Dictionary<string, string> Dic = new Dictionary<string, string>()
        {
            { "602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7", "GAS" },
            { "c56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b", "NEO"}
        };

        public static DateTime ToDateTime(this JToken timeStamp)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local).AddSeconds((int)timeStamp);
        }

        public static string ToName(this JToken assetId)
        {
            Dic.TryGetValue((string)assetId, out string name);
            return name;
        }

        public static JObject DownloadJson(this WebClient web, string address, int page)
        {
            var raw = web.DownloadString($"https://neoscan.io/api/main_net/v1/get_address_abstracts/{address}/{page}");
            return JObject.Parse(raw);
        }
    }
}
