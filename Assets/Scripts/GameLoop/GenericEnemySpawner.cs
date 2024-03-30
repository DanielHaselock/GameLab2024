using Fusion;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameLoop
{
    public class GenericEnemySpawner : MonoBehaviour
    {
        [SerializeField] public NetworkPrefabRef prefab;
        public NetworkPrefabRef Prefab => prefab;
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(transform.position, Vector3.up, 1);
            Handles.color = Color.red;
            Handles.DrawLine(transform.position, transform.position + transform.forward, 5);
            Handles.Label(transform.position ,$"Spawn Point {(Prefab==null?"**NULL**":"")}");
#endif

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.1f);
        }
    }
    

}