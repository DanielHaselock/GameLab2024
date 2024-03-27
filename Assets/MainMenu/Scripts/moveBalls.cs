using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveBalls : MonoBehaviour
{
    public float moveDistance = 0.1f; // The distance the object moves up and down
    public float moveSpeed = 1.0f; // The speed of the movement

    private float startY; // The initial y position of the object

    void Start()
    {
        startY = transform.position.y; // Get the initial y position
    }

    void Update()
    {
        // Calculate the vertical offset using a sine function
        float offset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;

        // Set the new position
        transform.position = new Vector3(transform.position.x, startY + offset, transform.position.z);
    }
}
