# UniVue

**UniVue是一给基于MVVM思想的Unity的UI解决方案框架。**它支持数据和模型的双向绑定，能够实现C#中WPF的功能，其底层UI模块依赖Unity的UGUI框架，除了数据和视图的双向绑定外，UniVue还实现高性能的UI事件系统，真正让你只关注游戏中数据的行为和视图的美术表现，这两者的交互全部依赖UniVue就能实现。



## **开发说明**

由于每个版本的源码有变动，实现方式也有变化，之前下面讲解中使用的图中的某些函数的名字已经改名，可能某些流程也已经改变，但是由于个人精力有限，实在挤不出时间写更完善的文档了，只能每次一点一点的写。emmmm，如果在使用过程中遇到了问题可以发邮件或者加我QQ，也可以提交Issue！**QQ：2947897147**；邮箱：**2947897147@qq.com**

我会在博客中对UniVue中的功能的使用进行讲解已经底层的实现逻辑、算法的实现。由于目前的电脑配置很差，开启Unity后录屏很卡，所有无法进行开视频进行讲解，emmm，后面升级装备了会在B站开视频进行讲解。

CSDN个人博客：[Avalon712-CSDN博客](https://blog.csdn.net/m0_62135731?spm=1000.2115.3001.5343)

这个仓库之后发布功能稳定的版本，因此很多最新的功能在这里是无法看见的，如果你想预览最新的功能，不妨fork一下这个仓库：https://github.com/Avalon712/UniVue-Develop 。这个仓库是我开发UniVue框架使用的项目，所有最新以及正在开发的功能都在这里可以看见，里面对每个功能都会有单元测试，也是学习UniVue框架的不二选择。



**UniVue的扩展框架：UniVue源生成器**

仓库地址：https://github.com/Avalon712/UniVue-SourceGenerator

关于UniVue中源生成器更详细的说明，请看这篇博客：[UniVue更新日志：使用源生成器优化Model和ViewModel层的设计-CSDN博客](https://blog.csdn.net/m0_62135731/article/details/139525492?spm=1001.2014.3001.5501)



**注：C#的源生器将会是完全替代反射的必要框架。通过生成低级代码完成对反射的替换，同时大幅度提高框架的易用性；**



**版本说明**

目前只对2021及其以上有支持，其它版本没试过，不知道什么情况，源码采用了很多C#9的语法，C#9似乎实在Unity2021开始支持的，如果你要使用源生器或许只能使用2021版本以上。同时不会对Unity都遗弃的功能进行支持，如：Text组件。因为如果对这些相同功能的组件也进行支持会导致ViewModel非常冗余，如果你要进行支持建议自己通过继承PropertyUI，然后看对于的组件是怎么实现更新逻辑的，照抄一遍就可以了。ViewModel的PropertyUI有将近30多个类，emmm，这就是我不打算支持的原因。后面PropertyUI还会进行扩展，可能会有70多个类。后续有时间了再看看要不要对2021版本以下的进行测试支持。



## 大版本核心功能一栏表

### **v1.0.0**

- **数据模型的双向绑定**。在UniVue中你永远不需要关心当数据更改时如何去更新视图，你只需要负责怎么去维护你的数据，即模型层，UniVue内置的更新逻辑会为你完美的实现实时的UI响应；
- **UI事件系统**。UniVue中内置的UI事件系统，能够实现你只需要一个EventCallAttribute特性标记一个方法就能在事件触发时自动回调这个函数，同时能够为你的方法参数自动赋值（具体的逻辑请参阅后面部分）。（在第一个版本中仅支持反射调用，预计在第二个版本中将会使用C#的源生成生成编译代码来替代反射的调用，对AOT这种编译方式提供更好的方式。）
- **Tween缓动动画**。UniVue内置一个简易补帧动画系统模块，以支持一些视图打开、关闭的动画效果，同时一些简单的定时器回调；
- **视图组件系统**。UniVue内置了一些比较常见的视图功能，如ListView、GridView、EnsureTipView(对话视图)、TipView等，同时通过使用**组合的方式**的方式你能够实现更多更灵活的视图功能。诸如循环列表、无限列表、循环背包等等内置都有视图组件，所有的使用方式只需要写一两行代码，甚至不用写代码；
- **视图控制系统。**UniVue中对所有的视图打开、关闭行为都受到严格的约束，你不必关系我要打开这个视图的视图那个视图应该被关闭等等，这些视图的控制逻辑全部由框架完成，你需要做的事件就是定义这些逻辑，而定义这些逻辑你只需想行了，不需要写任何代码、写任何配置，通过在构建UI的视图把这些逻辑想清楚，然后正确的命给名，OK，这些视图的展现逻辑就将如你想象那般，比如我想的是：“A视图打开时，B视图必须关闭；当C视图没有被打开时，不允许D视图打开........”这些逻辑真的就是靠想就可以了。关于这儿的更详细的描述请看后文。
- **规则引擎**。之所以你能够几乎不用写任何的UI代码，都依赖规则引擎的解析功能，对于每个视图都会被规则引擎在一个指定的时机进行规则解析得出上面的功能，然而第一个版本的规则引擎比较死板，不能够自定义扩展规则，后续版本将进行增强；

第一个版本只能算是一个基础的版本，实现了几乎所有核心的模块（还有几个核心模块在开发中。）

- **输入系统**。目前输入系统较为简单，只有两个类：**DraggableInput**和**MouseInput**，其中DraggableInput输入模块，允许你实现各种拖拽功能，同时通过DraggableInput输入的组合方式，能够实现更加灵活的方式。



### v1.1.0

- **IConsumableModel接口**：在v1.0.0版本的基础上新增IConsumableModel接口的功能，实现更快的更新速度更少的查询缓存占用；
- **源生成器同步更新至v1.1.0**：为支持IConsumableModel接口的功能，UniVue的源生成器提供了自动生成更新逻辑的代码；



### v1.2.0

- **支持自定义规则**：v1.2.0版本中全面优化了规则引擎的代码，完全单独抽离出来成为一个独立的模块，UniVue的所有模块都依赖规则引擎模块；



### v1.3.0

- **新增Modal模态视图**：处于ViewLevel.Modal级别的视图被打开后，在关闭它之前，不允许任何视图再打开；
- **优化System级别视图逻辑**：System级别的视图现在为同级互斥，具有相同父视图的视图为同一级，所有根视图（没有父视图的视图）为同一级；



### v1.4.0

- **运行时调试器**：通过UniVue>RuntimeDebuger打开调式器，运行时调试器可以方便地对模型绑定、事件绑定、路由事件绑定进行方便的调试，可以直接通过调试器完成各种UI测试；



### v1.5.0

- **本地化**：对多语言进行支持，通过Vue.SwitchLanguage()进行切换语言环境，内置支持解析的语言文件为属性文件格式（关于属性文件的格式自行查阅，是一种很适合做key-value结构的文件）；



### v2.0.0-preview

- 优化整个View层的设计，不再提供任何默认的IView接口实现，实现更多灵活的功能；
- 优化Model层的设计，源生成器将默认为实现IBindableModel接口的类提供一个BindableTypeInfo对象描述其绑定信息；

- 优化整个Event系统，**所有的反射调用全部移除，全部采用直接调用**，事件调用性能消耗成本几乎可以不计；

- **UniVue所有运行时模块都不再使用任何反射**，性能全面提高；
- 重构LoopList、LoopGrid组件，同时只保留了这两个组件，之前的其它组件全部删除不再使用；

- 优化规则引擎，通过使用内部实现ArrayPool和C#的Span减少数组对象的内存分布，同时对内部三大规则EventRule、ModelRule、RouteRule优化，字符串GC得到大幅度降低；

- ViewLevel新增Unmanaged级别的视图，此类的视图的打开关闭不受其它级别视图的影响，同时不会被压入视图堆栈；
- 重写运行时调式器，减少了95%以上的反射使用；

- 暂时移除I18n模块，在正式版本中发布此功能；

- 移除Input模块，考虑到Unity的InputSystem功能更加全面，UniVue将不再提供任何输入模块；

- 对内部的频繁使用数组对象（List）全面提高开启缓存功能，默认开启缓存；

- 命名规则只支持大写开头下划线分隔+UI后缀的方式，不再提高任何其它命名规则（过去提供的多种命名风格维护起来太困难，每修改新增一条规则就要有8种不同的实现，直接废弃了，UniVue2正式版中将会提供能够覆盖默认规则的实现接口）；
- 废弃AtomModel和GroupModel；



### 未来版本核心功能

**未来版本将会出现的核心功能**

2. **UI特效**。目前第一个版本的Tween补帧动画模块较为简单，无法支撑起更多复杂的UI效果，在后续版本中将引入强大的UI特效模块实现更多花哨的UI效果；
4. **UI优化算法**。UniVue在后续的版本可能会对UGUI的部分功能进行扩展，特别是Canvas的重绘逻辑，或许将基于屏幕后处理的原理实现一个更高性能的UI合批算法，降低UI的合批次数，减少Canvas的重绘，这个模块预计在UniVue3.0发布；
5. **C#源生成器**。第一个版本中的UniVue的源生成器只能对Model层进行优化，在后续版本中将彻底进行完善源生器框架，使用源生器生成所有的低级代码，将所有的低效代码进行优化重组，后续源生器或许将作为内置的选项而不是可选项；
7. **其它解放编码的功能：**更多的视图组件，如：灵活可变的循环列表视图，可多级展开的树形视图、多级菜单视图、大小可自定义的视图等等。
6. **对Lua的支持**：后序将会实现对Lua的支持。



## 二、Model层的使用

#### 不使用UniVue的源生成器

在不使用UniVue的源生成器时，你需要编写更多的低级代码，即在属性改变时调用Vue.Updater方法去更新UI，同时实现View层更高Model层数据的行为，此外为每个实现IBindableModel接口的类提供BindableTypeInfo对象，描述模型的绑定信息。

#### 使用UniVue的源生器

使用UniVue源生器需要你的Unity版本在2022及其以上，经过我的测试，在这个版本以下使用源生成器会出现源生成器生成的源文件无法被Unity的编译器编译的BUG。

在使用源生成器后，以下特性的功能将是可用的：

- **AlsoNotifyAttribute**：注解在字段上，当前属性更改时也通知指定的其它属性；

- **BindableAttribute**：注解此特性的类将自动实现IBindableModel接口，将会为这个类中所有字段自动生成属性方法；

- **CodeInjectAttribute**：在属性方法的指定位置注入指定代码；

- **DontNotifyAttribute**：指定不要为注解此特性的字段生成通知属性；

- **PropertyNameAttribute**：为字段定义属性名称；



## 三、ViewModel

​       ViewModel实现类是PropertyUI和UIBundle类。

###  1.PropertyUI 

​       这是一个细腻度的类，它关心的是一个UI与Model的某个属性值的绑定关系，以及如何将绑定的数据显示到绑定的UI组件上。该类是一个抽象类，其子类有很多实现，所有子类实现的目标都是一个：如何将可绑定的各种属性类型的数据显示的UI上。如：string类型的数据可以显示到TMP_Text、TMP_InputField两个UI组件上，而枚举类型可以显示当ToggleGroup、TMP_Dropdown、TMP_Text、TMP_InputField组件上......详见代码中的各种实现。

### 2.ModelUI 

​     这个类顾名思义，是维护了一个View与一个Model的关系（注意：一个View可以绑定多个Model，一个Model也可以绑定多个View）。同时这个类提供了对于那些可交互UI组件的对模型更新的通知行为。当ModelUI管理的PropertyUI中有UI更改了模型的数据就可以通过此类来通知Model进行更新，而更新权在IModelUpdater上，这意味者你可以实现单向的绑定。

### 3.ViewUpdater

管理所有的ModelUI对象，负责视图的更新，维护VMTable。



## 四、View

 视图是本框架一个很重要的概念，简单而言他是一个由很多UI组件构建成的一个完整用户界面。UniVue所有规则的最小区别单元就是一个View，每个View都必须有一个承载它的GameObject，这个GameObject在UniVue中成为ViewObject。

六个等级：Transient（瞬态，即打开后一段时间后会自动关闭）、Common（通用型，即可打开可关闭）、Modal（模态级别，打开后如果不关闭将不允许打开任何视图）、System（系统级，每次打开的**同级视图**中的系统级别视图只能有一个）、Permanent（持久级，不会被关闭，永远处于打开状态）、Unmanaged（非托管级，这个级别的视图的打开关闭不会被检查，同时不会被压入视图堆栈）

 **一种关系：父子关系**

父子关系的确定是根据ViewObject的层级关系确定的，拥有同一个父亲的视图处于同级视图，所有根视图（没有父亲的视图）也属于同级视图

**四个事件：Open、Close、Skip、Return。**

- Open：打开指定视图；
- Close：关闭指定视图；
- Skip：关闭当前视图同时打开指定视图；
- Return：关闭当前最新被打开的视图，打开上一个被关闭的视图.

==所有视图将自动被ViewRouter管理，ViewRouter将实现上面所有的功能。同时对所有非Unmanaged级别的视图的打开、关闭都必须经过ViewRouter。==

### 2.IView

定义了视图的所有行为，实现此接口的类被视为一个View对象，只有View对象都有它的ViewObject，这个ViewObject是自动加载的。

### 3.ViewRouter

这个类管理这所有的视图以及控制视图的打开、关闭行为。



## 五、事件系统

UniVue除了提供实现数据、视图的双向绑定外还提供了强大的事件系统。事件系统可以方便地实现对UI进行事件绑定，以及从UI组件上获取到事件参数。

### 1.EventCallAttribute

在方法上注解该特性，当此事件触发时将会进行调用此函数。标记此特性的方法不能是一个泛型方法以及方法参数不能有in、out关键字。同时该方法中的方法参数的类型以及参数名如果与UI中标记的一致，那么将会对其进行自动赋值。更强大的是，如果方法参数是一个自定义的实体类型，在满足实体的属性名以及类型与UI中标记的一致，也能对其进行赋值。(有关这一部分的详细细节，参阅**命名系统**[关于事件参数映射为方法参数的说明](#7.关于事件参数映射为方法参数的说明))

==注解该特性的方法支持的方法参数类型：**int、float、string、bool、enum（需要能够从UniVue.ViewModel.Enums中获取到枚举的信息）、EventUI、ArgumentUI、EventCall、Sprite、自定义类型(自定义类型需要注册IComstomArgument接口)**。==

### 2.EventUI

在UI组件中命名的事件将会对应一个EventUI事件。该事件封装的信息包括：触发该事件的视图名称、事件参数。**注意：即使是一个相同的事件，在不同的视图下触发时，其事件参数是不同。**

### 3.ArgumentUI

从UI组件上获取到的事件参数。

### 4.EventCall

每个注解了EventCallAttribute的方法将会对应一个EventCall对象。他表示当前正在被执行的事件。

### 5.IEventRegister

该接口定义了向事件管理器中注册事件、注销事件的行为。

### 8.EventManager

管理所有的事件。



## 六、简单的UI缓动动画系统

由于不想框架依赖诸如DOTween等第三方缓动动画技术框架，UniVue内部实现了一个简单的缓动系统。

### 1.DefaultViewTweens

所有默认的UI动画都由该类进行实现。

### 2.TweenBehavior

通过该类可以方便创建各种缓动效果。

### 3.TweenTaskExecutor

执行缓动任务。



## 七、命名系统

​     命名系统是整个UniVue的核心驱动，UniVue的所有核心模块都依靠命名系统完成。

### 1.命名规则解析引擎 - RuleEngine

 这个类实现了所有命名规则的解析、匹配。如果你的对命名还是不太清楚，可以看此类的源码。



### 2.通用说明

**UniVue不支持所有遗留的UI组件，如：Text、InputField、Dropdown。**

#### UI组件绑定模型支持的类型：

- int

- float

- bool

- enum以及注解了[Flags]特性的enum

- string

- Sprite（精灵图）

- List&lt;int&gt;

- List&lt;string&gt;

- List&lt;Sprite&gt;

- List&lt;flaot&gt;

- List&lt;bool&gt;

- List&lt;enum&gt;

  


#### 各种类型允许绑定的UI组件

- int: TMP_InputField、TMP_Text、Slider、**Toggles（Toggle.isOn的数量等于绑定的int的值）**、**Image（Filled）**
- float: TMP_InputField、TMP_Text、Slider、**Image（Filled）**
- bool: Toggle
- enum: TMP_Dropdown、ToggleGroup（多个Toggle组成单选效果）、TMP_Text、TMP_InputField
- string: TMP_InputField、TMP_Text
- [Flags] enum: Toggle（绑定多个Toggle可以实现复选效果）、TMP_Text
- Sprite: Image



#### UI组件名称

关于首字母大小写取决于命名格式（NamingFormat）

- Button: Button、Btn、button、btn

- TMP_InputField: Input、input

- TMP_Text: Text、text、txt、Txt

- Slider: Slider、slider

- Image : image、img、Img、Image

- Toggle: Toggle、toggle

- TMP_Dropdown: Dropdown、dropdown

  

### 3.数据绑定命名规则

数据绑定的命名规则决定了你的UI组件与模型数据是否能够实现双向绑定的关键。

#### 1）命名规则说明

​							**==[ModelName|TypeName] + PropertyName + UI组件名称==**

- 如果一个视图只绑定了一种类型的数据，那么可以省去这一部分；

- 如果一个视图绑定了多个相同类型的数据，那么需要依靠ModelName(自定义的名称)来加以区分数据绑定；

- 如果一个视图绑定了多个类型不同同时每个类型的数据都只有一种时，可以采用TypeName加以区分，这是**默认选项**；

- 注意这个命名规则中只有UI组件的名称的顺序可以改变，即UI组件的名称可以在最前（NamingFormat.UI_Prefix）也可以在最后（NamingFormat.UI_Suffix）；

- **注意无论你采用何种命名格式，类型名和属性名都必须与模型中的类型名与属性名保持一致性，这是区分大小写的；**

  

#### 2）举例说明

- Player_Name_Txt: 绑定一个类型为Player的Name属性的TMP_Text组件，同Player_Name_Text

- Player_level_Slider: 绑定一个类型为Player的level属性的Slider组件




### 4.视图事件绑定命名规则

#### 1）命名规则说明

规则说明：

​							==**路由事件(Open|Skip|Close|Return) + 视图名称 + 按钮UI组件的名称（仅支持Button和Toggle）**==

- 视图名称一定要存在，否则会导致不能实现路由事件。
- 注意这个命名规则中只有UI组件的名称的顺序可以改变，即UI组件的名称可以在最前（NamingFormat.UI_Prefix）也可以在最后（NamingFormat.UI_Suffix）；
- **对于Toggle绑定的视图事件，只有当toggle.isOn=true时会触发路由事件；**

#### 2）命名举例说明

- Open_PlayerInfoView_Btn：打开视图名为PlayerInfoView，同Open_PlayerInfoView_Button

- Skip_LoginView_Btn: 关闭当前视图，打开视图名为LoginView的视图，同Skip_LoginView_Button

- Close_RegisterView_Btn: 关闭视图名为RegisterView的视图，同Close_RegisterView_Button

- Return_Btn：关闭当前视图，打开上一次被关闭的视图，同Return_Button




### 5.事件命名规则

#### 1）命名规则说明

##### ①事件触发器命名

​													==**Evt+ 事件名+ UI组件名称**==

##### ②事件参数命名

​                                                 ==**Arg + 事件名[参数名] + UI组件名称**==

##### ③事件触发器+事件参数命名

​												**==Evt&Arg(或evt&arg) + 事件名[参数名] + UI组件名称==**

#### 2）举例说明

- 事件触发器：Evt_Login_Btn  登录事件(Login)触发按钮，InputEvtBuy 购买事件(Buy)触发输入

- 事件参数：Arg_Login[name]_Txt  登录事件(Login)参数(参数名为name)，参数值为TMP_Text.text

- 事件触发器+事件参数: Evt&Arg_Buy[num]_Slider  购买事件(Buy)触发Slider，事件参数名为num，参数值为Slider.value




### 6.综合命名

如果一个UI组件即绑定了数据又是一个事件参数，则使用" & "进行隔开两个命名规则，**注意'' & ''中&字符的前后均有一个空格！**如：**Player_Name_Txt & Arg_Login[Name]_Txt**。可以在VueConfig中修改这个字符。

**举例说明：**

- Player_Level_Slider & Arg_Grade[level]_Slider
- Open_PlayerInfoView_Btn & Evt_Refresh_Btn
- Room_ID_Input & Evt&Arg_JoinRoom[roomId]_Input
- Player_Level_Btn & Open_PlayerLevelGradeView_Btn & Evt_Grade_Btn



### 7.关于事件参数映射为方法参数的说明

如果事件参数是Arg_Login[name]_Input，那么方法参数的类型是string同时方法参数名为name，这样才能为方法参数正确赋值；简单而言就是赋值的过程必须是参数类型和参数名和事件参数返回的类型以及参数名一致才能正确赋值。

如果方法参数绑定的是一个自定义类型，那么要让事件参数成功映射为一个自定义类型，那么就要保证属性名、属性类型与事件参数的类型、参数名一致。下面以一个用户注册页面作为讲解：

假如这个页面具有这些UI：

- Arg_Signup[Name]_Input : 用户名称输入
- Arg_Signup[Password]_Input : 用户账号输入
- Arg_Signup[Email]_Input : 用户邮箱输入
- Evt_Signup_Btn : 用户注册按钮

注册事件(事件名为"Signup")回调方法如下:

```C#
[EventCall("Signup")]
private void UserSignup(User user){
 //......
}
```

要想成功将上面的事件参数映射为User自定义类型的对象，则User的各个属性以及属性名应该与事件参数相对应，即如下：

```C#
public class User{
    public string Name{get;set;} //Arg_Signup[Name]_Input
    public string Password{get;set;} //Arg_Signup[Password]_Input
    public string Email{get;set;} //Arg_Signup[Email]_Input 
    
    public User(){} //无参构造函数是必要的
}
```

同时你需要实现ICustomArgument接口同时注册到EventManager中来完成这个对象映射的过程。否则这个回调将永远不会被调用，为了防止异常，事件调用是保守性的，即所有参数都不为null才会进行调用。



## 八、编辑器扩展功能

在Unity的最上方的工具栏可以看见"**UniVue**"字样，点击，会出现提供的三个扩展功能。

### RenameEditorWindow

位置: **UniVu > RenameEditor**

功能：对视图下的所有GameObject按一定的规则进行重新命名，重新命名可以极大的加快组件查找效率，删去所有不必要的组件匹配与查找，提高程序性能。**（通过对树结构进行减枝，来实现加快查找）**

重名后的访问规则如下：

- **以字符'~'开头的GameObject及其后代GameObject都不会被进行组件查找；**
- **以字符'@'开头的GameObject不会被进行组件查找，但是其后代GameObject会被进行查找；**
- 你可以通过修改VueConfig文件修改上述字符，使用其它字符完成；



### RuntimeDebugerWindow

位置：**UniVue > RuntimeDebuger**

功能：可以在运行时实时调试，无需对代码进行调试，方便定位问题；



## 十、源生成器

最近的大更新中使用了源生成器来提高效率，关于UniVue中的源生成器，请看这篇博客：[UniVue更新日志：使用源生成器优化Model和ViewModel层的设计-CSDN博客](https://blog.csdn.net/m0_62135731/article/details/139525492?spm=1001.2014.3001.5501)


