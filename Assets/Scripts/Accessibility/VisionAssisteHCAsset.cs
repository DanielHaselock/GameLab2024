using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Accessibility
{
    [CreateAssetMenu(menuName = "GameLabs/Vision Assist High Contrast Assist", fileName = "VisionAssistHCAsset")]
    public class VisionAssistHCAsset : ScriptableObject
    {
        public SerializableDictionary<string, Color> LocalPlayerColors;
        public SerializableDictionary<string, Color> RemotePlayerColors;
        public SerializableDictionary<string, Color> EnemyColors;
        public SerializableDictionary<string, Color> ItemOfInterestColors;
    }
}