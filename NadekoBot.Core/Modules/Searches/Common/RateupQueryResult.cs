using System;
using System.Collections.Generic;
using System.Text;

namespace NadekoBot.Core.Modules.Searches.Common
{
    class RateupQueryResult
    {
        public string errorCode { get; set; }
        public ErrorInfo errorInfo { get; set; }
        public List<FriendRateupData> friendRateups { get; set; }
        public RateupQueryResult()
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
            public List<KeyValuePair<string, string[]>> itemsByArea { get; set; }
            public FriendRateupData()
            {
                itemsByArea = new List<KeyValuePair<string, string[]>>();
            }
        }

        public static RateupQueryResult ConvertToModel(string str)
        {
            RateupQueryResult parsedToModel = new RateupQueryResult();
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

            string lastFriend = null;
            while (lines.Count > 1) // The last line is a rogue empty one after the last "<br>" that can be ignored
            {
                string newFriend = lines.Dequeue();
                if (newFriend.Equals(lastFriend))
                {
                    // get area and item names and add them to the previous friend
                    parsedToModel.friendRateups[parsedToModel.friendRateups.Count - 1].itemsByArea.Add(
                        new KeyValuePair<string, string[]>(
                            lines.Dequeue(),                    // area name
                            lines.Dequeue().Split(',')          // item list
                            )
                        );
                }
                else
                {
                    // Get trio of Friend name, area and items
                    FriendRateupData rateupData = new FriendRateupData();
                    rateupData.friendName = lastFriend = newFriend;
                    rateupData.itemsByArea.Add(
                        new KeyValuePair<string, string[]>(
                            lines.Dequeue(),                    // area name
                            lines.Dequeue().Split(','))         // item list
                        );

                    parsedToModel.friendRateups.Add(rateupData);
                }
            }
            
            return parsedToModel;
        }
    }
}
