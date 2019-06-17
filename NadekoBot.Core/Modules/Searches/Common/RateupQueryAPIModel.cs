using System;
using System.Collections.Generic;
using System.Text;

namespace NadekoBot.Core.Modules.Searches.Common
{
    class RateupQueryAPIModel
    {
        public string errorCode { get; set; }
        public ErrorInfo errorInfo { get; set; }
        public List<FriendRateupData> friendRateups { get; set; }

        public class ErrorInfo
        {
            public string basicErrorInfo { get; set; }
            public string debugInfo { get; set; }
        }

        public class FriendRateupData
        {
            public string friendName { get; set; }
            public string area { get; set; }
            public string[] items { get; set; }
        }

        public static RateupQueryAPIModel ConvertString(string str)
        {
            RateupQueryAPIModel parsedData = new RateupQueryAPIModel();

            Queue<string> lines = new Queue<string>(str.Split("<br>"));

            parsedData.errorCode = lines.Dequeue();
            if (!parsedData.errorCode.Equals("OK"))
            {
                parsedData.errorInfo.basicErrorInfo = lines.Dequeue();
                parsedData.errorInfo.debugInfo = lines.Dequeue();
                parsedData.friendRateups = null;
                return parsedData;
            }

            parsedData.errorInfo = null;
            while (lines.Count > 0)
            {
                FriendRateupData rateupData = new FriendRateupData();
                rateupData.friendName = lines.Dequeue();
                rateupData.area = lines.Dequeue();
                rateupData.items = lines.Dequeue().Split(',');

                parsedData.friendRateups.Add(rateupData);
            }

            return parsedData;
        }
    }
}
