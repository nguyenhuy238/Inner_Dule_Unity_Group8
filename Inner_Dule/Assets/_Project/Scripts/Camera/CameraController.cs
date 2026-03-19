using UnityEngine;
using Cinemachine;

namespace InnerDuel.Camera
{
    /// <summary>
    /// Manages the 2D fighting game camera using Cinemachine.
    /// Tracks two players, maintains them both on screen, and adjusts zoom based on horizontal distance.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Cinemachine Components")]
        [Tooltip("The virtual camera that will be controlled.")]
        public CinemachineVirtualCamera virtualCamera;
        [Tooltip("The target group containing the players.")]
        public CinemachineTargetGroup targetGroup;

        [Header("Zoom Settings")]
        [Tooltip("Minimum orthographic size when players are close.")]
        public float minZoom = 5f;
        [Tooltip("Maximum orthographic size when players are far apart.")]
        public float maxZoom = 10f;
        [Tooltip("Multiplier for how much distance affects zoom (targetSize = minZoom + distance * multiplier).")]
        public float zoomMultiplier = 0.5f;
        [Tooltip("How smoothly the camera zooms (lower value = faster).")]
        public float zoomSmoothTime = 0.1f;
        [Tooltip("Extra zoom-out applied when players are very close to avoid overly tight framing.")]
        public float closeRangeZoomPadding = 1.25f;
        [Tooltip("Horizontal distance where close-range padding fades out to zero.")]
        public float closeRangeDistance = 3f;

        [Header("Framing Settings")]
        [Tooltip("Damping for camera movement. Adjust in Cinemachine Framing Transposer for more control.")]
        public float movementDamping = 0.1f;

        [Header("Camera Confiner")]
        [Tooltip("Enable to confine the camera within a specific area.")]
        public bool enableCameraBound = true;
        [Tooltip("The collider used to define the camera's boundaries (must be PolygonCollider2D or CompositeCollider2D).")]
        public Collider2D confinerCollider;
        [Tooltip("Auto-detect map bounds from SpriteRenderers under MapRoot when confinerCollider is not assigned.")]
        public bool autoDetectMapBounds = true;
        [Tooltip("Name of the map root object used for auto bounds detection.")]
        public string mapRootName = "MapRoot";
        [Tooltip("Inset used for auto bounds to avoid showing map edges.")]
        public float mapBoundsInset = 0.1f;

        [Header("Ending Sequence")]
        public float endingZoomDuration = 2f;
        public float endingFinalZoom = 3f;

        private Transform player1;
        private Transform player2;
        private bool isEndingSequence = false;
        private float endingTimer = 0f;
        private Vector3 endingPosition;
        private float currentZoomVelocity;
        private float initialOrthographicSize;
        private CinemachineBrain mainBrain;
        private UnityEngine.Camera mainCamera;
        private PolygonCollider2D runtimeAutoConfinerCollider;
        private Bounds detectedMapBounds;
        private bool hasDetectedMapBounds;
        private int cachedMapRootChildCount = -1;

        private void Start()
        {
            SetupCamera();

            // Tự động tìm 2 nhân vật dựa trên Tag "Player" mà bố đã cài lúc trước
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length >= 2)
            {
                // Gán nhân vật vào camera
                SetTargets(players[0].transform, players[1].transform);
            }
        }

      
        private void LateUpdate()
        {
            TryBuildAutoMapBounds();
            RefreshLiveVirtualCamera();

            if (isEndingSequence)
            {
                HandleEndingSequence();
            }
            else
            {
                UpdateCameraZoom();
            }
        }

        private void SetupCamera()
        {
            // Ensure CinemachineBrain exists on Main Camera and cache it.
            mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                mainBrain = mainCamera.GetComponent<CinemachineBrain>();
                if (mainBrain == null)
                {
                    mainBrain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
                }
            }

            if (virtualCamera == null)
            {
                // Prefer the currently live camera from CinemachineBrain to avoid controlling the wrong vcam.
                TryAssignVirtualCameraFromBrain();

                if (virtualCamera == null)
                {
                    virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
                }

                if (virtualCamera == null)
                {
                    var allVirtualCameras = FindObjectsOfType<CinemachineVirtualCamera>();
                    for (int i = 0; i < allVirtualCameras.Length; i++)
                    {
                        if (allVirtualCameras[i].name == "FightCamera")
                        {
                            virtualCamera = allVirtualCameras[i];
                            break;
                        }
                    }

                    if (virtualCamera == null && allVirtualCameras.Length > 0)
                    {
                        virtualCamera = allVirtualCameras[0];
                    }
                }
            }

            if (targetGroup == null)
            {
                targetGroup = GetComponentInChildren<CinemachineTargetGroup>();
                if (targetGroup == null) targetGroup = FindObjectOfType<CinemachineTargetGroup>();
            }

            TryBuildAutoMapBounds();
            ConfigureCinemachineComponents();
        }

        private void RefreshLiveVirtualCamera()
        {
            if (mainBrain == null) return;
            if (mainBrain.ActiveVirtualCamera == null) return;

            var liveVirtualCamera = mainBrain.ActiveVirtualCamera.VirtualCameraGameObject != null
                ? mainBrain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>()
                : null;

            if (liveVirtualCamera != null && liveVirtualCamera != virtualCamera)
            {
                virtualCamera = liveVirtualCamera;
                currentZoomVelocity = 0f;
                ConfigureCinemachineComponents();
            }
        }

        private void TryAssignVirtualCameraFromBrain()
        {
            if (mainBrain == null) return;
            if (mainBrain.ActiveVirtualCamera == null) return;

            var activeCameraObject = mainBrain.ActiveVirtualCamera.VirtualCameraGameObject;
            if (activeCameraObject == null) return;

            var activeVirtualCamera = activeCameraObject.GetComponent<CinemachineVirtualCamera>();
            if (activeVirtualCamera != null)
            {
                virtualCamera = activeVirtualCamera;
            }
        }

        private void ConfigureCinemachineComponents()
        {
            if (virtualCamera == null || targetGroup == null) return;

            virtualCamera.Follow = targetGroup.transform;
            virtualCamera.LookAt = null;

            // Configure Lens
            virtualCamera.m_Lens.Orthographic = true;
            initialOrthographicSize = virtualCamera.m_Lens.OrthographicSize;

            // Configure Framing Transposer
            var framing = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (framing == null)
            {
                framing = virtualCamera.AddCinemachineComponent<CinemachineFramingTransposer>();
            }

            framing.m_ScreenX = 0.5f;
            framing.m_ScreenY = 0.5f;
            framing.m_DeadZoneWidth = 0f;
            framing.m_DeadZoneHeight = 0f;
            framing.m_SoftZoneWidth = 0f;
            framing.m_SoftZoneHeight = 0f;
            framing.m_XDamping = movementDamping;
            framing.m_YDamping = movementDamping;
            framing.m_ZDamping = 0f;
            framing.m_GroupFramingMode = CinemachineFramingTransposer.FramingMode.None;

            // Configure Confiner
            Collider2D activeConfinerCollider = confinerCollider != null ? confinerCollider : runtimeAutoConfinerCollider;
            if (enableCameraBound && activeConfinerCollider != null)
            {
                var confiner = virtualCamera.GetComponent<CinemachineConfiner>();
                if (confiner == null) confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner>();
                
                confiner.m_BoundingShape2D = activeConfinerCollider;
                confiner.m_ConfineMode = CinemachineConfiner.Mode.Confine2D;
                confiner.m_Damping = 0f;
                confiner.InvalidatePathCache();
            }
        }

        private void TryBuildAutoMapBounds()
        {
            if (!autoDetectMapBounds || confinerCollider != null) return;

            GameObject mapRoot = GameObject.Find(mapRootName);
            if (mapRoot == null) return;

            int currentMapRootChildCount = mapRoot.transform.childCount;
            if (hasDetectedMapBounds && runtimeAutoConfinerCollider != null && currentMapRootChildCount == cachedMapRootChildCount)
            {
                return;
            }

            SpriteRenderer[] mapRenderers = mapRoot.GetComponentsInChildren<SpriteRenderer>(true);
            if (mapRenderers == null || mapRenderers.Length == 0) return;

            bool hasBounds = false;
            Bounds combinedBounds = new Bounds();

            for (int i = 0; i < mapRenderers.Length; i++)
            {
                SpriteRenderer renderer = mapRenderers[i];
                if (renderer == null || renderer.sprite == null || !renderer.enabled) continue;

                if (!hasBounds)
                {
                    combinedBounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds) return;

            detectedMapBounds = combinedBounds;
            hasDetectedMapBounds = true;
            cachedMapRootChildCount = currentMapRootChildCount;

            if (runtimeAutoConfinerCollider == null)
            {
                GameObject autoConfiner = new GameObject("AutoCameraConfiner");
                autoConfiner.transform.SetParent(mapRoot.transform, false);
                runtimeAutoConfinerCollider = autoConfiner.AddComponent<PolygonCollider2D>();
                runtimeAutoConfinerCollider.isTrigger = true;
            }

            Vector3 confinerSize3 = combinedBounds.size - (Vector3.one * Mathf.Max(0f, mapBoundsInset * 2f));
            Vector2 confinerSize = new Vector2(
                Mathf.Max(0.1f, confinerSize3.x),
                Mathf.Max(0.1f, confinerSize3.y)
            );

            Vector3 localCenter = mapRoot.transform.InverseTransformPoint(combinedBounds.center);
            float halfWidth = confinerSize.x * 0.5f;
            float halfHeight = confinerSize.y * 0.5f;
            Vector2[] points = new Vector2[]
            {
                new Vector2(localCenter.x - halfWidth, localCenter.y - halfHeight),
                new Vector2(localCenter.x - halfWidth, localCenter.y + halfHeight),
                new Vector2(localCenter.x + halfWidth, localCenter.y + halfHeight),
                new Vector2(localCenter.x + halfWidth, localCenter.y - halfHeight),
            };
            runtimeAutoConfinerCollider.pathCount = 1;
            runtimeAutoConfinerCollider.SetPath(0, points);

            ConfigureCinemachineComponents();
        }

        private float GetMapSafeMaxZoom()
        {
            if (!hasDetectedMapBounds) return float.PositiveInfinity;

            float aspect = mainCamera != null
                ? mainCamera.aspect
                : (float)Screen.width / Mathf.Max(1f, Screen.height);

            float heightLimit = detectedMapBounds.extents.y;
            float widthLimit = detectedMapBounds.extents.x / Mathf.Max(0.01f, aspect);
            float safeLimit = Mathf.Min(heightLimit, widthLimit) - Mathf.Max(0f, mapBoundsInset);

            return Mathf.Max(0.1f, safeLimit);
        }

        public void SetTargets(Transform p1, Transform p2)
        {
            player1 = p1;
            player2 = p2;

            if (targetGroup != null)
            {
                // Clear and rebuild target group
                targetGroup.m_Targets = new CinemachineTargetGroup.Target[0];

                if (player1 != null) targetGroup.AddMember(player1, 1f, 0f);
                if (player2 != null) targetGroup.AddMember(player2, 1f, 0f);
            }

            ResetCamera();
        }

        private void UpdateCameraZoom()
        {
            if (player1 == null || player2 == null || virtualCamera == null) return;

            // Requirement 2 & 9: Horizontal distance only
            float distance = Mathf.Abs(player1.position.x - player2.position.x);

            // Add extra padding at close range so the camera does not zoom too tightly.
            float closeRangeFactor = 1f - Mathf.Clamp01(distance / Mathf.Max(0.01f, closeRangeDistance));
            float closeRangePadding = closeRangeFactor * closeRangeZoomPadding;

            // Requirement 2 & 3 & 4: Calculate target zoom
            float targetSize = minZoom + closeRangePadding + (distance * zoomMultiplier);

            float effectiveMinZoom = minZoom;
            float effectiveMaxZoom = maxZoom;

            float mapSafeMaxZoom = GetMapSafeMaxZoom();
            if (!float.IsInfinity(mapSafeMaxZoom))
            {
                effectiveMaxZoom = Mathf.Min(effectiveMaxZoom, mapSafeMaxZoom);
            }

            if (effectiveMinZoom > effectiveMaxZoom)
            {
                effectiveMinZoom = effectiveMaxZoom;
            }

            targetSize = Mathf.Clamp(targetSize, effectiveMinZoom, effectiveMaxZoom);

            // Requirement 8: Smooth zoom
            virtualCamera.m_Lens.OrthographicSize = Mathf.SmoothDamp(
                virtualCamera.m_Lens.OrthographicSize,
                targetSize,
                ref currentZoomVelocity,
                zoomSmoothTime
            );
        }

        public void StartEndingSequence(Transform winnerTransform)
        {
            isEndingSequence = true;
            endingTimer = 0f;
            initialOrthographicSize = virtualCamera.m_Lens.OrthographicSize;

            if (winnerTransform != null && virtualCamera != null)
            {
                virtualCamera.Follow = winnerTransform;
            }
        }

        private void HandleEndingSequence()
        {
            endingTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(endingTimer / endingZoomDuration);

            if (virtualCamera != null)
            {
                // Smoothly zoom in on the winner
                virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(initialOrthographicSize, endingFinalZoom, progress);
            }

            // Slow motion effect
            if (progress < 1f)
            {
                Time.timeScale = Mathf.Lerp(1f, 0.3f, progress);
            }
        }

        public void ResetCamera()
        {
            isEndingSequence = false;
            endingTimer = 0f;
            Time.timeScale = 1f;
            currentZoomVelocity = 0f;

            if (targetGroup != null && virtualCamera != null)
            {
                virtualCamera.Follow = targetGroup.transform;
            }
        }

        private void OnDrawGizmos()
        {
            if (player1 != null && player2 != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 midpoint = (player1.position + player2.position) * 0.5f;
                Gizmos.DrawLine(player1.position, player2.position);
                Gizmos.DrawWireSphere(midpoint, 0.5f);
            }
        }
    }
}
