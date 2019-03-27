using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GoogleARCore;

public class LoadAssetFromServer : MonoBehaviour {


    GameObject castle;

    public Camera FirstPersonCamera;

    string url = "http://vellorechess.com/castle";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(WaitForReq());

        }
    }

    IEnumerator WaitForReq()
    {
        WWW www = new WWW(url);
        yield return www;
        AssetBundle bundle = www.assetBundle;
        Debug.Log(www.error);
        castle = Instantiate(bundle.LoadAsset("castle_mdl")) as GameObject;
    }
}
