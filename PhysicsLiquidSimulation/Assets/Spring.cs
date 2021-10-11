using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring
{
    public float RestLength;
    public float K;
    public Particle ParticleA;
    public Particle ParticleB;
    public Spring(float _restLength, float _k, Particle _pA, Particle _pB) {
        RestLength = _restLength;
        K = _k;
        ParticleA = _pA;
        ParticleB = _pB;
    }
    public float DistanceBetweenParticles
    {
        get {
            return Vector3.Distance(ParticleA.transform.position, ParticleB.transform.position);
        }
    }
}
