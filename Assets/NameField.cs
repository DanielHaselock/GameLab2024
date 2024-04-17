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

        var randName = $"Random{Random.Range(1, 300).ToString()}";
        NetworkManager.Instance.SetSessionUserNickName(randName);

        inputField.onValueChanged.AddListener((text) =>
        { NetworkManager.Instance.SetSessionUserNickName(text); });
    }
}
