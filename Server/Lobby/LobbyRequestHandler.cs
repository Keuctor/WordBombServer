using LiteNetLib;
using LiteNetLib.Utils;
using System.Globalization;
using WordBombServer.Common;
using WordBombServer.Common.Packets.Request;
using WordBombServer.Common.Packets.Response;

namespace WordBombServer.Server.Lobby
{
    public class LobbyRequestHandler
    {
        private Dictionary<string, Lobby> lobbies = new Dictionary<string, Lobby>();
        private Dictionary<int, string> playersInLobbies = new Dictionary<int, string>();

        public List<Lobby> LobbiesList = new List<Lobby>();

        private WordBomb wordBomb;

        public LobbyRequestHandler(WordBomb wordBomb, NetPacketProcessor processor)
        {
            this.wordBomb = wordBomb;
            RequestTimeoutList.OnFail += OnFailTimeout;

            processor.SubscribeReusable<CreateRoomRequest, NetPeer>(CreateLobby);
            processor.SubscribeReusable<LeaveRoomRequest, NetPeer>(LeaveRoom);
            processor.SubscribeReusable<JoinRoomRequest, NetPeer>(JoinRoom);
            processor.SubscribeReusable<ChangeRoomSettingRequest, NetPeer>(ChangeRoomSetting);
            processor.SubscribeReusable<ChatMessageRequest, NetPeer>(SendChatMessage);
            processor.SubscribeReusable<KickPlayerRequest, NetPeer>(KickPlayer);
            processor.SubscribeReusable<UpdatePlayerInfoRequest, NetPeer>(UpdatePlayerInfo);
            processor.SubscribeReusable<GameStartRequest, NetPeer>(StartRoom);
            processor.SubscribeReusable<LoadingCompleteRequest, NetPeer>(SetLoadingComplete);
            processor.SubscribeReusable<WordChangeRequest, NetPeer>(WordChanged);
            processor.SubscribeReusable<SubmitWordRequest, NetPeer>(SubmitWord);
            processor.SubscribeReusable<GiftPlayerRequest, NetPeer>(GiftPlayer);
            processor.SubscribeReusable<GetLobbiesRequest, NetPeer>(GetLobbies);
            processor.SubscribeReusable<QuickGameRequest, NetPeer>(QuickLobby);
            processor.SubscribeReusable<UnlockAvatarRequest, NetPeer>(UnlockAvatar);
            processor.SubscribeReusable<LeaderboardRequest, NetPeer>(GetLeaderboard);
        }

        private void OnFailTimeout(Type obj, NetPeer peer)
        {
            ErrorResponse(peer, "REQUEST_SPAM_PROTECTION");
        }

        private void GetLeaderboard(LeaderboardRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            if (wordBomb.LoggedInUsers.TryGetValue(peer.Id, out var userName))
            {
                var leaderboardPlayers = wordBomb.UserContext.Users.OrderByDescending(t => t.CoinCount).Take(16);
                var response = new LeaderboardResponse();
                response.LeaderboardData = new List<LeaderboardData>();
                foreach (var p in leaderboardPlayers)
                {
                    response.LeaderboardData.Add(new LeaderboardData()
                    {
                        AvatarID = p.AvatarID,
                        CoinCount = p.CoinCount,
                        DisplayName = p.DisplayName,
                        WinCount = p.WinCount
                    });
                }
                wordBomb.SendPacket(peer, response);
            }
            else
            {
                ErrorResponse(peer, "ERROR_NOT_LOGGED_IN");
            }
        }

        private void UnlockAvatar(UnlockAvatarRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            if (wordBomb.LoggedInUsers.TryGetValue(peer.Id, out var userName))
            {
                var user = wordBomb.UserContext.GetUser(userName);
                if (user.EmeraldCount >= request.Price)
                {
                    user.EmeraldCount -= request.Price;
                    var response = new UnlockAvatarResponse()
                    {
                        EmeraldCount = user.EmeraldCount
                    };
                    wordBomb.SendPacket(peer, response);
                }
                else
                {
                    wordBomb.lobbyRequestHandler.ErrorResponse(peer, "NOT_ENOUGH_EMERALD");
                }
            }
        }

        private void QuickLobby(QuickGameRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            var response = new QuickGameResponse();
            var preffered = request.Language;

            var lobbies = GetLobbies();
            var quickLobby = lobbies.FirstOrDefault(t => t.Language == preffered);
            if (quickLobby != null)
            {
                response.RoomCode = quickLobby.Code;
                wordBomb.SendPacket(peer, response);
            }
            else
            {
                quickLobby = lobbies.FirstOrDefault();
                if (quickLobby != null)
                {
                    response.RoomCode = quickLobby.Code;
                    wordBomb.SendPacket(peer, response);
                }
                else
                {
                    wordBomb.SendPacket(peer, response);
                }
            }
        }

        public LobbyInfo[] GetLobbies(int max = 50)
        {
            var notPrivateLobbies = lobbies.Where(t => !t.Value.IsPrivate)
                .OrderByDescending(T => T.Value.Players.Count)
                .ToArray();

            var lobbyInfos = new LobbyInfo[Math.Min(max, notPrivateLobbies.Length)];
            for (int i = 0; i < notPrivateLobbies.Length; i++)
            {
                if (i > 49)
                    break;

                var lobby = notPrivateLobbies[i];
                lobbyInfos[i] = new LobbyInfo()
                {
                    Title = lobby.Value.Name,
                    Code = lobby.Value.Code,
                    Language = lobby.Value.Language,
                    PlayerCount = lobby.Value.Players.Count,
                    Mode = lobby.Value.Mode
                };
            }
            return lobbyInfos;
        }

        private void GetLobbies(GetLobbiesRequest request, NetPeer peer)
        {
            var query = new LobbiesQueryResponse();
            query.Lobbies = GetLobbies();
            wordBomb.SendPacket(peer, query);
        }

        private void GiftPlayer(GiftPlayerRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            if (playersInLobbies.TryGetValue(peer.Id, out string code))
            {
                if (lobbies.TryGetValue(code, out Lobby lobby))
                {
                    var p = lobby.Players.SingleOrDefault(t => t.Id == request.TargetId);
                    if (p != null)
                    {
                        var response = new GiftPlayerResponse()
                        {
                            Target = request.TargetId,
                            Sender = peer.Id,
                        };
                        wordBomb.SendPacket(p.Peer, response);
                    }
                }
            }
        }

        public void SubmitWord(SubmitWordRequest submitWordRequest, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(submitWordRequest.GetType(), peer))
                return;

            if (playersInLobbies.TryGetValue(peer.Id, out string code))
            {
                if (lobbies.TryGetValue(code, out Lobby lobby))
                {
                    Player player = lobby.Properties.MatchPlayers[lobby.Properties.CurrentPlayerIndex];
                    if (player.Id == peer.Id)
                    {
                        var guess = submitWordRequest.Word;
                        if (string.IsNullOrEmpty(guess))
                            return;

                        lobby.Properties.MatchWord = "";
                        var submitWordResponse = new SubmitWordResponse();
                        submitWordResponse.SenderId = peer.Id;
                        submitWordResponse.Word = guess.Substring(0, Math.Min(guess.Length, 50));

                        bool giveXp = false;


                        if (lobby.Mode == 1 && !guess.StartsWith(lobby.Properties.TargetWord))
                        {
                            if (player.EmeraldCounter > 0)
                            {
                                player.EmeraldCounter--;
                            }
                            player.Combo = 1;
                            submitWordResponse.FailType = 1;
                        }
                        else
                        if (lobby.Mode == 2 && lobby.Properties.TargetLength != guess.Length)
                        {
                            if (player.EmeraldCounter > 0)
                            {
                                player.EmeraldCounter--;
                            }

                            player.Combo = 1;
                            submitWordResponse.FailType = 3;
                        }
                        else if (lobby.Properties.MatchedWords.Contains(guess))
                        {
                            if (player.EmeraldCounter > 0)
                            {
                                player.EmeraldCounter--;
                            }

                            player.Combo = 1;
                            submitWordResponse.FailType = 2;
                        }
                        else if (guess.Contains(lobby.Properties.TargetWord) && wordBomb.WordProvider.HasWord(lobby.Language, guess))
                        {
                            lobby.LastSentText = guess;
                            player.EmeraldCounter++;
                            if (player.EmeraldCounter >= 6)
                            {
                                player.Emerald++;
                                player.EmeraldCounter = 0;
                            }

                            if (guess.Length >= 6)
                                player.Combo++;

                            giveXp = true;

                            lobby.Properties.MatchedWords.Add(guess);
                            lobby.NextPlayer(true);

                            ChangeTurn(lobby, lobby.Properties.CurrentPlayerIndex, lobby.Mode != 1);
                            submitWordResponse.FailType = 0;
                        }
                        else
                        {
                            if (player.EmeraldCounter > 0)
                            {
                                player.EmeraldCounter--;
                            }
                            player.Combo = 1;
                            submitWordResponse.FailType = 1;
                            wordBomb.AddSuggestion(lobby.Language, guess);
                        }

                        submitWordResponse.Id = lobby.Properties.MatchPlayers[lobby.Properties.CurrentPlayerIndex].Id;

                        if (submitWordResponse.FailType == 0)
                        {
                            if (wordBomb.LoggedInUsers.TryGetValue(peer.Id, out var name))
                            {
                                var user = wordBomb.UserContext.GetUser(name);

                                int addedXP = 0;
                                if (giveXp)
                                {
                                    var level = user.Experience / 100;
                                    if (level == 1 || level == 0)
                                    {
                                        addedXP = 4 + guess.Length;
                                    }
                                    else if (level == 2)
                                    {
                                        addedXP = 3 + guess.Length;
                                    }
                                    else if (level == 3)
                                    {
                                        addedXP = 2 + guess.Length;
                                    }
                                    else if (level == 4 || level == 5 || level == 6)
                                    {
                                        addedXP = 6;
                                    }
                                    else if (level == 7 || level == 8 || level == 9)
                                    {
                                        addedXP = 5;
                                    }
                                    else if (level == 10 || level == 11 || level == 12)
                                    {
                                        addedXP = 4;
                                    }
                                    else if (level == 13 || level == 14 || level == 15)
                                    {
                                        addedXP = 3;
                                    }
                                    else if (level == 16 || level == 17 || level == 18)
                                    {
                                        addedXP = 2;
                                    }
                                    else
                                    {
                                        addedXP = 1;
                                    }
                                }

                                var addCoin = guess.Length * player.Combo;

                                if (lobby.Solo)
                                {
                                    if (addCoin >= 2)
                                    {
                                        addCoin = addCoin / 2;
                                    }
                                }

                                var addedEmerald = player.Emerald;
                                player.Emerald = 0;

                                user.Experience += addedXP;
                                user.CoinCount += addCoin;
                                user.EmeraldCount += addedEmerald;

                                var updateUserData = new UpdateUserData()
                                {
                                    Id = peer.Id,
                                    Coin = (byte)addCoin,
                                    Emerald = addedEmerald,
                                    XP = (short)addedXP
                                };

                                foreach (var p in lobby.Players)
                                {
                                    wordBomb.SendPacket(p.Peer, updateUserData);
                                }
                            }
                        }

                        foreach (var p in lobby.Players)
                        {
                            wordBomb.SendPacket(p.Peer, submitWordResponse);
                        }
                    }
                    else
                    {
                        //ErrorResponse(peer, "{NOT_YOUR_TURN}");
                    }
                }
            }
        }
        public void WordChanged(WordChangeRequest arg1, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(arg1.GetType(), peer))
                return;

            if (playersInLobbies.TryGetValue(peer.Id, out string code))
            {
                if (lobbies.TryGetValue(code, out Lobby lobby))
                {
                    if (lobby.Properties.MatchPlayers[lobby.Properties.CurrentPlayerIndex].Id == peer.Id)
                    {
                        lobby.Properties.MatchWord = arg1.Word;
                        var matchWordUpdate = new WordUpdateResponse()
                        {
                            Word = lobby.Properties.MatchWord
                        };
                        foreach (var p in lobby.Players)
                        {
                            if (p.Id != peer.Id)
                            {
                                wordBomb.SendPacket(p.Peer, matchWordUpdate);
                            }
                        }
                    }
                    else
                    {
                        //ErrorResponse(peer, "{NOT_YOUR_TURN}");
                    }
                }
            }
        }

        public void TickLobbies()
        {
            for (int i = 0; i < LobbiesList.Count; i++)
            {
                var lobby = LobbiesList[i];
                if (!lobby.Properties.MatchStarted)
                    continue;

                var currentI = lobby.Properties.CurrentPlayerIndex;
                var currentP = lobby.Properties.MatchPlayers[currentI];
                lobby.Tick();

                if (lobby.Properties.MatchEnded)
                {
                    lobby.Properties.MatchStarted = false;
                    var matchWinnerResponse = new MatchWinnerResponse()
                    {
                        Countdown = 10
                    };

                    if (lobby.Solo)
                    {
                        matchWinnerResponse.Id = lobby.Properties.MatchPlayers[0].Id;
                    }
                    else
                    {
                        matchWinnerResponse.Id = lobby.Properties.MatchPlayers.First(t => !t.IsDead).Id;
                    }


                    if (wordBomb.LoggedInUsers.TryGetValue(matchWinnerResponse.Id, out var userName))
                    {
                        var userDta = wordBomb.UserContext.GetUser(userName);
                        if (lobby.Round >= 10)
                        {
                            userDta.WinCount++;
                        }
                    }

                    foreach (var p in lobby.Players)
                    {
                        p.IsDead = false;
                        p.GameLoaded = false;
                        p.Combo = 1;
                        p.Emerald = 0;
                        p.EmeraldCounter = 0;
                        wordBomb.SendPacket(p.Peer, matchWinnerResponse);
                    }
                    return;
                }

                if (currentI != lobby.Properties.CurrentPlayerIndex || (lobby.Solo && lobby.Properties.Time <= 0))
                {
                    lobby.Properties.MatchWord = "";
                    ChangeTurn(lobby, lobby.Properties.CurrentPlayerIndex, lobby.Solo);
                    DecreaseHealth(lobby, currentP);
                }
            }
        }

        public void DecreaseHealth(Lobby lobby, Player player)
        {
            Console.WriteLine("decrease health " + lobby.Properties.PlayerHealths[player.Id]);
            byte prop = lobby.Properties.PlayerHealths[player.Id];
            prop--;
            if (prop <= 0)
            {
                EliminatePlayer(lobby, player);
            }
            else
            {
                lobby.Properties.PlayerHealths[player.Id] = prop;
                var response = new PlayerDecreaseHealthResponse()
                {
                    Id = player.Id,
                    NewHealth = prop
                };
                foreach (var p in lobby.Players)
                {
                    wordBomb.SendPacket(p.Peer, response);
                }
            }
        }

        public void EliminatePlayer(Lobby lobby, Player player)
        {
            var response = new EliminatePlayerResponse()
            {
                Id = player.Id,
                Reason = 0
            };
            foreach (var p in lobby.Players)
            {
                wordBomb.SendPacket(p.Peer, response);
            }
            player.IsDead = true;
        }

        public void ChangeTurn(Lobby lobby, int index, bool changeWord)
        {
            var nextPlayer = lobby.Properties.MatchPlayers[index];
            var turnChangedResponse = new TurnChangedResponse();
            if (lobby.Mode == 2)
            {
                if (changeWord)
                {
                    var limit = (byte)GetTargetLength(lobby.Round);
                    turnChangedResponse.TargetLength = limit;
                    lobby.Properties.TargetLength = limit;
                }
                else
                {
                    turnChangedResponse.TargetLength = lobby.Properties.TargetLength;
                }
            }
            else
            {
                turnChangedResponse.TargetLength = 0;
                lobby.Properties.TargetLength = 0;
            }

            turnChangedResponse.Id = nextPlayer.Id;
            turnChangedResponse.Index = index;
            turnChangedResponse.Timer = lobby.Properties.CurrentMaxTime;
            turnChangedResponse.Round = lobby.Round;
            if (changeWord)
            {
                lobby.Properties.TargetWord = wordBomb.WordProvider.GetRandomWordPart(lobby.Mode == 2 ? 1 : 2, lobby.Language);
            }
            else
            {
                if (lobby.Mode == 1)
                {
                    if (!string.IsNullOrEmpty(lobby.LastSentText))
                    {
                        var lastText = lobby.LastSentText;
                        lastText = new string(lastText[lastText.Length - 1], 1);
                        if (lastText.Contains("Ğ") || lastText.Contains("J"))
                        {
                            lastText = wordBomb.WordProvider.GetRandomWordPart(2, lobby.Language);
                        }
                        lobby.Properties.TargetWord = lastText;
                    }
                    else
                    {
                        lobby.Properties.TargetWord = wordBomb.WordProvider.GetRandomWordPart(2, lobby.Language);
                    }
                }
            }
            turnChangedResponse.NewWordPart = lobby.Properties.TargetWord;
            foreach (var player in lobby.Players)
            {
                wordBomb.SendPacket(player.Peer, turnChangedResponse);
            }
        }

        /// <summary>
        /// This will start the game when everyone is ready on the room
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void SetLoadingComplete(LoadingCompleteRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            if (peer != null)
            {
                if (playersInLobbies.TryGetValue(peer.Id, out string code))
                {
                    if (lobbies.TryGetValue(code, out Lobby lobby))
                    {
                        var p = lobby.Players.SingleOrDefault(t => t.Id == peer.Id);
                        if (p != null)
                        {
                            p.GameLoaded = true;

                            foreach (var lp in lobby.Players)
                            {
                                wordBomb.SendPacket(lp.Peer, new PlayerLoadedResponse()
                                {
                                    LoadedPlayerCount = (byte)lobby.Players.Count(t => t.GameLoaded),
                                    TotalPlayer = (byte)lobby.Players.Count
                                });
                            }
                        }
                        else
                        {
                            ErrorResponse(peer, "{UNKNOWN_PLAYER#FAILED}");
                        }
                        if (IsLobbyLoadedForAllPlayers(lobby))
                        {
                            StartCountdownForLobby(lobby);
                        }
                    }
                    else
                    {
                        ErrorResponse(peer, "{REPORT#0001}");
                        Console.WriteLine("This error is important {#0001}");
                    }
                }
            }
        }

        public bool IsLobbyLoadedForAllPlayers(Lobby lobby)
        {
            return (lobby.Players.TrueForAll(t => t.GameLoaded));
        }

        public static int Remap(int source, int sourceFrom, int sourceTo, int targetFrom, int targetTo)
        {
            return targetFrom + (source - sourceFrom) * (targetTo - targetFrom) / (sourceTo - sourceFrom);
        }

        public int GetTargetLength(int round)
        {
            return new Random().Next(4, Math.Min(10, Remap(round, 1, 70, 7, 12)));
        }

        private void StartCountdownForLobby(Lobby lobby)
        {
            var properties = lobby.StartMatch();
            var countdownResponse = new StartCountdownResponse()
            {
                Countdown = (int)properties.CountDown,
                FirstWordPart = wordBomb.WordProvider.GetRandomWordPart(lobby.Mode == 2 ? 1 : 2, lobby.Language),
                Timer = properties.CurrentMaxTime,
                TargetLength = lobby.Mode == 2 ? (byte)GetTargetLength(lobby.Round) : (byte)0,
                OrderOfPlayers = lobby.Properties.MatchPlayers.Select(t => t.Id).ToArray()
            };

            lobby.Properties.TargetWord = countdownResponse.FirstWordPart;
            lobby.Properties.TargetLength = countdownResponse.TargetLength;

            foreach (var player in lobby.Players)
            {
                wordBomb.SendPacket(player.Peer, countdownResponse);
            }
        }

        /// <summary>
        /// This will send a start response to all clients to load scene but will wait 
        /// before starting the game for LoadingCompleteRequest
        /// </summary>
        /// <param name="request"></param>
        /// <param name="peer"></param>
        public void StartRoom(GameStartRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            if (playersInLobbies.TryGetValue(peer.Id, out string code))
            {
                if (lobbies.TryGetValue(code, out Lobby lobby))
                {
                    if (lobby.Players.Count > 0)
                    {
                        if (lobby.Host.Id == peer.Id)
                        {
                            var response = new GameStartResponse();
                            foreach (var p in lobby.Players)
                            {
                                p.IsDead = false;
                                p.GameLoaded = false;
                                p.Combo = 1;
                                p.Emerald = 0;
                                p.EmeraldCounter = 0;
                                wordBomb.SendPacket(p.Peer, response);
                            }
                        }
                        else
                        {
                            ErrorResponse(peer, "{YOU_ARE_NOT_HOST}");
                        }
                    }
                    else
                    {
                        ErrorResponse(peer, "MORE_PLAYER_NEEDED");
                    }
                }
            }
        }


        private void UpdatePlayerInfo(UpdatePlayerInfoRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            if (wordBomb.LoggedInUsers.TryGetValue(peer.Id, out string name))
            {
                var user = wordBomb.UserContext.GetUser(name);
                user.AvatarID = (byte)request.AvatarId;
            }

            if (playersInLobbies.TryGetValue(peer.Id, out string code))
            {
                if (lobbies.TryGetValue(code, out Lobby lobby))
                {
                    var player = lobby.Players.FirstOrDefault(t => t.Id == peer.Id);
                    if (player != null)
                    {
                        player.AvatarId = request.AvatarId;

                        var updateResponse = new UpdatePlayerInfoResponse()
                        {
                            Id = player.Id,
                            AvatarId = player.AvatarId,
                            Experience = player.Experience,
                        };
                        foreach (var p in lobby.Players)
                        {
                            if (p.Id != peer.Id)
                            {
                                wordBomb.SendPacket(p.Peer, updateResponse);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("{ERROR.UPDATEPLAYERINFO.NULL}");
                    }
                }
                else
                {
                    playersInLobbies.Remove(peer.Id);
                    ErrorResponse(peer, "{ROOM_DOES_NOT_EXIST}");
                }
            }
        }

        private void KickPlayer(KickPlayerRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            if (playersInLobbies.TryGetValue(peer.Id, out string code))
            {
                if (lobbies.TryGetValue(code, out Lobby lobby))
                {
                    if (peer.Id != lobby.Host.Id)
                    {

                        ErrorResponse(peer, "{YOU_ARE_NOT_HOST}");
                        return;
                    }
                    var kickPlayer = lobby.Players.Find(t => t.Id == request.Id);
                    if (kickPlayer != null)
                    {
                        var response = new KickPlayerResponse()
                        {
                            Id = request.Id
                        };
                        foreach (var p in lobby.Players)
                        {
                            wordBomb.SendPacket(p.Peer, response);
                        }
                        lobby.KickedPlayerList.Add(kickPlayer.Peer.EndPoint.Address);
                        TryLeaveLobby(kickPlayer.Peer);
                    }
                    else
                    {
                        ErrorResponse(peer, "{KICKED_PLAYER_ID_DOES_NOT_EXIST_IN_ROOM}");
                    }
                }
                else
                {
                    playersInLobbies.Remove(peer.Id);
                    ErrorResponse(peer, "{ROOM_DOES_NOT_EXIST}");
                }
            }
            else
            {
                ErrorResponse(peer, "{NOT_IN_ROOM}");
            }
        }

        private void SendChatMessage(ChatMessageRequest msgRequest, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(msgRequest.GetType(), peer))
                return;

            if (msgRequest.Message.Length > 100)
                msgRequest.Message = msgRequest.Message.Substring(0, 100) + "...";

            var msg = msgRequest.Message;

            if (playersInLobbies.TryGetValue(peer.Id, out string code))
            {
                if (lobbies.TryGetValue(code, out Lobby lobby))
                {
                    foreach (var p in lobby.Players)
                    {
                        if (p.Id == peer.Id)
                            continue;

                        var chatMessage = new ChatMessageResponse()
                        {
                            Id = peer.Id,
                            Message = msg
                        };
                        wordBomb.SendPacket(p.Peer, chatMessage);
                    }
                }
                else
                {
                    playersInLobbies.Remove(peer.Id);
                    ErrorResponse(peer, "{ROOM_DOES_NOT_EXIST}");
                }
            }
            else
            {
                ErrorResponse(peer, "{NOT_IN_ROOM}");
            }
        }

        private void ChangeRoomSetting(ChangeRoomSettingRequest settings, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(settings.GetType(), peer))
                return;

            if (playersInLobbies.TryGetValue(peer.Id, out string code))
            {
                if (lobbies.TryGetValue(code, out Lobby lobby))
                {
                    if (peer.Id == lobby.Host.Id)
                    {
                        if (settings.Language == lobby.Language && settings.Mode == lobby.Mode
                             && settings.Speed == lobby.Speed && settings.IsPrivate == lobby.IsPrivate)
                        {
                            ErrorResponse(peer, "{CHANGED_TO_SAME_VALUES}");
                        }
                        else
                        {
                            lobby.Language = settings.Language;
                            lobby.Mode = settings.Mode;
                            lobby.Speed = settings.Speed;
                            lobby.IsPrivate = settings.IsPrivate;

                            var roomSettingsChanged = new RoomSettingsChangedResponse()
                            {
                                NewLanguage = lobby.Language,
                                NewMode = lobby.Mode,
                                NewSpeed = lobby.Speed,
                                NewLobbyIsPrivate = lobby.IsPrivate
                            };

                            foreach (var player in lobby.Players)
                            {
                                wordBomb.SendPacket(player.Peer, roomSettingsChanged);
                            }
                        }
                    }
                    else
                    {
                        ErrorResponse(peer, "{YOU_ARE_NOT_HOST}");
                    }
                }
                else
                {
                    playersInLobbies.Remove(peer.Id);
                    ErrorResponse(peer, "{ROOM_DOES_NOT_EXIST}");
                }
            }
            else
            {
                ErrorResponse(peer, "{CANT_FIND_ROOM_TO_CHANGE_SETTINGS}");
            }
        }

        public void ErrorResponse(NetPeer peer, string error)
        {
            Console.WriteLine("Error Sent:  " + error);
            wordBomb.SendPacket(peer, new FailedOperationResponse()
            {
                Error = error
            });
        }
        private void LeaveRoom(LeaveRoomRequest request, NetPeer peer)
        {
            TryLeaveLobby(peer);
        }

        public void TryLeaveLobby(NetPeer peer)
        {
            if (playersInLobbies.TryGetValue(peer.Id, out string code))
            {
                var lobby = lobbies[code];

                var playersInTheLobby = lobby.Players;
                var p = lobby.Players.First(t => t.Id == peer.Id);

                //if host is leaving the room
                if (lobby.Host == peer)
                {
                    if (lobby.Players.Count > 1)
                    {
                        lobby.Host = lobby.Players.First(t => t.Id != peer.Id).Peer;
                        var hostChangedResponse = new RoomHostChangedResponse()
                        {
                            Id = lobby.Host.Id
                        };
                        foreach (var pleft in playersInTheLobby)
                        {
                            if (pleft.Id != peer.Id)
                            {
                                wordBomb.SendPacket(pleft.Peer, hostChangedResponse);
                            }
                        }
                    }
                }

                lobby.Players.Remove(p);
                playersInLobbies.Remove(peer.Id);

                if (lobby.Players.Count == 0)
                {
                    DeleteLobby(lobby);
                    return;
                }
                if (lobby.Properties.MatchStarted)
                {
                    var inGamePlayer = lobby.Properties.MatchPlayers.SingleOrDefault(t => t.Id == peer.Id);
                    if (inGamePlayer != null)
                    {
                        if (!inGamePlayer.IsDead)
                        {
                            inGamePlayer.IsDead = true;
                            var playerEliminated = new EliminatePlayerResponse()
                            {
                                Id = inGamePlayer.Id,
                                Reason = 1,
                            };
                            foreach (var pleft in playersInTheLobby)
                            {
                                wordBomb.SendPacket(pleft.Peer, playerEliminated);
                            }
                            //if its the turn for quitting player
                            if (inGamePlayer.Id == lobby.Properties.MatchPlayers[lobby.Properties.CurrentPlayerIndex].Id)
                            {
                                lobby.Properties.MatchWord = "";
                                lobby.NextPlayer(false);
                                ChangeTurn(lobby, lobby.Properties.CurrentPlayerIndex, false);
                            }
                        }
                    }
                }

                playersInTheLobby = lobby.Players;
                var playerLeftResponse = new PlayerLeftResponse()
                {
                    Id = peer.Id
                };
                foreach (var pleft in playersInTheLobby)
                {
                    wordBomb.SendPacket(pleft.Peer, playerLeftResponse);
                }
            }
        }


        private void DeleteLobby(Lobby lobby)
        {
            LobbiesList.Remove(lobby);
            lobbies.Remove(lobby.Code);
            Console.WriteLine("Delete lobby " + lobby.Code);
        }

        private void JoinRoom(JoinRoomRequest joinRoom, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(joinRoom.GetType(), peer))
                return;

            joinRoom.RoomCode = joinRoom.RoomCode.ToUpper(CultureInfo.InvariantCulture);

            if (lobbies.TryGetValue(joinRoom.RoomCode, out Lobby lobby))
            {
                if (lobby.KickedPlayerList.Contains(peer.EndPoint.Address))
                {
                    ErrorResponse(peer, "YOU_ARE_KICKED");
                    return;
                }
                if (wordBomb.LoggedInUsers.TryGetValue(peer.Id, out var userName))
                {
                    var joinRoomResponse = new JoinRoomResponse()
                    {
                        GameLanguage = lobby.Language,
                        GameMode = lobby.Mode,
                        HostId = lobby.Host.Id,
                        Players = lobby.Players.ToArray(),
                        RoomCode = lobby.Code,
                        GameSpeed = lobby.Speed,
                        RoomTitle = lobby.Name,
                        IsPrivate = lobby.IsPrivate
                    };

                    var userData = wordBomb.UserContext.GetUser(userName);

                    var player = new Player()
                    {
                        AvatarId = userData.AvatarID,
                        Id = peer.Id,
                        Experience = userData.Experience,
                        CrownCount = userData.WinCount,
                        Peer = peer,
                        RoomCode = lobby.Code,
                        UserName = userData.DisplayName,
                        IsMobile = joinRoom.IsMobile,
                    };

                    lobby.Players.Add(player);
                    wordBomb.SendPacket(peer, joinRoomResponse);
                    playersInLobbies.Add(peer.Id, lobby.Code);

                    var playerJoinedResponse = new PlayerJoinedResponse()
                    {
                        Player = player,
                    };
                    foreach (var p in lobby.Players)
                    {
                        if (p.Id != peer.Id)
                        {
                            wordBomb.SendPacket(p.Peer, playerJoinedResponse);
                        }
                    }
                }
                else
                {
                    ErrorResponse(peer, "ERROR_NOT_LOGGED_IN");
                }
            }
            else
            {
                ErrorResponse(peer, "CANT_FIND_LOBBY");
            }
        }


        public void CreateLobby(CreateRoomRequest request, NetPeer peer)
        {
            if (!Startup.RequestTimer.AddType(request.GetType(), peer))
                return;

            if (wordBomb.LoggedInUsers.TryGetValue(peer.Id, out var userName))
            {
                var userData = wordBomb.UserContext.GetUser(userName);
                if (userData != null)
                {
                    var lobby = new Lobby(userData.DisplayName)
                    {
                        Language = request.GameLanguage,
                        Mode = request.GameMode,
                        Host = peer,
                        IsPrivate = request.IsPrivate,
                        Speed = request.GameSpeed,
                        Players = new List<Player>()
                        {
                            new Player()
                            {
                                AvatarId = userData.AvatarID,
                                Id = peer.Id,
                                Experience = userData.Experience,
                                Peer = peer,
                                UserName = userData.DisplayName,
                                CrownCount = userData.WinCount,
                                IsMobile = request.IsMobile
                            }
                        }
                    };

                    var response = new CreateRoomResponse()
                    {
                        IsPrivate = lobby.IsPrivate,
                        GameLanguage = lobby.Language,
                        GameMode = lobby.Mode,
                        RoomCode = lobby.Code,
                        RoomTitle = lobby.Name,
                        GameSpeed = lobby.Speed
                    };

                    playersInLobbies.Add(peer.Id, lobby.Code);
                    lobbies.Add(lobby.Code, lobby);
                    wordBomb.SendPacket(peer, response);
                    LobbiesList.Add(lobby);
                }
                else
                {
                    ErrorResponse(peer, "ERROR_NOT_LOGGED_IN");
                }
            }

        }
    }
}
