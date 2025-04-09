using UnityEngine;

[ExecuteAlways]
public class DynamicCamera : MonoBehaviour
{
    [SerializeField]
    float minSize = 3f; // Minimum camera size
    [SerializeField]
    float BorderSize = 3f;
    [SerializeField, Range(0f,1f)]
    float smoothing = 0.1f; // Smoothing factor for camera movement

    void Update()
    {
        float _smoothing = smoothing;

#if UNITY_EDITOR
        if (Application.isEditor && !Application.isPlaying)
        {
            _smoothing = 1;
        }
#endif

        // Get the distance between the two players
        Vector3 player1Pos = GameObject.Find("Player (1)").transform.position;
        Vector3 player2Pos = GameObject.Find("Player (2)").transform.position;

        // Set the camera position to be halfway between the two players
        Vector3 cameraPosition = (player1Pos + player2Pos) * .5f;
        Vector3 targetPosDelta = -transform.position + cameraPosition + transform.forward * -10;
        transform.position += targetPosDelta * _smoothing; // Keep the camera at the same distance

        // Align with camera angle
        player1Pos = Vector3.ProjectOnPlane(player1Pos, transform.forward);
        player2Pos = Vector3.ProjectOnPlane(player2Pos, transform.forward);

        // Set the camera size based on the distance between the two players
        float distanceBetweenPlayers = Vector2.Distance(player1Pos, player2Pos) + BorderSize * 2;
        float targetSizeDelta = -Camera.main.orthographicSize + Mathf.Max(distanceBetweenPlayers / 2, minSize);
        Camera.main.orthographicSize += targetSizeDelta * _smoothing;

    }
}
