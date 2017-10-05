using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity_Volume : MonoBehaviour {

    [Tooltip("The strength of gravity in this volume")]
    public float gravMult = 2f;

    [Tooltip("The direction of gravity in this volume")]
    public Vector3 gravityVec = Vector3.up;


}
