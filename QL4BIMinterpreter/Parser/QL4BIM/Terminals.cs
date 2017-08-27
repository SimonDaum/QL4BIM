/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMinterpreter.

QL4BIMinterpreter is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMinterpreter is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMinterpreter. If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using QL4BIMinterpreter.P21;

namespace QL4BIMinterpreter.QL4BIM
{
    public enum Schema { Ifc2X3, Ifc4, CityGml };

    public class QLEntityId
    {
        private int id;
        private static int _heighestId;

        private static int _startId;

        public static int HeighestId => _heighestId;

        public static void SetStartId()
        {
            _startId = _heighestId + 1;
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public QLEntityId(string id)
        {
            if (id[0] != '#')
                throw new ArgumentException("no # as first token in id");

            var value = int.Parse(id.Substring(1, id.Length - 1));

            if (value > _heighestId)
                _heighestId = value;

            Id = value + _startId;
        }

        public string ConcreteType { get; set; }

    }

    public class QLEnum
    {
        public string QLStr { get; set; }

        public QLEnum(string enumstring)
        {
            if (enumstring[0] != '.' || enumstring[enumstring.Length - 1] != '.')
                throw new ArgumentException("no . as first and last token in enumstring");

            QLStr = enumstring.Substring(1, enumstring.Length - 2);
        }
    }

    public class QLString
    {
        public string QLStr { get; set; }

        public QLString(string ifcString)
        {
            if (ifcString[0] == '\'' && ifcString[ifcString.Length - 1] == '\'')
                QLStr = ifcString.Substring(1, ifcString.Length - 2);
            else
                QLStr = ifcString;
        }
    }

    public class QLList
    {
        private List<QLPart> list = new List<QLPart>();

        public bool HasRef => list.Any(p => p.QLEntityId != null);

        public List<QLPart> List
        {
            get { return list; }
            set { list = value; }
        }

        public void Add(QLPart part)
        {
            list.Add(part);          
        }

        public QLPart this[int index] => List[index];
    }

    public class QLClass
    {
        public String ClassName { get; set; }
        public QLList QLDirectList { get; set; }
        public QLList QLInverseList { get; set; }
    }

    public class QLEntity
    {
        public string ClassName => qlClass.ClassName;
        public QLList QLDirectList => qlClass.QLDirectList;

        public  Schema Schema { get; set; }


        public QLList QlInverseList
        {
            get { return qlClass.QLInverseList; }
            set { qlClass.QLInverseList = value; }
        }

        private QLClass qlClass;
        private QLEntityId qlEntityId;

        public void SetEntityAndClass(string id, QLClass qlClass)
        {
            qlEntityId = new QLEntityId(id);
            this.qlClass = qlClass;
        }

        public int Id => qlEntityId.Id;

        public QLEntityId QlEntityId => qlEntityId;



        public override string ToString()
        {
            var globalId = GlobalId;
            if (globalId != String.Empty)
                return "GId:" + globalId;
            
            return "IId:" + (Id-1);
        }

        public string GlobalId
        {
            get
            {
                var globalIdPart = this.GetPropertyValue("GlobalId");
                if (globalIdPart != null)
                    return globalIdPart.QLString.QLStr;
                else
                {
                    return string.Empty;
                }
            }
        }
    }

    public static class QLEntityExtension
    {
        public static IP21Repository Repository;
        private static string _lastClassName;
        private static string _lastProperty;
        private static int _lastIndex;
        private static bool _lastIsDirect;

        public static string GetGloablId(this QLEntity entity)
        {
            var part = GetPropertyValue(entity, "GlobalId");
            if (part == null)
                return string.Empty;

            return part.ToString();
        }

        public static QLPart GetPropertyValue(this QLEntity entity, string propertyName)
        {   
            //if (_lastProperty == propertyName && _lastClassName == entity.ClassName && _lastIndex != -1)
            //    return _lastIsDirect ? entity.QLDirectList[_lastIndex] : entity.QlInverseList[_lastIndex];

            var _lastIndex1 = Repository.DirectPropertyIndex(entity.ClassName, propertyName);
            if (_lastIndex1 != -1)
            {
                _lastClassName = entity.ClassName;
                _lastProperty = propertyName;
                _lastIsDirect = true;
                return entity.QLDirectList[_lastIndex1];
            }

            var _lastIndex2 = Repository.InversePropertyIndex(entity.ClassName, propertyName);
            if (_lastIndex2 != -1)
            {
                _lastClassName = entity.ClassName;
                _lastProperty = propertyName;
                _lastIsDirect = false;
                return entity.QlInverseList[_lastIndex2];
            }

            return null;
        }
    }

    public class QLExchangeFile
    {   
        public Schema CurrentSchema { get; set; }

        public List<QLEntity> QLExchangeFiles { get; } = new List<QLEntity>();

        public void Add(QLEntity qlEntity)
        {
            qlEntity.Schema = CurrentSchema;
            QLExchangeFiles.Add(qlEntity);
        }
    }

    public class QLPart
    {   
        public bool IsNull { set; get; }

        public bool IsEmptyList { set; get; }

        public QLEnum QLEnum { get; set; }

        public QLString QLString { get; set; }

        public QLEntityId QLEntityId { get; set; }

        public QLList QLList { get; set; }

        public QLClass QLClass { get; set; }

        public int? QLNumber { get; set; }

        public double? QLFloat { get; set; }

        public void SetFloat(string value)
        {
            value = value.EndsWith(".") ? value + "0" : value;

            QLFloat = double.Parse(value, CultureInfo.InvariantCulture);
        }

        public void SetNumber(string value)
        {
            QLNumber = int.Parse(value);
        }


        public override string ToString()
        {
            if (IsNull)
                return "Null";
            if (IsEmptyList)
                return "Empty";
            if (QLEnum != null)
                return QLEnum.QLStr;
            if (QLString != null)
                return QLString.QLStr;
            if (QLEntityId != null)
                return QLEntityId.Id.ToString();
            if (QLList != null)
                return QLList.ToString();
            if (QLClass != null)
                return QLClass.ToString();
            if (QLNumber != null)
                return QLNumber.ToString();
            if (QLFloat != null)
                return QLFloat.ToString();

            return string.Empty;
        }

    }

    public class QLProperty
    {
        public QLProperty(string propName, string type, NetType netType, bool optional)
        {
            if (propName[0] == '*')
            {
                PropName = propName.Substring(1, propName.Length - 1);
                Inverse = true;
            }
            else
            {
                PropName = propName;
                Inverse = false;
            }

            Type = type;
            NetType = netType;
            Optional = optional;
        }

        public string PropName { get; }

        public string Type { get; }

        public bool Optional { get; }

        public NetType NetType { get; }

        public bool Inverse { get; }
    }



}
