using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class TMMStoneRelay : MonoBehaviour
{
    public static TMMStoneRelay Instance { get; private set; }
    // Start is called before the first frame update
    /*async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {

        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();


    }*/
    private void Awake()
    {
        Instance = this;
    }
    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new(allocation, "dtls"));

            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (RelayServiceException e) { Debug.Log(e);return null; }
    }
    public async void JoinRelay(string code)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new(joinAllocation, "dtls"));
        }
        catch (RelayServiceException e) { Debug.Log(e); }
    }
}
