using UnityEngine;

public class PaddleMovement : MonoBehaviour
{
    public float speed;
   public float maxX = 7f;

    float movementHorizontal;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        movementHorizontal = Input.GetAxis("Horizontal");
        if((movementHorizontal>0 && transform.position.x<maxX) || (movementHorizontal<0 && transform.position.x<maxX))
        transform.position += Vector3.right *movementHorizontal*speed*Time.deltaTime;

        
    }
}
