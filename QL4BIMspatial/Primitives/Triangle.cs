using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace QL4BIMspatial
{
    public class Triangle : IHasBounds
    {
        private readonly int aIndex;
        private readonly int bIndex;
        private readonly int cIndex;
        private double area;
        private Box bounds;
        private Point center;
        private Vector<double> normal;
        private Vector<double> vecAB, vecAC, vecBC;
        private double fA00;
        private double fA01;
        private double fA11;
        private double fDet;
        private List<Vector<double>> samplingPoints = new List<Vector<double>>();
        private static int instanceCount;


        public Triangle(Point a, Point b, Point c, int aIndex, int bIndex, int cIndex): this (a, b, c)
        {
            this.aIndex = aIndex;
            this.bIndex = bIndex;
            this.cIndex = cIndex;
        }


        public Triangle(Point a, Point b, Point c)
        {
            A = a;
            B = b;
            C = c;
            instanceCount++;
            Id = instanceCount;
            PopulateFields();
        }

        /// <summary>
        ///     Gets the triangle's first point.
        /// </summary>
        public Point A { get; private set; }

        /// <summary>
        ///     Gets the triangle's second point.
        /// </summary>
        public Point B { get; private set; }

        /// <summary>
        ///     Gets the triangle's third point.
        /// </summary>
        public Point C { get; private set; }

        /// <summary>
        ///     Gets the triangle's center point.
        /// </summary>
        public Point Center
        {
            get { return center; }
        }

        /// <summary>
        ///     Gets a vector representation of the edge between A and B.
        /// </summary>
        public Vector<double> AB
        {
            get
            {
                //PopulateFields();

                return vecAB;
            }
        }

        /// <summary>
        ///     Gets a vector representation of the edge between A and C.
        /// </summary>
        public Vector<double> AC
        {
            get
            {
                //PopulateFields();

                return vecAC;
            }
        }

        /// <summary>
        ///     Gets a vector representation of the edge between B and C.
        /// </summary>
        public Vector<double> BC
        {
            get
            {
                //PopulateFields();

                return vecBC;
            }
        }

        /// <summary>
        ///     Gets a enumerable representation of the triangle's points in the following order: A, B, C.
        /// </summary>
        public IEnumerable<Point> Vertices
        {
            get
            {
                yield return A;
                yield return B;
                yield return C;
            }
        }

        /// <summary>
        ///     Gets a enumerable representation of the triangle's edges in the following order: AB, AC, BC.
        /// </summary>
        public IEnumerable<Vector<double>> Edges
        {
            get
            {
                PopulateFields();
                yield return vecAB;
                yield return vecAC;
                yield return vecBC;
            }
        }

        /// <summary>
        ///     Gets the triangle's unit normal vector.
        /// </summary>
        public Vector<double> Normal
        {
            get
            {
                //PopulateFields();

                return normal;
            }
        }

        /// <summary>
        ///     Gets the triangle's area.
        /// </summary>
        public double Area
        {
            get
            {
                //PopulateFields();

                return area;
            }
        }

        /// <summary>
        ///     Gets the smallest axis aligned bounding box containing this triangle.
        /// </summary>
        public Box Bounds
        {
            get
            {
                //PopulateFields();

                return bounds;
            }
        }

        public List<Vector<double>> SamplingPoints
        {
            get { return samplingPoints; }
            set { samplingPoints = value; }
        }

        public int Id { get; private set; }

        public static void Test()
        {
            var A = new Point(-45.76, 57.763, -53.717);
            var B = new Point(45.76, 48.693, 53.717);
            var C = new Point(50.358, 31.678, -44.718);
            var triangle = new Triangle(A, B, C);

            var pointList = new List<Point>();

            pointList.Add( new Point(183.039, 35.087, 214.867)); //N0
            pointList.Add(new Point(194.535, -7.448, -31.219)); //N01
            pointList.Add(new Point(-183.039, 71.368, -214.867)); //N2
            pointList.Add(new Point(-10.072, 57.538, 8.944));//N3
            pointList.Add(new Point(10.072, 48.918, -8.944));//N4
            pointList.Add(new Point(66.823, 36.67, 16.143));//N5
            pointList.Add(new Point(3.679, 39.616, -78.748));//N6
            pointList.Add(new Point(14.153, 63.172, -11.217));//N7

            var i = 0;
            foreach (var point in pointList)
            {   
              Debug.WriteLine("N" + i++ +": " + triangle.MinSqrDistance(point.Vector));
            }

        }

        private void PopulateFields()
        {
            // calculates the dependent fields after the points have changed:
            vecAB = B.Vector - A.Vector; //edge0
            vecAC = C.Vector - A.Vector; //edge1
            vecBC = C.Vector - B.Vector;
            center = new Point((A.Vector + B.Vector + C.Vector).Divide(3.0) as DenseVector);
            normal = vecAB.CrossProduct(vecAC);
            area = normal.Norm(2);
            normal = normal.Normalize(2);

            fA00 = LengthSq(vecAB);
            fA01 = vecAB.DotProduct(vecAC);
            fA11 = LengthSq(vecAC);
            fDet = Math.Abs(fA00 * fA11 - fA01 * fA01);

            bounds = new Box(Interval.Union(Vertices.Select(p => p.X)),Interval.Union(Vertices.Select(p => p.Y)), Interval.Union(Vertices.Select(p => p.Z)));

        }

        private double LengthSq(Vector<double> a)
        {
            return a[0]*a[0] + a[1]*a[1] + a[2]*a[2];
        }

        private double SqrDistance(Vector<double> pointA, Vector<double> pointB)
        {
            return pointA[0]*pointB[0] + pointA[1]*pointB[1] + pointA[2]*pointB[2];
        }

        public double MaxSqrDistance(Triangle triangle)
        {
            var dist = new double[9];

            if (this == triangle)
                throw new Exception();

            dist[0] = SqrDistance(A.Vector, triangle.A.Vector);
            dist[1] = SqrDistance(A.Vector, triangle.B.Vector);
            dist[2] = SqrDistance(A.Vector, triangle.C.Vector);

            dist[3] = SqrDistance(B.Vector, triangle.A.Vector);
            dist[4] = SqrDistance(B.Vector, triangle.B.Vector);
            dist[5] = SqrDistance(B.Vector, triangle.C.Vector);

            dist[6] = SqrDistance(C.Vector, triangle.A.Vector);
            dist[7] = SqrDistance(C.Vector, triangle.B.Vector);
            dist[8] = SqrDistance(C.Vector, triangle.C.Vector);

            return dist.Max();
        }


        public double MinSqrDistance(Triangle triangle)
        {
            var dist = new double[6];

            if(this == triangle)
                throw new Exception();

            dist[0] = MinSqrDistance(triangle.A.Vector);
            dist[1] = MinSqrDistance(triangle.B.Vector);
            dist[2] = MinSqrDistance(triangle.C.Vector);

            dist[3] = triangle.MinSqrDistance(A.Vector);
            dist[4] = triangle.MinSqrDistance(B.Vector);
            dist[5] = triangle.MinSqrDistance(C.Vector);

            return dist.Min();
        }

        public double MinSqrDistance(Vector<double> pt)
        {
            double fSqrDist;
	        var kDiff = A.Vector - pt;
	        var fB0 = kDiff.DotProduct(vecAB); 
	        var fB1 = kDiff.DotProduct(vecAC);
            var fC = LengthSq(kDiff);
	        double fS = fA01*fB1-fA11*fB0;
	        double fT = fA01*fB0-fA00*fB1;


	        if ( fS + fT <= fDet )
	        {
		        if ( fS < (float)0.0 )
		        {
			        if ( fT < (float)0.0 )  // region 4
			        {
				        if ( fB0 < (float)0.0 )
				        {
					        fT = (float)0.0;
					        if ( -fB0 >= fA00 )
					        {
						        fS = (float)1.0;
						        fSqrDist = fA00+((float)2.0)*fB0+fC;
					        }
					        else
					        {
						        fS = -fB0/fA00;
						        fSqrDist = fB0*fS+fC;
					        }
				        }
				        else
				        {
					        fS = (float)0.0;
					        if ( fB1 >= (float)0.0 )
					        {
						        fT = (float)0.0;
						        fSqrDist = fC;
					        }
					        else if ( -fB1 >= fA11 )
					        {
						        fT = (float)1.0;
						        fSqrDist = fA11+((float)2.0)*fB1+fC;
					        }
					        else
					        {
						        fT = -fB1/fA11;
						        fSqrDist = fB1*fT+fC;
					        }
				        }
			        }
			        else  // region 3
			        {
				        fS = (float)0.0;
				        if ( fB1 >= (float)0.0 )
				        {
					        fT = (float)0.0;
					        fSqrDist = fC;
				        }
				        else if ( -fB1 >= fA11 )
				        {
					        fT = (float)1.0;
					        fSqrDist = fA11+((float)2.0)*fB1+fC;
				        }
				        else
				        {
					        fT = -fB1/fA11;
					        fSqrDist = fB1*fT+fC;
				        }
			        }
		        }
		        else if ( fT < (float)0.0 )  // region 5
		        {
			        fT = (float)0.0;
			        if ( fB0 >= (float)0.0 )
			        {
				        fS = (float)0.0;
				        fSqrDist = fC;
			        }
			        else if ( -fB0 >= fA00 )
			        {
				        fS = (float)1.0;
				        fSqrDist = fA00+((float)2.0)*fB0+fC;
			        }
			        else
			        {
				        fS = -fB0/fA00;
				        fSqrDist = fB0*fS+fC;
			        }
		        }
		        else  // region 0
		        {
			        // minimum at interior point
			        double fInvDet = 1.0/fDet;
			        fS *= fInvDet;
			        fT *= fInvDet;
			        fSqrDist = fS*(fA00*fS+fA01*fT+((float)2.0)*fB0) +
				        fT*(fA01*fS+fA11*fT+((float)2.0)*fB1)+fC;
		        }
	        }
	        else
	        {
		        double fTmp0, fTmp1, fNumer, fDenom;

		        if ( fS < (float)0.0 )  // region 2
		        {
			        fTmp0 = fA01 + fB0;
			        fTmp1 = fA11 + fB1;
			        if ( fTmp1 > fTmp0 )
			        {
				        fNumer = fTmp1 - fTmp0;
				        fDenom = fA00-2.0f*fA01+fA11;
				        if ( fNumer >= fDenom )
				        {
					        fS = (float)1.0;
					        fT = (float)0.0;
					        fSqrDist = fA00+((float)2.0)*fB0+fC;
				        }
				        else
				        {
					        fS = fNumer/fDenom;
					        fT = (float)1.0 - fS;
					        fSqrDist = fS*(fA00*fS+fA01*fT+2.0f*fB0) +
						        fT*(fA01*fS+fA11*fT+((float)2.0)*fB1)+fC;
				        }
			        }
			        else
			        {
				        fS = (float)0.0;
				        if ( fTmp1 <= (float)0.0 )
				        {
					        fT = (float)1.0;
					        fSqrDist = fA11+((float)2.0)*fB1+fC;
				        }
				        else if ( fB1 >= (float)0.0 )
				        {
					        fT = (float)0.0;
					        fSqrDist = fC;
				        }
				        else
				        {
					        fT = -fB1/fA11;
					        fSqrDist = fB1*fT+fC;
				        }
			        }
		        }
		        else if ( fT < (float)0.0 )  // region 6
		        {
			        fTmp0 = fA01 + fB1;
			        fTmp1 = fA00 + fB0;
			        if ( fTmp1 > fTmp0 )
			        {
				        fNumer = fTmp1 - fTmp0;
				        fDenom = fA00-((float)2.0)*fA01+fA11;
				        if ( fNumer >= fDenom )
				        {
					        fT = (float)1.0;
					        fS = (float)0.0;
					        fSqrDist = fA11+((float)2.0)*fB1+fC;
				        }
				        else
				        {
					        fT = fNumer/fDenom;
					        fS = (float)1.0 - fT;
					        fSqrDist = fS*(fA00*fS+fA01*fT+((float)2.0)*fB0) +
						        fT*(fA01*fS+fA11*fT+((float)2.0)*fB1)+fC;
				        }
			        }
			        else
			        {
				        fT = (float)0.0;
				        if ( fTmp1 <= (float)0.0 )
				        {
					        fS = (float)1.0;
					        fSqrDist = fA00+((float)2.0)*fB0+fC;
				        }
				        else if ( fB0 >= (float)0.0 )
				        {
					        fS = (float)0.0;
					        fSqrDist = fC;
				        }
				        else
				        {
					        fS = -fB0/fA00;
					        fSqrDist = fB0*fS+fC;
				        }
			        }
		        }
		        else  // region 1
		        {
			        fNumer = fA11 + fB1 - fA01 - fB0;
			        if ( fNumer <= (float)0.0 )
			        {
				        fS = (float)0.0;
				        fT = (float)1.0;
				        fSqrDist = fA11+((float)2.0)*fB1+fC;
			        }
			        else
			        {
				        fDenom = fA00-2.0f*fA01+fA11;
				        if ( fNumer >= fDenom )
				        {
					        fS = (float)1.0;
					        fT = (float)0.0;
					        fSqrDist = fA00+((float)2.0)*fB0+fC;
				        }
				        else
				        {
					        fS = fNumer/fDenom;
					        fT = (float)1.0 - fS;
					        fSqrDist = fS*(fA00*fS+fA01*fT+((float)2.0)*fB0) +
						        fT*(fA01*fS+fA11*fT+((float)2.0)*fB1)+fC;
				        }
			        }
		        }
	        }

	        fSqrDist = Math.Abs(fSqrDist);



            return fSqrDist;
        }



        Box IHasBounds.Bounds
        {
            get { return bounds; }
        }

        public int AIndex
        {
            get { return aIndex; }
        }

        public int BIndex
        {
            get { return bIndex; }
        }

        public int CIndex
        {
            get { return cIndex; }
        }
    }
}