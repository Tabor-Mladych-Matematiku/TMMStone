using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button HostButton;
    [SerializeField] private Button ClientButton;
    private void Awake()
    {
        HostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            HostButton.enabled = false;
            ClientButton.enabled = false;
        });
        ClientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            HostButton.enabled = false;
            ClientButton.enabled = false;
        });
    }
}
