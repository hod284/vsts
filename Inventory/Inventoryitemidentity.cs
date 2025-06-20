using BNG;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using VSTS;

public class Inventoryitemidentity : NetworkBehaviour
{
    [SerializeField] private string _InventoryItemName;
    [SerializeField] private bool _Front =true;
    [SerializeField] private NetworkObject _NetworkOb;
    [SerializeField] private iteminventory _Inventory_PC;
    [SerializeField] private iteminventory _Inventory_VR;
    [SerializeField] private int _MaxiumCount =1;
    [SerializeField] private Transform _Parent;
    private Action<ulong, ulong> _ChangeOwnership; 
    bool _Retrun;
    public bool GetReturn { get=>_Retrun; }
    public void SetReturn(bool b)   => _Retrun =b; 
    public string GetName { get => _InventoryItemName; }
    public bool GetFront { get => _Front; }
    public NetworkObject GetNetworkOb { get => _NetworkOb; }
    public void SetInventory_PC(iteminventory IN) => _Inventory_PC = IN;
    public void SetInventory_VR(iteminventory IN) => _Inventory_VR = IN;
    public iteminventory GetInventory_PC { get=> _Inventory_PC;}
    public iteminventory GetInventory_VR { get => _Inventory_VR; }
    public int GetMaxiumCount { get => _MaxiumCount; }
    public void SetMaxiumCount(int c) => _MaxiumCount =c; 

    private void Awake()
    {
        _ChangeOwnership += changeownership_ServerRpc;  
        if (!GetComponent<PointerEvents>())
            gameObject.AddComponent<PointerEvents>();
        var pointer = gameObject.GetComponent<PointerEvents>();
        pointer.OnPointerClickEvent = new PointerEventDataEvent();
        pointer.OnPointerClickEvent.AddListener(PointerClick);
        var li = GameObject.FindObjectsOfType<iteminventory>();
        for (int i = 0; i < li.Length; i++)
        {
            if (li[i].GetisPC)
                _Inventory_PC = li[i];
            else
                _Inventory_VR = li[i];
        }
        if (_NetworkOb != null)
        {
            if (_InventoryItemName != _NetworkOb.GetComponent<Inventoryitemidentity>().GetName)
                _NetworkOb = null;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void ADD_ServerRpc(ulong id)
    {
        ulong networkObjectId = 0;
        if (IsHost && _NetworkOb != null)
        {
            var ob = GameObject.Instantiate(_NetworkOb,_Parent);
            ob.gameObject.transform.transform.position = transform.position;
            ob.gameObject.transform.rotation = transform.rotation;
            ob.Spawn();
            networkObjectId= ob.NetworkObjectId;
            ob.transform.SetParent(_Parent);
        }
        Replace_ClientRpc(id, networkObjectId);
    }
    [ServerRpc(RequireOwnership = false)]
    public void changeownership_ServerRpc(ulong networkObjectId, ulong id)
    {
        Debug.Log($"id : {id}");
        Debug.Log($"networkObjectId : {networkObjectId}");
        if (id == 0)
        {
            Debug.Log($"id : {id}");
            return;
        }
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject obj))
        {
            Debug.Log($"id : {id}");
            obj.ChangeOwnership(id);
        }
    }
    [ClientRpc]
    public void Replace_ClientRpc(ulong id, ulong networkObjectId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject obj))
        {
            obj.GetComponent<NetworkGrabbable>().enabled = false;
            if (NetworkManager.LocalClientId == id)
            {
                if (_Inventory_PC != null)
                {
                    var it = _Inventory_PC.GetInventoryList;
                    int index = it.FindIndex(x => x.GetName == obj.GetComponent<Inventoryitemidentity>().GetName);
                    if (index >= 0)
                    {
                        _Inventory_PC.GetInventoryList[index] = obj.GetComponent<Inventoryitemidentity>();
                        _Inventory_PC.GetInventoryList[index].transform.position = _Inventory_PC.GetRePoint;
                    }
                }
                if (_Inventory_VR != null)
                {
                    var it = _Inventory_VR.GetInventoryList;
                    int index = it.FindIndex(x => x.GetName == obj.GetComponent<Inventoryitemidentity>().GetName);
                    if (index >= 0)
                    {
                        _Inventory_VR.GetInventoryList[index] = obj.GetComponent<Inventoryitemidentity>();
                        _Inventory_VR.GetInventoryList[index].transform.position = _Inventory_VR.GetRePoint;          
                    }
                }
                _ChangeOwnership(networkObjectId, id);
            }
        }
    }

    public void Addnetwork()
    {
        if (IsClient && !IsServer)
            ADD_ServerRpc(NetworkManager.LocalClient.ClientId);
        else
        {
            if (_NetworkOb != null)
            {
                var ob = GameObject.Instantiate(_NetworkOb,_Parent);
                ob.gameObject.transform.transform.position = transform.position;
                ob.gameObject.transform.rotation = transform.rotation;
                ob.Spawn();
                ob.ChangeOwnership(NetworkManager.LocalClient.ClientId);
                ob.transform.SetParent(_Parent);
                if (_Inventory_PC != null)
                {
                    ob.transform.SetParent(_Parent);
                    _Inventory_PC.GetInventoryList[_Inventory_PC.GetInventoryList.Count - 1] = ob.GetComponent<Inventoryitemidentity>();
                    _Inventory_PC.GetInventoryList[_Inventory_PC.GetInventoryList.Count - 1].transform.position = _Inventory_PC.GetRePoint;
                }
                if (_Inventory_VR != null)
                {
                    ob.transform.SetParent(_Parent);
                    _Inventory_VR.GetInventoryList[_Inventory_VR.GetInventoryList.Count - 1] = ob.GetComponent<Inventoryitemidentity>();
                    _Inventory_VR.GetInventoryList[_Inventory_VR.GetInventoryList.Count - 1].transform.position = _Inventory_VR.GetRePoint;
                }
            }
        }
        Debug.Log("call");
    }
    
    public void PointerClick(PointerEventData eventData)
    {
        if(_Inventory_PC != null)
        _Inventory_PC.Adding(this);
        if(_Inventory_VR != null)
        _Inventory_VR.Adding(this); 
    }
}
