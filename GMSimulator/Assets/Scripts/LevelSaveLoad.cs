using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static LevelWallObj;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public class LevelSaveLoad : MonoBehaviour {
    private List<SaveableLevelObject> saveLevelObjectsList = new List<SaveableLevelObject>();
    private List<SaveableLevelObject> saveStackableLevelObjectsList = new List<SaveableLevelObject>();
    private List<NodeObjectSaveable> saveNodeObjectsList = new List<NodeObjectSaveable>();
    private List<WallObjectSaveable> saveWallsList = new List<WallObjectSaveable>();

    public static string saveFolderName = "LevelObjects";
    
    public void SaveLevelButton() {
        SaveLevel("testLevel");
    }

    public void LoadLevelButton() {
        LoadLevel("testLevel");
    }

    static string SaveLocation(string LevelName) {
        string saveLocation = Application.persistentDataPath + "/" + saveFolderName + "/";
        
        if(!Directory.Exists(saveLocation)) {
            Directory.CreateDirectory(saveLocation);
        }
        return saveLocation + LevelName;
    }

    private void SaveLevel(string saveName) {
        LevelObject[] levelObjects = FindObjectsOfType<LevelObject>();

        saveLevelObjectsList.Clear();
        saveWallsList.Clear();
        saveStackableLevelObjectsList.Clear();

        foreach(LevelObject lvlObj in levelObjects) {
            if (!lvlObj.isWallObject) {
                if (!lvlObj.isStackableObj) {
                    saveLevelObjectsList.Add(lvlObj.GetSaveableObject());
                } else {
                    saveStackableLevelObjectsList.Add(lvlObj.GetSaveableObject());
                }
            } else {
                WallObjectSaveable w = new WallObjectSaveable();
                w.levelObject = lvlObj.GetSaveableObject();
                w.wallObject = lvlObj.GetComponent<LevelWallObj>().GetSaveable();
                saveWallsList.Add(w);
            }
        }

        NodeObject[] nodeObjects = FindObjectsOfType<NodeObject>();
        saveNodeObjectsList.Clear();

        foreach (NodeObject nodeObject in nodeObjects) {
            saveNodeObjectsList.Add(nodeObject.GetSaveable());
        }

        LevelSaveable levelSave = new LevelSaveable();
        levelSave.saveLevelObjectsList = saveLevelObjectsList;
        levelSave.saveStackableLevelObjectsList = saveStackableLevelObjectsList;
        levelSave.saveNodeObjectsList = saveNodeObjectsList;
        levelSave.saveWallsList = saveWallsList;

        string saveLocation = SaveLocation(saveName);

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(saveLocation, FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, levelSave);
        stream.Close();
        Debug.Log(saveLocation);
    }

    private bool LoadLevel(string saveName) {
        bool retVal = true;
        string saveFile = SaveLocation(saveName);

        if(!File.Exists(saveFile)) {
            retVal = false;
        } else {
            IFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(saveFile, FileMode.Open);
            LevelSaveable save = (LevelSaveable)formatter.Deserialize(stream);
            stream.Close();
            LoadLevelActual(save);
        }
        return retVal;
    }

    void LoadLevelActual(LevelSaveable levelSaveable) {
        //Create Level Objects
        for (int i = 0; i < levelSaveable.saveLevelObjectsList.Count; i++) {
            SaveableLevelObject sObj = levelSaveable.saveLevelObjectsList[i];
            Node nodeToPlace = GridBase.GetInstance().grid[sObj.posX, sObj.posZ];

            GameObject go = Instantiate(
                ResourceManager.GetInstance().GetObjBase(sObj.objId).objPrefab,
                nodeToPlace.vis.transform.position,
                Quaternion.Euler(
                    sObj.rotX,
                    sObj.rotY,
                    sObj.rotZ))
                as GameObject;

            nodeToPlace.placedObj = go.GetComponent<LevelObject>();
            nodeToPlace.placedObj.gridPosX = nodeToPlace.nodePosX;
            nodeToPlace.placedObj.gridPosZ = nodeToPlace.nodePosZ;
            nodeToPlace.placedObj.worldRotation = nodeToPlace.placedObj.transform.localEulerAngles;
        }

        //Create Stackable Level Objects
       for (int i = 0; i < levelSaveable.saveStackableLevelObjectsList.Count; i++) {
            SaveableLevelObject sObj = levelSaveable.saveStackableLevelObjectsList[i];
            Node nodeToPlace = GridBase.GetInstance().grid[sObj.posX, sObj.posZ];

            GameObject go = Instantiate(
                ResourceManager.GetInstance().GetStackObjBase(sObj.objId).objPrefab,
                nodeToPlace.vis.transform.position,
                Quaternion.Euler(
                    sObj.rotX,
                    sObj.rotY,
                    sObj.rotZ))
                as GameObject;

            LevelObject stackObj = go.GetComponent<LevelObject>();
            stackObj.gridPosX = nodeToPlace.nodePosX;
            stackObj.gridPosZ = nodeToPlace.nodePosZ;
            nodeToPlace.stackedObjs.Add(stackObj);
        }

        //Paint Tiles
        for (int i = 0; i < levelSaveable.saveNodeObjectsList.Count; i++) {
            Node node =
                GridBase.GetInstance().grid[levelSaveable.saveNodeObjectsList[i].posX,
                levelSaveable.saveNodeObjectsList[i].posZ];
            node.vis.GetComponent<NodeObject>().UpdateNodeObject(node, levelSaveable.saveNodeObjectsList[i]);
        }

        //Create Walls
        for (int i = 0; i < levelSaveable.saveWallsList.Count; i++) {
            WallObjectSaveable sWall = levelSaveable.saveWallsList[i];
            Node nodeToPlace = GridBase.GetInstance().grid[sWall.levelObject.posX, sWall.levelObject.posZ];

            GameObject go = Instantiate(ResourceManager.GetInstance().wallPrefab,
                nodeToPlace.vis.transform.position,
                Quaternion.identity
                ) as GameObject;

            LevelObject lvlObj = go.GetComponent<LevelObject>();
            lvlObj.gridPosX = nodeToPlace.nodePosX;
            lvlObj.gridPosZ = nodeToPlace.nodePosZ;

            LevelWallObj wallObj = go.GetComponent<LevelWallObj>();
            wallObj.UpdateWall(sWall.wallObject.direction);
            wallObj.UpdateCorners(
                sWall.wallObject.cornerA,
                sWall.wallObject.cornerB,
                sWall.wallObject.cornerC);
        }
    }


    [Serializable]
    public class LevelSaveable {
        public List<SaveableLevelObject> saveLevelObjectsList;
        public List<SaveableLevelObject> saveStackableLevelObjectsList;
        public List<NodeObjectSaveable> saveNodeObjectsList;
        public List<WallObjectSaveable> saveWallsList;
    }

    [Serializable]
    public class WallObjectSaveable {
        public SaveableLevelObject levelObject;
        public WallObjectSaveableProperties wallObject;
    }
}
