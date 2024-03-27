using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectiveData", menuName = "GameLabs/ObjectiveData")]
public class ObjectiveData : ScriptableObject
{
    public string objectiveText;
    public string key;
    public int value;
    public int targetValue;
    public enum OperationType {Add, Sub};
    public OperationType operationType;

    public void Initialize(string name, string key,int currentValue, int target, OperationType operation)
    {
        this.objectiveText = name;
        this.key = key;
        this.value = currentValue; 
        this.targetValue = target;
        this.operationType = operation;
    }
}
