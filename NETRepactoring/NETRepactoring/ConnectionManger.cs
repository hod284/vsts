using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using System.Net.Sockets;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ExampleNetworkDiscovery))]
[RequireComponent(typeof(NetworkManager))]

public class ConnectionManger : MonoBehaviour
{
    [SerializeField] private List<GameObject> Playerprefab;
    [SerializeField] private ExampleNetworkDiscovery m_Discovery;
    [SerializeField] private NetworkManager m_NetworkManager;
    [SerializeField] private NetworkHostManager m_NetworkHostVlaueScript;
    [SerializeField] private Dictionary<IPAddress, DiscoveryResponseData> discoveredServers = new Dictionary<IPAddress, DiscoveryResponseData>();
    private Coroutine StartClientConnection;
    private bool ConnnectingSucess = false;
     private string SensrioStartSceenName = "";
     private LoadSceneMode SensrioStartLoadSceneMode;
    public bool ConnnectingSucess_Public { get => ConnnectingSucess; }

    void Awake()
    {
        m_NetworkManager.OnServerStarted += OnServerStarted;
    }

    private void OnServerStarted()
    {
        Debug.Log("OnServerStarted");
        m_NetworkManager.SceneManager.OnLoadComplete += SceentLoadCompleteSpawn;
    }
  
    /// <summary>
    /// 신동기화 함수
    /// </summary>
    public void NetWorkLoadSceen(string Name, LoadSceneMode loadSceneMode, bool senariostart = false)
    {
        if (senariostart)
        {
            SensrioStartSceenName = Name;
            SensrioStartLoadSceneMode = loadSceneMode;
        }
        else
            SensrioStartSceenName = "";
        m_NetworkManager.SceneManager.LoadScene(Name, loadSceneMode);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (m_Discovery == null) // This will only happen once because m_Discovery is a serialize field
        {
            m_Discovery = GetComponent<ExampleNetworkDiscovery>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(m_Discovery.OnServerFound, OnServerFound);
            Undo.RecordObjects(new Object[] { this, m_Discovery }, "Set NetworkDiscovery");
        }
    }
#endif

    void OnServerFound(IPEndPoint sender, DiscoveryResponseData response)
    {
        discoveredServers[sender.Address] = response;
    }
    public void StartHOST()
    {
        
        Debug.Log("StartHOST");
        m_Discovery.StartServer();
        SetConnettion();
        m_NetworkManager.StartHost();
    }
    public void StartSever()
    {
        Debug.Log("StartSever");
        m_Discovery.StartServer();
        SetConnettion();
        m_NetworkManager.StartServer();
    }
    public void StopNetcode()
    {
        Debug.Log("StopNetcode");
        NetworkManager.Singleton.Shutdown(true);
        m_Discovery.StopDiscovery();
        StartClientConnection =  null;
    }
    public void StartClient()
    {
        Debug.Log("StartClient");
        StartClientConnection = StartCoroutine( Connectiong());
        if (ConnnectingSucess)
            StartClientConnection = null;
    }
    private IEnumerator Connectiong()
    {
        int n = 0;
        m_Discovery.StartClient();
        m_Discovery.ClientBroadcast(new DiscoveryBroadcastData());
        float TIME = 0;
        while (TIME <20.0F)
        {
            TIME += Time.deltaTime * 1.0f;
            foreach (var discoveredServer in discoveredServers)
            {
                if (n == 0)
                {
                    UnityTransport transport = (UnityTransport)m_NetworkManager.NetworkConfig.NetworkTransport;
                    transport.SetConnectionData(discoveredServer.Key.ToString(), discoveredServer.Value.Port);
                    TIME = 20.0F;
                    n = 1;
                }
                else
                    break;
            }
            yield return null;
        }
        if (n == 1)
        {
            m_NetworkManager.StartClient();
            ConnnectingSucess = true;
            Debug.Log("connection");
        }
        else
        {
            ConnnectingSucess = false;
            Debug.LogError("MAKING THE HOST");
        }
    }
    private void SetConnettion()
    {
        UnityTransport transport = (UnityTransport)m_NetworkManager.NetworkConfig.NetworkTransport;
        IPHostEntry host = Dns.GetHostByName(Dns.GetHostName());
        for (int i = 0; i < host.AddressList.Length; i++)
        {
            if (host.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
            {
                transport.SetConnectionData(host.AddressList[i].ToString(), 7777);
            }
        }
        Debug.Log("SetConnettion");
    }
    private void SceentLoadCompleteSpawn(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName == SensrioStartSceenName)
        {
            var POS = new Vector3(0, 0, 0);
            var player = Instantiate(Playerprefab[m_NetworkHostVlaueScript.GetChacracterIndex], POS, Quaternion.identity);
            var net = player.GetComponent<NetworkObject>();
            net.SpawnAsPlayerObject(clientId, true);
        }
    }
}
