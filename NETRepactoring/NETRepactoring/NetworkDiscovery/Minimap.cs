using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities.UniversalDelegates;
using UnityEngine;


public class Minimap : MonoBehaviour
{
    [SerializeField] private Transform _Player;
    [SerializeField] private RectTransform _MapRect;
    [SerializeField] private RectTransform _PlayerImage;
    [SerializeField] private Collider _3DmapSize;
    [SerializeField] private Vector2 _Gap;
    [SerializeField] private RectTransform _NPCImageIconPrefab;
    [SerializeField] private RectTransform _OtherPlayerImageIconPrefab;
    [SerializeField] private RectTransform _GoalImageIconPrefab;
    [SerializeField] bool _FloorPlan =true;
    private List<RectTransform> _GoalTransformImage;
    private List<Transform> _GoalTransform;
    private List<Transform> _OtherObject;
    private List<RectTransform> _OtherObjectImage;
    private float _MinX;
    private float _MaxX;
    private float _MinY;
    private float _MaxY;
    private float _MinZ;
    private float _MaxZ;
    private List<ObjectSelectType> _ObjectSelectTypes; 
    public void SetPlayer(Transform transform) => _Player = transform;
    public Transform GetPlayer { get => _Player; }
    public void Awake()
    {
        _GoalTransformImage = new List<RectTransform>();
        _GoalTransform = new List<Transform>();
        _OtherObject = new List<Transform>();
        _OtherObjectImage = new List<RectTransform>();
    }

    private void OnEnable()
    {
        ResetList();
    }
    public void ResetList()
    {
        _ObjectSelectTypes = GameObject.FindObjectsOfType<ObjectSelectType>().ToList();
        _GoalTransform.Clear();
        _OtherObject.Clear();
        for (int i = 0; i < _ObjectSelectTypes.Count; i++)
        {
            var ob = _ObjectSelectTypes[i].transform.GetComponent<ObjectSelectType>();
            if (ob.GetObjecttype == Objecttype.GoalObject && _GoalImageIconPrefab != null)
            {
                instance_goal(ob.transform);
            }
            else if (ob.GetObjecttype == Objecttype.Npc && _NPCImageIconPrefab != null)
            {
                instance_npc(ob.transform);
            }
            else if (ob.GetObjecttype == Objecttype.OtherPlayer && _OtherPlayerImageIconPrefab != null)
            {
                instance_OtherPlayer(ob.transform);
            }
        }
    }
    public void AddList(Transform tr, Objecttype objectSelectType)
    {
        switch (objectSelectType)
        {
            case Objecttype.GoalObject:
                if (_GoalTransform.Count > 0)
                {
                    var F = _GoalTransform.Find(X => X.transform == tr);
                    if (F == null)
                    {
                        instance_goal(tr);
                    }
                }
                else
                {
                    instance_goal(tr);
                }
            break;
            case Objecttype.OtherPlayer:
                if (_OtherObject.Count > 0)
                {
                    var F = _OtherObject.Find(X => X.transform == tr);
                    if (F == null)
                    {
                        instance_OtherPlayer(tr);
                    }
                }
                else
                    instance_OtherPlayer(tr);
                break ;
            case Objecttype.Npc:
                if (_OtherObject.Count > 0)
                {
                    var F = _OtherObject.Find(X => X.transform == tr);
                    if (F == null)
                    {
                        instance_npc(tr);
                    }
                }
                else
                    instance_npc(tr);
                break;
        }
    }

    private void instance_goal(Transform tr)
    {
        _GoalTransform.Add(tr);
        var instance = GameObject.Instantiate(_GoalImageIconPrefab, _MapRect);
        instance.gameObject.SetActive(true);
        instance.anchoredPosition = ConvertWorldToScreenPoint(tr.position);
        _GoalTransformImage.Add(instance);
    }
    private void instance_OtherPlayer(Transform tr)
    {
        _OtherObject.Add(tr);
        var instance = GameObject.Instantiate(_OtherPlayerImageIconPrefab, _MapRect);
        instance.gameObject.SetActive(true);
        _OtherObjectImage.Add(instance);
    }
    private void instance_npc(Transform tr)
    {
        _OtherObject.Add(tr);
        var instance = GameObject.Instantiate(_NPCImageIconPrefab, _MapRect);
        instance.gameObject.SetActive(true);
        _OtherObjectImage.Add(instance);
    }
    public void DeleteList(Transform tr, Objecttype objectSelectType)
    {
        switch (objectSelectType)
        {
            case Objecttype.GoalObject:
                if (_GoalTransform.Count > 0)
                {
                    var F = _GoalTransform.Find(X => X.transform == tr);
                    if (F != null)
                    {
                        int index = _GoalTransform.FindIndex(X => X.transform == tr);
                        _GoalTransform.Remove(F);
                        var im =_GoalTransformImage[index];
                        _GoalTransformImage.RemoveAt(index);
                        DestroyImmediate(im.gameObject);
                    }
                }
                break;
            case Objecttype.OtherPlayer:
                if (_OtherObject.Count > 0)
                {
                    var F = _OtherObject.Find(X => X.transform == tr);
                    if (F != null)
                    {
                        int index = _OtherObject.FindIndex(X => X.transform == tr);
                        _OtherObject.Remove(F);
                        var im = _OtherObjectImage[index];
                        _OtherObjectImage.RemoveAt(index);
                        DestroyImmediate(im.gameObject);
                    }
                }
                break;
            case Objecttype.Npc:
                if (_OtherObject.Count > 0)
                {
                    var F = _OtherObject.Find(X => X.transform == tr);
                    if (F != null)
                    {
                        int index = _OtherObject.FindIndex(X => X.transform == tr);
                        _OtherObject.Remove(F);
                        var im = _OtherObjectImage[index];
                        _OtherObjectImage.RemoveAt(index);
                        DestroyImmediate(im.gameObject);
                    }
                }
                break;
        }
    }


    private void Start()
    {
        float pox = _3DmapSize.transform.position.x;
        float poy = _3DmapSize.transform.position.y;
        float poz = _3DmapSize.transform.position.z;
        _MinX = -(_3DmapSize.bounds.size.x / 2) + pox;
        _MaxX = (_3DmapSize.bounds.size.x / 2) + pox;
        _MinY = -(_3DmapSize.bounds.size.y / 2) + poy;
        _MaxY = (_3DmapSize.bounds.size.y / 2) + poy;
        _MinZ = -(_3DmapSize.bounds.size.z / 2) + poz;
        _MaxZ = (_3DmapSize.bounds.size.z / 2) + poz;
        if (_GoalTransform.Count > 0)
        {
            for (int i = 0; i < _GoalTransform.Count; i++)
                _GoalTransformImage[i].anchoredPosition = ConvertWorldToScreenPoint(_GoalTransform[i].position);
        }
    }
    private void FixedUpdate()
    {
        _PlayerImage.anchoredPosition = ConvertWorldToScreenPoint(_Player.position);
        if (_OtherObject.Count > 0)
        {
            for (int i = 0; i < _OtherObject.Count; i++)
                _OtherObjectImage[i].anchoredPosition = ConvertWorldToScreenPoint(_OtherObject[i].position);
        }    
    }
    private Vector2 ConvertWorldToScreenPoint(Vector3 worldPosition)
    {
        Vector2 worldMin = _FloorPlan ? new Vector2(_MinX, _MinZ) : new Vector2(_MinZ, _MinY);
        Vector2 worldMax = _FloorPlan ? new Vector2(_MaxX, _MaxZ) : new Vector2(_MaxZ, _MaxY);

        // 플레이어의 월드 좌표
        Vector2 playerWorldPos =_FloorPlan? new Vector2(worldPosition.x,worldPosition.z):
            new Vector2(worldPosition.z,worldPosition.y);

        // 월드 좌표를 0~1 범위로 정규화
        Vector2 normalizedPos = new Vector2(
        Mathf.InverseLerp(worldMin.x, worldMax.x, playerWorldPos.x),
        Mathf.InverseLerp(worldMin.y, worldMax.y, playerWorldPos.y)
    );
        float mapsizex = _MapRect.rect.width - Mathf.Abs(_Gap.x);
        float mapsizey = _MapRect.rect.height - Mathf.Abs(_Gap.y);
        // 3. 정규화된 좌표를 미니맵의 크기에 맞게 변환
        Vector2 localPos = new Vector2(
            (normalizedPos.x - 0.5f) * mapsizex,  // 중심을 기준으로 변환
            (normalizedPos.y - 0.5f) * mapsizey);

        return localPos;
    }
}
