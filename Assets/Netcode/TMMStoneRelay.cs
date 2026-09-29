using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
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
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsListening)
        {
            Debug.LogError("Cannot create a Relay host without an idle NetworkManager.");
            return null;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError("The NetworkManager has no UnityTransport component.");
            return null;
        }

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            transport.SetRelayServerData(new(allocation, "dtls"));

            if (!NetworkManager.Singleton.StartHost())
            {
                Debug.LogError("Failed to start the Relay host.");
                return null;
            }

            return joinCode;
        }
        catch (RelayServiceException e) { Debug.Log(e);return null; }
    }
    public async Task<bool> JoinRelay(string code)
    {
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsListening)
        {
            Debug.LogError("Cannot join Relay without an idle NetworkManager.");
            return false;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError("The NetworkManager has no UnityTransport component.");
            return false;
        }

        const int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Debug.Log($"Joining Relay with code {code} (attempt {attempt}/{maxAttempts}).");
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

                transport.SetRelayServerData(new(joinAllocation, "dtls"));
                if (!NetworkManager.Singleton.StartClient())
                {
                    Debug.LogError("Failed to start the Relay client.");
                    return false;
                }
                return true;
            }
            catch (RelayServiceException e)
            {
                if (attempt == maxAttempts)
                {
                    Debug.LogException(e);
                    return false;
                }

                await Task.Delay(500 * attempt);
            }
        }
        return false;
    }
}
