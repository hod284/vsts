using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

public class ProceduralRegular : MonoBehaviour
{
    public int segments = 32;
    public float radius = 1.0f;
    public float aangle = 45.0f;




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
