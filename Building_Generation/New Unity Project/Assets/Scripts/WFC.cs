using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFC : MonoBehaviour
{
    public Vector2Int mapDimensions;
    public float cellWidth;
    public Tile[] tileTypes; // TODO what type? also better name
    
    int iteration;
    Cell[,] grid;
    bool genRunning;
    int delayCounter;
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
        delayCounter = 0;

        initVal = 16;
        Random.InitState(initVal);
        // Random.InitState(2);
        retryGen = false;
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
            Vector3 targetPos = new Vector3(targetCoords.x,0,targetCoords.y);
            spawnedTiles.Add(Instantiate(chosenTile, targetPos, Quaternion.identity));
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
                Vector3 targetPos = new Vector3(x,0,y);
                spawnedTiles.Add(Instantiate(chosenTile, targetPos, Quaternion.identity));
                
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
}
