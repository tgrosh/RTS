using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour {    
    Vector2 gridSize = Vector2.one;
    bool snapToGrid;

    public void SnapToGrid(Vector3 gridSize)
    {
        this.snapToGrid = true;
        this.gridSize = gridSize;
    }

	// Update is called once per frame
	void Update () {
        Vector3? point = GameManager.current.WorldMousePosition();
        if (point.HasValue)
        {
            if (!snapToGrid)
            {
                transform.position = new Vector3(point.Value.x, point.Value.y + .01f, point.Value.z);
            } else
            {
                transform.position = new Vector3(((int)(point.Value.x / gridSize.x)) * gridSize.x, point.Value.y + .01f, ((int)(point.Value.z / gridSize.y)) * gridSize.y);
            }
        }
    }
}
