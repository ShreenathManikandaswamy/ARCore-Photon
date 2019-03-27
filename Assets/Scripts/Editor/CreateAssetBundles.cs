using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateAssetBundles : Editor {

    [MenuItem("Assets/ Build AssetBundles")]
    static void BuildAllAssetBundles(){
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
    }

	
}
