## **#2024/6/17 发布UniVue v1.0.0**

核心基础模块的完成



## **#2024/6/26修复UniVue v1.0.0中的BUG:** 

1. 基于ViewConfig构建的视图时，可能会重复生成UIEvent和UIBundle的问题;
2. LoogGrid组件的布局错误问题;



## **#2024/6/26发布UniVue v1.0.1**

1. 优化VMTable，UI更新的复杂度为O(1)常数级;
2. VueConfig继承自ScriptableObject;
3. 优化LoopList、LoopGrid、ClampList组件的Item数据绑定逻辑;
4. 支持为ViewObject（GameObject）生成UIEvent、路由事件、模型数据绑定，无需再构建视图后才能进行绑定，这部分API见**ViewUtil**类;



## **#2024/6/27修复BUG**

1. 当ViewConfig的视图名称与文件名称不一致时导致错误的视图构建;



## **#2024/6/27将版本v1.0.1合并为v1.0.0**



## **#2024/6/29发布版本v1.1.0**

1. IBindableModel继承新的接口IConsumableModel，此接口能够实现不要将模型绑定到视图而是直接将数据更新到UI上，这样可以为以下两种场景带来方便：	

   - 情景一：“我不想绑定模型数据，只是单纯想将模型数据渲染到视图上，渲染完后这个模型可能不需要了”；
   - 情景二：“这个视图绑定的数据经常在变化，不想生成冗余的VMTable更新缓存”；

   总之使用这个功能可以极大的减少VMTable表中的Model更新缓存的数量，特别是针对LoopList、LoopGrid、ClampList这类组件（新版本中已经不会再生成VMTable的Model更新记录，极大减少缓存占用），同时使用IConsumableModel的功能使得更新速度更快，因为不会再二次查询VMTable；

2. UniVue源生器同步更新，为了对IConsumableModel接口功能的支持，源生器能够自动生成所有的更新逻辑的低级代码，强烈推荐使用源生器！



## **#2024/7/9发布版本v1.2.0**

1. **支持自定义规则**：对之前的规则引擎部分的代码进行完全重构，现在支持自定义规则（但是现在无法取代默认的规则实现），只需要实现接口IRuleFilter即可；

2. 优化UI事件命名：现在可以对一个UI进行多重事件设置以及参数设置，如下举例：

   ```
   Evt&Arg_SetPrice[price]_Input & Arg_Buy[price]_Input
   解释：TMP_InputField是输入内容可以作为事件SetPrice、Buy的事件参数，同时它本身也是一个事件SetPrice
   ```

3. 对多个规则的命名顺序不再进行要求：现在对命名顺序没有要求，可以按照任意顺序，之前必须按照："数据绑定 &amp; 视图事件 &amp; 自定义事件绑定"这样的顺序进行命名才能被正确解析，现在无需这样。但是任然建议在两个命名间使用可以识别的分隔符合来隔开增强可读性，原则上讲现在已经不需要再使用" & "符合进行隔开；

4. 优化UI事件的主动赋值操作：之前的SetEventArgs()只支持字典的方式传递参数值，现在可以不用字典，这样可以减少垃圾对象的产生；

5. UIQuerier可以对UIEvent进行查询操作；

6. ViewRouter只对根视图执行置顶操作；

7. 修复AtomModel、GroupModel对枚举类型无法进行执行UpdateModel()的bug；

8. 修复ModelUtil对枚举类型无法执行UpdateModel()的bug；



## **#2024/7/12发布版本v1.3.0**

1. 新增ViewLevel.Modal级别的视图等级，Modal模态视图打开时如果不关闭此视图无法打开任何视图（就是之前版本的forbid=true时行为）；
2. **优化System级别的视图的逻辑：现在System级别的视图为同级互斥。同级的含义指：具有相同的父视图，所有的根视图均为同一级。同级下永远只有一个System级别的视图被打开；**
3. 当父视图没有被打开时不允许打开其子视图；
4. 为事件系统新增IEntityMapper接口，为AOT编译提供基于非反射创建对象的实现，如果你的项目要进行IL2CPP编译，如果事件回调的参数有自定义的类型，你应该手动注册相应实体类型的接口实现，以实现事件回调时能够正确获得参数值；
5. 优化视图的构建逻辑，**现在必须要求视图名称就是ViewObject.name，同时所有名称以"View"结尾的GameObject将被视为一个ViewObject对象**；
6. 优化API的使用；