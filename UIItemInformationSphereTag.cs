using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VSTS;

public class UIItemInformationSphereTag : MonoBehaviour
{
    [SerializeField] private UIitemidentity _Itemidentity;
    [SerializeField] private UIItemInformationDirector _ItemDirector;
    public void SetItemidentity(UIitemidentity item) => _Itemidentity = item;
    public void SetItemDirector(UIItemInformationDirector ItemDirector) => _ItemDirector = ItemDirector;

    private void Start()
    {
        var pointer = gameObject.GetComponent<PointerEvents>();
        pointer.OnPointerEnterEvent = new PointerEventDataEvent();
        pointer.OnPointerEnterEvent.AddListener(OnPointerEnter);
        pointer.OnPointerExitEvent = new PointerEventDataEvent();
        pointer.OnPointerExitEvent.AddListener(OnPointerExit);
        pointer.OnPointerDownEvent = new PointerEventDataEvent();
        pointer.OnPointerDownEvent.AddListener(OnPointerDown);
    }
   
    public void TagOn()
    {
        gameObject.GetComponent<MeshRenderer>().enabled =true;
    }
    public void TagOff()
    {
        gameObject.GetComponent<MeshRenderer>().enabled =false;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        _ItemDirector.UICusorChange_UI();
        _Itemidentity.OnlightingOn_Displaymeshs();
        if( _ItemDirector.GetSphereTagMode)
       _ItemDirector.SphereMeshRenderOff();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
       if (!_ItemDirector.GetinformationUIShowing)
       {
           _ItemDirector.UICusorChange_DEFAULT();
           if (_ItemDirector.GetSphereTagMode)
               _ItemDirector.SphereMeshRenderOn();
       }  
        _Itemidentity.OnlightingOff_Displaymeshs();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        _Itemidentity.SetHeadandExplain();
        _Itemidentity.ShowingInformation();
    }
}
