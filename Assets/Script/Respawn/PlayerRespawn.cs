using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Transform currentCheckpoint;
    private CharacterController characterController;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        currentCheckpoint = checkpoint;
    }

    public void Respawn()
    {
        if (currentCheckpoint == null)
            return;

        characterController.enabled = false;

        transform.position = currentCheckpoint.position;
        transform.rotation = currentCheckpoint.rotation;

        characterController.enabled = true;
    }
}