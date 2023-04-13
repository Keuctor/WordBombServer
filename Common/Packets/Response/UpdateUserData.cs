

namespace WordBombServer.Common.Packets.Response
{
    public class UpdateUserData
    {
        public int Id { get; set; }
        public byte Coin { get; set; }
        public byte Emerald { get; set; }
        public short XP { get; set; }
    }
}
