using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class AutoTerrainCell
{
    public AutoTerrainPrefab template;
    public GameObject instance;

    public AutoTerrainCell()
    {
    }

    public AutoTerrainCell(AutoTerrainPrefab template)
    {
        this.template = template;
    }

    public AutoTerrainCell(AutoTerrainPrefab template, GameObject instance)
    {
        this.template = template;
        this.instance = instance;
    }
}
