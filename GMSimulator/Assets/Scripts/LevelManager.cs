using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    GridBase gridBase;

    public List<GameObject> inSceneGameObjects = new List<GameObject>();
    public List<GameObject> inSceneWalls = new List<GameObject>();
    public List<GameObject> inSceneStackObjects = new List<GameObject>();

    private static LevelManager instance = null;

    public static LevelManager GetInstance() {
        return instance;
    }

    private void Awake() {
        instance = this;
    }

    private void Start() {
        gridBase = GridBase.GetInstance();
    }

    void InitLevelObjects() {
        if(inSceneGameObjects.Count > 0) {
            for (int i = 0; i < inSceneGameObjects.Count; i++) {
                LevelObject obj = inSceneGameObjects[i].GetComponent<LevelObject>();
                obj.UpdateNode(gridBase.grid);
            }
        }
    }
}
