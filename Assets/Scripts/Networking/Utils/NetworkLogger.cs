using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Networking.Utils
{
   public static class NetworkLogger
   {
      public static void Log(string str)
      {
         Debug.Log($"<color=cyan>{str}</color>");
      }

      public static void Error(string str)
      {
         Debug.Log($"<color=red>{str}</color>");
      }

      public static void Warn(string str)
      {
         Debug.Log($"<color=yellow>{str}</color>");
      }
   }
}
