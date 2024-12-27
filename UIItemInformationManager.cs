using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using VSTS;

public class UIItemInformationManager : MonoBehaviour
{
    [SerializeField] private string _ItemName = string.Empty;
    [SerializeField] private string _ParentItemName = string.Empty;
    [SerializeField] private string _ParentGroupName = string.Empty;
    [SerializeField] private string _NowGroupName = string.Empty;
    [SerializeField] private string _EquipDesc = string.Empty;
    [SerializeField] private string _ParentEquipDesc = string.Empty;
    [SerializeField] private Text _ItemNameText;
    [SerializeField] private Text _EquipDescText;
    [SerializeField] private GameObject _DetailInformationTextPrefab;
    [SerializeField] private GameObject _Contents;
    [SerializeField] private GameObject _headButtonPrefab;
    [SerializeField] private List<GameObject> _Headbutton;
    [SerializeField] private List<GameObject> _ExplainOB;
    [SerializeField] private List<string> _Head;
    [SerializeField] private List<string> _Explain;
    [SerializeField] private List<string> _ParentHead;
    [SerializeField] private List<string> _ParentExplain;
    [SerializeField] private GameObject _Parent;
    [SerializeField] private GameObject _Revert;
    [SerializeField] private float _LimittedHeight;
    [SerializeField] private float _Space;
    [SerializeField] private UIItemInformationDirector _ItemDirector;
    [SerializeField] private Image _Xbuton;
    private Stack<Itemstack> _PrevStack = new Stack<Itemstack>();
    public void SetHead(List<string> head) => _Head = head;
    public void SetExplain(List<string> explain) => _Explain = explain;
    public void SetParentHead(List<string> head) => _ParentHead = head;
    public void SetParentExplain(List<string> explain) => _ParentExplain = explain;
    public void SetItemDirector(UIItemInformationDirector ItemDirector) =>  _ItemDirector = ItemDirector;
    public void SetActiveExplainOB(int n)
    {
        for (int i = 0; i < _ExplainOB.Count; i++)
        {
            if (i == n)
            {
                _ExplainOB[i].SetActive(true);
                _Headbutton[i].GetComponent<Image>().color = Color.white;
            }
            else
            {
                _ExplainOB[i].SetActive(false);
                _Headbutton[i].GetComponent<Image>().color = Color.black;
            }
        }
    }
    public void SetParentactive(bool active) =>_Parent.SetActive(active);
    
    public void SetRevertActive(bool active) => _Revert.SetActive(active);
    
    public void SetheTitle(string title) => _ItemName=title;
    public void SetheParentTitle(string title) => _ParentItemName = title;
    public void SetParentGroupName(string pg) => _ParentGroupName = pg;
    public void SetNowGroupName(string ng) => _NowGroupName = ng;
    public void SetParentEquipDesc(string pe) => _ParentEquipDesc = pe;
    public void SetEquipDesc(string ed) => _EquipDesc = ed;
    public void ClearPrevStack() => _PrevStack.Clear();
    public void ClearInformationlistandGameObject()
    {
        for (int i = 0; i < _Headbutton.Count; i++)
        {
            Destroy(_Headbutton[i].gameObject);
            Destroy(_ExplainOB[i].gameObject);
        }
        _Headbutton.Clear();
        _ExplainOB.Clear();
    }
    public void SetParent()
    {
       _PrevStack.Push(new Itemstack(_ItemNameText.text, _NowGroupName, _EquipDesc, _Head,_Explain));
        ClearInformationlistandGameObject();
        _Revert.transform.GetChild(0).GetComponent<Text>().text = _ItemName;
        SetDataChange(_ParentHead, _ParentExplain, _ParentItemName, _ParentGroupName, _ParentEquipDesc, true);
        _Revert.gameObject.SetActive(true);
        inite();
    }
    public void SetChild()
    {
        var previtem =  _PrevStack.Pop();
        ClearInformationlistandGameObject();
        SetDataChange(previtem.Head,previtem.Explain,previtem.NameDesc,previtem.GroupName,previtem.EquipDesc,false);
        inite();
        if (_PrevStack.Count > 0)
        {
            var li =_PrevStack.ToList();
            _Revert.GetComponentInChildren<Text>().text = li[li.Count - 1].NameDesc;
            _Revert.gameObject.SetActive(true);
        }
        else
        _Revert.gameObject.SetActive(false);
    }
    private void SetDataChange(List<string> Head ,List<string> Explain ,string NameDesc, string GroupName,string EquipDesc , bool parent )
    {
        _Head.Clear();
        _Head.AddRange(Head);
        _Explain.Clear();
        _Explain.AddRange(Explain);
        _ItemName = NameDesc;
        _NowGroupName = GroupName;
        _EquipDesc = EquipDesc;
        string Group = string.Empty;
        if (parent)
            Group = _ParentGroupName;
        else
            Group = _NowGroupName;
        if (_ItemDirector.GetItemInformationsGroup[Group].parent_group != string.Empty)
        {
            var head = new List<string>();
            var body = new List<string>();
            _ItemDirector.SetContents_Parent(ref head, ref body, _ItemDirector.GetItemInformationsGroup[Group]);
            _ParentHead.Clear();
            _ParentHead.AddRange(head);
            _ParentExplain.Clear();
            _ParentExplain.AddRange(body);
            _Parent.transform.GetChild(0).GetComponent<Text>().text = _ParentItemName;
            _ParentGroupName = _ItemDirector.GetItemInformationsGroup[Group].parent_group;
            _ParentItemName = _ItemDirector.GetItemInformationsGroup[_ParentGroupName].NameDesc;
            _ParentEquipDesc = _ItemDirector.GetItemInformationsGroup[_ParentGroupName].EquipDesc;
            _Parent.gameObject.SetActive(true);
        }
        else
        {
            _ParentHead.Clear();
            _ParentExplain.Clear();
            _ParentItemName = string.Empty;
            _ParentEquipDesc = string.Empty;
            _ParentGroupName = string.Empty;
            _Parent.gameObject.SetActive(false);
        }
    }
    public void inite()
    {
        if (InputManager.Instance.InputType == E_INPUT_DEVICE.PC)
            InputManager.Instance.IsCursorVisible = true;
        if(_ParentItemName != string.Empty)
            _Parent.GetComponentInChildren<Text>().text = _ParentItemName;
        _ItemNameText.text = _ItemName;
        _EquipDescText .text = _EquipDesc;
        for (int i = 0; i < _Head.Count; i++)
        {
            var detailitemtext = Instantiate(_DetailInformationTextPrefab, _Contents.transform);
            var button= GameObject.Instantiate(_headButtonPrefab, _headButtonPrefab.transform.parent);
            button.GetComponentInChildren<Text>().text = _Head[i];
            button.GetComponentInChildren<UIButtonClickScript>().SetIndex(i);
            _Headbutton.Add(button);
            button.gameObject.SetActive(true);
            if(i ==0)
            button.GetComponent<Image>().color = Color.white;
            detailitemtext.transform.GetChild(1).GetComponent<Text>().text = _Explain[i] ;
            Makealength( detailitemtext);
        }

    }
   
    public void OnPointerDownEnter()
    {
        _ItemDirector.OffBoxColider();
        _ItemDirector.UICusorChange_UI();
       if (gameObject.transform.GetChild(0).gameObject.activeSelf == false)
       {
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
            inite();
        }
       else
            inite();
    }
   
    public void ActiveOnOFF()
    {
        if(InputManager.Instance.InputType == E_INPUT_DEVICE.PC)
            InputManager.Instance.IsCursorVisible = false;

        if (gameObject.transform.GetChild(0).gameObject.activeSelf == true)
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
        if (!_ItemDirector.GetSphereTagMode)
            _ItemDirector.OnBoxColider();
        else
            _ItemDirector.SphereOn();
        _ItemDirector.UICusorChange_DEFAULT();
        _ItemDirector.Laseron();
        _ItemDirector.UICamerChange();
        _Xbuton.color = Color.white;
        _ItemDirector.SetInformationUIShowing(false);
    }
    public void ActiveOnOFF_HOVER()
    {
        _Xbuton.color = Color.gray;
    }
    public void ActiveOnOFF_EXIT()
    {
        _Xbuton.color = Color.white;
    }
    private void Makealength(GameObject detailitemtext)
    {
        float size1= detailitemtext.transform.GetChild(1).GetComponent<Text>().preferredHeight + _Space;
        float sizef = 0.0f;
        if (_LimittedHeight < size1)
            sizef =size1;
        else
            sizef = _LimittedHeight;
        detailitemtext.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(detailitemtext.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta.x, sizef);
        detailitemtext.GetComponent<RectTransform>().sizeDelta = new Vector2(detailitemtext.GetComponent<RectTransform>().sizeDelta.x, sizef);
        _ExplainOB.Add(detailitemtext);
        _ExplainOB[0].SetActive(true);
    }
}
public class Itemstack
{
    public string NameDesc = string.Empty;
    public string EquipDesc = string.Empty;
    public string GroupName = string.Empty;
    public List<string> Head = new List<string>();
   public List<string> Explain = new List<string>();
    public Itemstack(string ND , string GN, string equip,List<string> He, List<string> Ex)
    {
        NameDesc = ND;
        Head.AddRange(He);
        Explain.AddRange(Ex);
        GroupName = GN;
        EquipDesc = equip;
    }
}
