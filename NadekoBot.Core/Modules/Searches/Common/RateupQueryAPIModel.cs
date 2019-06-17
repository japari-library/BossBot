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
        public RateupQueryAPIModel()
        {
            errorInfo = new ErrorInfo();
            friendRateups = new List<FriendRateupData>();
        }

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

        public static RateupQueryAPIModel ConvertToModel(string str)
        {
            RateupQueryAPIModel parsedToModel = new RateupQueryAPIModel();
            Queue<string> lines = new Queue<string>(str.Split("<br>"));

            // Get error code, and error info if one is received
            parsedToModel.errorCode = lines.Dequeue();
            if (!parsedToModel.errorCode.Equals("OK"))
            {
                parsedToModel.errorInfo.basicErrorInfo = lines.Dequeue();
                parsedToModel.errorInfo.debugInfo = lines.Dequeue();
                parsedToModel.friendRateups = null; // no friend results given
                return parsedToModel;
            }

            parsedToModel.errorInfo = null; // no errors reported
            while (lines.Count > 1) // The last line is a rogue empty one after the last "<br>" that can be ignored
            {
                // Get trio of Friend name, area and items
                FriendRateupData rateupData = new FriendRateupData();
                rateupData.friendName = lines.Dequeue();
                rateupData.area = lines.Dequeue();
                rateupData.items = lines.Dequeue().Split(',');

                parsedToModel.friendRateups.Add(rateupData);
            }

            return parsedToModel;
        }
    }
}
