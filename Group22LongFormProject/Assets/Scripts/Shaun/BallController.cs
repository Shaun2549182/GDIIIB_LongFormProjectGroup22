using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [SerializeField] private float initialSpeed = 10f;
    [SerializeField] private Transform paddleTransform;

    private Rigidbody2D rb;
    private bool isLaunched = false;
    private bool isActive = false;
    private Vector3 paddleOffset;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (paddleTransform != null)
        {
            paddleOffset = transform.position - paddleTransform.position;
        }
    }

    private void OnEnable() => GameManager.OnPhaseChanged += HandlePhaseChanged;
    private void OnDisable() => GameManager.OnPhaseChanged -= HandlePhaseChanged;

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        isActive = (newPhase == GamePhase.Arcade);

        if (isActive)
        {
            ResetBall();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            ResetBall();
        }
    }

    public void ResetBall() // Changed from private to public
    {
        isLaunched = false;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;

        if (paddleTransform != null)
        {
            transform.position = paddleTransform.position + paddleOffset;
        }
    }

    private void Update()
    {
        if (!isActive) return;

        if (!isLaunched)
        {
            // Follow paddle position prior to launch
            if (paddleTransform != null)
            {
                transform.position = paddleTransform.position + paddleOffset;
            }

            // Space, Click, or Jump button launches ball
            if (Input.GetButtonDown("Jump") || Input.GetMouseButtonDown(0))
            {
                Launch();
            }
        }
    }

    private void Launch()
    {
        isLaunched = true;
        Vector2 launchDirection = new Vector2(Random.Range(-0.3f, 0.3f), 1f).normalized;
        rb.linearVelocity = launchDirection * initialSpeed;
    }

    private void FixedUpdate()
    {
        if (isActive && isLaunched)
        {
            // Lock ball to standard speed so physics bounces don't slow it down or accelerate it
            rb.linearVelocity = rb.linearVelocity.normalized * initialSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isActive || !isLaunched) return;

        // 1. Dynamic Paddle Bounce (steer ball depending on where it hits the paddle)
        if (collision.gameObject.CompareTag("Paddle"))
        {
            // Calculate offset relative to paddle center (-1 at far left, 0 at center, +1 at far right)
            float hitPoint = transform.position.x - collision.transform.position.x;
            float paddleWidth = collision.collider.bounds.size.x;
            float normalizedHit = Mathf.Clamp(hitPoint / (paddleWidth * 0.5f), -1f, 1f);

            // Calculate bounce vector (e.g., steep angle at edges, vertical in center)
            Vector2 bounceDirection = new Vector2(normalizedHit, 1f).normalized;
            rb.linearVelocity = bounceDirection * initialSpeed;
            return;
        }

        // 2. Prevent Vertical Lockup (stuck bouncing straight up/down near walls)
        if (Mathf.Abs(rb.linearVelocity.x) < 0.8f)
        {
            // Push inward toward screen center (assuming screen origin is X = 0)
            float pushDirection = (transform.position.x < 0) ? 1.5f : -1.5f;
            rb.linearVelocity = new Vector2(pushDirection, rb.linearVelocity.y).normalized * initialSpeed;
        }

        // 3. Prevent Horizontal Lockup (stuck bouncing side-to-side)
        if (Mathf.Abs(rb.linearVelocity.y) < 0.2f)
        {
            float overrideY = (rb.linearVelocity.y < 0) ? -1f : 1f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, overrideY).normalized * initialSpeed;
        }
    }
}