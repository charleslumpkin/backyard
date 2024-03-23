using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System.Collections;
using System.Linq;


[System.Serializable]
public class BuildingPartReference
{
    public int ID;
    public BuildingPart Part;

    public BuildingPartReference(int id, BuildingPart part)
    {
        ID = id;
        Part = part;
    }
}

[System.Serializable]
public class Neighbors
{
    public BuildingPart up;
    public BuildingPart down;
    public BuildingPart left;
    public BuildingPart right;
    public BuildingPart forward;
    public BuildingPart backward;
}

public class PathResult
{
    public BuildingPart StartNode { get; set; }
    public BuildingPart EndNode { get; set; }
    public int PathLength { get; set; }
    public List<BuildingPart> Path { get; set; }
}

public enum GoalType
{
    Grounded,
    VerticalSupport,
    SpecificNode
}


// Custom node class for UCS
public class UCSNode
{
    public BuildingPart Part;
    public int Cost;
    public UCSNode Parent;

    public UCSNode(BuildingPart part, int cost, UCSNode parent)
    {
        Part = part;
        Cost = cost;
        Parent = parent;
    }
}

public class BuildingGroup : MonoBehaviour
{
    public int buildingGroupID;
    public int[,,] bpMatrix;
    public Vector3Int offset;
    public Vector3Int bpUpperBound;
    public Vector3Int bpLowerBound;
    public bool massPropagated = false;
    public PathResult currentPathResult;


    public Dictionary<int, BuildingPart> buildingPartById = new Dictionary<int, BuildingPart>();







    void Update()
    {
        if (AllSupportCalculationsFinished() && !massPropagated)
        {
            Debug.Log("All support calculations finished. Propagating mass...");
            PropagateMassFromGroundNodes();
            massPropagated = true;
            checkOverloaded();
        }
    }

    private bool AllSupportCalculationsFinished()
    {
        return buildingPartById.Values.All(bp => !bp.changed);
    }

    private void PropagateMassFromGroundNodes()
    {
        var groundNodes = buildingPartById.Values.Where(bp => bp.isGrounded).ToList();
        foreach (var groundNode in groundNodes)
        {
            Debug.Log($"Propagating mass from ground node {groundNode.ID}");
            DFSPropagateMass(groundNode);
        }
    }

    private void DFSPropagateMass(BuildingPart node)
    {

        
        if (node == null) return;

        // Start from the leaf nodes
        var children = buildingPartById.Values.Where(bp => bp.supportedBy == node).ToList();
        foreach (var child in children)
        {
            DFSPropagateMass(child);
        }

        // Once all children have calculated their total mass, sum it up for the current node
        node.totalSupportedMass = node.mass + children.Sum(child => child.totalSupportedMass);
    }

    public void checkOverloaded()
    {
        foreach (var bp in buildingPartById.Values)
        {
            // Check if the building part is overloaded and is a vertical support
            if (bp.isVerticalSupport && bp.IsOverloaded())
            {
                SetOverloadedFlag(bp);
            }
        }
    }

    private void SetOverloadedFlag(BuildingPart startNode)
    {
        // Use a DFS approach to set the overloaded flag for all supported parts
        var stack = new Stack<BuildingPart>();
        stack.Push(startNode);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            // Set the overloaded flag for the current part
            current.isOverloaded = true;

            // Get all parts that are directly supported by the current part
            var supportedParts = buildingPartById.Values.Where(bp => bp.supportedBy == current && !bp.isVerticalSupport).ToList();

            foreach (var part in supportedParts)
            {
                stack.Push(part);
            }
        }
    }




    public void RegisterBuildingPart(int id, BuildingPart bp)
    {

        if (!buildingPartById.ContainsKey(id))
        {
            buildingPartById.Add(id, bp);
        }
        else
        {
            Debug.LogWarning($"BuildingPart with ID {id} already registered!");
        }

    }

    public BuildingPart GetBuildingPartById(int id)
    {
        if (buildingPartById.TryGetValue(id, out BuildingPart bp))
        {
            return bp;
        }
        else
        {
            if (id != 0) Debug.LogWarning($"BuildingPart with ID {id} not found.");
            return null;
        }
    }

    public void DeregisterBuildingPart(int id)
    {
        if (buildingPartById.ContainsKey(id))
        {
            buildingPartById.Remove(id);
        }
        else
        {
            Debug.LogWarning($"Trying to deregister BuildingPart with ID {id}, but it's not registered.");
        }
    }

    public Neighbors GetNeighbors(BuildingPart bp)
    {
        Neighbors neighbors = new Neighbors();
        Vector3Int bpPos = bp.localPosition;

        Vector3Int up = bp.localPosition + offset + Vector3Int.up;
        Vector3Int down = bp.localPosition + offset + Vector3Int.down;
        Vector3Int left = bp.localPosition + offset + Vector3Int.left;
        Vector3Int right = bp.localPosition + offset + Vector3Int.right;
        Vector3Int forward = bp.localPosition + offset + Vector3Int.forward;
        Vector3Int backward = bp.localPosition + offset + Vector3Int.back;

        if (IsWithinBounds(up))
        {
            neighbors.up = GetBuildingPartById(bpMatrix[up.x, up.y, up.z]);
        }
        if (IsWithinBounds(down))
        {
            neighbors.down = GetBuildingPartById(bpMatrix[down.x, down.y, down.z]);
        }
        if (IsWithinBounds(left))
        {
            neighbors.left = GetBuildingPartById(bpMatrix[left.x, left.y, left.z]);
        }
        if (IsWithinBounds(right))
        {
            neighbors.right = GetBuildingPartById(bpMatrix[right.x, right.y, right.z]);
        }
        if (IsWithinBounds(forward))
        {
            neighbors.forward = GetBuildingPartById(bpMatrix[forward.x, forward.y, forward.z]);
        }
        if (IsWithinBounds(backward))
        {
            neighbors.backward = GetBuildingPartById(bpMatrix[backward.x, backward.y, backward.z]);
        }

        return neighbors;
    }

    public void RemoveBuildingPartFromMatrix(int id)
    {
        BuildingPart bp = GetBuildingPartById(id);
        Vector3Int bpPos = bp.localPosition + offset;

        bpMatrix[bpPos.x, bpPos.y, bpPos.z] = 0;
        DeregisterBuildingPart(id);
    }

    public void AddBuildingPartToMatrix(int id, Vector3Int bpPos)
    {
        Vector3Int bpOffset = bpPos + offset;
        bpMatrix[bpOffset.x, bpOffset.y, bpOffset.z] = id;
        RegisterBuildingPart(id, GetBuildingPartById(id));
    }


    void Awake()
    {
        bpMatrix = new int[1, 1, 1];
    }

    public void RemoveBuildingPartFromMatrix(Vector3Int bpCoord)
    {
        if (bpCoord.x >= 0 && bpCoord.x < bpMatrix.GetLength(0) && bpCoord.y >= 0 && bpCoord.y < bpMatrix.GetLength(1) && bpCoord.z >= 0 && bpCoord.z < bpMatrix.GetLength(2))
            bpMatrix[bpCoord.x, bpCoord.y, bpCoord.z] = 0;
    }

    public bool IsWithinBounds(Vector3Int position)
    {
        return position.x >= 0 && position.x < bpMatrix.GetLength(0) &&
               position.y >= 0 && position.y < bpMatrix.GetLength(1) &&
               position.z >= 0 && position.z < bpMatrix.GetLength(2);
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
        BuildingPart bp = buildingPart.GetComponent<BuildingPart>();
        bp.ID = currentID;
        bp.bg = this;
        bp.localPosition = new Vector3Int((int)buildingPart.transform.localPosition.x, (int)buildingPart.transform.localPosition.y, (int)buildingPart.transform.localPosition.z);

        Debug.Log("============ AddBuildingPart: " + bp.ID + " ============\n");
        buildingPart.GetComponent<Renderer>().material.color = Color.white;
        buildingPart.GetComponent<BoxCollider>().enabled = true;

        RegisterBuildingPart(currentID, bp);

        ExpandMatrixAndInsertPart(bp.localPosition, buildingPart);
        AstarPath.active.Scan();
        massPropagated = false;
    }


    public void RecalculateAIGraph()
    {
        for (int x = 0; x < bpMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < bpMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < bpMatrix.GetLength(2); z++)
                {
                    if (bpMatrix[x, y, z] != 0)
                    {
                        //GameObject part = FindBuildingPart((x, y, z));
                        //part.GetComponent<BuildingPart>().recalculateAIGraph();
                    }
                }
            }
        }
    }


    public void SetBuildingGroupID(int newBuildingGroupID)
    {
        buildingGroupID = newBuildingGroupID;
    }

    public void ExpandMatrixAndInsertPart(Vector3Int newBuildingPartArrayCoordinate, GameObject newBuildingPart)
    {
        bool verbose = false;

        if (bpMatrix.GetLength(0) == 1 && bpMatrix.GetLength(1) == 1 && bpMatrix.GetLength(2) == 1 && newBuildingPartArrayCoordinate.x == 0 && newBuildingPartArrayCoordinate.y == 0 && newBuildingPartArrayCoordinate.z == 0)
        {
            bpMatrix[0, 0, 0] = newBuildingPart.GetComponent<BuildingPart>().ID;
        }
        else
        {
            int oldUpperBoundX = bpUpperBound.x;
            int oldUpperBoundY = bpUpperBound.y;
            int oldUpperBoundZ = bpUpperBound.z;
            int oldLowerBoundX = bpLowerBound.x;
            int oldLowerBoundY = bpLowerBound.y;
            int oldLowerBoundZ = bpLowerBound.z;

            int copyIntoX = 0;
            int copyIntoY = 0;
            int copyIntoZ = 0;


            if (newBuildingPartArrayCoordinate.x > oldUpperBoundX)
            {
                bpUpperBound.x = newBuildingPartArrayCoordinate.x;
                copyIntoX = 0;
            }
            if (newBuildingPartArrayCoordinate.x < oldLowerBoundX)
            {
                bpLowerBound.x = newBuildingPartArrayCoordinate.x;
                copyIntoX = oldLowerBoundX - bpLowerBound.x;
            }
            if (newBuildingPartArrayCoordinate.y > oldUpperBoundY)
            {
                bpUpperBound.y = newBuildingPartArrayCoordinate.y;
                copyIntoY = 0;
            }
            if (newBuildingPartArrayCoordinate.y < oldLowerBoundY)
            {
                bpLowerBound.y = newBuildingPartArrayCoordinate.y;
                copyIntoY = oldLowerBoundY - bpLowerBound.y;
            }
            if (newBuildingPartArrayCoordinate.z > oldUpperBoundZ)
            {
                bpUpperBound.z = newBuildingPartArrayCoordinate.z;
                copyIntoZ = 0;
            }
            if (newBuildingPartArrayCoordinate.z < oldLowerBoundZ)
            {
                bpLowerBound.z = newBuildingPartArrayCoordinate.z;
                copyIntoZ = oldLowerBoundZ - bpLowerBound.z;
            }

            offset = new Vector3Int(-bpLowerBound.x, -bpLowerBound.y, -bpLowerBound.z);


            int sizeX = bpUpperBound.x - bpLowerBound.x + 1;
            int sizeY = bpUpperBound.y - bpLowerBound.y + 1;
            int sizeZ = bpUpperBound.z - bpLowerBound.z + 1;

            int assigmentX = 0;
            int assigmentY = 0;
            int assigmentZ = 0;

            if (copyIntoX > 0)
            {
                assigmentX = 0;
            }
            else
            {
                assigmentX = offset.x + newBuildingPartArrayCoordinate.x;
            }

            if (copyIntoY > 0)
            {
                assigmentY = 0;

            }
            else
            {
                assigmentY = offset.y + newBuildingPartArrayCoordinate.y;
            }

            if (copyIntoZ > 0)
            {
                assigmentZ = 0;

            }
            else
            {
                assigmentZ = offset.z + newBuildingPartArrayCoordinate.z;
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
                cOutput += "offset: " + offset + "\n";
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

            newBpMatrix[assigmentX, assigmentY, assigmentZ] = newBuildingPart.GetComponent<BuildingPart>().ID;
            bpMatrix = newBpMatrix;

        }
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