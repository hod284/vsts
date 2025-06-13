using BNG;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VSTS;

public class iteminventory : MonoBehaviour
{
    [SerializeField] private List<Inventoryitemidentity> _InventoryList;
    [SerializeField] private List<Vector3> _InventoryList_rotation;
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
    [SerializeField] private Networkinventory _NetWorkObjetManager;
    private UnityEngine.UI.Button _InventoryButton;
    private float _TwinkleSpeed = 1.0f;
    private float _TwinkleGolattime = 1.0f;
    private Color _TwinkleTargetColor = new Color(243.0f, 227.0f, 0.0f);
    private Color _TwinkleStartColor = Color.white;
    private bool _Twinkle = false;
    float _LastPress = 0f;
    float _Delay = 0.2f;
    private bool _IsActive = false;
    private Vector3 _RePoint = new Vector3(100, 100, 100);
    public float GetMaxiumInventorySlot { get => _MaxiumInventorySlot; }
    public bool GetIsActive { get => _IsActive; }
    public List<Inventoryitemidentity> GetInventoryList { get => _InventoryList; }
    public void SetItemName(string iname) => _ItemName.text = iname;
    public float GetRadius { get => _Radius; }
    public bool GetisPC { get => _IsPc; }
    public Networkinventory GetNetWorkObjetManager { get => _NetWorkObjetManager; }
    public Vector3 GetRePoint { get => _RePoint; }
    public GameObject GetHand { get => _Hand; }
    public  Grabber GetLeftGrabber { get => _LeftGrabber; }
    public Grabber GetRightGrabber { get => _RightGrabber; }
    private int _PageIndex=0;
    private GameObject _HoveringItme;
    [SerializeField] private Inventoryitemidentity _SelectItem;
    private CharacterController _Player;
    private float _CircleAngle = 360.0f;
    private float _LimittedValue = 0.3F;
    private float _Angle;
    private bool _itemSelecting;
    private InventoryParent _NowParent;
    [SerializeField] private bool _IsPc=false;
    
    public void SetSelectItem(Inventoryitemidentity inventoryitemidentity) => _SelectItem = inventoryitemidentity;
    public float GetCircleAngle { get => _CircleAngle; }

    public event Action OnAddedItem;
    private void Awake()
    {
        if (Unity.Netcode.NetworkManager.Singleton.IsClient|| Unity.Netcode.NetworkManager.Singleton.IsHost || Unity.Netcode.NetworkManager.Singleton.IsServer)
            _NetWorkObjetManager.gameObject.SetActive(true); 
        else
            _NetWorkObjetManager.gameObject.SetActive(false);

    }
    private void OnEnable()
    {
        switch (InputManager.Instance.InputType)
        {
            case E_INPUT_DEVICE.PC:
                gameObject.SetActive(_IsPc);
                break;
            case E_INPUT_DEVICE.VR:
                gameObject.SetActive(_IsPc ? false: true);
                break;
        }
        _Angle = _CircleAngle / _MaxiumInventorySlot;
        _Player = GameObject.FindAnyObjectByType<CharacterController>();
        var grabs = GameObject.FindObjectsOfType<Grabber>().ToList();
        _RightGrabber = grabs.Find(x => x.HandSide == ControllerHand.Right);
        _LeftGrabber = grabs.Find(x => x.HandSide == ControllerHand.Left);
        if (_Hand == null)
        {
            var hands = GameObject.FindObjectsOfType<HandController>().ToList();
            _Hand = hands.Find(x => x.HandAnchor.GetComponent<TrackedDevice>().Device == TrackableDevice.RightController).gameObject;
        }
        if (_Followingobject == null)
            _Followingobject = GameObject.FindObjectOfType<InventoryFollowingObject>().gameObject.transform;
    }
    public void FindItem(string obname)
    {
       var IT = _InventoryList.Find(x => x.GetName == obname);
        IT.GetComponent<Rigidbody>().isKinematic = true;
       _HoveringItme = IT.gameObject;
        IT.transform.position = _Hand.transform.position;
        IT.GetComponent<Inventoryitemidentity>().SetReturn(false);
    }
    public void Resetitem(string obname)
    {
        _HoveringItme = null;
        var ob= _InventoryList.Find(x => x.GetName == obname);
        ob.gameObject.transform.position = _RePoint;
        ob.GetComponent<Inventoryitemidentity>().SetReturn(true);
        _ItemName.text =string.Empty;
    }

    public void CloseInventory()
    {
        _RightGrabber.ForceGrab = true;
        _RightGrabber.ForceRelease = false;
        for (int i = 0; i < _ParentList.Count; i++)
            _ParentList[i].gameObject.SetActive(false);
        _NextButton.gameObject.SetActive(false);
        _PreviewButton.gameObject.SetActive(false);
        _ItemName.transform.parent.gameObject.SetActive(false);
        _ItemName.gameObject.SetActive(false);
        _Player.GetComponent<SmoothLocomotion>().enabled = true;
        _Player.GetComponent<LocomotionManager>().enabled = true;
        _Player.GetComponent<PlayerElevator>().enabled = true;
        if (_IsPc)
            InputManager.Instance.CursorActivate(false, 2);
       
      
        _IsActive = false;
    }
    public void Openinventory()
    {
        if (_InventoryList.Count == 0 && _ParentList.Count == 0)
        {
            var it = Instantiate(_Parent, _Parent.transform.parent);
            _ParentList.Add(it.GetComponent<InventoryParent>());
        }
        else if (_NetWorkObjetManager.gameObject.activeSelf)
        {
            foreach (var item in _InventoryList)
            {
                if (!item.GetComponent<NetworkGrabbable>().enabled)
                    item.GetComponent<NetworkGrabbable>().enabled = true;
            }
        }
        _IsActive = true;
        _RightGrabber.ForceGrab = false;
        if (_NetWorkObjetManager.gameObject.activeSelf&& _InventoryList.Count>0 )
        {
            var IT = _InventoryList.Find(X => X.GetComponent<NetworkGrabbable>().BeingHeld);
            if (IT != null)
                IT.GetComponent<NetworkGrabbable>().DropItem(false,false);
        }
        else
        {
          _RightGrabber.ForceRelease = true;
        }
        var listparent = GameObject.FindObjectOfType<Inventoryitemlist>();
        var _CharacterController = GameObject.FindObjectOfType<CharacterController>();
        var y = 360.0f - Mathf.Abs(_CharacterController.transform.localEulerAngles.y);
        listparent.transform.localEulerAngles = new Vector3(0, -y, 0);
        for(int i  =0; i< _InventoryList_rotation.Count; i++)
            _InventoryList[i].transform.localEulerAngles = _InventoryList_rotation[i];
       var ob = _ParentList.Find(x => x.gameObject.activeSelf);
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
            {
                _InventoryList[i].transform.position = _RePoint;
                _InventoryList[i].GetComponent<Inventoryitemidentity>().SetReturn(true);
            }
        }
        else
        {
            ResetHoveringitem();
            if (_itemSelecting)
            {
                _itemSelecting = false;
                for (int i = 0; i < _InventoryList.Count; i++)
                {
                    _InventoryList[i].transform.position = _RePoint;
                    _InventoryList[i].GetComponent<Inventoryitemidentity>().SetReturn(true);
                }
            }
            CloseInventory();
            _RightGrabber.ForceGrab = false;
            _RightGrabber.ForceRelease = false;
        }

    }
    private void InputKey()
    {
        if (Input.GetKeyDown(KeyCode.F) || InputBridge.Instance.BButtonDown)
        {
            if (Time.time - _LastPress > _Delay)
            {
                _LastPress = Time.time;
                Openinventory();
            }
        }
    }
    private void FixedUpdate()
    {
        InputKey();
         
           if (_NowParent != null&&_itemSelecting&&!_IsPc)
           {
            float Angle = Mathf.Atan2(InputBridge.Instance.LeftThumbstickAxis.y, InputBridge.Instance.LeftThumbstickAxis.x) * Mathf.Rad2Deg;
            float cursorangle = Angle > 0 ? (_CircleAngle / 2.0f) - Angle : (_CircleAngle / 2.0f) + Mathf.Abs(Angle);
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
          
            if (_HoveringItme != null)
                _HoveringItme.transform.position = _Hand.transform.position;
            if (InputBridge.Instance.RightGripDown || InputBridge.Instance.LeftTriggerDown)
            {
                if (_HoveringItme != null)
                {
                    _SelectItem = _InventoryList.Find(x => x.GetName == _HoveringItme.GetComponent<Inventoryitemidentity>().GetName);
                    _SelectItem.transform.position = _Hand.transform.position;
                    _SelectItem.SetReturn(false);
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
        Twinkle();
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
        if (_ParentList.Count > 0)
        {
            _Player.GetComponent<SmoothLocomotion>().enabled = false;
            _Player.GetComponent<LocomotionManager>().enabled = false;
            _Player.GetComponent<PlayerElevator>().enabled = false;
            if (_IsPc)
                InputManager.Instance.CursorActivate(true, 2);
        }
        _ItemName.text =string.Empty;
    }

    public void Adding(Inventoryitemidentity item)
    {
        if(OnAddedItem != null )
        OnAddedItem.Invoke();

        int count=0;
        if (_InventoryList.Count > 0)
        {
            var li = _InventoryList.FindAll(x => x.GetName == item.GetName);
            count = li.Count;
        }
        if ( count < item.GetMaxiumCount || _InventoryList.Count ==0)
        {
            if (item.GetNetworkOb != null)
            {
                _InventoryList.Add(item);
                _InventoryList_rotation.Add(item.transform.localEulerAngles);
                if (_NetWorkObjetManager.gameObject.activeSelf)
                {
                    item.gameObject.SetActive(false);
                    item.Addnetwork();
                }
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
    }
    public void Remove(Inventoryitemidentity item)
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
           _HoveringItme.transform.position = _RePoint;
            _HoveringItme.GetComponent<Inventoryitemidentity>().SetReturn(true);
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
                    ob.transform.position = _Hand.transform.position;
                    ob.SetReturn(false);
                    _HoveringItme = ob.gameObject;
                    _ItemName.text = parentactive.GetInventoryButtonList[i].GetName;
                    parentactive.GetInventoryButtonList[i].SetMaterial(parentactive.GetInventoryButtonList[i].GetHighlightMaterial);
                }
                else
                {
                    var ob = _InventoryList.Find(x => x.GetName == parentactive.GetInventoryButtonList[i].GetName);
                     ob.transform.position = _RePoint;
                    ob.SetReturn(true);
                    parentactive.GetInventoryButtonList[i].SetMaterial(parentactive.GetInventoryButtonList[i].GetOriginalMaterial);
                }
            }
            exit.MaterialChange(exit.GetOriginalMaterial);
        }
    }
   

    public void StartHighlight(string Itemname)
    {
        for (int I = 0; I < _ParentList.Count; I++)
        {
            var bu = _ParentList[I].GetInventoryButtonList;
            var hi = bu.Find(x => x.GetComponent<InventoryButton>().GetName == Itemname);
            if (hi != null)
            {
                var gam = hi.transform.GetChild(0);
                ObjectHighlighter.SetOutline(gam.gameObject,ObjectHighlighter.E_OUTLINE.YELLOW);
                ObjectHighlighter.SetOutline(hi.gameObject, ObjectHighlighter.E_OUTLINE.YELLOW);
                break;
            }
        }
    }
    public void StopHighlight(string Itemname)
    {
        for (int I = 0; I < _ParentList.Count; I++)
        {
            var bu = _ParentList[I].GetInventoryButtonList;
            var hi = bu.Find(x => x.GetComponent<InventoryButton>().GetName == Itemname);
            if (hi != null)
            {
                var gam = hi.transform.GetChild(0);
                ObjectHighlighter.RemoveOutline(gam.gameObject);
                ObjectHighlighter.RemoveOutline(hi.gameObject);
                break;
            }
        }
    }
    public void StartButtonHighlight()
    {
        if(_InventoryButton == null)
        _InventoryButton = GameObject.FindObjectOfType<HUDMainInformationPanel>().GetbtnToolBox;
        _Twinkle = true;
    }
    public void StopButtonHighlight()
    {
        _Twinkle = false;
        if (_InventoryButton == null)
            _InventoryButton = GameObject.FindObjectOfType<HUDMainInformationPanel>().GetbtnToolBox;
        else
            _InventoryButton.GetComponent<Image>().color = Color.white;
    }
    public void Twinkle()
    {
        if (_Twinkle)
        {
            float t = Mathf.PingPong(Time.time * _TwinkleSpeed, _TwinkleGolattime);
            _InventoryButton.GetComponent<Image>().color = Color.Lerp(_TwinkleStartColor, _TwinkleTargetColor, t);
        }
    }
}
