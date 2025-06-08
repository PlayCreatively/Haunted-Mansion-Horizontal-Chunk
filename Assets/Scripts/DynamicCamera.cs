using GameManagers;
using System;
using UnityEngine;

[ExecuteAlways, DefaultExecutionOrder(+500)]
public class DynamicCamera : MonoBehaviour
{
    [SerializeField]
    float BorderSize = 3f;
    [SerializeField, Range(0f, 1f)]
    float smoothing = 0.1f; // Smoothing factor for camera movement

    Player[] players;
    Camera mainCamera;

    void Start()
    {
        players = FindObjectsByType<Player>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        mainCamera = GetComponent<Camera>();
    }

    public float zoom = 5f;
    public Vector2 position = Vector2.zero;
    public bool followPlayers = false;
    void Update()
    {
        if (followPlayers && players.Length > 0)
            UpdateFollowPlayers();
    }

    void UpdateFollowPlayers()
    {
        float _smoothing = smoothing;

#if UNITY_EDITOR
        if (players.Length == 0 || mainCamera == null)
            Start();

        if (Application.isEditor && !Application.isPlaying)
        {
            _smoothing = 1;

            if (editMode)
            {
                UpdateEditModeView();
                return;
            }
        }
#endif

        bool zoomOut = true;
        for (int i = 0; i < players.Length; i++)
            if (players[i] != null)
                zoomOut &= players[i].ZoomInput;

        {
            //// worldBounds
            Vector2 worldSize = new(2f * zoom * mainCamera.aspect, 2f * zoom);
            Rect worldBounds = new(position - worldSize * .5f, worldSize);
            //// playersRect
            Rect playerRect;
            if (players.Length == 2 && players[0] != null && players[1] != null)
            {
                Vector2 p1 = AlignWithCamera(GetPlayerPos(0));
                Vector2 p2 = AlignWithCamera(GetPlayerPos(1));
                Vector2 playersCenter = (p1 + p2) * .5f;
                Vector2 playersSize = new(MathF.Abs(p1.x - p2.x) + BorderSize, MathF.Abs(p1.y - p2.y) + BorderSize);
                playerRect = new(playersCenter - playersSize * .5f, playersSize);
            }
            else
            {
                Vector2 size = Vector2.one * BorderSize;
                if (players[0] != null)
                    playerRect = new((Vector2)AlignWithCamera(GetPlayerPos(0)) - size * .5f, size);
                else
                    playerRect = new((Vector2)AlignWithCamera(GetPlayerPos(1)) - size * .5f, size);

            }

            playerRect = playerRect.CropToBounds(worldBounds).ExpandToRatio(mainCamera.aspect).Restrict(worldBounds);

            Vector3 targetPos;
            float targetZoom;

            if (zoomOut || Game.IsPaused)
            {
                _smoothing = .1f;
                Time.timeScale = 0f;
                targetZoom = zoom;
                targetPos = InverseAlignWithCamera(new(position.x, position.y, -30f));
            }
            else
            {
                Time.timeScale = 1f;
                targetPos = InverseAlignWithCamera((Vector3)playerRect.center - Vector3.forward * 30);
                targetZoom = playerRect.height * .5f;
            }

            Quaternion targetRotation = Quaternion.Euler(45, 45, 0);
            
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, targetPos, _smoothing), Quaternion.Lerp(transform.rotation, targetRotation, _smoothing * .15f));
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, _smoothing * .5f);

            return;
        }
    }

    Vector3 AlignWithCamera(Vector3 pos) => Quaternion.Inverse(mainCamera.transform.rotation) * pos;
    Vector3 InverseAlignWithCamera(Vector3 pos) => mainCamera.transform.rotation * pos;

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
        mainCamera.orthographicSize = zoom;
        Vector3 targetPos = new(position.x, position.y, -10);

        transform.position = transform.forward * -30 + transform.right * targetPos.x + transform.up * targetPos.y;
    }
#endif
}
