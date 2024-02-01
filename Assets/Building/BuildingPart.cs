using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPart : MonoBehaviour
{
    public int buildingPartID = 0;
    public bool enableGizmos = false;
    public float mass = 10f;
    public float totalSupportedMass = 0f;
    public float maxSupporatbleMass = 80f;
    public int supportDistance = 10;
    public bool isTouchingGround = false;
    public bool isVerticalSupport = false;
    public bool isHorizontalSupport = false;
    public bool isOverloaded = false;
    public List<BuildingPartData> supportedBuildingParts = new List<BuildingPartData>();
    public List<BuildingPartData> supportingBuildingParts = new List<BuildingPartData>();
    public bool changed = true;

    public (int x, int y, int z) localPosition = (0, 0, 0);

    public void calcTotalSupportedMass()
    {
        totalSupportedMass = 0f;
        for (int i = 0; i < supportingBuildingParts.Count; i++)
        {
            totalSupportedMass += supportingBuildingParts[i].mass;
        }
    }

    public void addSupportedLoad(int buildingPartID, int distance, float supportedMass)
    {
        supportedBuildingParts.Add(new BuildingPartData(buildingPartID, distance, supportedMass));
    }

    public BuildingPartData findSupportedLoad(int buildingPartID)
    {
        BuildingPartData returnBuildingPart = new BuildingPartData(0, 0, 0);
        for (int i = 0; i < supportedBuildingParts.Count; i++)
        {
            if (supportedBuildingParts[i].buildingPartID == buildingPartID)
            {
                returnBuildingPart = supportedBuildingParts[i];

            }
        }

        return returnBuildingPart;
    }

    public void removeSupportedLoad(int buildingPartID)
    {
        for (int i = 0; i < supportedBuildingParts.Count; i++)
        {
            if (supportedBuildingParts[i].buildingPartID == buildingPartID)
            {
                supportedBuildingParts.RemoveAt(i);
            }
        }
    }

    public void updateSupportedLoad(int buildingPartID, int distance, float supportedMass)
    {
        BuildingPartData supportedBuildingPart = findSupportedLoad(buildingPartID);
        supportedBuildingPart.distance = distance;
        supportedBuildingPart.mass = supportedMass;

    }

    public void addSupportingLoad(int buildingPartID, int distance, float supportingMass)
    {
        supportingBuildingParts.Add(new BuildingPartData(buildingPartID, distance, supportingMass));
    }

    public BuildingPartData findSupportingLoad(int buildingPartID)
    {
        BuildingPartData returnBuildingPart = new BuildingPartData(0, 0, 0);
        for (int i = 0; i < supportingBuildingParts.Count; i++)
        {
            if (supportingBuildingParts[i].buildingPartID == buildingPartID)
            {
                returnBuildingPart = supportingBuildingParts[i];
            }
        }

        return returnBuildingPart;
    }

    public void removeSupportingLoad(int buildingPartID)
    {
        for (int i = 0; i < supportingBuildingParts.Count; i++)
        {
            if (supportingBuildingParts[i].buildingPartID == buildingPartID)
            {
                supportingBuildingParts.RemoveAt(i);
            }
        }
    }

    public void updateSupportingLoad(int buildingPartID, int distance, float supportingMass)
    {
        BuildingPartData supportingBuildingPart = findSupportingLoad(buildingPartID);
        supportingBuildingPart.distance = distance;
        supportingBuildingPart.mass = supportingMass;

    }

    public void updateBidirectionalLoads()
    {
        if (isHorizontalSupport)
        {

            (int x, int y, int z) translatedLocalPosition = transform.parent.gameObject.GetComponent<BuildingGroup>().translateCoordinateWithOffset(localPosition);
            Dictionary<int, int> verticalSupports = transform.parent.gameObject.GetComponent<BuildingGroup>().FindShortestPathsToVerticalSupports(translatedLocalPosition, supportDistance);

            float dividedMass = mass / verticalSupports.Count;

            foreach (KeyValuePair<int, int> entry in verticalSupports)
            {
                if (entry.Value <= supportDistance)
                {
                    if (findSupportedLoad(entry.Key).buildingPartID == 0)
                    {
                        addSupportedLoad(entry.Key, entry.Value, dividedMass);
                        transform.parent.gameObject.GetComponent<BuildingGroup>().FindBuildingPartByID(entry.Key).GetComponent<BuildingPart>().addSupportingLoad(buildingPartID, entry.Value, dividedMass);
                    }
                    else
                    {
                        updateSupportedLoad(entry.Key, entry.Value, dividedMass);
                        transform.parent.gameObject.GetComponent<BuildingGroup>().FindBuildingPartByID(entry.Key).
                        GetComponent<BuildingPart>().updateSupportingLoad(buildingPartID, entry.Value, dividedMass);
                    }
                }
            }

        }

        if (isVerticalSupport)
        {
            (int x, int y, int z) translatedLocalPosition = transform.parent.gameObject.GetComponent<BuildingGroup>().translateCoordinateWithOffset(localPosition);
            Dictionary<int, int> horizontalSupports = transform.parent.gameObject.GetComponent<BuildingGroup>().FindShortestPathsToHorizontalSupports(translatedLocalPosition, supportDistance);

            foreach (KeyValuePair<int, int> entry in horizontalSupports)
            {
                if (entry.Value <= supportDistance)
                {
                    addSupportingLoad(entry.Key, entry.Value, 0);
                    transform.parent.gameObject.GetComponent<BuildingGroup>().FindBuildingPartByID(entry.Key).GetComponent<BuildingPart>().addSupportedLoad(buildingPartID, entry.Value, 0);
                    transform.parent.gameObject.GetComponent<BuildingGroup>().FindBuildingPartByID(entry.Key).GetComponent<BuildingPart>().changed = true;
                }
            }
        }
    }


    void Update()
    {
        if (buildingPartID != 0)
        {
            if (changed)
            {
                isTouchingGround = isTouchingGroundCheck();
                isVerticalSupport = isVerticalSupportCheck();
                isHorizontalSupport = !isVerticalSupport;
                updateBidirectionalLoads();
                changed = false;
            }

            if (isVerticalSupport)
            {
                calcTotalSupportedMass();
                isOverloaded = isVerticalOverloadedCheck();
            }
        }
    }

    public void SetBuildingPartID(int newBuildingPartID)
    {
        buildingPartID = newBuildingPartID;
    }

    public int GetBuildingPartID()
    {
        return buildingPartID;
    }

    void OnDrawGizmos()
    {
        if (enableGizmos)
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3(transform.position.x, transform.position.y + transform.localScale.y / 2, transform.position.z);
            Gizmos.DrawWireCube(center, transform.localScale);
        }
    }

    private void OnGUI()
    {
        float fps = 1f / Time.deltaTime;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.LowerRight;
        style.fontSize = 20;
        style.normal.textColor = Color.red;

        GUI.Label(new Rect(Screen.width - 100, Screen.height - 30, 100, 30), $"FPS: {fps.ToString("F1d")}", style);
    }

    public bool isTouchingGroundCheck()
    {
        bool returnBool = false;
        Vector3 center = new Vector3(transform.position.x, transform.position.y + transform.localScale.y / 2, transform.position.z);
        Collider[] colliders = Physics.OverlapBox(center, transform.localScale / 2, Quaternion.identity, LayerMask.GetMask("Terrain"));
        
        if (colliders.Length > 0)
        {
            returnBool = true;
        }
        else
        {
            returnBool = false;
        }

        return returnBool;
    }

    public bool isVerticalOverloadedCheck()
    {
        bool returnBool = false;
        
        if (isVerticalSupport)
        {
            if (totalSupportedMass > maxSupporatbleMass)
            {
                returnBool = true;
            }
            else
            {
                returnBool = false;
            }
        }

        return returnBool;
    }

    public bool isVerticalSupportCheck()
    {
        bool returnBool = false;

        if (transform.parent.gameObject.GetComponent<BuildingGroup>().CheckSupport(localPosition) || isTouchingGround)
        {
            returnBool = true;
        }
        else
        {
            returnBool = false;
        }

        return returnBool;
    }
}