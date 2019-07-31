/*
* Author:  tangyanzhong
* Email:   rellikt@qq.com
* ABSetting是一个用来配置AssetBundle打包配置的工具，设置的规则是通过ABName定义ab包的包名，通过paths去定义这个包名下面包含了哪几个路径，
* 通过subFolderlevel来定义ab路径是用哪种方式去打包的，具体的定义方式可以参考Assets/Editor/Build/ABSetting.asset这个配置文件中的写法。
* 支持多个路径下的资源打包到同一ABName下，支持多种文件目录结构。
* 同时我们对需要输出的AB目录进行增删的时候要注意对setting的维护，这样才能在打包的时候得到正确的ABName，从而通过Unity的内置打包系统输出依赖关系
* 和我们需要的AB包。
* 每次完整出包前建议刷新AB配置。
*/

using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;


[Serializable]
public class ABInfo
{
    public string ABName;
    public List<UnityEngine.Object> paths;
    //public bool calculateDep;
    //0:当前目录下的文件全部打包成一个ab。
    //1:当前目录的子目录下的文件，按子目录的名字打包成一个AB。
    //2:当前目录的2级子目录下的文件，按目录的名字打包成一个AB。
    //3:场景打包类型，所有场景文件单独打包成一个ab包，放到Scene目录下。
    public int subFolderLevel;
}

[Serializable]
[CreateAssetMenu(fileName = "ABSetting", menuName = "AssetBundle/ABSetting")]
public class ABSetting : ScriptableObject
{
    public List<ABInfo> infos = new List<ABInfo>();

    public static ABSetting GetSetting()
    {
        return AssetDatabase.LoadAssetAtPath<ABSetting>("Assets/Editor/Build/ABSetting.asset");
    }

    [MenuItem("配置工具/同步场景BuildSetting到场景配置")]
    public static void UpdateConfigSceneSetting()
    {
        var setting = AssetDatabase.LoadAssetAtPath<ConfigScene>("Assets/RawResources/Config/ConfigScene.asset");
        var lst = EditorBuildSettings.scenes;
        var ret = new List<SceneInfo>();
        int i = 0;
        foreach (var value in lst)
        {
            if (!value.enabled) continue;
            ret.Add(new SceneInfo
            {
                index = i ++,
                name = value.path.Substring(value.path.LastIndexOf('/') + 1, value.path.Length - value.path.LastIndexOf('/') - 7)
            });
        }
        setting.scenes = ret;
        EditorUtility.SetDirty(setting);
        AssetDatabase.SaveAssets();
    }
}