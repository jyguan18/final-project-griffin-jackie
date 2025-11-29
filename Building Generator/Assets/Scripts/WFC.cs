using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WFC : MonoBehaviour
{
    public Vector2Int mapDimensions;
    public float cellWidth;

    public float cellBottom = -0.5f;
    public float cellBottomEpsilon = 0.001f;
    public InfiniteTerrain terrain;
    public Tile[] tileTypes; // TODO what type? also better name
    public Tile[] edgeTileTypes;
    public Tile[] waterTileTypes;

    public float waterHeight;
    
    int iteration;
    Cell[,] grid;
    bool genRunning;
    // int delayCounter;
    int initVal;
    bool retryGen;

    List<Tile> initializedTileTypes;
    List<Tile> spawnedTiles;

    

    

    // Start is called before the first frame update
    void Start()
    {
        spawnedTiles = new List<Tile>();
        initializeTileSet();
        iteration = 0;
        resetGrid();
        genRunning = true;
        // delayCounter = 0;

        initVal = 0;
        // Random.InitState(initVal);
        // Random.InitState(2);
        retryGen = false;

        // TODO perhaps should build rotating into this so don't need to manually make tiles for the four directions? then need probably an option to mark which/if to do rotations in parameters of Tile
        // I guess would need a pre-pass to assess all the possible connections (since don't just need to update the tile being rotated but also each of the neighbor tiles' lists of neighbor tiles)
    }

    void initializeTileSet() {
        // iterate through all of the tiles in tileTypes, update connections based on autoflag members, add rotated versions based on rotation members
        // so e.g. instead of:
        //  Empty, X+, X-, Z+, Z-, X+X-, Z+Z-, X+Z+, X+Z-, X-Z+, X-Z-
        // can have:
        //  Empty
        //      thisAutoFlag "empty"
        //      NeighborsXP...ZNAutoFlags "wall"
        //  X+
        //      thisAutoFlag "wall"
        //      neighborsXPAutoFlags "wall"
        //      neighborsXN...ZNAutoFlags "empty"
        //      addRot90 true
        //      addRot180 true
        //      addRot270 true
        //  X+X-
        //      thisAutoFlag "wall"
        //      neighborsXP...XNAutoFlags "wall"
        //      neighborsZP...ZNAutoFlags "empty"
        //      addRot90 false
        //      addRot180 true
        //      addRot270 false
        //  X+Z+
        //      thisAutoFlag "wall"
        //      neighborsXPAutoFlags "wall"
        //      neighborsZPAutoFlags "wall"
        //      neighborsXN,ZNAutoFlags "empty"
        //      addRot90 true
        //      addRot180 true
        //      addRot270 true

        initializedTileTypes = new List<Tile>();

        // Note for now ONLY handling ones in tileTypes but theoretically should change to do ones that appear only in the other list too
        Dictionary<string,List<Tile>> tileFlagMap = new Dictionary<string, List<Tile>>();
        foreach (Tile tile in tileTypes) {
            initializedTileTypes.Add(tile);
            foreach (string s in tile.autoFlags) {
                if (tileFlagMap.ContainsKey(s)) {
                    tileFlagMap[s].Add(tile);
                } else {
                    tileFlagMap.Add(s, new List<Tile>{tile});
                }
            }
            
            if (tile.addRot90) {
                Tile rotTile = Instantiate(tile);
                rotTile.transform.parent = this.transform;
                rotTile.rotation = Quaternion.AngleAxis(90,Vector3.up);
                rotTile.name += " 90";

                (rotTile.neighborsXPAutoFlags, rotTile.neighborsXNAutoFlags,
                 rotTile.neighborsZPAutoFlags, rotTile.neighborsZNAutoFlags) = 
                    (rotTile.neighborsZPAutoFlags, rotTile.neighborsZNAutoFlags,
                    rotTile.neighborsXNAutoFlags, rotTile.neighborsXPAutoFlags);
                
                (rotTile.neighborsXPos, rotTile.neighborsZNeg, rotTile.neighborsXNeg, rotTile.neighborsZPos)
                  = (rotTile.neighborsZPos, rotTile.neighborsXPos, rotTile.neighborsZNeg, rotTile.neighborsXNeg);
                
                initializedTileTypes.Add(rotTile);
                foreach (string s in rotTile.autoFlags) {
                    if (tileFlagMap.ContainsKey(s)) {
                        tileFlagMap[s].Add(rotTile);
                    } else {
                        tileFlagMap.Add(s, new List<Tile>{rotTile});
                    }
                }
            }
            
            if (tile.addRot180) {
                Tile rotTile = Instantiate(tile);
                rotTile.transform.parent = this.transform;
                rotTile.rotation = Quaternion.AngleAxis(180,Vector3.up);
                rotTile.name += " 180";

                (rotTile.neighborsXPAutoFlags, rotTile.neighborsXNAutoFlags) = 
                    (rotTile.neighborsXNAutoFlags, rotTile.neighborsXPAutoFlags);
                (rotTile.neighborsZPAutoFlags, rotTile.neighborsZNAutoFlags) = 
                    (rotTile.neighborsZNAutoFlags, rotTile.neighborsZPAutoFlags);
                
                (rotTile.neighborsXPos, rotTile.neighborsZNeg, rotTile.neighborsXNeg, rotTile.neighborsZPos)
                  = (rotTile.neighborsXNeg, rotTile.neighborsZPos, rotTile.neighborsXPos, rotTile.neighborsZNeg);

                initializedTileTypes.Add(rotTile);
                foreach (string s in rotTile.autoFlags) {
                    if (tileFlagMap.ContainsKey(s)) {
                        tileFlagMap[s].Add(rotTile);
                    } else {
                        tileFlagMap.Add(s, new List<Tile>{rotTile});
                    }
                }
            }

            if (tile.addRot270) {
                Tile rotTile = Instantiate(tile); // TODO is instantiate fine for this?
                rotTile.transform.parent = this.transform;
                rotTile.rotation = Quaternion.AngleAxis(270,Vector3.up);
                rotTile.name += " 270";


                (rotTile.neighborsXPAutoFlags, rotTile.neighborsXNAutoFlags,
                 rotTile.neighborsZPAutoFlags, rotTile.neighborsZNAutoFlags) = 
                    (rotTile.neighborsZNAutoFlags, rotTile.neighborsZPAutoFlags,
                    rotTile.neighborsXPAutoFlags, rotTile.neighborsXNAutoFlags);
                
                // TODO really should just not mix these styles (or should just stop supporting old one) but will handle this way I guess
                (rotTile.neighborsXPos, rotTile.neighborsZNeg, rotTile.neighborsXNeg, rotTile.neighborsZPos)
                  = (rotTile.neighborsZNeg, rotTile.neighborsXNeg, rotTile.neighborsZPos, rotTile.neighborsXPos);
                
                
                initializedTileTypes.Add(rotTile);
                foreach (string s in rotTile.autoFlags) {
                    if (tileFlagMap.ContainsKey(s)) {
                        tileFlagMap[s].Add(rotTile);
                    } else {
                        tileFlagMap.Add(s, new List<Tile>{rotTile});
                    }
                }
            }
        }

        foreach (Tile tile in initializedTileTypes) {
            foreach (string s in tile.neighborsXPAutoFlags) {
                foreach (Tile otherTile in tileFlagMap[s]) {
                    bool validMatch = false;
                    foreach (string s2 in otherTile.neighborsXNAutoFlags) {
                        if (tile.autoFlags.Contains(s2)) {
                            validMatch = true;
                            break;
                        }
                    }
                    if (validMatch) {
                        if (!tile.neighborsXPos.Contains(otherTile)) {
                            tile.neighborsXPos.Add(otherTile);
                        }
                    }
                }
            }

            foreach (string s in tile.neighborsXNAutoFlags) {
                foreach (Tile otherTile in tileFlagMap[s]) {
                    bool validMatch = false;
                    foreach (string s2 in otherTile.neighborsXPAutoFlags) {
                        if (tile.autoFlags.Contains(s2)) {
                            validMatch = true;
                            break;
                        }
                    }
                    if (validMatch) {
                        if (!tile.neighborsXNeg.Contains(otherTile)) {
                            tile.neighborsXNeg.Add(otherTile);
                        }
                    }
                }
            }

            foreach (string s in tile.neighborsZPAutoFlags) {
                foreach (Tile otherTile in tileFlagMap[s]) {
                    bool validMatch = false;
                    foreach (string s2 in otherTile.neighborsZNAutoFlags) {
                        if (tile.autoFlags.Contains(s2)) {
                            validMatch = true;
                            break;
                        }
                    }
                    if (validMatch) {
                        if (!tile.neighborsZPos.Contains(otherTile)) {
                            tile.neighborsZPos.Add(otherTile);
                        }
                    }
                }
            }

            foreach (string s in tile.neighborsZNAutoFlags) {
                foreach (Tile otherTile in tileFlagMap[s]) {
                    bool validMatch = false;
                    foreach (string s2 in otherTile.neighborsZPAutoFlags) {
                        if (tile.autoFlags.Contains(s2)) {
                            validMatch = true;
                            break;
                        }
                    }
                    if (validMatch) {
                        if (!tile.neighborsZNeg.Contains(otherTile)) {
                            tile.neighborsZNeg.Add(otherTile);
                        }
                    }
                }
            }

        }

        // TODO should set all of the template objects non-active and the new ones active I guess
    }

    void resetGrid() {
        grid = new Cell[mapDimensions.x, mapDimensions.y];
        for (int i = spawnedTiles.Count - 1; i >= 0; --i) {
            // Tile curTile = spawnedTiles[i];
            Destroy(spawnedTiles[i].gameObject);
            spawnedTiles.RemoveAt(i);
        }
        // for (int y = 0; y < mapDimensions.y; ++y) {
        //     for (int x = 0; x < mapDimensions.x; ++x) {
                
        //         grid[x,y] = new Cell();
        //         grid[x,y].possibleTiles = new List<Tile>(tileTypes);
                
        //         // TODO: how should the height be used exactly?
        //         //  want to discretize some by just taking height at certain points I think. center probably, but center of sides maybe?
        //         //  what then is done with? I think:
        //         //   evaluate slope/gradient, possibly just looking at difference between highest and lowest side center height
        //         //   if very low slope: treat as standard cell which connects like in the flat cases, possibly embedding building model slightly in the ground
        //         //   slightly higher slope: deform model some to align sides with neighboring tiles
        //         //      are things like towers able to be deformed this way? I guess also can make tiles contain a deformable and non-deformable part (e.g. the tower and wall part in test models)
        //         //   much higher slope: only allow special tiles which programmatically set geometry? like procedurally make e.g. a stair model with right step up to match slope
        //         // TODO need to figure out how to link height-setting up with terrain (to pull them from the actual mesh values)

        //         // test placeholder for height
        //         // grid[x,y].centerHeight = 0f;
        //         // grid[x,y].centerHeight = (float)x * 0.05f;
        //         // grid[x,y].heightXP = ((float)x + 0.5f) * 0.05f;
        //         // grid[x,y].heightXN = ((float)x - 0.5f) * 0.05f;
        //         // grid[x,y].heightZP = grid[x,y].centerHeight;
        //         // grid[x,y].heightZN = grid[x,y].centerHeight;
        //         // good to note maybe: same scale in unity and maya so for these test models the height of 0.5 for the bricks on walls which is scaled down by 1/10 in the test here it ends up being that each tile is exactly 1 brick below the next
        //         //  maybe can discretize to a point like that? then easier to deal with varying the models to suit it
        //         grid[x,y].centerHeight = Mathf.Sin((float)x * Mathf.PI / 7.0f);
        //         grid[x,y].heightXP = Mathf.Sin(((float)x + 0.5f) * Mathf.PI / 7.0f);
        //         grid[x,y].heightXN = Mathf.Sin(((float)x - 0.5f) * Mathf.PI / 7.0f);
        //         grid[x,y].heightZP = grid[x,y].centerHeight;
        //         grid[x,y].heightZN = grid[x,y].centerHeight;

        //         if (terrain != null) {
        //             // TODO make connection/tile rules that care about cell height
        //             grid[x,y].centerHeight = -cellBottom + terrain.GetTerrainHeight(new Vector2(0f, 0f) * cellWidth, new Vector2(x, y) * cellWidth);
        //         }

        //     }
        // }

        // float minHeight = 10000f;
        for (int y = 0; y < mapDimensions.y; ++y) {
            for (int x = 0; x < mapDimensions.x; ++x) {
                
                grid[x,y] = new Cell();
                // grid[x,y].centerHeight = 0f;

                // if (edgeTileTypes.Length > 0 && (x == 0 || y == 0 || x >= mapDimensions.x - 1 || y >= mapDimensions.y - 1)) {
                //     // TODO update how that's handled to update list in initialize too
                //     grid[x,y].possibleTiles = new List<Tile>(edgeTileTypes);

                // } else {
                grid[x,y].possibleTiles = new List<Tile>(initializedTileTypes);

                // }

                if (terrain != null) {
                    // TODO make connection/tile rules that care about cell height
                    // TODO I think not using any of the height members of cell anymore, maybe remove
                    // grid[x,y].centerHeight = terrain.GetTerrainHeight(new Vector2(0f, 0f) * cellWidth, new Vector2(x, y) * cellWidth);

                    // TODO just sample corners? IDK
                    float minHeight = float.MaxValue;
                    float maxHeight = float.MinValue;
                    // List<float> sampledHeights = new List<float>();
                    for (float dx = -0.5f; dx <= 0.5f; dx += 0.5f) {
                        for (float dz = -0.5f; dz <= 0.5f; dz += 0.5f) {
                            float curHeight = terrain.GetTerrainHeight(new Vector2(dx, dz) * cellWidth, new Vector2(x, y) * cellWidth);
                            if (curHeight < minHeight) {
                                minHeight = curHeight;
                            }
                            if (curHeight > maxHeight) {
                                maxHeight = curHeight;
                            }
                            // sampledHeights.Add(curHeight);
                        }
                    }
                    float heightDifference = maxHeight - minHeight;
                    // bool possibilitiesUpdated = false;
                    // TODO only run propagate on parts that did update? eh might not be worth running the tracking
                    for (int i = grid[x,y].possibleTiles.Count - 1; i >= 0; --i) {
                        Tile curTile = grid[x,y].possibleTiles[i];
                        
                        if (curTile.maxHeightDifference >= 0f && heightDifference > curTile.maxHeightDifference) {
                            // possibilitiesUpdated = true;
                            grid[x,y].possibleTiles.RemoveAt(i);
                        } else if (curTile.allowedHeightBounds.Length > 0) {
                            
                            bool validHeight = false;
                            foreach (Vector2 heightPair in curTile.allowedHeightBounds) {
                                float hpMin = Mathf.Min(heightPair.x, heightPair.y);
                                float hpMax = Mathf.Max(heightPair.x, heightPair.y);
                                // probably makes more sense to be entirely within bounds
                                if (maxHeight <= hpMax && minHeight >= hpMin) {
                                    validHeight = true;
                                    break;
                                }
                                // foreach(float curHeight in sampledHeights) {
                                //     if (curHeight >= hpMin && curHeight <= hpMax) {
                                //         validHeight = true;
                                //         break;
                                //     }
                                // }
                                // if (validHeight) {
                                //     break;
                                // }
                            }
                            if (!validHeight) {
                                // possibilitiesUpdated = true;
                                grid[x,y].possibleTiles.RemoveAt(i);
                            }
                        }
                    }

                    // if (grid[x,y].centerHeight < waterHeight) {
                        // TODO update how that's handled
                        // grid[x,y].possibleTiles = new List<Tile>(waterTileTypes);
                    // }
                    // minHeight = Mathf.Min(grid[x,y].centerHeight, minHeight);
                    
                }

            }
        }
        for (int y = 0; y < mapDimensions.y; ++y) {
            for (int x = 0; x < mapDimensions.x; ++x) {
                propagate(x,y,true);
            }
        }
        // Debug.Log(minHeight);

    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)){
            iteration = 0;
            resetGrid();
            genRunning = true;
        }
        if (Input.GetKeyDown(KeyCode.C)){
            genRunning = true;
        }
        if (Input.GetKeyDown(KeyCode.G)){
            iteration = 0;
            if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) {
                ++initVal;
            }
            Debug.Log("init " + initVal);
            Random.InitState(initVal);
            resetGrid();
            genRunning = true;
        }
        if (genRunning) {
        // if (genRunning && (++delayCounter % 60 == 0)) {
            if (Input.GetKey(KeyCode.F)) {
                while (genRunning) {
                    if (!runStep()) {
                        if (retryGen) {
                            // failsafe; whether impossible configurations can appear depends on input I believe. only had failures in the case I was testing due to a mistake in neighbor list, but for other tilesets may be legitimately possible I believe
                            iteration = 0;
                            resetGrid();
                            retryGen = false;
                            break;
                            // genRunning = true;
                        } else {
                            Debug.Log("done");
                            genRunning = false;
                        }
                    }
                }
                Debug.Log(iteration);

            } else {
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
        Tile newTile = Instantiate(chosenTile, targetPos, chosenTile.rotation);
        newTile.transform.parent = this.transform;

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

                MeshCollider meshCollider = mf.gameObject.GetComponent<MeshCollider>();

                if (meshCollider == null)
                {
                   meshCollider = mf.gameObject.AddComponent<MeshCollider>();
                }

                meshCollider.sharedMesh = m;
            }
        }
        spawnedTiles.Add(newTile);
        
    }
}
