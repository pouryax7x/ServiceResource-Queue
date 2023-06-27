using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ServiceResource.Enums;

[Generator]
public class EnumClassValidation
{
    public EnumClassValidation()
    {
        foreach (Service_MethodName value in Enum.GetValues(typeof(Service_MethodName)))
        {
            string className = value.ToString();
            string assemblyQualifiedName = $"ServiceResource.Services.{className}, ServiceResource";
            Type classType = Type.GetType(assemblyQualifiedName);
            if (classType == null)
            {
                throw new NotImplementedException();
            }
        }
    }
}
