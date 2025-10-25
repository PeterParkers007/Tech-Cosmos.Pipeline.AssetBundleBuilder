using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TechCosmos.AssetBundleBuilder.Data
{
    [System.Serializable]
    public class BundleNamingRule
    {
        [Header("ƥ��·���ؼ���")]
        public string pathKeyword = "";  // ��: "UI", "Character"

        [Header("����ģʽ")]
        public NamingPattern namingPattern = NamingPattern.TwoLevelFolders;

        [Header("�Զ������")]
        public string customPattern = ""; // ��: "ui/{0}/{1}"

        [Header("���ȼ�")]
        public int priority = 0;
    }
}

