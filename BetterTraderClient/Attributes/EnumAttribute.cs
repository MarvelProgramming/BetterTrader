using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderClient.Attributes
{
    /// <summary>
    /// Source: <seealso href="https://discussions.unity.com/t/enum-drop-down-menu-in-inspector-for-nested-arrays/19915/3"/>
    /// </summary>
    public class EnumAttribute : PropertyAttribute
    {
        public Type enumType;

        public EnumAttribute(Type enumType)
        {
            this.enumType = enumType;
        }
    }
}
