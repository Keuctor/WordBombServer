using LiteNetLib.Utils;

namespace WordBombServer.Common
{
    public class LobbyInfo : INetSerializable
    {
        public string Title { get; set; }
        public string Code { get; set; }
        public int PlayerCount { get; set; }
        public byte Language { get; set; }
        public byte Mode { get; set; }
        public byte GameType { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Title = reader.GetString();
            Code = reader.GetString();
            PlayerCount = reader.GetInt();
            Language = reader.GetByte();
            Mode = reader.GetByte();
            GameType = reader.GetByte();
        }
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Title);
            writer.Put(Code);
            writer.Put(PlayerCount);
            writer.Put(Language);
            writer.Put(Mode);
            writer.Put(GameType);
        }
    }
}
