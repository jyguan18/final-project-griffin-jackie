using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFC : MonoBehaviour
{
    public Vector2Int mapDimensions;
    public float cellWidth;

    public float cellBottom = -0.5f;
    public float cellBottomEpsilon = 0.001f;
    public InfiniteTerrain terrain;
    public Tile[] tileTypes; // TODO what type? also better name
    
    int iteration;
    Cell[,] grid;
    bool genRunning;
    // int delayCounter;
    int initVal;
    bool retryGen;
    List<Tile> spawnedTiles;

    

    

    // Start is called before the first frame update
    void Start()
    {
        spawnedTiles = new List<Tile>();
        iteration = 0;
        resetGrid();
        genRunning = true;
        // delayCounter = 0;

        initVal = 16;
        Random.InitState(initVal);
        // Random.InitState(2);
        retryGen = false;

        // TODO perhaps should build rotating into this so don't need to manually make tiles for the four directions? then need probably an option to mark which/if to do rotations in parameters of Tile
        // I guess would need a pre-pass to assess all the possible connections (since don't just need to update the tile being rotated but also each of the neighbor tiles' lists of neighbor tiles)
    }

    void resetGrid() {
        grid = new Cell[mapDimensions.x, mapDimensions.y];
        for (int i = spawnedTiles.Count - 1; i >= 0; --i) {
            // Tile curTile = spawnedTiles[i];
            Destroy(spawnedTiles[i].gameObject);
            spawnedTiles.RemoveAt(i);
        }
        for (int y = 0; y < mapDimensions.y; ++y) {
            for (int x = 0; x < mapDimensions.x; ++x) {
                
                grid[x,y] = new Cell();
                grid[x,y].possibleTiles = new List<Tile>(tileTypes);
                
                // TODO: how should the height be used exactly?
                //  want to discretize some by just taking height at certain points I think. center probably, but center of sides maybe?
                //  what then is done with? I think:
                //   evaluate slope/gradient, possibly just looking at difference between highest and lowest side center height
                //   if very low slope: treat as standard cell which connects like in the flat cases, possibly embedding building model slightly in the ground
                //   slightly higher slope: deform model some to align sides with neighboring tiles
                //      are things like towers able to be deformed this way? I guess also can make tiles contain a deformable and non-deformable part (e.g. the tower and wall part in test models)
                //   much higher slope: only allow special tiles which programmatically set geometry? like procedurally make e.g. a stair model with right step up to match slope
                // TODO need to figure out how to link height-setting up with terrain (to pull them from the actual mesh values)

                // test placeholder for height
                // grid[x,y].centerHeight = 0f;
                // grid[x,y].centerHeight = (float)x * 0.05f;
                // grid[x,y].heightXP = ((float)x + 0.5f) * 0.05f;
                // grid[x,y].heightXN = ((float)x - 0.5f) * 0.05f;
                // grid[x,y].heightZP = grid[x,y].centerHeight;
                // grid[x,y].heightZN = grid[x,y].centerHeight;
                // good to note maybe: same scale in unity and maya so for these test models the height of 0.5 for the bricks on walls which is scaled down by 1/10 in the test here it ends up being that each tile is exactly 1 brick below the next
                //  maybe can discretize to a point like that? then easier to deal with varying the models to suit it
                grid[x,y].centerHeight = Mathf.Sin((float)x * Mathf.PI / 7.0f);
                grid[x,y].heightXP = Mathf.Sin(((float)x + 0.5f) * Mathf.PI / 7.0f);
                grid[x,y].heightXN = Mathf.Sin(((float)x - 0.5f) * Mathf.PI / 7.0f);
                grid[x,y].heightZP = grid[x,y].centerHeight;
                grid[x,y].heightZN = grid[x,y].centerHeight;

                if (terrain != null) {
                    // TODO make connection/tile rules that care about cell height
                    grid[x,y].centerHeight = -cellBottom + terrain.GetTerrainHeight(new Vector2(0f, 0f) * cellWidth, new Vector2(x, y) * cellWidth);
                }

            }
        }

    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)){
            iteration = 0;
            resetGrid();
            genRunning = true;
        }
        if (Input.GetKeyDown(KeyCode.G)){
            iteration = 0;
            ++initVal;
            Debug.Log("init " + initVal);
            Random.InitState(initVal);
            resetGrid();
            genRunning = true;
        }
        if (genRunning) {
        // if (genRunning && (++delayCounter % 60 == 0)) {
            Debug.Log(iteration);
            if (!runStep()) {
                if (retryGen) {
                    // failsafe; whether impossible configurations can appear depends on input I believe. only had failures in the case I was testing due to a mistake in neighbor list, but for other tilesets may be legitimately possible I believe
                    iteration = 0;
                    resetGrid();
                    retryGen = false;
                    // genRunning = true;
                } else {
                    Debug.Log("done");
                    genRunning = false;
                }
            }
        }
    }

    bool runStep() {
        // TODO probably better way to pick order
        Cell targetCell = new Cell();
        int leastCount = int.MaxValue;
        ++iteration;
        Vector2Int targetCoords = new Vector2Int(0,0);
        // TODO this is the super-naive version of entropy check; can improve
        // also might eventually need to add e.g. backtracking
        for (int y = 0; y < mapDimensions.y; ++y) {
            for (int x = 0; x < mapDimensions.x; ++x) {
                int len = grid[x,y].possibleTiles.Count;
                if (leastCount > len && len > 1) {
                    leastCount = len;
                    targetCell = grid[x,y];
                    targetCoords = new Vector2Int(x,y);
                }
            }
        }
        if (leastCount != int.MaxValue) {
            // found a target cell
            
            int targetTileIdx = Random.Range(0,targetCell.possibleTiles.Count);
            Tile chosenTile = targetCell.possibleTiles[targetTileIdx];
            targetCell.possibleTiles = new List<Tile>{chosenTile};
            // Vector3 targetPos = new Vector3(targetCoords.x,targetCell.centerHeight,targetCoords.y);
            // spawnedTiles.Add(Instantiate(chosenTile, targetPos, Quaternion.identity));
            makeTile(chosenTile, targetCoords);
            // TODO propagate; probably helper function in this class is best way?
            propagate(targetCoords.x, targetCoords.y, true);
            if (retryGen) {
                return false;
            }
            return true;
        }
        
        return false;
    }

    void propagate(int x, int y, bool start) {
        // check neighbors to update this cell's possibilities
        // if updated or if start (since start we already changed), repeat on cell's neigbhors
        // is that it?

        // TODO I don't think is correct yet but need to experiment
        
        bool possibilitiesUpdated = false;
        if (start) {
            possibilitiesUpdated = true;
        } else {
            if (grid[x,y].possibleTiles.Count <= 1) {
                // TODO might need to make sure never get 0 case
                return;
            // } else if (grid[x,y].possibleTiles.Count <= 0) {
            //     Debug.Log("Zero case " + x + " " + y);
            //     return;
            }
            // foreach(Tile t in grid[x,y].possibleTiles) {
            // if (x == 3 && y == 0) {
            //     Debug.Log("3,0: " + grid[x,y].possibleTiles.Count);
            //     foreach (Tile t in grid[x,y].possibleTiles) {
            //         Debug.Log(t.ToString());
            //     }
            // }
            for (int i = grid[x,y].possibleTiles.Count - 1; i >= 0; --i) {
                Tile t = grid[x,y].possibleTiles[i];
                if (x < mapDimensions.x - 1) {
                    bool validNeighbor = false;
                    // if the x+1,y cell is able to contain a tile that is a potential neighbor to this tile, it's fine
                    foreach(Tile n in t.neighborsXPos) {
                        // TODO I don't recall if contains works the way I want for how objects work in Unity/C#, will need to test
                        if (grid[x+1,y].possibleTiles.Contains(n)) {
                            validNeighbor = true;
                            break;
                        }
                    }
                    if (!validNeighbor) {
                        grid[x,y].possibleTiles.RemoveAt(i);
                        possibilitiesUpdated = true;
                        continue;
                    }
                }

                if (x > 0) {
                    bool validNeighbor = false;
                    foreach(Tile n in t.neighborsXNeg) {
                        if (grid[x-1,y].possibleTiles.Contains(n)) {
                            validNeighbor = true;
                            break;
                        }
                    }
                    if (!validNeighbor) {
                        grid[x,y].possibleTiles.RemoveAt(i);
                        possibilitiesUpdated = true;
                        continue;
                    }
                }

                if (y < mapDimensions.y - 1) {
                    bool validNeighbor = false;
                    foreach(Tile n in t.neighborsZPos) {
                        if (grid[x,y+1].possibleTiles.Contains(n)) {
                            validNeighbor = true;
                            break;
                        }
                    }
                    if (!validNeighbor) {
                        grid[x,y].possibleTiles.RemoveAt(i);
                        possibilitiesUpdated = true;
                        continue;
                    }
                }

                if (y > 0) {
                    bool validNeighbor = false;
                    foreach(Tile n in t.neighborsZNeg) {
                        if (grid[x,y-1].possibleTiles.Contains(n)) {
                            validNeighbor = true;
                            break;
                        }
                    }
                    if (!validNeighbor) {
                        grid[x,y].possibleTiles.RemoveAt(i);
                        possibilitiesUpdated = true;
                        continue;
                    }
                }
            }

            if (grid[x,y].possibleTiles.Count < 1) {
                Debug.Log("Failed, no possible tiles at " + x + ", " + y);
                retryGen = true;
                return;
            }
            if (possibilitiesUpdated && grid[x,y].possibleTiles.Count == 1) {
                Tile chosenTile = grid[x,y].possibleTiles[0];
                // Vector3 targetPos = new Vector3(x,grid[x,y].centerHeight,y);
                // Vector3 targetPos = new Vector3(x,0,y);
                // spawnedTiles.Add(Instantiate(chosenTile, targetPos, Quaternion.identity));
                // makeTile(chosenTile, targetPos);
                makeTile(chosenTile, new Vector2Int(x,y));
                
            }
        }
        
        if (possibilitiesUpdated) {
            if (x < mapDimensions.x - 1)
                propagate(x+1,y,false);
            if (x > 0)
                propagate(x-1,y,false);
            if (y < mapDimensions.y - 1)
                propagate(x,y+1,false);
            if (y > 0)
                propagate(x,y-1,false);
        }
    }

    void makeTile(Tile chosenTile, Vector2Int targetCoords) {
        
        Vector3 targetPos = new Vector3(targetCoords.x * cellWidth,0,targetCoords.y * cellWidth);
        Tile newTile = Instantiate(chosenTile, targetPos, Quaternion.identity);
        // List<Mesh> meshes;
        // foreach (Mesh m in meshes) {
        // TODO deal with transforms
        Cell chosenCell = grid[targetCoords.x, targetCoords.y];
        float centerHeight = 0f;
        if (terrain != null) {
            centerHeight = -cellBottom + terrain.GetTerrainHeight(new Vector2(0f, 0f) * cellWidth, new Vector2(targetCoords.x, targetCoords.y) * cellWidth);
        }
        foreach (Transform childTransform in newTile.transform) {
            MeshFilter[] meshFilters = childTransform.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter mf in meshFilters) {
                Mesh m = mf.mesh;
                Vector3[] vertices = m.vertices;
                // I think x goes -0.5 *cellWidth to 0.5 * cellWidth in this example
                // want to map -0.5 * cellWidth -> heightXN
                // 0.5 * cellWidth -> height XP;

                // TODO actually I think a better way to do this once I have this set up to read from the actual terrain
                //  For each vertex just pull the height of the terrain directly under it and offset its y by that
                // again I think also still I want to handle steeper slopes in some other case but for shallow slopes I think shouldn't be too bad
                for (int i = 0; i < vertices.Length; ++i) {
                    vertices[i] = childTransform.TransformVector(vertices[i]);
                    // TODO setting for where base of model is?
                    if (terrain != null) {
                        if (mf.tag == "Tile Apply Height Above Base" || chosenTile.applyHeightAboveBase || vertices[i].y <= cellBottom + cellBottomEpsilon) {
                            vertices[i].y += -cellBottom + terrain.GetTerrainHeight((new Vector2(vertices[i].x, vertices[i].z)), new Vector2(targetCoords.x, targetCoords.y) * cellWidth);
                        } else {
                            vertices[i].y += centerHeight;
                        }
                    }
                    // if (chosenTile.applyHeightAboveBase || vertices[i].y <= -0.5f + 0.001f) {
                    //     vertices[i].y += Mathf.Lerp(chosenCell.heightXN, chosenCell.heightXP, vertices[i].x / cellWidth + 0.5f);
                    // } else {
                    //     vertices[i].y += chosenCell.centerHeight;
                    // }
                    vertices[i] = childTransform.InverseTransformVector(vertices[i]);
                }
                m.vertices = vertices;
                m.RecalculateNormals();
                m.RecalculateBounds();
            }
        }
        spawnedTiles.Add(newTile);
        
    }
}
