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

    //Create Wall vars
    bool createWall;
    public GameObject wallPrefab;
    Node startNodeWall;
    Node endNodeWall;
    //public Material[] wallPlacementMat;
    bool deleteWall;

    private void Start() {
        gridBase = GridBase.GetInstance();
        manager = LevelManager.GetInstance();
        ui = InterfaceManager.GetInstance();

        PaintAll();
    }

    private void Update() {
        
        PlaceObject();
        PaintTile();
        DeleteObjs();
        PlaceStackObj();
        CreateWall();
        DeleteStackObjs();
        DeleteWallsActual();
        
    }

    private void UpdateMousePosition() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            mousePos = hit.point;
        }
    }

    #region Objects
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
    #endregion

    #region Materials
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
    #endregion

    #region Walls
    public void OpenWallCreation() {
        CloseAll();
        createWall = true;
    }

    public void CreateWall() {
        if (createWall) {
            UpdateMousePosition();
            Node curNode = gridBase.NodeFromWorldPosition(mousePos);
            worldPos = curNode.vis.transform.position;

            if(startNodeWall == null) {
                if (Input.GetMouseButtonUp(0) && ! ui.mouseOverUIElement) {
                    startNodeWall = curNode;
                    Debug.Log("StartNode: " + curNode.nodePosX + "," + curNode.nodePosZ);
                }
            } else {
                if (Input.GetMouseButtonUp(0) && !ui.mouseOverUIElement) {
                    endNodeWall = curNode;
                    Debug.Log("EndNode: " + curNode.nodePosX + "," + curNode.nodePosZ);
                }
            }

            if (startNodeWall != null && endNodeWall != null) {
                int difX = endNodeWall.nodePosX - startNodeWall.nodePosX;
                int difZ = endNodeWall.nodePosZ - startNodeWall.nodePosZ;

                CreateWallInNode(startNodeWall.nodePosX, startNodeWall.nodePosZ, difX != 0 ? LevelWallObj.WallDirection.bc : LevelWallObj.WallDirection.ab);

                Node finalXNode = null;
                Node finalZNode = null;

                if (difX != 0) {
                    bool xHigher = (difX > 0);

                    for (int i = 1; i < Mathf.Abs(difX) + 1; i++) {
                        int offset = xHigher ? i : -i;
                        int posX = startNodeWall.nodePosX + offset;
                        int posZ = startNodeWall.nodePosZ;

                        if (posX < 0)
                            posX = 0;
                        if (posX > gridBase.sizeX)
                            posX = gridBase.sizeX;
                        if (posZ < 0)
                            posZ = 0;
                        if (posZ > gridBase.sizeZ)
                            posZ = gridBase.sizeZ;

                        finalXNode = gridBase.grid[posX, posZ];

                        LevelWallObj.WallDirection targetDir = LevelWallObj.WallDirection.bc;
                        CreateWallInNode(posX, posZ, targetDir);
                    }

                    UpdateWallCorners(xHigher ? startNodeWall : endNodeWall, false, true, false);
                    UpdateWallCorners(xHigher ? endNodeWall : startNodeWall, false, false, true);
                }

                if (difZ != 0) {
                    bool zHigher = (difZ > 0);

                    for (int i = 1; i < Mathf.Abs(difZ) + 1; i++) {
                        int offset = zHigher ? i : -i;
                        int posX = startNodeWall.nodePosX;
                        int posZ = startNodeWall.nodePosZ + offset;

                        if (posX < 0)
                            posX = 0;
                        if (posX > gridBase.sizeX)
                            posX = gridBase.sizeX;
                        if (posZ < 0)
                            posZ = 0;
                        if (posZ > gridBase.sizeZ)
                            posZ = gridBase.sizeZ;

                        finalZNode = gridBase.grid[posX, posZ];

                        LevelWallObj.WallDirection targetDir = LevelWallObj.WallDirection.ab;
                        CreateWallInNode(posX, posZ, targetDir);
                    }

                    UpdateWallCorners(zHigher ? startNodeWall : finalZNode, false, true, false);
                    UpdateWallCorners(zHigher ? finalZNode : startNodeWall, true, false, false);
                }

                if (difX != 0 && difZ != 0) {
                    bool xHigher = (difX > 0);
                    bool zHigher = (difZ > 0);

                    for (int i = 1; i < Mathf.Abs(difX) + 1; i++) {
                        int offset = xHigher ? i : -i;
                        int posX = startNodeWall.nodePosX + offset;
                        int posZ = endNodeWall.nodePosZ; //Height taken from where mouse was released

                        if (posX < 0)
                            posX = 0;
                        if (posX > gridBase.sizeX)
                            posX = gridBase.sizeX;
                        if (posZ < 0)
                            posZ = 0;
                        if (posZ > gridBase.sizeZ)
                            posZ = gridBase.sizeZ;

                        LevelWallObj.WallDirection targetDir = LevelWallObj.WallDirection.bc;
                        CreateWallInNode(posX, posZ, targetDir);
                    }

                    for (int i = 1; i < Mathf.Abs(difZ) + 1; i++) {
                        int offset = zHigher ? i : -i;
                        int posX = endNodeWall.nodePosX;
                        int posZ = startNodeWall.nodePosZ + offset;

                        if (posX < 0)
                            posX = 0;
                        if (posX > gridBase.sizeX)
                            posX = gridBase.sizeX;
                        if (posZ < 0)
                            posZ = 0;
                        if (posZ > gridBase.sizeZ)
                            posZ = gridBase.sizeZ;

                        LevelWallObj.WallDirection targetDir = LevelWallObj.WallDirection.ab;

                        CreateWallInNode(posX, posZ, targetDir);
                    }

                    // Corners for boxes, check if ab/bc are all switched
                    if (startNodeWall.nodePosZ > endNodeWall.nodePosZ) {
                        manager.inSceneWalls.Remove(finalXNode.wallObj.gameObject);
                        Destroy(finalXNode.wallObj.gameObject);
                        finalXNode.wallObj = null;


                        UpdateWallNode(finalZNode, LevelWallObj.WallDirection.all);
                        UpdateWallNode(endNodeWall, LevelWallObj.WallDirection.ab);

                        if (startNodeWall.nodePosX > endNodeWall.nodePosX) {
                            CreateWallOrUpdateNode(finalXNode, LevelWallObj.WallDirection.bc);
                            UpdateWallCorners(finalXNode, false, true, false);

                            CreateWallOrUpdateNode(finalZNode, LevelWallObj.WallDirection.ab);
                            UpdateWallCorners(finalZNode, false, true, false);

                            Node nextToStartNode = DestroyCurrentNodeAndGetPrevious(startNodeWall, true);
                            UpdateWallCorners(nextToStartNode, false, false, true);

                            CreateWallOrUpdateNode(endNodeWall, LevelWallObj.WallDirection.all);
                            UpdateWallCorners(endNodeWall, false, true, false);
                        } else {
                            Node beforeFinalX = DestroyCurrentNodeAndGetPrevious(finalXNode, true);
                            UpdateWallCorners(beforeFinalX, false, false, true);

                            CreateWallOrUpdateNode(finalZNode, LevelWallObj.WallDirection.all);
                            UpdateWallCorners(finalZNode, false, true, false);

                            CreateWallOrUpdateNode(startNodeWall, LevelWallObj.WallDirection.bc);
                            UpdateWallCorners(startNodeWall, false, true, false);

                            CreateWallOrUpdateNode(endNodeWall, LevelWallObj.WallDirection.ab);
                            UpdateWallCorners(endNodeWall, false, true, false);
                        }

                    } else {
                        if (startNodeWall.nodePosX > endNodeWall.nodePosX) {
                            Node northWestNode = DestroyCurrentNodeAndGetPrevious(finalZNode, true);
                            UpdateWallCorners(northWestNode, false, false, true);

                            CreateWallOrUpdateNode(finalXNode, LevelWallObj.WallDirection.all);
                            UpdateWallCorners(finalXNode, false, true, false);

                            CreateWallOrUpdateNode(startNodeWall, LevelWallObj.WallDirection.ab);
                            UpdateWallCorners(startNodeWall, false, true, false);

                            CreateWallOrUpdateNode(endNodeWall, LevelWallObj.WallDirection.bc);
                            UpdateWallCorners(endNodeWall, false, true, false);
                        } else {
                            CreateWallOrUpdateNode(finalZNode, LevelWallObj.WallDirection.bc);
                            UpdateWallCorners(finalZNode, false, true, false);

                            CreateWallOrUpdateNode(finalXNode, LevelWallObj.WallDirection.ab);
                            UpdateWallCorners(finalXNode, false, true, false);

                            CreateWallOrUpdateNode(startNodeWall, LevelWallObj.WallDirection.all);
                            UpdateWallCorners(startNodeWall, false, true, false);

                            Node nextToEndNode = DestroyCurrentNodeAndGetPrevious(endNodeWall, true);
                            UpdateWallCorners(nextToEndNode, false, false, true);
                        }
                    }
                }

                startNodeWall = null;
                endNodeWall = null;
            }
        }
    }

    private void CreateWallInNode(int posX, int posZ, LevelWallObj.WallDirection direction) {
        Node node = gridBase.grid[posX, posZ];
        Vector3 wallPosition = node.vis.transform.position;

        if (node.wallObj == null) {
            GameObject actualObjPlaced = Instantiate(wallPrefab, wallPosition, Quaternion.identity) as GameObject;
            LevelObject placedObjProperties = actualObjPlaced.GetComponent<LevelObject>();
            LevelWallObj placedWallProperties = actualObjPlaced.GetComponent<LevelWallObj>();

            placedObjProperties.gridPosX = posX;
            placedObjProperties.gridPosZ = posZ;
            manager.inSceneWalls.Add(actualObjPlaced);
            node.wallObj = placedWallProperties;

            UpdateWallNode(node, direction);
        } else {
            UpdateWallNode(node, direction);
        }

        UpdateWallCorners(node, false, false, false);
    }

    private void UpdateWallNode(Node node, LevelWallObj.WallDirection direction) {
        node.wallObj.UpdateWall(direction);
    }

    private void UpdateWallCorners(Node node, bool a, bool b, bool c) {
        if (node.wallObj != null)
            node.wallObj.UpdateCorners(a, b, c);
    }

    private void CreateWallOrUpdateNode(Node node, LevelWallObj.WallDirection direction) {
        if (node.wallObj == null) {
            CreateWallInNode(node.nodePosX, node.nodePosZ, direction);
        } else {
            UpdateWallNode(node, direction);
        }
    }

    private Node DestroyCurrentNodeAndGetPrevious(Node curNode, bool positive) {
        int i = (positive) ? 1 : -1;
        Node beforeCurNode = gridBase.grid[curNode.nodePosX - i, curNode.nodePosZ];

        if (curNode.wallObj != null) {
            if (manager.inSceneWalls.Contains(curNode.wallObj.gameObject)) {
                manager.inSceneWalls.Remove(curNode.wallObj.gameObject);
                Destroy(curNode.wallObj.gameObject);
                curNode.wallObj = null;
            }
        }
        return beforeCurNode;
    }

    public void DeleteWall() {
        CloseAll();
        deleteWall = true;
    }

    private void DeleteWallsActual() {
        if (deleteWall) {
            UpdateMousePosition();
            Node curNode = gridBase.NodeFromWorldPosition(mousePos);
            
            if (Input.GetMouseButtonDown(0) && !ui.mouseOverUIElement) {
                if (curNode.wallObj != null) {
                    if (manager.inSceneWalls.Contains(curNode.wallObj.gameObject)) {
                        manager.inSceneWalls.Remove(curNode.wallObj.gameObject);
                        Destroy(curNode.wallObj.gameObject);
                    }
                    curNode.wallObj = null;
                }
            }
        }
    }
    #endregion

    private void CloseAll() {
        hasObj = false;
        deleteObj = false;
        paintTile = false;
        placeStackObj = false;
        createWall = false;
        hasMaterial = false;
        deleteStackObj = false;
        deleteWall = false;
    }
}
