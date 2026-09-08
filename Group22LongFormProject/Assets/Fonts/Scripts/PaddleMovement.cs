using JetBrains.Annotations;
using UnityEngine;

public class PaddleMovement : MonoBehaviour
{
    public float speed;
    float movementHorizontal; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        movementHorizontal = Input.GetAxis("Horizontal");
        transform.position += Vector3.right *movementHorizontal*speed*Time.deltaTime;
    }
}
