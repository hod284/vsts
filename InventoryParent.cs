using BNG;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryParent : MonoBehaviour
{
    [SerializeField] private List<InventoryButton> _InventoryButtonList;
    [SerializeField] private iteminventory _Inventory;
    [SerializeField] private GameObject _Prefab;
    [SerializeField] private Vector3 _ObjectPositionInSlot;
    [SerializeField] private Vector3 _ObjectScaleInSlot;
    [SerializeField] private InventoryExitButton _SphereExitButon;
    public List<InventoryButton> GetInventoryButtonList { get => _InventoryButtonList; }
    public InventoryExitButton GetSphereExitButon { get => _SphereExitButon; }
    private float _GapAngle;
    private void OnEnable()
    {
        for (int i = 0; i < _InventoryButtonList.Count; i++)
        {
            _InventoryButtonList[i].SetMaterial(_InventoryButtonList[i].GetOriginalMaterial);
            _InventoryButtonList[i].gameObject.SetActive(true);
        }
        _SphereExitButon.MaterialChange(_SphereExitButon.GetOriginalMaterial);
    }
   
    public void AddInventoryItem(InventoryIdenty ob)
    {
        _GapAngle = _Inventory.GetCircleAngle / _Inventory.GetMaxiumInventorySlot;
        var item = Instantiate(_Prefab, transform);
        item.gameObject.SetActive(gameObject.activeSelf);
        string name = ob.GetComponent<InventoryIdenty>().GetName;
        item.GetComponent<InventoryButton>().SetName(name);
        _InventoryButtonList.Add(item.GetComponent<InventoryButton>());
        MovingAnimation();
        var itemob = Instantiate(ob.gameObject, item.transform);
        itemob.layer =item.layer;
        itemob.gameObject.SetActive(true);
        itemob.transform.localPosition = _ObjectPositionInSlot; 
        if(itemob.GetComponent<InventoryIdenty>().GetFront)
        itemob.transform.localEulerAngles = new Vector3(90.0F,0.0F,0.0F);
        else
            itemob.transform.localEulerAngles = Vector3.zero;
        itemob.transform.localScale = _ObjectScaleInSlot;
        if (itemob.GetComponent<InventoryIdenty>())
            Destroy(itemob.GetComponent<InventoryIdenty>());
        if (itemob.GetComponent<Grabbable>())
            Destroy(itemob.GetComponent<Grabbable>());
        if (itemob.GetComponent<Rigidbody>())
            Destroy(itemob.GetComponent<Rigidbody>());
        if (itemob.GetComponent<MeshCollider>())
            Destroy(itemob.GetComponent<MeshCollider>());
        if (itemob.GetComponent<BoxCollider>())
            Destroy(itemob.GetComponent<BoxCollider>());
    }
    public void DeletInventoryItem(InventoryIdenty deleteitem)
    {
        var deletbutton = _InventoryButtonList.Find(x => x.GetName == deleteitem.GetName);
        _InventoryButtonList.Remove(deletbutton);
        DestroyImmediate(deletbutton.transform.GetChild(0).gameObject);
        DestroyImmediate(deletbutton.gameObject);
        MovingAnimation();
    }
    private void MovingAnimation()
    {
        float angle = 0;
        for (int i = 0; i < _InventoryButtonList.Count; i++)
        {
            if (i == 0)
                _InventoryButtonList[i].transform.localEulerAngles = new Vector3(0, 0, 0);
            if (i != 0)
                _InventoryButtonList[i].transform.localEulerAngles = new Vector3(0, 0, angle);
            angle += _GapAngle;
        }
        _SphereExitButon.transform.SetAsLastSibling();
    }  
}
