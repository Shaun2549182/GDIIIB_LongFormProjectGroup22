using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PaddleController : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private float xClamp = 7.5f; // Left/Right movement boundary

    private Rigidbody2D rb;
    private bool isActive = false;
    private Vector3 startPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    private void OnEnable() => GameManager.OnPhaseChanged += HandlePhaseChanged;
    private void OnDisable() => GameManager.OnPhaseChanged -= HandlePhaseChanged;

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        isActive = (newPhase == GamePhase.Arcade);

        if (!isActive)
        {
            rb.linearVelocity = Vector2.zero;
            transform.position = startPosition; // Reset to center
        }
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        float input = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(input * speed, 0f);

        // Clamp paddle within screen bounds
        float clampedX = Mathf.Clamp(transform.position.x, -xClamp, xClamp);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }
}