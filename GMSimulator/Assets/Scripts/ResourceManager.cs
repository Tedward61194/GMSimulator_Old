using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public List<LevelGameObjectBase> LevelGameObjects = new List<LevelGameObjectBase>();
    public List<LevelStackedObjBase> LevelGameObjectsStacked = new List<LevelStackedObjBase>();
    public List<Material> LevelMaterials = new List<Material>();
    public GameObject wallPrefab;

    private static ResourceManager instance = null;

    private void Awake() {
        instance = this;
    }

    public LevelGameObjectBase GetObjBase(string objId) {
        LevelGameObjectBase retVal = null;

        for (int i = 0; i < LevelGameObjects.Count; i++) {
            if(objId.Equals(LevelGameObjects[i].objId)) {
                retVal = LevelGameObjects[i];
                break;
            }
        }
        return retVal;
    }

    public LevelStackedObjBase GetStackObjBase(string stackId) {
        LevelStackedObjBase retVal = null;

        for (int i = 0; i < LevelGameObjects.Count; i++) {
            if(stackId.Equals(LevelGameObjectsStacked[i].stackId)) {
                retVal = LevelGameObjectsStacked[i];
                break;
            }
        }
        return retVal;
    }

    public Material GetMaterial(int matId) {
        Material retVal = null;
        for (int i = 0; i < LevelMaterials.Count; i++) {
            if (matId == i) {
                retVal = LevelMaterials[i];
                break;
            }
        }
        return retVal;
    }

    public int GetMaterialId(Material mat) {
        int id = -1;
        for(int i = 0; i < LevelMaterials.Count; i++) {
            if(mat.Equals(LevelMaterials[i])) {
                id = i;
                break;
            }
        }
        return id;
    }
    
    public static ResourceManager GetInstance() {
        return instance;
    }

    [System.Serializable]
    public class LevelGameObjectBase {
        public string objId;
        public GameObject objPrefab;
    }

    [System.Serializable]
    public class LevelStackedObjBase {
        public string stackId;
        public GameObject objPrefab;
    }
}
