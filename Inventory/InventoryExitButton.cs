using BNG;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using VSTS;
public class InventoryExitButton : MonoBehaviour
{
    [SerializeField] private iteminventory _Iteminventory;
    [SerializeField] private Material _HighlightMaterial;
    [SerializeField] private Material _OriginalMaterial;
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
        _Iteminventory.GetRightGrabber.ForceGrab = false;
        _Iteminventory.GetRightGrabber.ForceRelease = false;
        for (int i = 0; i < _Iteminventory.GetInventoryList.Count; i++)
        {
            _Iteminventory.GetInventoryList[i].transform.position = _Iteminventory.GetRePoint;
            _Iteminventory.GetInventoryList[i].GetComponent<Inventoryitemidentity>().SetReturn(true);
        }

    }
  
    private void FixedUpdate()
    {
        if (InputManager.Instance.InputType == E_INPUT_DEVICE.PC)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            bool hit = hits.Any(x => x.collider.gameObject == gameObject);
            if (hit)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _Iteminventory.CloseInventory();
                    _Iteminventory.GetRightGrabber.ForceGrab = false;
                    _Iteminventory.GetRightGrabber.ForceRelease = false;
                    for (int i = 0; i < _Iteminventory.GetInventoryList.Count; i++)
                    {
                        _Iteminventory.GetInventoryList[i].transform.position = _Iteminventory.GetRePoint;
                        _Iteminventory.GetInventoryList[i].GetComponent<Inventoryitemidentity>().SetReturn(true);
                    }
                }
                else
                    MaterialChange(_HighlightMaterial);
            }
            else
                MaterialChange(_OriginalMaterial);
        }
    }


    public void MaterialChange(Material material) => GetComponent<MeshRenderer>().material = material;
}
