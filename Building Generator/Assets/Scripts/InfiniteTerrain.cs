using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Terrain Settings")]
    public GameObject terrainChunkPrefab;
    public Material terrainMaterial;
    public Gradient terrainGradient;

    [Header("Water Settings")]
    public Transform waterPlane;

    [Header("Generation Stats")]
    [SerializeField] float globalMinHeight = 0f;
    [SerializeField] float globalMaxHeight = 20f;
    [SerializeField] int chunkSize = 10;
    [SerializeField] int viewDistance = 5;
    [SerializeField] float noiseScale = 0.03f;
    [SerializeField] float heightMultiplier = 7;

    private Dictionary<Vector2, TerrainChunk> activeChunks = new Dictionary<Vector2, TerrainChunk>();


    private Texture2D gradientTexture;

    private Vector2 playerChunkCoord;

    void Start()
    {
        GradientToTexture();

        terrainMaterial.SetTexture("_TerrainGradient", gradientTexture);
        terrainMaterial.SetFloat("_MinTerrainHeight", globalMinHeight);
        terrainMaterial.SetFloat("_MaxTerrainHeight", globalMaxHeight);

        UpdateTerrain();
    }

    void Update()
    {
        Vector2 currentChunkCoord = new Vector2(
            Mathf.FloorToInt(player.position.x / chunkSize),
            Mathf.FloorToInt(player.position.z / chunkSize)
        );

        if (currentChunkCoord != playerChunkCoord)
        {
            playerChunkCoord = currentChunkCoord;
            UpdateTerrain();
        }
    }

    void UpdateTerrain()
    {
        List<Vector2> chunksToDestroy = new List<Vector2>(activeChunks.Keys);

        for (int z = -viewDistance; z <= viewDistance; z++)
        {
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                Vector2 chunkCoord = new Vector2(playerChunkCoord.x + x, playerChunkCoord.y + z);

                if (activeChunks.ContainsKey(chunkCoord))
                {
                    chunksToDestroy.Remove(chunkCoord);
                }
                else
                {
                    SpawnChunk(chunkCoord);
                }
            }
        }

        foreach (Vector2 coord in chunksToDestroy)
        {
            Destroy(activeChunks[coord].gameObject);
            activeChunks.Remove(coord);
        }

        if (waterPlane != null)
        {
            float targetX = playerChunkCoord.x * chunkSize;
            float targetZ = playerChunkCoord.y * chunkSize;

            waterPlane.position = new Vector3(targetX, waterPlane.position.y, targetZ);
        }
    }

    void SpawnChunk(Vector2 coord)
    {
        Vector3 worldPosition = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);
        int xOffset = (int)coord.x * chunkSize;
        int zOffset = (int)coord.y * chunkSize;

        GameObject chunkGO = Instantiate(terrainChunkPrefab, worldPosition, Quaternion.identity);
        chunkGO.transform.parent = this.transform;
        TerrainChunk newChunk = chunkGO.GetComponent<TerrainChunk>();

        newChunk.GenerateTerrain(chunkSize, xOffset, zOffset, noiseScale, heightMultiplier, terrainMaterial);

        activeChunks.Add(coord, newChunk);
    }

    public float GetTerrainHeight(Vector2 coord, Vector2 cellCoord) {
        // Vector3 worldPosition = new Vector3(cellCoord.x, 0, cellCoord.y);
        // int xOffset = (int)cellCoord.x;
        // int zOffset = (int)cellCoord.y;

        float noiseX = (coord.x + cellCoord.x) * noiseScale;
        float noiseZ = (coord.y + cellCoord.y) * noiseScale;
        float yPos = Mathf.PerlinNoise(noiseX, noiseZ) * heightMultiplier;
        return yPos;

    }

    private void GradientToTexture()
    {
        gradientTexture = new Texture2D(1, 100);
        Color[] pixelColors = new Color[100];
        for (int i = 0; i < 100; i++)
        {
            pixelColors[i] = terrainGradient.Evaluate((float)i / 99.0f);
        }
        gradientTexture.SetPixels(pixelColors);
        gradientTexture.Apply();
    }
}