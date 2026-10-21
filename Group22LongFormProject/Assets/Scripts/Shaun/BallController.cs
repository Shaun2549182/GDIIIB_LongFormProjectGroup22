using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Base Settings")]
    [SerializeField] private float initialSpeed = 10f;
    [SerializeField] private Transform paddleTransform;

    [Header("Character Colors")]
    [SerializeField] private Color liamColor = Color.white;
    [SerializeField] private Color benColor = new Color(1f, 0.41f, 0.71f); // Hot Pink
    [SerializeField] private Color emmaColor = new Color(0.2f, 0.6f, 1f);   // Bright Blue

    [Header("Juice / Screen Shake")]
    [SerializeField] private float wallShakeDuration = 0.12f;
    [SerializeField] private float wallShakeMagnitude = 0.1f;

    private bool isLaunched = false;
    private bool isActive = false;
    private Vector3 paddleOffset;
    private Vector3 originalScale;
    private float currentSpeed;
    private Vector2 lastVelocity; // Cache pre-collision velocity to prevent double reflection!

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Enable continuous collision detection to prevent tunneling through thin bricks
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.gravityScale = 0f;
        }

        originalScale = transform.localScale;
        currentSpeed = initialSpeed;

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
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }
            ResetBall();
        }
    }

    public void ResetBall()
    {
        isLaunched = false;

        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
        }

        // Apply character color and Act 2 physics/scale changes
        ApplyCharacterModifiers();

        if (paddleTransform != null)
        {
            transform.position = paddleTransform.position + paddleOffset;
        }
    }

    private void ApplyCharacterModifiers()
    {
        transform.localScale = originalScale;
        currentSpeed = initialSpeed;

        if (GameManager.Instance == null) return;

        string activeCharacter = (GameManager.Instance.SelectedCharacterWord ?? "").ToLower().Trim();
        bool isAct2 = GameManager.Instance.CurrentAct == 2;

        // 1. Color assignment (Acts 1 and 2)
        switch (activeCharacter)
        {
            case "ben":
                if (spriteRenderer != null) spriteRenderer.color = benColor;
                break;
            case "emma":
                if (spriteRenderer != null) spriteRenderer.color = emmaColor;
                break;
            case "liam":
            default:
                if (spriteRenderer != null) spriteRenderer.color = liamColor;
                break;
        }

        // 2. Act 2 Character Physics & Scale Modifiers
        if (isAct2)
        {
            switch (activeCharacter)
            {
                case "liam": // Clumsy: Bigger, faster
                    currentSpeed *= 1.25f;
                    transform.localScale = originalScale * 1.3f;
                    break;

                case "ben": // Curious: Smaller, floatier
                    currentSpeed *= 0.95f;
                    transform.localScale = originalScale * 0.85f;
                    break;

                case "emma": // Diligent: Faster precision
                    currentSpeed *= 1.15f;
                    break;
            }
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
        rb.linearVelocity = launchDirection * currentSpeed;
        lastVelocity = rb.linearVelocity;
    }

    private void FixedUpdate()
    {
        if (isActive && isLaunched && rb != null)
        {
            // Cache pre-collision velocity before physics step resolves
            lastVelocity = rb.linearVelocity;

            // Maintain constant ball speed without changing direction
            if (rb.linearVelocity != Vector2.zero)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isActive || !isLaunched) return;

        // 1. Wall Hit -> Trigger Screen Shake
        if (collision.gameObject.CompareTag("Wall"))
        {
            CameraShake.Instance?.Shake(wallShakeDuration, wallShakeMagnitude);
        }

        // 2. Dynamic Paddle Bounce (steer ball depending on where it hits paddle)
        if (collision.gameObject.CompareTag("Paddle"))
        {
            float hitPoint = transform.position.x - collision.transform.position.x;
            float paddleWidth = collision.collider.bounds.size.x;
            float normalizedHit = Mathf.Clamp(hitPoint / (paddleWidth * 0.5f), -1f, 1f);

            Vector2 bounceDirection = new Vector2(normalizedHit, 1f).normalized;
            rb.linearVelocity = bounceDirection * currentSpeed;
            lastVelocity = rb.linearVelocity;
            return;
        }

        // 3. Bricks / Walls Collision -> Calculate clean reflection from PRE-COLLISION velocity
        if (collision.contacts.Length > 0)
        {
            Vector2 normal = collision.contacts[0].normal;

            // Reflect the pre-collision velocity vector off the contact surface normal
            Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, normal).normalized;

            // Liam's Act 2 Clumsy Bounce Wobble
            if (GameManager.Instance != null && GameManager.Instance.CurrentAct == 2)
            {
                string charName = (GameManager.Instance.SelectedCharacterWord ?? "").ToLower().Trim();
                if (charName == "liam")
                {
                    Vector2 randomWobble = Random.insideUnitCircle * 0.15f;
                    reflectDir = (reflectDir + randomWobble).normalized;
                }
            }

            // Anti-Wall Hugging & Anti-Lockup (Ejects ball cleanly off side walls)
            float minAngleThreshold = 0.3f;

            if (Mathf.Abs(reflectDir.x) < minAngleThreshold)
            {
                float dirX = (normal.x != 0) ? Mathf.Sign(normal.x) : (reflectDir.x >= 0 ? 1f : -1f);
                reflectDir.x = dirX * minAngleThreshold;
                reflectDir = reflectDir.normalized;
            }

            if (Mathf.Abs(reflectDir.y) < minAngleThreshold)
            {
                float dirY = (normal.y != 0) ? Mathf.Sign(normal.y) : (reflectDir.y >= 0 ? 1f : -1f);
                reflectDir.y = dirY * minAngleThreshold;
                reflectDir = reflectDir.normalized;
            }

            rb.linearVelocity = reflectDir * currentSpeed;
            lastVelocity = rb.linearVelocity;
        }
    }
}