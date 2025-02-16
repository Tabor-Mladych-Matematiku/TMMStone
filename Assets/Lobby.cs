using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TMMStoneLobby : MonoBehaviour
{
    public static TMMStoneLobby Instance { get; private set; }

    public event EventHandler<LobbyUpdateEventArgs> OnLobbyUpdated;
    public class LobbyUpdateEventArgs
    {
        public Lobby lobby;
    }

    public event EventHandler<LobbyEventArgs> OnLobbyListChanged;
    public class LobbyEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    private Lobby hostLobby;
    private Lobby jl;
    private Lobby joinedLobby
    {
        get => jl; set
        {
            jl = value;
            OnLobbyUpdated?.Invoke(this, new() { lobby = jl });
        }
    }
    private float heartbeattimer;
    private float lobbyupdatetimer;
    private string PlayerName { get => LobbyUI.Instance.PlayerName; }
    private const string KEY_START_GAME = nameof(KEY_START_GAME);
    private void Start()
    {
        //Authenticate();
    }
    public async void Authenticate()
    {
        InitializationOptions opts = new();
        opts.SetProfile(PlayerName);
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in!");
            ListLobbies();
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        HandleLobbyHeartbeat();
        UpdateLobbyData();
    }
    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeattimer -= Time.deltaTime;
            if (heartbeattimer < 0f)
            {
                heartbeattimer = 20f;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId) return true;
            }
        }
        return false;
    }
    private async void UpdateLobbyData()
    {
        if (joinedLobby != null)
        {
            lobbyupdatetimer -= Time.deltaTime;
            if (lobbyupdatetimer < 0f)
            {
                lobbyupdatetimer = 1.1f;
                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                if (!IsPlayerInLobby())
                {
                    joinedLobby = null;
                }
                else if (joinedLobby.Data[KEY_START_GAME].Value != "0")
                {
                    if (!IsLobbyHost())
                    {
                        TMMStoneRelay.Instance.JoinRelay(joinedLobby.Data[KEY_START_GAME].Value);
                        LobbyUI.Instance.Hide();
                    }
                    joinedLobby = null;
                }
            }
        }
    }
    public async void CreateLobby(bool Private, string lobbyName)
    {
        try
        {
            CreateLobbyOptions opts = new()
            {
                IsPrivate = Private,
                Player = GetPlayer(),
                Data = new()
                {
                    {KEY_START_GAME,new(DataObject.VisibilityOptions.Member,"0") }
                }
            };
            hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 2, opts);
            joinedLobby = hostLobby;
            Debug.Log("Hosting lobby: " + hostLobby.Name + " Id: " + hostLobby.Id + " Code: " + hostLobby.LobbyCode);
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new()
            {
                Filters = new()
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT)
                }
            };

            QueryResponse query = await Lobbies.Instance.QueryLobbiesAsync(options);
            OnLobbyListChanged?.Invoke(this, new LobbyEventArgs { lobbyList = query.Results });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void JoinLobby(string LobbyId)
    {
        try
        {
            joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(LobbyId, new()
            {
                Player = GetPlayer()
            });
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void JoinLobbybyCode(string code)
    {
        try
        {
            joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code
                , new()
                {
                    Player = GetPlayer()
                }
                );
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void QuickJoin(string LobbyId)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(new()
            {
                Player = GetPlayer()
            });
            Debug.Log("QuickJoined lobby!");
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>()
                    {
                        { "PlayerName",new(PlayerDataObject.VisibilityOptions.Member,PlayerName) }
                    }
        };
    }
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Lobby " + lobby.Name + " PlayerCount: " + lobby.Players.Count);
        foreach (var player in lobby.Players)
        {
            Debug.Log(player.Id);
        }
    }
    public async void UpdateLobby(UpdateLobbyOptions newopts)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, newopts);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            hostLobby = null;
            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public bool IsLobbyHost() => joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;

    public async void StartGame()
    {
        if (IsLobbyHost())
        {
            if (joinedLobby.Players.Count == joinedLobby.MaxPlayers) { 
                try
                {
                    string relaycode = await TMMStoneRelay.Instance.CreateRelay();
                    Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new()
                    {
                        Data = new() {
                        {KEY_START_GAME,new (DataObject.VisibilityOptions.Member,relaycode) }
                    }
                    });
                    joinedLobby = lobby;
                    LobbyUI.Instance.Hide();
                    JoinedLobbyUI.Instance.Hide();
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                }
            }
            else
            {
                Debug.Log("Not Enough players to start a game");//TODO: report in UI
            }
        }
    }
}

