using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class PlayerSpawnPoint : MonoBehaviour
{
    public Vector3 SpawnPosition => transform.position + new Vector3(0, 0.25f, 0);
    public Quaternion SpawnRotation => transform.rotation;
    
    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        Handles.color = Color.cyan;
        Handles.DrawSolidDisc(transform.position, Vector3.up, 1);
        Handles.color = Color.red;
        Handles.DrawLine(transform.position, transform.position + transform.forward, 5);
        Handles.Label(SpawnPosition ,"Player Spawn Point");
        #endif

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(SpawnPosition, 0.1f);
    }
}
