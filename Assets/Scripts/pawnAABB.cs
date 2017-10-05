using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pawnAABB : MonoBehaviour {

    //custom data type containing collision results
    public struct CollisionResults {
        public Vector3 distance;//the distance to travel 
        public bool hitTop;//if there was contact to the top of the pawn
        public bool hitBottom;//if there was contact to the bottom of the pawn
        public bool hitLeft;//if there was contact to the left of the pawn
        public bool hitRight;//if there was contact to the right of the pawn
        private Bounds bounds;//the objects hull
        
        //Constructor for the results datatype
        public CollisionResults(Vector3 distance, Bounds bounds, float skinWidth) {
            //Collision reporting booleans are false by default
            hitTop = false;
            hitBottom = false;
            hitLeft = false;
            hitRight = false;
            
            this.distance = distance;
            this.bounds = bounds;
            
            //we want the bounds to be slightly smaller than the object itself to properly check collisions
            this.bounds.Expand(-skinWidth * 2);
        }

        //When checking for collisions horizontally, get the origins for the feeler vectors left or right
        public Vector3[] GetOriginsH() {
            //an array of points that will be the origins
            Vector3[] origins = new Vector3[3];

            //the center of the pawn
            float x = bounds.center.x;
            //if we're attempting to move left, we start on the left side of the box, and vise versa
            if( distance.x < 0 ) x = bounds.min.x;
            if( distance.x > 0 ) x = bounds.max.x;

            //we're using 3 feeler vectors at the top, middle, and bottom of that edge
            origins[0] = new Vector3(x, bounds.min.y, 0);
            origins[1] = new Vector3(x, bounds.center.y, 0);
            origins[2] = new Vector3(x, bounds.max.y, 0);

            //return the vector (really they're points) array
            return origins;
        }

        //When checking for collisions vertically, get the origins for the feeler vectors top or bottom
        public Vector3[] GetOriginsV() {
            //an array of points that will be the origins
            Vector3[] origins = new Vector3[3];

            //the center of the pawn
            float y = bounds.center.y;
            //if we're attempting to move up, we start on the top side of the box, and vise versa
            if( distance.y < 0 ) y = bounds.min.y;
            if( distance.y > 0 ) y = bounds.max.y;

            //we're using 3 feeler vectors at the left, middle, and right of that edge
            origins[0] = new Vector3(bounds.min.x, y, 0);
            origins[1] = new Vector3(bounds.center.x, y, 0);
            origins[2] = new Vector3(bounds.max.x, y, 0);

            //return the vector (really they're points) array
            return origins;
        }

        //we call after a collider dings something, changes distance to reflect that we can't move any further
        public void Limit(float length, bool isHorAxis ) {
            if( isHorAxis ) {//we ARE moving horizontally
                if(distance.x < 0 ) {//our movement is going left
                    hitLeft = true;//set this bool to true so we know we've collided on this side
                    distance.x = -length;//change the distance we're travelling in this direction to reflect the collision
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

#region variables

    BoxCollider2D aabb;

    [Tooltip("How offset the collision origins are.  Effects edge sensativity.")]
    [SerializeField]
    private float skinWidth = .1f;
    [Tooltip("Which object types can this hull collide with.  Select all applicable.")]
    [SerializeField]
    private LayerMask collidableWith;

#endregion

    //Intialize
    void Start() {
        aabb = GetComponent<BoxCollider2D>();//assign the pawns collider to this hull
    }

    /**
     * Takes a vector and checks for collisions in the direction of movement
     * Returns collision data containing a collection of information
     **/
    public CollisionResults Move( Vector3 distance ) {

        CollisionResults result = new CollisionResults(distance, aabb.bounds, skinWidth);

        //Use a direct reference to the result (rather than the COPY that would be passed through without the 'ref' keyword)
        DoRayCast(ref result, false);//check up or down
        DoRayCast(ref result, true);//check left or right

        //pass the results back out as a collisionresults structure
        return result;

    }

    //actually cast the rays to check for collisions
    //requires direct reference to the results data and whether we're doing this horizontally or vertically
    private void DoRayCast(ref CollisionResults results, bool doHorizontal) {
        //get the sign.  Read as "if hoizontal get sign of x, if not horizonta(vertical) get sign of y)
        float sign = Mathf.Sign(doHorizontal ? results.distance.x : results.distance.y);
        //set the direction of the vector.  read as "if horizontal, go right or left, * sign, otherwise etc."
        Vector3 dir = sign * (doHorizontal ? Vector3.right : Vector3.up);

        //we shoot this ray from slightly inside the object to get accurate results w/ close objects, hence the "skin"
        float rayLength = skinWidth + Mathf.Abs( doHorizontal ? results.distance.x : results.distance.y );

        //get an array of points as vector3s that these feeler vectors will originate from
        Vector3[] origins = doHorizontal ? results.GetOriginsH() : results.GetOriginsV();

        //for each of these orign points
        foreach (Vector3 origin in origins ) {
            //draws these rays in editor
            Debug.DrawRay(origin, dir * rayLength);

            //get hit data for a ray cast from the origin in the direction of travel, until it collides with something we've said this pawn can collide with
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, rayLength, collidableWith);
            //if it hit something, and that something is closer than we intended to travel
            if( hit.collider && hit.distance < rayLength ) {
                rayLength = hit.distance;//we're now only going to search the new shorter distance.  We don't care about colliding with anything further than this
                results.Limit(rayLength - skinWidth, doHorizontal);//in the colliding data we now limit the distance we can move to this new distance.  This is were we actually say "we collided"
            }
        }        
    }

}
