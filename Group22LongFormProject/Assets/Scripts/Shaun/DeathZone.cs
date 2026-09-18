using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [SerializeField] private ArcadeLifeManager lifeManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            lifeManager.OnBallFell();
        }
        else if (collision.GetComponent<FallingWordItem>() != null)
        {
            Destroy(collision.gameObject);
        }
    }
}