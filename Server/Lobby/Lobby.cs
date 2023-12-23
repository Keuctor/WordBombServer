
using LiteNetLib;
using System.Diagnostics;
using System.Net;
using WordBombServer.Common;
using WordBombServer.Common.Packets.Response;

namespace WordBombServer.Server.Lobby
{
    public class MatchProperties
    {
        public bool MatchStarted { get; set; }
        public bool MatchEnded { get; set; }

        public Dictionary<int, byte> PlayerHealths = new Dictionary<int, byte>();
        public List<string> MatchedWords = new List<string>();
        public float Time { get; set; }
        public int CurrentMaxTime { get; set; }

        public float CountDown = 3;

        public string MatchWord;

        public string TargetWord;

        public byte TargetLength;

        public List<Player> MatchPlayers = new List<Player>(8);

        public int CurrentPlayerIndex;
    }
    public class Lobby
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Code { get;  set; }
        public byte Mode { get; set; }
        public byte Speed { get; set; }
        public byte Language { get; set; }

        public List<IPAddress> KickedPlayerList = new List<IPAddress>();
        public bool IsPrivate { get; set; }
        public NetPeer Host { get; set; }
        public bool Solo { get; set; }

        public List<Player> Players = new List<Player>(8);

        public int SpeedUpCounter = 4;

        public short Round;

        public string LastSentText;

        private static string Chars = "ABCDEFGHJKLMNOPRSTUVXYZ";
        private static Random random = new Random();

        public MatchProperties Properties;
        public int MinSpeed = 4;


        public Lobby(string name)
        {
            this.Id = Guid.NewGuid().ToString("N");
            this.Code = random.Next(1000, 9999).ToString() + Chars[random.Next(Chars.Length)];
            this.Name = name;
            Properties = new MatchProperties();
        }


        public MatchProperties StartMatch()
        {
            this.Properties.MatchPlayers = Players.OrderBy(t => Guid.NewGuid()).ToList();
            this.Solo = this.Properties.MatchPlayers.Count == 1;
            this.Properties.MatchWord = "";
            this.Properties.CurrentPlayerIndex = 0;
            var at = this.Mode == 2 ? 4 : 0;
            this.Properties.CurrentMaxTime = (this.Speed == 0 ? 20 : this.Speed == 1 ? 16 : 12) + at;
            this.Properties.Time = this.Properties.CurrentMaxTime;
            this.Properties.PlayerHealths = new Dictionary<int, byte>();
            this.Properties.CountDown = 3;
            this.Properties.MatchedWords.Clear();
            SpeedUpCounter = 6;
            Round = 1;

            if (this.Speed == 0)
            {
                MinSpeed = 6;
            }
            else if (this.Speed == 1)
            {
                MinSpeed = 5;
            }
            else if (this.Speed == 2)
            {
                MinSpeed = 4;
            }

            if (Players.Any(t => t.IsMobile))
            {
                MinSpeed++;
            }

            if (Solo) {
                MinSpeed--;
            }

            foreach (var p in this.Players)
            {
                p.IsDead = false;
                this.Properties.PlayerHealths.Add(p.Id, 2);
            }
            this.Properties.MatchStarted = true;
            this.Properties.MatchEnded = false;
            return this.Properties;
        }

        public void NextPlayer(bool speedUp)
        {
            if (speedUp)
            {
                if (SpeedUpCounter == 0)
                {
                    if (Properties.CurrentMaxTime > MinSpeed)
                    {
                        Properties.CurrentMaxTime--;
                    }
                    SpeedUpCounter = 4;
                }
                SpeedUpCounter--;
            }
            else
            {
                if (Properties.CurrentMaxTime < 7)
                {
                    Properties.CurrentMaxTime++;
                    SpeedUpCounter = 4;
                }
            }

            Properties.Time = Properties.CurrentMaxTime;
            do
            {
                Properties.CurrentPlayerIndex = (Properties.CurrentPlayerIndex + 1) % Properties.MatchPlayers.Count;
                if (Properties.CurrentPlayerIndex == 0)
                {
                    Round++;
                }
            }
            while (Properties.MatchPlayers[Properties.CurrentPlayerIndex].IsDead);
        }

        public void Tick()
        {
            if (Properties.CountDown == 0)
            {
                Properties.Time -= 0.25f;
                
                if (Solo && Properties.MatchPlayers.TrueForAll(t => t.IsDead) ||
                    (!Solo && Properties.MatchPlayers.Count(t => !t.IsDead) <= 1))
                {
                    Properties.MatchEnded = true;
                    return;
                }
                if (Properties.Time < 0)
                {
                    NextPlayer(false);
                }
            }
            else
            {
                Properties.CountDown -= 0.25f;
            }
        }
    }
}
