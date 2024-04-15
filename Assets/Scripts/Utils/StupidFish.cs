using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class StupidFish : MonoBehaviour
{
   [SerializeField] private float angleCorrection = -90;
   
   private Vector3 motionVec;
   private Vector3 origin;
   private bool up = false;
   
   private void Start()
   {
      origin = transform.position;
      transform.position = origin + new Vector3(0,0.5f,0);
      
      StartCoroutine(Bouy());
      StartCoroutine(DumbFish());
   }
   private void Update()
   {
      if(motionVec != Vector3.zero)
         transform.forward = Vector3.Lerp(transform.forward, motionVec, 0.05f);
   }

   IEnumerator Bouy()
   {
      float bTime = 0;
      var curr = transform.position;
      while (true)
      {
         yield return new WaitForEndOfFrame();
         var newPosition = curr;
         bTime += Time.deltaTime / 2f;
         if (up)
         {
            newPosition.y = Mathf.Lerp(curr.y, origin.y + 0.5f, bTime);
         }
         else
         {
            newPosition.y = Mathf.Lerp(curr.y, origin.y - 0.5f, bTime);
         }
         transform.position = newPosition;
         if (bTime >= 1)
         {
            bTime = 0;
            up = !up;
            curr = transform.position;
         }
      }
   }
   
   IEnumerator DumbFish()
   {
      while (true)
      {
         Vector3 curr = transform.position;
         Vector3 newPos = origin + new Vector3(Random.insideUnitCircle.x, curr.y, Random.insideUnitCircle.y) * Random.Range(3, 5f);
         float speed = Random.Range(2, 5);
         float dur = Vector3.Distance(curr, newPos) / speed;
         float timeStep = 0;
         while (timeStep <= 1)
         {
            curr.y = transform.position.y;
            newPos.y = transform.position.y;
            
            timeStep += Time.deltaTime / dur;
            //its hacky, I know x_x
            motionVec = (newPos - curr).normalized;
            var temp = transform.forward;
            transform.forward = motionVec;
            transform.Rotate(Vector3.up, angleCorrection);
            motionVec = transform.forward;
            transform.forward = temp;
            transform.position = Vector3.Lerp(curr, newPos, timeStep);
            yield return new WaitForEndOfFrame();
         }

         yield return new WaitForSeconds(0.1f);
      }
   }
}
