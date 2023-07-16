using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Recurses through a GameObject's entire hiararchy tree and sets the layer for each element to the one specified. Has a maximum depth of 50.
        /// </summary>
        public static void SetLayerForEntireHierarchy(this GameObject gameObject, int layer, int depth = 0)
        {
            if (depth >= 50)
            {
                return;
            }

            gameObject.layer = layer;

            foreach(Transform child in gameObject.transform)
            {
                SetLayerForEntireHierarchy(child.gameObject, layer, depth + 1);
            }
        }

        public static bool HasChildWithNameThatContains(this GameObject gameObject, string name)
        {
            List<Transform> children = gameObject.GetAllChildTransforms();

            return children.Any(child => child.name.Contains(name));
        }

        /// <summary>
        /// Gets all the transforms of the children of the gameObject and returns it.
        /// </summary>
        public static List<Transform> GetAllChildTransforms(this GameObject gameObject)
        {
            return _GetAllChildTransforms(gameObject, true);
        }

        private static List<Transform> _GetAllChildTransforms(GameObject gameObject, bool isRoot = false, List<Transform> transforms = null)
        {
            transforms = transforms ?? new List<Transform>();

            if (!isRoot)
            {
                transforms.Add(gameObject.transform);
            }

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                _GetAllChildTransforms(gameObject.transform.GetChild(i).gameObject, transforms: transforms);
            }

            return transforms;
        }
    }
}
