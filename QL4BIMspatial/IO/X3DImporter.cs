/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMspatial.

QL4BIMspatial is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMspatial is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMspatial. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace QL4BIMspatial
{
    public class X3DImporter : IX3DImporter
    {
        private static readonly int[,] XAxisLookup =
        {
            // first index is the up-index (always positive), second index the forward axis normalized by the up-sign 
            //    -z, -y, -x,  0, +x, +y, +z
            {2, -3, 0, 0, 0, 3, -2}, // +/- x (0)
            {-1, 0, 3, 0, -3, 0, 1}, // +/- y (1)
            {0, 1, -2, 0, 2, -1, 0} // +/- z (2)
        };

        private Tuple<int, int, int>[] currIndices;
        private string currName;
        private int currTag;
        private Matrix<double> currTransform;
        private Tuple<double, double, double>[] currVertices;
        private Tuple<double, double, double>[] currNormals;
        private Tuple<double, double, double>[] currWorlsNormals;
        private Tuple<double, double, double>[] currWorldPositions;
        private List<Tuple<string, XmlNode>> triangleSetNodeResultList;


        public X3DImporter()
            : this(2, 3)
        {
        }

        /// <summary>
        ///     For x/y/z-Axis use 1/2/3, respectively. Use -1/-2/-3 to indicate the negative axes.
        /// </summary>
        /// <param name="upAxis"></param>
        /// <param name="forwardAxis"></param>
        private X3DImporter(int upAxis, int forwardAxis)
        {
            int upSign = Math.Sign(upAxis);
            int upIndex = upSign*upAxis - 1;

            // upIndex is always positive. Normalize forwardAxis by multiplying with upSign. +3 to translate it to a zero-based array index.
            int xAxis = XAxisLookup[upIndex, upSign*forwardAxis + 3];
        }

        public List<Tuple<string, XmlNode>> TriangleSetNodeResultList
        {
            get { return triangleSetNodeResultList; }
        }

        public IndexedFaceSet Import(string path, string triangleSetName)
        {
            SnoopForTriangleSets(path);
            Tuple<string, XmlNode> indexedFaceSetAndName =
                TriangleSetNodeResultList.Single(s => s.Item1.EndsWith(triangleSetName));


            IndexedFaceSet parsedIndexedFaceSet = ParseIndexedFaceSet(indexedFaceSetAndName.Item2);

            return parsedIndexedFaceSet;
        }

        public IEnumerable<IndexedFaceSet> ImportAll(string path)
        {
            var results = new List<IndexedFaceSet>();

            SnoopForTriangleSets(path);
            foreach (var indexedFaceSetAndName in TriangleSetNodeResultList)
            {
                IndexedFaceSet parsedIndexedFaceSet = ParseIndexedFaceSet(indexedFaceSetAndName.Item2);
                results.Add(parsedIndexedFaceSet);
            }

            return results;
        }


        private void DepthFirstSearchTriangleSet(XmlNode node)
        {
            var enumerators = new Stack<IEnumerator>();
            enumerators.Push(new[] {node}.GetEnumerator());

            while (enumerators.Count > 0)
            {
                IEnumerator en = enumerators.Peek();

                if (en.MoveNext())
                {
                    node = (XmlNode) en.Current;

                    if ((node.Name == "IndexedTriangleSet") || (node.Name == "IndexedFaceSet"))
                    {
                        if (node.ParentNode != null && node.ParentNode.ParentNode != null)
                        {
                            var name = GetNameForSet(node);                            
                            TriangleSetNodeResultList.Add(new Tuple<string, XmlNode>(name, node));
                        }
                        else
                        {
                            throw new InvalidDataException();
                        }
                    }
                    else
                        enumerators.Push(node.ChildNodes.GetEnumerator());
                }
                else
                {
                    enumerators.Pop();
                }
            }
        }

        private string GetNameForSet(XmlNode node)
        {
            string name;
            var parentParentAtt = node.ParentNode.ParentNode.ParentNode.Attributes["DEF"];

            if (parentParentAtt != null)
                name = parentParentAtt.InnerText;
            else
            {
                var parentParentParentAtt = node.ParentNode.ParentNode.ParentNode.Attributes["DEF"];
                if (parentParentParentAtt != null)
                    name = parentParentParentAtt.InnerText;
                else
                    name = "NoName";
            }
            return name;
        }

        private void SnoopForTriangleSets(string path)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            XmlElement root = xmlDocument.DocumentElement;

            XmlElement element = root;

            triangleSetNodeResultList = new List<Tuple<string, XmlNode>>();
            DepthFirstSearchTriangleSet(element);
        }

        private IndexedFaceSet ParseIndexedFaceSet(XmlNode xmlNode)
        {
            currName = GetNameForSet(xmlNode);
            currTag++;
            currIndices = GetCoordIndices(xmlNode);
            currVertices = GetCoordVertices(xmlNode);
            currNormals = GetCoordNormals(xmlNode);
            currTransform = GetTransformMatrix(xmlNode);
            CreateWorldAndObjectPositions();
            return new IndexedFaceSet(currWorldPositions, currIndices, currName, currTag);
        }

        private Matrix<double> GetTransformMatrix(XmlNode node)
        {
            XmlNode parent = node.ParentNode;
            if (parent.Name != "Shape")
                throw new ArgumentOutOfRangeException("indexedFaceSetAndNode", "Only Shape nodes are allowed as input.");

            IList<Matrix<double>> transforms = new List<Matrix<double>>();
            while (parent != null)
            {
                if (parent.Name == "Transform")
                    transforms.Add(ParseTransform(parent));
                parent = parent.ParentNode;
            }

            int matricesCount = transforms.Count;

            Matrix<double> resultingMatrix = transforms[matricesCount - 1];

            for (int i = matricesCount - 2; i >= 0; i--)
                resultingMatrix = transforms[i]*resultingMatrix;


            return resultingMatrix;
        }

        private Matrix ParseTransform(XmlNode xmlNode)
        {
            string[] translation = xmlNode.Attributes["translation"].InnerText.Trim().Split();
            string[] scale = xmlNode.Attributes["scale"].InnerText.Trim().Split();
            string[] rotation = xmlNode.Attributes["rotation"].InnerText.Trim().Split();

            var translationMatrix = new DenseMatrix(4, 4);
            translationMatrix[0, 0] = 1;
            translationMatrix[1, 1] = 1;
            translationMatrix[2, 2] = 1;
            translationMatrix[3, 3] = 1;

            translationMatrix[3, 0] = double.Parse(translation[0]);
            translationMatrix[3, 1] = double.Parse(translation[1]);
            translationMatrix[3, 2] = double.Parse(translation[2]);

            var scaleMatrix = new DenseMatrix(4, 4);
            scaleMatrix[0, 0] = double.Parse(scale[0]);
            scaleMatrix[1, 1] = double.Parse(scale[1]);
            scaleMatrix[2, 2] = double.Parse(scale[2]);
            scaleMatrix[3, 3] = 1;

            //The 3x3 matrix representation of a rotation (x y z a) is

            //[ tx2+c  txy+sz txz-sy
            //  txy-sz ty2+c  tyz+sx
            //  txz+sy tyz-sx tz2+c  ]

            //where c = cos(a), s = sin(a), and t = 1-c.

            var rotationMatrix = new DenseMatrix(4, 4);
            double x = double.Parse(rotation[0]);
            double y = double.Parse(rotation[1]);
            double z = double.Parse(rotation[2]);
            double a = double.Parse(rotation[3]);
            double c = Math.Cos(a);
            double s = Math.Sin(a);
            double t = 1 - c;

            // tx2+c  txy+sz txz-sy
            rotationMatrix[0, 0] = t*x*x + c;
            rotationMatrix[0, 1] = t*x*y + s*z;
            rotationMatrix[0, 2] = t*x*z - s*y;

            //  txy-sz ty2+c  tyz+sx
            rotationMatrix[1, 0] = t*x*y - s*z;
            rotationMatrix[1, 1] = t*y*y + c;
            rotationMatrix[1, 2] = t*y*z + s*x;

            //  txz+sy tyz-sx tz2+c 
            rotationMatrix[2, 0] = t*x*z + s*y;
            rotationMatrix[2, 1] = t*y*z - s*x;
            rotationMatrix[2, 2] = t*z*z + c;

            rotationMatrix[3, 3] = 1;

            DenseMatrix result = scaleMatrix*rotationMatrix*translationMatrix;
            return result;
        }

        private Tuple<int, int, int>[] GetCoordIndices(XmlNode indexedFaceSet)
        {
            XmlAttribute coordNode = indexedFaceSet.Attributes["coordIndex"] ?? indexedFaceSet.Attributes["index"];
            var coordIndex = coordNode.InnerText.Trim().Split().Select(int.Parse).Where(n => n != -1).ToArray();
            int coordCount = coordIndex.Count();

            var indexTuples = new Tuple<int, int, int>[coordCount/3];
            for (int i = 0; i < coordCount; i += 3)
            {
                indexTuples[i/3] = new Tuple<int, int, int>(coordIndex[i],coordIndex[i + 1],coordIndex[i + 2]);
            }

            return indexTuples;
        }

        private Tuple<double, double, double>[] GetCoordNormals(XmlNode indexedFaceSet)
        {
            return GetTriples(indexedFaceSet, "vector", 1);
        }

        private Tuple<double, double, double>[] GetCoordVertices(XmlNode indexedFaceSet)
        {
            return GetTriples(indexedFaceSet, "point", 0);
        }

        private static Tuple<double, double, double>[] GetTriples(XmlNode indexedFaceSet, string attributeName, int childIndex)
        {
            XmlNode child = indexedFaceSet.ChildNodes[childIndex]; 

            if(child == null)
                return null;

            string[] coordVertex = child.Attributes[attributeName].InnerText.Trim().Split();
            int coordCount = coordVertex.Count();
            var coordTuples = new Tuple<double, double, double>[coordCount / 3];

            for (int i = 0; i < coordCount; i += 3)
            {
                double tempX = double.Parse(coordVertex[i]);
                double tempY = double.Parse(coordVertex[i + 1]);
                double tempZ = double.Parse(coordVertex[i + 2]);

                coordTuples[i / 3] = new Tuple<double, double, double>(tempX, tempY, tempZ);
            }

            return coordTuples;
        }

        private void CreateWorldAndObjectPositions()
        {
            currWorldPositions = new Tuple<double, double, double>[currVertices.Length];
            currWorlsNormals = new Tuple<double, double, double>[currVertices.Length];

            for (int i = 0; i < currVertices.Length; i++)
                currWorldPositions[i]  = TransformToWorld(i, currVertices);

            if(currNormals == null)
                return;

            for (int i = 0; i < currVertices.Length; i++)
                currWorlsNormals[i] = TransformToWorld(i, currNormals);
            
        }

        private Tuple<double, double, double> TransformToWorld(int i, Tuple<double, double, double>[] source)
        {
            var vertex = source[i];

            var vertexVectorAsMatrix = new DenseMatrix(4, 4);
            vertexVectorAsMatrix[0, 0] = vertex.Item1;
            vertexVectorAsMatrix[0, 1] = vertex.Item2;
            vertexVectorAsMatrix[0, 2] = vertex.Item3;
            vertexVectorAsMatrix[0, 3] = 1;

            Matrix<double> resultMatrix = vertexVectorAsMatrix.Multiply(currTransform);
            var resultVector = resultMatrix.Row(0);
            return new Tuple<double, double, double>(resultVector[0], resultVector[1],
                resultVector[2]);
        }
    }


}
