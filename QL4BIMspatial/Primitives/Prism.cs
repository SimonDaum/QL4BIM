using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace QL4BIMspatial
{
    public class Prism
    {
        public Prism(Triangle baseFace, double length, Axis axis, bool positiv)
        {
            Base = baseFace;

            if (!positiv)
                length = length*-1;

            if(axis == Axis.X)
                Translation = new DenseVector(new[] { length, 0d, 0d });

            if (axis == Axis.Y)
                Translation = new DenseVector(new[] { 0d, length, 0d });

            if (axis == Axis.Z)
                Translation = new DenseVector(new[] { 0d, 0d, length });
        }

        public Triangle Base { get; private set; }

        public Vector<double> Translation { get; private set; }

        public Box Bounds
        {
            get { throw new NotImplementedException(); }
        }
    }
}