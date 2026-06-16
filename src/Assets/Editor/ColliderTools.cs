using UnityEditor;
using UnityEngine;

public static class ColliderTools
{
    // Adiciona MeshColliders aos MeshFilters dos objectos seleccionados e filhos.
    [MenuItem("Desk Racers/Colliders/Add Mesh Colliders To Selection")]
    public static void AddMeshCollidersToSelection()
    {
        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            MeshFilter[] meshFilters = selectedObject.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter.sharedMesh == null)
                {
                    continue;
                }

                if (meshFilter.GetComponent<Collider>() != null)
                {
                    continue;
                }

                MeshCollider meshCollider = Undo.AddComponent<MeshCollider>(meshFilter.gameObject);
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                meshCollider.convex = false;
                meshCollider.cookingOptions = MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices;
                GameObjectUtility.SetStaticEditorFlags(meshFilter.gameObject, StaticEditorFlags.BatchingStatic);
                EditorUtility.SetDirty(meshFilter.gameObject);
            }
        }
    }

    // Remove MeshColliders dos objectos seleccionados e filhos.
    [MenuItem("Desk Racers/Colliders/Remove Mesh Colliders From Selection")]
    public static void RemoveMeshCollidersFromSelection()
    {
        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            MeshCollider[] meshColliders = selectedObject.GetComponentsInChildren<MeshCollider>(true);
            foreach (MeshCollider meshCollider in meshColliders)
            {
                Undo.DestroyObjectImmediate(meshCollider);
            }
        }
    }
}
