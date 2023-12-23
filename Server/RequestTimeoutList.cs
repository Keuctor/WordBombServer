using LiteNetLib;
using System.Net;
using WordBombServer.Common.Packets.Request;

namespace WordBombServer.Server
{


    public class RequestTimeoutList
    {
        private Dictionary<Type, int> timeOutList = new Dictionary<Type, int>();
        public static event Action<Type, NetPeer> OnFail;

        public Dictionary<IPAddress, Dictionary<Type, int>> Requests;
        public RequestTimeoutList()
        {
            Requests = new Dictionary<IPAddress, Dictionary<Type, int>>();
            timeOutList.Add(typeof(ChangeRoomSettingRequest), 600);
            timeOutList.Add(typeof(ChatMessageRequest), 1000);
            timeOutList.Add(typeof(CheatCodeRequest), 2000);
            timeOutList.Add(typeof(CreateRoomRequest), 3000);
            timeOutList.Add(typeof(JoinRoomRequest), 2000);
            timeOutList.Add(typeof(KickPlayerRequest), 1000);
            timeOutList.Add(typeof(LeaderboardRequest), 2000);
            timeOutList.Add(typeof(LeaveRoomRequest), 2000);
            timeOutList.Add(typeof(LoginRequest), 2000);
            timeOutList.Add(typeof(QuickGameRequest), 2000);
            timeOutList.Add(typeof(GiftPlayerRequest), 1000);
            timeOutList.Add(typeof(RegisterRequest), 5000);
            timeOutList.Add(typeof(SuggestWordRequest), 1000);
            timeOutList.Add(typeof(UnlockAvatarRequest), 2000);
            timeOutList.Add(typeof(UpdateDisplayNameRequest), 2000);
            timeOutList.Add(typeof(UpdatePlayerInfoRequest), 1000);
            timeOutList.Add(typeof(GameStartRequest), 5000);
            timeOutList.Add(typeof(LogoutRequest), 1000);
        }
        public bool AddType(Type type, NetPeer peer)
        {
            if (Requests.TryGetValue(peer.EndPoint.Address, out var result))
            {
                if (result.TryGetValue(type, out _))
                {
                    OnFail?.Invoke(type, peer);
                    return false;
                }

                if (!timeOutList.ContainsKey(type)) {
                    return true;
                }
                result.Add(type, timeOutList[type]);
            }
            else
            {
                Requests.Add(peer.EndPoint.Address, new Dictionary<Type, int>() {
                    { type,timeOutList[type]}
                });
            }
            return true;
        }

        public void Tick()
        {
            foreach (KeyValuePair<IPAddress, Dictionary<Type, int>> netPeerDictionary in Requests)
            {
                var adress = netPeerDictionary.Key;
                var dictionary = netPeerDictionary.Value;
                foreach (KeyValuePair<Type, int> typeInt in dictionary)
                {
                    dictionary[typeInt.Key] = typeInt.Value - 250;
                    if (dictionary[typeInt.Key] < 0)
                    {
                        dictionary.Remove(typeInt.Key);
                    }
                }
            }
        }
    }
}
