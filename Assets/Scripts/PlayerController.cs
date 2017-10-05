using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [SerializeField]
    Vector3 Velocity = new Vector3();
    [Tooltip("Which direction is gravity pointing?  Should be normalized and cardinal!")]
    [SerializeField]
    private Vector3 gravityDirection = Vector3.down;
    [Tooltip("How quickly the player accelerates when 'running'")]
    [SerializeField]
    private float accel = 5;
    [Tooltip("How long it should take to reach the apex of the player's jump.")]
    [SerializeField]
    private float jumpTime = .75f;
    [Tooltip("How high (in unity units) the player should be able to jump.")]
    [SerializeField]
    private float jumpHeight = 3;
    [Tooltip("How powerful the Jetpack thrust is.")]
    [SerializeField]
    private float jetpackImpulse = 5;
    [Tooltip("Max charge time a jetpack has")]
    public float jetpackTime = 3f;
    [Tooltip("Current charge time the jetpack has")]
    public float jetpackTimer = 3f;

    //The strength of gravity (default), derived on start
    private float gravStrength;
    //The Strength of jump power, derived at the start
    private float jumpImpulse;
    //the collision hull
    private pawnAABB pawn;
    //tells whether or not the player is on the ground
    private bool isGrounded = false;
    private BoxCollider2D aabb;


    // Use this for initialization
    void Start () {
        pawn = GetComponent<pawnAABB>();
        DeriveJumpValues();
        aabb = GetComponent<BoxCollider2D>();
	}

    // Update is called once per frame
    void Update() {

        handleInput();

        //Move the player, which triggers collision detection in the pawnAABB class, which returns collision results
        pawnAABB.CollisionResults results = pawn.Move(Velocity * Time.deltaTime);

        //kill the velocity after a collision
        if( results.hitTop || results.hitBottom ) Velocity.y = 0;
        if( results.hitLeft || results.hitRight ) Velocity.x = 0;


        //apply the transformation based on the collision results
        transform.position += results.distance;

        //if we've   collided w/ the ground, toggle that boolean
        isGrounded = results.hitBottom;//TODO:Change to be relative to gravity!

    }

    private void LateUpdate() {
        //collide w/ volumes
        GameObject[] volumes = GameObject.FindGameObjectsWithTag("Volume");
        foreach (GameObject volume in volumes ) {
            if( aabb.bounds.Intersects(volume.GetComponent<BoxCollider2D>().bounds)){
                gravityDirection = volume.GetComponent<Gravity_Volume>().gravityVec;
                gravStrength = volume.GetComponent<Gravity_Volume>().gravMult;
            } else {
                DeriveJumpValues();
                gravityDirection = Vector3.down;
            }
        }
    }

    #region input handlers

    /**
     * This function checks on various inputs for: Jumping, Jetpacking, and running
     * This function, and the functions it calls, compile a velocity vector, they don't move the player.
     * When the function completes the velocity vector is applied to the player all at once in the update
     **/
    private void handleInput() {

        //charge jetpacks
        JetpackRecharge();
               
        //jump
        if( Input.GetButtonDown("Jump") && isGrounded ) Velocity.y = jumpImpulse;

        //lateral move
        float axisH = Input.GetAxisRaw("Horizontal");

        //add jetpack velocity
        if( Input.GetMouseButton(0) && jetpackTimer > 0 ) {
            Jetpack();
            jetpackTimer -= Time.deltaTime;
        }

        if( axisH == 0 ) {
            Decelerate(accel);
        } else {
            AccelerateHor(accel * axisH);
        }

        //grav
        Velocity += gravityDirection * gravStrength * Time.deltaTime;

    }

    //apply "friction", only if the player is grounded
    private void Decelerate(float amount) {
        //slow down the player
        if( Velocity.x > 0 && isGrounded) { //going right
            AccelerateHor(-amount);
            if( Velocity.x < 0 ) Velocity.x = 0;
        }
        if( Velocity.x < 0 && isGrounded) { //going right
            AccelerateHor(amount);
            if( Velocity.x > 0 ) Velocity.x = 0;
        }
    }

    //accelerate horizontally
    private void AccelerateHor(float amount) {
        Velocity.x += amount * Time.deltaTime;
    }

    void DeriveJumpValues() {
        gravStrength = ( jumpHeight * 2 ) / ( jumpTime * jumpTime );
        jumpImpulse = gravStrength * jumpTime;
    }

    //adds a directional force to the velocity vector in the direction of the mouse
    private void Jetpack() {
        //Get rocket vector positioning
        Vector3 mousePos = Input.mousePosition;//get the mouse position on the screen as an (x,y,0) vector
        mousePos.z = Camera.main.transform.position.z * -1f;//get the "depth" of the camera for this vector 3
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);//convert the mouse to the world coordinates
        print("mouse position in world space: " + mousePos);

        //find the vector in the direction of the mouse from the player
        Vector3 direction = mousePos - transform.position;

        print("Direction: " + direction);

        Velocity.x += direction.normalized.x * jetpackImpulse * Time.deltaTime;
        Velocity.y += direction.normalized.y * jetpackImpulse * Time.deltaTime;

    }

    //if a player is on the ground, we recharge their jetpack
    //jetpacks recharge over time and have a maximum limit
    private void JetpackRecharge() {
        //once a player lands, start recharging their jetpack
        if( isGrounded ) {
            if( jetpackTimer > jetpackTime ) {
                jetpackTimer = jetpackTime;
            } else {
                jetpackTimer += Time.deltaTime;
            }
        }
    }

    #endregion


}
