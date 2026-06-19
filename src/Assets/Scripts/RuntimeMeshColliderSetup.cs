using UnityEngine;

namespace DeskRacers
{
    public class RuntimeMeshColliderSetup : MonoBehaviour
    {
        public bool includeInactive;

        // Adiciona MeshColliders aos modelos filhos quando a pista arranca.
        void Awake()
        {
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(includeInactive);
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

                MeshCollider meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                meshCollider.convex = false;
            }
        }
    }
}
