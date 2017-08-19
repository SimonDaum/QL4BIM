using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity.Utility;
using QL4BIMinterpreter.QL4BIM;
using QL4BIMspatial;

namespace QL4BIMinterpreter.P21
{   


    public class P21Reader : IP21Reader
    {
        private readonly IP21Repository repository;

        private Dictionary<string, List<QLProperty>> classNameToProperties;
        private Dictionary<string, NetType> typeNameToNetType;
        private Dictionary<string, HashSet<Tuple<string, string, string>>> invertedAttributeReversed;
        private List<string> entityLines;
        private Dictionary<string, P21InheritanceNode> inheritanceNodes;
        private Dictionary<string, bool> typeIsAbstract;
        private bool isIfc4;
        private readonly string[] classesToSkipInverse = {"IFCCARTESIANPOINTLIST3D", "IFCTRIANGULATEDFACESET"};

        public P21Reader(IP21Repository repository)
        {
            this.repository = repository;
            entityLines = new List<string>();
        }

        private void LoadSchema()
        {
            inheritanceNodes = new Dictionary<string, P21InheritanceNode>();
            typeIsAbstract = new Dictionary<string, bool>();
            typeNameToNetType = new Dictionary<string, NetType>();

            string schemaFilename = isIfc4 ? @"..\Schema\ifc4.exp" : @"..\Schema\ifc2x3_tc1.exp";

            var schemaLines = File.ReadAllLines(schemaFilename).ToArray();
            entityLines.Clear();

            var sb = new StringBuilder();
            var isInAdd = false;
            var isInverse = false;
            for (int i = 0; i < schemaLines.Length; i++)
            {
                var currentLine = schemaLines[i].Trim();
                if (currentLine == string.Empty)
                    continue;

                if (currentLine.Last().Equals(';') && currentLine.StartsWith("TYPE", StringComparison.InvariantCulture) &&
                    !currentLine.StartsWith("TYPEOF", StringComparison.InvariantCulture))
                {
                    var shortLine = currentLine.Substring(0, currentLine.Length - 1);
                    var splitted = shortLine.Split(' ');
                    var splitted2 = shortLine.Split('=');

                    typeNameToNetType.Add(splitted[1], new NetType(splitted[1], splitted2[1]));
                }


                if (currentLine.StartsWith("ENTITY", StringComparison.InvariantCulture))
                {
                    var className = currentLine.Substring(7);
                    sb.Append(className);
                    isInAdd = true;
                    isInverse = false;
                    continue;
                }

                if (currentLine.Equals("END_ENTITY;", StringComparison.InvariantCulture))
                {

                    var line = sb.ToString();
                    isInverse = false;

                    typeIsAbstract.Add(line.Substring(0, line.IndexOf(' ')).ToUpper(),
                        line.IndexOf("ABSTRACT", StringComparison.InvariantCulture) != -1);

                    if (line.Contains("SUPERTYPE"))
                    {
                        var index0 = line.IndexOf("SUPERTYPE", StringComparison.InvariantCulture);
                        var index1 = line.IndexOf("ABSTRACT", StringComparison.InvariantCulture);
                        var index2 = line.IndexOf(";", StringComparison.InvariantCulture);
                        var index3 = line.IndexOf("SUBTYPE", StringComparison.InvariantCulture);


                        var indexStart = index1 == -1 ? index0 : index1;
                        var indexEnd = index3 == -1 ? index2 : index3;

                        line = line.Substring(0, indexStart) + line.Substring(indexEnd, line.Length - indexEnd);
                    }

                    entityLines.Add(line);
                    sb.Clear();
                    isInAdd = false;
                }

                if (currentLine.Equals("WHERE", StringComparison.InvariantCulture) ||
                    currentLine.Equals("UNIQUE", StringComparison.InvariantCulture) ||
                    currentLine.Equals("DERIVE", StringComparison.InvariantCulture))
                {
                    isInAdd = false;
                    isInverse = false;
                }

                if (currentLine.Equals("INVERSE", StringComparison.InvariantCulture))
                {
                    isInverse = true;
                }

                if (isInAdd)
                {
                    if (currentLine.Equals("INVERSE", StringComparison.InvariantCulture))
                        continue;

                    if (isInverse)
                    {
                        currentLine = "*" + currentLine;
                    }

                    sb.Append(" " + currentLine);
                }
            }

            var typeNameToNetTypeCopy = typeNameToNetType.ToDictionary(p => p.Key, p => p.Value);
            foreach (var type in typeNameToNetTypeCopy)
            {
                if (!type.Value.IfcType.Contains("Ifc"))
                    continue;

                var tempNetType = type.Value;

                while (tempNetType.IfcType.StartsWith("Ifc"))
                {   
                    if(tempNetType.IfcType == "IfcPropertySetDefinition")
                        break;
                    
                    tempNetType = typeNameToNetType[tempNetType.IfcType];
                }

                typeNameToNetType.Remove(type.Key);
                typeNameToNetType.Add(type.Key, tempNetType);
            }

            for (int i = 0; i < entityLines.Count; i++)
            {
                var line = entityLines[i];
                var index1 = line.IndexOf("SUBTYPE", StringComparison.InvariantCulture);
                var index2 = line.IndexOf(";", StringComparison.InvariantCulture);

                if (index1 == -1)
                    continue;

                var list = new List<string>();
                ResolveSubtyping(line, ref list);

                string mid = string.Empty;
                if (list.Count > 0)
                    mid = ";" + string.Join(";", list);

                line = line.Substring(0, index1) + mid + line.Substring(index2, line.Length - index2);
                entityLines[i] = line;
            }
        }

        public string[] GetAllSubtypNames(string typeName)
        {
            var upperTypeName = typeName.ToUpper();
            if (!inheritanceNodes.ContainsKey(upperTypeName))
                return new []{upperTypeName};

            var queue = new Queue<P21InheritanceNode>();

            var hashSet = new HashSet<string>();
            queue.Enqueue(inheritanceNodes[upperTypeName]);
            var sb = new List<string>();

            while (queue.Count != 0)
            {
                var currentNode = queue.Dequeue();
                if (!typeIsAbstract[currentNode.Name] && !sb.Contains(currentNode.Name))
                    sb.Add(currentNode.Name);

                foreach (var child in currentNode.Subtypes)
                {
                    if (!hashSet.Contains(child.Name))
                    {
                        queue.Enqueue(child);
                        hashSet.Add(child.Name);
                    }

                }

            }

            return sb.ToArray();
        }


        public QLEntity[] LoadIfcFile(string ifcFilename)
        {

            bool currentIfc4;
            var linesAsString = ExtractEntityLines(ifcFilename, out currentIfc4);

            QLEntityId.SetStartId();

            var scanner = new Scanner(linesAsString.ToStream());
            var parser = new Parser(scanner);

            var oldSchema = parser.QLExchangeFile.CurrentSchema;
            parser.QLExchangeFile.CurrentSchema = currentIfc4 ? Schema.Ifc4 : Schema.Ifc2X3;

            parser.Parse();

            if (entityLines.Count == 0 || parser.QLExchangeFile.CurrentSchema != oldSchema)
            {
                isIfc4 = currentIfc4;
                LoadSchema();
            }

            var entities = parser.QLExchangeFile.QLExchangeFiles.ToArray();


            


            classNameToProperties = new Dictionary<string, List<QLProperty>>();
            invertedAttributeReversed = new Dictionary<string, HashSet<Tuple<string, string, string>>>();

            var entitiesWithPropNames = entities.Select(e => LookUpPropertyNames(entityLines, e)).Where(e => e != null).ToArray();

            foreach (var pair in classNameToProperties)
            {
                foreach (var inverseProp in pair.Value.Where(p => p.Inverse))
                {
                    var className = pair.Key;
                    var splittedTyp = inverseProp.Type.Split(' ');
                    var inverseTyp = splittedTyp.First(t => t.StartsWith("Ifc")).ToUpper();
                    var inverseAttribute = splittedTyp.Last();

                    var hashSet = new HashSet<string>();
                    if (inheritanceNodes.ContainsKey(inverseTyp))
                    {
                        var basetype = inheritanceNodes[inverseTyp];
                        var queue = new Queue<P21InheritanceNode>();
                        queue.Enqueue(basetype);
                        while (queue.Count > 0)
                        {
                            var current = queue.Dequeue();
                            hashSet.Add(current.Name);
                            foreach (var subtype in current.Subtypes)
                            {
                                if (!hashSet.Contains(subtype.Name))
                                {
                                    queue.Enqueue(subtype);
                                    hashSet.Add(subtype.Name);
                                }
                            }
                        }
                        foreach (var typeName in hashSet)
                            AddInverseAttribute(typeName, inverseAttribute, inverseProp, className);
                        
                    }
                    else
                        AddInverseAttribute(inverseTyp, inverseAttribute, inverseProp, className);
                }
            }

            var idToEntity = new Dictionary<int, QLEntity>();
            foreach (var entity in entitiesWithPropNames)
                idToEntity.Add(entity.Id, entity);

            foreach (var entity in entitiesWithPropNames)
                SetInverseValues(entity, entity.QLDirectList, 0, 0, idToEntity);


            invertedAttributeReversed.Clear();
            typeNameToNetType.Clear();
            classNameToProperties.Clear();
            entityLines.Clear();

            return entitiesWithPropNames;
        }

        public void SetInverseValues(QLEntity entity, QLList list, int level, int propIndex, Dictionary<int, QLEntity> idToEntity)
        {
            var className = entity.ClassName;
            if(classesToSkipInverse.Contains(className)) //has list of lists and no inverse attributes
                return;

            for (int i = 0; i < list.List.Count; i++)
            {
                var part = list.List[i];
                var currentPropIndex = level == 0 ? i : propIndex;

                //class lookup 
                var propName = repository.DirectProperty(className, currentPropIndex).PropName;

                List<Tuple<string, string, string>> invertedAttribute = null;
                if (invertedAttributeReversed.ContainsKey(className))
                {
                    invertedAttribute = invertedAttributeReversed[className].ToList();
                }

                if (part.QLEntityId != null)
                {
                    var referencedWrapper = idToEntity[part.QLEntityId.Id];

                    //direct attributes
                    if (string.IsNullOrEmpty(part.QLEntityId.ConcreteType))
                        part.QLEntityId.ConcreteType = referencedWrapper.ClassName;

                    var inverseAttributeTarget =
                        invertedAttribute?.FirstOrDefault(a => a.Item1 == propName && a.Item3 == referencedWrapper.ClassName);

                    if (inverseAttributeTarget != null)
                    {
                        var inverseList = referencedWrapper.QlInverseList;
                        var inverseCount = repository.InversePropertyCount(referencedWrapper.ClassName);
                        if (inverseList == null)
                        {
                            var ifcList = new QLList();
                            for (int j = 0; j < inverseCount; j++)
                                ifcList.Add(new QLPart());

                            referencedWrapper.QlInverseList = ifcList;
                        }
                        SetInversePropertyVal(referencedWrapper,inverseAttributeTarget.Item2, entity.QlEntityId, false);
                    }
                }

                if (part.QLList != null)
                {
                    propIndex = i;
                    SetInverseValues(entity, part.QLList, level + 1, propIndex, idToEntity);
                }
            }


        }


        private void AddInverseAttribute(string inverseTyp, string inverseAttribute, QLProperty inverseProp,
            string className)
        {
            if (invertedAttributeReversed.ContainsKey(inverseTyp))
            {
                var tuple = new Tuple<string, string, string>(inverseAttribute, inverseProp.PropName, className);
                var hashSet = invertedAttributeReversed[inverseTyp];

                if (!hashSet.Contains(tuple))
                    hashSet.Add(tuple);
            }
            else
            {
                invertedAttributeReversed.Add(inverseTyp, new HashSet<Tuple<string, string, string>>()
                {
                    new Tuple<string, string, string>(inverseAttribute, inverseProp.PropName, className)
                });
            }
        }

        private string ExtractEntityLines(string ifcFilename, out bool isIfc4)
        {
            var allLines = File.ReadAllLines(ifcFilename).ToArray();
            var schemaIndicator = allLines.FirstOrDefault(
                l => l.StartsWith("FILE_SCHEMA", StringComparison.InvariantCultureIgnoreCase)).ToUpperInvariant();

            if (schemaIndicator.Contains("IFC4"))
                isIfc4 = true;
            else
            {
                if (schemaIndicator.Contains("IFC2X3"))
                    isIfc4 = false;
                else
                {
                    throw new ArgumentException("only IFC4 and IFC2x3 are supported");
                }
            }

            var lines = allLines.Where(l => l != string.Empty && l[0] == '#').ToList();
            var linesAsString = string.Join(Environment.NewLine, lines) + Environment.NewLine;
            return linesAsString;
        }

        private void ResolveSubtyping(string line, ref List<string> list)
        {
            if (!line.Contains("SUBTYPE"))
                return;


            var index1 = line.IndexOf("SUBTYPE", StringComparison.InvariantCulture) + 12;
            var index2 = line.IndexOf(";", StringComparison.InvariantCulture);

            var subType = line.Substring(index1, index2 - index1 - 1).Trim();
            var currentType = line.Substring(0, line.IndexOf(' ')).Trim();

            var currentInheritancenNode = GetInheritanceNode(subType);
            var childNode = GetInheritanceNode(currentType);
            currentInheritancenNode.AddSubtype(childNode);

            var subTypeEntity = entityLines.First(l => l.StartsWith(subType));
            var splittedEntityMid = SplittedEntityMid(subTypeEntity);
            ResolveSubtyping(subTypeEntity, ref list);

            list.AddRange(splittedEntityMid);
        }

        private P21InheritanceNode GetInheritanceNode(string type)
        {
            P21InheritanceNode childNode;
            var uppered = type.ToUpper();

            if (inheritanceNodes.ContainsKey(uppered))
            {
                childNode = inheritanceNodes[uppered];
            }
            else
            {
                var isAbstract = typeIsAbstract[uppered];
                childNode = new P21InheritanceNode(uppered, isAbstract);
                inheritanceNodes.Add(uppered, childNode);
            }
            return childNode;
        }

        private IEnumerable<string> SplittedEntityMid(string suberTypeEntity)
        {
            var splittedEntity = suberTypeEntity.Split(';');
            var splittedEntityMid = new List<string>();

            for (int i = 1; i < splittedEntity.Length - 1; i++)
                splittedEntityMid.Add(splittedEntity[i]);

            return splittedEntityMid;
        }

        private QLEntity LookUpPropertyNames(List<string> entityLines, QLEntity entity)
        {
            var className = entity.ClassName;

            if (!classNameToProperties.ContainsKey(className))
            {
                var entityLine = entityLines.First(e => e.StartsWith(className, StringComparison.InvariantCultureIgnoreCase));
                var props = new List<QLProperty>();


                var splitted = entityLine.Split(';');
                for (int i = 1; i < splitted.Length - 1; i++)
                {
                    var item = splitted[i].Trim();

                    var isOp = false;
                    if (item.Contains("OPTIONAL"))
                    {
                        item = item.Replace("OPTIONAL", string.Empty);
                        isOp = true;
                    }

                    item = item.Replace(" : ", "~");
                    var parts = item.Split('~');

                    var type = parts[1].Trim();

                    NetType netType = new NetType("Entity", "Entity");
                    if (typeNameToNetType.ContainsKey(type))
                        netType = typeNameToNetType[type];

                    props.Add(new QLProperty(parts[0].Trim(), type, netType, isOp));
                }

                classNameToProperties.Add(className, props);
            }

            repository.SetProperties(className, classNameToProperties[className]);
            return entity;
        }




        public void SetInversePropertyVal(QLEntity entity, string propertyName, QLEntityId qlEntityId, bool deleteOld)
        {
            var index = repository.InversePropertyIndex(entity.ClassName, propertyName);

            var part = entity.QlInverseList.List[index];
            if (deleteOld || (part.QLEntityId == null && part.QLList == null))
                entity.QlInverseList.List[index] = new QLPart() { QLEntityId = qlEntityId };
            else
            {
                if (part.QLEntityId != null)
                {
                    var list = new QLList();
                    list.Add(new QLPart() { QLEntityId = part.QLEntityId });
                    list.Add(new QLPart() { QLEntityId = qlEntityId });

                    part.QLEntityId = null;
                    part.QLList = list;
                }
                else
                {
                    part.QLList.Add(new QLPart() { QLEntityId = qlEntityId });
                }
            }
        }

    }

    public class NetType
    {

        public string IfcType { get; private set; }

        public string Container { get; private set; }

        public string BaseType { get; set; }

        public NetType(string baseType, string ifcType)
        {
            BaseType = baseType;
            IfcType = ifcType.Trim();

            if (ifcType.Contains("OF"))
            {
                ifcType = ifcType.Replace("OF", "*");
                var temp = ifcType.Split('*');
                Container = temp[0].Trim();
                IfcType = temp[1].Trim();
            }
        }

        public override string ToString()
        {
            return "Base: " + BaseType + " -> " + IfcType + " Container: " + Container;
        }
    }

    static class StreamExtentions
    {
        public static Stream ToStream(this string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}