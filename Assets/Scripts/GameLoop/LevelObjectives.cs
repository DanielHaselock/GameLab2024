using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameLabs/LevelObjectives", fileName = "New Level Objectives")]
public class LevelObjectives : ScriptableObject
{
    public List<ObjectiveData> objectives;
}
