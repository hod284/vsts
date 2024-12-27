using BNG;
using Newtonsoft.Json;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using VSTS;
using System.IO;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;
using System;

[Serializable]
public class ItemInformationsGroup : SerializableDictionaryBase<string, VSTS_GroupInfo> { }

public class UIItemInformationDirector : MonoBehaviour
{
    [SerializeField] private UIItemInformationManager _InformationManager_vr;
    [SerializeField] private UIItemInformationManager _InformationManager_pc;
    [SerializeField] private List<GameObject> _Items;
    [SerializeField] private List<VSTS_Group> _ItemGroups = new List<VSTS_Group>();
    [SerializeField] private ItemInformationsGroup _ItemInformations_Group = new();
    [SerializeField] private string _GroupPath;
    [SerializeField] private string _GroupInfoPath;
    [SerializeField] private C_ACC_Subtype _SubmarineData;
    [SerializeField] private Camera _UICamera;
    [SerializeField] private Camera _OBCamera;
    [SerializeField] private UIItemInformationSphereTag _SphereTagPrefab;
    [SerializeField] private List<UIItemInformationSphereTag> _SphereTags;
    [SerializeField] private GameObject _SphereParents;
    [SerializeField] private Transform _Righthand;
    private GameObject _Uicusor;
    private float _UIfarClipPlane = 100.0f;
    private float _UInearClipPlane = 0.3f;
    private float _UIsize = 3.0f;
    private float _LimitedDistance = 3.0f;
    private Transform _Player;
    private SmoothLocomotion _SmoothLocomotion;
    private PointerEventData eventData;
    private UIItemInformationManager _Nowifmanager;
    private bool _FirstLoopCheck = false;
    [SerializeField] private bool _SphereTagMode = false;
    [SerializeField] private bool _PopupShowing = false;
    public bool GetSphereTagMode { get => _SphereTagMode; }
    public List<GameObject> GetItem { get => _Items; }
    public List<VSTS_Group> GetItemGroups { get => _ItemGroups; }
    public C_ACC_Subtype GetSubmarineData { get => _SubmarineData; }
    public ItemInformationsGroup GetItemInformationsGroup { get => _ItemInformations_Group; }
    public UIItemInformationManager GetNowifmanager { get => _Nowifmanager; }
    public void SetSubmarineData(C_ACC_Subtype submarineData) => _SubmarineData = submarineData;
    public void SetInformationUIShowing(bool sh) => _PopupShowing = sh;
    public bool GetinformationUIShowing { get => _PopupShowing; }
    [Button("아이템 제원 입력")]
    public void Inite()
    {
        clear();
        _ItemGroups = itemInformationMethod();
        for (int i = 0; i < _ItemGroups.Count; i++)
        {
            var ga = GameObject.Find(_ItemGroups[i].ObjectMeshName);
            if (ga != null)
            {
                if (ga.transform.childCount > 0)
                {
                    SET(ga, _ItemGroups[i]);
                    if (!ga.GetComponent<UIItemInformationSphereTag>())
                        _Items.Add(ga);
                    for (int j = 0; j < ga.transform.childCount; j++)
                    {
                        var CH = ga.transform.GetChild(j);
                        VSTS_Group cga = _ItemGroups.Find(x => x.ObjectMeshName == CH.gameObject.name);
                        if (cga.ObjectMeshName == null)
                        {
                            SET(CH.gameObject, _ItemGroups[i]);
                            if(!CH.GetComponent<UIItemInformationSphereTag>())
                            _Items.Add(CH.gameObject);
                        }
                    }
                }
                else
                {
                    SET(ga, _ItemGroups[i]);
                    if (!ga.GetComponent<UIItemInformationSphereTag>())
                        _Items.Add(ga);
                }
            }
        }
    }
    [Button("아이템 제원 전부 삭제")]
    public void clear()
    {
        if (_Items.Count > 0)
        {
            try
            {
                Debug.Log(_Items.Count.ToString());
                for (int i = 0; i < _Items.Count; i++)
                {
                    var component = _Items[i].GetComponent<UIitemidentity>();
                    DestroyImmediate(component);
                    var point = _Items[i].GetComponent<PointerEvents>();
                    DestroyImmediate(point);
                }
                _Items.Clear();
            }
            catch (Exception e )
            {
                _Items.Clear();
            }
        }
    }
    [Button("GroupDictionary셋팅")]
    public void GroupDictionary()
    {
        _ItemInformations_Group.Clear();
        var gr = GetVSTS_GroupInfoFromJSON();
        for (int i = 0; i < gr.Count; i++)
        {
            VSTS_GroupInfo vSTS_GroupInfo = new VSTS_GroupInfo();
            if (gr[i].MOS == "null")
                vSTS_GroupInfo.MOS = string.Empty;
            else
                vSTS_GroupInfo.MOS = gr[i].MOS;
            if (gr[i].parent_group == "null")
                vSTS_GroupInfo.parent_group =string.Empty;
            else
                vSTS_GroupInfo.parent_group = gr[i].parent_group;
            if (gr[i].BM_group_code == "null")
                vSTS_GroupInfo.BM_group_code = string.Empty;
            else
                vSTS_GroupInfo.BM_group_code = gr[i].BM_group_code;
            if (gr[i].EquipDesc =="null")
                vSTS_GroupInfo.EquipDesc = string.Empty;
            else
            vSTS_GroupInfo.EquipDesc = gr[i].EquipDesc;
            if (gr[i].InstallLocationDesc == "null")
                vSTS_GroupInfo.InstallLocationDesc = string.Empty;
             else
                vSTS_GroupInfo.InstallLocationDesc = gr[i].InstallLocationDesc;
            if (gr[i].FuncDesc == "null")
                vSTS_GroupInfo.FuncDesc = string.Empty;
            else
                vSTS_GroupInfo.FuncDesc = gr[i].FuncDesc;
            if (gr[i].NameDesc == "null")
                vSTS_GroupInfo.NameDesc = string.Empty;
            else
                vSTS_GroupInfo.NameDesc = gr[i].NameDesc;
            if (gr[i].SpecDesc == "null")
                vSTS_GroupInfo.SpecDesc = string.Empty;
            else
                vSTS_GroupInfo.SpecDesc = gr[i].SpecDesc;
            vSTS_GroupInfo.idx = gr[i].idx;
            if (gr[i].display_meshes == "null")
            {
                vSTS_GroupInfo.display_meshes = string.Empty;
                vSTS_GroupInfo.displaymeshes_OB = null;
            }
            else
            {
                vSTS_GroupInfo.display_meshes = gr[i].display_meshes;
                var meshnames = gr[i].display_meshes.Split(';');
                List<GameObject> galist = new List<GameObject>();
                for (int j = 0; j < meshnames.Length; j++)
                {
                    var ga = GameObject.Find(meshnames[j]);
                    if (ga != null)
                        galist.Add(ga);
                }
                vSTS_GroupInfo.displaymeshes_OB = galist.ToArray();
            }
            _ItemInformations_Group.Add(gr[i].BM_group_code, vSTS_GroupInfo);
        }
    }
    [Button("GroupDictionary삭제")]
    public void GroupDictionary_CLEAR()
    {
        _ItemInformations_Group.Clear();
    }
  
    [Button("모든 메쉬 콜라이더 트리거 ON")]
    public void MeshColiderTrrigerOn()
    {
        if (_Items.Count > 0)
        {
            for (int i = 0; i < _Items.Count; i++)
            {
                var component = _Items[i].GetComponent<MeshCollider>();
                component.convex = true;
                component.isTrigger = true;
            }
        }
    }
    [Button("모든 메쉬 콜라이더 트리거 OFF")]
    public void MeshColiderTrrigerOff()
    {
        if (_Items.Count > 0)
        {
            for (int i = 0; i < _Items.Count; i++)
            {
                var component = _Items[i].GetComponent<MeshCollider>();
                component.isTrigger = false;
                component.convex = false;
            }
        }
    }
    [Button("태그 붙이기")]
    public void SetTag()
    {
        if(_SphereTags.Count>0)
        ClearTag();
        _ItemGroups = itemInformationMethod();
        for (int i = 0; i < _ItemGroups.Count; i++)
        {
            var ga = GameObject.Find(_ItemGroups[i].ObjectMeshName);
            if (ga != null)
            {
                var ch = ga.transform.GetComponentsInChildren<UIitemidentity>().ToList();
                var othergroup = ch.Find(x=>x.GetGroupCode != ga.GetComponent<UIitemidentity>().GetGroupCode);
                if (ga.transform.childCount == 0 && othergroup == null && ga.GetComponent<MeshRenderer>())
                    SetSphereTag(ga.transform,ga.GetComponent<UIitemidentity>());
                else if (ga.transform.childCount > 0 && othergroup == null)
                    SetSphereTag(ga.transform.GetChild(0), ga.GetComponent<UIitemidentity>());
            }
        }
    }
    [Button("태그 삭제")]
    public void ClearTag()
    {
        for (int i = 0; i < _SphereTags.Count; i++)
           DestroyImmediate(_SphereTags[i].gameObject);
        _SphereTags.Clear();
    }
    [Button("ItemGroup입력")]
    public void SetItemGroup()
    {
        _ItemGroups = itemInformationMethod();
    }
    [Button("ItemGroup 삭제")]
    public void ClearItemGroup()
    {
        _ItemGroups.Clear();
    }
    private void Start()
    {
        OnSphereTagOFF();
        UICamerChange();
        _Player = GameObject.FindObjectOfType<CharacterController>().transform;
        _SmoothLocomotion = _Player.GetComponent<SmoothLocomotion>();
        switch (InputManager.Instance.InputType)
        {
            case E_INPUT_DEVICE.PC:
                _InformationManager_vr.gameObject.SetActive(false);
                _InformationManager_pc.gameObject.SetActive(true);
                _Nowifmanager = _InformationManager_pc;
                break;
            case E_INPUT_DEVICE.VR:
                _InformationManager_vr.gameObject.SetActive(true);
                _InformationManager_pc.gameObject.SetActive(false);
                _Nowifmanager = _InformationManager_vr;
                break;
        }
    }
    public void OffBoxColider()
    {
        if (!_SphereTagMode)
        {
            for (int i = 0; i < _Items.Count; i++)
                _Items[i].GetComponent<MeshCollider>().enabled = false;
        }
    }
    public void OnBoxColider()
    {
        if (!_SphereTagMode)
        {
            for (int i = 0; i < _Items.Count; i++)
                _Items[i].GetComponent<MeshCollider>().enabled = true;
        }
    }
    public void OnSphereTagOn()
    {
        OffBoxColider();
        _SphereTagMode = true;
        SphereOn();
    }
    public void OnSphereTagOFF()
    {
         _SphereTagMode = false;
         OnBoxColider();
         SphereOff();
    }
    public void SphereOff()
    {
        if (_SphereTags.Count > 0)
        {
            for (int i = 0; i < _SphereTags.Count; i++)
            {
                _SphereTags[i].TagOff();
                _SphereTags[i].GetComponent<SphereCollider>().enabled = false;
            }
        }
    }
    public void SphereMeshRenderOff()
    {
        if (_SphereTags.Count > 0)
        {
            for (int i = 0; i < _SphereTags.Count; i++)
            {
                _SphereTags[i].TagOff();
            }
        }
    }
    public void SphereOn()
    {
        if (_SphereTags.Count > 0)
        {
            for (int i = 0; i < _SphereTags.Count; i++)
            {
                _SphereTags[i].TagOn();
                _SphereTags[i].GetComponent<SphereCollider>().enabled = true;
            }
        }
    }
    public void SphereMeshRenderOn()
    {
        if (_SphereTags.Count > 0)
        {
            for (int i = 0; i < _SphereTags.Count; i++)
            {
                _SphereTags[i].TagOn();
            }
        }
    }

    public void UICamerChange()
    {
        _UICamera.orthographic =false;
        _UICamera.nearClipPlane = _OBCamera.nearClipPlane;
        _UICamera.fieldOfView = _OBCamera.fieldOfView;
        _UICamera.farClipPlane = _OBCamera.farClipPlane;
    }
    public void UICamerReturn()
    {
        if (!_SphereTagMode)
        {
            _UICamera.orthographic = true;
            _UICamera.nearClipPlane = _UInearClipPlane;
            _UICamera.orthographicSize = _UIsize;
            _UICamera.farClipPlane = _UIfarClipPlane;
        }
    }
    private void SET( GameObject ga, VSTS_Group Item)
    {
        if (!ga.GetComponent<MeshCollider>())
             ga.AddComponent<MeshCollider>();
        if (!ga.GetComponent<UIitemidentity>())
        {
            ga.AddComponent<UIitemidentity>();
            var itemidenti = ga.GetComponent<UIitemidentity>();
            if (_ItemInformations_Group.Count > 0)
            {
                itemidenti.SetEquipDesc(_ItemInformations_Group[Item.BM_group_code].EquipDesc);
                itemidenti.SetGroupCode(Item.BM_group_code);
                itemidenti.SetNameDesc(_ItemInformations_Group[Item.BM_group_code].NameDesc);
                itemidenti.SetHighlightObject(_ItemInformations_Group[Item.BM_group_code].displaymeshes_OB);
            }
            itemidenti.SetInformationDirectorOB(this);
            ga.AddComponent<PointerEvents>();
        }
        else
        {
            if (!ga.GetComponent<PointerEvents>())
                ga.gameObject.AddComponent<PointerEvents>();
            var itemidenti = ga.GetComponent<UIitemidentity> ();
            if (_ItemInformations_Group.Count > 0)
            {
                if (itemidenti.GetGroupCode == string.Empty)
                itemidenti.SetGroupCode(Item.BM_group_code);
                if (itemidenti.GetNameDesc == string.Empty)
                    itemidenti.SetNameDesc(_ItemInformations_Group[Item.BM_group_code].NameDesc);
                if (itemidenti.GetHighlightObject == null)
                    itemidenti.SetHighlightObject(_ItemInformations_Group[Item.BM_group_code].displaymeshes_OB);
                if(itemidenti.GetEquipDesc ==string.Empty)
                    itemidenti.SetEquipDesc(_ItemInformations_Group[Item.BM_group_code].EquipDesc);
            }
            itemidenti.SetInformationDirectorOB(this);
        }
    }
    public void SetContents_Parent(ref List<string> heads, ref List<string> bodys, VSTS_GroupInfo vstsgroupinfo)
    {
        heads.Clear();
        bodys.Clear();

        if (vstsgroupinfo.FuncDesc != null)
        {
            heads.Add("\u25C6" + "기능");
            bodys.Add(vstsgroupinfo.FuncDesc);
        }
        if (vstsgroupinfo.SpecDesc != null)
        {
            heads.Add("\u25C6" + "주요제원");
            bodys.Add(vstsgroupinfo.SpecDesc);
        }
    }
    private List<VSTS_Group> itemInformationMethod()
    {
        List<VSTS_Group> vsts = new List<VSTS_Group>(); 
        string filePath = Path.Combine(Application.streamingAssetsPath, _GroupPath);

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            // JsonUtility를 사용하여 JSON 데이터 파싱
            vsts = JsonConvert.DeserializeObject<List<VSTS_Group>>(jsonData);
        }
        return vsts;
    }
    private List<VSTS_GroupInfo> GetVSTS_GroupInfoFromJSON()
    {
        List<VSTS_GroupInfo> vsts = new List<VSTS_GroupInfo>();
        string filePath = Path.Combine(Application.streamingAssetsPath, _GroupInfoPath);

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            // JsonUtility를 사용하여 JSON 데이터 파싱
            vsts = JsonConvert.DeserializeObject<List<VSTS_GroupInfo>>(jsonData);
        }
        return vsts;
    }
    public void UICusorChange_UI()
    {
        if (_Righthand.transform.GetComponentInChildren<UICursor>())
        {
            _Uicusor = _Righthand.transform.GetComponentInChildren<UICursor>().gameObject;
            _Uicusor.layer = 5;
        }
        else
        {
            _Uicusor = _Righthand.transform.transform.GetChild(0).gameObject;
            _Uicusor.layer = 5;
        }
    }
    public void UICusorChange_DEFAULT()
    {
        if (_Uicusor != null)
       _Uicusor.layer =0; 
    }
    private void SetSphereTag(Transform  transform, UIitemidentity ga)
    {
        var tag = GameObject.Instantiate(_SphereTagPrefab, _SphereParents.transform);
        tag.SetItemidentity(ga);
        tag.SetItemDirector(GetComponent<UIItemInformationDirector>());
        _SphereTags.Add(tag);
        var center = transform.GetComponent<Collider>().bounds.center;
        tag.transform.position = center;
    }
    private void FixedUpdate()
    {
        var axixvalue = _SmoothLocomotion.MoveAction.action.ReadValue<Vector2>();
        if (axixvalue.magnitude > 0)
        {
            _Righthand.gameObject.SetActive(false);
            if (!_FirstLoopCheck)
            {
               var li  = _Items.FindAll(x => x.layer == 5);
                for (int i = 0; i < li.Count; i++)
                {
                    li[i].GetComponent<PointerEvents>().OnPointerExitEvent.Invoke(eventData);
                    if(i == li.Count-1)
                        _FirstLoopCheck = true;
                }
                if (_SphereTagMode)
                {
                    for (int i = 0; i < _SphereTags.Count; i++)
                    {
                        if (_Player != null && _SphereTags[i] != null)
                            _SphereTags[i].gameObject.layer = 12;
                        if (i == _SphereTags.Count - 1)
                            _FirstLoopCheck = true;
                    }
                }
            }
        }
        else
        {
            _Righthand.gameObject.SetActive(true);
            _FirstLoopCheck = false;
            if (_SphereTagMode)
            {
                for (int i = 0; i < _SphereTags.Count; i++)
                {
                    if (_Player != null && _SphereTags[i] != null)
                    {
                        float di = Vector3.Distance(_Player.transform.position, _SphereTags[i].transform.position);
                        if (_LimitedDistance >= Math.Abs(di))
                            _SphereTags[i].gameObject.layer = 5;
                        else
                            _SphereTags[i].gameObject.layer = 12;
                    }
                }
            }
        }
    }
    
    public void Laseroff() => _Righthand.GetComponent<UIPointer_Custom>()._Laser.gameObject.SetActive(false);
    public void Laseron() => _Righthand.GetComponent<UIPointer_Custom>()._Laser.gameObject.SetActive(true);
}

