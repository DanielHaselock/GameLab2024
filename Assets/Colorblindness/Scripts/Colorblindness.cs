using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace SOHNE.Accessibility.Colorblindness
{
    public enum ColorblindTypes
    {
        Normal = 0,
        Protanopia,
        Protanomaly,
        Deuteranopia,
        Deuteranomaly,
        Tritanopia,
        Tritanomaly,
        Achromatopsia,
        Achromatomaly,
    }

    public class Colorblindness : MonoBehaviour
    {
        Volume[] volumes;
        VolumeComponent lastFilter;

        int maxType;
        int _currentType = 0;
        public int CurrentType
        {
            get => _currentType;

            set
            {
                if (_currentType >= maxType) _currentType = 0;
                else _currentType = value;
            }
        }

        void SearchVolumes() => volumes = GameObject.FindObjectsOfType<Volume>();
        
        public static Colorblindness Instance { get; private set; }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
#if !RENDERPIPELINE
            Debug.LogError("There is no type of <b>SRP</b> included in this project.");
#endif
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            maxType = (int) System.Enum.GetValues(typeof(ColorblindTypes)).Cast<ColorblindTypes>().Last();
        }
        
        void Start()
        {
            if (PlayerPrefs.HasKey("Accessibility.ColorblindType"))
                CurrentType = PlayerPrefs.GetInt("Accessibility.ColorblindType");
            else
                PlayerPrefs.SetInt("Accessibility.ColorblindType", 0);

            SearchVolumes();
            StartCoroutine(ApplyFilter());
        }
        
        public void Change(int filterIndex = -1)
        {
            filterIndex = filterIndex <= -1 ? PlayerPrefs.GetInt("Accessibility.ColorblindType") : filterIndex;
            CurrentType = Mathf.Clamp(filterIndex, 0, maxType);
            if (filterIndex >= 0)
            {
                PlayerPrefs.SetInt("Accessibility.ColorblindType", filterIndex);
            }
            StartCoroutine(ApplyFilter());
        }
        
        IEnumerator ApplyFilter()
        {
            yield return new WaitForEndOfFrame();
            SearchVolumes();
            ResourceRequest loadRequest = Resources.LoadAsync<VolumeProfile>($"Colorblind/{(ColorblindTypes)CurrentType}");

            do yield return null; while (!loadRequest.isDone);

            var filter = loadRequest.asset as VolumeProfile;

            if (filter == null)
            {
                Debug.LogError("An error has occured! Please, report");
                yield break;
            }

            if (lastFilter != null)
            {
                foreach (var volume in volumes)
                {
                    volume.profile.components.Remove(lastFilter);

                    foreach (var component in filter.components)
                        volume.profile.components.Add(component);
                }
            }

            lastFilter = filter.components[0];
        }
    }
}