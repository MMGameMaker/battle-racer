using Best.SignalR;
using Best.SignalR.Encoders;
using Cysharp.Threading.Tasks;
using MyBox;
using System;
using UnityEngine;

public class ZenHubManager : MonoBehaviour
{
    public static ConnectionStates HUBState => instance.hub == null ? ConnectionStates.Initial : instance.hub.State;

    private HubConnection hub;
    private static ZenHubManager instance;
    private UniTaskCompletionSource utcsConnect;
    void Awake()
    {
        instance = this;
        this.RegisterListener((int)EventID.OnDisconnected, OnDisconnected);
    }
    private void Start()
    {
        //if(ZenUtls.IsTestnet)
        //    Best.HTTP.Shared.HTTPManager.Logger.Level = Best.HTTP.Shared.Logger.Loglevels.All;
    }
    private void OnDestroy()
    {
        if (hub != null)
        {
            hub.OnConnected -= Hub_OnConnected;
            hub.StartClose();
        }
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnDisconnected, OnDisconnected);
    }

    private void OnDisconnected(object obj)
    {
        ZenUtls.AccessToken = string.Empty;
        if (hub != null)
        {
            hub.OnConnected -= Hub_OnConnected;
            hub.StartClose();
            hub = null;
        }
    }

    public static ConnectionStates State
    {
        get { return instance.hub.State; }
    }

    public static async UniTask ConnectAsync(string accessToken)
    {
        if (instance.hub != null)
        {
            instance.hub.OnConnected -= Hub_OnConnected;
            instance.hub.StartClose();
            instance.hub = null;
        }

        instance.hub = new HubConnection(
            new Uri($"{ZenUtls.URLHub}/hubs/game?access_token={accessToken}"), 
            new JsonProtocol(new JsonDotNetEncoder()),
            new HubOptions {
                SkipNegotiation = false,
                PreferedTransport = TransportTypes.WebSocket,
                PingInterval = TimeSpan.FromSeconds(15),
                PingTimeoutInterval = TimeSpan.FromSeconds(90),
                MaxRedirects = 100,
                ConnectTimeout = TimeSpan.FromSeconds(60)
            });
        
        instance.hub.OnConnected += Hub_OnConnected;
        instance.hub.On<byte[]>("ReceiveMessage", OnReceiveMessage);

        instance.utcsConnect = new UniTaskCompletionSource();
        await instance.hub.ConnectAsync();
        await instance.utcsConnect.Task;
    }

    private static void OnReceiveMessage(byte[] obj)
    {
        var s = ZenUtls.BytesToObject<SignalRespDTO>(obj);
    }
    private static void Hub_OnConnected(HubConnection obj)
    {
        Debug.Log("Hub is connected!!");
        instance.utcsConnect.TrySetResult();
    }
}
