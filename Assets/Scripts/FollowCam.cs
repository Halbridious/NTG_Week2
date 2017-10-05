using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour {

    [SerializeField]
    GameObject target;

    // Update is called once per frame
    void Update() {
        if( target ) {
            transform.position = target.transform.position;
        }
    }
}
