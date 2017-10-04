using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pawnAABB : MonoBehaviour {

    public struct CollisionResults {
        public Vector3 distance;
        public bool hitTop;
        public bool hitBottom;
        public bool hitLeft;
        public bool hitRight;
        private Bounds bounds;

        public CollisionResults(Vector3 distance, Bounds bounds, float skinWidth) {
            hitTop = false;
            hitBottom = false;
            hitLeft = false;
            hitRight = false;
            this.distance = distance;
            this.bounds = bounds;
            this.bounds.Expand(-skinWidth * 2);
        }

        public Vector3[] GetOriginsH() {
            Vector3[] origins = new Vector3[3];

            float x = bounds.center.x;
            if( distance.x < 0 ) x = bounds.min.x;
            if( distance.x > 0 ) x = bounds.max.x;

            origins[0] = new Vector3(x, bounds.min.y, 0);
            origins[1] = new Vector3(x, bounds.center.y, 0);
            origins[2] = new Vector3(x, bounds.max.y, 0);

            return origins;
        }

        public Vector3[] GetOriginsV() {
            Vector3[] origins = new Vector3[3];

            float y = bounds.center.y;
            if( distance.y < 0 ) y = bounds.min.y;
            if( distance.y > 0 ) y = bounds.max.y;

            origins[0] = new Vector3(bounds.min.x, y, 0);
            origins[1] = new Vector3(bounds.center.x, y, 0);
            origins[2] = new Vector3(bounds.max.x, y, 0);

            return origins;
        }

        //we call after a collider dings something
        public void Limit(float length, bool isHorAxis ) {
            if( isHorAxis ) {
                if(distance.x < 0 ) {//going left
                    hitLeft = true;
                    distance.x = -length;
                }else if(distance.x > 0 ) {//going right
                    hitRight = true;
                    distance.x = length;
                }
            } else {
                if( distance.y > 0 ) { //going up
                    distance.y = length;
                    hitTop = true;
                }else if( distance.y < 0 ) { //going down
                    distance.y = -length;
                    hitBottom = true;
                }
            }
        }

    }

    BoxCollider2D aabb;

    [SerializeField]
    private float skinWidth = .1f;

    [SerializeField]
    private LayerMask collidableWith;

    void Start() {
        aabb = GetComponent<BoxCollider2D>();
    }


    public CollisionResults Move(Vector3 distance ) {
        //want to return:
        //how far the object can move
        //whether or not there were any collisions
        //what side the collisions were on

        CollisionResults result = new CollisionResults(distance, aabb.bounds, skinWidth);

        DoRayCast(ref result, false);
        DoRayCast(ref result, true);

        return result;

    }

    private void DoRayCast(ref CollisionResults results, bool doHorizontal) {

        float sign = Mathf.Sign(doHorizontal ? results.distance.x : results.distance.y);
        Vector3 dir = sign * (doHorizontal ? Vector3.right : Vector3.up);

        float rayLength = skinWidth + Mathf.Abs( doHorizontal ? results.distance.x : results.distance.y );

        Vector3[] origins = doHorizontal ? results.GetOriginsH() : results.GetOriginsV();

        foreach (Vector3 origin in origins ) {

            Debug.DrawRay(origin, dir * rayLength);

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, rayLength, collidableWith);

            if( hit.collider && hit.distance < rayLength ) {
                rayLength = hit.distance;
                results.Limit(rayLength - skinWidth, doHorizontal);
            }
        }        
    }

}
