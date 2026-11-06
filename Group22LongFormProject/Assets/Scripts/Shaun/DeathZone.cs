using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [SerializeField] private ArcadeLifeManager lifeManager;

    private void Awake()
    {
        if (lifeManager == null)
        {
            lifeManager = FindFirstObjectByType<ArcadeLifeManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            if (lifeManager == null)
            {
                lifeManager = FindFirstObjectByType<ArcadeLifeManager>();
            }

            lifeManager?.OnBallFell();
        }
        else if (collision.GetComponent<FallingWordItem>() != null)
        {
            Destroy(collision.gameObject);
        }
    }
}