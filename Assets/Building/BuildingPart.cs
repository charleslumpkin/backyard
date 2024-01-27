
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class  BuildingPart : MonoBehaviour
{
    public int buildingPartID;
    public bool isVerticalSupport = false;
    private float mass = 10f;
   //private float maxSupporatbleMass = 80f;
    
    public void SetBuildingPartID(int newBuildingPartID)
    {
        buildingPartID = newBuildingPartID;
    }

    public void SetMass(float newMass)
    {
        mass = newMass;
    }

    public float GetMass()
    {
        return mass;
    }

    public void SetIsVerticalSupport(bool newIsVerticalSupport)
    {
        isVerticalSupport = newIsVerticalSupport;
    }   

    public bool GetIsVerticalSupport()
    {
        return isVerticalSupport;
    }   
 
    public int GetBuildingPartID()
    {
        return buildingPartID;
    }

    

}