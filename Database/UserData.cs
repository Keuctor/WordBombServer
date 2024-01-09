using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordBombServer.Database
{
    public class UserData
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public short AvatarID { get; set; }
        public float Experience { get; set; }
        public short WinCount { get; set; }
        public int EmeraldCount { get; set; }
        public int CoinCount { get; set; }
        public string UnlockedAvatars { get; set; }
    }
}
