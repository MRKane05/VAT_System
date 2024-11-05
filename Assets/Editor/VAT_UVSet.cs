using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;
using System.Collections.Generic;
using System.Linq;

//A quick tool for assigning mesh UV2 based off of vertex index. This can be reset by the editor reimporting the mesh however - Kano
public class VAT_UVSet : EditorWindow {
    private const int MAX_TEXTURE_SIZE = 4096;


    [MenuItem("Tools/UV2 Modifier")]
    

    public static void ShowWindow()
    {
        GetWindow<VAT_UVSet>("VAT UV2 Modifier");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Modify UV2s"))
        {
            ModifyUVs();
        }
    }

    private void ModifyUVs()
    {
        Object[] selectedObjects = Selection.objects;
        foreach (Object obj in selectedObjects)
        {
            GameObject go = obj as GameObject;
            if (go != null)
            {
                SkinnedMeshRenderer smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
                if (smr != null)
                {
                    Mesh mesh = smr.sharedMesh;
                    if (mesh != null)
                    {

                        // Add your UV modification logic here.
                        ModifyUVs(mesh, smr, go);
                        // Mark the mesh as modified and save the asset
                        EditorUtility.SetDirty(mesh);
                        AssetDatabase.SaveAssets();
                        // Process the mesh and UVs here
                        Debug.Log("Processed mesh: " + mesh.name);
                    }
                    else
                    {

                    }
                }
                else
                {
                    Debug.LogWarning(go.name + " does not have a SkinnedMeshRenderer.");
                }
            }
        }
    }


    private void ModifyUVs(Mesh mesh, SkinnedMeshRenderer smr, GameObject go)
    {

        Vector2[] newUVs = new Vector2[mesh.vertexCount];

        // Access the bones and weights of the skinned mesh
        BoneWeight[] boneWeights = mesh.boneWeights;
        Transform[] bones = smr.bones;

        int textureHeight = mesh.vertexCount;
        textureHeight = GetNearestPowerOfTwo(textureHeight);

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            // Find the bone with the highest weight for each vertex
            //BoneWeight weight = boneWeights[i]; 
            int boneIndex = i; // I guess...
            newUVs[i] = new Vector2(0, (boneIndex + 0.5f) / textureHeight);   //As our textel size will be 256, and we want to offset to be in the middle of our pixel
        }

        // Assign the modified UVs to the mesh
        //mesh.uv2 = newUVs;
        mesh.SetUVs(1, newUVs.ToList());
    }

    private int GetNearestPowerOfTwo(int x)
    {
        if (x < 0) { return 0; }
        --x;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        return x + 1;
    }

    private static int GetMajorBoneIndex(BoneWeight weight)
    {
        int bestIndex = 0;
        float bestWeight = -1f;
        //We can't simply iterate through these.
        if (weight.weight0 > bestWeight)
        {
            bestIndex = weight.boneIndex0;
            bestWeight = weight.weight0;
        }
        if (weight.weight1 > bestWeight)
        {
            bestIndex = weight.boneIndex1;
            bestWeight = weight.weight1;
        }
        if (weight.weight2 > bestWeight)
        {
            bestIndex = weight.boneIndex2;
            bestWeight = weight.weight2;
        }
        if (weight.weight3 > bestWeight)
        {
            bestIndex = weight.boneIndex3;
            bestWeight = weight.weight3;
        }

        return bestIndex;
    }

    private static int GetMajorBoneIndexAI(BoneWeight weight)
    {
        // Find the bone index with the highest weight
        float maxWeight = weight.weight0;
        int majorIndex = weight.boneIndex0;

        if (weight.weight1 > maxWeight) { maxWeight = weight.weight1; majorIndex = weight.boneIndex1; }
        if (weight.weight2 > maxWeight) { maxWeight = weight.weight2; majorIndex = weight.boneIndex2; }
        if (weight.weight3 > maxWeight) { majorIndex = weight.boneIndex3; }

        return majorIndex;
    }


    private static HumanBodyBones GetHumanBodyBoneIndex(SkinnedMeshRenderer smr, Transform boneTransform, GameObject go)
    {
        Animator animator = go.GetComponent<Animator>();
        if (!animator)
        {
            animator = go.GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
            {
                HumanBodyBones humanBone = (HumanBodyBones)i;
                if (animator.GetBoneTransform(humanBone) == boneTransform)
                {
                    //Debug.Log(humanBone);
                    return humanBone;
                }
            }
        }

        return HumanBodyBones.LastBone;
    }
}
