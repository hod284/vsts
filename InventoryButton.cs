using BNG;
using TriInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour
{
    [SerializeField] private string _ObjectName; 
    [SerializeField] private iteminventory _Iteminventory;
    [SerializeField] private Material _HighlightMaterial;
    [SerializeField] private Material _OriginalMaterial;
    [SerializeField] private InputSystemUIInputModule _PCInputSystemUI;
    private float _Angle;
    private int segments = 32;
    public void SetName(string obname) => _ObjectName = obname;
    public Material GetOriginalMaterial { get => _OriginalMaterial; }
    public Material GetHighlightMaterial { get => _HighlightMaterial; }
    public string GetName { get => _ObjectName; }
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
        ItemSelect();
    }
    public void PointerExit(PointerEventData eventData) 
    {
        ItemNoSelect();
    }
    public void PointerClick(PointerEventData eventData)
    {
        ItemSelected();
    }

    // 부채꼴메쉬 만들때 주의할 점은 이 오브젝트를 자식으로 둘 경우  재대로 된 메쉬콜라이더 안 그려진다 그래서 최상위 루트에서 만든어야한다는것
    // 원인은 정확히 모르겠으나 자식으로 둘경우 센터점을 제대로 못잡는 것같다
    // 자식으로 두고 center점을 월드 포지션으로 넣을 경우 부채꼴이 그려지긴하나 0을 중심으로 약간 찌그러진 모양으로 나옴
    // 자식으로 두고 로컬포지션으로 둘때는 메쉬콜라이더가 사각형으로 만들어짐
    [Button("부채꼴 메쉬 프리펩 생성")]
    private void MakingtheMesh()
    {
        _Angle = _Iteminventory.GetCircleAngle / _Iteminventory.GetMaxiumInventorySlot;
        float radius = _Iteminventory.GetRadius;
        Vector3 center = Vector3.zero;
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];
        vertices[0] = center;


        float angleStep = (_Angle - 0) / segments;
        for (int i = 0; i <= segments; i++)
        {
            float angle = 0 + i * angleStep;
            float x = center.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = center.y + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            vertices[i + 1] = new Vector3(x, y, 0);
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        GetComponent<MeshFilter>().mesh = mesh;
        this.gameObject.AddComponent<MeshCollider>();
    }
    private void OnMouseEnter()
    {
        if (_PCInputSystemUI.enabled)
            ItemSelect();
    }
    private void OnMouseExit()
    {
        if (_PCInputSystemUI.enabled)
            ItemNoSelect();
    }
    private void OnMouseDown()
    {
        if (_PCInputSystemUI.enabled)
            ItemSelected();
    }
    public void ItemSelect()
    {
        _Iteminventory.FindItem(_ObjectName);
        _Iteminventory.SetItemName(_ObjectName);
        GetComponent<MeshRenderer>().material = _HighlightMaterial;
    }
    public void ItemSelected()
    {
        _Iteminventory.Resetitem(_ObjectName);
        var item =_Iteminventory.GetInventoryList.Find(x=>x.GetName ==_ObjectName);
        item.gameObject.SetActive(true);
        _Iteminventory.CloseInventory();
        GetComponent<MeshRenderer>().material = _OriginalMaterial;
    }
    public void ItemNoSelect()
    {
        _Iteminventory.Resetitem(_ObjectName);
        GetComponent<MeshRenderer>().material = _OriginalMaterial;
    }
    public void SetMaterial(Material MA) => GetComponent<MeshRenderer>().material = MA;
}
