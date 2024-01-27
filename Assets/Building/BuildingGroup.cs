
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildingGroup : MonoBehaviour
{
    public int buildingGroupID;
    //public int initialBuildingGroupDiameter = 20;
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



    public void AddBuildingPart(GameObject newBuildingPart, Vector3 newBuildingPartCoordinate)
    {
        GameObject buildingPart = Instantiate(newBuildingPart, newBuildingPartCoordinate, Quaternion.identity);
        buildingPart.transform.parent = transform;
        buildingPart.transform.name = "BP-" + transform.childCount;
        buildingPart.GetComponent<BuildingPart>().SetBuildingPartID(transform.childCount);
        buildingPart.layer = LayerMask.NameToLayer("BuildingPart");
        Vector3 localPosition = buildingPart.transform.localPosition;

        // PrintMatrix("Before CheckExpandMatrixAndInsertPart", bpMatrix);

        ExpandMatrixAndInsertPart(localPosition, buildingPart);

         PrintMatrix("After CheckExpandMatrixAndInsertPart", bpMatrix);
    }

    public Vector3 TranslateBuildingPartCoordinateToBuildingPartArrayCoordinate(Vector3 buildingPartCoordinate)
    {
        Vector3 buildingPartArrayCoordinate = buildingPartCoordinate + offset;
        return buildingPartArrayCoordinate;
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





    void Awake()
    {
        bpMatrix = new int[1, 1, 1];
    }
}