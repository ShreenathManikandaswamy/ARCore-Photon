using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadAssetBundles : MonoBehaviour {

    AssetBundle MyLoadedAssetBundle;

    public string path;
    public string castle;

	// Use this for initialization
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadAssetBundle(path);
            InstantiateObjectFromBundle(castle);
        }
	}
	
    void LoadAssetBundle(string URL)
    {
        MyLoadedAssetBundle = AssetBundle.LoadFromFile(URL);
        Debug.Log(MyLoadedAssetBundle == null ? "Failed to load AssetBundle" : "AssetBundle Loaded");
    }

    void InstantiateObjectFromBundle(string assetname)
    {
        var prefab = MyLoadedAssetBundle.LoadAsset(assetname);
        Instantiate(prefab);
    }
}
