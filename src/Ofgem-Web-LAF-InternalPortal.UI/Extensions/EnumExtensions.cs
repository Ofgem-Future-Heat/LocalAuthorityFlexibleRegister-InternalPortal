using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Ofgem_Web_LAF_InternalPortal.Extensions;

[ExcludeFromCodeCoverage]
public static class EnumExtensions
{
    public static string GetDescription(this Enum genericEnum)
    {
        Type genericEnumType = genericEnum.GetType();
        MemberInfo[] memberInfo = genericEnumType.GetMember(genericEnum.ToString());
        if ((memberInfo != null && memberInfo.Length > 0))
        {
            var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            if (_Attribs != null && _Attribs.Length > 0)
            {
                return ((System.ComponentModel.DescriptionAttribute)_Attribs[0]).Description;
            }
        }
        return genericEnum.ToString();
    }
}