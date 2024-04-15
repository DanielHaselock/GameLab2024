using Networking.Behaviours;
using UnityEngine;
using TMPro;

public class startGameClicked : MonoBehaviour
{
    public void OnButtonClick()
    {
        NetworkManager.Instance.SmartConnect();
    }
}
