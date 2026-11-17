using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    private static readonly int YVelocityParameter = Animator.StringToHash("yVelocity");
    private static readonly int IsGroundedParameter = Animator.StringToHash("isGrounded");
    private static readonly int XDirectionParameter = Animator.StringToHash("xDirection");
    private static readonly int IsDashingParameter = Animator.StringToHash("isDashing");

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;
    public float dashSpeed = 20f;
    public float bounceGain = 8f;
    public float DashDuration = 0.5f;
    public float WallJumpMultiplierX;
    public float WallJumpMultiplierY;

    public float bounceDuration = 0.2f;
    private bool facingRight = true;
    private float direction = 1;

    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundLayer;
    private bool canDoubleJump = false;
    private bool canAirDash = false;
    public bool isGrounded = false;
    public bool isWallJumping = false;
    [SerializeField] private float groundCheckDisance;
    [SerializeField] private float wallCheckDistance = 0.1f;

    public Vector2 DashVelocity;
    public Vector2 bounceVelocity;
    public Vector2 vectorDashBounce;
    public Vector2 WJVelocity;

    public bool isDashing = false;
    public bool isDashBouncing = false;
    public bool isOnWall = false;
    private bool hasDashBounced;

    private Animator anim;
    private Rigidbody2D rb;
    public BoxCollider2D coll;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponentInChildren<BoxCollider2D>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        HandleGrounding();
        HandleFlip();
        HandleAnimations();

        HandleJump();

        float MoveX = Input.GetAxis("Horizontal");
        float MoveY = Input.GetAxis("Vertical");
        vectorDashBounce = new Vector2(MoveX, MoveY);

        if (Input.GetKeyDown(KeyCode.Q) && !isDashing && (isGrounded || canAirDash))
        {
            if (!isGrounded)
            {
                canAirDash = false;
            }

            StartCoroutine(Dash(vectorDashBounce.x, vectorDashBounce.y));
        }

    }
    IEnumerator Dash(float DashmagX, float DashmagY)
    {
        isDashing = true;
        hasDashBounced = false;
        DashVelocity = new Vector2(DashmagX, 0f).normalized * dashSpeed;
        if (DashVelocity.magnitude == 0f)
        {
            DashVelocity = dashSpeed * transform.right;
        }
        yield return new WaitForSeconds(DashDuration);
        isDashing = false;
    }
    IEnumerator DashBounce()
    {
        if (canDoubleJump == false)
        {
            canDoubleJump = true;
        }
        bounceVelocity = new Vector2(DashVelocity.x * -0.5f, DashVelocity.y + bounceGain);
        isDashBouncing = true;
        yield return new WaitForSeconds(bounceDuration);
        isDashBouncing = false;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Spike" || collision.gameObject.name == "Enemy")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            // Reload current scene
            // Lose screen?
            // -> Menu?
            // *retry *back to main menu *quit game?
        }
    }



// Enemies //
// Position, Speed, Direction, range
// Go forward -> range then turn around
// Go at speed
// If hit player = die
//
//
//
    IEnumerator WallJump()
    {
        isWallJumping = true;
        flip();
        if (canDoubleJump == false)
        {
            canDoubleJump = true;
        }
        WJVelocity = new Vector2(WallJumpMultiplierX * direction, WallJumpMultiplierY);
        yield return new WaitForSeconds(0.2f);
        isWallJumping = false;
    }

    void FixedUpdate()
    {
        HandleWallChecks();

        if (isDashBouncing)
        {
            rb.linearVelocity = bounceVelocity;
        }
        else if (isDashing)
        {
            rb.linearVelocity = DashVelocity;
        }
        else if (isWallJumping)
        {
            rb.linearVelocity = WJVelocity;
        }

        if (!isDashing && !isDashBouncing && !isWallJumping)
        {
            HandleMovement();

        }
        if (rb.linearVelocity.sqrMagnitude > 625f)
        {
            rb.linearVelocity = rb.linearVelocity * 0.8f;
        }
    }

    private void HandleWallChecks()
    {
        Bounds bounds = coll != null
            ? coll.bounds
            : new Bounds(transform.position, Vector3.zero);
        Vector2 leftOrigin = new Vector2(bounds.min.x, bounds.center.y);
        Vector2 rightOrigin = new Vector2(bounds.max.x, bounds.center.y);
        RaycastHit2D leftHit = Physics2D.Raycast(leftOrigin, Vector2.left, wallCheckDistance, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(rightOrigin, Vector2.right, wallCheckDistance, groundLayer);

        isOnWall = IsHitObject(leftHit, "JumpWalls") || IsHitObject(rightHit, "JumpWalls");

        if (isDashing && !hasDashBounced &&
            (IsHitObject(leftHit, "Platforms") || IsHitObject(leftHit, "JumpWalls") ||
             IsHitObject(rightHit, "Platforms") || IsHitObject(rightHit, "JumpWalls")))
        {
            hasDashBounced = true;
            StartCoroutine(DashBounce());
        }
    }

    private static bool IsHitObject(RaycastHit2D hit, string objectName)
    {
        return hit.collider != null && hit.collider.gameObject.name == objectName;
    }

    private void HandleMovement()
    {
        rb.linearVelocity = new Vector2(Input.GetAxis("Horizontal") * moveSpeed, rb.linearVelocity.y);
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        else if (isGrounded)
        {
            canDoubleJump = true;
            canAirDash = true;
        }
    }

    private void Jump()
    {
        if ((isGrounded || canDoubleJump) && !isOnWall && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (!isGrounded && canDoubleJump)
            {
                canDoubleJump = false;
            }
            else
            {
                canDoubleJump = true;
            }
        }
        if (isOnWall)
        {
            StartCoroutine(WallJump());
        }
    }


    public void flip()
    {
        if (!isDashing)
        {
            transform.Rotate(0, 180, 0);
            facingRight = !facingRight;
            direction = direction * -1;
        }
    }

    public void HandleGrounding()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDisance, groundLayer);
    }

    public virtual void HandleAnimations()
    {
        anim.SetFloat(YVelocityParameter, rb.linearVelocity.y);
        anim.SetBool(IsGroundedParameter, isGrounded);
        anim.SetFloat(XDirectionParameter, direction);
        anim.SetBool(IsDashingParameter, isDashing);
    }

    protected virtual void HandleFlip()
    {

        bool leftPressed =
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.LeftArrow);

        bool rightPressed =
            Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.RightArrow);

        if (leftPressed && rightPressed)
        {
            direction = 0;
        }
        else if (leftPressed)
        {
            direction = -1;
        }
        else if (rightPressed)
        {
            direction = 1;
        }
        else
        {
            direction = 0;
        }

        if (direction < 0 && facingRight)
        {
            flip();
        }
        else if (direction > 0 && !facingRight)
        {
            flip();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDisance));

        Bounds bounds = coll != null
            ? coll.bounds
            : new Bounds(transform.position, Vector3.zero);
        Vector3 leftOrigin = new Vector3(bounds.min.x, bounds.center.y);
        Vector3 rightOrigin = new Vector3(bounds.max.x, bounds.center.y);
        Gizmos.DrawLine(leftOrigin, leftOrigin + Vector3.left * wallCheckDistance);
        Gizmos.DrawLine(rightOrigin, rightOrigin + Vector3.right * wallCheckDistance);
    }
}
