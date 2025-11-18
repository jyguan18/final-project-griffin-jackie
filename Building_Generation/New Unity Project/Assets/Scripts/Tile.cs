using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Tile[] neighborsXPos;
    public Tile[] neighborsXNeg;
    public Tile[] neighborsZPos;
    public Tile[] neighborsZNeg;

    public bool applyHeightAboveBase; // TODO variable name. Also maybe should be per component of the tile actually? like wall+tower piece can have just wall affected fully

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
