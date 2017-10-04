using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_rayCaster : MonoBehaviour {

    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private GameObject cylindar;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if( Input.GetButtonDown("Fire1") ) DoRayCast();

	}

    private void DoRayCast() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow, 5);
        if( Physics.Raycast(ray, out hit, 10, layerMask) ) {
            //hit.collider.GetComponent<MeshRenderer>().material.color = Color.red;

            Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            cylindar.transform.position = hit.point;
            cylindar.transform.rotation = rot;
            //Instantiate(prefab, hit.point, rot);

        }
        


    }

}
