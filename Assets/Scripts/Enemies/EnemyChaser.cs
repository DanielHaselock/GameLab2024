using UnityEngine;
public class EnemyChaser : Enemy
{
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (_targetPlayer != null)
        {
            navMeshAgent.destination = _targetPlayer.transform.position;
        }
    }
    public override void ChangeTargeting()
    {
        base.ChangeTargeting();
        switch (_seenPlayers.Count)
        {
            case 0:
                _targetPlayer = null;
                break;
            case 1:
                _targetPlayer = _seenPlayers[0];
                break;
            default:                
                if (Vector3.Distance(transform.position, _seenPlayers[0].transform.position) > Vector3.Distance(transform.position, _seenPlayers[1].transform.position))
                    _targetPlayer = _seenPlayers[1];
                else
                    _targetPlayer = _seenPlayers[0];
                break;
        }
    }
}
