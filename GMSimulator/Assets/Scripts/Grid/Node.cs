using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {
    public int nodePosX;
    public int nodePosZ;
    public GameObject vis;
    public MeshRenderer tileRenderer;
    public bool isWalkable;
    public LevelObject placedObj;
    public List<LevelObject> stackedObjs = new List<LevelObject>();
    //public WallObj wallObj;
}
