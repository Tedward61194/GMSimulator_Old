using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBase : MonoBehaviour
{
    public GameObject nodePrefab;

    public int sizeX;
    public int sizeZ;
    public int offset = 1; //Needed because size is not 1x1

    public Node[,] grid;

    public static GridBase instance = null; //Enforces Singleton pattern
    public static GridBase GetInstance() {
        return instance;
    }

    private void Awake() {
        instance = this;
        CreateGrid();
        CreateMouseCollision();
    }

    private void CreateGrid() {
        grid = new Node[sizeX, sizeZ];

        for (int x = 0; x < sizeX; x++) {
            for (int z = 0; z < sizeZ; z++) {
                float posX = x * offset;
                float posZ = z * offset;

                // create floor at position and attach to parent
                GameObject go = Instantiate(nodePrefab, new Vector3(posX, 0, posZ), Quaternion.identity) as GameObject;
                go.transform.parent = transform.GetChild(1).transform;

                //Used for Serialization
                //NodeObject nodeObj = go.GetComponent<NodeObject>();
                //nodeObj.posX = x;
                //nodeObj.posZ = z;

                Node node = new Node();
                node.vis = go;
                node.tileRenderer = node.vis.GetComponentInChildren<MeshRenderer>();
                node.isWalkable = true;
                node.nodePosX = x;
                node.nodePosZ = z;
                grid[x, z] = node;
            }
        }
    }

    private void CreateMouseCollision() {
        GameObject go = new GameObject();
        go.AddComponent<BoxCollider>();
        go.GetComponent<BoxCollider>().size = new Vector3(sizeX * offset, 0.1f, sizeZ * offset);
        go.transform.position = new Vector3((sizeX * offset) / 2 - 1, 0, (sizeZ * offset) / 2 - 1); //position = size, divided by 2 b/c position is in center, minus radius of node.vis
    }

    public Node NodeFromWorldPosition(Vector3 worldPosition) {
        float worldX = worldPosition.x;
        float worldZ = worldPosition.z;

        worldX /= offset;
        worldZ /= offset;

        int x = Mathf.RoundToInt(worldX);
        int z = Mathf.RoundToInt(worldZ);

        //int x = Mathf.FloorToInt(worldX);
        //int z = Mathf.FloorToInt(worldZ);

        if (x >= sizeX)
            x = sizeX-1;
        if (z >= sizeZ)
            z = sizeZ-1;
        if (x < 0)
            x = 0;
        if (z < 0)
            z = 0;

        return grid[x, z];
    }
}
