using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ABManager
{
    /// <summary>
    /// AssetBundleManifest
    /// </summary>
    AssetBundle assetBundleMain;
    AssetBundleManifest assetBundleManifest;


    /// <summary>
    /// 加载 AssetBundleManifest
    /// </summary>
    public void LoadAssetBundleManifest(string assetBundlePath)
    {
        assetBundleMain = AssetBundle.LoadFromFile(assetBundlePath);
        assetBundleManifest = assetBundleMain.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    }

    /// <summary>
    /// 卸载 AssetBundleManifest
    /// </summary>
    public void UnloadAssetBundleManifest()
    {
        if(assetBundleMain != null)
            assetBundleMain.Unload(true);
    }




    /// <summary>
    /// 加载所有AssetBundle
    /// </summary>
    public void LoadAllAssetBundle()
    {

    }




}