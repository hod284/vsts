using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NpcInventory: MonoBehaviour
{
    [SerializeField] private Inventoryitemidentity _InventoryItem;
    [SerializeField] private Transform _HandPosition;
    private Transform _GetItem;
    private bool _Holding;
    public void NpcGetItem(string itemname)
    {
        if (_Holding)
            NpcLostItem(itemname);
        var li = GameObject.FindObjectsOfType<Inventoryitemidentity>().ToList();
        _GetItem = li.Find(x => x.GetName == itemname).transform;
        _GetItem.gameObject.SetActive(true);
        FollowingItem().Forget();
    }

    private async UniTask FollowingItem()
    {
        if (_Holding)
        {
            _GetItem.position = _HandPosition.position;
            _GetItem.rotation = _HandPosition.rotation;
            await UniTask.Yield();
        }
    }

    public void NpcLostItem(string itemname)
    {
        _GetItem.gameObject.SetActive(false);
        _Holding = false;
        _GetItem = null;
    }
}
