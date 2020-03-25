using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObject : MonoBehaviour
{
    public string obj_Id;
    public int gridPosX;
    public int gridPosZ;
    public GameObject modelVisualization;
    public Vector3 worldPositionOffset;
    public Vector3 worldRotation;
    public bool isStackableObj = false;
    public bool isWallObject = false;

    public float rotateDegrees = 45;

    public void UpdateNode(Node[,] grid) {
        Node node = grid[gridPosX, gridPosZ];
        Vector3 worldPosition = node.vis.transform.position;
        transform.rotation = Quaternion.Euler(worldRotation);
        transform.position = worldPosition;
    }

    public void ChangeRotation() {
        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles += new Vector3(0, rotateDegrees, 0);
        transform.localRotation = Quaternion.Euler(eulerAngles);
    }

    public SaveableLevelObject GetSaveableObject() {
        SaveableLevelObject savedObj = new SaveableLevelObject();
        savedObj.objId = obj_Id;
        savedObj.posX = gridPosX;
        savedObj.posZ = gridPosZ;

        worldRotation = transform.localEulerAngles;
        savedObj.rotX = worldRotation.x;
        savedObj.rotY = worldRotation.y;
        savedObj.rotZ = worldRotation.z;

        savedObj.isWallObject = isWallObject;
        savedObj.isStackable = isStackableObj;

        return savedObj;
    }
}

[System.Serializable]
public class SaveableLevelObject {
    public string objId;
    public int posX;
    public int posZ;
    public float rotX;
    public float rotY;
    public float rotZ;
    public bool isWallObject = false;
    public bool isStackable = false;
}
