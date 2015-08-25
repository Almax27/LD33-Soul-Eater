using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CombineMeshes : MonoBehaviour {

    [ContextMenu("Combine And Destroy Children")]
    void CombineChildrenSaveDelete()
    {
        CombineChildren(true);
    }
        
    [ContextMenu("Combine Children")]
    void CombineChildren()
    {
        CombineChildren(false);
    }

    [ContextMenu("Jitter Children Z")]
    void JitterChildrenZ()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        for(int i = 0; i < meshFilters.Length; i++)
        {
            Transform t = meshFilters[i].transform;
            Vector3 p = t.position;
            p.z = Random.Range(-0.01f,0.01f);
            t.position = p;
        }
    }

    [ContextMenu("Save mesh as asset")]
    void SaveMeshAsAsset()
    {
        #if UNITY_EDITOR
        var path = EditorUtility.SaveFilePanelInProject("Save mesh asset", "CombinedMesh", "asset", "Select save file path");
        if(!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(GetComponent<MeshFilter>().sharedMesh, path);
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(path, typeof(Object)));
        }
        #endif
    }

    void CombineChildren(bool delete)
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            if(delete)
            {
                Destroy(meshFilters[i].gameObject);
            }
            else
            {
                meshFilters[i].gameObject.SetActive(false);
            }
            i++;
        }
        transform.GetComponent<MeshFilter>().sharedMesh = new Mesh();
        transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
        transform.gameObject.active = true;
    }
}
