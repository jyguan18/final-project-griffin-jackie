using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public List<Tile> neighborsXPos;
    public List<Tile> neighborsXNeg;
    public List<Tile> neighborsZPos;
    public List<Tile> neighborsZNeg;
    public bool applyHeightAboveBase; // TODO variable name. Also maybe should be per component of the tile actually? like wall+tower piece can have just wall affected fully

    public bool addRot90;
    public bool addRot180;
    public bool addRot270;
    public string[] autoFlags; //perhaps should be a single string. rn I think will treat as "acts as any of these" rather than needing to be it matches all of them
    public string[] neighborsXPAutoFlags;
    public string[] neighborsXNAutoFlags;
    public string[] neighborsZPAutoFlags;
    public string[] neighborsZNAutoFlags;

    // public bool hasMinHeight;
    // public float minHeight;
    // public bool hasMaxHeight;
    // public float maxHeight;
    public Vector2[] allowedHeightBounds; //pairs of min and max height that can be between? doing an array so can do e.g. one that can appear bottom and top but not middle. probably not relevant for this tileset but just to keep the option open
    public float maxHeightDifference = -1f; // negative -> disable check

    public bool placeAtWaterHeight;

    [HideInInspector]
    public Quaternion rotation = Quaternion.identity;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
