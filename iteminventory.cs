using BNG;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VSTS;

public class iteminventory : MonoBehaviour
{
    [SerializeField] private List<InventoryIdenty> _InventoryList;
    [SerializeField] private List<InventoryParent> _ParentList;
    [SerializeField] private GameObject _NextButton;
    [SerializeField] private GameObject _PreviewButton;
    [SerializeField] private GameObject _Hand;
    [SerializeField] private float _Radius =3;
    [SerializeField] private Transform _Parent;
    [SerializeField] private int _MaxiumInventorySlot = 8;
    [SerializeField] private Transform _Followingobject;
    [SerializeField] private Text _ItemName;
    [SerializeField] private Grabber _LeftGrabber;
    [SerializeField] private Grabber _RightGrabber;
    public float GetMaxiumInventorySlot { get => _MaxiumInventorySlot; }
    public List<InventoryIdenty> GetInventoryList { get => _InventoryList; }
    public void SetItemName(string iname) => _ItemName.text = iname;
    public float GetRadius { get => _Radius; }
    private int _PageIndex=0;
    private bool _RunTask;
    private GameObject _HoveringItme;
    private InventoryIdenty _SelectItem;
    private CharacterController _Player;
    private float _CircleAngle = 360.0f;
    private float _LimittedValue = 0.3F;
    private float _Angle;
    private bool _itemSelecting;
    private InventoryParent _NowParent;
    [SerializeField] private bool _IsPc=false;
    public float GetCircleAngle { get => _CircleAngle; }
    private void OnEnable()
    {
        _Angle = _CircleAngle / _MaxiumInventorySlot;
        _RunTask = true;
        _Player = GameObject.FindAnyObjectByType<CharacterController>();
        var grabs = GameObject.FindObjectsOfType<Grabber>().ToList();
        _RightGrabber = grabs.Find(x => x.HandSide == ControllerHand.Right);
        _RightGrabber.onReleaseEvent.AddListener(Drop);
        _LeftGrabber = grabs.Find(x => x.HandSide == ControllerHand.Left);
        _LeftGrabber.onReleaseEvent.AddListener(Drop);
    }
    private void OnDisable()
    {
        _RunTask = false;
    }
    private void Start()
    {
        FollowingThePosition().Forget();
    }
    public void FindItem(string obname)
    {
       var IT = _InventoryList.Find(x => x.GetName == obname);
        IT.GetComponent<Rigidbody>().isKinematic = true;
        IT.gameObject.SetActive(true);
        _HoveringItme = IT.gameObject; 
    }
    public void Resetitem(string obname)
    {
        _HoveringItme = null;
        var ob= _InventoryList.Find(x => x.GetName == obname);
        ob.gameObject.SetActive(false);
        _ItemName.text =string.Empty;
    }
 
    public void CloseInventory()
    {
        for (int i = 0; i < _ParentList.Count; i++)
            _ParentList[i].gameObject.SetActive(false);
        _NextButton.gameObject.SetActive(false);
        _PreviewButton.gameObject.SetActive(false);
        _ItemName.transform.parent.gameObject.SetActive(false);
        _ItemName.gameObject.SetActive(false);
        _Player.GetComponent<SmoothLocomotion>().enabled = true;
        _Player.GetComponent<LocomotionManager>().enabled = true;
        _Player.GetComponent<PlayerElevator>().enabled = true;
    }
    private async UniTask FollowingThePosition()
    { 
       while (_RunTask)
       {
           float Angle = Mathf.Atan2(InputBridge.Instance.LeftThumbstickAxis.y, InputBridge.Instance.LeftThumbstickAxis.x) * Mathf.Rad2Deg;
           float cursorangle = Angle > 0 ? (_CircleAngle/2.0f) -Angle: (_CircleAngle / 2.0f) + Mathf.Abs(Angle);
           if (_NowParent != null&&_itemSelecting&&!_IsPc)
           {
               var exit = _NowParent.GetSphereExitButon;
               if (InputBridge.Instance.LeftThumbstickAxis.y>0.0F)
               {
                   SetObject(_NowParent, cursorangle, exit, InputBridge.Instance.LeftThumbstickAxis.y,_LimittedValue);
               }
               else if (InputBridge.Instance.LeftThumbstickAxis.y < 0.0f)
               {
                   SetObject(_NowParent, cursorangle, exit, InputBridge.Instance.LeftThumbstickAxis.y, -1.0F*_LimittedValue);
               }
               else if (InputBridge.Instance.LeftThumbstickAxis.x > 0.0f)
               {
                   SetObject(_NowParent, cursorangle, exit, InputBridge.Instance.LeftThumbstickAxis.x, _LimittedValue);
               }
               else if (InputBridge.Instance.LeftThumbstickAxis.x < 0.0F)
               {
                   SetObject(_NowParent, cursorangle, exit, InputBridge.Instance.LeftThumbstickAxis.x, -1.0F * _LimittedValue);
               }
               else if (InputBridge.Instance.LeftThumbstickAxis.y <0.1f&& InputBridge.Instance.LeftThumbstickAxis.y >= 0.0f ||
                   InputBridge.Instance.LeftThumbstickAxis.x<0.1f && InputBridge.Instance.LeftThumbstickAxis.x >=0.0f)
               {
                   _ItemName.text = string.Empty;
                    ResetHoveringitem();
                   for (int i = 0; i < _NowParent.GetInventoryButtonList.Count; i++)
                       _NowParent.GetInventoryButtonList[i].SetMaterial(_NowParent.GetInventoryButtonList[i].GetOriginalMaterial);
                   exit.MaterialChange(exit.GetHighlightMaterial);
               }
           }
          
            transform.position = _Followingobject.position;
            var parent =_Player.transform.GetChild(0);
            float x = _CircleAngle - parent.transform.localEulerAngles.x;
            transform.localEulerAngles = new Vector3(x,_Player.transform.localEulerAngles.y,0);
            if (Input.GetKeyDown(KeyCode.I)|| InputBridge.Instance.BButtonDown)
            {
               var ob= _ParentList.Find(x => x.gameObject.activeSelf);
                if (ob == null)
                {
                    _PageIndex = 0;
                    if (_ParentList.Count > 0)
                    {
                        _ItemName.transform.parent.gameObject.SetActive(true);
                        _ItemName.gameObject.SetActive(true);
                        _itemSelecting = true;
                    }
                    PageActive();
                    for (int i = 0; i < _InventoryList.Count; i++)
                        _InventoryList[i].gameObject.SetActive(false);
                    _Player.GetComponent<SmoothLocomotion>().enabled = false;
                    _Player.GetComponent<LocomotionManager>().enabled =false;
                    _Player.GetComponent<PlayerElevator>().enabled = false;
                }
                else
                {
                    ResetHoveringitem();
                    if (_itemSelecting)
                    {
                        _itemSelecting = false;
                       for (int i = 0; i < _InventoryList.Count; i++)
                            _InventoryList[i].gameObject.SetActive(false);
                    }
                    CloseInventory();
                }
            }
            if (_HoveringItme != null)
                _HoveringItme.transform.position = _Hand.transform.position;
            if (InputBridge.Instance.RightGripDown || InputBridge.Instance.LeftTriggerDown)
            {
                if (_HoveringItme != null)
                {
                    _SelectItem = _InventoryList.Find(x => x.GetName == _HoveringItme.GetComponent<InventoryIdenty>().GetName);
                    _SelectItem.gameObject.SetActive(true);
                    _HoveringItme = null;
                    CloseInventory();
                    _itemSelecting = false;
                }
                else
                {
                    if(!_IsPc)
                    CloseInventory();
                    _itemSelecting = false;
                }
            }
            
            await UniTask.Yield();
       }
    }
         
    public void NextInventory()
    {
        _PageIndex++;
        _PageIndex =  _PageIndex < _ParentList.Count ? _PageIndex: _ParentList.Count-1;
        ResetHoveringitem();
        PageActive();
      
    }
    public void PreviewInventory()
    {
        _PageIndex--;
        _PageIndex = _PageIndex >= 0 ? _PageIndex : 0;
        ResetHoveringitem();
        PageActive();
       
    }
    private void PageActive()
    {
        for (int i = 0; i < _ParentList.Count; i++)
        {
            if (_PageIndex == i)
            {
                _ParentList[i].gameObject.SetActive(true);
                _NowParent = _ParentList[i];
            }
            else
                _ParentList[i].gameObject.SetActive(false);
        }
        if (_ParentList.Count > 1)
        {
            if (_PageIndex == _ParentList.Count - 1)
            {
                _NextButton.gameObject.SetActive(false);
                _PreviewButton.gameObject.SetActive(true);
            }
            else if (_PageIndex == 0)
            {
                _NextButton.gameObject.SetActive(true);
                _PreviewButton.gameObject.SetActive(false);
            }
            else
            {
                _NextButton.gameObject.SetActive(true);
                _PreviewButton.gameObject.SetActive(true);
            }
        }
        _ItemName.text =string.Empty;
    }

    public void Adding(InventoryIdenty item)
    {
        if (!_InventoryList.Contains(item))
        {
           item.gameObject.SetActive(false);
            _InventoryList.Add(item);
            if (_ParentList.Count == 0)
            {
                var it = Instantiate(_Parent, _Parent.transform.parent);
                _ParentList.Add(it.GetComponent<InventoryParent>());
                it.GetComponent<InventoryParent>().AddInventoryItem(item);
            }
            else
            {
                var parent = _ParentList.Find(x => x.GetInventoryButtonList.Count != _MaxiumInventorySlot);
                if (parent == null)
                {
                    var it = Instantiate(_Parent, _Parent.transform.parent);
                    _ParentList.Add(it.GetComponent<InventoryParent>());
                    it.GetComponent<InventoryParent>().AddInventoryItem(item);
                }
                else
                    parent.AddInventoryItem(item);
            }
        }
    }
    public void Remove(InventoryIdenty item)
    {
        if (_ParentList.Count != 0)
        {
            _InventoryList.Remove(item);
            var parent = _ParentList.Find(x => x.GetInventoryButtonList.Find(x=>x.GetName == item.GetName));
            parent.DeletInventoryItem(item);
        }
    }
    public void ResetHoveringitem()
    {
        if (_HoveringItme != null)
        {
            _HoveringItme.gameObject.SetActive(false);
            _HoveringItme = null;
        }
    }
    private void SetObject(InventoryParent parentactive ,float cursorangle, InventoryExitButton exit,float value1,float value2 )
    {
        if (value1 > value2|| value1 < value2)
        {
            for (int i = 0; i < parentactive.GetInventoryButtonList.Count; i++)
            {
                if (parentactive.GetInventoryButtonList[i].transform.localEulerAngles.z < cursorangle &&
                    parentactive.GetInventoryButtonList[i].transform.localEulerAngles.z + _Angle > cursorangle)
                {
                    var ob = _InventoryList.Find(x => x.GetName == parentactive.GetInventoryButtonList[i].GetName);
                    ob.gameObject.SetActive(true);
                    _HoveringItme = ob.gameObject;
                    _ItemName.text = parentactive.GetInventoryButtonList[i].GetName;
                    parentactive.GetInventoryButtonList[i].SetMaterial(parentactive.GetInventoryButtonList[i].GetHighlightMaterial);
                }
                else
                {
                    var ob = _InventoryList.Find(x => x.GetName == parentactive.GetInventoryButtonList[i].GetName);
                    ob.gameObject.SetActive(false);
                    parentactive.GetInventoryButtonList[i].SetMaterial(parentactive.GetInventoryButtonList[i].GetOriginalMaterial);
                }
            }
            exit.MaterialChange(exit.GetOriginalMaterial);
        }
    }
    public void Drop(Grabbable grabbable)
    {
        if (_SelectItem.GetActiveOff)
        {
            var it = GetInventoryList.Find(x => x.GetName == _SelectItem.GetName);
            if (it != null)
                it.gameObject.SetActive(false);
        }
    }
}
