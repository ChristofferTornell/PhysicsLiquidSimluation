using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] List<Particle> particles = new List<Particle>();
    [SerializeField] List<Floor> floors = new List<Floor>();
    [SerializeField] float smoothingWidth;
    [SerializeField] float particleSize;
    [SerializeField] float particleMass;
    [SerializeField] float particleViscosity;
    [SerializeField] [Range(0,1)] float bounciness;
    public static float inverseTimestep;
    [SerializeField] Vector3 gravity;
    private void OnValidate() {
        particles.Clear();
        particles.AddRange(FindObjectsOfType<Particle>());

        floors.Clear();
        floors.AddRange(FindObjectsOfType<Floor>());
    }
    private void Start() {
        inverseTimestep = 1 / Time.fixedDeltaTime;
    }
    private void FixedUpdate() {
        UpdateForces();
        HandleCollisions();
        UpdateMovement();
    }
    private void UpdateForces() {
        CalculatePressure();
        foreach (Particle p in particles) {
            p.velocity = AccelerationSum(p) * Time.fixedDeltaTime;
        }
    }
    private void HandleCollisions() {
        for (int i = 0; i < particles.Count; i++) {
            Particle currentParticle = particles[i];
            currentParticle.collisionVelocity = Vector3.zero;
            for (int j = particles.Count - 1; j > i; j--) {
                Particle colliderToCompare = particles[j];
                if (Vector3.Distance(currentParticle.Position, colliderToCompare.Position) <= particleSize*2) {
                    CollisionHit(currentParticle, colliderToCompare);
                }
            }
            Vector3 floorCollisionVelocity = Vector3.zero;
            int hits = 0;
            float penetration = 0;
            Debug.Log("checking collision for : " + i);
            for (int j = 0; j < floors.Count; j++) {
                Floor floor = floors[i];
                if (CheckCollisionHit(currentParticle, floor, out Vector3? normal, out penetration)){
                    floorCollisionVelocity += (Vector3)normal;
                    hits++;
                }
            }
            if (hits > 0) {
                currentParticle.collisionVelocity += (floorCollisionVelocity/hits).normalized * penetration;
            }

        }
    }

    private bool CheckCollisionHit(Particle p, Floor f, out Vector3? normal, out float penetration) {
        normal = null;
        float edgeDistanceFromLeftEdge = p.transform.position.x + p.transform.localScale.x/2 - f.xPos - f.transform.localScale.x/2;
        float edgeDistanceFromRightEdge = p.transform.position.x - p.transform.localScale.x/2 - (f.xPos + f.transform.localScale.x/2);
        float edgeDistanceFromDownEdge = p.transform.position.y + p.transform.localScale.y/2 - f.yPos - f.transform.localScale.y/2;
        float edgeDistanceFromUpEdge = p.transform.position.y - p.transform.localScale.y/2 - (f.yPos + f.transform.localScale.y/2);
        if (edgeDistanceFromLeftEdge <= 0 || edgeDistanceFromRightEdge <= 0) {
            if (edgeDistanceFromDownEdge <= 0 || edgeDistanceFromUpEdge <= 0) {
                float[] distances = new float[] { Mathf.Abs(edgeDistanceFromLeftEdge), Mathf.Abs(edgeDistanceFromRightEdge), Mathf.Abs(edgeDistanceFromDownEdge), Mathf.Abs(edgeDistanceFromUpEdge) };
                int smallest = Array.IndexOf(distances, distances.Min());
                Debug.Log("smallest: " + smallest);
                switch (smallest) {
                    case 0:
                        normal = Vector3.left;
                        break;
                    case 1:
                        normal = Vector3.right;
                        break;
                    case 2:
                        normal = Vector3.down;
                        break;
                    case 3:
                        normal = Vector3.up;
                        break;
                }
                penetration = distances[smallest];
                return true;
            }
        }
        penetration = 0;
        return false;
    }

    private void UpdateMovement() {
        foreach (Particle p in particles) {
            p.transform.position += p.velocity;
        }
    }
    public void CalculatePressure() {
        for (int i = 0; i < particles.Count; i++) {
            Particle particleToCompare = particles[i];
            for (int j = particles.Count-1; j > i; j--) {
                Particle currentParticle = particles[j];
                if (currentParticle != particleToCompare) {
                    currentParticle.Pressure = particleMass *
                        GaussianKernel(Vector3.Distance(currentParticle.Position, particleToCompare.Position), smoothingWidth);
                }
            }
        }
    }
    public float GaussianKernel(float r, float h) {
        return (1 / (Mathf.Pow((float)Math.PI, 1.5f) * h * h * h)) * Mathf.Pow((float)Math.E, (r * r) / (h * h));
    }
    private Vector3 AccelerationSum(Particle p) {
        return new Vector3(p.Pressure, p.Pressure)
            //+ new Vector3(particleViscosity, particleViscosity, 0)
            + gravity
            + p.ExternalAcceleration;
    }
    private void CollisionHit(Particle pA, Particle pB) {
        Vector3 preVelocityA = pA.velocity;
        Vector3 preVelocityB = pB.velocity;

        pA.collisionVelocity = ((1 - bounciness) * preVelocityA + (1 + bounciness) * preVelocityB) * 0.5f;
        pB.collisionVelocity = pA.collisionVelocity + bounciness * (preVelocityA - preVelocityB);
    }
}