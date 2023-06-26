using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Utils
{
    public static class ThreadingUtils
    {
        private static ConcurrentBag<Action> PendingActions { get; set; } = new ConcurrentBag<Action>();

        public static void ExecutePendingActions()
        {
            while(PendingActions.Count > 0)
            {
                if (PendingActions.TryTake(out Action action))
                {
                    action();
                }
            }
        }

        public static void ExecuteOnMainThread(Action action)
        {
            PendingActions.Add(action);
        }
    }
}
