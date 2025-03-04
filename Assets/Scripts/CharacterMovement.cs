using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterMovement : MonoBehaviour
{
    private static readonly int sr_IsRunning = Animator.StringToHash("isRunning");
    private static readonly int sr_IsJumping = Animator.StringToHash("Jumping");
    private Animator Animator;
    [Header("Movement Settings")]
    [SerializeField] private float MoveSpeed = 5f;
    [SerializeField] private float JumpForce = 10f;
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private float extraHeight = 3.3f;
    private Rigidbody2D Rb;
    private bool isFacingRight = true;
    private float horizontalInput;
    private bool isDashing = false;
    private bool canDash = true;
    [SerializeField] private float dashDuration = 0.15f;  // How long the dash lasts
    [SerializeField] private float dashSpeed = 35f;
    private const float dashDelay = 1f;
    private bool dashed = false;
    private float originalGravity = 9.5f;    // Store to restore after dash


    protected void Awake()
    {
        Animator = GetComponent<Animator>();
        Rb = GetComponent<Rigidbody2D>();
        if (!Rb)
            Debug.LogError("Rigidbody2D is missing!");
    }
    
    public void SetHorizontalInput(Vector2 value)
    {
        horizontalInput = value.x;
    }
    
    public void Move()
    {
        Animator.SetBool(sr_IsRunning, horizontalInput != 0);
        if (!isDashing)
            Rb.velocity = new Vector2(horizontalInput * MoveSpeed, Rb.velocity.y);

        if ((horizontalInput > 0 && !isFacingRight) || (horizontalInput < 0 && isFacingRight))
            flip();

        UpdateGroundedState();
        HandleFalling();       
    }

    private void UpdateGroundedState()
    {
        Animator.SetBool(sr_IsJumping, !isGrounded());
    }

    private void HandleFalling()
    {
        if (!isGrounded() && Rb.velocity.y < 0)
        {
            Animator.SetBool(sr_IsJumping, true);
        }
    }

    private void flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private bool isGrounded()
    {
        Vector2 position = transform.position;
        Vector2 boxSize = new Vector2(3.5f, 1f); // Adjust to match your collider
        Collider2D collider = Physics2D.OverlapBox(
            position + Vector2.down * extraHeight, 
            boxSize, 
            0f, 
            GroundLayer
        );
        if(collider) canDash = true;
        return collider != null;
    }

    public void Dash()
    {
        if (!isDashing && canDash)
        {
            Debug.Log("Starting Dash Coroutine");
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        canDash = false;

        float storedGravity = Rb.gravityScale;
        Rb.gravityScale = 0f;

        Rb.velocity = Vector2.zero;

        float dashDirection = isFacingRight ? 1f : -1f;

        Rb.velocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        Rb.gravityScale = storedGravity;
        isDashing = false;
        Debug.Log("Dash ended");

        yield return new WaitForSeconds(dashDelay);

        canDash = true;
   
    }
    
    private void OnDrawGizmos()
    {
        if (GroundLayer != 0)
        {
            Vector2 position = transform.position;
            Vector2 boxSize = new Vector2(3.5f, 1f); 
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(position + Vector2.down*extraHeight, boxSize);
        }
    }
    public void Jump() 
    {
        if (!isGrounded()) return;
        Animator.SetBool(sr_IsJumping, true);
        Rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
    }
    
    public void OnJumpReleased()
    {
        // Check if the button was released and the velocity is upward
        if (Rb.velocity.y > 0)
        {
            Rb.velocity = new Vector2(Rb.velocity.x, 0);
        }
    }

  
}
