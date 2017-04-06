using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {
    public static GameManager current;
    public AutoTerrain autoTerrain;
    public GameObject agentPrefab;
    public Vector2 placementGrid = Vector2.one;

    Vector3 agentDestination;
    AIPath agent;

    Queue<Construction> constructionStack = new Queue<Construction>();

    private Construction constructionInProgress;

    void Awake()
    {
        current = this;
    }

	// Use this for initialization
	void Start () {
        autoTerrain.Generate();
        agent = Instantiate(agentPrefab, autoTerrain.TerrainCenter, Quaternion.identity).GetComponent<AIPath>();

        RTS_Cam.RTS_Camera cam = Camera.main.GetComponent<RTS_Cam.RTS_Camera>();
        cam.SetStartPosition(autoTerrain.TerrainCenter);
        cam.limitX = autoTerrain.limitX; 
        cam.limitY = autoTerrain.limitY;
    }
	
	// Update is called once per frame
	void Update () {
        //if (MouseButtonDownInWorld(0))
        //{
        //    Vector3? target = WorldMousePosition();
        //    if (target.HasValue)
        //    {
        //        GameObject.Find("PathfindingTarget").transform.position = target.Value;
        //        agent.target = GameObject.Find("PathfindingTarget").transform;
        //    }
        //}        
    }

    public bool MouseButtonDownInWorld(int mouseButton)
    {
        return !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(mouseButton);
    }

    public void StartConstruction(Construction construction)
    {
        constructionInProgress = Instantiate(construction);
        constructionInProgress.StartPlacement();
    }

    public void AddConstructionJob(Construction construction)
    {
        constructionStack.Enqueue(construction);
    }

    public Construction GetNextConstructionJob()
    {
        return constructionStack.Count > 0 ? constructionStack.Dequeue() : null;
    }

    public Vector3? WorldMousePosition()
    {
        RaycastHit hit;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }        

        return null;
    }
}
