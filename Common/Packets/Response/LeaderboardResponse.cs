using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordBombServer.Common.Packets.Response
{
    public class LeaderboardResponse
    {
        public List<LeaderboardData> LeaderboardData { get; set; }
    }
    public class LeaderboardData : INetSerializable
    {
        public string DisplayName { get; set; }
        public short WinCount { get; set; }
        public int CoinCount { get; set; }
        public short AvatarID { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            DisplayName = reader.GetString();
            WinCount = reader.GetShort();
            CoinCount = reader.GetInt();
            AvatarID = reader.GetShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(DisplayName);
            writer.Put(WinCount);
            writer.Put(CoinCount);
            writer.Put(AvatarID);
        }
    }
}
