using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRespawn playerRespawn =
                other.GetComponent<PlayerRespawn>();

            if (playerRespawn != null)
            {
                playerRespawn.SetCheckpoint(respawnPoint);

                Debug.Log(
                    "Checkpoint updated: "
                    + gameObject.name
                    + " → "
                    + respawnPoint.name
                );
            }
        }
    }
}