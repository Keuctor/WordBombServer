

namespace WordBombServer.Common.Packets.Request
{
    public class CreateRoomRequest
    {
        public byte GameMode { get; set; }
        public byte GameLanguage { get; set; }
        public byte GameSpeed { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsMobile { get; set; }
    }
}
