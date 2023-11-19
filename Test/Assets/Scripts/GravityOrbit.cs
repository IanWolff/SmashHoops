using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityOrbit : MonoBehaviour
{
    public float Gravity;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ( collision.GetComponent<GravityController>() )
        {
            collision.GetComponent<GravityController>().Gravity = this.GetComponent<GravityOrbit>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
