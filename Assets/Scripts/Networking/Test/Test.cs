using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Networking.Tests
{
    public class Test : NetworkBehaviour
    {
        [SerializeField] private NetworkPrefabRef _ballRef;

        private IEnumerator Start()
        {
            yield return new WaitWhile(() => Runner == null);
            if(!Runner.IsServer)
                yield break;
            var ball = Runner.Spawn(_ballRef, new Vector3(-10, 10, 0), Quaternion.identity);
        }
    }
}

