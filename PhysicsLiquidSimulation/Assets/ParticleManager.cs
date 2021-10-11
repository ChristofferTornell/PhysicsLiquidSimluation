using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] List<Particle> particles = new List<Particle>();
    [SerializeField] List<Floor> floors = new List<Floor>();
    [SerializeField] List<PhysicsBody> bodies = new List<PhysicsBody>();
    [SerializeField] float smoothingWidth;
    [SerializeField] float particleRadius = 0.5f;
    [SerializeField] float restDensity = 0.5f;
    [SerializeField] float plasticity;
    [SerializeField] float yieldRatio;
    [SerializeField] float particleViscosity;
    [SerializeField] float particleViscosity2;
    [SerializeField] float particleViscosity3;
    Spring[,] springs;
    [SerializeField] float springK;
    [SerializeField] [Range(0, 1)] float stiffness = 1f;
    [SerializeField] [Range(0, 1)] float bounciness;
    public static float inverseTimestep;
    [SerializeField] Vector3 gravity;
    private void OnValidate() {
        particles.Clear();
        particles.AddRange(FindObjectsOfType<Particle>());

        floors.Clear();
        floors.AddRange(FindObjectsOfType<Floor>());
    }
    private void Start() {
        bodies.AddRange(particles);
        bodies.AddRange(floors);
        inverseTimestep = 1 / Time.fixedDeltaTime;
        InitSprings();
    }

    private void InitSprings() {
        springs = new Spring[particles.Count, particles.Count];
        //for (int i = 0; i < springs.GetLength(0); i++) {
        //    for (int j = 0; j < springs.GetLength(1); j++) {
        //        Spring s = springs[i, j];
        //        if (i == j) {
        //            continue;
        //        }
        //        s.RestLength = springRestLength;
        //        s.K = springK;
        //        s.ParticleA = particles[i];
        //        s.ParticleB = particles[j];
        //    }
        //}
    }

    private void FixedUpdate() {
        //AdjustSprings();
        //ApplySpringDisplacements();

        ApplyGravity();
        ApplyViscosity();
        HandleCollisions();
        MoveParticles();
        AdjustSprings();
        ApplySpringDisplacements();
        DoubleDensityRelaxation();
        ResolveCollisions();
        //HandleCollisions();
        UpdateVelocity();
    }

    private void ResolveCollisions() {
        //foreach (PhysicsBody pB in bodies) {
        //    pB.storedBodyPosition = pB.transform.position;
        //    pB.transform.position += pB.velocity;
        //    //clear force and torque buffers
        //
        //}
    }
    private void ApplyGravity() {
        foreach (Particle p in particles) {
            p.v += gravity * Time.fixedDeltaTime;
        }
    }
    private void ApplyViscosity() {
        for (int i = 0; i < particles.Count; i++) {
            Particle p = particles[i];
            for (int j = particles.Count - 1; j > i; j--) {
                Particle pCompare = particles[j];
                if (p != pCompare) {
                    float dist = Vector3.Distance(p.Position, pCompare.Position);
                    float q = dist / smoothingWidth;
                    if (q >= 1) {
                        continue;
                    }
                    Vector3 u = (p.v - pCompare.v) * dist;
                    if (u.magnitude > 0) {
                        Vector3 impulse = Time.fixedDeltaTime * (1 - q) * (particleViscosity2 * u + particleViscosity3 * u) * dist;
                        pCompare.v -= impulse / 2;
                        pCompare.v += impulse / 2;

                    }
                }
            }
        }
    }
    private void MoveParticles() {
        foreach (Particle p in particles) {
            p.previousPos = p.transform.position;
            p.transform.position += Time.fixedDeltaTime * p.v;
        }
    }

    private void AdjustSprings() {
        for (int i = 0; i < particles.Count; i++) {
            Particle p = particles[i];
            for (int j = particles.Count - 1; j > i; j--) {
                Particle pCompare = particles[j];
                if (p != pCompare) {
                    float dist = Vector3.Distance(p.Position, pCompare.Position);
                    float q = dist / smoothingWidth;
                    if (q < 1) {
                        if (springs[i, j] == null) {
                            springs[i, j] = new Spring(smoothingWidth, springK, p, pCompare);
                        }
                        Spring s = springs[i, j];
                        float d = yieldRatio * s.RestLength;
                        if (s.DistanceBetweenParticles > s.RestLength + d) {
                            s.RestLength += Time.fixedDeltaTime * plasticity * (dist - s.RestLength - d);
                        } else if (s.DistanceBetweenParticles < s.RestLength - d) {
                            s.RestLength -= Time.fixedDeltaTime * plasticity * (s.RestLength - d - dist);
                        }
                    }
                }
            }
        }
        for (int i = 0; i < springs.GetLength(0); i++) {
            for (int j = 0; j < springs.GetLength(1); j++) {
                Spring s = springs[i, j];
                if (s != null && s.RestLength > smoothingWidth) {
                    springs[i, j] = null;
                }
            }
        }
    }
    private void ApplySpringDisplacements() {
        for (int i = 0; i < springs.GetLength(0); i++) {
            for (int j = 0; j < springs.GetLength(1); j++) {
                Spring s = springs[i, j];
                if (s != null) {
                    //Vector3 springDisplacement = Time.fixedDeltaTime * Time.fixedDeltaTime * s.K * (1 - s.RestLength / smoothingWidth)
                    //     * (s.RestLength - s.DistanceBetweenParticles) * (s.ParticleB.Position - s.ParticleA.Position).normalized;
                    Vector3 springDisplacement = s.K * (1 - s.RestLength / smoothingWidth)
                         * (s.RestLength - s.DistanceBetweenParticles) * (s.ParticleB.Position - s.ParticleA.Position).normalized;
                    s.ParticleA.transform.position -= springDisplacement * 0.5f;
                    s.ParticleB.transform.position += springDisplacement * 0.5f;
                    //Debug.Log("displacement: " + springDisplacement);
                }
            }
        }
    }

    //private void Incrompessability() {
    //    for (int i = 0; i < particles.Count; i++) {
    //        Particle p = particles[i];
    //        for (int j = particles.Count - 1; j > i; j--) {
    //            Particle particleToCompare = particles[j];
    //            if (p != particleToCompare) {
    //                float dist = Vector3.Distance(p.Position, particleToCompare.Position);
    //                float q = dist / smoothingWidth;
    //                if (dist > smoothingWidth * 2) {
    //                    continue;
    //                }
    //                float density = q * q;
    //                float nearDensity = density * q;
    //                Vector3 incrompressibility = Time.fixedDeltaTime * Time.fixedDeltaTime * (density * q + nearDensity * density) * (particleToCompare.Position - p.Position).normalized;
    //                p.transform.position += incrompressibility;
    //                //currentParticle.Pressure = particleMass * GaussianKernel(dist, smoothingWidth);
    //            }
    //        }
    //    }
    //
    //}
    private void UpdateForces() {
        foreach (Particle p in particles) {
            p.forceVelocity = AccelerationSum(p) * Time.fixedDeltaTime;
        }
    }
    private void HandleCollisions() {
        for (int i = 0; i < particles.Count; i++) {
            Particle p = particles[i];
            //p.collisionVelocity = Vector3.zero;
            //for (int j = particles.Count - 1; j > i; j--) {
            //    Particle pCompare = particles[j];
            //    if (Vector3.Distance(p.Position, pCompare.Position) <= particleRadius*2) {
            //        CollisionHit(p, pCompare);
            //    }
            //}
            for (int j = 0; j < floors.Count; j++) {
                Floor floor = floors[j];
                if (CheckCollisionHit(p, floor, out Vector3? normal)){
                    p.v -= (1 + bounciness) * (Vector3)normal * Vector3.Dot((Vector3)normal, p.v);
                }
            }

        }
    }

    private bool CheckCollisionHit(Particle p, Floor f, out Vector3? normal) {
        normal = null;
        float pOffsetR = p.transform.position.x + particleRadius;
        float pOffsetU = p.transform.position.y + particleRadius;
        float pOffsetL = p.transform.position.x - particleRadius;
        float pOffsetD = p.transform.position.y - particleRadius;

        float fHalfWidth = f.transform.localScale.x * 0.5f;
        float fHalfHeight = f.transform.localScale.y * 0.5f;

        float fOffsetR = f.xPos + fHalfWidth;
        float fOffsetU = f.yPos + fHalfHeight;
        float fOffsetL = f.xPos - fHalfWidth;
        float fOffsetD = f.yPos - fHalfHeight;


        float edgeDistanceFromLeftEdge = Mathf.Abs(pOffsetR - fOffsetL);
        float edgeDistanceFromRightEdge = Mathf.Abs(pOffsetL - fOffsetR);
        float edgeDistanceFromDownEdge = Mathf.Abs(pOffsetU - fOffsetD);
        float edgeDistanceFromUpEdge = Mathf.Abs(pOffsetD - fOffsetU);
        if (pOffsetR > fOffsetL && pOffsetL < fOffsetR &&  //Hit horizontal bounds
            pOffsetU > fOffsetD && pOffsetD < fOffsetU) { //Hit up edge
            float[] distances = new float[] { Mathf.Abs(edgeDistanceFromLeftEdge), Mathf.Abs(edgeDistanceFromRightEdge), Mathf.Abs(edgeDistanceFromDownEdge), Mathf.Abs(edgeDistanceFromUpEdge) };
            int smallest = Array.IndexOf(distances, distances.Min());
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
            return true;
        }
        return false;
    }

    private void UpdateVelocity() {
        foreach (Particle p in particles) {
            p.v = (p.transform.position - p.previousPos)/Time.fixedDeltaTime;
        }
    }
    public void DoubleDensityRelaxation() {
        for (int i = 0; i < particles.Count; i++) {
            Particle p = particles[i];
            p.Density = 0;
            p.NearDensity = 0;
            for (int j = 0; j < particles.Count; j++) {
                Particle particleToCompare = particles[j];
                if (p != particleToCompare) {
                    float dist = Vector3.Distance(p.Position, particleToCompare.Position);
                    if (dist > smoothingWidth * 2) {
                        continue;
                    }
                    float _d = (1 - dist / smoothingWidth);
                    if (_d < 1) {
                        p.Density += _d * _d;
                        p.NearDensity += p.Density * _d;
                    }
                }
            }
            p.Pressure = stiffness * (p.Density - restDensity);
            p.NearPressure = stiffness * p.NearDensity;
            Vector3 deltaPos = Vector3.zero;
            for (int j = 0; j < particles.Count; j++) {
                Particle particleToCompare = particles[j];
                if (p != particleToCompare) {
                    float dist = Vector3.Distance(p.Position, particleToCompare.Position);
                    if (dist > smoothingWidth * 2) {
                        continue;
                    }
                    float _d = (1 - dist / smoothingWidth);
                    if (_d < 1) {
                        p.Density += _d * _d;
                        p.NearDensity += p.Density * _d;
                    }
                    Vector3 incrompressibility = Time.fixedDeltaTime * Time.fixedDeltaTime * (p.Pressure * _d + p.NearPressure * _d*_d) * (particleToCompare.Position - p.Position).normalized;
                    particleToCompare.transform.position += incrompressibility * 0.5f;
                    deltaPos -= incrompressibility*0.5f;
                }
            }
            p.transform.position += deltaPos;

            //for (int j = particles.Count-1; j > i; j--) {
            //    Particle currentParticle = particles[j];
            //    if (currentParticle != particleToCompare) {
            //        float dist = Vector3.Distance(currentParticle.Position, particleToCompare.Position);
            //        if (dist > smoothingWidth*2) {
            //            continue;
            //        }
            //        float _d = (1 - dist / smoothingWidth);
            //        float density = _d * _d;
            //        float nearDensity = density * _d;
            //        Vector3 incrompressibility = Time.fixedDeltaTime * Time.fixedDeltaTime * (density * _d + nearDensity * density) * (particleToCompare.Position - currentParticle.Position).normalized;
            //        currentParticle.transform.position += incrompressibility;
            //        //currentParticle.Pressure = particleMass * GaussianKernel(dist, smoothingWidth);
            //    }
            //}
        }
    }
    public float GaussianKernel(float r, float h) {
        return (1 / (Mathf.Pow((float)Math.PI, 1.5f) * h * h * h)) * Mathf.Pow((float)Math.E, (r * r) / (h * h));
    }
    private Vector3 AccelerationSum(Particle p) {
        return new Vector3(0, 0)
            //+ new Vector3(particleViscosity, particleViscosity, 0)
            + gravity
            + p.ExternalAcceleration
            ;
    }
    private void CollisionHit(Particle pA, Particle pB) {
        Vector3 preVelocityA = pA.collisionVelocity;
        Vector3 preVelocityB = pB.collisionVelocity;

        pA.v = ((1 - bounciness) * preVelocityA + (1 + bounciness) * preVelocityB) * 0.5f;
        pB.v = pA.collisionVelocity + bounciness * (preVelocityA - preVelocityB);
    }
}