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
    public Sprite objectiveUISprite;
    public enum OperationType {Add, Sub};
    public OperationType operationType;
}
