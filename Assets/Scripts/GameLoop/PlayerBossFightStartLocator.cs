using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameLoop
{
    public class PlayerBossFightStartLocator : MonoBehaviour
    {
        public Vector3 SpawnPosition => transform.position + new Vector3(0, 0.5f, 0);
        public Quaternion SpawnRotation => transform.rotation;
    
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(transform.position, Vector3.up, 1);
            Handles.color = Color.blue;
            Handles.DrawLine(transform.position, transform.position + transform.forward, 5);
            Handles.Label(SpawnPosition ,"Player Spawn Point");
#endif

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(SpawnPosition, 0.1f);
        }
    }
}