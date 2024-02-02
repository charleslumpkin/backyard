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
    private bool destroyToggled = false;

    public (int x, int y, int z) localPosition = (0, 0, 0);

    public void calcTotalSupportedMass()
    {
        totalSupportedMass = 0f;
        for (int i = 0; i < supportedBuildingParts.Count; i++)
        {
            totalSupportedMass += supportedBuildingParts[i].mass;
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
        // Debug.Log("removeSupportedLoad");
        // Debug.Log("Removing " + buildingPartID + " from supportedBuildingParts." + this.buildingPartID);
        for (int i = 0; i < supportedBuildingParts.Count; i++)
        {
            if (supportedBuildingParts[i].buildingPartID == buildingPartID)
            {
                // Debug.Log("FOUND MATCH at " + i);
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
        // Debug.Log("removeSupportingLoad");
        // Debug.Log("Removing " + buildingPartID + " from supportingBuildingParts." + this.buildingPartID);
        for (int i = 0; i < supportingBuildingParts.Count; i++)
        {
            if (supportingBuildingParts[i].buildingPartID == buildingPartID)
            {
                // Debug.Log("FOUND MATCH at " + i);
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

    public void AddOrUpdateSupportedLoad(int buildingPartID, int distance, float supportedMass)
    {
        if (findSupportedLoad(buildingPartID).buildingPartID != 0)
        {
            updateSupportedLoad(buildingPartID, distance, supportedMass);

        }
        else
        {
            addSupportedLoad(buildingPartID, distance, supportedMass);
        }
    }

    public void AddOrUpdateSupportingLoad(int buildingPartID, int distance, float supportingMass)
    {
        if (findSupportingLoad(buildingPartID).buildingPartID != 0)
        {
            updateSupportingLoad(buildingPartID, distance, supportingMass);
        }
        else
        {
            addSupportingLoad(buildingPartID, distance, supportingMass);
        }
    }

    public void updateBidirectionalLoads()
    {
        // Debug.Log("Updating bidirectional loads: " + buildingPartID);
        if (isHorizontalSupport)
        {
            // Debug.Log("isHorizontalSupport");
            (int x, int y, int z) translatedLocalPosition = transform.parent.gameObject.GetComponent<BuildingGroup>().translateCoordinateWithOffset(localPosition);
            Dictionary<int, int> verticalSupports = transform.parent.gameObject.GetComponent<BuildingGroup>().FindShortestPathsToVerticalSupports(translatedLocalPosition, supportDistance);

            float dividedMass = mass / verticalSupports.Count;

            foreach (KeyValuePair<int, int> entry in verticalSupports)
            {
                if (entry.Value <= supportDistance)
                {
                    // Debug.Log("Adding or updating supported load to: " + entry.Key + " with distance: " + entry.Value + " and mass: " + dividedMass);
                    transform.parent.gameObject.GetComponent<BuildingGroup>().FindBuildingPartByID(entry.Key).GetComponent<BuildingPart>().AddOrUpdateSupportedLoad(buildingPartID, entry.Value, dividedMass);
                    transform.parent.gameObject.GetComponent<BuildingGroup>().FindBuildingPartByID(entry.Key).GetComponent<BuildingPart>().changed = true;
                }
            }

        }

        if (isVerticalSupport)
        {
            // Debug.Log("isVerticalSupport");
            (int x, int y, int z) translatedLocalPosition = transform.parent.gameObject.GetComponent<BuildingGroup>().translateCoordinateWithOffset(localPosition);
            Dictionary<int, int> horizontalSupports = transform.parent.gameObject.GetComponent<BuildingGroup>().FindShortestPathsToHorizontalSupports(translatedLocalPosition, supportDistance);

            foreach (KeyValuePair<int, int> entry in horizontalSupports)
            {
                if (entry.Value <= supportDistance)
                {
                    // Debug.Log("Adding or updating supporting load to: " + entry.Key + " with distance: " + entry.Value + " and mass: " + mass);
                    transform.parent.gameObject.GetComponent<BuildingGroup>().FindBuildingPartByID(entry.Key).GetComponent<BuildingPart>().AddOrUpdateSupportingLoad(buildingPartID, entry.Value, 0);

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

                if (isOverloaded)
                {
                    destroyBuildingPart();
                }
            }

            if (isHorizontalSupport)
            {

                if (isOverloaded)
                {
                    destroyBuildingPart();
                }
            }
        }
    }

    public void destroyBuildingPart()
    {
        if (!destroyToggled)
        {
            // Debug.Log("Destroying building part: " + buildingPartID);
            if (isVerticalSupport)
            {
                // Debug.Log("isVerticalSupport");
                foreach (BuildingPartData supportedBuildingPart in supportedBuildingParts)
                {
                    // Debug.Log("Removing supporting load from: " + supportedBuildingPart.buildingPartID);
                    transform.parent.gameObject.GetComponent<BuildingGroup>().FindBuildingPartByID(supportedBuildingPart.buildingPartID).GetComponent<BuildingPart>().removeSupportingLoad(buildingPartID);
                    transform.parent.gameObject.GetComponent<BuildingGroup>().FindBuildingPartByID(supportedBuildingPart.buildingPartID).GetComponent<BuildingPart>().changed = true;
                    transform.parent.gameObject.GetComponent<BuildingGroup>().FindBuildingPartByID(supportedBuildingPart.buildingPartID).GetComponent<BuildingPart>().isOverloaded = true;
                }

            }
            if (isHorizontalSupport)
            {
                // Debug.Log("isHorizontalSupport");
                foreach (BuildingPartData supportingBuildingPart in supportingBuildingParts)
                {
                    // Debug.Log("Removing supported load from: " + supportingBuildingPart.buildingPartID);
                    transform.parent.gameObject.GetComponent<BuildingGroup>().FindBuildingPartByID(supportingBuildingPart.buildingPartID).GetComponent<BuildingPart>().removeSupportedLoad(buildingPartID);
                    transform.parent.gameObject.GetComponent<BuildingGroup>().FindBuildingPartByID(supportingBuildingPart.buildingPartID).GetComponent<BuildingPart>().changed = true;
                }
            }
            transform.parent.gameObject.GetComponent<BuildingGroup>().RemoveBuildingPartFromMatrix((transform.parent.gameObject.GetComponent<BuildingGroup>().translateCoordinateWithOffset(localPosition)));
            gameObject.AddComponent<Rigidbody>();
            Destroy(gameObject, 3f); // Destroy the parent GameObject after 3 seconds
            destroyToggled = true;
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

        GUI.Label(new Rect(Screen.width - 100, Screen.height - 30, 100, 30), $"FPS: {fps.ToString("0.00")}", style);
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