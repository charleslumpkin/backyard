using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public bool isBuilding = false;
    public GameObject currentBuildingPart;
    public List<GameObject> buildingGroups = new List<GameObject>();
    public int totalParts = 0;
    public int currentPart = 0;
    public float rotationSpeed = 360f; // Speed of rotation
    private Vector3 currentAxis = Vector3.right; // Start with X axis
    private int rotationsCompleted = 0; // Track completed rotations on current axis
    private bool isRotating = false; // To prevent starting a new rotation before finishing the current


    // we need to come back and update thsi based on the size of the buildingPartsArray in the BuildingGroup
    public float maxBuildingGroupDistance = 20.0f;



    public GameObject FindOrAddBuildingGroup(Vector3 newBuildingGroupCoordinate)
    {
        GameObject returnGroup = FindClosestBuildingGroup(newBuildingGroupCoordinate);
        if (returnGroup == null)
        {
            returnGroup = AddBuildingGroup(newBuildingGroupCoordinate);
        }
        return returnGroup;
    }


    public GameObject FindClosestBuildingGroup(Vector3 newBuildingGroupCoordinate)
    {

        GameObject returnGroup = null;

        if (buildingGroups.Count > 0)
        {
            foreach (GameObject buildingGroup in buildingGroups)
            {
                if (Vector3.Distance(buildingGroup.transform.position, newBuildingGroupCoordinate) < maxBuildingGroupDistance)
                {
                    returnGroup = buildingGroup;
                }
            }
        }

        return returnGroup;
    }

    public GameObject AddBuildingGroup(Vector3 newBuildingGroupCoordinate)
    {
        GameObject newBuildingGroup = Instantiate(GameObject.Find("BuildingGroup"), newBuildingGroupCoordinate, Quaternion.identity);
        newBuildingGroup.transform.position = newBuildingGroupCoordinate;
        newBuildingGroup.transform.parent = GameObject.Find("Constructions").transform;
        newBuildingGroup.GetComponent<BuildingGroup>().SetBuildingGroupID(buildingGroups.Count);
        newBuildingGroup.transform.name = "BG-" + buildingGroups.Count;
        buildingGroups.Add(newBuildingGroup);

        return newBuildingGroup;
    }




    // Start is called before the first frame update
    void Start()
    {
        // Get the BuildingMaterials GameObject
        GameObject buildingParts = GameObject.Find("BuildingParts");

        // Check if the BuildingMaterials GameObject exists
        if (buildingParts != null)
        {
            // Check if the BuildingMaterials GameObject has any children
            if (buildingParts.transform.childCount > 0)
            {
                // Get the first child of the BuildingMaterials GameObject
                GameObject selectedPart = buildingParts.transform.GetChild(currentPart).gameObject;

                // Set the currentBuildingPart variable to the first child of the BuildingMaterials GameObject
                currentBuildingPart = selectedPart;
                currentBuildingPart.SetActive(false);
            }
        }
    }

    IEnumerator RotateAroundAxis(Vector3 axis)
    {
        isRotating = true;
        Quaternion startRotation = currentBuildingPart.transform.rotation;
        Quaternion endRotation = currentBuildingPart.transform.rotation * Quaternion.Euler(axis * 90);

        float time = 0;
        while (time < .25f)
        {
            currentBuildingPart.transform.rotation = Quaternion.Lerp(startRotation, endRotation, time);
            time += Time.deltaTime * rotationSpeed / 90f;
            yield return null;
        }

        currentBuildingPart.transform.rotation = endRotation; // Ensures the rotation completes exactly at 90 degrees

        rotationsCompleted++;
        if (rotationsCompleted == 4) // After completing 360 degrees
        {
            rotationsCompleted = 0; // Reset counter
            SwitchAxis(); // Switch to the next axis
        }
        isRotating = false;
    }

    void SwitchAxis()
    {
        if (currentAxis == Vector3.right) // If current is X
        {
            currentAxis = Vector3.up; // Switch to Y
        }
        else if (currentAxis == Vector3.up) // If current is Y
        {
            currentAxis = Vector3.forward; // Switch to Z
        }
        else // If current is Z
        {
            currentAxis = Vector3.right; // Back to X
        }
    }



    // Update is called once per frame
    void Update()
    {

        // Check if the right mouse button is clicked
        if (Input.GetMouseButtonDown(1))
        {
            // Toggle the isBuilding variable
            isBuilding = !isBuilding;
        }

        if (isBuilding)
        {
            currentBuildingPart.SetActive(true);

            // Define the origin and direction of the ray
            Vector3 origin = transform.position;
            Vector3 direction = transform.forward;

            // Create a raycast hit variable to store the information of the hit point
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity, ~LayerMask.GetMask("SelectedBuildingPart")))
            {
                // Check if the hit object is the terrain
                if (hit.collider.CompareTag("Terrain") || hit.collider.CompareTag("BuildingPart"))
                {
                    hit.point -= direction * 0.01f;
                    hit.point = new Vector3((int)hit.point.x + 0.5f, (int)hit.point.y, (int)hit.point.z + 0.5f);
                    // Check if the raycast length is less than 10f
                    if (hit.distance < 10f)
                    {
                        // Set the position of the currentBuildingPart to the hit point
                        currentBuildingPart.transform.position = hit.point;
                        currentBuildingPart.SetActive(true);

                        if (Input.mouseScrollDelta.y != 0)
                        {
                            GameObject buildingParts = GameObject.Find("BuildingParts");
                            int childCount = buildingParts.transform.childCount;
                            // Calculate the new index based on the scroll direction
                            int newIndex = currentPart + (int)Mathf.Sign(Input.mouseScrollDelta.y);

                            // Wrap around the index if it goes beyond the child count
                            if (newIndex < 0)
                            {
                                newIndex = childCount - 1;
                            }
                            else if (newIndex >= childCount)
                            {
                                newIndex = 0;
                            }

                            currentPart = newIndex;
                            // Get the new selected part based on the new index

                            currentBuildingPart.SetActive(false);

                            GameObject selectedPart = buildingParts.transform.GetChild(currentPart).gameObject;

                            // Set the currentBuildingPart variable to the new selected part
                            currentBuildingPart = selectedPart;
                            currentBuildingPart.SetActive(false);
                        }

                        if (Input.GetKeyDown(KeyCode.R) && !isRotating)
                        {
                            StartCoroutine(RotateAroundAxis(currentAxis));
                        }


                        if (Input.GetMouseButtonDown(0))
                        {
                            GameObject selectedBuildingGroup = FindOrAddBuildingGroup(hit.point);
                            selectedBuildingGroup.GetComponent<BuildingGroup>().AddBuildingPart(currentBuildingPart, hit.point);

                        }
                    }
                    else
                    {
                        currentBuildingPart.SetActive(false);
                    }


                    // Draw a debug ray from the object to the nearest point on the terrain
                    Debug.DrawRay(origin, direction * hit.distance, Color.red);
                }
            }
        }
        else
        {
            currentBuildingPart.SetActive(false);
        }
    }
}

