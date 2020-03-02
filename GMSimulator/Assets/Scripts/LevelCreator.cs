using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCreator : MonoBehaviour
{
    LevelManager manager;
    GridBase gridBase;
    InterfaceManager ui;

    //Place obj vars
    bool hasObj;
    GameObject objToPlace;
    GameObject cloneObj;
    LevelObject objProperties;
    Vector3 mousePos;
    Vector3 worldPos;
    bool deleteObj;

    //Paint tile variables
    bool hasMaterial;
    bool paintTile;
    public Material matToPlace;
    Node previousNode;
    Material prevMaterial;
    Quaternion targetRot;
    Quaternion prevRotation;

    //Place stack objs vars
    bool placeStackObj;
    GameObject stackObjToPlace;
    GameObject stackCloneObj;
    LevelObject stackObjProperties;
    bool deleteStackObj;

    private void Start() {
        gridBase = GridBase.GetInstance();
        manager = LevelManager.GetInstance();
        ui = InterfaceManager.GetInstance();

        //PaintAll();
    }

    private void Update() {
        
        PlaceObject();
        PaintTile();
        DeleteObjs();
        PlaceStackObj();
        //CreateWall();
        DeleteStackObjs();
        //DeleteWallsActual();
        
    }

    private void UpdateMousePosition() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            mousePos = hit.point;
        }
    }

    public void PassGameObjectToPlace(string objId) {
        if (cloneObj != null) {
            Destroy(cloneObj);
        }

        CloseAll();
        hasObj = true;
        cloneObj = null;
        objToPlace = ResourceManager.GetInstance().GetObjBase(objId).objPrefab;
    }

    private void PlaceObject() {
        if (hasObj) {
            UpdateMousePosition();
            Node curNode = gridBase.NodeFromWorldPosition(mousePos);
            worldPos = curNode.vis.transform.position;

            if (cloneObj == null) {
                cloneObj = Instantiate(objToPlace, worldPos, Quaternion.identity) as GameObject;
                objProperties = cloneObj.GetComponent<LevelObject>();
            } else {
                cloneObj.transform.position = worldPos;

                if (Input.GetMouseButton(0) && !ui.mouseOverUIElement) {
                    if (curNode.placedObj != null) {
                        manager.inSceneGameObjects.Remove(curNode.placedObj.gameObject);
                        Destroy(curNode.placedObj.gameObject);
                        curNode.placedObj = null;
                    }
                    GameObject actualObjPlaced = Instantiate(objToPlace, worldPos, cloneObj.transform.rotation) as GameObject;
                    LevelObject placedObjProperties = actualObjPlaced.GetComponent<LevelObject>();

                    placedObjProperties.gridPosX = curNode.nodePosX;
                    placedObjProperties.gridPosZ = curNode.nodePosZ;
                    curNode.placedObj = placedObjProperties;
                    manager.inSceneGameObjects.Add(actualObjPlaced);
                }

                if (Input.GetMouseButtonUp(1)) {
                    objProperties.ChangeRotation();
                }
            }
        } else {
            if(cloneObj != null) {
                Destroy(cloneObj);
            }
        }
    }

    public void PassStackedObjectToPlace(string objId) {
        if (stackCloneObj != null) {
            Destroy(stackCloneObj);
        }

        CloseAll();
        placeStackObj = true;
        stackCloneObj = null;
        stackObjToPlace = ResourceManager.GetInstance().GetStackObjBase(objId).objPrefab;
    }

    private void PlaceStackObj() {
        if (placeStackObj) {
            UpdateMousePosition();
            Node curNode = gridBase.NodeFromWorldPosition(mousePos);
            worldPos = curNode.vis.transform.position;

            if (stackCloneObj == null) {
                cloneObj = Instantiate(stackObjToPlace, worldPos, Quaternion.identity) as GameObject;
                stackObjProperties = stackCloneObj.GetComponent<LevelObject>();
            } else {
                stackCloneObj.transform.position = worldPos;

                if (Input.GetMouseButton(0) && !ui.mouseOverUIElement) {
                    GameObject actualObjPlaced = Instantiate(stackObjToPlace, worldPos, stackCloneObj.transform.rotation) as GameObject;
                    LevelObject placedObjProperties = actualObjPlaced.GetComponent<LevelObject>();

                    placedObjProperties.gridPosX = curNode.nodePosX;
                    placedObjProperties.gridPosZ = curNode.nodePosZ;
                    curNode.stackedObjs.Add(placedObjProperties);
                    manager.inSceneStackObjects.Add(actualObjPlaced);
                }

                if (Input.GetMouseButtonUp(1)) {
                    stackObjProperties.ChangeRotation();
                }
            }
        } else {
            if (cloneObj != null) {
                Destroy(stackCloneObj);
            }
        }
    }

    public void PassMaterialToPaint(int matId) {
        deleteObj = false;
        placeStackObj = false;
        hasObj = false;
        matToPlace = ResourceManager.GetInstance().GetMaterial(matId);
        hasMaterial = true;
    }

    private void PaintTile() {
        if(hasMaterial) {
            UpdateMousePosition();
            Node curNode = gridBase.NodeFromWorldPosition(mousePos);

            if (previousNode == null) {
                previousNode = curNode;
                prevMaterial = previousNode.tileRenderer.material;
                prevRotation = previousNode.vis.transform.rotation;
            } else {
                if (previousNode != curNode) {
                    if (paintTile) {
                        int matId = ResourceManager.GetInstance().GetMaterialId(matToPlace);
                        curNode.vis.GetComponent<NodeObject>().textureid = matId;
                        paintTile = false;
                    } else {
                        previousNode.tileRenderer.material = prevMaterial;
                        previousNode.vis.transform.rotation = prevRotation;
                    }

                    previousNode = curNode;
                    prevMaterial = curNode.tileRenderer.material;
                    prevRotation = curNode.vis.transform.rotation;
                }
            }

            curNode.tileRenderer.material = matToPlace;
            curNode.vis.transform.localRotation = targetRot;

            if (Input.GetMouseButton(0) && !ui.mouseOverUIElement) {
                paintTile = true;
            }

            if (Input.GetMouseButtonUp(1)) {
                Vector3 eulerAngles = curNode.vis.transform.eulerAngles;
                eulerAngles += new Vector3(0, 90, 0);
                targetRot = Quaternion.Euler(eulerAngles);
            }
        }
    }

    public void PaintAll() {
        for (int x = 0; x < gridBase.sizeX; x++) {
            for (int z = 0; z < gridBase.sizeZ; z++) {
                gridBase.grid[x, z].tileRenderer.material = matToPlace;
                int matId = ResourceManager.GetInstance().GetMaterialId(matToPlace);
                gridBase.grid[x, z].vis.GetComponent<NodeObject>().textureid = matId;
            }
        }
        previousNode = null;
    }

    private void DeleteObjs() {
        if (deleteObj) {
            UpdateMousePosition();
            Node curNode = gridBase.NodeFromWorldPosition(mousePos);

            if (Input.GetMouseButton(0) && !ui.mouseOverUIElement) {
                if (curNode.placedObj != null) {
                    if (manager.inSceneGameObjects.Contains(curNode.placedObj.gameObject)) {
                        manager.inSceneGameObjects.Remove(curNode.placedObj.gameObject);
                        Destroy(curNode.placedObj.gameObject);
                    }
                    curNode.placedObj = null;
                }
            }
        }
    }

    private void DeleteStackObjs() {
        if (deleteStackObj) {
            UpdateMousePosition();
            Node curNode = gridBase.NodeFromWorldPosition(mousePos);

            if (Input.GetMouseButton(0) && !ui.mouseOverUIElement) {
                if (curNode.stackedObjs.Count > 0) {
                    for (int i = 0; i < curNode.stackedObjs.Count; i++) {
                        if (manager.inSceneStackObjects.Contains(curNode.stackedObjs[i].gameObject)) {
                            manager.inSceneStackObjects.Remove(curNode.stackedObjs[i].gameObject);
                            Destroy(curNode.stackedObjs[i].gameObject);
                        }
                    }
                    curNode.stackedObjs.Clear();
                }
            }
        }
    }

    public void DeleteObj() {
        CloseAll();
        deleteObj = true;
    }

    public void DeleteStackObj() {
        CloseAll();
        deleteStackObj = true;
    }

    private void CloseAll() {
        hasObj = false;
        deleteObj = false;
        paintTile = false;
        placeStackObj = false;
        //createWall = false;
        hasMaterial = false;
        deleteStackObj = false;
        //deleteWall = false;
    }
}
