using Accessibility;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    public class Oleacc
    {
        [DllImport("oleacc.dll")]
        private static extern IntPtr AccessibleObjectFromPoint(Point pt, [Out, MarshalAs(UnmanagedType.Interface)] out IAccessible accObj, [Out] out object ChildID);

        public static IAccessible AccessibleObjectFromPoint(Point pt, out object ChildID)
        {
            AccessibleObjectFromPoint(pt, out IAccessible accObj, out ChildID);
            return accObj;
        }
    }
}
