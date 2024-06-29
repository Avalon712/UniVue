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



**#2024/6/29发布版本v1.1.0**

1. IBindableModel继承新的接口IConsumableModel，此接口能够实现不要将模型绑定到视图而是直接将数据更新到UI上，这样可以为以下两种场景带来方便：	

   - 情景一：“我不想绑定模型数据，只是单纯想将模型数据渲染到视图上，渲染完后这个模型可能不需要了”；
   - 情景二：“这个视图绑定的数据经常在变化，不想生成冗余的VMTable更新缓存”；

   总之使用这个功能可以极大的减少VMTable表中的Model更新缓存的数量，特别是针对LoopList、LoopGrid、ClampList这类组件（新版本中已经不会再生成VMTable的Model更新记录，极大减少缓存占用），同时使用IConsumableModel的功能使得更新速度更快，因为不会再二次查询VMTable；

2. UniVue源生器同步更新，为了对IConsumableModel接口功能的支持，源生器能够自动生成所有的更新逻辑的低级代码，强烈推荐使用源生器！