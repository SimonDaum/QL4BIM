using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;


namespace IFCViewerX86
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// 

    /// <summary>
    /// Types of supported movements
    /// </summary>


    /// <summary>
    /// IFCItemX64 presents a single ifc item for drawing 
    /// </summary>
    class IFCItemX64
    {
        public void CreateItem(IFCItemX64 parent, long ifcID, string ifcType, string globalID, string name, string desc)
        {

            this.parent = parent;
            this.next = null;
            this.child = null;
            this.globalID = globalID;
            this.ifcID = ifcID;
            this.ifcType = ifcType;
            this.description = desc;
            this.name = name;

            if (parent != null)
            {
                if (parent.child == null)
                {
                    parent.child = this;
                }
                else
                {
                    IFCItemX64 NextChild = parent;

                    while (true)
                    {
                        if (NextChild.next == null)
                        {
                            NextChild.next = this;
                            break;
                        }
                        else
                        {
                            NextChild = NextChild.next;
                        }

                    }

                }

            }
        }
        public long ifcID = 0;
        public string globalID;
        public string ifcType;
        public string name;
        public string description;
        public IFCItemX64 parent = null;
        public IFCItemX64 next = null;
        public IFCItemX64 child = null;
        public long noVerticesForFaces;
        public long noPrimitivesForFaces;
        public float[] verticesForFaces;
        public int[] indicesForFaces;
        public int vertexOffsetForFaces;
        public int indexOffsetForFaces;
        public int noVerticesForWireFrame;
        public int noPrimitivesForWireFrame;
        public float[] verticesForWireFrame;
        public int[] indicesForWireFrame;
        public int[] indicesForWireFrameLineParts;
        public int vertexOffsetForWireFrame;
        public int indexOffsetForWireFrame;

    }


    /// <summary>
    /// Class aims to read ifc file and draw its objects 
    /// </summary>
    class IFCViewerWrapperX64
    {

        private IFCItemX64 _rootIfcItem = null;
        private int counter = 0;

        private bool _enableWireFrames = true;
        private bool _enableFaces = true;
        private bool _enableHover = true;
        private int currentPos = 0;
        private int currentPosInd = 0;

        private IFCItemX64 _hoverIfcItem = null;
        private IFCItemX64 _selectedIfcItem = null;
        double[] center = new double[3];
        double size = 0;
        private readonly IList<Tuple<string, int[], double[]>> indicesVerticesPerId;
        private static bool modelInMilimeters;

        /// <summary>
        /// ctor
        /// </summary>
        public IFCViewerWrapperX64()
        {
            indicesVerticesPerId = new List<Tuple<string, int[], double[]>>();
        }

        // -------------------------------------------------------------------
        // Private Methods 

        private IList<Tuple<string, int[], double[]>> ParseIfcFile(string sPath)
        {
            if (!File.Exists(sPath))
                return null;

            long ifcModel = IfcEngine.x64.sdaiOpenModelBN(0, sPath, "IFC2X3_TC1.exp");

            string xmlSettings_IFC2x3 = @"IFC2X3-Settings.xml";
            string xmlSettings_IFC4 = @"IFC4-Settings.xml";

            if (ifcModel != 0)
            {

                IntPtr outputValue = IntPtr.Zero;

                IfcEngine.x64.GetSPFFHeaderItem(ifcModel, 9, 0, IfcEngine.x64.sdaiSTRING, out outputValue);

                string s = Marshal.PtrToStringAnsi(outputValue);


                XmlTextReader textReader = null;
                if (s.Contains("IFC2") == true)
                {
                    textReader = new XmlTextReader(xmlSettings_IFC2x3);
                }
                else
                {
                    if (s.Contains("IFC4") == true)
                    {
                        IfcEngine.x64.sdaiCloseModel(ifcModel);
                        ifcModel = IfcEngine.x64.sdaiOpenModelBN(0, sPath, "IFC4.exp");

                        if (ifcModel != 0)
                            textReader = new XmlTextReader(xmlSettings_IFC4);
                    }
                }

                if (textReader == null)
                    return null;

                // if node type us an attribute
                while (textReader.Read())
                {
                    textReader.MoveToElement();

                    if (textReader.AttributeCount > 0)
                    {
                        if (textReader.LocalName == "object")
                        {
                            if (textReader.GetAttribute("name") != null)
                            {
                                string name = textReader.GetAttribute("name");
                                RetrieveObjects(ifcModel, name);
                            }
                        }
                    }
                }

                int a = 0;
                GenerateGeometry(ifcModel, _rootIfcItem, ref a);


                IfcEngine.x64.sdaiCloseModel(ifcModel);


                return indicesVerticesPerId;
            }

            return null;
        }

        private void GenerateFacesGeometry(long ifcModel, IFCItemX64 ifcItem)
        {
            if (ifcItem.ifcID != 0)
            {

                long noVertices = 0, noIndices = 0;
                IfcEngine.x64.initializeModellingInstance(ifcModel, ref noVertices, ref noIndices, 0, ifcItem.ifcID);

                if (noVertices != 0 && noIndices != 0)
                {
                    ifcItem.noVerticesForFaces = noVertices;
                    ifcItem.noPrimitivesForFaces = noIndices / 3;
                    ifcItem.verticesForFaces = new float[6 * noVertices];
                    ifcItem.indicesForFaces = new int[noIndices];



                    IfcEngine.x64.finalizeModelling(ifcModel, ifcItem.verticesForFaces, ifcItem.indicesForFaces, 0);


                    var doubleList = new List<double>();
                    if (modelInMilimeters)
                        doubleList.AddRange(ifcItem.verticesForFaces.Select(value => (double)value / 1000));
                    else
                        doubleList.AddRange(ifcItem.verticesForFaces.Select(value => (double)value));



                    indicesVerticesPerId.Add(new Tuple<string, int[], double[]>(ifcItem.globalID, ifcItem.indicesForFaces, doubleList.ToArray()));
                }
            }
        }

        void GenerateGeometry(long ifcModel, IFCItemX64 ifcItem, ref int a)
        {
            while (ifcItem != null)
            {
                Int64 setting = 0, mask = 0;
                mask += IfcEngine.x64.flagbit2;        //    PRECISION (32/64 bit)
                mask += IfcEngine.x64.flagbit3;        //    INDEX ARRAY (32/64 bit)
                mask += IfcEngine.x64.flagbit5;        //    NORMALS
                mask += IfcEngine.x64.flagbit8;        //    TRIANGLES
                mask += IfcEngine.x64.flagbit12;       //    WIREFRAME

                setting += 0;                          //    SINGLE PRECISION (float)
                setting += 0;                          //    32 BIT INDEX ARRAY (Int32)
                setting += IfcEngine.x64.flagbit5;     //    NORMALS ON
                setting += IfcEngine.x64.flagbit8;     //    TRIANGLES ON
                setting += 0;                          //    WIREFRAME OFF
                IfcEngine.x64.setFormat(ifcModel, setting, mask);

                GenerateFacesGeometry(ifcModel, ifcItem);

                IfcEngine.x64.cleanMemory(ifcModel, 0);

                GenerateGeometry(ifcModel, ifcItem.child, ref a);
                ifcItem = ifcItem.next;
            }
        }

        private void RetrieveObjects(long ifcModel, string objectDisplayName)
        {



            long ifcObjectInstances = IfcEngine.x64.sdaiGetEntityExtentBN(ifcModel, objectDisplayName),
                noIfcObjectIntances = IfcEngine.x64.sdaiGetMemberCount(ifcObjectInstances);

            if (noIfcObjectIntances != 0)
            {
                IFCItemX64 NewItem = null;
                if (_rootIfcItem == null)
                {
                    _rootIfcItem = new IFCItemX64();
                    _rootIfcItem.CreateItem(null, 0, "", objectDisplayName, "", "");

                    NewItem = _rootIfcItem;
                }
                else
                {
                    IFCItemX64 LastItem = _rootIfcItem;
                    while (LastItem != null)
                    {
                        if (LastItem.next == null)
                        {
                            LastItem.next = new IFCItemX64();
                            LastItem.next.CreateItem(null, 0, "", objectDisplayName, "", "");

                            NewItem = LastItem.next;

                            break;
                        }
                        else
                            LastItem = LastItem.next;
                    };
                }


                for (int i = 0; i < noIfcObjectIntances; ++i)
                {
                        

                    NormalEntityProcessing(objectDisplayName, ifcObjectInstances, i, NewItem);
                }
            }
        }

        private static void NormalEntityProcessing(string objectDisplayName, long ifcObjectInstances, int i, IFCItemX64 NewItem)
        {


     
            long ifcObjectIns = 0;
            IfcEngine.x64.engiGetAggrElement(ifcObjectInstances, i, IfcEngine.x64.sdaiINSTANCE, out ifcObjectIns);

            IntPtr value = IntPtr.Zero;
            IfcEngine.x64.sdaiGetAttrBN(ifcObjectIns, "GlobalId", IfcEngine.x64.sdaiSTRING, out value);

            string globalID = Marshal.PtrToStringAnsi((IntPtr) value);

            if (string.CompareOrdinal(objectDisplayName, "IfcSIUnit") == 0) 
            {
                value = IntPtr.Zero;
                IfcEngine.x64.sdaiGetAttrBN(ifcObjectIns, "Prefix", IfcEngine.x64.sdaiSTRING, out value);
                string milInicator = Marshal.PtrToStringAnsi((IntPtr) value);

                if (string.CompareOrdinal(milInicator, ".MILLI.") == 0)
                    modelInMilimeters = true;

                return;
            }

            value = IntPtr.Zero;
            IfcEngine.x64.sdaiGetAttrBN(ifcObjectIns, "Name", IfcEngine.x64.sdaiSTRING, out value);

            string name = Marshal.PtrToStringAnsi((IntPtr) value);

            value = IntPtr.Zero;
            IfcEngine.x64.sdaiGetAttrBN(ifcObjectIns, "Description", IfcEngine.x64.sdaiSTRING, out value);

            string description = Marshal.PtrToStringAnsi((IntPtr) value);

            IFCItemX64 subItem = new IFCItemX64();
            subItem.CreateItem(NewItem, ifcObjectIns, objectDisplayName, globalID, name, description);
        }


  

        public IList<Tuple<string, int[], double[]>> OpenIfcFile(string ifcFilePath)
        {
            _rootIfcItem = null;
            return ParseIfcFile(ifcFilePath);
        }
    }
}
