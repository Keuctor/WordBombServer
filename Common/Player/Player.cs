using LiteNetLib.Utils;
using LiteNetLib;

namespace WordBombServer.Common
{

    public class Player : INetSerializable
    {
        public const int MAX_PLAYER_NAME_LENGTH = 20; 
        public int Id { get; set; }  
        public string RoomCode { get; set; } 
        public string UserName { get; set; } 
        public int CrownCount { get; set; } 
        public short AvatarId { get; set; } 
        public byte Combo { get; set; } 
        public byte Emerald { get; set; } 
        public byte EmeraldCounter { get; set; }
        public NetPeer Peer { get; set; } 
        public bool GameLoaded { get; set; } 
        public bool IsDead { get; set; }
        public bool IsMobile { get; set; }
        public float Experience { get; set; }
        public Player()
        {
            RoomCode = null;
            UserName = null;
            AvatarId = 0;
            Combo = 1;
            GameLoaded = false;
            IsMobile = false;
            Experience = 0;
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetInt();
            UserName = reader.GetString();
            AvatarId = reader.GetShort();
            GameLoaded = reader.GetBool();
            CrownCount = reader.GetInt();
            Experience = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(UserName);
            writer.Put(AvatarId);
            writer.Put(GameLoaded);
            writer.Put(CrownCount);
            writer.Put(Experience);
        }
    }

}
