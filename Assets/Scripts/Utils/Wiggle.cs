using System.Collections;
using UnityEngine;

public class Wiggle : MonoBehaviour
{
    private Vector3 origin;
    // Start is called before the first frame update
    void Start()
    {
        origin = transform.position;
        StartCoroutine(WiggleRoutine());
    }

    private IEnumerator WiggleRoutine()
    {
        while (true)
        {
            float timeStep = 0;
            var curr = transform.position;
            var rand = new Vector3(Random.insideUnitSphere.x, Random.insideUnitSphere.y, Random.insideUnitSphere.z);
            rand = Vector3.ClampMagnitude(rand, 0.35f);
            var next = origin + rand;
            while (timeStep <= 1)
            {
                timeStep += Time.deltaTime;
                transform.position = Vector3.Lerp(curr, next, timeStep);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
