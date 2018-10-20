using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PhysicalObject : MonoBehaviour
{
    const float DAMAGE_TRESHOLD = 7.5f;

    public Rigidbody body;
    Collider objCollider;

    public float durability; //How much this object can take before being broken, -1 is unbreakable;
    public float density; //How dense an object is determines how much damage it applies to other objects

    private Vector3 velocity;
    private Vector3 torque;

    private bool isPickedUp;
    Transform handTransform; //TODO: we might need to use this for velocity later on if 


    [System.Serializable] public class ColliderEvent : UnityEvent<Collider> { };
    [System.Serializable] public class HandEvent : UnityEvent<PhysicalObject> { };

    public UnityEvent
        onDestroy,
        onHolding;
    public ColliderEvent
        onHit;
    public HandEvent
        onPickUp,
        onDrop;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isPickedUp && (collision.gameObject.tag == "Hand"))
            return; //ignore this collision

        {
            float otherDensity = collision.gameObject.GetComponent<PhysicalObject>()?.density ?? 0.5f;
            if (otherDensity <= 0) return;
            float potentialDamage = otherDensity * collision.relativeVelocity.magnitude;
            durability -= (potentialDamage > DAMAGE_TRESHOLD * density) ? potentialDamage : 0;
            if (durability <= 0) Destroy(gameObject);
        }
        //onHit.Invoke(collision.collider);
    }

    /// <summary>
    /// Called when picking up this item
    /// </summary>
    public void OnPickUp(PhysicalObject pickedUpByHand)
    {
        //internal stuff

        //NOTE: REMEMBER TO USE RIGIDBODY.MovePosition instead of changing transform.position to avoid weird physics, or lack thereof
        body.isKinematic = true;


        //external stuff
        onPickUp.Invoke(pickedUpByHand);
    }

    /// <summary>
    /// Called every fixedupdate frame when holding this
    /// </summary>
    public void OnHolding(PhysicalObject heldByHand)
    {
        //internal

        //maybe calculate velocity or not?;

        //external stuff
        onHolding.Invoke();
    }

    public void OnDrop(PhysicalObject droppedByHand)
    {
        //internal stuff

        //NOTE: REMEMBER TO USE RIGIDBODY.MovePosition instead of changing transform.position to avoid weird physics, or lack thereof
        body.isKinematic = false;

        //body.velocity = droppedByHand.GetTrackedObjectVelocity();

        //external stuff
        onDrop.Invoke(droppedByHand);
    }

    private void OnDestroy()
    {
        onDestroy.Invoke();
    }
}