using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TechCosmos.AssetBundleBuilder.Data
{
    [System.Serializable]
    public class DefaultNamingRule
    {
        public NamingPattern pattern = NamingPattern.ParentFolder;
        public string customPattern = "misc/{0}";
    }
}
