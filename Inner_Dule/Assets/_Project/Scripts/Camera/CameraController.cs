using UnityEngine;
using Cinemachine;

namespace InnerDuel.Camera
{
    public class CameraController : MonoBehaviour
    {
        [Header("Cinemachine Setup")]
        public CinemachineVirtualCamera virtualCamera;
        public CinemachineTargetGroup targetGroup;
        
        [Header("Camera Settings")]
        public float minDistance = 8f;
        public float maxDistance = 20f;
        public bool useFixedZoom = true;
        public float fixedZoom = 12f;
        public float zoomSpeed = 2f;
        public float followSpeed = 5f;

        [Header("Camera Confiner")]
        public bool enableCameraBound = true;
        public Collider2D confinerCollider;
        public Vector2 minCameraPos = new Vector2(-50f, -50f);
        public Vector2 maxCameraPos = new Vector2(50f, 50f);
        
        [Header("Ending Sequence")]
        public float endingZoomDuration = 2f;
        public float endingFinalZoom = 3f;
        
        private Transform player1;
        private Transform player2;
        private bool isEndingSequence = false;
        private float endingTimer = 0f;
        private Vector3 endingPosition;
        
        private void Start()
        {
            SetupCamera();
        }
        
        private void Update()
        {
            if (isEndingSequence)
            {
                HandleEndingSequence();
            }
            else
            {
                UpdateCameraPosition();
            }
        }
        
        private void SetupCamera()
        {
            if (virtualCamera == null)
            {
                virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            }
            
            if (targetGroup == null)
            {
                targetGroup = FindObjectOfType<CinemachineTargetGroup>();
            }

            // Ensure Cinemachine is actually driving the Main Camera at runtime.
            // Without a CinemachineBrain, Virtual Cameras won't affect the Camera view.
            if (GetComponent<CinemachineBrain>() == null)
            {
                gameObject.AddComponent<CinemachineBrain>();
            }
            
            // Setup target group if it doesn't exist
            if (targetGroup == null && virtualCamera != null)
            {
                targetGroup = gameObject.AddComponent<CinemachineTargetGroup>();
                virtualCamera.Follow = targetGroup.transform;
                virtualCamera.LookAt = targetGroup.transform;
            }

            EnsureVirtualCameraRig();
        }

        private void EnsureVirtualCameraRig()
        {
            if (virtualCamera == null || targetGroup == null) return;

            // In your scene, vcam can accidentally be set to follow the Main Camera itself.
            // That makes the camera jump away from gameplay when Cinemachine goes Live.
            virtualCamera.Follow = targetGroup.transform;
            virtualCamera.LookAt = targetGroup.transform;

            // Ensure Follow actually moves the camera (Body must not be "Do Nothing").
            // For 2D, FramingTransposer is a safe default.
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
            framing.m_BiasX = 0f;
            framing.m_BiasY = 0f;
            framing.m_XDamping = 0f;
            framing.m_YDamping = 0f;
            framing.m_ZDamping = 0f;

            // Set a strict camera size if enabled (no smooth zooming)
            if (virtualCamera != null)
            {
                virtualCamera.m_Lens.Orthographic = true;
                virtualCamera.m_Lens.OrthographicSize = useFixedZoom ? fixedZoom : Mathf.Clamp(virtualCamera.m_Lens.OrthographicSize, minDistance, maxDistance);
            }

            // If vcam transform was authored at a weird Z, keep a standard 2D camera distance.
            var p = virtualCamera.transform.position;
            if (p.z > -0.5f) virtualCamera.transform.position = new Vector3(p.x, p.y, -10f);

            // Add confiner if needed
            if (enableCameraBound)
            {
                var confiner = virtualCamera.GetComponent<Cinemachine.CinemachineConfiner>();
                if (confiner == null)
                {
                    confiner = virtualCamera.gameObject.AddComponent<Cinemachine.CinemachineConfiner>();
                }

                if (confinerCollider != null)
                {
                    confiner.m_BoundingShape2D = confinerCollider as PolygonCollider2D;
                    confiner.m_ConfineMode = Cinemachine.CinemachineConfiner.Mode.Confine2D;
                    confiner.m_Damping = 0f;
                    confiner.m_ConfineScreenEdges = true;
                    confiner.InvalidatePathCache();
                }
            }
        }
        
        public void SetTargets(Transform p1, Transform p2)
        {
            player1 = p1;
            player2 = p2;
            
            if (targetGroup != null)
            {
                // Clear existing targets
                targetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
                
                // Add player targets
                if (player1 != null)
                {
                    targetGroup.AddMember(player1, 1f, 0f);
                }
                
                if (player2 != null)
                {
                    targetGroup.AddMember(player2, 1f, 0f);
                }
            }

            EnsureVirtualCameraRig();
        }
        
        private void UpdateCameraPosition()
        {
            if ((player1 == null && player2 == null) || targetGroup == null || virtualCamera == null) return;

            Vector3 center;
            if (player1 != null && player2 != null)
            {
                center = (player1.position + player2.position) / 2f;
            }
            else if (player1 != null)
            {
                center = player1.position;
            }
            else
            {
                center = player2.position;
            }

            // Set camera size immediately (no smooth zoom)
            float targetSize;
            if (useFixedZoom)
            {
                targetSize = fixedZoom;
            }
            else
            {
                float distance = player1 != null && player2 != null ? Vector3.Distance(player1.position, player2.position) : fixedZoom;
                targetSize = Mathf.Clamp(distance * 0.8f, minDistance, maxDistance);
            }
            virtualCamera.m_Lens.OrthographicSize = targetSize;

            // Ensure there is no damping from Cinemachine Body
            var framing = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (framing != null)
            {
                framing.m_XDamping = 0f;
                framing.m_YDamping = 0f;
                framing.m_ZDamping = 0f;
                framing.m_DeadZoneWidth = 0f;
                framing.m_DeadZoneHeight = 0f;
                framing.m_SoftZoneWidth = 0f;
                framing.m_SoftZoneHeight = 0f;
            }

            // Position directly at player center
            var destination = new Vector3(center.x, center.y, virtualCamera.transform.position.z);

            if (enableCameraBound)
            {
                if (confinerCollider != null)
                {
                    // clamp inside polygon bounds using collider bounds as fallback
                    var b = confinerCollider.bounds;
                    float halfHeight = targetSize;
                    float halfWidth = targetSize * (UnityEngine.Camera.main != null ? UnityEngine.Camera.main.aspect : 1f);
                    destination.x = Mathf.Clamp(destination.x, b.min.x + halfWidth, b.max.x - halfWidth);
                    destination.y = Mathf.Clamp(destination.y, b.min.y + halfHeight, b.max.y - halfHeight);
                }
                else
                {
                    destination.x = Mathf.Clamp(destination.x, minCameraPos.x, maxCameraPos.x);
                    destination.y = Mathf.Clamp(destination.y, minCameraPos.y, maxCameraPos.y);
                }
            }

            virtualCamera.transform.position = destination;
        }
        
        public void StartEndingSequence(Transform winnerTransform)
        {
            isEndingSequence = true;
            endingTimer = 0f;
            
            if (winnerTransform != null)
            {
                endingPosition = winnerTransform.position;
            }
            
            // Switch to single target follow
            if (virtualCamera != null)
            {
                virtualCamera.Follow = winnerTransform;
            }
        }
        
        private void HandleEndingSequence()
        {
            endingTimer += Time.deltaTime;
            
            if (virtualCamera != null)
            {
                // Gradually zoom in on winner
                float progress = endingTimer / endingZoomDuration;
                float targetSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, endingFinalZoom, progress);
                virtualCamera.m_Lens.OrthographicSize = targetSize;
            }
            
            // Add slow-motion effect
            if (endingTimer < 1f)
            {
                Time.timeScale = Mathf.Lerp(1f, 0.3f, endingTimer);
            }
        }
        
        public void ResetCamera()
        {
            isEndingSequence = false;
            endingTimer = 0f;
            Time.timeScale = 1f;
            
            // Reset to target group follow
            if (targetGroup != null)
            {
                virtualCamera.Follow = targetGroup.transform;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (player1 != null && player2 != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(player1.position, player2.position);
                
                // Draw camera bounds
                Gizmos.color = Color.cyan;
                Vector3 center = (player1.position + player2.position) / 2f;
                float distance = Vector3.Distance(player1.position, player2.position);
                Gizmos.DrawWireCube(center, new Vector3(distance, distance * 0.6f, 0));
            }
        }
    }
}
