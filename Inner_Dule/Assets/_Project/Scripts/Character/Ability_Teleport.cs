using UnityEngine;
using System.Collections;

namespace InnerDuel.Characters
{
    public class Ability_Teleport : BaseCharacterAbility
    {
        [Header("Teleport Settings")]
        public float teleportDistance = 4f;
        public GameObject teleportEffect;
        
        public override void OnSkill2() // Bound to Attack 2
        {
            if (controller == null) return;
            
            StartCoroutine(TeleportRoutine());
        }
        
        private IEnumerator TeleportRoutine()
        {
            // Visual Effect before
            if (teleportEffect != null) Instantiate(teleportEffect, transform.position, Quaternion.identity);
            
            // Wait a tiny bit?
            yield return null;
            
            // Teleport
            float dir = transform.localScale.x > 0 ? 1f : -1f; 
            // Check SpriteRenderer flip instead usually
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) dir = sr.flipX ? -1f : 1f;
            
            Vector3 targetPos = transform.position + new Vector3(dir * teleportDistance, 0f, 0f);
            
            // Bounds check could be added here
            
            transform.position = targetPos;
            
            // Visual Effect after
            if (teleportEffect != null) Instantiate(teleportEffect, transform.position, Quaternion.identity);
        }
    }
}
