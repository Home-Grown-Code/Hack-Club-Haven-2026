
using UnityEngine;

public class EnemyWalk : MonoBehaviour
{
    private Rigidbody2D rb;

    public float moveSpeed = 3f;
    public float direction = 1f;

    public float wallCheckDistance = 0.5f;

    [SerializeField] private float groundCheckOffset = 0.5f;
    [SerializeField] private float groundCheckDistance = 1f;

    public LayerMask groundLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        RaycastHit2D wallHit = Physics2D.Raycast(
            transform.position,
            Vector2.right * direction,
            wallCheckDistance,
            groundLayer
        );

        Vector2 groundCheckPosition = (Vector2)transform.position +
            new Vector2(groundCheckOffset * direction, -0.5f);

        RaycastHit2D groundHit = Physics2D.Raycast(
            groundCheckPosition,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        if (wallHit.collider != null || groundHit.collider == null)
        {
            direction *= -1f;
        }

        rb.linearVelocity = new Vector2(
            moveSpeed * direction,
            rb.linearVelocity.y
        );
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            transform.position,
            Vector2.right * direction * wallCheckDistance
        );

        Vector2 groundCheckPosition = (Vector2)transform.position +
            new Vector2(groundCheckOffset * direction, -0.5f);

        Gizmos.color = Color.green;

        Gizmos.DrawRay(
            groundCheckPosition,
            Vector2.down * groundCheckDistance
        );
    }
}
