using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public enum AttackType { Melee, Ranged }
    public AttackType attackType;

    public Animator animator;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    [Header("Tấn công thường")]
    public int attackDamage = 20;
    public float attackRate = 2f;
    public GameObject arrowPrefab;

    [Header("Skill Prefabs (Dành cho Cung Thủ)")]
    public GameObject skill1ArrowPrefab; // Mũi tên skill 1
    public GameObject skill3ArrowPrefab; // Mũi tên năng lượng skill 3

    [Header("Thời gian hồi chiêu")]
    public float skill1CD = 3f;
    public float skill2CD = 5f;
    public float skill3CD = 8f;

    private float nextAttackTime = 0f;
    private float nextS1Time = 0f;
    private float nextS2Time = 0f;
    private float nextS3Time = 0f;

    private bool isDefending = false;

    void Update()
    {
        int pNum = GetComponent<PlayerMovement>().playerNumber;

        if (pNum == 1) // CAM XUC (Cận chiến)
        {
            if (Time.time >= nextAttackTime && Input.GetKeyDown(KeyCode.H)) { PerformAttack(); nextAttackTime = Time.time + 1f / attackRate; }
            if (Time.time >= nextS1Time && Input.GetKeyDown(KeyCode.J)) { PerformMeleeSkill("Skill1", 30); nextS1Time = Time.time + skill1CD; }
            if (Time.time >= nextS2Time && Input.GetKeyDown(KeyCode.K)) { PerformMeleeSkill("Skill2", 45); nextS2Time = Time.time + skill2CD; }
            if (Time.time >= nextS3Time && Input.GetKeyDown(KeyCode.L)) { PerformMeleeSkill("Skill3", 70); nextS3Time = Time.time + skill3CD; }
            UpdateDefend(Input.GetKey(KeyCode.S));
        }
        else if (pNum == 2) // LY TRI (Cung thủ)
        {
            if (Time.time >= nextAttackTime && Input.GetKeyDown(KeyCode.Keypad4)) { PerformAttack(); nextAttackTime = Time.time + 1f / attackRate; }

            // Skill 1: Bắn 1 mũi tên đặc biệt
            if (Time.time >= nextS1Time && Input.GetKeyDown(KeyCode.Keypad1))
            {
                animator.SetTrigger("Skill1");
                Shoot(skill1ArrowPrefab, 0f);
                nextS1Time = Time.time + skill1CD;
            }
            // Skill 2: Bắn 3 mũi tên tỏa ra
            if (Time.time >= nextS2Time && Input.GetKeyDown(KeyCode.Keypad2))
            {
                animator.SetTrigger("Skill2");
                Shoot(arrowPrefab, 0f);   // Mũi tên thẳng
                Shoot(arrowPrefab, 15f);  // Mũi tên chéo lên
                Shoot(arrowPrefab, -15f); // Mũi tên chéo xuống
                nextS2Time = Time.time + skill2CD;
            }
            // Skill 3: Bắn mũi tên năng lượng cực mạnh
            if (Time.time >= nextS3Time && Input.GetKeyDown(KeyCode.Keypad3))
            {
                animator.SetTrigger("Skill3");
                Shoot(skill3ArrowPrefab, 0f);
                nextS3Time = Time.time + skill3CD;
            }

            UpdateDefend(Input.GetKey(KeyCode.DownArrow));
        }
    }

    void PerformAttack()
    {
        if (isDefending) return;
        animator.SetTrigger("Attack");
        if (attackType == AttackType.Ranged) Shoot(arrowPrefab, 0f);
        else MeleeDamage(attackDamage);
    }

    void PerformMeleeSkill(string trigger, int damage)
    {
        if (isDefending) return;
        animator.SetTrigger(trigger);
        MeleeDamage(damage);
    }

    void UpdateDefend(bool holdingKey)
    {
        isDefending = holdingKey;
        animator.SetBool("isDefending", isDefending);
    }

    void MeleeDamage(int damage)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            PlayerHealth health = enemy.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(damage);
        }
    }

    // Hàm Shoot nâng cấp để bắn theo góc quay
    void Shoot(GameObject prefab, float angleOffset)
    {
        if (prefab == null) return;

        // Tính toán góc quay (quay quanh trục Z)
        Quaternion rotation = transform.rotation * Quaternion.Euler(0, 0, angleOffset);

        GameObject arrow = Instantiate(prefab, attackPoint.position, rotation);
        arrow.GetComponent<Arrow>().owner = gameObject;
    }
}