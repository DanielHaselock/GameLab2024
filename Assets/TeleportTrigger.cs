using Fusion;
using UnityEngine;

public class TeleportTrigger : NetworkBehaviour
{
    [SerializeField] private Transform teleportLoc; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.ToLower().Equals("player") && Runner.IsServer)
        {
            var player = other.GetComponent<Player>();
            if(player == null)
                return;
            
            player.MarkForTeleport(teleportLoc.position,teleportLoc.rotation);
        }
    }
}
