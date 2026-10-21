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

    [Header("Ball Colors")]
    [SerializeField] private Color defaultColor = Color.red;                 // Default Ball Color (Act 1 / Unselected)
    [SerializeField] private Color liamColor = Color.white;                  // Liam (White)
    [SerializeField] private Color benColor = new Color(1f, 0.41f, 0.71f);   // Ben (Hot Pink)
    [SerializeField] private Color emmaColor = new Color(0.2f, 0.6f, 1f);     // Emma (Bright Blue)

    [Header("Juice / Screen Shake")]
    [SerializeField] private float wallShakeDuration = 0.12f;
    [SerializeField] private float wallShakeMagnitude = 0.1f;

    private bool isLaunched = false;
    private bool isActive = false;
    private Vector3 paddleOffset;
    private Vector3 originalScale;
    private float currentSpeed;
    private Vector2 lastVelocity;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

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

    private void Start()
    {
        // Ensures the ball color updates immediately when scene loads (e.g. entering Act 2)
        ApplyCharacterModifiers();
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

        // Re-apply modifiers & color on ball reset
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

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        string activeCharacter = "";
        bool isAct2 = false;

        if (GameManager.Instance != null)
        {
            activeCharacter = (GameManager.Instance.SelectedCharacterWord ?? "").ToLower().Trim();
            isAct2 = GameManager.Instance.CurrentAct == 2;
        }

        // 1. Color assignment logic
        switch (activeCharacter)
        {
            case "liam":
                if (spriteRenderer != null) spriteRenderer.color = liamColor;
                break;

            case "ben":
                if (spriteRenderer != null) spriteRenderer.color = benColor;
                break;

            case "emma":
                if (spriteRenderer != null) spriteRenderer.color = emmaColor;
                break;

            default:
                // No character selected yet (Act 1) or unmapped character -> Red
                if (spriteRenderer != null) spriteRenderer.color = defaultColor;
                break;
        }

        // 2. Act 2 Physics & Scale Modifiers
        if (isAct2)
        {
            switch (activeCharacter)
            {
                case "liam": // Clumsy: Faster, bigger
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
            if (paddleTransform != null)
            {
                transform.position = paddleTransform.position + paddleOffset;
            }

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
            lastVelocity = rb.linearVelocity;

            if (rb.linearVelocity != Vector2.zero)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isActive || !isLaunched) return;

        if (collision.gameObject.CompareTag("Wall"))
        {
            CameraShake.Instance?.Shake(wallShakeDuration, wallShakeMagnitude);
        }

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

        if (collision.contacts.Length > 0)
        {
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, normal).normalized;

            if (GameManager.Instance != null && GameManager.Instance.CurrentAct == 2)
            {
                string charName = (GameManager.Instance.SelectedCharacterWord ?? "").ToLower().Trim();
                if (charName == "liam")
                {
                    Vector2 randomWobble = Random.insideUnitCircle * 0.15f;
                    reflectDir = (reflectDir + randomWobble).normalized;
                }
            }

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