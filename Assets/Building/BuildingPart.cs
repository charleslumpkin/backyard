using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pathfinding;
using Unity.VisualScripting;
using Unity.Mathematics;
using UnityEditor;
using System.Linq;
using Unity.PlasticSCM.Editor.WebApi;



public class BuildingPart : MonoBehaviour
{
    public int ID = 0;
    public int shortestPathToGround = int.MaxValue;
    public BuildingGroup bg;
    public bool changed = true;
    public bool isGrounded = false;
    public bool isVerticalSupport = false;
    public bool isOverloaded = false;
    //public bool supportCalculationDone = false;

    public BuildingPart supportedBy;
    public Neighbors neighbors;


    public Vector3Int localPosition = new Vector3Int(0, 0, 0);
    public float mass = 10f;
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float totalSupportedMass = 0f;
    public float maxSupporatbleMass = 80f;
    public bool directlyDestroyed = false;
    public bool alreadyDestroyed = false;

    public bool enableGizmos = true;

    void Update()
    {

        if (ID != 0)
        {
            if (changed && !isOverloaded)
            {
                isGrounded = IsTouchingGround();
                neighbors = GetNeighbors();
                PathResult pr = FindShortestPath(this, GoalType.Grounded);
                if (pr != null)
                {
                    SetSuportedBy(pr);
                    ResetTotalSupportedMass();
                    isVerticalSupport = IsVerticalSupport();
                    MarkNeighborsChangedIfShorter();
                    bg.currentPathResult = pr;
                }
                else
                {
                    isOverloaded = true;
                    MarkNeighborsChanged();
                }

                changed = false;
            }

            if (isOverloaded && !alreadyDestroyed)
            {
                DestroyBuildingPart();
                alreadyDestroyed = true;

            }
        }
    }

    public void ResetTotalSupportedMass()
    {
        totalSupportedMass = mass;
    }


    public void SetSuportedBy(PathResult pr)
    {
        if (pr != null)
        {
            shortestPathToGround = pr.PathLength;
        }
        else
        {
            shortestPathToGround = int.MaxValue;
        }
        if (pr.Path != null)
        {
            if (pr.Path.Count > 1)
            {
                supportedBy = pr.Path[1];
            }
        }
        else
        {
            supportedBy = null;
        }
    }


    public bool IsOverloaded()
    {
        if (totalSupportedMass > maxSupporatbleMass)
        {
            if (isVerticalSupport)
            {
                // Check if this vertical block supports any non-vertical blocks
                var supportsNonVertical = bg.buildingPartById.Values.Any(bp => bp.supportedBy == this && !bp.isVerticalSupport);
                return supportsNonVertical;
            }
            return true;
        }

        return false;
    }

    public void DestroyBuildingPart()
    {
        bg.RemoveBuildingPartFromMatrix(ID);
        bg.massPropagated = false; 
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().useGravity = true;
        MarkNeighborsChanged();

        
        if (directlyDestroyed)
        {
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject, 10f);
        }

    }




    public PathResult FindShortestPath(BuildingPart startNode, GoalType goalType, BuildingPart endNode = null)
    {
        List<UCSNode> frontier = new List<UCSNode>();
        HashSet<BuildingPart> explored = new HashSet<BuildingPart>();

        frontier.Add(new UCSNode(startNode, 0, null));

        while (frontier.Count > 0)
        {
            frontier.Sort((a, b) => a.Cost.CompareTo(b.Cost));
            UCSNode currentNode = frontier[0];
            frontier.RemoveAt(0);

            if (explored.Contains(currentNode.Part))
                continue;

            bool goalReached = false;
            switch (goalType)
            {
                case GoalType.Grounded:
                    if (currentNode.Part.isGrounded)
                        goalReached = true;
                    break;
                case GoalType.VerticalSupport:
                    if (currentNode.Part.isVerticalSupport)
                        goalReached = true;
                    break;
                case GoalType.SpecificNode:
                    if (currentNode.Part == endNode)
                        goalReached = true;
                    break;
            }

            if (goalReached)
            {
                List<BuildingPart> path = new List<BuildingPart>();
                UCSNode pathNode = currentNode;
                while (pathNode != null)
                {
                    path.Add(pathNode.Part);
                    pathNode = pathNode.Parent;
                }
                path.Reverse(); // If you want the path from start to goal

                return new PathResult
                {
                    StartNode = startNode,
                    EndNode = currentNode.Part,
                    PathLength = currentNode.Cost,
                    Path = path
                };
            }

            explored.Add(currentNode.Part);

            foreach (var neighbor in currentNode.Part.GetNeighborList())
            {
                if (!explored.Contains(neighbor))
                {
                    int newCost = currentNode.Cost + 1;
                    UCSNode newNode = new UCSNode(neighbor, newCost, currentNode);
                    frontier.Add(newNode);
                }
            }
        }

        return null; // No path found
    }


    public void MarkNeighborsChangedIfShorter()
    {
        List<BuildingPart> neighbors = GetNeighborList();
        foreach (var neighbor in neighbors)
        {
            if (shortestPathToGround + 1 < neighbor.shortestPathToGround)
            {
                neighbor.changed = true;
            }
            if(shortestPathToGround - 1 > neighbor.shortestPathToGround)
            {
                neighbor.changed = true;
            }
            if(shortestPathToGround == int.MaxValue)
            {
                neighbor.changed = true;
            }
            if(shortestPathToGround == neighbor.shortestPathToGround)
            {
                neighbor.changed = true;
            }
        }


    }

    public void MarkNeighborsChanged()
    {
        List<BuildingPart> neighbors = GetNeighborList();
        foreach (var neighbor in neighbors)
        {
            neighbor.changed = true;
        }
    }

    // Assuming a method that returns a list of neighboring BuildingPart objects
    private List<BuildingPart> GetNeighborList()
    {
        //Grab Neighbors from GetNeighbors Method  and return them as a list
        Neighbors neighbors = GetNeighbors();
        List<BuildingPart> neighborList = new List<BuildingPart>();
        if (neighbors.up != null)
        {
            neighborList.Add(neighbors.up);
        }
        if (neighbors.down != null)
        {
            neighborList.Add(neighbors.down);
        }
        if (neighbors.left != null)
        {
            neighborList.Add(neighbors.left);
        }
        if (neighbors.right != null)
        {
            neighborList.Add(neighbors.right);
        }
        if (neighbors.forward != null)
        {
            neighborList.Add(neighbors.forward);
        }
        if (neighbors.backward != null)
        {
            neighborList.Add(neighbors.backward);
        }
        return neighborList;

    }


    public Neighbors GetNeighbors()
    {
        return bg.GetNeighbors(this);
    }

    public bool IsVerticalSupport()
    {
        bool returnBool = false;
        if (isGrounded)
        {
            returnBool = true;
        }
        else if (neighbors.down != null && neighbors.down.isVerticalSupport)
        {
            returnBool = true;
        }


        return returnBool;
    }

    public bool IsTouchingGround()
    {
        bool returnBool = false;
        Vector3 newScale = transform.parent.localScale;

        Vector3 center = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Collider[] colliders = Physics.OverlapBox(center, newScale / 2, Quaternion.identity, LayerMask.GetMask("Terrain"));

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


    void Awake()
    {
        currentHealth = maxHealth;
        neighbors = new Neighbors();
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position; // Assuming this is the center of the BuildingPart

        // Prepare the text to be displayed
        string displayText = $"sp: {shortestPathToGround}\nMass: {totalSupportedMass:F2}"; // F2 for displaying the number with 2 decimal places

        // Configure the style of the text
        GUIStyle style = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            normal = { textColor = Color.white }
        };

        // Determine the label position, adjust Y offset as needed to position the label above the part
        Vector3 labelPosition = center;

        // Draw the label using UnityEditor.Handles
        UnityEditor.Handles.Label(labelPosition, displayText, style);
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


    public void recalculateAIGraph()
    {
        Bounds bounds = GetComponent<BoxCollider>().bounds;
        //translate to world position
        bounds.center = transform.TransformPoint(bounds.center);
        var guo = new GraphUpdateObject(bounds);

        // Set some settings
        guo.updatePhysics = true;
        AstarPath.active.UpdateGraphs(guo);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            directlyDestroyed = true;
            DestroyBuildingPart();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Zombie")
        {
            other.gameObject.GetComponent<AIPath>().canMove = false;
            other.gameObject.GetComponent<AIDestinationSetter>().target = transform;
            other.gameObject.GetComponent<AIPath>().canMove = true;
        }
    }

}