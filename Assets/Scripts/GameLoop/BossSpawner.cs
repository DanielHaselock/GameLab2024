using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameLoop
{
    public class BossSpawner : MonoBehaviour
    {
        public Vector3 SpawnPos => transform.position;
        public Quaternion SpawnRotation => transform.rotation;
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(SpawnPos, 0.25f);
#if UNITY_EDITOR
            var color = Color.red;
            color.a = 0.25f;
            Handles.color = color;
            Handles.DrawSolidDisc(transform.position, Vector3.up, 1);
            color.a = 1f;
            Handles.color = color;
            Handles.DrawLine(transform.position, transform.position + transform.forward, 5);
            Handles.Label(transform.position ,$"Boss Spawn Point");
#endif
        }
    }
}