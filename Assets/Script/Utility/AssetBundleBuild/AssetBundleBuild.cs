#if UNITY_EDITOR
using System.IO;
using UnityEditor;

namespace Script.Utility.AssetBundleBuild
{
    public class AssetBundleBuild : Editor
    {
        private static readonly string AssetBundleOutputPath = "./AssetBundles";


        [MenuItem("Tool/AssetBundle/Build")]
        public static void BuildAssetBundle()
        {
            BuildAssetBundles(AssetBundleOutputPath);
        }

        private static void BuildAssetBundles(string assetBundleOutPath, BuildTarget buildTarget = BuildTarget.StandaloneWindows)
        {
            var path = assetBundleOutPath + $"/{buildTarget.ToString()}";
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            else
            {
                Directory.CreateDirectory(path);
            }

            BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None,
                buildTarget);
        }
    }
}
#endif