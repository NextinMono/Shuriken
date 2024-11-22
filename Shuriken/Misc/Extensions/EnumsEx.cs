using System;

namespace Shuriken.Misc.Extensions;
public static class EnumsEx
{    public static T SetFlag<T>(this Enum value, T flag, bool set)
    {
        //https://stackoverflow.com/questions/5850873/enum-hasflag-why-no-enum-setflag
        Type underlyingType = Enum.GetUnderlyingType(value.GetType());

        // note: AsInt mean: math integer vs enum (not the c# int type)
        dynamic valueAsInt = Convert.ChangeType(value, underlyingType);
        dynamic flagAsInt = Convert.ChangeType(flag, underlyingType);
        if (set)
        {
            valueAsInt |= flagAsInt;
        }
        else
        {
            valueAsInt &= ~flagAsInt;
        }

        return (T)valueAsInt;
    }

}
