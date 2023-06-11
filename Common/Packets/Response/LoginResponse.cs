

namespace WordBombServer.Common.Packets.Response
{
    public class LoginResponse
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public int CoinCount { get; set; }
        public int EmeraldCount { get; set; }
        public int CrownCount { get; set; }
        public float Experience { get; set; }
        public short AvatarId { get; set; }
        public string UnlockedAvatars { get; set; }
    }
}

