using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


namespace QL4BIMprimitives
{
    public class Ray
    {
        /// <summary>
        ///     Creates a new ray with given starting point and direction.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="direction"></param>
        public Ray(Vector<double> start, Vector<double> direction)
        {
            Start = start;
            Direction = direction;
        }

        /// <summary>
        ///     Creates a new ray from the given starting point into the specified cartesian axis direction.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="axis"></param>
        /// <param name="dir"></param>
        public Ray(Vector<double> start, Axis axis, AxisDirection dir)
        {
            Start = start;

            int val = (dir == AxisDirection.Positive) ? 1 : -1;
            Direction = DenseVector.Create(3, i => ((Axis) i == axis) ? val : 0);
        }

        /// <summary>
        ///     Gets the ray's start point.
        /// </summary>
        public Vector<double> Start { get; private set; }

        /// <summary>
        ///     Gets the direction in which the ray points.
        /// </summary>
        public Vector<double> Direction { get; private set; }
    }
}