using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public int playerNumber = 1;
    public float speed = 5f;
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private Collider2D myCollider;
    private float moveInput;
    public Animator animator; // Bố nhớ kéo Animator của nhân vật vào đây

    private bool isGrounded;
    public LayerMask whatIsGround;

    private int extraJumps;
    public int extraJumpsValue = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        isGrounded = myCollider.IsTouchingLayers(whatIsGround);

        if (isGrounded)
        {
            extraJumps = extraJumpsValue;
        }

        // --- GỬI DỮ LIỆU SANG ANIMATOR ---
        if (animator != null)
        {
            animator.SetFloat("speed", Mathf.Abs(moveInput));
            animator.SetBool("isGrounded", isGrounded);
        }

        if (playerNumber == 1)
        {
            moveInput = 0;
            if (Input.GetKey(KeyCode.A)) moveInput = -1;
            if (Input.GetKey(KeyCode.D)) moveInput = 1;
            if (Input.GetKeyDown(KeyCode.W)) JumpLogic();
        }
        else if (playerNumber == 2)
        {
            moveInput = 0;
            if (Input.GetKey(KeyCode.LeftArrow)) moveInput = -1;
            if (Input.GetKey(KeyCode.RightArrow)) moveInput = 1;
            if (Input.GetKeyDown(KeyCode.UpArrow)) JumpLogic();
        }
    }

    void JumpLogic()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        else if (extraJumps > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            extraJumps--;
        }
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);

        if (moveInput > 0) transform.eulerAngles = new Vector3(0, 0, 0);
        else if (moveInput < 0) transform.eulerAngles = new Vector3(0, 180, 0);
    }
}