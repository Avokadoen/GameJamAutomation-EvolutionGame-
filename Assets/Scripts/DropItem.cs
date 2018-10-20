using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour {
	public void Drop(GameObject toInstantiate)
    {
        Instantiate<GameObject>(toInstantiate, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Quaternion.Euler(Vector3.up));
    }
}
