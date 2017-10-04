using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    Vector3 Velocity = new Vector3();

    //will be derived
    private float gravity;
    private float jumpImpulse;

    [SerializeField]
    private float accel = 5;

    [SerializeField]
    private float jumpTime = .75f;

    [SerializeField]
    private float jumpHeight = 3;

    private pawnAABB pawn;

    private bool isGrounded = false;


	// Use this for initialization
	void Start () {
        pawn = GetComponent<pawnAABB>();
        DeriveJumpValues();
	}

    // Update is called once per frame
    void Update() {
        handleInput();

        //do the move
        pawnAABB.CollisionResults results = pawn.Move(Velocity * Time.deltaTime);
        if( results.hitTop || results.hitBottom ) Velocity.y = 0;
        if( results.hitLeft || results.hitRight ) Velocity.x = 0;
        transform.position += results.distance;

        isGrounded = results.hitBottom;

    }

    private void handleInput() {
        //grav
        Velocity.y -= gravity * Time.deltaTime;

        //jump
        if( Input.GetButtonDown("Jump") && isGrounded ) Velocity.y = jumpImpulse;

        //lateral move
        float axisH = Input.GetAxisRaw("Horizontal");

        if( axisH == 0 ) {
            Decelerate(accel);
        } else {
            AccelerateHor(accel * axisH);
        }      

    }

    private void Decelerate(float amount) {
        //slow down the player
        if( Velocity.x > 0 ) { //going right
            AccelerateHor(-amount);
            if( Velocity.x < 0 ) Velocity.x = 0;
        }
        if( Velocity.x < 0 ) { //going right
            AccelerateHor(amount);
            if( Velocity.x > 0 ) Velocity.x = 0;
        }
    }

    private void AccelerateHor(float amount) {
        Velocity.x += amount * Time.deltaTime;
    }

    void DeriveJumpValues() {
        gravity = ( jumpHeight * 2 ) / ( jumpTime * jumpTime );
        jumpImpulse = gravity * jumpTime;
    }
}
