using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces
{
    public interface ISerializer
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string obj);
    }
}
