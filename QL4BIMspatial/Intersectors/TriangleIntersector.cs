using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public class TriangleIntersector : ITriangleIntersector
    {
        //Berühren ist hier auch intersecting!

        //#define fabs(x) (float(fabs(x)))        /* implement as is fastest on your machine */


        /* if USE_EPSILON_TEST is true then we do a check:
                 if |dv|<EPSILON then dv=0.0;
           else no check is done (which is less robust)
        */
        private const bool UseEpsilonTest = true;
        private const double Epsilon = 0.000001;
        private const double OneMinusEpsilon = 1 - Epsilon;

        public bool DoIntersect(Triangle triangleA, Triangle triangleB)
        {
            return DoIntersect(triangleA.A.Vector, triangleA.B.Vector, triangleA.C.Vector,
                triangleB.A.Vector, triangleB.B.Vector, triangleB.C.Vector);
        }

        private double fabs(double x)
        {
            return Math.Abs(x);
        }


        /* some macros */

        private Vector<double> cross(Vector<double> v1, Vector<double> v2)
        {
            var dest = new DenseVector(3);
            dest[0] = v1[1]*v2[2] - v1[2]*v2[1];
            dest[1] = v1[2]*v2[0] - v1[0]*v2[2];
            dest[2] = v1[0]*v2[1] - v1[1]*v2[0];
            return dest;
        }

        private double dot(Vector<double> v1, Vector<double> v2)
        {
            return (v1[0]*v2[0] + v1[1]*v2[1] + v1[2]*v2[2]);
        }

        private Vector<double> sub(Vector<double> v1, Vector<double> v2)
        {
            var dest = new DenseVector(3);
            dest[0] = v1[0] - v2[0];
            dest[1] = v1[1] - v2[1];
            dest[2] = v1[2] - v2[2];

            return dest;
        }

        /* sort so that a<=b */
        //sort(isect1[0],isect1[1]);
        //sort(isect2[0],isect2[1]);

        private void sort(ref Vector<double> isect)
        {
            if (isect[0] > isect[1])
            {
                double c = isect[0];
                isect[0] = isect[1];
                isect[1] = c;
            }
        }


        /* this edge to edge test is based on Franlin Antonio's gem:
           "Faster Line Segment Intersection", in Graphics Gems III,
           pp. 199-202 */

        private int EdgeEdgeTest(Vector<double> V0, Vector<double> U0, Vector<double> U1, double Ax, double Ay, int i0, int i1)
        {
            double Bx = U0[i0] - U1[i0];
            double By = U0[i1] - U1[i1];
            double Cx = V0[i0] - U0[i0];
            double Cy = V0[i1] - U0[i1];
            double f = Ay*Bx - Ax*By;   //demoninator
            double d = By*Cx - Bx*Cy; //numerator1
            if ((f > 0 && d >= 0 && d <= f) || (f < 0 && d <= 0 && d >= f))
            {
                double e = Ax*Cy - Ay*Cx;
                if (f > 0)
                {
                    if (e >= 0 && e <= f) return 1;
                }
                else
                {
                    if (e <= 0 && e >= f) return 1;
                }
            }

            return 0;
        }

        public bool EdgeAgainsEdge(DenseVector V0, DenseVector V1, DenseVector U0, DenseVector U1, int i0, int i1, out DenseVector p, out double alpha, out double beta)
        {
            double Ax = V1[i0] - V0[i0];
            double Ay = V1[i1] - V0[i1];
            double Bx = U0[i0] - U1[i0];
            double By = U0[i1] - U1[i1];
            double Cx = V0[i0] - U0[i0];
            double Cy = V0[i1] - U0[i1];
            double f = Ay * Bx - Ax * By; //denominator
            double d = By * Cx - Bx * Cy; //nominator

            p = null;

            if ((f > 0 && (d < 0 || d > f)) || (f < 0 && (d > 0 || d < f)))
            {
                alpha = 0;
                beta = 0;
                return false;
            }

            double e = Ax * Cy - Ay * Cx; //nominator 
            if ((f > 0 && (e < 0 || e > f)) || (f < 0 && (e > 0 || e < f)))
            {
                alpha = 0;
                beta = 0;
                return false;
            }

            alpha = d/f;
            if (alpha < Epsilon || alpha > OneMinusEpsilon || double.IsNaN(alpha))
            {
                alpha = 0;
                beta = 0;
                return false;
            }

            beta = e / f;
            if (beta < Epsilon || beta > OneMinusEpsilon || double.IsNaN(beta))
            {
                alpha = 0;
                beta = 0;
                return false;
            }

            p = new DenseVector(new[] {(V0[i0] + alpha*Ax), (V0[i1] + alpha*Ay)});
            return true;
        }

        public bool EdgeAgainsEdge(PolygonPoint V0, PolygonPoint V1, PolygonPoint U0, PolygonPoint U1, int i0, int i1, out PolygonPoint secPointA, out PolygonPoint secPointB)
        {   
            DenseVector pVector;
            secPointA = null;
            secPointB = null;
            double alpha, beta;
            var intersect = EdgeAgainsEdge(V0.Vector, V1.Vector, U0.Vector, U1.Vector, i0, i1, out pVector, out alpha, out beta);

            if (intersect)
            {
                secPointA = new PolygonPoint(pVector, true, alpha);
                secPointB = new PolygonPoint(pVector, true, beta);

                secPointA.Neighbor = secPointB;
                secPointB.Neighbor = secPointA;
            }

            return intersect;
        }


        public int EdgeAgainsEdge(DenseVector V0, DenseVector V1, DenseVector U0, DenseVector U1, int i0, int i1)
        {
            double Ax = V1[i0] - V0[i0];
            double Ay = V1[i1] - V0[i1];

            /* test edge U0,U1 against V0,V1 */
            if (EdgeEdgeTest(V0, U0, U1, Ax, Ay, i0, i1) == 1)
                return 1;

            return 0;
        }


        private int EdgeAgainstTriEdges(Vector<double> V0, Vector<double> V1, Vector<double> U0, Vector<double> U1, Vector<double> U2, int i0, int i1)
        {
            double Ax = V1[i0] - V0[i0];
            double Ay = V1[i1] - V0[i1];
            /* test edge U0,U1 against V0,V1 */
            if (EdgeEdgeTest(V0, U0, U1, Ax, Ay, i0, i1) == 1)
                return 1;
            /* test edge U1,U2 against V0,V1 */
            if (EdgeEdgeTest(V0, U1, U2, Ax, Ay, i0, i1) == 1)
                return 1;
            /* test edge U2,U1 against V0,V1 */
            if (EdgeEdgeTest(V0, U2, U0, Ax, Ay, i0, i1) == 1)
                return 1;

            return 0;
        }

        private int PointInTri(Vector<double> V0, Vector<double> U0, Vector<double> U1, Vector<double> U2, int i0,
            int i1)
        {
            /* is T1 completly inside T2? */
            /* check if V0 is inside tri(U0,U1,U2) */
            double a = U1[i1] - U0[i1];
            double b = -(U1[i0] - U0[i0]);
            double c = -a*U0[i0] - b*U0[i1];
            double d0 = a*V0[i0] + b*V0[i1] + c;

            a = U2[i1] - U1[i1];
            b = -(U2[i0] - U1[i0]);
            c = -a*U1[i0] - b*U1[i1];
            double d1 = a*V0[i0] + b*V0[i1] + c;

            a = U0[i1] - U2[i1];
            b = -(U0[i0] - U2[i0]);
            c = -a*U2[i0] - b*U2[i1];
            double d2 = a*V0[i0] + b*V0[i1] + c;
            if (d0*d1 > 0.0)
            {
                if (d0*d2 > 0.0) return 1;
            }

            return 0;
        }


        private int CoplanarTriTri(Vector<double> N, Vector<double> V0, Vector<double> V1, Vector<double> V2,
            Vector<double> U0, Vector<double> U1, Vector<double> U2)
        {
            var A = new DenseVector(3);
            int index0, index1;
            /* first project onto an axis-aligned plane, that maximizes the area */
            /* of the triangles, compute indices: i0,i1. */
            A[0] = fabs(N[0]);
            A[1] = fabs(N[1]);
            A[2] = fabs(N[2]);
            if (A[0] > A[1])
            {
                if (A[0] > A[2])
                {
                    index0 = 1; /* A[0] is greatest */
                    index1 = 2;
                }
                else
                {
                    index0 = 0; /* A[2] is greatest */
                    index1 = 1;
                }
            }
            else /* A[0]<=A[1] */
            {
                if (A[2] > A[1])
                {
                    index0 = 0; /* A[2] is greatest */
                    index1 = 1;
                }
                else
                {
                    index0 = 0; /* A[1] is greatest */
                    index1 = 2;
                }
            }

            /* test all edges of triangle 1 against the edges of triangle 2 */
            if (EdgeAgainstTriEdges(V0, V1, U0, U1, U2, index0, index1) == 1)
                return 1;

            if (EdgeAgainstTriEdges(V1, V2, U0, U1, U2, index0, index1) == 1)
                return 1;

            if (EdgeAgainstTriEdges(V2, V0, U0, U1, U2, index0, index1) == 1)
                return 1;

            /* finally, test if tri1 is totally contained in tri2 or vice versa */
            if (PointInTri(V0, U0, U1, U2, index0, index1) == 1)
                return 1;
            if (PointInTri(U0, V0, V1, V2, index0, index1) == 1)
                return 1;

            return 0;
        }


        private bool ComputeIntervals(double VV0, double VV1, double VV2, double D0, double D1, double D2, double D0D1,
            double D0D2,
            out double A, out double B, out double C, out double X0, out double X1)
        {
            bool result = true;
            if (D0D1 > 0.0f)
            {
                /* here we know that D0D2<=0.0 */
                /* that is D0, D1 are on the same side, D2 on the other or on the plane */
                A = VV2;
                B = (VV0 - VV2)*D2;
                C = (VV1 - VV2)*D2;
                X0 = D2 - D0;
                X1 = D2 - D1;
            }
            else if (D0D2 > 0.0f)
            {
                /* here we know that d0d1<=0.0 */
                A = VV1;
                B = (VV0 - VV1)*D1;
                C = (VV2 - VV1)*D1;
                X0 = D1 - D0;
                X1 = D1 - D2;
            }
            else if (D1*D2 > 0.0f || D0 != 0.0f)
            {
                /* here we know that d0d1<=0.0 or that D0!=0.0 */
                A = VV0;
                B = (VV1 - VV0)*D0;
                C = (VV2 - VV0)*D0;
                X0 = D0 - D1;
                X1 = D0 - D2;
            }
            else if (D1 != 0.0f)
            {
                A = VV1;
                B = (VV0 - VV1)*D1;
                C = (VV2 - VV1)*D1;
                X0 = D1 - D0;
                X1 = D1 - D2;
            }
            else if (D2 != 0.0f)
            {
                A = VV2;
                B = (VV0 - VV2)*D2;
                C = (VV1 - VV2)*D2;
                X0 = D2 - D0;
                X1 = D2 - D1;
            }
            else
            {
                /* triangles are coplanar */
                //return CoplanarTriTri(N1,V0,V1,V2,U0,U1,U2);
                A = B = C = X0 = X1 = 0;
                result = false;
            }

            return result;
        }


        private bool DoIntersect(Vector<double> V0, Vector<double> V1, Vector<double> V2,
            Vector<double> U0, Vector<double> U1, Vector<double> U2)
        {
            Vector<double> E1, E2;
            Vector<double> N1, N2;
            double d1, d2;
            double du0, du1, du2, dv0, dv1, dv2;
            Vector<double> D;
            Vector<double> isect1, isect2;
            double du0du1, du0du2, dv0dv1, dv0dv2;
            int index;
            double vp0, vp1, vp2;
            double up0, up1, up2;
            double bb, cc, max;

            /* compute plane equation of triangle(V0,V1,V2) */
            E1 = sub(V1, V0);
            E2 = sub(V2, V0);
            N1 = cross(E1, E2);
            d1 = -dot(N1, V0);
            /* plane equation 1: N1.X+d1=0 */

            /* put U0,U1,U2 into plane equation 1 to compute signed distances to the plane*/
            du0 = dot(N1, U0) + d1;
            du1 = dot(N1, U1) + d1;
            du2 = dot(N1, U2) + d1;

            /* coplanarity robustness check */
            if (UseEpsilonTest)
            {
                if (fabs(du0) < Epsilon) du0 = 0.0;
                if (fabs(du1) < Epsilon) du1 = 0.0;
                if (fabs(du2) < Epsilon) du2 = 0.0;
            }

            du0du1 = du0*du1;
            du0du2 = du0*du2;

            if (du0du1 > 0.0f && du0du2 > 0.0f) /* same sign on all of them + not equal 0 ? */
                return false; /* no intersection occurs */

            /* compute plane of triangle (U0,U1,U2) */
            E1 = sub(U1, U0);
            E2 = sub(U2, U0);
            N2 = cross(E1, E2);
            d2 = -dot(N2, U0);
            /* plane equation 2: N2.X+d2=0 */

            /* put V0,V1,V2 into plane equation 2 */
            dv0 = dot(N2, V0) + d2;
            dv1 = dot(N2, V1) + d2;
            dv2 = dot(N2, V2) + d2;

            if (UseEpsilonTest)
            {
                if (fabs(dv0) < Epsilon) dv0 = 0.0;
                if (fabs(dv1) < Epsilon) dv1 = 0.0;
                if (fabs(dv2) < Epsilon) dv2 = 0.0;
            }

            dv0dv1 = dv0*dv1;
            dv0dv2 = dv0*dv2;

            if (dv0dv1 > 0.0f && dv0dv2 > 0.0f) /* same sign on all of them + not equal 0 ? */
                return false; /* no intersection occurs */

            /* compute direction of intersection line */
            D = cross(N1, N2);

            /* compute and index to the largest component of D */
            max = (float) fabs(D[0]);
            index = 0;
            bb = (float) fabs(D[1]);
            cc = (float) fabs(D[2]);
            if (bb > max)
            {
                max = bb;
                index = 1;
            }
            if (cc > max)
            {
                max = cc;
                index = 2;
            }

            /* this is the simplified projection onto L*/
            vp0 = V0[index];
            vp1 = V1[index];
            vp2 = V2[index];

            up0 = U0[index];
            up1 = U1[index];
            up2 = U2[index];

            /* compute interval for triangle 1 */
            double a, b, c, x0, x1;
            if (!ComputeIntervals(vp0, vp1, vp2, dv0, dv1, dv2, dv0dv1, dv0dv2, out a, out b, out c, out x0, out x1))
                return CoplanarTriTri(N1, V0, V1, V2, U0, U1, U2) == 1;

            /* compute interval for triangle 2 */
            double d, e, f, y0, y1;
            if (!ComputeIntervals(up0, up1, up2, du0, du1, du2, du0du1, du0du2, out d, out e, out f, out y0, out y1))
                return CoplanarTriTri(N1, V0, V1, V2, U0, U1, U2) == 1;

            double xx = x0*x1;
            double yy = y0*y1;
            double xxyy = xx*yy;

            double tmp = a*xxyy;

            isect1 = new DenseVector(2);
            isect1[0] = tmp + b*x1*yy;
            isect1[1] = tmp + c*x0*yy;

            tmp = d*xxyy;
            isect2 = new DenseVector(2);
            isect2[0] = tmp + e*xx*y1;
            isect2[1] = tmp + f*xx*y0;

            sort(ref isect1);
            sort(ref isect2);

            if (isect1[1] < isect2[0] || isect2[1] < isect1[0])
                return false;

            return true;
        }
    }
}