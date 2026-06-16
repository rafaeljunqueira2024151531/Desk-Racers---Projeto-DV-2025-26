using UnityEngine;

namespace DeskRacers
{
    public class RaceWaypointPath : MonoBehaviour
    {
        public Transform[] waypoints;

        // Preenche automaticamente a lista com os filhos se estiver vazia.
        void OnValidate()
        {
            if (waypoints != null && waypoints.Length > 0)
            {
                return;
            }

            waypoints = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                waypoints[i] = transform.GetChild(i);
            }
        }
    }
}
