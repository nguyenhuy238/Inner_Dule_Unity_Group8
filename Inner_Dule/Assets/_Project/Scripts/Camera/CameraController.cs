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

        [Header("Framing Settings")]
        [Tooltip("Damping for camera movement. Adjust in Cinemachine Framing Transposer for more control.")]
        public float movementDamping = 0.1f;

        [Header("Camera Confiner")]
        [Tooltip("Enable to confine the camera within a specific area.")]
        public bool enableCameraBound = true;
        [Tooltip("The collider used to define the camera's boundaries (must be PolygonCollider2D or CompositeCollider2D).")]
        public Collider2D confinerCollider;

        [Header("Ending Sequence")]
        public float endingZoomDuration = 2f;
        public float endingFinalZoom = 3f;

        private Transform player1;
        private Transform player2;
        private bool isEndingSequence = false;
        private float endingTimer = 0f;
        private float currentZoomVelocity;
        private float initialOrthographicSize;

        private void Start()
        {
            SetupCamera();
        }

        private void LateUpdate()
        {
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
            if (virtualCamera == null)
            {
                virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
                if (virtualCamera == null) virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            }

            if (targetGroup == null)
            {
                targetGroup = GetComponentInChildren<CinemachineTargetGroup>();
                if (targetGroup == null) targetGroup = FindObjectOfType<CinemachineTargetGroup>();
            }

            // Ensure CinemachineBrain exists on Main Camera
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null && mainCamera.GetComponent<CinemachineBrain>() == null)
            {
                mainCamera.gameObject.AddComponent<CinemachineBrain>();
            }

            ConfigureCinemachineComponents();
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
            if (enableCameraBound && confinerCollider != null)
            {
                var confiner = virtualCamera.GetComponent<CinemachineConfiner>();
                if (confiner == null) confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner>();
                
                confiner.m_BoundingShape2D = confinerCollider;
                confiner.m_ConfineMode = CinemachineConfiner.Mode.Confine2D;
                confiner.m_Damping = 0f;
                confiner.InvalidatePathCache();
            }
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

            // Requirement 2 & 3 & 4: Calculate target zoom
            float targetSize = minZoom + (distance * zoomMultiplier);
            targetSize = Mathf.Clamp(targetSize, minZoom, maxZoom);

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
