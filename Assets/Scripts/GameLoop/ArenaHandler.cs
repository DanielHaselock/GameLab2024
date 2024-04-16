using UnityEngine;

namespace GameLoop
{
    public class ArenaHandler : MonoBehaviour
    {
        [SerializeField] private GameObject openArena;
        [SerializeField] private GameObject closedArena;

        public void CloseArena()
        {
            openArena.SetActive(false);
            closedArena.SetActive(true);
        }
    }
}