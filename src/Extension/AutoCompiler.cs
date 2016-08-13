using System;
using System.Collections;
using System.Collections.Generic;

using System.Runtime.CompilerServices;
using System.Data;

using System.Reflection.Emit;
using System.Reflection;
using Volte.Data.JsonObject;

namespace Volte.Data.Dapper
{
    internal  class AutoCompiler<T> {
        const string ZFILE_NAME = "AutoCompiler";
        public static object _PENDING;
        public static Dictionary<string, MyDelegate> Convert_Dict;

        public delegate List<T> MyDelegate(IDataReader _DataReader);

        private static readonly MethodInfo DataRecord_IsDBNull       = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
        private static readonly MethodInfo DataReader_Read           = typeof(IDataReader).GetMethod("Read");

        static AutoCompiler()
        {
            Convert_Dict = new Dictionary<string, MyDelegate>();
            _PENDING = new object();
        }

        public static void WhileLoopByAttributeMapping(ILGenerator gen, Type classType, List<AttributeMapping> _Attributes)
        {

            // Preparing locals
            LocalBuilder list = gen.DeclareLocal(typeof(System.Collections.Generic.List<>).MakeGenericType(classType));

            Label Loop = gen.DefineLabel();
            Label Exit = gen.DefineLabel();

            // Writing body
            gen.Emit(OpCodes.Nop);

            //*** List<ZULABELMEntity> list = new List<ZULABELMEntity>();
            gen.Emit(OpCodes.Newobj, typeof(System.Collections.Generic.List<>).MakeGenericType(classType).GetConstructor(Type.EmptyTypes));
            gen.Emit(OpCodes.Stloc_S, list);

            //*** while (IDataReader.Read()) {
            gen.MarkLabel(Loop);

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Callvirt, DataReader_Read);
            gen.Emit(OpCodes.Brfalse, Exit);

            LocalBuilder item = gen.DeclareLocal(classType);

            LocalBuilder[] colIndices = new LocalBuilder[_Attributes.Count];

            for (int i = 0; i < _Attributes.Count; i++) {
                colIndices[i] = gen.DeclareLocal(typeof(int));
            }

            setValueByAttributeMapping(gen, classType, _Attributes, item);

            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Callvirt, typeof(EntityObject).GetMethod("set_Verified"));

            //*** list.Add(item);
            gen.Emit(OpCodes.Ldloc_S, list);
            gen.Emit(OpCodes.Ldloc_S, item);
            gen.Emit(OpCodes.Callvirt, typeof(System.Collections.Generic.List<>).MakeGenericType(classType).GetMethod("Add"));

            //***}
            gen.Emit(OpCodes.Br, Loop);
            gen.MarkLabel(Exit);

            //*** _DataReader.Close();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Callvirt, typeof(IDataReader).GetMethod("Close"));

            //*** return list;
            gen.Emit(OpCodes.Ldloc_S, list);
            gen.Emit(OpCodes.Ret);
        }

        private static void setValueByAttributeMapping(ILGenerator gen, Type classType, List<AttributeMapping> _Attributes, LocalBuilder item)
        {
            // classType item = new classType();
            gen.Emit(OpCodes.Newobj, classType.GetConstructor(Type.EmptyTypes));
            gen.Emit(OpCodes.Stloc_S, item);
            int i = 0;

            foreach (AttributeMapping _Attribute in _Attributes) {
                Label common = gen.DefineLabel();

                //** if(!_DataReader.IsDBNull(0))
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldc_I4_S, i);
                gen.Emit(OpCodes.Callvirt, DataRecord_IsDBNull);
                gen.Emit(OpCodes.Stloc_3);
                gen.Emit(OpCodes.Ldloc_3);
                gen.Emit(OpCodes.Brtrue_S, common);
                gen.Emit(OpCodes.Ldloc_1);

                //obj.LABEL_ID=_DataReader.GetXXX(0);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldc_I4_S, i);
                gen.Emit(OpCodes.Callvirt, typeof(IDataRecord).GetMethod("Get" + _Attribute.Type));
                gen.Emit(OpCodes.Callvirt, classType.GetMethod("set_" + _Attribute.Name));
                //ZZTrace.Debug(ZFILE_NAME,"set_" + _Attribute.Name+" "+"Get" + _Attribute.Type );
                gen.Emit(OpCodes.Nop);
                gen.MarkLabel(common);

                i++;
            }
        }

        public static void WhileLoopByField(ILGenerator gen, Type classType, IDataReader _DataReader)
        {

            // Preparing locals
            LocalBuilder list = gen.DeclareLocal(typeof(System.Collections.Generic.List<>).MakeGenericType(classType));

            Label Loop = gen.DefineLabel();
            Label Exit = gen.DefineLabel();

            // Writing body
            gen.Emit(OpCodes.Nop);

            //*** List<ZULABELMEntity> list = new List<ZULABELMEntity>();
            gen.Emit(OpCodes.Newobj, typeof(System.Collections.Generic.List<>).MakeGenericType(classType).GetConstructor(Type.EmptyTypes));
            gen.Emit(OpCodes.Stloc_S, list);

            //*** while (IDataReader.Read()) {
            gen.MarkLabel(Loop);

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Callvirt,  DataReader_Read);
            gen.Emit(OpCodes.Brfalse, Exit);

            LocalBuilder item = gen.DeclareLocal(classType);

            LocalBuilder[] colIndices = new LocalBuilder[_DataReader.FieldCount];

            for (int i = 0; i < _DataReader.FieldCount; i++) {
                colIndices[i] = gen.DeclareLocal(typeof(int));
            }

            setValueByField(gen, classType, _DataReader, item);

            //*** list.Add(item);
            gen.Emit(OpCodes.Ldloc_S, list);
            gen.Emit(OpCodes.Ldloc_S, item);
            gen.Emit(OpCodes.Callvirt, typeof(System.Collections.Generic.List<>).MakeGenericType(classType).GetMethod("Add"));

            //***}
            gen.Emit(OpCodes.Br, Loop);
            gen.MarkLabel(Exit);

            //*** _DataReader.Close();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Callvirt, typeof(IDataReader).GetMethod("Close"));

            //*** return list;
            gen.Emit(OpCodes.Ldloc_S, list);
            gen.Emit(OpCodes.Ret);
        }

        private static void setValueByField(ILGenerator gen, Type classType, IDataReader _DataReader, LocalBuilder item)
        {
            // classType item = new classType();
            gen.Emit(OpCodes.Newobj, classType.GetConstructor(Type.EmptyTypes));
            gen.Emit(OpCodes.Stloc_S, item);
            int i = 0;

            int fieldCount = _DataReader.FieldCount;
            Dictionary<string, PropertyInfo> _Columns = new Dictionary<string, PropertyInfo>();
            PropertyInfo[] properties = classType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (PropertyInfo p in properties) {
                _Columns[p.Name.ToLower()] = p;
            }

            while (i < fieldCount) {
                string cKey = _DataReader.GetName(i).ToLower();

                if (_Columns.ContainsKey(cKey)) {
                    string Name = _Columns[cKey].Name;
                    string PropertyType = Type.GetTypeCode(_Columns[cKey].PropertyType).ToString();
                    Label common = gen.DefineLabel();

                    //** if(!_DataReader.IsDBNull(0))
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldc_I4_S, i);
                    gen.Emit(OpCodes.Callvirt, DataRecord_IsDBNull);
                    gen.Emit(OpCodes.Stloc_3);
                    gen.Emit(OpCodes.Ldloc_3);
                    gen.Emit(OpCodes.Brtrue_S, common);
                    gen.Emit(OpCodes.Ldloc_1);

                    //**obj.LABEL_ID=_DataReader.GetXXX(0);
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldc_I4_S, i);
                    gen.Emit(OpCodes.Callvirt, typeof(IDataRecord).GetMethod("Get" + PropertyType));
                    gen.Emit(OpCodes.Callvirt, classType.GetMethod("set_" + Name));
                    //ZZTrace.Debug(ZFILE_NAME,"ByField set_" + Name+" "+"Get" + PropertyType);
                    gen.Emit(OpCodes.Nop);
                    gen.MarkLabel(common);
                }

                //ZZTrace.Error(ZFILE_NAME,"["+cKey+"]");
                if (cKey == "verified") {
                    gen.Emit(OpCodes.Ldloc_1);
                    gen.Emit(OpCodes.Ldc_I4_1);
                    gen.Emit(OpCodes.Callvirt, typeof(EntityObject).GetMethod("set_Verified"));
                }

                i++;
            }
        }

        public static List<T> ConvertToEntity(Type classType, List<AttributeMapping> _Attributes, IDataReader _DataReader)
        {
            string entityName = classType.ToString();

            //ZZTrace.Debug(ZFILE_NAME,"ByAttributeMapping set_" + entityName);

            MyDelegate handler;

            lock (_PENDING) {
                if (Convert_Dict.ContainsKey(entityName)) {
                    handler = Convert_Dict[entityName];
                } else {
                    var _dynamicMethod = new DynamicMethod(string.Empty, typeof(List<T>),  new Type[] { typeof(IDataReader) }, typeof(AutoCompiler<T>));

                    ParameterBuilder _DataReader2 = _dynamicMethod.DefineParameter(1, ParameterAttributes.None, "_DataReader");
                    ILGenerator ilgen = _dynamicMethod.GetILGenerator();
                    WhileLoopByAttributeMapping(ilgen, classType, _Attributes);

                    handler = (MyDelegate) _dynamicMethod.CreateDelegate(typeof(MyDelegate));
                    Convert_Dict[entityName] = handler;
                }
            }

            return handler(_DataReader);
        }

        public static List<T> ConvertToEntity(IDataReader _DataReader)
        {
            Type classType = typeof(T);
            string entityName = "part_" + classType.ToString();

            //ZZTrace.Debug(ZFILE_NAME,entityName);

            MyDelegate handler;

            lock (_PENDING) {
                if (Convert_Dict.ContainsKey(entityName)) {
                    handler = Convert_Dict[entityName];
                } else {
                    DynamicMethod _dynamicMethod = new DynamicMethod(string.Empty, typeof(List<T>),  new Type[] { typeof(IDataReader) }, typeof(AutoCompiler<T>));

                    ParameterBuilder _DataReader2 = _dynamicMethod.DefineParameter(1, ParameterAttributes.None, "_DataReader");
                    ILGenerator ilgen = _dynamicMethod.GetILGenerator();
                    WhileLoopByField(ilgen, classType, _DataReader);

                    handler = (MyDelegate) _dynamicMethod.CreateDelegate(typeof(MyDelegate));
                    Convert_Dict[entityName] = handler;
                }
            }

            return handler(_DataReader);
        }
    }
}
