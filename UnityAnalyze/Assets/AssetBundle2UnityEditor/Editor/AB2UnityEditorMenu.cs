using com.ihaiu;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AB2UnityEditorMenu
{
    [MenuItem("ihaiu tool/Open AB2UnityEditorWindow")]
    public static void OpenAB2UnityEditorWindow()
    {
        AB2UnityEditorWindow.Open();
    }


    [MenuItem("ihaiu tool/SeeAssetBundleInfoWindow")]
    public static void OpenASeeAssetBundleInfoWindow()
    {
        SeeAssetBundleInfoWindow.Open();
    }
}
