using UnityEngine;
using System;
using System.Collections;

namespace Engine
{
    public class EngineURLConst
    {
        public static readonly string RESOURCES_PATH = "assets/hotgun/resources/";
        public static readonly string CONFIG_PATH = "assets/configs/";
        public static readonly string HEADICON_PATH = "icons/heads";
        public static readonly string SKILLICON_PATH = "icons/skills";

        public static readonly string DATA_CONFIG = "data.assetbundle";

        public static string GetResource(string name)
        {
            return (RESOURCES_PATH + name + ".prefab").ToLower();
        }
    }
}