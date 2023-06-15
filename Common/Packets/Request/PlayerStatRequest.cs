

namespace WordBombServer.Common.Packets.Request
{
    public class PlayerStatRequest
    {
        public bool CreateRoom { get; set; }
        public bool JoinRoom { get; set; }
        public bool UnlockBox { get; set; }
        public bool ClaimedDailyBonus { get; set; }
    }
}
