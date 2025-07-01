using BNG;
using Newtonsoft.Json;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using VSTS;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;
using System;
using System.IO;
using Unity.Collections;

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
    [SerializeField] private Camera _InformationCamera;
    [SerializeField] private UIItemInformationSphereTag _SphereTagPrefab;
    [SerializeField] private List<UIItemInformationSphereTag> _SphereTags;
    [SerializeField] private GameObject _SphereParents;
    [SerializeField] private Transform _Righthand;
    private GameObject _Uicusor;
    private float _UIfarClipPlane = 100.0f;
    private float _UInearClipPlane = 0.3f;
    private float _UIsize = 3.0f;
    private float _LimitedDistance = 15.0f;
    private Transform _Player;
    private SmoothLocomotion _SmoothLocomotion;
    private PointerEventData eventData;
    private UIItemInformationManager _Nowifmanager;
    private bool _FirstLoopCheck = false;
    private bool _SelectModelShowing = false;
    [SerializeField] private bool _SphereTagMode = false;
    [SerializeField] private bool _PopupShowing = false;
    private VSTS_GroupInfo VSTSGroupInfoTemp;
    public bool GetSphereTagMode { get => _SphereTagMode; }
    public List<GameObject> GetItem { get => _Items; }
    public GameObject GetUicusor { get => _Uicusor; }
    public List<VSTS_Group> GetItemGroups { get => _ItemGroups; }
    public C_ACC_Subtype GetSubmarineData { get => _SubmarineData; }
    public ItemInformationsGroup GetItemInformationsGroup { get => _ItemInformations_Group; }
    public UIItemInformationManager GetNowifmanager { get => _Nowifmanager; }
    public void SetSubmarineData(C_ACC_Subtype submarineData) => _SubmarineData = submarineData;
    public void SetInformationUIShowing(bool sh) => _PopupShowing = sh;
    public bool GetinformationUIShowing { get => _PopupShowing; }
    [Title("테이블 셋팅(테이블 입력)")]
    [InfoBox("테이블 업데이트시 초기화 버튼 순서대로 누르고 테이블 입력 버튼 순서대로 눌러주세요\n(그렇지 않을경우 에러가 나거나 displaymesh 오브젝트가 제대로 안들어 가서 오브젝트가 선택이 안된것처럼 보일수 있습니다)")]
    [Button("테이블 입력 버튼 1번 GroupDictionary셋팅")]
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
            if (gr[i].img_filename == "null")
                vSTS_GroupInfo.img_filename = string.Empty;
            else
                vSTS_GroupInfo.img_filename = gr[i].img_filename;
            _ItemInformations_Group.Add(gr[i].BM_group_code, vSTS_GroupInfo);
        }
    }
    [Button("테이블 입력 버튼 2번 ItemGroup입력")]
    public void SetItemGroup()
    {
        _ItemGroups = itemInformationMethod();
    }
    [Button("테이블 입력 버튼 3번 아이템 제원 입력")]
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
                            if (!CH.GetComponent<UIItemInformationSphereTag>())
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
    [Button("테이블 입력 버튼 4번 태그 붙이기")]
    public void SetTag()
    {
        if (_SphereTags.Count > 0)
            ClearTag();
        _ItemGroups = itemInformationMethod();
        for (int i = 0; i < _ItemGroups.Count; i++)
        {
            var ga = GameObject.Find(_ItemGroups[i].ObjectMeshName);
            if (ga != null)
            {
                var ch = ga.transform.GetComponentsInChildren<UIitemidentity>().ToList();
                var othergroup = ch.Find(x => x.GetGroupCode != ga.GetComponent<UIitemidentity>().GetGroupCode);
                if (ga.transform.childCount == 0 && othergroup == null && ga.GetComponent<MeshRenderer>())
                    SetSphereTag(ga.transform, ga.GetComponent<UIitemidentity>());
                else if (ga.transform.childCount > 0 && othergroup == null)
                    SetSphereTag(ga.transform.GetChild(0), ga.GetComponent<UIitemidentity>());
            }
        }
    }
    [Title("테이블 셋팅(초기화)")]
    [InfoBox("테이블 업데이트시 초기화 버튼 순서대로 누르고 테이블 입력 버튼 순서대로 눌러주세요\n(그렇지 않을경우 에러가 나거나 displaymesh 오브젝트가 제대로 안들어 가서 오브젝트가 선택이 안된것처럼 보일수 있습니다)")]
    [Button("초기화 버튼 1번 GroupDictionary삭제")]
    public void GroupDictionary_CLEAR()
    {
        _ItemInformations_Group.Clear();
    }
    [Button("초기화 버튼 2번 ItemGroup 삭제")]
    public void ClearItemGroup()
    {
        _ItemGroups.Clear();
    }
   
    [Button("초기화 버튼 3번 아이템 제원 전부 삭제")]
    public void clear()
    {
        if (_Items.Count > 0)
        {
            Debug.Log(_Items.Count.ToString());
            for (int i = 0; i < _Items.Count; i++)
            {
                if (_Items[i].gameObject != null)
                {
                    var component = _Items[i].GetComponent<UIitemidentity>();
                    if (component != null)
                        DestroyImmediate(component);
                    var point = _Items[i].GetComponent<PointerEvents>();
                    if (point != null)
                        DestroyImmediate(point);
                }
            }
            _Items.Clear();
        }
    }
   
    [Button("초기화 버튼 4번 태그 삭제")]
    public void ClearTag()
    {
        for (int i = 0; i < _SphereTags.Count; i++)
            DestroyImmediate(_SphereTags[i].gameObject);
        _SphereTags.Clear();
    }
    [Title("콜라이더 관련 버튼")]
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
    [Button("모든 메쉬 콜라이더 트리거 delete")]
    public void MeshColiderTrrigerdelete()
    {
        if (_Items.Count > 0)
        {
            for (int i = 0; i < _Items.Count; i++)
            {
                var component = _Items[i].GetComponent<MeshCollider>();
                GameObject.DestroyImmediate(component);
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
    [Title("일치 불일치 여부 목록파일 저장 경로")]
    [InfoBox("Groupinfo에 있는 displaymesh 목록과 맵안에있는 오브젝트 이름이 일치 할경우 일치라고 나오고 불일치일경 오브젝트 메쉬 네임이 그대로 나오는 csv파일 저장경로입니다")]
    public string MakingThePath;
    [Title("일치 불일치 여부 목록파일 만드는 버튼")]
    [Button("일치하는 모델 안 일치하는 모델목록.csv ")]
    [InfoBox("Groupinfo에 있는 displaymesh 목록과 맵안에있는 오브젝트 이름이 일치 할경우 일치라고 나오고 불일치일경 오브젝트 메쉬 네임이 그대로 나오는 csv파일을 만드는 버튼입니다")]
    public void makingthecsv()
    {
        var item = GetVSTS_GroupInfoFromJSON();
        var checkdictionary = item.Select(x => x.display_meshes).ToList();
        var di = item.Select(x => x.display_meshes).ToList();


        for (int i = 0; i < di.Count; i++)
        {
            var at = di[i].Split(';');

            for (int j = 0; j < at.Length; j++)
            {
                var ga = GameObject.Find(at[j]);
                if (ga != null)
                    at[j] = "있음";
            }
            string ch = String.Join(";", at);
            checkdictionary[i] = ch;
        }
        using (System.IO.StreamWriter st = new System.IO.StreamWriter(MakingThePath, false, System.Text.Encoding.UTF8))
        {
            st.WriteLine("ITEMINFORMATION_GROUP에 있는 displaymesh목록,일치 불일치 여부");
            for (int i = 0; i < checkdictionary.Count; i++)
            {

                st.WriteLine(di[i] + "," + checkdictionary[i]);
            }
            st.Close();
        }
    }
    private void Start()
    {    
        OnSphereTagOFF();
        UICamerChange();
        _Player = GameObject.FindObjectOfType<CharacterController>().transform;
        _SmoothLocomotion = _Player.GetComponent<SmoothLocomotion>();
        _Uicusor = _Righthand.transform.GetComponentInChildren<UICursor>()? _Righthand.transform.GetComponentInChildren<UICursor>().gameObject:
         _Righthand.transform.transform.GetChild(0).gameObject;
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
        if (!_SphereTagMode&&!_SelectModelShowing)
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
                if (Item.BM_group_code != null && _ItemInformations_Group.TryGetValue(Item.BM_group_code, out VSTSGroupInfoTemp))
                {
                    itemidenti.SetEquipDesc(_ItemInformations_Group[Item.BM_group_code].EquipDesc);
                    itemidenti.SetGroupCode(Item.BM_group_code);
                    itemidenti.SetImageName(_ItemInformations_Group[Item.BM_group_code].img_filename);
                    itemidenti.SetNameDesc(_ItemInformations_Group[Item.BM_group_code].NameDesc);
                    itemidenti.SetHighlightObject(_ItemInformations_Group[Item.BM_group_code].displaymeshes_OB);
                }
                else
                   if (Item.BM_group_code != null)
                    Debug.Log($"{Item.BM_group_code}는 VSTS_GroupInfo에 있는 BM_group_code중에 일치하는게 없습니다");
                 else
                    Debug.Log($"{Item.ObjectMeshName}에 BM_group_code가 없습니다");
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
                if (Item.BM_group_code != null && _ItemInformations_Group.TryGetValue(Item.BM_group_code, out VSTSGroupInfoTemp))
                {
                    if (itemidenti.GetGroupCode == string.Empty)
                        itemidenti.SetGroupCode(Item.BM_group_code);
                    if (itemidenti.GetNameDesc == string.Empty)
                        itemidenti.SetNameDesc(_ItemInformations_Group[Item.BM_group_code].NameDesc);
                    if (itemidenti.GetHighlightObject == null)
                        itemidenti.SetHighlightObject(_ItemInformations_Group[Item.BM_group_code].displaymeshes_OB);
                    if (itemidenti.GetEquipDesc == string.Empty)
                        itemidenti.SetEquipDesc(_ItemInformations_Group[Item.BM_group_code].EquipDesc);
                    if (itemidenti.GetImageName == string.Empty)
                        itemidenti.SetImageName(_ItemInformations_Group[Item.BM_group_code].img_filename);
                }
                else
                {
                    if(Item.BM_group_code != null)
                        Debug.Log($"{Item.BM_group_code}는 VSTS_GroupInfo에 있는 BM_group_code중에 일치하는게 없습니다");
                    else
                        Debug.Log($"{Item.ObjectMeshName}에 BM_group_code가 없습니다");
                }
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
         var text =  Resources.Load(_GroupPath)as TextAsset;
        string filePath =text.text;
        if (text != null)
        {
            // JsonUtility를 사용하여 JSON 데이터 파싱
            vsts = JsonConvert.DeserializeObject<List<VSTS_Group>>(filePath);
        }
        return vsts;
    }
    private List<VSTS_GroupInfo> GetVSTS_GroupInfoFromJSON()
    {
        List<VSTS_GroupInfo> vsts = new List<VSTS_GroupInfo>();
        var text = Resources.Load(_GroupInfoPath) as TextAsset;
        string filePath = text.text;
        if (text != null)
        {
            // JsonUtility를 사용하여 JSON 데이터 파싱
            vsts = JsonConvert.DeserializeObject<List<VSTS_GroupInfo>>(filePath);
        }
        return vsts;
    }
   
    private void SetSphereTag(Transform  transform, UIitemidentity ga)
    {
        var tag = GameObject.Instantiate(_SphereTagPrefab, _SphereParents.transform);
        tag.SetItemidentity(ga);
        tag.SetItemDirector(GetComponent<UIItemInformationDirector>());
        tag.gameObject.layer = 12;
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
               var li  = _Items.FindAll(x => x.layer == 12);
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
                            _SphereTags[i].gameObject.layer = 11;
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
                            _SphereTags[i].gameObject.layer = 12;
                        else
                            _SphereTags[i].gameObject.layer = 11;
                    }
                }
            }
        }
       
    }
    
    public void Laser(bool active) => _Righthand.GetComponent<UIPointer_Custom>()._Laser.gameObject.SetActive(active);
    public void SelectModelshowing(bool SelectModel) => _SelectModelShowing= SelectModel;

    
}

