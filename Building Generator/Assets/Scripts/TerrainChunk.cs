using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainChunk : MonoBehaviour
{
    private Mesh mesh;
    private MeshCollider meshCollider;

    void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
    }

    public void GenerateTerrain(int size, int xOffset, int zOffset, float noiseScale, float heightMultiplier, Material mat)
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = mat;

        Vector3[] vertices = new Vector3[(size + 1) * (size + 1)];
        int[] triangles = new int[size * size * 6];

        int i = 0;
        for (int z = 0; z <= size; z++)
        {
            for (int x = 0; x <= size; x++)
            {
                float noiseX = (x + xOffset) * noiseScale;
                float noiseZ = (z + zOffset) * noiseScale;
                float yPos = Mathf.PerlinNoise(noiseX, noiseZ) * heightMultiplier;

                vertices[i] = new Vector3(x, yPos, z);
                i++;
            }
        }

        int vertex = 0;
        int triangleIndex = 0;
        for (int z = 0; z < size; z++)
        {
            for (int x = 0; x < size; x++)
            {
                triangles[triangleIndex + 0] = vertex + 0;
                triangles[triangleIndex + 1] = vertex + size + 1;
                triangles[triangleIndex + 2] = vertex + 1;
                triangles[triangleIndex + 3] = vertex + 1;
                triangles[triangleIndex + 4] = vertex + size + 1;
                triangles[triangleIndex + 5] = vertex + size + 2;
                vertex++;
                triangleIndex += 6;
            }
            vertex++;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateTangents();
        // ASIDE: probably should make shader account for shadows, looks a bit odd rn having shadows on buildings but not landscape
        mesh.RecalculateNormals();


        if (meshCollider != null)
        {
            meshCollider.sharedMesh = mesh;
        }
    }
}