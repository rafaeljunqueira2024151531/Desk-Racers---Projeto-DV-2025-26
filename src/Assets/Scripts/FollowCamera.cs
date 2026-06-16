using UnityEngine;

namespace DeskRacers
{
    public class FollowCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 5f, -7f);
        public float followSharpness = 8f;
        public float lookAhead = 2.5f;

        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.TransformPoint(offset);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-followSharpness * Time.deltaTime));
            Vector3 lookTarget = target.position + target.forward * lookAhead;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookTarget - transform.position, Vector3.up), 1f - Mathf.Exp(-followSharpness * Time.deltaTime));
        }
    }
}
