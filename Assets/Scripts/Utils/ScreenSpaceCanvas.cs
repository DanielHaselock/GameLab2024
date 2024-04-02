using System;
using UnityEngine;

namespace Utils
{
    public class ScreenSpaceCanvas : MonoBehaviour
    {
        private void Awake()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
        }
    }
}