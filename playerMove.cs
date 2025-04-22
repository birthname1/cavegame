using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Tilemaps;

public class playerMovement : MonoBehaviour
{
    private Animator animator;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public float moveSpeed = 4.0f;
    private bool isGrounded;
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public bool facingRight = true;

    public Transform groundCheck;
    public float groundCheckDistance = 0.25f;

    private bool jumpInput = false; // Flag to store jump inpu

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpInput = true; // Store jump input
        }
    }

    void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, 
        Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
        
        if (jumpInput && isGrounded)
        {
            Jump();
        }

        jumpInput = false; // Reset jump input flag

        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        Movement();
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }


    


    void Movement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }
    }

    public void Flip()
    {
        facingRight = !facingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    
}