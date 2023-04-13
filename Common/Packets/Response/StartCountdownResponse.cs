

namespace WordBombServer.Common.Packets.Response
{
    public class StartCountdownResponse
    {
        public int Countdown { get; set; }
        public string FirstWordPart { get; set; }
        public byte TargetLength { get; set; }
        public int Timer { get; set; }
        public int[] OrderOfPlayers { get; set; }
    }
}
