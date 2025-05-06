using System;
using UnityEngine;

[ExecuteAlways, DefaultExecutionOrder(-200)]
public class DynamicCamera : MonoBehaviour
{
    [SerializeField]
    float minSize = 3f; // Minimum camera size
    [SerializeField]
    float BorderSize = 3f;
    [SerializeField, Range(0f,1f)]
    float smoothing = 0.1f; // Smoothing factor for camera movement

    Player[] players;

    void Start()
    {
        players = FindObjectsByType<Player>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    void Update()
    {
        float _smoothing = smoothing;

#if UNITY_EDITOR
        if(players.Length == 0)
         Start();

        if (Application.isEditor && !Application.isPlaying)
        {
            _smoothing = 1;
        }
        if (editMode)
        {
            UpdateEditModeView();
            return;
        }
#endif
        Vector3 cameraPosition = Vector3.zero;

        // Set the camera position to be halfway between the two players
        foreach (var player in players)
            cameraPosition += player.transform.position;

        float maxDistance = 0f;

        for (int i = 0; i < players.Length; i++)
        {
            for (int j = i + 1; j < players.Length; j++)
            {
                float distance = Vector3.Distance(players[i].transform.position, players[j].transform.position);
                if (distance > maxDistance)
                    maxDistance = distance;
            }
        }

        cameraPosition /= players.Length;

        // Align with camera angle
        Vector3[] playersPos = new Vector3[players.Length];

        for (int i = 0; i < players.Length; i++)
            playersPos[i] = Vector3.ProjectOnPlane(playersPos[i], transform.forward);

        Rect playersRect = Encompass(playersPos);
        //cameraPosition = playersRect.center;

        Vector3 targetPosDelta = -transform.position + cameraPosition + (transform.forward * -30);
        transform.position = cameraPosition + (transform.forward * -30); // Keep the camera at the same distance

        // Set the camera size based on the distance between the two players
        float distanceBetweenPlayers = maxDistance + BorderSize * 2;
        float targetSizeDelta = -Camera.main.orthographicSize + Mathf.Max(distanceBetweenPlayers * .5f, minSize);
        Camera.main.orthographicSize += targetSizeDelta * _smoothing;

    }

    private void OnDrawGizmos()
    {
        // draw player rect

        Gizmos.color = Color.red;
        Vector3[] playersPos = new Vector3[players.Length];
        for (int i = 0; i < players.Length; i++)
            playersPos[i] = Vector3.ProjectOnPlane(players[i].transform.position, transform.forward);
        Rect playersRect = Encompass(playersPos);
        Gizmos.DrawLineStrip(playersPos, true);
        // draw player rect
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(playersRect.xMin, playersRect.yMax), new Vector3(playersRect.xMax, playersRect.yMin));
        Gizmos.DrawLine(new Vector3(playersRect.xMin, playersRect.yMin), new Vector3(playersRect.xMax, playersRect.yMax));
        Gizmos.DrawLine(new Vector3(playersRect.xMin, playersRect.yMin), new Vector3(playersRect.xMax, playersRect.yMin));
        Gizmos.DrawLine(new Vector3(playersRect.xMin, playersRect.yMax), new Vector3(playersRect.xMax, playersRect.yMax));
        
    }

    public static Rect Encompass(Vector3[] points)
    {
        if (points == null || points.Length == 0)
            throw new ArgumentException("points array cannot be null or empty", nameof(points));

        // Initialise with first point to avoid sentinel ±∞ values.
        float minX = points[0].x;
        float minY = points[0].y;
        float maxX = minX;
        float maxY = minY;

        for (int i = 1; i < points.Length; ++i)
        {
            Vector3 p = points[i];
            if (p.x < minX) minX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.x > maxX) maxX = p.x;
            if (p.y > maxY) maxY = p.y;
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }


#if UNITY_EDITOR
    public bool editMode = false;
    public float zoom = 5f;
    public Vector2 position = Vector2.zero;

    void UpdateEditModeView()
    {
        Camera.main.orthographicSize = zoom;
        Vector3 targetPos = new(position.x, position.y, -10);

        transform.position = transform.forward * -30 + transform.right * targetPos.x + transform.up * targetPos.y;
    }
#endif
}
