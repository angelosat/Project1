using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class Interpolation
    {
        static public Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return a + (b - a) * t;
        }
        static public float Lerp(float a, float b, float t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return a + (b - a) * t;
        }
        static public float Sine(float a, float b, float t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            double h = (1 - Math.Cos(t * MathHelper.Pi)) / 2;
            return (float)(a * (1 - h) + b * h);
        }
        static public float Discrete(float a, float b, float t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return t >= 1 ? b : a;
        }
        static public float Exp(float a, float b, float t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            float aa = (float)(Math.Exp(2 * t) - 1) / (float)(Math.Exp(2) - 1);
            return a * (1 - aa) + b * (aa);
        }
        static public float ExpInverse(float a, float b, float t)
        {
            return Exp(b, a, 1 - t);
        }
    }
}
