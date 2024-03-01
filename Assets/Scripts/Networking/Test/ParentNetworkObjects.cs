using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class ParentNetworkObjects : NetworkBehaviour
{
   [SerializeField] private Transform _toParent;
   private void FixedUpdate()
   {
      if(Runner.IsServer && _toParent.parent == null)
         _toParent.SetParent(this.transform);
         
   }
}
