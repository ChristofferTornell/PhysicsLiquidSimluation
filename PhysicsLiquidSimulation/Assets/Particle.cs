using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
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
            return collisionVelocity * ParticleManager.inverseTimestep;
        }
    }
    public Vector3 ImaginaryPosition
    {
        get {
            return Position + velocity;
        }
    }
    [HideInInspector] public Vector3 collisionVelocity;
    [HideInInspector] public Vector3 velocity;
    float density;
    [HideInInspector] public float Pressure;
    Color color;
    float force;
    int id;
}
