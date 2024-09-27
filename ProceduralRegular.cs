using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

public class ProceduralRegular : MonoBehaviour
{
    public int segments = 32;
    public float radius = 1.0f;
    public float aangle = 45.0f;

    // 부채꼴메쉬 만들때 주의할 점은 이 오브젝트를 자식으로 둘경우  재대로된 메쉬콜라이더 안그려진다 그래서 최상위 루트에서 만든어야한다는것
    // 원인은 정확히 모르겠으나 center점을 자식으로 둘경우 센터점을 제대로 못잡는 것같다
    // 자식으로 두고 center점을 월드 포지션으로 넣을 경우 부채꼴이 그려지긴하나 0을 중심으로 약간 찌그러진 모양으로 나옴
    // 자식으로 두고 로컬포지션으로 둘때는 메쉬콜라이더가 사각형으로 만들어짐
    [Button("making")]
    void setMeshData()
    {
        Vector3 center= Vector3.zero;
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];
        vertices[0] = Vector3.zero;


        float angleStep = (aangle - 0) / segments;
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

    
}
