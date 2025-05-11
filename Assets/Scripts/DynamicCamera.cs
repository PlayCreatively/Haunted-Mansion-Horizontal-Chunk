using System;
using UnityEngine;

[ExecuteAlways, DefaultExecutionOrder(-200)]
public class DynamicCamera : MonoBehaviour
{
    [SerializeField]
    float BorderSize = 3f;
    [SerializeField, Range(0f, 1f)]
    float smoothing = 0.1f; // Smoothing factor for camera movement

    Player[] players;

    void Start()
    {
        players = FindObjectsByType<Player>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    public float zoom = 5f;
    public Vector2 position = Vector2.zero;
    void Update()
    {
        float _smoothing = smoothing;

#if UNITY_EDITOR
        if (players.Length == 0)
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

        {
            //// worldBounds
            Vector2 worldSize = new(2f * zoom * Camera.main.aspect, 2f * zoom);
            Rect worldBounds = new(position - worldSize * .5f, worldSize);
            //// playersRect
            Vector2 p1 = AlignWithCamera(GetPlayerPos(0));
            Vector2 p2 = AlignWithCamera(GetPlayerPos(1));
            Vector2 playersCenter = (p1 + p2) * .5f;
            Vector2 playersSize = new(MathF.Abs(p1.x - p2.x) + BorderSize, MathF.Abs(p1.y - p2.y) + BorderSize);
            Rect playerRect = new(playersCenter - playersSize * .5f, playersSize);

            playerRect = playerRect.CropToBounds(worldBounds).ExpandToRatio(Camera.main.aspect).Restrict(worldBounds);

            transform.position = InverseAlignWithCamera((Vector3)playerRect.center - Vector3.forward * 30);

            Camera.main.orthographicSize = playerRect.height * .5f;
            return;
        }
    }
    Vector3 AlignWithCamera(Vector3 pos) => Quaternion.Inverse(Camera.main.transform.rotation) * pos;
    Vector3 InverseAlignWithCamera(Vector3 pos) => Camera.main.transform.rotation * pos;

    Vector3 GetPlayerPos(int i)
    {
        var player = players[i];
        var pos = player.transform.position;
        pos.y = player.LastGroundedHeight + .5f;
        return pos;
    }

#if UNITY_EDITOR
    public bool editMode = false;

    void UpdateEditModeView()
    {
        Camera.main.orthographicSize = zoom;
        Vector3 targetPos = new(position.x, position.y, -10);

        transform.position = transform.forward * -30 + transform.right * targetPos.x + transform.up * targetPos.y;
    }
#endif
}
