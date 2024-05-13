using UnityEngine;

namespace UniVue.View.Config
{
    public sealed class SceneConfig : ScriptableObject
    {
        /// <summary>
        /// 配置文件所属场景名称
        /// </summary>
        [Header("配置文件所属场景名称")]
        public string sceneName;

        /// <summary>
        /// 场景下所有视图配置
        /// </summary>
        [Header("场景下所有视图配置")]
        public CanvasConfig[] canvasConfigs;

    }
}
