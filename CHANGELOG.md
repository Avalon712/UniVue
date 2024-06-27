**#2024/6/17 发布UniVue v1.0.0：核心基础模块的完成**



**#2024/6/26修复UniVue v1.0.0中的BUG:** 

1. 基于ViewConfig构建的视图时，可能会重复生成UIEvent和UIBundle的问题;
2. LoogGrid组件的布局错误问题;



**#2024/6/26发布UniVue v1.0.1：核心模块优化**

1. 优化VMTable，UI更新的复杂度为O(1)常数级;
2. VueConfig继承自ScriptableObject;
3. 优化LoopList、LoopGrid、ClampList组件的Item数据绑定逻辑;
4. 支持为ViewObject（GameObject）生成UIEvent、路由事件、模型数据绑定，无需再构建视图后才能进行绑定，这部分API见**ViewUtil**类;



**#2024/6/27修复BUG**

1. 当ViewConfig的视图名称与文件名称不一致时导致错误的视图构建;



**#2024/6/27将版本v1.0.1合并为v1.0.0**