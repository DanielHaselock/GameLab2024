using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Networking.Behaviours;

public class NameField : MonoBehaviour
{
    TMP_InputField inputField;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();

        var randName = $"Player{Random.Range(0, 1000):000}";
        NetworkManager.Instance.SetSessionUserNickName(randName);

        inputField.onValueChanged.AddListener((text) =>
        { NetworkManager.Instance.SetSessionUserNickName(text); });
    }
}
