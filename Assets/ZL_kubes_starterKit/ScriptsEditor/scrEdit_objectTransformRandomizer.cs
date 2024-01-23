using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class scrEdit_objectTransformRandomizer : ScriptableWizard
{
    Vector2 scrollPosition  = new Vector2(0,0);

    //position/movement ---------------------------------------
    bool rnd_move_x = true;
    bool rnd_move_y = true;
    bool rnd_move_z = true;

    public float x_minDist = -1.0f;
    public float x_maxDist = 1.0f;

    public float y_minDist = -1.0f;
    public float y_maxDist = 1.0f;

    public float z_minDist = -1.0f;
    public float z_maxDist = 1.0f;

    //position steps ---------------------------------------
    bool rnd_moveSteps_x = true;
    bool rnd_moveSteps_y = true;
    bool rnd_moveSteps_z = true;

    public float x_posStep = 1.0f;
    public int x_posStep_min = -2;
    public int x_posStep_max = 2;

    public float y_posStep = 1.0f;
    public int y_posStep_min = -2;
    public int y_posStep_max = 2;

    public float z_posStep = 1.0f;
    public int z_posStep_min = -2;
    public int z_posStep_max = 2;

    //rotation degrees ---------------------------------------
    bool rnd_rot_x = true;
    bool rnd_rot_y = true;
    bool rnd_rot_z = true;

    public float x_minRot = -360.0f;
    public float x_maxRot = 360.0f;

    public float y_minRot = -360.0f;
    public float y_maxRot = 360.0f;

    public float z_minRot = -360.0f;
    public float z_maxRot = 360.0f;

    //rotation steps ---------------------------------------
    bool rnd_rotStep_x = true;
    bool rnd_rotStep_y = true;
    bool rnd_rotStep_z = true;

    public float x_rotStep = 90.0f;
    public int x_rotStep_min = 0;
    public int x_rotStep_max = 4;

    public float y_rotStep = 90.0f;
    public int y_rotStep_min = 0;
    public int y_rotStep_max = 4;

    public float z_rotStep = 90.0f;
    public int z_rotStep_min = 0;
    public int z_rotStep_max = 4;

    //scale ---------------------------------------
    bool rnd_scale_x = true;
    bool rnd_scale_y = true;
    bool rnd_scale_z = true;

    bool rnd_scale_unif = false;

    public float x_minScale = 1.0f;
    public float x_maxScale = 2.0f;
    public float y_minScale = 1.0f;
    public float y_maxScale = 2.0f;
    public float z_minScale = 1.0f;
    public float z_maxScale = 2.0f;

    

    //rotation steps ---------------------------------------
    bool rnd_scaleStep_x = true;
    bool rnd_scaleStep_y = true;
    bool rnd_scaleStep_z = true;

    bool rnd_scaleStep_unif = false;

    public float x_scaleStep = 0.25f;
    public int x_scaleStep_min = 4;
    public int x_scaleStep_max = 8;

    public float y_scaleStep = 0.25f;
    public int y_scaleStep_min = 4;
    public int y_scaleStep_max = 8;

    public float z_scaleStep = 0.25f;
    public int z_scaleStep_min = 4;
    public int z_scaleStep_max = 8;

    // add menu item
    [MenuItem("ZerinLabs/Object transform randomizer")]

    //-----------------------------------------------------------------------------------------------------------------
    static void Init()
    {
        scrEdit_objectTransformRandomizer window = (scrEdit_objectTransformRandomizer)EditorWindow.GetWindow(typeof(scrEdit_objectTransformRandomizer));
        window.Show();
    }

    //-----------------------------------------------------------------------------------------------------------------
    void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        GUILayout.Label("\n P O S I T I O N    R A N D O M I Z E\n", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);//------

        GUILayout.Label("Moving by 'UNITS'", EditorStyles.boldLabel); //-----------------------------------------------------------------------------

        GUILayout.Space(10);

        rnd_move_x = GUILayout.Toggle(rnd_move_x, "Move on X axis");
        x_minDist = EditorGUILayout.FloatField("  - Min:", x_minDist);
        x_maxDist = EditorGUILayout.FloatField("  - Max:", x_maxDist);

        GUILayout.Space(5);

        rnd_move_y = GUILayout.Toggle(rnd_move_y, "Move on Y");
        y_minDist = EditorGUILayout.FloatField("  - Min:", y_minDist);
        y_maxDist = EditorGUILayout.FloatField("  - Max:", y_maxDist);

        GUILayout.Space(5);

        rnd_move_z = GUILayout.Toggle(rnd_move_z, "Move on Z");
        z_minDist = EditorGUILayout.FloatField("  - Min:", z_minDist);
        z_maxDist = EditorGUILayout.FloatField("  - Max:", z_maxDist);
               
        GUILayout.Space(10);

        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Rand. POSITION (by UNITS)", GUILayout.Width(256), GUILayout.Height(32)))
        {
            randomMoveUnits();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.Label("Moving by 'STEPS'", EditorStyles.boldLabel); //-----------------------------------------------------------------------------

        GUILayout.Space(10);

        rnd_moveSteps_x = GUILayout.Toggle(rnd_moveSteps_x, "Move on X axis");
        x_posStep = EditorGUILayout.FloatField("  - Step dist. (units):", x_posStep);
        x_posStep_min = EditorGUILayout.IntField("      - Min (int):", x_posStep_min);
        x_maxDist = EditorGUILayout.IntField("      - Max (int):", x_posStep_max);

        GUILayout.Space(5);

        rnd_moveSteps_y = GUILayout.Toggle(rnd_moveSteps_y, "Move on Y axis");
        y_posStep = EditorGUILayout.FloatField("  - Step dist. (units):", y_posStep);
        y_posStep_min = EditorGUILayout.IntField("      - Min (int):", y_posStep_min);
        y_maxDist = EditorGUILayout.IntField("      - Max (int):", y_posStep_max);

        GUILayout.Space(5);

        rnd_moveSteps_z = GUILayout.Toggle(rnd_moveSteps_z, "Move on Z axis");
        z_posStep = EditorGUILayout.FloatField("  - Step dist. (units):", z_posStep);
        z_posStep_min = EditorGUILayout.IntField("      - Min (int):", z_posStep_min);
        z_maxDist = EditorGUILayout.IntField("      - Max (int):", z_posStep_max);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Rand. POSITION (by STEPS)", GUILayout.Width(256), GUILayout.Height(32)))
        {
            randomMoveSteps();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        GUILayout.Label("\nR O T A T I O N   R A N D O M I Z E\n", EditorStyles.boldLabel);  //-----------------------------------------------------------------------------
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.Label("Rotating by 'DEGREES'", EditorStyles.boldLabel);  //-----------------------------------------------------------------------------

        GUILayout.Space(10);

        rnd_rot_x = GUILayout.Toggle(rnd_rot_x, "Rotation on X");
        x_minRot = EditorGUILayout.FloatField("  - Min angle:", x_minRot);
        x_maxRot = EditorGUILayout.FloatField("  - Max angle:", x_maxRot);

        GUILayout.Space(5);

        rnd_rot_y = GUILayout.Toggle(rnd_rot_y, "Rotation on Y");
        y_minRot = EditorGUILayout.FloatField("  - Min angle:", y_minRot);
        y_maxRot = EditorGUILayout.FloatField("  - Max angle:", y_maxRot);

        GUILayout.Space(5);

        rnd_rot_z = GUILayout.Toggle(rnd_rot_z, "Rotation on Z");
        z_minRot = EditorGUILayout.FloatField("  - Min angle:", z_minRot);
        z_maxRot = EditorGUILayout.FloatField("  - Max angle:", z_maxRot);

        GUILayout.Space(10);
                
        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Rand. ROTATION (by ANGLE)", GUILayout.Width(256), GUILayout.Height(32)))
        {
            randomRotationAngle();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.Label("Rotating by 'STEPS'", EditorStyles.boldLabel); //-----------------------------------------------------------------------------

        GUILayout.Space(10);

        rnd_rotStep_x = GUILayout.Toggle(rnd_rotStep_x, "Rotate on X axis");
        x_rotStep = EditorGUILayout.FloatField("  - Step rotation (angle):", x_rotStep);
        x_rotStep_min = EditorGUILayout.IntField("      - Min (int):", x_rotStep_min);
        x_rotStep_max = EditorGUILayout.IntField("      - Max (int):", x_rotStep_max);

        GUILayout.Space(5);

        rnd_rotStep_y = GUILayout.Toggle(rnd_rotStep_y, "Rotate on Y axis");
        y_rotStep = EditorGUILayout.FloatField("  - Step rotation (angle):", y_rotStep);
        y_rotStep_min = EditorGUILayout.IntField("      - Min (int):", y_rotStep_min);
        y_rotStep_max = EditorGUILayout.IntField("      - Max (int):", y_rotStep_max);

        GUILayout.Space(5);

        rnd_rotStep_z = GUILayout.Toggle(rnd_rotStep_z, "Rotate on Z axis");
        z_rotStep = EditorGUILayout.FloatField("  - Step rotation (angle):", z_rotStep);
        z_rotStep_min = EditorGUILayout.IntField("      - Min (int):", z_rotStep_min);
        z_rotStep_max = EditorGUILayout.IntField("      - Max (int):", z_rotStep_max);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Rand. ROTATION (by STEPS)", GUILayout.Width(256), GUILayout.Height(32)))
        {
            randomRotationSteps();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        GUILayout.Label("\nS C A L E   R A N D O M I Z E\n", EditorStyles.boldLabel);  //-----------------------------------------------------------------------------
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.Label("Scale by 'UNITS'", EditorStyles.boldLabel);  //-----------------------------------------------------------------------------

        GUILayout.Space(10);

        rnd_scale_unif = GUILayout.Toggle(rnd_scale_unif, "Uniform scale (X params will be used)");

        GUILayout.Space(10);

        rnd_scale_x = GUILayout.Toggle(rnd_scale_x, "Scale on X");
        x_minScale = EditorGUILayout.FloatField("  - Min angle:", x_minScale);
        x_maxScale = EditorGUILayout.FloatField("  - Max angle:", x_maxScale);

        GUILayout.Space(5);

        rnd_scale_y = GUILayout.Toggle(rnd_scale_y, "Scale on Y");
        y_minScale = EditorGUILayout.FloatField("  - Min angle:", y_minScale);
        y_maxScale = EditorGUILayout.FloatField("  - Max angle:", y_maxScale);

        GUILayout.Space(5);

        rnd_scale_z = GUILayout.Toggle(rnd_scale_z, "Scale on Z");
        z_minScale = EditorGUILayout.FloatField("  - Min angle:", z_minScale);
        z_maxScale = EditorGUILayout.FloatField("  - Max angle:", z_maxScale);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Rand. SCALE (by UNITS)", GUILayout.Width(256), GUILayout.Height(32)))
        {
            randomScaleUnits();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.Label("Scaling by 'STEPS'", EditorStyles.boldLabel); //-----------------------------------------------------------------------------

        GUILayout.Space(10);

        rnd_scaleStep_unif = GUILayout.Toggle(rnd_scaleStep_unif, "Uniform scale (X params will be used)");

        GUILayout.Space(10);

        rnd_scaleStep_x = GUILayout.Toggle(rnd_scaleStep_x, "Scale on X axis");
        x_scaleStep = EditorGUILayout.FloatField("  - Step rotation (angle):", x_scaleStep);
        x_scaleStep_min = EditorGUILayout.IntField("      - Min (int):", x_scaleStep_min);
        x_scaleStep_max = EditorGUILayout.IntField("      - Max (int):", x_scaleStep_max);

        GUILayout.Space(5);

        rnd_scaleStep_y = GUILayout.Toggle(rnd_scaleStep_y, "Scale on Y axis");
        y_scaleStep = EditorGUILayout.FloatField("  - Step rotation (angle):", y_scaleStep);
        y_scaleStep_min = EditorGUILayout.IntField("      - Min (int):", y_scaleStep_min);
        y_scaleStep_max = EditorGUILayout.IntField("      - Max (int):", y_scaleStep_max);

        GUILayout.Space(5);

        rnd_scaleStep_z = GUILayout.Toggle(rnd_scaleStep_z, "Scale on Z axis");
        z_scaleStep = EditorGUILayout.FloatField("  - Step rotation (angle):", z_scaleStep);
        z_scaleStep_min = EditorGUILayout.IntField("      - Min (int):", z_scaleStep_min);
        z_scaleStep_max = EditorGUILayout.IntField("      - Max (int):", z_scaleStep_max);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Rand. SCALE (by STEPS)", GUILayout.Width(256), GUILayout.Height(32)))
        {
            randomScaleSteps();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
    }

    //-----------------------------------------------------------------------------------------------------------------
    void randomMoveUnits()
    {
        for (int i = 0; i < Selection.transforms.Length; i++)
        {
            GameObject go = Selection.transforms[i].gameObject;

            float x = 0f;
            float y = 0f;
            float z = 0f;

            if (rnd_move_x == true)
            {
                x = Random.Range(x_minDist, x_maxDist);
            }

            if (rnd_move_y == true)
            {
                y = Random.Range(y_minDist, y_maxDist);
            }

            if (rnd_move_z == true)
            {
                z = Random.Range(z_minDist, z_maxDist);
            }

            Undo.RegisterCompleteObjectUndo(go, "Changed random position (units)");

            Vector3 shift = new Vector3(x, y, z);

            go.transform.position = go.transform.position + shift;

        }
    }

    //-----------------------------------------------------------------------------------------------------------------
    void randomMoveSteps()
    {
        for (int i = 0; i < Selection.transforms.Length; i++)
        {
            GameObject go = Selection.transforms[i].gameObject;

            float x = 0f;
            float y = 0f;
            float z = 0f;

            if (rnd_moveSteps_x == true)
            {
                x = x_posStep * Random.Range(x_posStep_min, x_posStep_max);
            }

            if (rnd_moveSteps_y == true)
            {
                y = y_posStep * Random.Range(y_posStep_min, y_posStep_max);
            }

            if (rnd_moveSteps_z == true)
            {
                z = z_posStep * Random.Range(z_posStep_min, z_posStep_max);
            }

            Undo.RegisterCompleteObjectUndo(go, "Changed random position (steps)");

            Vector3 shift = new Vector3(x, y, z);

            go.transform.position = go.transform.position + shift;

        }
    }

    //-----------------------------------------------------------------------------------------------------------------
    void randomRotationAngle()
    {
        for (int i = 0; i < Selection.transforms.Length; i++)
        {
            GameObject go = Selection.transforms[i].gameObject;

            float rotAmount = 0f;

            Undo.RegisterCompleteObjectUndo(go, "Changed random rotation (degrees)");

            if (rnd_rot_x == true)
            {
                rotAmount = Random.Range(x_minRot, x_maxRot);
                go.transform.Rotate(rotAmount, 0, 0);
            }
            
            if (rnd_rot_y == true)
            {
                rotAmount = Random.Range(y_minRot, y_maxRot);
                go.transform.Rotate(0, rotAmount, 0);
            }

            if (rnd_rot_z == true)
            {
                rotAmount = Random.Range(z_minRot, z_maxRot);
                go.transform.Rotate(0, 0, rotAmount);
            }

            Undo.RegisterCompleteObjectUndo(go, "Changed random rotation (degrees)");
        }
    }

    //-----------------------------------------------------------------------------------------------------------------
    void randomRotationSteps()
    {
        for (int i = 0; i < Selection.transforms.Length; i++)
        {
            GameObject go = Selection.transforms[i].gameObject;

            float rotAmount = 0f;

            if (rnd_rotStep_x == true)
            {
                rotAmount = x_rotStep * Random.Range(x_rotStep_min, x_rotStep_max);
                go.transform.Rotate(rotAmount, 0, 0);
            }

            if (rnd_rotStep_y == true)
            {
                rotAmount = y_rotStep * Random.Range(y_rotStep_min, y_rotStep_max);
                go.transform.Rotate(0, rotAmount, 0);
            }

            if (rnd_rotStep_z == true)
            {
                rotAmount = z_rotStep * Random.Range(z_rotStep_min, z_rotStep_max);
                go.transform.Rotate(0, 0, rotAmount);
            }

            Undo.RegisterCompleteObjectUndo(go, "Changed random rotation (steps)");
        }
    }
    //-----------------------------------------------------------------------------------------------------------------
    void randomScaleUnits()
    {
        for (int i = 0; i < Selection.transforms.Length; i++)
        {
            GameObject go = Selection.transforms[i].gameObject;

            float x = go.transform.localScale.x;
            float y = go.transform.localScale.y;
            float z = go.transform.localScale.z;

            if (rnd_scale_x == true)
            {
                x = Random.Range(x_minScale, x_maxScale);
            }

            if (rnd_scale_y == true)
            {
                y = Random.Range(y_minScale, y_maxScale);
            }

            if (rnd_scale_z == true)
            {
                z = Random.Range(z_minScale, z_maxScale);
            }

            Undo.RegisterCompleteObjectUndo(go, "Changed random scale (units)");

            Vector3 scale = new Vector3(x, y, z);

            if (rnd_scale_unif == true)
            {
                scale = new Vector3(x, x, x);
            }

            go.transform.localScale = scale;

        }
    }

    //-----------------------------------------------------------------------------------------------------------------
    void randomScaleSteps()
    {
        for (int i = 0; i < Selection.transforms.Length; i++)
        {
            GameObject go = Selection.transforms[i].gameObject;

            float x = go.transform.localScale.x;
            float y = go.transform.localScale.y;
            float z = go.transform.localScale.z;

            if (rnd_scaleStep_x == true)
            {
                x = x_scaleStep * Random.Range(x_scaleStep_min, x_scaleStep_max);
            }

            if (rnd_scaleStep_y == true)
            {
                y = y_scaleStep * Random.Range(y_scaleStep_min, y_scaleStep_max);
            }

            if (rnd_scaleStep_z == true)
            {
                z = z_scaleStep * Random.Range(z_scaleStep_min, z_scaleStep_max); 
            }

            Undo.RegisterCompleteObjectUndo(go, "Changed random scale (steps)");

            Vector3 scale = new Vector3(x, y, z);

            if (rnd_scaleStep_unif == true)
            {
                scale = new Vector3(x, x, x);
            }

            go.transform.localScale = scale;

        }
    }
}
#endif