using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    public GravityOrbit Gravity;
    private Rigidbody2D rb2D;

    public float rotationSpeed = 20;
    
    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (Gravity)
        {
            // Calculate where gravity is pulling
            Vector2 attractorPull = Vector2.zero;
            attractorPull = (transform.position - Gravity.transform.position).normalized;

            // Calculate how player should rotate
            Vector2 characterPull = transform.up;
            Quaternion targetrotation = Quaternion.FromToRotation(characterPull, attractorPull) * transform.rotation;

            // Rotate player towards attractor
            transform.up = Vector2.Lerp(transform.up, attractorPull, rotationSpeed * Time.deltaTime);

            // Apply attractor pull
            rb2D.AddForce((-attractorPull * Gravity.Gravity) * rb2D.mass);
        }
    }
}
