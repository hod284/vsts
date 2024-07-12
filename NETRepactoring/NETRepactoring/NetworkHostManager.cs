
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using VSTS;
using System.Linq;

public class NetworkHostManager : NetworkBehaviour
{
    [SerializeField] private PlayerInformation PlayerInformationPrefab;
    [SerializeField] private PlayerInformationToSever InformationtoSever;
    [SerializeField] private List<PlayerInformation> PlayerInformationlist;
    private NetworkVariable<FixedString128Bytes> ArmyNumber = new NetworkVariable<FixedString128Bytes>("Initial Value");
    private NetworkVariable<FixedString128Bytes> Classes = new NetworkVariable<FixedString128Bytes>("Initial Value");
    private NetworkVariable<FixedString128Bytes> PlayerName = new NetworkVariable<FixedString128Bytes>("Initial Value");
    private NetworkVariable<int> ChacracterIndex = new NetworkVariable<int>(0);
    public List<PlayerInformation> PlayerInformationlist_Public { get => PlayerInformationlist; }
    public void SetChacracterIndex(int index)
    {
        if (IsHost)
            ChacracterIndex.Value = index;
    }
    public void SetArmyNumber(string number)
    {
        if (IsHost)
            ArmyNumber.Value = number;
    }
    public void SetClasses(string classes)
    {
        if (IsHost)
            Classes.Value = classes;
    }

    public void SetPlayerName(string name)
    {
        if (IsHost)
            PlayerName.Value = name;
    }
    public int GetChacracterIndex { get => ChacracterIndex.Value; }
    public string GetArmyNumber { get => ArmyNumber.Value.ToString(); }
    public string GetClasses { get => Classes.Value.ToString(); } 
    public string GetPlayerName { get => PlayerName.Value.ToString(); }
    public void AddingList(PlayerInformation playerInformation)
    {
        PlayerInformationlist.Add(playerInformation);
    }
    private void Start()
    {
        NetworkManager.OnClientConnectedCallback += InformationSpaw;
        NetworkManager.OnClientDisconnectCallback += ThisDestory;
        DontDestroyOnLoad(this); 
    }


    private void InformationSpaw(ulong id)
    {
        
         if (IsHost)
        {
            if (NetworkManager.ConnectedClientsList.Count > 1)
            {
               var spawnobject = Instantiate(PlayerInformationPrefab);
                spawnobject.SetClientid(id);
                spawnobject.GetComponent<NetworkObject>().Spawn(true);
            }
        }
    }

    private void ThisDestory(ulong id)
    {
        var ga= PlayerInformationlist.Find(x=>x.GetClientid ==id );
        if (ga != null)
        {
            ga.GetComponent<NetworkObject>().Despawn();
            PlayerInformationlist.Remove(ga);
        }
    }
    public void TheDIscconnet(string ArmyNUmber)
    {
        var ga = PlayerInformationlist.Find(x => x.GetArmyNumber == ArmyNUmber);
        if (ga != null)
        {
            ga.GetComponent<NetworkObject>().NetworkManager.Shutdown();
            PlayerInformationlist.Remove(ga);
        }
    }
}
