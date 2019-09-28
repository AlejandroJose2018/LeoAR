using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateAssets : MonoBehaviour
{
    [MenuItem("Assets/Build Asset Bundles")]
    static void BuildABs()
    {
        BuildPipeline.BuildAssetBundles("Assets/ABs/Android", BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}
