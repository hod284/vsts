using BNG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryIdenty : MonoBehaviour
{
    [SerializeField] private string _InventoryItemName;
    [SerializeField] private bool _Front =true;
    private bool _ActiveOff = true;
    public void SetActiveOff (bool b) => _ActiveOff = b;
    public bool GetActiveOff { get=>_ActiveOff; }
    public string GetName { get => _InventoryItemName; }
    public bool GetFront { get => _Front; }

    [SerializeField] private iteminventory _Inventory;
    private void Awake()
    {
        if (!GetComponent<PointerEvents>())
            gameObject.AddComponent<PointerEvents>();
        var pointer = gameObject.GetComponent<PointerEvents>();
        pointer.OnPointerClickEvent = new PointerEventDataEvent();
        pointer.OnPointerClickEvent.AddListener(PointerClick);
    }
    
    public void PointerClick(PointerEventData eventData)
    {
        _Inventory.Adding(this);
    }
}
