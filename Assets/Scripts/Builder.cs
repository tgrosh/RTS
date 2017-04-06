using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour {
    bool isBusy;
    Construction currentJob;
    AIPath navAgent;
    Transform navTarget;

	// Use this for initialization
	void Start () {
        navAgent = GetComponent<AIPath>();
	}
	
	// Update is called once per frame
	void Update () {
		if (currentJob == null)
        {
            currentJob = GameManager.current.GetNextConstructionJob();

            if (currentJob != null)
            {
                isBusy = true;
                navTarget = currentJob.targetPosition;
                navAgent.target = navTarget;                
            } else if (isBusy)
            {
                isBusy = false;
            }
        } else {
            if (!currentJob.isConstructing)
            {
                if (Vector3.Distance(transform.position, navTarget.position) < .5f)
                {
                    currentJob.StartContstruction();
                }
            }
        }
    }
}
