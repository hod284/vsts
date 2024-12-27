using BNG;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using VSTS;
public class UIitemidentity : MonoBehaviour
{
    [SerializeField] private string _GroupCode = string.Empty;
    [SerializeField] private string _NameDesc = string.Empty;
    [SerializeField] private string _EquipDesc = string.Empty;
    [SerializeField] private List<string> _Heads = new List<string>();
    [SerializeField] private List<string> _Bodys = new List<string>();
    [SerializeField] private GameObject[] _HighlightObject; 
    [SerializeField] private UIItemInformationDirector _InformationDirectorOB;
    private VSTS_GroupInfo _VSTS_BMSTcontents = new VSTS_GroupInfo();

    public void  SetVSTS_BMSTcontents(VSTS_GroupInfo _VSTS_BMSTcontent) => _VSTS_BMSTcontents = _VSTS_BMSTcontent;
    public void SetGroupCode(string ID) => _GroupCode = ID;
    public void SetNameDesc(string Name) => _NameDesc = Name;
    public void SetEquipDesc(string EquipDesc) => _EquipDesc = EquipDesc;
    public string GetGroupCode { get => _GroupCode; }
    public string GetNameDesc { get => _NameDesc; }
    public string GetEquipDesc { get => _EquipDesc; }
    public void AddHead(string head) => _Heads.Add(head);
    public void AddBody(string body) => _Bodys.Add(body);
    public void ClearHead() => _Heads.Clear();
    public void ClearBody() => _Bodys.Clear();

    public List<string> GetHead  { get => _Heads; }
    public List<string> GetBody { get => _Bodys; }
    public GameObject[]  GetHighlightObject { get => _HighlightObject; }
    public void SetHighlightObject(GameObject[] obs)  => _HighlightObject =obs; 
    public void SetInformationDirectorOB(UIItemInformationDirector idrector) => _InformationDirectorOB = idrector; 
    private void Start()
    {
        inite();
    }
    private void inite()
    {
        if (!GetComponent<MeshCollider>())
        {
            if (GetComponent<MeshRenderer>())
                gameObject.AddComponent<MeshCollider>();
        }
        if (!GetComponent<PointerEvents>())
            gameObject.AddComponent<PointerEvents>();
        var childs = transform.GetComponentsInChildren<UIitemidentity>().ToList();
        var othergroup = childs.Find(x => x.GetGroupCode != _GroupCode);
        if (othergroup == null|| childs.Count ==0)
        {
            var pointer = gameObject.GetComponent<PointerEvents>();
            pointer.OnPointerClickEvent = new PointerEventDataEvent();
            pointer.OnPointerClickEvent.AddListener(OnPointerDownEnter);
            pointer.OnPointerEnterEvent = new PointerEventDataEvent();
            pointer.OnPointerEnterEvent.AddListener(OnPointerlightingOn);
            pointer.OnPointerExitEvent = new PointerEventDataEvent();
            pointer.OnPointerExitEvent.AddListener(OnPointerlightingOff);
        }
    }
    public void OnPointerDownEnter(PointerEventData eventData)
    {
        SetHeadandExplain();
        ShowingInformation();
    }
    public void OnPointerlightingOn(PointerEventData eventData)
    {
        _InformationDirectorOB.UICusorChange_UI();
        OnlightingOn_Displaymeshs();
    }
    public void OnlightingOn_Displaymeshs()
    {
        for (int i = 0; i < _HighlightObject.Length; i++)
        {
            if (_HighlightObject[i].GetComponent<UIitemidentity>())
            {
                var h = _HighlightObject[i].GetComponent<UIitemidentity>();
                h.OnlightingOn();
            }
        }
    }
    public void OnlightingOn()
    {
        if (transform.parent.GetComponent<UIitemidentity>())
        {
            if (transform.parent.GetComponent<UIitemidentity>().GetGroupCode == _GroupCode)
            {
                ObjectHighlighter.SetOutline(transform.parent.gameObject, ObjectHighlighter.E_OUTLINE.RED);
                SetMaterial_ParentFromChildren(transform.parent);
            }
            else
            {
                ObjectHighlighter.SetOutline(gameObject, ObjectHighlighter.E_OUTLINE.RED);
                if (transform.childCount > 0)
                    SetMaterial_ThisObjectFromChildren();
                else
                    SetMaterial(gameObject);
            }
        }
        else
        {
            ObjectHighlighter.SetOutline(gameObject, ObjectHighlighter.E_OUTLINE.RED);
            if (transform.childCount > 0)
                SetMaterial_ThisObjectFromChildren();
            else
                SetMaterial(gameObject);
        }
    }
    public void OnPointerlightingOff(PointerEventData eventData)
    {
        if(!_InformationDirectorOB.GetinformationUIShowing)
         _InformationDirectorOB.UICusorChange_DEFAULT();
        OnlightingOff_Displaymeshs();
    }
    public void OnlightingOff_Displaymeshs()
    {
        for (int i = 0; i < _HighlightObject.Length; i++)
        {
            if (_HighlightObject[i].GetComponent<UIitemidentity>())
            {
                var h = _HighlightObject[i].GetComponent<UIitemidentity>();
                h.OnlightingOff();
            }
        }
    }
    public void OnlightingOff()
    {
        if (transform.parent.GetComponent<UIitemidentity>())
        {
            if (transform.parent.GetComponent<UIitemidentity>().GetGroupCode == _GroupCode)
            {
                ObjectHighlighter.RemoveOutline(transform.parent.gameObject);
                ResetMaterial_ParentFromChildren(transform.parent);
            }
            else
            {
                ObjectHighlighter.RemoveOutline(gameObject);
                if (transform.childCount > 0)
                    ResetMaterial_ThisObjectFromChildren();
                else
                    ResetMaterial(gameObject);
            }
        }
        else
        {
            ObjectHighlighter.RemoveOutline(gameObject);
            if (transform.childCount > 0)
                ResetMaterial_ThisObjectFromChildren();
            else
                ResetMaterial(gameObject);
        }
    }


    public void ShowingInformation()
    {
        // 정보창이 너무 작아서 수정 필요
        // var _popup = UIManager.Instance.OpenUI<UIPopupInformationMachine>(E_UI_TYPE.UIPopupInformationMachine);
        //  if( ItemInformation != null)
        //    _popup.SetBasicInformation(ItemID, ItemInformation);
        //  else
        //    _popup.SetBasicInformation(ItemID, Contents.text);
        // 수정되기전에 
        _InformationDirectorOB.Laseroff();
        OnlightingOff_Displaymeshs();
        _InformationDirectorOB.UICamerReturn();
        if (_InformationDirectorOB.GetSphereTagMode)
            _InformationDirectorOB.SphereOff();
        if(_NameDesc ==string.Empty)
        _NameDesc = _InformationDirectorOB.GetItemInformationsGroup[_GroupCode].NameDesc;
        if (_EquipDesc == string.Empty)
            _EquipDesc = _InformationDirectorOB.GetItemInformationsGroup[_GroupCode].EquipDesc;
        
        UIItemInformationManager informanger_real = null;
        informanger_real = _InformationDirectorOB.GetNowifmanager;
        if (informanger_real == null)
            return;
        informanger_real.ClearInformationlistandGameObject();
        _InformationDirectorOB.SetInformationUIShowing(true);
         informanger_real.SetheTitle (_NameDesc);
        informanger_real.SetEquipDesc(_EquipDesc);
        informanger_real.SetNowGroupName(_GroupCode);
        informanger_real.SetHead(_Heads);
         informanger_real.SetExplain(_Bodys);
        if (_VSTS_BMSTcontents.parent_group != string.Empty)
        {
             var item = _InformationDirectorOB.GetItemInformationsGroup[_VSTS_BMSTcontents.parent_group];
            var head = new List<string>();
            var body = new List<string>();
            _InformationDirectorOB.SetContents_Parent(ref head, ref body, item);
            informanger_real.SetParentGroupName(_VSTS_BMSTcontents.parent_group);
            informanger_real.SetParentEquipDesc(item.EquipDesc);
             informanger_real.SetheParentTitle(item.NameDesc);
             informanger_real.SetParentHead(head);
             informanger_real.SetParentExplain(body);
             informanger_real.SetParentactive(true);
             informanger_real.SetRevertActive(false);
        }
        else
        {
             informanger_real.SetParentactive(false);
            informanger_real.SetRevertActive(false);
        }

        informanger_real.ClearPrevStack();
        informanger_real.OnPointerDownEnter();
    }
    public void SetHeadandExplain()
    {
        if (Contain(_GroupCode))
            _VSTS_BMSTcontents = SetDic(_GroupCode);
        SetContents();
    }
    public void SetHeadandExplain_Parent(UIitemidentity uIitemidentity)
    {
        uIitemidentity.SetVSTS_BMSTcontents(SetDic(_VSTS_BMSTcontents.parent_group));
        uIitemidentity.SetContents();
    }
    public VSTS_GroupInfo SetDic(string objectname)
    {
        var co = new VSTS_GroupInfo();
        co = _InformationDirectorOB.GetItemInformationsGroup[objectname];
        return co;
    }
    public bool Contain(string objectname)
    {
        bool contain = false;      
        contain = _InformationDirectorOB.GetItemInformationsGroup.ContainsKey(objectname);
        return contain;
    }
    public void SetContents()
    {
        _Heads.Clear();
        _Bodys.Clear();

        if (_VSTS_BMSTcontents.FuncDesc != null)
        {
            _Heads.Add("\u25C6" + "기능");
            _Bodys.Add(_VSTS_BMSTcontents.FuncDesc);
        }
        if (_VSTS_BMSTcontents.SpecDesc != null)
        {
            _Heads.Add("\u25C6" + "주요제원");
            _Bodys.Add(_VSTS_BMSTcontents.SpecDesc);
        }
    }
    
    public void SetMaterial( GameObject target) => target.layer = 5;

    
    public void ResetMaterial(GameObject target) => target.layer = 0;
    
    public void SetMaterial_ParentFromChildren(Transform parent)
    {
        parent.GetComponent<UIitemidentity>().SetMaterial(parent.gameObject);
        for (int i = 0; i < parent.childCount; i++)
        {
            var itemidentity = parent.GetChild(i).GetComponent<UIitemidentity>();
            if (itemidentity != null)
                itemidentity.SetMaterial(parent.GetChild(i).gameObject);
        }
    }
    public void SetMaterial_ThisObjectFromChildren()
    {
        SetMaterial(gameObject);
        for (int i = 0; i < transform.childCount; i++)
        {
            var itemidentity = transform.GetChild(i).GetComponent<UIitemidentity>();
            if (itemidentity != null)
            {
                if (itemidentity.GetGroupCode == _GroupCode)
                    itemidentity.SetMaterial(transform.GetChild(i).gameObject);
            }
        }
    }
    public void ResetMaterial_ParentFromChildren(Transform parent)
    {
        parent.GetComponent<UIitemidentity>().ResetMaterial(parent.gameObject);
        for (int i = 0; i < parent.childCount; i++)
        { 
            var itemidentity = parent.GetChild(i).GetComponent<UIitemidentity>();
            if (itemidentity != null)
                itemidentity.ResetMaterial(parent.GetChild(i).gameObject);
        }
    }
    public void ResetMaterial_ThisObjectFromChildren()
    {
       ResetMaterial(gameObject);
        for (int i = 0; i < transform.childCount; i++)
        {
            var itemidentity = transform.GetChild(i).GetComponent<UIitemidentity>();
            if (itemidentity != null)
            {
                if(itemidentity.GetGroupCode == _GroupCode )
                itemidentity.ResetMaterial(transform.GetChild(i).gameObject);
            }
        }
    }
}
