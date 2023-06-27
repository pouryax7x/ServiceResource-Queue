using ServiceResource.Enums;
using ServiceResource.Interfaces;
using System;
using Xunit;

namespace SR_Test
{
    public class EnumClassTests
    {
        [Fact]
        public void VerifyClassesExistForEnumMembers()
        {
            foreach (MethodName value in Enum.GetValues(typeof(MethodName)))
            {
                string className = value.ToString();
                string assemblyQualifiedName = $"ServiceResource.Services.{className}, ServiceResource";
                Type classType = Type.GetType(assemblyQualifiedName);

                Assert.True(classType != null, $"وجود ندارد {className}  : کلاس ");
            }
        }

        [Fact]
        public void VerifyClassesDrivedFromBaseSRService()
        {
            foreach (MethodName value in Enum.GetValues(typeof(MethodName)))
            {
                string className = value.ToString();
                string assemblyQualifiedName = $"ServiceResource.Services.{className}, ServiceResource";
                Type classType = Type.GetType(assemblyQualifiedName);

                var baseType = typeof(BaseSRService);
                //var genericArguments = classType.BaseType.GetGenericArguments();
                // var specificBaseType = baseType.MakeGenericType(genericArguments);

                Assert.True(baseType.IsAssignableFrom(classType));
            }
        }
    }
}
