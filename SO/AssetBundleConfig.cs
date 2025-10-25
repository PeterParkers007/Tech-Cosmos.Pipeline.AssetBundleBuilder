using System;
using System.Collections.Generic;
using UnityEngine;
using TechCosmos.AssetBundleBuilder.Data;
namespace TechCosmos.AssetBundleBuilder.SO
{
    [CreateAssetMenu(fileName = "AssetBundleConfig", menuName = "Tech-Cosmos/AssetBundle Config")]
    public class AssetBundleConfig : ScriptableObject
    {
        [Header("命名规则配置")]
        public List<BundleNamingRule> namingRules = new List<BundleNamingRule>();

        [Header("默认规则")]
        public DefaultNamingRule defaultRule = new DefaultNamingRule();

        [Header("高级选项")]
        public bool useFlatStructure = false;
        public int maxFolderDepth = 3;
        public string separator = "/";
    }
}
