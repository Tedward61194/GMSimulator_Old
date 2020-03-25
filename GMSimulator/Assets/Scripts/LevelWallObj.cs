using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelWallObj : MonoBehaviour
{
    public WallDirection wallDirection;
    public bool cornerA;
    public bool cornerB;
    public bool cornerC;

    public enum WallDirection {
        ab,
        bc,
        all
    }

    public List<WallBase> wallsList = new List<WallBase>();
    public List<CornerBase> cornersList = new List<CornerBase>();

    public void UpdateWall(WallDirection direction) {
        wallsList[0].active = false;
        wallsList[0].wall.SetActive(false);
        wallsList[1].active = false;
        wallsList[1].wall.SetActive(false);

        switch(direction) {
            case WallDirection.ab:
                wallsList[0].active = true;
                wallsList[0].wall.SetActive(true);
                break;
            case WallDirection.bc:
                wallsList[1].active = true;
                wallsList[1].wall.SetActive(true);
                break;
            case WallDirection.all:
                wallsList[0].active = true;
                wallsList[0].wall.SetActive(true);
                wallsList[1].active = true;
                wallsList[1].wall.SetActive(true);
                break;
        }
        wallDirection = direction;
    }

    public void UpdateCorners(bool a, bool b, bool c) {
        cornersList[0].corner.SetActive(a);
        cornersList[1].corner.SetActive(b);
        cornersList[2].corner.SetActive(c);

        cornerA = a;
        cornerB = b;
        cornerC = c;
    }

    public WallObjectSaveableProperties GetSaveable() {
        WallObjectSaveableProperties s = new WallObjectSaveableProperties();
        s.direction = wallDirection;
        s.cornerA = cornerA;
        s.cornerB = cornerB;
        s.cornerC = cornerC;

        return s;
    }

    [Serializable]
    public class WallObjectSaveableProperties {
        public WallDirection direction;
        public bool cornerA;
        public bool cornerB;
        public bool cornerC;
    }

    [Serializable]
    public class WallBase {
        public bool active;
        public GameObject wall;
    }

    [Serializable]
    public class CornerBase {
        public GameObject corner;
    }
}
