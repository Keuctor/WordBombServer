using LiteNetLib;
using LiteNetLib.Utils;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using WordBombServer.Common;
using WordBombServer.Common.Packets.Request;
using WordBombServer.Common.Packets.Response;
using WordBombServer.Database;
using WordBombServer.Server.Lobby;

namespace WordBombServer.Server
{
    public class WordBomb : INetEventListener
    {
        public const int CHANGE_NAME_PRICE = 10;

        public Dictionary<int, NetPeer> Players
         = new Dictionary<int, NetPeer>();

        public Dictionary<int, string> LoggedInUsers = new Dictionary<int, string>();

        private NetManager _netManager;
        private readonly NetPacketProcessor _netPacketProcessor;
        public int MaxConnection { get; }
        public int Port { get; }

        public LobbyRequestHandler lobbyRequestHandler;

        public WordProvider WordProvider;
        public UserContext UserContext;
        public UserCodeContext UseCodeContext;

        public WordBomb(int maxConnection, int port)
        {
            UseCodeContext = new UserCodeContext();
            UserContext = new UserContext();
            WordProvider = new WordProvider();
            WordProvider.LoadWords();

            this.MaxConnection = maxConnection;
            this.Port = port;
            _netPacketProcessor = new NetPacketProcessor();
            lobbyRequestHandler = new LobbyRequestHandler(this, _netPacketProcessor);
            _netPacketProcessor.SubscribeReusable<SuggestWordRequest, NetPeer>(SuggestWord);
            _netPacketProcessor.SubscribeReusable<RegisterRequest, NetPeer>(RegisterUser);
            _netPacketProcessor.SubscribeReusable<LoginRequest, NetPeer>(LoginUser);
            _netPacketProcessor.SubscribeReusable<UpdateDisplayNameRequest, NetPeer>(UpdatePlayer);
            _netPacketProcessor.SubscribeReusable<CheatCodeRequest, NetPeer>(EnterCheatCode);
            _netPacketProcessor.SubscribeReusable<LogoutRequest, NetPeer>(LogoutUser);

            _netManager = new NetManager(this);
        }

        private void LogoutUser(LogoutRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;
            if (LoggedInUsers.TryGetValue(peer.Id, out var userName))
            {
                LoggedInUsers.Remove(peer.Id);
                SendPacket(peer, new LogoutResponse());
            }
            else { 
              lobbyRequestHandler.ErrorResponse(peer, "NOT_LOGGED_IN");
            }
        }

        private void EnterCheatCode(CheatCodeRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            if (LoggedInUsers.TryGetValue(peer.Id, out var userName))
            {
                var user = UserContext.GetUser(userName);

                var code = request.Code.ToUpper();
                if (BonusCodes.Codes.Contains(code))
                {
                    if (UseCodeContext.UseCode(user.Id, code))
                    {
                        user.EmeraldCount += 50;
                        SendPacket(peer, new UpdateUserData()
                        {
                            Id = peer.Id,
                            Emerald = 50,
                        });
                        lobbyRequestHandler.ErrorResponse(peer, "CODE_USED");
                    }
                    else
                    {
                        lobbyRequestHandler.ErrorResponse(peer, "ALREADY_USED_CODE");
                    }
                }
                else
                {
                    lobbyRequestHandler.ErrorResponse(peer, "CODE_NOT_EXIST");
                }
            }
        }

        private void UpdatePlayer(UpdateDisplayNameRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            if (request.DisplayName.Any(t => !char.IsLetterOrDigit(t))
             || request.DisplayName.Length < 3 || request.DisplayName.Length > 20)
            {
                lobbyRequestHandler.ErrorResponse(peer, "USERNAME_LENGTH_ERROR");
                return;
            }

            if (LoggedInUsers.TryGetValue(peer.Id, out var userName))
            {
                var user = UserContext.GetUser(userName);
                if (user.Name != request.DisplayName)
                {
                    if (user.EmeraldCount < CHANGE_NAME_PRICE)
                    {
                        lobbyRequestHandler.ErrorResponse(peer, "NOT_ENOUGH_EMERALD");
                        return;
                    }
                    user.EmeraldCount -= CHANGE_NAME_PRICE;
                }

                user.DisplayName = request.DisplayName;

                var displayNameResponse = new UpdateDisplayNameResponse()
                {
                    DisplayName = request.DisplayName,
                    EmeraldCount = user.EmeraldCount
                };

                SendPacket(peer, displayNameResponse);
            }
            else
            {
                lobbyRequestHandler.ErrorResponse(peer, "NOT_LOGGED_IN");
            }
        }

        private void LoginUser(LoginRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            if (request.UserName.Any(t => !char.IsLetterOrDigit(t))
             || request.UserName.Length < 3 || request.UserName.Length > 20)
            {
                lobbyRequestHandler.ErrorResponse(peer, "USERNAME_LENGTH_ERROR");
                return;
            }

            if (!UserContext.HasUser(request.UserName))
            {
                lobbyRequestHandler.ErrorResponse(peer, "USERNAME_NOT_EXIST");
                return;
            }

            var user = UserContext.GetUser(request.UserName);
            if (user.Password == request.Password)
            {
                var response = new LoginResponse()
                {
                    AvatarId = user.AvatarID,
                    CoinCount = user.CoinCount,
                    CrownCount = user.WinCount,
                    EmeraldCount = user.EmeraldCount,
                    Experience = user.Experience,
                    UserName = user.Name,
                    DisplayName = user.DisplayName
                };
                UserLogin(response, peer);
            }
            else
            {
                lobbyRequestHandler.ErrorResponse(peer, "INCORRECT_PASSWORD");
            }
        }

        public void UserLogin(LoginResponse response, NetPeer peer)
        {
            foreach (var p in LoggedInUsers)
            {
                if (p.Value == response.UserName)
                {
                    lobbyRequestHandler.ErrorResponse(peer, "ALREADY_SIGNED");
                    return;
                }
            }

            LoggedInUsers.Add(peer.Id, response.UserName);
            SendPacket(peer, response);
        }

        private void RegisterUser(RegisterRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            if (UserContext.HasUser(request.UserName))
            {
                lobbyRequestHandler.ErrorResponse(peer, "USERNAME_ALREADY_EXIST");
                return;
            }
            if (request.UserName.Any(t => !char.IsLetterOrDigit(t))
                || request.UserName.Length < 3 || request.UserName.Length > 20)
            {
                lobbyRequestHandler.ErrorResponse(peer, "USERNAME_LENGTH_ERROR");
                return;
            }

            var id = UserContext.GenerateUserId();
            var user = new UserData()
            {
                Id = id,
                Name = request.UserName,
                DisplayName = request.UserName,
                Password = request.Password,
                AvatarID = request.AvatarId,
                Experience = 0,
                WinCount = 0,
                EmeraldCount = 0,
                CoinCount = 0,
            };

            UserContext.AddUser(user);

            var response = new LoginResponse()
            {
                AvatarId = user.AvatarID,
                CoinCount = user.CoinCount,
                CrownCount = user.WinCount,
                EmeraldCount = user.EmeraldCount,
                Experience = user.Experience,
                UserName = user.Name,
                DisplayName = user.DisplayName
            };

            UserLogin(response, peer);
        }

        private void SuggestWord(SuggestWordRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            if (CanSuggest(request.Language, request.Word))
            {
                lobbyRequestHandler.ErrorResponse(peer, "SUGGESTED_WORD_ADDED_FOR_VERIFY");
                AddSuggestion(request.Language, request.Word);
            }
            else
            {
                lobbyRequestHandler.ErrorResponse(peer, "SUGGESTED_WORD_ALREADY_EXIST");
            }
        }

        public bool CanSuggest(byte language, string word)
        {
            if (string.IsNullOrEmpty(word))
                return false;

            if (word.Length < 2 || word.Length > 20)
                return false;

            if (WordProvider.HasWord(language, word))
                return false;

            return true;
        }

        public void AddSuggestion(byte language, string word)
        {
            if (CanSuggest(language, word))
            {
                WordProvider.WriteSuggestion(language, word);
            }
        }

        public void SendPacket<T>(NetPeer peer, T packet) where T : class, new()
        {
            try
            {
                peer.Send(_netPacketProcessor.Write(packet), DeliveryMethod.ReliableOrdered);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.ToString());
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public void StartServer()
        {
            SetupPacketProcessor();
            _netManager.Start(Port);
        }

        private void SetupPacketProcessor()
        {
            _netPacketProcessor.RegisterNestedType(() =>
            {
                return new Player();
            });

            _netPacketProcessor.RegisterNestedType(() =>
            {
                return new LobbyInfo();
            });

            _netPacketProcessor.RegisterNestedType(() =>
            {
                return new LeaderboardData();
            });
        }

        public void ServerTick()
        {
            _netManager.PollEvents();
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            if (_netManager.ConnectedPeersCount > MaxConnection)
                request.Reject();

            Console.WriteLine($"{_netManager.ConnectedPeersCount} / {MaxConnection} | {request.Data.PeekString()}");
            request.AcceptIfKey(GameInfo.VERSION);
        }


        public void OnPeerConnected(NetPeer peer)
        {
            Players.Add(peer.Id, peer);
            peer.Send(_netPacketProcessor.Write(new PlayerConnectionResponse()
            {
                Id = peer.Id
            }), DeliveryMethod.ReliableOrdered);

            Console.WriteLine("We got connection sent Id:{1} / {0}", peer.EndPoint, peer.Id);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (LoggedInUsers.TryGetValue(peer.Id, out var _))
            {
                LoggedInUsers.Remove(peer.Id);
            }

            lobbyRequestHandler.TryLeaveLobby(peer);
            Players.Remove(peer.Id);
            Console.WriteLine($"Peer disconnected: {peer.EndPoint} reason: {0}", disconnectInfo.Reason);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            try
            {
                _netPacketProcessor.ReadAllPackets(reader, peer);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.ToString());
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            Console.WriteLine("OnNetworkReceiveUnconnected " + messageType);
        }
        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Console.WriteLine("OnNetworkError " + socketError);
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            //Console.WriteLine("OnNetworkLatencyUpdate " + latency);
        }
    }
}
