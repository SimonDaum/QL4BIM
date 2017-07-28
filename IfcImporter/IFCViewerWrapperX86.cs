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
    enum MOVE_TYPE
    {
        ROTATE,
        PAN,
        ZOOM,
        NONE,
    }

    /// <summary>
    /// IFCItem presents a single ifc item for drawing 
    /// </summary>
    class IFCItem
    {
        public void CreateItem(IFCItem parent, int ifcID, string ifcType, string globalID, string name, string desc)
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
                    IFCItem NextChild = parent;

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
        public int ifcID = 0;
        public string globalID;
        public string ifcType;
        public string name;
        public string description;
        public IFCItem parent = null;
        public IFCItem next = null;
        public IFCItem child = null;
        public int noVerticesForFaces;
        public int noPrimitivesForFaces;
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
    class IFCViewerWrapperX86
    {

        private IFCItem _rootIfcItem = null;
        private int counter = 0;

        private bool _enableWireFrames = true;
        private bool _enableFaces = true;
        private bool _enableHover = true;
        private int currentPos = 0;
        private int currentPosInd = 0;

        private IFCItem _hoverIfcItem = null;
        private IFCItem _selectedIfcItem = null;
        double[] center = new double[3];
        double size = 0;
        private readonly IList<Tuple<string, int[], double[]>> indicesVerticesPerId;
        private static bool modelInMilimeters;

        /// <summary>
        /// ctor
        /// </summary>
        public IFCViewerWrapperX86()
        {
            indicesVerticesPerId = new List<Tuple<string, int[], double[]>>();
        }

        // -------------------------------------------------------------------
        // Private Methods 

        private IList<Tuple<string, int[], double[]>> ParseIfcFile(string sPath)
        {
            if (!File.Exists(sPath))
                throw new FileNotFoundException(sPath);

            int ifcModel = IfcEngine.x86.sdaiOpenModelBN(0, sPath, "IFC2X3_TC1.exp");

            string xmlSettings_IFC2x3 = @"IFC2X3-Settings.xml";
            string xmlSettings_IFC4 = @"IFC4-Settings.xml";

            if (ifcModel != 0)
            {

                IntPtr outputValue = IntPtr.Zero;

                IfcEngine.x86.GetSPFFHeaderItem(ifcModel, 9, 0, IfcEngine.x86.sdaiSTRING, out outputValue);

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
                        IfcEngine.x86.sdaiCloseModel(ifcModel);
                        ifcModel = IfcEngine.x86.sdaiOpenModelBN(0, sPath, "IFC4.exp");

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


                IfcEngine.x86.sdaiCloseModel(ifcModel);


                return indicesVerticesPerId;
            }

            return null;
        }

        private void GenerateFacesGeometry(int ifcModel, IFCItem ifcItem)
        {
            if (ifcItem.ifcID != 0)
            {

                int noVertices = 0, noIndices = 0;
                IfcEngine.x86.initializeModellingInstance(ifcModel, ref noVertices, ref noIndices, 0, ifcItem.ifcID);

                if (noVertices != 0 && noIndices != 0)
                {
                    ifcItem.noVerticesForFaces = noVertices;
                    ifcItem.noPrimitivesForFaces = noIndices / 3;
                    ifcItem.verticesForFaces = new float[3 * noVertices];
                    ifcItem.indicesForFaces = new int[noIndices];

                    float[] pVertices = new float[noVertices * 3];

                    IfcEngine.x86.finalizeModelling(ifcModel, pVertices, ifcItem.indicesForFaces, 0);

                    int i = 0;
                    while (i < noVertices)
                    {
                        ifcItem.verticesForFaces[3 * i + 0] = pVertices[3 * i + 0];
                        ifcItem.verticesForFaces[3 * i + 1] = pVertices[3 * i + 1];
                        ifcItem.verticesForFaces[3 * i + 2] = pVertices[3 * i + 2];
                        i++;
                    }

                    var doubleList = new List<double>();
                    if(modelInMilimeters)
                        doubleList.AddRange(ifcItem.verticesForFaces.Select(value => (double) value / 1000));
                    else
                        doubleList.AddRange(ifcItem.verticesForFaces.Select(value => (double)value));



                    indicesVerticesPerId.Add(new Tuple<string, int[], double[]>(ifcItem.globalID, ifcItem.indicesForFaces, doubleList.ToArray()));
                }
            }
        }

        void GenerateGeometry(int ifcModel, IFCItem ifcItem, ref int a)
        {
            while (ifcItem != null)
            {
                // -----------------------------------------------------------------
                // Generate WireFrames Geometry

                int setting = 0, mask = 0;
                mask += IfcEngine.x86.flagbit2;        //    PRECISION (32/64 bit)
                mask += IfcEngine.x86.flagbit3;        //	   INDEX ARRAY (32/64 bit)
                mask += IfcEngine.x86.flagbit5;        //    NORMALS
                mask += IfcEngine.x86.flagbit8;        //    TRIANGLES
                mask += IfcEngine.x86.flagbit12;       //    WIREFRAME
                setting += 0;		     //    DOUBLE PRECISION (double)

                if (IntPtr.Size == 4) // indication for 32
                {
                    setting += 0;            //    32 BIT INDEX ARRAY (Int32)
                }
                else
                {
                    if (IntPtr.Size == 8)
                    {
                        setting += IfcEngine.x86.flagbit3;     // 64 BIT INDEX ARRAY (Int64)
                    }
                }

                setting += 0;            //    NORMALS OFF
                setting += 0;			 //    TRIANGLES OFF
                setting += IfcEngine.x86.flagbit12;    //    WIREFRAME ON


                IfcEngine.x86.setFormat(ifcModel, setting, mask);

                //GenerateWireFrameGeometry(ifcModel, ifcItem);
                // -----------------------------------------------------------------
                // Generate Faces Geometry

                setting = 0;
                setting += 0;		     //    SINGLE PRECISION (float)
                if (IntPtr.Size == 4) // indication for 32
                {
                    setting += 0;            //    32 BIT INDEX ARRAY (Int32)
                }
                else
                {
                    if (IntPtr.Size == 8)
                    {
                        setting += IfcEngine.x86.flagbit3;     //    64 BIT INDEX ARRAY (Int64)
                    }
                }

                setting += 0;// IfcEngine.x86.flagbit5;     //    NORMALS ON
                setting += IfcEngine.x86.flagbit8;     //    TRIANGLES ON
                setting += 0;			 //    WIREFRAME OFF 
                IfcEngine.x86.setFormat(ifcModel, setting, mask);

                GenerateFacesGeometry(ifcModel, ifcItem);

                IfcEngine.x86.cleanMemory(ifcModel, 0);

                GenerateGeometry(ifcModel, ifcItem.child, ref a);
                ifcItem = ifcItem.next;
            }
        }

        private void RetrieveObjects(int ifcModel, string objectDisplayName)
        {



            int ifcObjectInstances = IfcEngine.x86.sdaiGetEntityExtentBN(ifcModel, objectDisplayName),
                noIfcObjectIntances = IfcEngine.x86.sdaiGetMemberCount(ifcObjectInstances);

            if (noIfcObjectIntances != 0)
            {
                IFCItem NewItem = null;
                if (_rootIfcItem == null)
                {
                    _rootIfcItem = new IFCItem();
                    _rootIfcItem.CreateItem(null, 0, "", objectDisplayName, "", "");

                    NewItem = _rootIfcItem;
                }
                else
                {
                    IFCItem LastItem = _rootIfcItem;
                    while (LastItem != null)
                    {
                        if (LastItem.next == null)
                        {
                            LastItem.next = new IFCItem();
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

        private static void NormalEntityProcessing(string objectDisplayName, int ifcObjectInstances, int i,
            IFCItem NewItem)
        {


     
            int ifcObjectIns = 0;
            IfcEngine.x86.engiGetAggrElement(ifcObjectInstances, i, IfcEngine.x86.sdaiINSTANCE, out ifcObjectIns);

            IntPtr value = IntPtr.Zero;
            IfcEngine.x86.sdaiGetAttrBN(ifcObjectIns, "GlobalId", IfcEngine.x86.sdaiSTRING, out value);

            string globalID = Marshal.PtrToStringAnsi((IntPtr) value);

            if (string.CompareOrdinal(objectDisplayName, "IfcSIUnit") == 0) 
            {
                value = IntPtr.Zero;
                IfcEngine.x86.sdaiGetAttrBN(ifcObjectIns, "Prefix", IfcEngine.x86.sdaiSTRING, out value);
                string milInicator = Marshal.PtrToStringAnsi((IntPtr) value);

                if (string.CompareOrdinal(milInicator, ".MILLI.") == 0)
                    modelInMilimeters = true;

                return;
            }

            value = IntPtr.Zero;
            IfcEngine.x86.sdaiGetAttrBN(ifcObjectIns, "Name", IfcEngine.x86.sdaiSTRING, out value);

            string name = Marshal.PtrToStringAnsi((IntPtr) value);

            value = IntPtr.Zero;
            IfcEngine.x86.sdaiGetAttrBN(ifcObjectIns, "Description", IfcEngine.x86.sdaiSTRING, out value);

            string description = Marshal.PtrToStringAnsi((IntPtr) value);

            IFCItem subItem = new IFCItem();
            subItem.CreateItem(NewItem, ifcObjectIns, objectDisplayName, globalID, name, description);
        }


        private void GetDimensions(IFCItem ifcItem, ref double[] min, ref double[] max, ref bool InitMinMax)
        {         
            while (ifcItem != null)
            {
                if (ifcItem.noVerticesForFaces != 0)
                {
                    if (InitMinMax == false)
                    {
                        min[0] = ifcItem.verticesForFaces[3 * 0 + 0];
                        min[1] = ifcItem.verticesForFaces[3 * 0 + 1];
                        min[2] = ifcItem.verticesForFaces[3 * 0 + 2];
                        max = min;

                        InitMinMax = true;
                    }

                    int i = 0;
                    while (i < ifcItem.noVerticesForFaces)
                    {

                        min[0] = Math.Min(min[0], ifcItem.verticesForFaces[6 * i + 0]);
                        min[1] = Math.Min(min[1], ifcItem.verticesForFaces[6 * i + 1]);
                        min[2] = Math.Min(min[2], ifcItem.verticesForFaces[6 * i + 2]);

                        max[0] = Math.Max(max[0], ifcItem.verticesForFaces[6 * i + 0]);
                        max[1] = Math.Max(max[1], ifcItem.verticesForFaces[6 * i + 1]);
                        max[2] = Math.Max(max[2], ifcItem.verticesForFaces[6 * i + 2]);

                        i++;
                    }
                }

                GetDimensions(ifcItem.child, ref min, ref max, ref InitMinMax);

                ifcItem = ifcItem.next; 
            }
        }
  

        public IList<Tuple<string, int[], double[]>> OpenIfcFile(string ifcFilePath)
        {
            _rootIfcItem = null;
            return ParseIfcFile(ifcFilePath);
        }
    }
}
