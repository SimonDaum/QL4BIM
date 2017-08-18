using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace QL4BIMprimitives
{
    public static class VectorExtensions
    {
        public static Vector<double> CrossProduct(this Vector<double> u, Vector<double> v)
        {
            if (u.Count != 3 || v.Count != 3)
                throw new ArgumentException("The cross product is only valid for 3 dimensional vectors.");

            return new DenseVector(new[]
                                   {
                                       u[1]*v[2] - u[2]*v[1],
                                       u[2]*v[0] - u[0]*v[2],
                                       u[0]*v[1] - u[1]*v[0]
                                   });
        }
    }
}