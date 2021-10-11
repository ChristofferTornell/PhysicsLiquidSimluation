using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : PhysicsBody
{
    public Vector3 Position
    {
        get {
            return transform.position;
        }
    }
    public Vector3 ExternalAcceleration
    {
        get {
            return Vector3.zero;
        }
    }
    public Vector3 ImaginaryPosition
    {
        get {
            return Position + forceVelocity;
        }
    }
    [HideInInspector] public Vector3 collisionVelocity;
    [HideInInspector] public Vector3 forceVelocity;

    [HideInInspector] public Vector3 previousPos;
    [HideInInspector] public Vector3 v;
    public Vector3 velocity
    {
        get {
            return forceVelocity + collisionVelocity;
        }
    }
    [HideInInspector] public float Density;
    [HideInInspector] public float NearDensity;

    [HideInInspector] public float Pressure;
    [HideInInspector] public float NearPressure;

    Color color;
    float force;
    int id;
}
