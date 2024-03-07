using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine.UIElements;


[System.Serializable]
public class BuildingPartData
{
    public int buildingPartID;
    public int distance;
    public float mass;

    public BuildingPartData(int id, int dist, float m)
    {
        buildingPartID = id;
        distance = dist;
        mass = m;
    }
}


public class BuildingGroup : MonoBehaviour
{
    public int buildingGroupID;
    public int[,,] bpMatrix;
    public Vector3 offset;
    public int bpMatrixLowerBoundX = 0;
    public int bpMatrixLowerBoundY = 0;
    public int bpMatrixLowerBoundZ = 0;
    public int bpMatrixUpperBoundX = 0;
    public int bpMatrixUpperBoundY = 0;
    public int bpMatrixUpperBoundZ = 0;
    public int bpOffsetX = 0;
    public int bpOffsetY = 0;
    public int bpOffsetZ = 0;

    public void RemoveBuildingPartFromMatrix((int x, int y, int z) bpCoord)
    {
        if (bpCoord.x >= 0 && bpCoord.x < bpMatrix.GetLength(0) && bpCoord.y >= 0 && bpCoord.y < bpMatrix.GetLength(1) && bpCoord.z >= 0 && bpCoord.z < bpMatrix.GetLength(2))
            bpMatrix[bpCoord.x, bpCoord.y, bpCoord.z] = 0;
    }


    public Dictionary<int, int> FindShortestPathsToVerticalSupports((int x, int y, int z) start, int maxDistance)
    {
        Dictionary<int, int> verticalSupportsDistances = new Dictionary<int, int>();
        Queue<((int x, int y, int z) position, int distance)> queue = new Queue<((int, int, int), int)>();
        HashSet<(int, int, int)> visited = new HashSet<(int, int, int)>();

        queue.Enqueue((start, 0)); // Start with the initial position and a distance of 0

        while (queue.Count > 0)
        {
            var (current, distance) = queue.Dequeue();

            if (distance > maxDistance || visited.Contains(current))
            {
                continue;
            }

            visited.Add(current); // Mark as visited
            int partId = bpMatrix[current.x, current.y, current.z];

            if (partId != 0)
            {
                BuildingPart part = FindBuildingPart((current.x, current.y, current.z)).GetComponent<BuildingPart>();
                if (part.isVerticalSupport)
                {

                    if (!verticalSupportsDistances.ContainsKey(partId) && distance != 0) // Exclude the starting point if it's a vertical support
                    {
                        verticalSupportsDistances[partId] = distance;
                    }
                    continue; // Stop expanding from this point as it's a vertical support
                }
            }

            foreach (var dir in new (int, int, int)[] { (1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1) })
            {
                (int x, int y, int z) next = (current.x + dir.Item1, current.y + dir.Item2, current.z + dir.Item3);
                if (IsWithinBounds(next) && !visited.Contains(next) && bpMatrix[next.x, next.y, next.z] != 0)
                {
                    queue.Enqueue((next, distance + 1));

                }
            }
        }

        return verticalSupportsDistances;
    }


    public Dictionary<int, int> FindShortestPathsToHorizontalSupports((int x, int y, int z) start, int maxDistance)
    {
        Dictionary<int, int> horizontalSupportsDistances = new Dictionary<int, int>();
        Queue<((int x, int y, int z) position, int distance)> queue = new Queue<((int, int, int), int)>();
        HashSet<(int, int, int)> visited = new HashSet<(int, int, int)>();

        queue.Enqueue((start, 0)); // Start with the initial position and a distance of 0

        while (queue.Count > 0)
        {
            var (current, distance) = queue.Dequeue();

            if (distance > maxDistance || visited.Contains(current))
            {
                continue;
            }

            visited.Add(current); // Mark as visited
            int partId = bpMatrix[current.x, current.y, current.z];

            if (partId != 0)
            {
                BuildingPart part = FindBuildingPart((current.x, current.y, current.z)).GetComponent<BuildingPart>();
                if (part.isHorizontalSupport)
                {
                    if (!horizontalSupportsDistances.ContainsKey(partId) && distance != 0)
                    {
                        horizontalSupportsDistances[partId] = distance;
                    }
                    // continue; // Stop expanding from this point as it's a horizontal support
                }
            }

            foreach (var dir in new (int, int, int)[] { (1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1) })
            {
                (int x, int y, int z) next = (current.x + dir.Item1, current.y + dir.Item2, current.z + dir.Item3);
                if (IsWithinBounds(next) && !visited.Contains(next) && bpMatrix[next.x, next.y, next.z] != 0 && FindBuildingPart((next.x, next.y, next.z)).GetComponent<BuildingPart>().isHorizontalSupport)
                {
                    queue.Enqueue((next, distance + 1));
                }
            }
        }

        return horizontalSupportsDistances;
    }


    public bool IsWithinBounds((int x, int y, int z) position)
    {
        return position.x >= 0 && position.x < bpMatrix.GetLength(0) &&
               position.y >= 0 && position.y < bpMatrix.GetLength(1) &&
               position.z >= 0 && position.z < bpMatrix.GetLength(2);
    }

    public GameObject FindBuildingPart((int x, int y, int z) bpCoord)
    {
        GameObject result = null;
        int[,,] matrix = bpMatrix;

        if (bpCoord.x >= 0 && bpCoord.x < matrix.GetLength(0) && bpCoord.y >= 0 && bpCoord.y < matrix.GetLength(1) && bpCoord.z >= 0 && bpCoord.z < matrix.GetLength(2))
        {
            int buildingPartID = matrix[bpCoord.x, bpCoord.y, bpCoord.z];
            if (buildingPartID != 0)
            {
                result = transform.Find("BP-" + buildingPartID).gameObject;
            }
        }

        return result;
    }

    public GameObject FindBuildingPartByID(int buildingPartID)
    {
        return transform.Find("BP-" + buildingPartID).gameObject;
    }

    public void AddBuildingPart(GameObject newBuildingPart, Vector3 newBuildingPartCoordinate)
    {
        int currentID = 0;
        GameObject characterCam = GameObject.Find("CharacterCam");

        if (characterCam != null)
        {
            BuildingManager buildingManager = characterCam.GetComponent<BuildingManager>();
            if (buildingManager != null)
            {
                buildingManager.totalParts += 1;
                currentID = buildingManager.totalParts;
            }
        }

        GameObject buildingPart = Instantiate(newBuildingPart, newBuildingPartCoordinate, newBuildingPart.transform.rotation);
        buildingPart.transform.parent = transform;
        buildingPart.transform.name = "BP-" + currentID;
        buildingPart.layer = LayerMask.NameToLayer("BuildingPart");
        buildingPart.transform.tag = "BuildingPart";
        buildingPart.GetComponent<BuildingPart>().SetBuildingPartID(currentID);
        buildingPart.GetComponent<BuildingPart>().localPosition = ((int)buildingPart.transform.localPosition.x, (int)buildingPart.transform.localPosition.y, (int)buildingPart.transform.localPosition.z);
        buildingPart.GetComponent<Renderer>().material.color = Color.white;
        buildingPart.GetComponent<BoxCollider>().enabled = true;

        ExpandMatrixAndInsertPart(buildingPart.transform.localPosition, buildingPart);

        AstarPath.active.Scan();


    }


    public void recalculateAIGraph()
    {
        for (int x = 0; x < bpMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < bpMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < bpMatrix.GetLength(2); z++)
                {
                    if (bpMatrix[x, y, z] != 0)
                    {
                        GameObject part = FindBuildingPart((x, y, z));
                        part.GetComponent<BuildingPart>().recalculateAIGraph();
                    }
                }
            }
        }

    }


    void Awake()
    {
        bpMatrix = new int[1, 1, 1];
    }

    public bool CheckSupport((int x, int y, int z) bpCoord)
    {
        bool verbose = false;
        bool result = false;
        int[,,] matrix = bpMatrix;
        (int x, int y, int z) translatedCoord = (bpCoord.x + bpOffsetX, bpCoord.y + bpOffsetY, bpCoord.z + bpOffsetZ);

        if (verbose)
        {
            PrintMatrix("CheckSupport", matrix);
        }

        if (translatedCoord.y > 0)
        {
            if (matrix[translatedCoord.x, translatedCoord.y - 1, translatedCoord.z] != 0)
            {
                GameObject partBelow = FindBuildingPart((translatedCoord.x, translatedCoord.y - 1, translatedCoord.z));
                if (partBelow != null)
                {
                    if (partBelow.GetComponent<BuildingPart>().isVerticalSupport || partBelow.GetComponent<BuildingPart>().isTouchingGround)
                    {
                        result = true;
                    }
                }
            }
        }

        return result;

    }

    //The new one
    public bool HasUninterruptedVerticalSupportToGround((int x, int y, int z) startCoord)
    {
        int x = startCoord.x + bpOffsetX;
        int y = startCoord.y + bpOffsetY;
        int z = startCoord.z + bpOffsetZ;

        // Check if the startCoord is already on the ground
        if (startCoord.y == 0) return true;

        // Iteratively check each part below the current one until reaching the ground
        while (y > 0)
        {
            y--; // Move down one level
            int partId = bpMatrix[x, y, z];
            if (partId == 0) return false; // No support found, chain is interrupted

            GameObject part = FindBuildingPartByID(partId);
            if (part == null || !part.GetComponent<BuildingPart>().isVerticalSupport) return false; // Support chain interrupted

            if (part.GetComponent<BuildingPart>().isTouchingGround) return true; // Support chain reaches the ground
        }

        return false; // In case the loop exits without reaching the ground (shouldn't happen with proper bounds checking)
    }

    public void MarkColumnAsChanged(int x, int z)
    {

        Debug.Log("MarkColumnAsChanged: " + x + " " + z);
        for (int y = 0; y < bpMatrix.GetLength(1); y++) // Assuming y is the vertical dimension
        {
            Debug.Log("Marking: " + x + " " + y + " " + z); 
            int partId = bpMatrix[x + bpOffsetX, y + bpOffsetY, z + bpOffsetZ];
            Debug.Log("PartID: " + partId);
            if (partId != 0) // There is a part at this position
            {
                Debug.Log("Not 0");
                GameObject part = FindBuildingPartByID(partId);
                if (part != null)
                {
                    Debug.Log("Part not null");
                    part.GetComponent<BuildingPart>().changed = true;
                }
            }
        }
    }


    public void SetBuildingGroupID(int newBuildingGroupID)
    {
        buildingGroupID = newBuildingGroupID;
    }

    public void ExpandMatrixAndInsertPart(Vector3 newBuildingPartArrayCoordinate, GameObject newBuildingPart)
    {
        bool verbose = false;

        if (bpMatrix.GetLength(0) == 1 && bpMatrix.GetLength(1) == 1 && bpMatrix.GetLength(2) == 1 && newBuildingPartArrayCoordinate.x == 0 && newBuildingPartArrayCoordinate.y == 0 && newBuildingPartArrayCoordinate.z == 0)
        {
            bpMatrix[0, 0, 0] = newBuildingPart.GetComponent<BuildingPart>().buildingPartID;
        }
        else
        {
            int oldUpperBoundX = bpMatrixUpperBoundX;
            int oldUpperBoundY = bpMatrixUpperBoundY;
            int oldUpperBoundZ = bpMatrixUpperBoundZ;
            int oldLowerBoundX = bpMatrixLowerBoundX;
            int oldLowerBoundY = bpMatrixLowerBoundY;
            int oldLowerBoundZ = bpMatrixLowerBoundZ;

            int copyIntoX = 0;
            int copyIntoY = 0;
            int copyIntoZ = 0;


            if (newBuildingPartArrayCoordinate.x > oldUpperBoundX)
            {
                bpMatrixUpperBoundX = (int)newBuildingPartArrayCoordinate.x;
                copyIntoX = 0;
            }
            if (newBuildingPartArrayCoordinate.x < oldLowerBoundX)
            {
                bpMatrixLowerBoundX = (int)newBuildingPartArrayCoordinate.x;
                copyIntoX = oldLowerBoundX - bpMatrixLowerBoundX;
            }
            if (newBuildingPartArrayCoordinate.y > oldUpperBoundY)
            {
                bpMatrixUpperBoundY = (int)newBuildingPartArrayCoordinate.y;
                copyIntoY = 0;
            }
            if (newBuildingPartArrayCoordinate.y < oldLowerBoundY)
            {
                bpMatrixLowerBoundY = (int)newBuildingPartArrayCoordinate.y;
                copyIntoY = oldLowerBoundY - bpMatrixLowerBoundY;
            }
            if (newBuildingPartArrayCoordinate.z > oldUpperBoundZ)
            {
                bpMatrixUpperBoundZ = (int)newBuildingPartArrayCoordinate.z;
                copyIntoZ = 0;
            }
            if (newBuildingPartArrayCoordinate.z < oldLowerBoundZ)
            {
                bpMatrixLowerBoundZ = (int)newBuildingPartArrayCoordinate.z;
                copyIntoZ = oldLowerBoundZ - bpMatrixLowerBoundZ;
            }

            bpOffsetX = -bpMatrixLowerBoundX;
            bpOffsetY = -bpMatrixLowerBoundY;
            bpOffsetZ = -bpMatrixLowerBoundZ;

            int sizeX = bpMatrixUpperBoundX - bpMatrixLowerBoundX + 1;
            int sizeY = bpMatrixUpperBoundY - bpMatrixLowerBoundY + 1;
            int sizeZ = bpMatrixUpperBoundZ - bpMatrixLowerBoundZ + 1;

            int assigmentX = 0;
            int assigmentY = 0;
            int assigmentZ = 0;

            if (copyIntoX > 0)
            {
                assigmentX = 0;
            }
            else
            {
                assigmentX = bpOffsetX + (int)newBuildingPartArrayCoordinate.x;
            }

            if (copyIntoY > 0)
            {
                assigmentY = 0;

            }
            else
            {
                assigmentY = bpOffsetY + (int)newBuildingPartArrayCoordinate.y;
            }

            if (copyIntoZ > 0)
            {
                assigmentZ = 0;

            }
            else
            {
                assigmentZ = bpOffsetZ + (int)newBuildingPartArrayCoordinate.z;
            }

            if (verbose)
            {
                var cOutput = "";
                cOutput += "==============================================================================================\n";
                cOutput += "newBuildingPartArrayCoordinate: " + newBuildingPartArrayCoordinate + "\n";
                cOutput += "oldUpperBoundX: " + oldUpperBoundX + "\n";
                cOutput += "oldUpperBoundY: " + oldUpperBoundY + "\n";
                cOutput += "oldUpperBoundZ: " + oldUpperBoundZ + "\n";
                cOutput += "oldLowerBoundX: " + oldLowerBoundX + "\n";
                cOutput += "oldLowerBoundY: " + oldLowerBoundY + "\n";
                cOutput += "oldLowerBoundZ: " + oldLowerBoundZ + "\n";
                cOutput += "copyFromX: " + copyIntoX + "\n";
                cOutput += "copyFromY: " + copyIntoY + "\n";
                cOutput += "copyFromZ: " + copyIntoZ + "\n";
                cOutput += "assigmentX: " + assigmentX + "\n";
                cOutput += "assigmentY: " + assigmentY + "\n";
                cOutput += "assigmentZ: " + assigmentZ + "\n";
                cOutput += "bpOffsetX: " + bpOffsetX + "\n";
                cOutput += "bpOffsetY: " + bpOffsetY + "\n";
                cOutput += "bpOffsetZ: " + bpOffsetZ + "\n";
                cOutput += "bpMatrixLowerBoundX: " + bpMatrixLowerBoundX + "\n";
                cOutput += "bpMatrixLowerBoundY: " + bpMatrixLowerBoundY + "\n";
                cOutput += "bpMatrixLowerBoundZ: " + bpMatrixLowerBoundZ + "\n";
                cOutput += "bpMatrixUpperBoundX: " + bpMatrixUpperBoundX + "\n";
                cOutput += "bpMatrixUpperBoundY: " + bpMatrixUpperBoundY + "\n";
                cOutput += "bpMatrixUpperBoundZ: " + bpMatrixUpperBoundZ + "\n";
                cOutput += "sizeX: " + sizeX + "\n";
                cOutput += "sizeY: " + sizeY + "\n";
                cOutput += "sizeZ: " + sizeZ + "\n";
                cOutput += "==============================================================================================\n";

                Debug.Log(cOutput);
            }

            int[,,] newBpMatrix = new int[sizeX, sizeY, sizeZ];
            for (int x = 0; x < bpMatrix.GetLength(0); x++)
            {
                for (int y = 0; y < bpMatrix.GetLength(1); y++)
                {
                    for (int z = 0; z < bpMatrix.GetLength(2); z++)
                    {
                        newBpMatrix[x + copyIntoX, y + copyIntoY, z + copyIntoZ] = bpMatrix[x, y, z];
                    }
                }
            }

            newBpMatrix[assigmentX, assigmentY, assigmentZ] = newBuildingPart.GetComponent<BuildingPart>().buildingPartID;
            bpMatrix = newBpMatrix;

        }
    }

    public (int x, int y, int z) translateCoordinateWithOffset((int x, int y, int z) buildingPartCoordinate)
    {
        return (buildingPartCoordinate.x + bpOffsetX, buildingPartCoordinate.y + bpOffsetY, buildingPartCoordinate.z + bpOffsetZ);
    }

    public void PrintMatrix(string header, int[,,] givenMatrix)
    {
        bool overrideEmptyLayer = true;
        string output = "";
        output += new string('=', givenMatrix.GetLength(0) * 4 + 2) + " \n";
        output += new string('=', givenMatrix.GetLength(0) * 4 + 2) + " \n";
        output += new string('=', givenMatrix.GetLength(0) * 4 + 2) + " \n";
        output += header + "\n";
        output += new string('=', givenMatrix.GetLength(0) * 4 + 2) + " \n";
        output += new string('=', givenMatrix.GetLength(0) * 4 + 2) + " \n";
        output += new string('=', givenMatrix.GetLength(0) * 4 + 2) + " \n";

        for (int y = 0; y < givenMatrix.GetLength(1); y++)
        {
            bool layerEmpty = true;
            string layer = "";

            int width = givenMatrix.GetLength(0);

            layer += new string('=', givenMatrix.GetLength(0) * 4 + 2) + " \n";
            layer += "Y:  " + y.ToString("00") + "\n";
            layer += new string('=', givenMatrix.GetLength(0) * 4 + 2) + " \n";
            layer += "X:  ";
            for (int x = 0; x < givenMatrix.GetLength(0); x++)
            {
                layer += x.ToString("00") + "  ";
            }
            layer += "\n";
            layer += new string('-', givenMatrix.GetLength(0) * 4 + 2) + " \n";

            for (int z = 0; z < givenMatrix.GetLength(2); z++)
            {
                string row = "";
                row += z.ToString("00") + "| ";
                for (int x = 0; x < givenMatrix.GetLength(0); x++)
                {
                    row += givenMatrix[x, y, z].ToString("00") + "  ";
                    if (givenMatrix[x, y, z] != 0)
                    {
                        layerEmpty = false;
                    }
                }
                row += "\n";
                layer += row;
            }
            if (!layerEmpty || overrideEmptyLayer)
            {
                output += layer + "\n\n\n";
            }
        }
        Debug.Log(output);
    }
}