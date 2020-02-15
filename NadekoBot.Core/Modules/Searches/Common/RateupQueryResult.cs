using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NadekoBot.Core.Modules.Searches.Common
{
    public class RateupQueryResult
    {
        private const int MaxFriendsInReturnedString = 2;
        public string ErrorCode { get; set; }
        public ErrorInfo ErrorInformation { get; set; }
        public List<FriendRateupData> FriendRateups { get; set; }
        public RateupQueryResult()
        {
            ErrorInformation = new ErrorInfo();
            FriendRateups = new List<FriendRateupData>();
        }

        public class ErrorInfo
        {
            public string BasicErrorInfo { get; set; }
            public string DebugInfo { get; set; }
        }

        public class FriendRateupData
        {
            public string FriendName { get; set; }
            public List<KeyValuePair<string, string[]>> ItemsByArea { get; set; }
            public FriendRateupData()
            {
                ItemsByArea = new List<KeyValuePair<string, string[]>>();
            }
        }

        public static RateupQueryResult ConvertToModel(string str)
        {
            RateupQueryResult parsedToModel = new RateupQueryResult();
            Queue<string> lines = new Queue<string>(str.Split("<br>"));

            // Get error code, and error info if one is received
            parsedToModel.ErrorCode = lines.Dequeue();
            if (!parsedToModel.ErrorCode.Equals("OK"))
            {
                parsedToModel.ErrorInformation.BasicErrorInfo = lines.Dequeue();
                parsedToModel.ErrorInformation.DebugInfo = lines.Dequeue();
                parsedToModel.FriendRateups = null; // no friend results given
                return parsedToModel;
            }

            parsedToModel.ErrorInformation = null; // no errors reported

            string lastFriend = null;
            while (lines.Count > 1) // The last line is a rogue empty one after the last "<br>" that can be ignored
            {
                string newFriend = lines.Dequeue();
                if (newFriend.Equals(lastFriend))
                {
                    // get area and item names and add them to the previous friend
                    parsedToModel.FriendRateups[parsedToModel.FriendRateups.Count - 1].ItemsByArea.Add(
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
                    rateupData.FriendName = lastFriend = newFriend;
                    rateupData.ItemsByArea.Add(
                        new KeyValuePair<string, string[]>(
                            lines.Dequeue(),                    // area name
                            lines.Dequeue().Split(','))         // item list
                        );

                    parsedToModel.FriendRateups.Add(rateupData);
                }
            }
            
            return parsedToModel;
        }

        public string GetDiscordString()
        {
            if (ErrorCode != "OK")
            {
                return ErrorInformation.BasicErrorInfo;
            }

            const string openingString = "```asciidoc";
            const string closingString = "```";

            StringBuilder message = new StringBuilder();
            message.AppendLine(openingString);

            int numberOfFriends = 0;
            foreach (FriendRateupData friendRateups in FriendRateups)
            {
                numberOfFriends++;
                if (numberOfFriends > MaxFriendsInReturnedString)
                {
                    break;
                }

                StringBuilder friendString = new StringBuilder();
                friendString.AppendLine(friendRateups.FriendName);
                friendString.AppendLine("=======" + Environment.NewLine);

                foreach (var (area, items) in friendRateups.ItemsByArea.Select(iba => (iba.Key, iba.Value)))
                {
                    friendString.Append(area + " :: ");
                    friendString.AppendLine(string.Join(", ", items));
                }
                friendString.AppendLine();

                if (message.Length + friendString.Length + closingString.Length > 2000)
                {
                    break;
                }
                message.Append(friendString);
            }
            message.Append(closingString);

            return message.ToString();
        }
    }
}
