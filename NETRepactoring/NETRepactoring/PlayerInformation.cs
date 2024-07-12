using Dissonance.Config;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInformation : NetworkBehaviour
{
    private NetworkVariable<FixedString128Bytes> TrainingEvent = new NetworkVariable<FixedString128Bytes>();
    private NetworkVariable<FixedString128Bytes> TrainingClasses = new NetworkVariable<FixedString128Bytes>();
    private NetworkVariable<bool> VrorPC = new NetworkVariable<bool>();
    private NetworkVariable<bool> Ready = new NetworkVariable<bool>();
    private NetworkVariable<FixedString128Bytes> ArmyNumber  = new NetworkVariable<FixedString128Bytes>("Initial Value");
    private NetworkVariable<FixedString128Bytes> Belong = new NetworkVariable<FixedString128Bytes>("Initial Value");
    private NetworkVariable<FixedString128Bytes> Classes = new NetworkVariable<FixedString128Bytes>("Initial Value");
    private NetworkVariable<FixedString128Bytes> SubmarineType = new NetworkVariable<FixedString128Bytes>("Initial Value");
    private NetworkVariable<FixedString128Bytes> PlayerName = new NetworkVariable<FixedString128Bytes>("Initial Value");
    private NetworkVariable<ulong> Clientid = new NetworkVariable<ulong>();
    public ulong GetClientid { get => Clientid.Value; }
    public bool GetReady { get => Ready.Value; }
    public string GetArmyNumber { get => ArmyNumber.Value.ToString(); }
    public string GetBelong { get => Belong.Value.ToString(); }
    public string GetClasses { get => Classes.Value.ToString(); }
    public string GetSubmarineType { get => SubmarineType.Value.ToString(); }
    public string GetPlayerName { get => PlayerName.Value.ToString(); }
    public string GetTrainingEvent { get => TrainingEvent.Value.ToString(); }
    public string GetTrainingClasses { get => TrainingClasses.Value.ToString(); }
    public string GetVrorPC { get => VrorPC.Value.ToString(); }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        var host = GameObject.FindObjectOfType<NetworkHostManager>();
        host.AddingList(this);
        var playerinf = GameObject.FindObjectOfType<PlayerInformationToSever>();
        ArmyNumber.Value = playerinf.GetArmyNumber;
    }
    private void Start()
    {
        if (NetworkManager.ConnectedClientsIds.Count > 1)
           DontDestroyOnLoad(this);
    }
    /// <summary>
    /// Host에서만 불러야 하는함수
    /// </summary>
    public void SetClientid(ulong id)
    {
           Clientid.Value = id;
    }
    /// <summary>
    /// client에서만 불러야 하는함수
    /// 군번
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ArmyNumber_ServerRpc(string Number)
    {
        ArmyNumber.Value = Number;
    }
    /// <summary>
    /// client에서만 불러야 하는함수
    /// 게임 레디
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void Ready_ServerRpc(bool ready)
    {
        Ready.Value = ready;
    }
    /// <summary>
    /// 양방향 통신을 위해 넣은 함수 
    /// 플레이어이름
    /// </summary>
    public void SetPlayerName(string name)
    {
        if (IsHost)
            PlayerName.Value = name;
        else if (IsClient && !IsServer)
            PalyerName_ServerRpc(name);
    }
    [ServerRpc(RequireOwnership = false)]
    private void PalyerName_ServerRpc(string Name)
    {
        PlayerName.Value = Name;
    }
    /// <summary>
    /// 양방향 통신을 위해 넣은 함수
    /// 소속
    /// </summary>
    public void SetBelong(string belong)
    {
        if (IsHost)
            Belong.Value = belong;
        else if (IsClient && !IsServer)
            Belong_ServerRpc(belong);
    }

    [ServerRpc(RequireOwnership = false)]
    private void Belong_ServerRpc(string belong)
    {
        Belong.Value = belong;
    }
    /// <summary>
    /// 양방향 통신을 위해 넣은 함수
    /// 계급
    /// </summary>
    public void SetClasse(string classes)
    {
        if (IsHost)
            Classes.Value = classes;
        else if (IsClient && !IsServer)
            Classes_ServerRpc(classes);
    }
    [ServerRpc(RequireOwnership = false)]
    private void Classes_ServerRpc(string classes)
    {
        Classes.Value = classes;
    }
    /// <summary>
    /// 양방향 통신을 위해 넣은 함수
    /// 잠수함명
    /// </summary>
    public void SetSubmarineType(string submarinetype)
    {
        if (IsHost)
            SubmarineType.Value = submarinetype;
        else if (IsClient && !IsServer)
            SubmarineType_ServerRpc(submarinetype);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SubmarineType_ServerRpc(string submarinetype)
    {
        SubmarineType.Value = submarinetype;
    }
    /// <summary>
    /// 양방향 통신을 위해 넣은 함수
    /// 훈련 이름
    /// </summary>
    public void SetTrainingEvent(string value)
    {
        if (IsHost)
            TrainingEvent.Value = value;
        else if (IsClient && !IsServer)
            TrainingEvent_ServerRpc(value);
    }
    [ServerRpc(RequireOwnership = false)]
    private void TrainingEvent_ServerRpc(string value)
    {
        TrainingEvent.Value = value;
    }
    /// <summary>
    /// 양방향 통신을 위해 넣은 함수
    /// 훈련 소속
    /// </summary>
    public void SetTrainingClasses(string value)
    {
        if (IsHost)
            TrainingClasses.Value = value;
        else if (IsClient && !IsServer)
            TrainingClasses_ServerRpc(value);
    }
    [ServerRpc(RequireOwnership = false)]
    private void TrainingClasses_ServerRpc(string value)
    {
        TrainingClasses.Value = value;
    }
    /// <summary>
    /// 양방향 통신을 위해 넣은 함수
    /// vr pc 선택
    /// </summary>
    public void SetVrorAR(bool value)
    {
        if (IsHost)
            VrorPC.Value = value;
        else if (IsClient && !IsServer)
            VrorPC_ServerRpc(value);
    }
    [ServerRpc(RequireOwnership = false)]
    private void VrorPC_ServerRpc(bool value)
    {
        VrorPC.Value = value;
    }
}
