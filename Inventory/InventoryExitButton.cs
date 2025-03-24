using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class InventoryExitButton : MonoBehaviour
{
    [SerializeField] private iteminventory _Iteminventory;
    [SerializeField] private Material _HighlightMaterial;
    [SerializeField] private Material _OriginalMaterial;
    [SerializeField] private InputSystemUIInputModule _PCInputSystemUI;
    public Material GetOriginalMaterial { get => _OriginalMaterial; }
    public Material GetHighlightMaterial { get => _HighlightMaterial; }
    private void Start()
    {
        if (gameObject.GetComponent<PointerEvents>())
        {
            var pointer = gameObject.GetComponent<PointerEvents>();
            pointer.OnPointerEnterEvent = new PointerEventDataEvent();
            pointer.OnPointerEnterEvent.AddListener(PointerEnter);
            pointer.OnPointerExitEvent = new PointerEventDataEvent();
            pointer.OnPointerExitEvent.AddListener(PointerExit);
            pointer.OnPointerClickEvent = new PointerEventDataEvent();
            pointer.OnPointerClickEvent.AddListener(PointerClick);
        }
        _PCInputSystemUI = GameObject.FindObjectOfType<EventSystem>().GetComponent<InputSystemUIInputModule>();
    }
    public void PointerEnter(PointerEventData eventData)
    {
            MaterialChange(_HighlightMaterial);
    }
    public void PointerExit(PointerEventData eventData)
    {
            MaterialChange(_OriginalMaterial);
    }
    public void PointerClick(PointerEventData eventData)
    {
            _Iteminventory.CloseInventory();
        for (int i = 0; i < _Iteminventory.GetInventoryList.Count; i++)
        {
            _Iteminventory.GetInventoryList[i].transform.position = _Iteminventory.GetRePoint;
            _Iteminventory.GetInventoryList[i].GetComponent<Inventoryitemidentity>().SetReturn(true);
        }

    }
    private void OnMouseEnter()
    {
        if(_PCInputSystemUI.enabled)
        MaterialChange(_HighlightMaterial);
    }
    private void OnMouseExit()
    {
        if (_PCInputSystemUI.enabled)
            MaterialChange(_OriginalMaterial);
    }
    private void OnMouseDown()
    {
        if (_PCInputSystemUI.enabled)
        {
            _Iteminventory.CloseInventory();
            for (int i = 0; i < _Iteminventory.GetInventoryList.Count; i++)
            {
               _Iteminventory.GetInventoryList[i].transform.position = _Iteminventory.GetRePoint;
                _Iteminventory.GetInventoryList[i].GetComponent<Inventoryitemidentity>().SetReturn(true);
            }
        }
    }

    public void MaterialChange(Material material) => GetComponent<MeshRenderer>().material = material;
}
