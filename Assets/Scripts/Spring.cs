// DampedSpring.cs
// Port of Ryan Juckett's damped spring formulation to modern C#
// Source derivation: https://www.ryanjuckett.com/damped-springs/

using System;
using UnityEngine;

namespace MotionUtils
{
    /// <summary>
    /// Pre‑computed coefficients that turn an O(Δt) integration step into a few multiplies.
    /// </summary>
    public readonly struct DampedSpringCoefficients
    {
        public readonly float PosPos;
        public readonly float PosVel;
        public readonly float VelPos;
        public readonly float VelVel;

        public DampedSpringCoefficients(float posPos, float posVel, float velPos, float velVel)
        {
            PosPos = posPos;
            PosVel = posVel;
            VelPos = velPos;
            VelVel = velVel;
        }
    }

    /// <summary>
    /// Utility functions for critically‑, over‑ and under‑damped numeric springs.
    /// </summary>
    public static class DampedSpring
    {
        private const float EPSILON = 1e-4f;

        /// <summary>
        /// Compute coefficients for one time step of a damped spring.
        /// </summary>
        public static DampedSpringCoefficients CalcCoefficients(float dt, float angularFrequency, float dampingRatio)
        {
            // Clamp inputs to sane values
            if (dampingRatio < 0f) dampingRatio = 0f;
            if (angularFrequency < 0f) angularFrequency = 0f;

            // Trivial case – no motion
            if (angularFrequency < EPSILON)
                return new DampedSpringCoefficients(1f, 0f, 0f, 1f);

            float posPos, posVel, velPos, velVel;

            if (dampingRatio > 1f + EPSILON)
            {
                // Over‑damped
                float za = -angularFrequency * dampingRatio;
                float zb = angularFrequency * MathF.Sqrt(dampingRatio * dampingRatio - 1f);
                float z1 = za - zb;
                float z2 = za + zb;

                float e1 = MathF.Exp(z1 * dt);
                float e2 = MathF.Exp(z2 * dt);

                float invTwoZb = 1f / (2f * zb); // == 1 / (z2 - z1)

                posPos = e1 + e2 + (z1 * e2 - z2 * e1) * invTwoZb;
                posVel = (-e1 - e2 + (e2 - e1) * invTwoZb) / angularFrequency;
                velPos = (z1 * e1 - z2 * e2) + (z2 - z1) * (z1 * e2 - z2 * e1) * invTwoZb;
                velVel = -z1 * e1 - z2 * e2 + (z2 - z1) * (e2 - e1) * invTwoZb;
            }
            else if (dampingRatio < 1f - EPSILON)
            {
                // Under‑damped
                float beta = Mathf.Sqrt(1f - dampingRatio * dampingRatio); // β
                float omegaD = angularFrequency * beta;                      // ω̂ = ωβ
                float exp = Mathf.Exp(-dampingRatio * angularFrequency * dt);
                float sin = Mathf.Sin(omegaD * dt);
                float cos = Mathf.Cos(omegaD * dt);

                posPos = exp * (cos + (dampingRatio / beta) * sin);
                posVel = exp * sin / omegaD;
                velPos = -angularFrequency * exp * sin / beta;          // ← fixed
                velVel = exp * (cos - (dampingRatio / beta) * sin);     // already correct
            }
            else
            {
                // Critically‑damped
                float exp = MathF.Exp(-angularFrequency * dt);
                float expDt = exp * dt;

                posPos = exp * (1f + angularFrequency * dt);
                posVel = expDt;
                velPos = -angularFrequency * expDt;
                velVel = exp * (1f - angularFrequency * dt);
            }

            return new DampedSpringCoefficients(posPos, posVel, velPos, velVel);
        }

        /// <summary>
        /// Advance a scalar spring by one time step.
        /// </summary>
        public static void Step(ref float position, ref float velocity, float equilibrium, float dt, float angularFrequency, float dampingRatio)
            => Step(ref position, ref velocity, equilibrium, CalcCoefficients(dt, angularFrequency, dampingRatio));

        /// <summary>
        /// Advance a scalar spring by one time step.
        /// </summary>
        public static void Step(ref float position, ref float velocity, float equilibrium, in DampedSpringCoefficients c)
        {
            float x = position - equilibrium;
            position = x * c.PosPos + velocity * c.PosVel + equilibrium;
            velocity = x * c.VelPos + velocity * c.VelVel;
        }

        /// <summary>
        /// Advance a Vector2 spring by one time step.
        /// </summary>
        public static void Step(ref Vector2 position, ref Vector2 velocity, Vector2 equilibrium, in DampedSpringCoefficients c)
        {
            Vector2 x = position - equilibrium;
            position = x * c.PosPos + velocity * c.PosVel + equilibrium;
            velocity = x * c.VelPos + velocity * c.VelVel;
        }

        /// <summary>
        /// Advance a Vector3 spring by one time step.
        /// </summary>
        public static void Step(ref Vector3 position, ref Vector3 velocity, Vector3 equilibrium, in DampedSpringCoefficients c)
        {
            Vector3 x = position - equilibrium;
            position = x * c.PosPos + velocity * c.PosVel + equilibrium;
            velocity = x * c.VelPos + velocity * c.VelVel;
        }
    }
}
