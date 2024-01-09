
namespace WordBombServer.Common.Packets.Response
{
    public class CreateRoomResponse
    {
        public string RoomTitle { get; set; }
        public byte GameMode { get; set; }
        public byte GameType { get; set; }
        public byte GameLanguage { get; set; }
        public byte GameSpeed { get; set; }
        public bool IsPrivate { get; set; }
        public string RoomCode { get; set; }
    }
}
