using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Construction : MonoBehaviour {
    public GameObject placementPrefab;
    public ParticleSystem constructionParticlesPrefab;
    public GameObject[] constructionStages;
    public float constructionStageDuration;
    [HideInInspector]
    public Transform targetPosition;
    [HideInInspector]
    public bool isConstructing = false;

    GameObject currentStage;
    GameObject constructionPlacement;
    ParticleSystem constructionParticles;
    float currentConstructionStageTime;
    int currentConstructionStage = 0;
    bool isPlacing = false;
    Vector3 position;
    Quaternion rotation;
    
    public void StartContstruction()
    {
        Destroy(constructionPlacement);
        constructionPlacement = null;
        
        ConstructStage(0);
        constructionParticles = Instantiate(constructionParticlesPrefab, position, rotation);
        isConstructing = true;
    }

    public void StopConstruction()
    {
        Destroy(constructionParticles.gameObject);
        Destroy(gameObject);
        isConstructing = false;
    }

    public void StartPlacement()
    {
        isPlacing = true;
        constructionPlacement = Instantiate(placementPrefab);
        FollowMouse mouseFollower = constructionPlacement.AddComponent<FollowMouse>();
        mouseFollower.SnapToGrid(GameManager.current.placementGrid);
    }

    public void RequestConstruction(Vector3 position, Quaternion rotation)
    {
        Destroy(constructionPlacement.GetComponent<FollowMouse>());
        this.position = position;
        this.rotation = rotation;
        GameManager.current.AddConstructionJob(this);
    }

    void ConstructStage(int stage)
    {
        if (currentStage != null)
        {
            Destroy(currentStage);
        }
        currentStage = Instantiate(constructionStages[stage], position, rotation);
    }

    void Update()
    {
        if (isPlacing)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Destroy(constructionPlacement);
                constructionPlacement = null;
                isPlacing = false;
            }
            if (GameManager.current.MouseButtonDownInWorld(0))
            {
                //not how we will do it later. want the agent to start construction when it arrives
                RequestConstruction(constructionPlacement.transform.position, constructionPlacement.transform.rotation);
                targetPosition = constructionPlacement.transform;
                isPlacing = false;
            }
        }

        if (isConstructing)
        {
            currentConstructionStageTime += Time.deltaTime;
            if (currentConstructionStageTime > constructionStageDuration)
            {
                currentConstructionStageTime = 0;
                currentConstructionStage++;
                ConstructStage(currentConstructionStage);
                if (currentConstructionStage == constructionStages.Length - 1)
                {
                    StopConstruction();
                }
            }
        }
    }
}
