using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TechCosmos.AssetBundleBuilder.Data
{
    [System.Serializable]
    public class BundleNamingRule
    {
        [Header("匹配路径关键词")]
        public string pathKeyword = "";  // 如: "UI", "Character"

        [Header("命名模式")]
        public NamingPattern namingPattern = NamingPattern.TwoLevelFolders;

        [Header("自定义参数")]
        public string customPattern = ""; // 如: "ui/{0}/{1}"

        [Header("优先级")]
        public int priority = 0;
    }
}

