using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;
using Microsoft.Practices.Unity.Utility;
using QL4BIMprimitives;
using PairTriangleMesh = Microsoft.Practices.Unity.Utility.Pair<QL4BIMspatial.TriangleMesh, QL4BIMspatial.TriangleMesh>;

namespace QL4BIMspatial
{   
    //no compile
    public class EqualOperator : IEqualOperator
    {
        private readonly IPointSampler pointSampler;
        private readonly ISpatialRepository spatialRepository;
        private readonly ISettings settings;

        public EqualOperator(IPointSampler pointSampler, ISpatialRepository spatialRepository, ISettings settings)
        {
            this.pointSampler = pointSampler;
            this.spatialRepository = spatialRepository;
            this.settings = settings;
        }

        public IEnumerable<Tuple<TriangleMesh, TriangleMesh, double>> EqualBruteForce(IEnumerable<Pair<TriangleMesh, List<TriangleMesh>>> enumerable)
        {   

            var stopwatch1 = new Stopwatch();
            stopwatch1.Start("EqualBruteForce Version");
            var outList = new List<Tuple<TriangleMesh, TriangleMesh, double>>();
            foreach (var pair in enumerable)
            {
                var mainMesh = pair.First;
                var otherMeshes = pair.Second;

                var pairs = otherMeshes.Select(om =>
                {
                    double distanceAB;
                    var isInTreshholdAB = EqualBruteForce(mainMesh, om, out distanceAB);

                    double distanceBA;
                    var isInTreshholdBA = EqualBruteForce(om, mainMesh, out distanceBA);

                    if (isInTreshholdAB & isInTreshholdBA)
                    {
                        var dist = distanceAB * distanceAB + distanceBA * distanceBA;
                        return new Tuple<TriangleMesh, TriangleMesh, double>(mainMesh, om, dist);
                    }

                    return null;
                }).Where(p => p != null).ToList();

                var minDist = pairs.Min(t => t.Item3);
                outList.AddRange(pairs.Where(p => p.Item3 == minDist && !double.IsNaN(p.Item3)));
                //outList.AddRange(pairs);
            }
            stopwatch1.Stop();
            return outList.OrderBy(t => t.Item3).Where(t => t.Item3 < 0.000000001);
        }

        public IEnumerable<Tuple<TriangleMesh, TriangleMesh, double>> EqualRTree(IEnumerable<Pair<TriangleMesh, List<TriangleMesh>>> enumerable)
        {
            var modiMeshes = spatialRepository.TriangleMeshesSet2;
            var modiMeshTuple = modiMeshes.Select(m => new Tuple<TriangleMesh, TriangleMesh, double>(m, null, 0));

            var stopwatch1 = new Stopwatch();
            stopwatch1.Start("RTree Version");

            foreach (var mesh in spatialRepository.TriangleMeshes)
            {   
                pointSampler.ResetSessionPointCount();
                foreach (var triangle in mesh.Triangles)
                    pointSampler.DistributePoints(triangle, settings.Equal.SamplePerSquareMeter);

                mesh.SampleCount = pointSampler.SessionPointCount;

            }

            var outList = new List<Tuple<TriangleMesh, TriangleMesh, double>>();
            foreach (var pair in enumerable)
            {
                var mainMesh = pair.First;
                var otherMeshes = pair.Second;

                var pairs = otherMeshes.Select(om =>
                {

                    double distanceAB;
                    int numberOfUnmatchedAB;
                    var isInTreshholdAB = EqualRTree(mainMesh, om, out distanceAB, out numberOfUnmatchedAB);
     
                    double distanceBA;
                    int numberOfUnmatchedBA;
                    var isInTreshholdBA = EqualRTree(om, mainMesh, out distanceBA, out numberOfUnmatchedBA);


                    if (isInTreshholdAB & isInTreshholdBA)
                    {   
                        var dist = distanceAB*distanceAB + distanceBA*distanceBA +
                                   (numberOfUnmatchedAB*numberOfUnmatchedAB) + 
                                   (numberOfUnmatchedBA*numberOfUnmatchedBA);
                        return new Tuple<TriangleMesh, TriangleMesh, double>(mainMesh, om, dist);
                    }

                    return null;
                }).Where(p => p != null && !double.IsNaN(p.Item3)).ToList();

                var minDist = pairs.Min(t => t.Item3);

                var tempList = pairs.Where(p => Math.Abs(p.Item3 - minDist) < 0.000000001).ToList();
                outList.AddRange(tempList);
                //outList.AddRange(pairs);


            }
            stopwatch1.Stop();
            return outList.OrderBy(t => t.Item3).Where(t => Math.Abs(t.Item3) < 0.000000001);
        }

        public bool EqualBruteForce(TriangleMesh meshA, TriangleMesh meshB, out double meanDist)
        {
            var box1 = meshA.Bounds;
            var box2 = meshB.Bounds;


            //todo
            if (!box1.Offset(settings.Equal.GlobalThreshold).Intersects(box2)) // BBs weiter als threshold weg
            {
                meanDist = 0;

                return false;
            }

            var triAToTrisBMinDistBounds = new List<Tuple<Triangle, List<Tuple<Triangle, double>>>>();
            foreach (var triangle1 in meshA.Triangles)
            {
                var triABox = triangle1.Bounds;
                var tupleList = new List<Tuple<Triangle, double>>();
                foreach (var triangle2 in meshB.Triangles)
                {
                    var minSqrDist = Box.BoxDistanceMin(triABox, triangle2.Bounds);
                    tupleList.Add(new Tuple<Triangle, double>(triangle2, minSqrDist));
                }
                triAToTrisBMinDistBounds.Add(new Tuple<Triangle, List<Tuple<Triangle, double>>>(triangle1, tupleList));
            }

            var triAWithTriBsSampleCandidates = new List<Tuple<Triangle, List<Triangle>>>();
            foreach (var tuple in triAToTrisBMinDistBounds)
            {
                var triABox = tuple.Item1.Bounds;
                var triBwithDistanceList = tuple.Item2;

                var minSqrDist = triBwithDistanceList.Min(t => t.Item2);

                var minSqrBTris = triBwithDistanceList.Where(t => t.Item2 == minSqrDist).ToList();

                var maxSqrDistFromCandidates = minSqrBTris.Select(t => Box.BoxDistanceMax(triABox, t.Item1.Bounds)).Max();

                var triBCandidates = triBwithDistanceList.Where(t => t.Item2 < maxSqrDistFromCandidates);

                triAWithTriBsSampleCandidates.Add(new Tuple<Triangle, List<Triangle>>(tuple.Item1, triBCandidates.Select(t => t.Item1).ToList()));
            }


            var distances = new List<double>();
            //iteriere über dreiecke von A
            foreach (var tuple in triAWithTriBsSampleCandidates)
            {
                var triA = tuple.Item1;

                //sampling auf A dreieck
                pointSampler.DistributePoints(triA, settings.Equal.SamplePerSquareMeter);

                var bTrianglesCandidates = tuple.Item2;

                //iteriere über sample points von A
                foreach (var sample in triA.SamplingPoints)
                {
                    var distancesOverTris2 = new List<double>();




                    foreach (var triangle2 in bTrianglesCandidates)
                    {
                        // sammle alle sqr dists über alle triangle candidaten von B
                        distancesOverTris2.Add(triangle2.MinSqrDistance(sample));
                    }

                    //minimale distance über triangle candidaten von B
                    var distanceOT2Min = distancesOverTris2.Minimum();

                    //minimale Distanz für Sampling über B Candidaten
                    distances.Add(distanceOT2Min);
                }
            }

            meanDist = Math.Sqrt(distances.Mean());
            return true;
        }

        public bool EqualBruteForceOri(TriangleMesh meshA, TriangleMesh meshB, out double meanDist)
        {
            var box1 = meshA.Bounds;
            var box2 = meshB.Bounds;


            //todo
            if (!box1.Offset(settings.Equal.GlobalThreshold).Intersects(box2)) // BBs weiter als threshold weg
            {
                meanDist = 0;

                return false;
            }

            var triAToTrisBwithMinDist = new List<Tuple<Triangle, List<Tuple<Triangle, double>>>>();
            foreach (var triangle1 in meshA.Triangles)
            {
                var tupleList = new List<Tuple<Triangle, double>>();
                foreach (var triangle2 in meshB.Triangles)
                {
                    var minSqrDist = triangle1.MinSqrDistance(triangle2);
                    tupleList.Add(new Tuple<Triangle, double>(triangle2, minSqrDist));
                }
                triAToTrisBwithMinDist.Add(new Tuple<Triangle, List<Tuple<Triangle, double>>>(triangle1, tupleList));
            }

            var triAWithTriBsSampleCandidates = new List<Tuple<Triangle, List<Triangle>>>();
            foreach (var tuple in triAToTrisBwithMinDist)
            {
                var triA = tuple.Item1;
                var triBwithDistanceList = tuple.Item2;

                var minSqrDist = triBwithDistanceList.Min(t => t.Item2);

                var minSqrBTris = triBwithDistanceList.Where(t => t.Item2 == minSqrDist).ToList();

                var maxSqrDistFromCandidates = minSqrBTris.Select(t => t.Item1.MaxSqrDistance(triA)).Max();

                var triBCandidates = triBwithDistanceList.Where(t => t.Item2 < maxSqrDistFromCandidates);

                triAWithTriBsSampleCandidates.Add(new Tuple<Triangle, List<Triangle>>(triA, triBCandidates.Select(t => t.Item1).ToList()));
            }


            var distances = new List<double>();
            //iteriere über dreiecke von A
            foreach (var tuple in triAWithTriBsSampleCandidates)
            {
                var triA = tuple.Item1;

                //sampling auf A dreieck
                pointSampler.DistributePoints(triA, settings.Equal.SamplePerSquareMeter);

                var treeBTrianglesCandidates = tuple.Item2;

                //iteriere über sample points von A
                foreach (var sample in triA.SamplingPoints)
                {
                    var distancesOverTris2 = new List<double>();
                    foreach (var triangle2 in treeBTrianglesCandidates)
                    {
                        // sammle alle sqr dists über alle triangle candidaten von B
                        distancesOverTris2.Add(triangle2.MinSqrDistance(sample));
                    }

                    //minimale distance über triangle candidaten von B
                    var distanceOT2Min = distancesOverTris2.Minimum();

                    //minimale Distanz für Sampling über B Candidaten
                    distances.Add(distanceOT2Min);
                }
            }

            meanDist = Math.Sqrt(distances.Mean());
            return true;
        }

        public bool EqualRTree(TriangleMesh meshA, TriangleMesh meshB,  out double meanDist, out int numberOfUnmatched)
        {
            var box1 = meshA.Bounds;
            var box2 = meshB.Bounds;


            var threshold = settings.Equal.GlobalThreshold;
            var thresholdSqr = threshold*threshold;
            numberOfUnmatched = 0;

            //todo
            if (!box1.Offset(threshold).Intersects(box2)) // BBs weiter als threshold weg
            {
                meanDist = 0;
                return false;
            }
                
            var distances = new List<double>();
            //iteriere über dreiecke von A

            foreach (var triangle1 in meshA.Triangles)
            {   


                //such andere dreiecke innerhalb threshold
                var tree1BoxExtended = triangle1.Bounds.Offset(threshold); 
                var tree2TrianglesCandidates = meshB.RTreeRoot.FindOverlap(tree1BoxExtended).ToList();

                //iteriere über sample points von A
                foreach (var sample in triangle1.SamplingPoints)
                {   
                    var distancesOverTris2 = new List<double>();
                    foreach (var triangle2 in tree2TrianglesCandidates)
                    {
                        // sammle alle sqr dists über alle triangle candidaten von B
                        distancesOverTris2.Add(triangle2.MinSqrDistance(sample));
                    }

                    //minimale distance über triangle candidaten von B
                    var distanceOT2Min = distancesOverTris2.Minimum();


                    //ggf keine triangle candidaten von B
                    var distanceOT2Count = distancesOverTris2.Count;

                    //wenn keine candidaten von B oder minimale Distanz größer als Threshold 
                    //auf thresshold setzen
                    if (distanceOT2Count == 0 || distanceOT2Min > thresholdSqr)
                        numberOfUnmatched++;
                    else
                    {
                        //minimale Distanz für Sampling über B Candidaten
                        distances.Add(Math.Sqrt(distanceOT2Min));
                    }

                    if (numberOfUnmatched > meshA.SampleCount/10)
                    {
                        meanDist = 0;
                        return false;
                    }
                }

            }

            meanDist = distances.Mean();
            return true;
        }

    }
}
