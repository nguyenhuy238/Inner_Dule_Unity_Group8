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
        public float zoomSpeed = 2f;
        public float followSpeed = 5f;
        
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
            
            // Setup target group if it doesn't exist
            if (targetGroup == null && virtualCamera != null)
            {
                targetGroup = gameObject.AddComponent<CinemachineTargetGroup>();
                virtualCamera.Follow = targetGroup.transform;
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
        }
        
        private void UpdateCameraPosition()
        {
            if (player1 == null || player2 == null || targetGroup == null) return;
            
            // Calculate distance between players
            float distance = Vector3.Distance(player1.position, player2.position);
            
            // Adjust camera orthographic size based on distance
            float targetSize = Mathf.Clamp(distance * 0.8f, minDistance, maxDistance);
            
            if (virtualCamera != null)
            {
                // Smoothly adjust camera size
                float currentSize = virtualCamera.m_Lens.OrthographicSize;
                virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(currentSize, targetSize, zoomSpeed * Time.deltaTime);
            }
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
