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



## #2024/7/14发布版本v1.4.0

1. **运行时调试器**：通过UniVue>RuntimeDebuger打开调式器，运行时调试器可以方便地对模型绑定、事件绑定、路由事件绑定进行方便的调试，可以直接通过调试器完成各种UI测试；
2. 修复ModelUtil无法更新List&lt;Enum&gt;的bug；



## #2024/7/18发布版本v1.5.0

1. **本地化**：现在对本地化提供了支持，本地化作为一个新的模块功能加入，其命名格式与模型绑定的命名格式大同小异，只不过模型名称部分必须是I18n，属性名部分为内容ID。本地化支持对TMP_Text文本内容进行显示也支持Image（注：内置的语言文件格式仅支持属性文件格式，如果你的语言文件格式不是属性文件，而是JSON、Excel等需要你自己实现文件解析的方法，即重写II18nResourceLoader中的LoadContents()接口方法，属性文件格式是一种最简单的key-value结构的文件格式）；
2. 优化PropertyUI的更新逻辑；
3. 优化枚举类型绑定TMP_Dropdown的逻辑，现在TMP_Dropdown显示的全部为枚举别名，如果没有枚举别名则显示为枚举值的字符串形式；
4. 新增ListDropdown的动态TMP_Dropdown，可以将List&lt;T&gt;的数据绑定到TMP_Dropdown组件上，动态显示值；
5. **EnumAliasAttribute支持多语言化**：每个枚举值的别名可分别为不同的语言进行设置不同的别名，当语言环境发生改变时这些值的显示也会同步显示当前语言环境对应的别名；



## #2024/7/26发布版本v1.5.1

1. **对int、float类型绑定Image进行支持**：当Image类型设置为了Filled填充类型时，可以将int、float类型绑定到Image组件上；
2. 删除对SuperGrid组件：太鸡肋了，后面会推出全新的可交互式组件系统替代；



## #2024/10/2发布UniVue2.0-preview

1.优化整个View层的设计，不再提供任何默认的IView接口实现，实现更多灵活的功能；

2.优化Model层的设计，源生成器将默认为实现IBindableModel接口的类提供一个BindableTypeInfo对象描述其绑定信息；

3.优化整个Event系统，**所有的反射调用全部移除，全部采用直接调用**，事件调用性能消耗成本几乎可以不计；

4.**UniVue所有运行时模块都不再使用任何反射**，性能全面提高；

5.重构LoopList、LoopGrid组件，同时只保留了这两个组件，之前的其它组件全部删除不再使用；

6.优化规则引擎，通过使用内部实现ArrayPool和C#的Span减少数组对象的内存分布，同时对内部三大规则EventRule、ModelRule、RouteRule优化，字符串GC得到大幅度降低；

7.ViewLevel新增Unmanaged级别的视图，此类的视图的打开关闭不受其它级别视图的影响，同时不会被压入视图堆栈；

8.重写运行时调式器，减少了95%以上的反射使用；

9.暂时移除I18n模块，在正式版本中发布此功能；

10.移除Input模块，考虑到Unity的InputSystem功能更加全面，UniVue将不再提供任何输入模块；

11.对内部的频繁使用数组对象（List）全面提高开启缓存功能，默认开启缓存；

12.命名规则只支持大写开头下划线分隔+UI后缀的方式，不再提高任何其它命名规则（过去提供的多种命名风格维护起来太困难，每修改新增一条规则就要有8种不同的实现，直接废弃了，UniVue2正式版中将会提供能够覆盖默认规则的实现接口）；

13.废弃AtomModel和GroupModel；



## #2024/12/14发布UniVue2.0-preview-fix

1.修复Return路由事件无法绑定的bug；

2.修复EnumDropdown初次绑定数据后渲染的数据被TMP_Dropdown的初始化覆盖的问题（修复方法：绑定数据后同步修改TMP_Dropdown的value属性的值）；

3.修复事件回调，当方法参数为EventUI但是事件的ArgumentUI为null时无法触发事件回调的bug；

4.移除Tween模块，考虑到有很多优秀的第三方库，这个模块应该作为用户可选项使用而不是和框架绑定在一起；