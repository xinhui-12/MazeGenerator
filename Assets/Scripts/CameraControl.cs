using UnityEditor;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform playerAgent;
    public float heightScale = 0.5f; //adjustable camera height
    public float radius = 1.0f; // Distance from player to camera

    private void LateUpdate()
    {
       if(playerAgent != null)
        {
            //Calculate the offset based on the scale half of the agent's surrent local's scale y component
            float playerHeight = playerAgent.localScale.y * heightScale;
            Vector3 offset = new Vector3(0f, playerHeight, -radius);

            // Set camera position to player's position plus the calculated offset
            transform.position = playerAgent.position + playerAgent.TransformVector(offset);

            // Set camera rotation to match player's rotation (assuming player controls the rotation)
            transform.rotation = playerAgent.rotation;
        }
    }
}
