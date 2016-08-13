using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Volte.Data.Dapper
{
    public class DynamicBuilder {
        public static Type DynamicCreateType(string className, IList<DynamicPropertyModel> lm)
        {
            AssemblyName DemoName = new AssemblyName("DynamicClass");
            AssemblyBuilder dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(DemoName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder mb = dynamicAssembly.DefineDynamicModule(DemoName.Name, DemoName.Name + ".dll");
            TypeBuilder tb = mb.DefineType(className + Guid.NewGuid().ToString().Replace("-", ""), TypeAttributes.Public);

            if (lm != null && lm.Count > 0) {
                foreach (var item in lm) {
                    createProperty(tb, item.Name, item.PropertyType);
                }
            }

            Type classType = tb.CreateType();
            return classType;
        }

        public static FieldBuilder createProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName,
                                            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                                            propertyType,
                                            Type.EmptyTypes);
            ILGenerator getIL = getPropMthdBldr.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);
            MethodBuilder setPropMthdBldr = tb.DefineMethod("set_" + propertyName,
                                            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                                            null, new Type[] { propertyType });
            ILGenerator setIL = setPropMthdBldr.GetILGenerator();
            /*
             * OpCodes.Ldarg_0:Ldarg是加载方法参数的意思。这里Ldarg_0事实上是对当前对象的引用即this。
             * 因为类的实例方法（非静态方法）在调用时，this 是会作为第一个参数传入的。
             */
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);  //OpCodes.Ldarg_1:加载参数列表的第一个参数了。
            setIL.Emit(OpCodes.Stfld, fieldBuilder);  //OpCodes.Stfld:用新值替换在对象引用或指针的字段中存储的值。
            setIL.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
            return fieldBuilder;
        }
    }
}
