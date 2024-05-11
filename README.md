# UniVue技术文档

## 一、简介

### **基于MVVM模式的UI框架**

​       一个原型的过程如下图显示

<img src="https://github.com/Avalon712/UniVue/blob/master/imgs/mvvm.png" alt="MV" width="800px" height="433px" />

​       与MVC、MVP模式不同的是，MVVM模式能够实现UI与模型的自更新行为，即当UI变化时他所关联的模型数据也能及时更新，当模型数据变化时UI显示的内容也能得到立即更新。而两者的中介者是ViewModel，本框架主要实现ViewModel的接口从上述图中可见分为是IUIUpdater、IUINotifier、IModelNotifier、IModelUpdater。


### 视图加载流程

<img src="https://github.com/Avalon712/UniVue/blob/master/imgs/视图加载流程.png" width="600px" height="634px" />



### 数据绑定流程

<img src="https://github.com/Avalon712/UniVue/blob/master/imgs/数据绑定流程.png" width="500px" height="324px" />



### UI更新流程

<img src="https://github.com/Avalon712/UniVue/blob/master/imgs/UI更新流程.png" width="450px" height="430px" />


### 模型更新流程

<img src="https://github.com/Avalon712/UniVue/blob/master/imgs/模型更新流程.png" width="550px" height="637px" />


### 事件触发流程

<img src="https://github.com/Avalon712/UniVue/blob/master/imgs/事件触发执行流程.png" width="440px" height="495px" />


### 资源卸载流程

<img src="https://github.com/Avalon712/UniVue/blob/master/imgs/资源卸载流程.png" width="450px" height="512px" />



### 自动装配与卸载EventCall的执行逻辑

<img src="https://github.com/Avalon712/UniVue/blob/master/imgs/自动装配与卸载EventCall的流程.png" width="1200px" height="609px" />



## 二、Model

### 1.IUIUpdater

该接口定义了对UI数据的更新行为

### 2.IUINotifier

该接口定义了当模型数据发生更该时通知UI进行更新的行为

### 3.IModelUpdater

该接口定义了如何更新模型数据

### 4.IModelNotifer

该接口定义了当用户的交互行为使得UI数据发生改变时，通知绑定了此UI的模型的数据进行更新的行为

### 5.IBindableModel : IModelUpdater, IUINotifier

该接口定义了可绑定模型数据的行为。

### 6.BaseModel : IBindableModel

Model层的实现基类是BaseModel，基类中提供了基于反射来显示模型数据更新的方法。但是如果你对程序性能要求很高，可以重写父类的UpdateModel()函数，这样可以减少基本数据类型的装箱操作带来的在堆上的小对象内存。

BaseModel实现了IUIUpdater, IModelUpdater接口。所有能被进行数据绑定的数据对象都应该继承自改类。

### 7.MonoModel : MonoBehaviour, IBindableModel

作用同BaseModel，与BaseModel不同的是，MonoModel继承了MonoBehaviour

### 8.ScriptableModel : ScriptableObject, IBindableMode

作用同BaseModel，与BaseModel不同的是，ScriptableModel 继承了ScriptableObject



## 三、ViewModel

​       ViewModel实现类是PropertyUI和UIBundle类。

###  1.PropertyUI : IUIUpdater

​       这是一个细腻度的类，它关心的是一个UI与Model的某个属性值的绑定关系，以及如何将绑定的数据显示到绑定的UI组件上。该类是一个抽象类，其子类有很多实现，所有子类实现的目标都是一个：如何将可绑定的各种属性类型的数据显示的UI上。如：string类型的数据可以显示到TMP_Text、TMP_InputField两个UI组件上，而枚举类型可以显示当ToggleGroup、TMP_Dropdown、TMP_Text、TMP_InputField组件上......详见代码中的各种实现。

### 2.UIBundle : IModelNotifier

​       这个类顾名思义，是维护了一个View与一个Model的关系（注意：一个View可以绑定多个Model，一个Model也可以绑定多个View）。同时这个类提供了对于那些可交互UI组件的对模型更新的通知行为。当UIBundle管理的PropertyUI中有UI更改了模型的数据就可以通过此类来通知Model进行更新，而更新权在IModelUpdater上，这意味者你可以实现单向的绑定。

### 3.ViewUpdater

管理所有的UIBundle对象，负责视图的更新。



## 四、View

 视图是本框架一个很重要的概念，简单而言他是一个由很多UI组件构建成的一个完整用户界面。但是它还包含另外一层概念：绑定数据。UniVue中对视图封装了一些视图打开、关闭动画，UI拖拽、优化的网格视图(GridView)、列表视图(ListView)，使用他们可以方便地创建背包、好友列表等，同时采用了高性能的滚动算法，即使有大量的数据显示，在滚动时也不会出现掉帧、卡帧，它们的性能消耗时非常低的。下面对框架提供的扩展View进行介绍。

**四个等级：Transient（瞬态，即打开后一段时间后会马上关闭）、Common（通用型，即可打开可关闭）、System（系统级，每次打开的系统级别视图只能有一个）、Permanent（持久级，不会被关闭，永远处于打开状态）。**

 **两种关系：Nested、Master。**

- 如果两个或多个视图处于Nested(嵌套)关系（意味着这些视图同属一个GameObject），那么当父视图没有被打开时，子视图不会被打开；当父视图被关闭时，子视图状态不变，即处于关闭的状态仍将处于关闭状态。
- 如果两个或多个视图是Master关系，那么当Master视图没有被打开时所有被Master控制的视图都不能被打开；当Master视图被关闭时，所有被Master视图控制的视图都会被关闭。

**四个事件：Open、Close、Skip、Return。**

- Open：打开指定视图；
- Close：关闭指定视图；
- Skip：关闭当前视图同时打开指定视图；
- Return：关闭当前最新被打开的视图，打开上一个被关闭的视图.

==所有视图将自动被ViewRouter管理，ViewRouter将实现上面所有的功能。同时对所有视图的打开、关闭都必须经过ViewRouter。==

### 1.IView

定义了视图的所有行为。

### 2.DynamincView : IView

这个视图可以通过new的方式动态地创建视图。它和BaseView的唯一区别就是DynamicView不继承自ScriptableObject。如果你想拓展自己的视图可以继承这个类或者实现IView接口，实现更多你自己想要实现的效果。

### 3.BaseView : ScriptableObject, IView

这是一个基类，但它不是抽象，他是一个UI界面的通用性。

这个类继承了ScipatbleObject，使得UI视图逻辑可以像资源那样被方便进行资源热更新。

### 4.GridView : BaseView

如果你想实现一个背包或网格视图。创建此类型视图是好的选择。没有复杂的API、没有复杂的代码就能实现一个高性能的滚动视图，即使是展现大量数据。同时利用绑定模型功能，可以轻松实现数据显示以及更新。

### 5.ListView : BaseView

同GridView。与GridView不同的是提供了无限滚动的功能，GridView提供这个功能感觉没有什么应用场景就没有提供，之前有的，后来删掉了这个功能。

### 6.TipView : BaseView

用于显示打开提示消息的视图，只显示一段提示消息。打开此视图建议通过TipView提高的Open()方法来指定要显示的提示消息的内容，如下所示：

```C#
        /// <summary>
        /// 打开当前的消息提示视图
        /// </summary>
        /// <param name="message">提示消息</param>
        /// <param name="top">是否显示与顶部</param>
        public void Open(string message,bool top=true)
        {
            _runtime.text.text = message;
            Vue.Router.Open(name, top);
        }
```



### 7.EnsuerTipView : BaseView

可以进行交互，玩家选择"确认"、"取消"选项。打开此视图建议通过EnsureTipView提供的Open()方法进行打开，可以绑定点击确认按钮时的回调，点击取消按钮时的回调。注意，这些回调函数是一次性的，当视图关闭后会自动注销这些事件。

```C#
/// <summary>
/// 打开此视图
/// </summary>
/// <remarks>注：事件绑定是暂时的，当视图关闭后会自动注销回调事件</remarks>
/// <param name="title">消息的标题,可以为null</param>
/// <param name="message">要显示的消息</param>
/// <param name="sure">点击"确定"按钮时回调函数</param>
/// <param name="cancel">点击"取消"按钮时回调函数</param>
public void Open(string title,string message,UnityAction sure,UnityAction cancel)
{
    if (_runtime.title != null) { _runtime.title.text = title; }
    _runtime.message.text = message;
    if(sure != null) { _runtime.sureBtn.onClick.AddListener(sure); }
    if(cancel != null) { _runtime.cancelBtn.onClick.AddListener(cancel); }
    Vue.Router.Open(name);
}
```



### 6.ViewRouter

这个类管理这所有的视图以及控制视图的打开、关闭行为。

### 7.IUIAudioEffectController

该接口定义了打开视图时播放的音效以及当视图被打开后执行回调函数、视图关闭后执行的回调函数。该接口可以方便地与你的音效系统进行绑定使用。

### 8.BaseView系列的使用讲解

在一个场景下你需要先创建一个**SceneConfig**配置文件（通过**ViewEditor**[ViewEditorWindow](#九.编辑器扩展功能)创建此文件），此文件包含了当前场景下所有的视图；此外还需要创建一个或多个**CanvasConfig**配置文件（通过**ViewEditor**[ViewEditorWindow](#九.编辑器扩展功能)创建此文件），此文件记录了哪些视图应该位于那个Canvas下（注意这个Canvas是指场景中存在的，而不是视图身上自带的Canvas）。之后将这些CanvasConfig配置文件引用给SceneConfig配置文件。最后当你需要加载当前场景下的视图时，只需加载当前场景的SceneConfig配置文件，然后使用下面的代码完成视图加载：

```C#
Vue.Instance.LoadViews(sceneConfig);
```

关于嵌套的视图（即一个视图下包含另外一个或多个视图）的配置，见[补充讲解](#八.补充讲解)。



## 五、事件系统

UniVue除了提供实现数据、视图的双向绑定外还提供了强大的事件系统。事件系统可以方便地实现对UI进行事件绑定，以及从UI组件上获取到事件参数。

### 1.EventCallAttribute

在方法上注解该特性，当此事件触发时将会进行调用此函数。标记此特性的方法不能是一个泛型方法以及方法参数不能有in、out关键字。同时该方法中的方法参数的类型以及参数名如果与UI中标记的一致，那么将会对其进行自动赋值。更强大的是，如果方法参数是一个自定义的实体类型，在满足实体的属性名以及类型与UI中标记的一致，也能对其进行赋值。(有关这一部分的详细细节，参阅**命名系统**[关于事件参数映射为方法参数的说明](#7.关于事件参数映射为方法参数的说明))

==注解该特性的方法支持的方法参数类型：**int、float、string、bool、enum、UIEvent、EventArg[]、EventCall、Sprite、自定义类型(不能是结构体)**。==

### 2.UIEvent

在UI组件中命名的事件将会对应一个UIEvent事件。该事件封装的信息包括：触发该事件的视图名称、事件参数。**注意：即使是一个相同的事件，在不同的视图下触发时，其事件参数是不同。**

### 3.EventArg

从UI组件上获取到的事件参数。

### 4.EventCall

每个注解了EventCallAttribute的方法将会对应一个EventCall对象。他表示当前正在被执行的事件。

### 5.IEventRegister

该接口定义了向事件管理器中注册事件、注销事件的行为。

### 6.UnityEventRegister : MonoBehaviour, IEventRegister

如果你想在一个MonoBehaviour中使用EventCallAttribute特性，你需要继承自该抽象类。

### 7.EventRegister : IEventRegister

如果你想在一个非MonoBehaviour中使用EventCallAttribute特性，你需要继承自该抽象类。

### 8.EventManager

管理所有的事件。

### 9.EventCallAutowireAttribute

只要在实现了接口**IEventRegister**的类上注解此特性，同时调用Vue.Instance.AutowireEventCalls()函数（此函数只会被执行一次）来装配事件注册器。注意事件注册器真正装配的时机是主动调用Vue.Instance.AutowireAndUnloadEventCalls()函数。



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

### 1.命名规则解析引擎

#### 1）NamingFormat

这个枚举类定义了常见的命名风格。注意：无论是哪种命名风格，指定UI组件名称都是必要的。

#### 2）NamingRuleEngine

 这个类实现了所有命名规则的解析、匹配。如果你的对命名还是不太清楚，可以看此类的源码，这个类是通过正则表达式来实现的解析和匹配。



### 2.通用说明

**UniVue不支持所有遗留的UI组件，如：Text、InputField、Dropdown。**

#### UI组件绑定模型支持的类型：

- int

- float

- bool

- enum以及注解了[Flags]特性的enum

- string

- Sprite（精灵图）

  


#### 各种类型允许绑定的UI组件

- int: TMP_InputField、TMP_Text、Slider、**Toggles（Toggle.isOn的数量等于绑定的int的值）**
- float: TMP_InputField、TMP_Text、Slider
- bool: Toggle、TMP_InputField、TMP_Text
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

**NamingFormat.CamelCase | NamingFormat.UI_Prefix**

- TxtPlayerName: 绑定一个类型为Player的Name属性的TMP_Text组件，同TextPlayerName;

- SliderPlayerLevel: 绑定一个类型为Player的Level属性的Slider组件

- TogglePlayerHobby: 绑定一个类型为Player的Hobby属性的Toggle组件

- ImgPlayerHeadImg: 绑定一个类型为Player的HeadImg属性的Image组件，同ImagePlayerHeadImg

  

**NamingFormat.CamelCase | NamingFormat.UI_Suffix**

- PlayerNameTxt: 绑定一个类型为Player的Name属性的TMP_Text组件，同PlayerNameText;

- PlayerLevelSlider: 绑定一个类型为Player的Level属性的Slider组件

- PlayerHobbyToggle: 绑定一个类型为Player的Hobby属性的Toggle组件

- PlayerHeadImgImg: 绑定一个类型为Player的HeadImg属性的Image组件，同PlayerHeadImgImag；

  

 **NamingFormat.UnderlineLower | NamingFormat.UI_Suffix**

- Player_Name_txt: 绑定一个类型为Player的Name属性的TMP_Text组件，同Player_Name_text
- Player_level_slider: 绑定一个类型为Player的level属性的Slider组件



**NamingFormat.UnderlineLower | NamingFormat.UI_Prefix**

- txt_Player_Name : 绑定一个类型为Player的Name属性的TMP_Text组件，同text_Player_Name
- slider_Player_level: 绑定一个类型为Player的level属性的Slider组件



**NamingFormat.SpaceLower | NamingFormat.UI_Suffix**

- Player Name txt: 绑定一个类型为Player的Name属性的TMP_Text组件，同Player Name text

- Player level slider: 绑定一个类型为Player的level属性的Slider组件

  

**NamingFormat.SpaceLower | NamingFormat.UI_Prefix**

- txt Player Name : 绑定一个类型为Player的Name属性的TMP_Text组件，同text Player Name

- slider Player level: 绑定一个类型为Player的level属性的Slider组件

  

**NamingFormat.SpaceUpper | NamingFormat.UI_Suffix**

- Player Name Txt: 绑定一个类型为Player的Name属性的TMP_Text组件，同Player Name Text

- Player level Slider: 绑定一个类型为Player的level属性的Slider组件

  

**NamingFormat.SpaceUpper | NamingFormat.UI_Prefix**

- Slider Player Level: 绑定一个类型为Player的Level属性的Slider组件

- Toggle Player Hobby: 绑定一个类型为Player的Hobby属性的Toggle组件

  

**NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix**

- Player_Name_Txt: 绑定一个类型为Player的Name属性的TMP_Text组件，同Player_Name_Text

- Player_level_Slider: 绑定一个类型为Player的level属性的Slider组件

  

**NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix**

- Txt_Player_Name: 绑定一个类型为Player的Name属性的TMP_Text组件，同Text_Player_Name
- Slider_Player_level: 绑定一个类型为Player的level属性的Slider组件



### 4.视图事件绑定命名规则

#### 1）命名规则说明

规则说明：

​							==**路由事件(Open|Skip|Close|Return) + 视图名称 + 按钮UI组件的名称（仅支持Button和Toggle）**==

- 视图名称一定要存在，否则会导致不能实现路由事件。
- 注意这个命名规则中只有UI组件的名称的顺序可以改变，即UI组件的名称可以在最前（NamingFormat.UI_Prefix）也可以在最后（NamingFormat.UI_Suffix）；
- **对于Toggle绑定的视图事件，只有当toggle.isOn=true时会触发路由事件；**

#### 2）命名举例说明

**NamingFormat.CamelCase | NamingFormat.UI_Prefix**

- BtnOpenPlayerInfoView：打开视图名为PlayerInfoView，同ButtonOpenPlayerInfoView

- BtnSkipLoginView: 关闭当前视图，打开视图名为LoginView的视图，同ButtonSkipLoginView

- BtnCloseRegisterView: 关闭视图名为RegisterView的视图，同ButtonCloseRegisterView

- BtnReturn：关闭当前视图，打开上一次被关闭的视图，同ButtonReturn

  

**NamingFormat.CamelCase | NamingFormat.UI_Suffix**

- OpenPlayerInfoViewBtn：打开视图名为PlayerInfoView，同OpenPlayerInfoViewButton

- SkipLoginViewBtn: 关闭当前视图，打开视图名为LoginView的视图，同SkipLoginViewButton

- CloseRegisterViewBtn: 关闭视图名为RegisterView的视图，同CloseRegisterViewButton

- ReturnBtn：关闭当前视图，打开上一次被关闭的视图，同ReturnButton

  


 **NamingFormat.UnderlineLower | NamingFormat.UI_Suffix**

- open_PlayerInfoView_btn：打开视图名为PlayerInfoView，同open_PlayerInfoView_button
- skip_LoginView_btn: 关闭当前视图，打开视图名为LoginView的视图，同skip_LoginView_button
- close_RegisterView_btn: 关闭视图名为RegisterView的视图，同close_RegisterView_button
- return_btn：关闭当前视图，打开上一次被关闭的视图，同return_button



**NamingFormat.UnderlineLower | NamingFormat.UI_Prefix**

- btn_open_PlayerInfoView：打开视图名为PlayerInfoView，同button_open_PlayerInfoView
- btn_skip_LoginView: 关闭当前视图，打开视图名为LoginView的视图，同button_skip_LoginView
- btn_close_RegisterView: 关闭视图名为RegisterView的视图，同button_close_RegisterView
- btn_return：关闭当前视图，打开上一次被关闭的视图，同button_return



**NamingFormat.SpaceLower | NamingFormat.UI_Suffix**

- open PlayerInfoView btn：打开视图名为PlayerInfoView，同open PlayerInfoView button

- skip LoginView btn: 关闭当前视图，打开视图名为LoginView的视图，同skip LoginView button

- close RegisterView btn: 关闭视图名为RegisterView的视图，同close RegisterView button

- return btn：关闭当前视图，打开上一次被关闭的视图，同return button

  


**NamingFormat.SpaceLower | NamingFormat.UI_Prefix**

- btn open PlayerInfoView：打开视图名为PlayerInfoView，同button open PlayerInfoView

- btn skip LoginView: 关闭当前视图，打开视图名为LoginView的视图，同button skip LoginView

- btn close RegisterView: 关闭视图名为RegisterView的视图，同button close RegisterView

- btn return：关闭当前视图，打开上一次被关闭的视图，同button return

  

**NamingFormat.SpaceUpper | NamingFormat.UI_Suffix**

- Open PlayerInfoView Btn：打开视图名为PlayerInfoView，同Open PlayerInfoView Button

- Skip LoginView Btn: 关闭当前视图，打开视图名为LoginView的视图，同Skip LoginView Button

- Close RegisterView Btn: 关闭视图名为RegisterView的视图，同Close RegisterView Button

- Return Btn：关闭当前视图，打开上一次被关闭的视图，同Return Button

  

**NamingFormat.SpaceUpper | NamingFormat.UI_Prefix**

- Btn Open PlayerInfoView：打开视图名为PlayerInfoView，同Button Open PlayerInfoView

- Btn Skip LoginView: 关闭当前视图，打开视图名为LoginView的视图，同Button Skip LoginView

- Btn Close RegisterView: 关闭视图名为RegisterView的视图，同Button Close RegisterView

- Btn Return：关闭当前视图，打开上一次被关闭的视图，同Button Return

  

**NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix**

- Open_PlayerInfoView_Btn：打开视图名为PlayerInfoView，同Open_PlayerInfoView_Button

- Skip_LoginView_Btn: 关闭当前视图，打开视图名为LoginView的视图，同Skip_LoginView_Button

- Close_RegisterView_Btn: 关闭视图名为RegisterView的视图，同Close_RegisterView_Button

- Return_Btn：关闭当前视图，打开上一次被关闭的视图，同Return_Button

  

**NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix**

- Btn_Open_PlayerInfoView：打开视图名为PlayerInfoView，同Button_Open_PlayerInfoView
- Btn_Skip_LoginView: 关闭当前视图，打开视图名为LoginView的视图，同Button_Skip_LoginView
- Btn_Close_RegisterView: 关闭视图名为RegisterView的视图，同Button_Close_RegisterView
- Btn_Return：关闭当前视图，打开上一次被关闭的视图，同Button_Return



### 5.事件命名规则

#### 1）命名规则说明

##### ①事件触发器命名

​													==**Evt(或evt) + 事件名+ UI组件名称**==

- 注意这个命名规则中只有UI组件的名称的顺序可以改变，即UI组件的名称可以在最前（NamingFormat.UI_Prefix）也可以在最后（NamingFormat.UI_Suffix）；

- 是Evt还是evt由命名风格决定；

  

##### ②事件参数命名

​                                                 ==**Arg(或arg) + 事件名[参数名] + UI组件名称**==

- 注意这个命名规则中只有UI组件的名称的顺序可以改变，即UI组件的名称可以在最前（NamingFormat.UI_Prefix）也可以在最后（NamingFormat.UI_Suffix）；
- 是Arg还是arg由命名风格决定；

##### ③事件触发器+事件参数命名

​												**==Evt&Arg(或evt&arg) + 事件名[参数名] + UI组件名称==**

- 注意这个命名规则中只有UI组件的名称的顺序可以改变，即UI组件的名称可以在最前（NamingFormat.UI_Prefix）也可以在最后（NamingFormat.UI_Suffix）；
- 是Evt&Arg还是evt&arg由命名风格决定；

#### 2）举例说明

**NamingFormat.CamelCase | NamingFormat.UI_Prefix**

- 事件触发器：BtnEvtLogin 登录事件(Login)触发按钮，InputEvtBuy 购买事件(Buy)触发输入

- 事件参数：TxtArgLogin[name] 登录事件(Login)参数(参数名为name)，参数值为TMP_Text.text

- 事件触发器+事件参数: SliderEvt&ArgBuy[num] 购买事件(Buy)触发Slider，事件参数名为num，参数值为Slider.value

  

**NamingFormat.CamelCase | NamingFormat.UI_Suffix**

- 事件触发器：EvtLoginBtn  登录事件(Login)触发按钮，InputEvtBuy 购买事件(Buy)触发输入

- 事件参数：ArgLogin[name]Txt  登录事件(Login)参数(参数名为name)，参数值为TMP_Text.text

- 事件触发器+事件参数: Evt&ArgBuy[num]Slider  购买事件(Buy)触发Slider，事件参数名为num，参数值为Slider.value

  


 **NamingFormat.UnderlineLower | NamingFormat.UI_Suffix**

- 事件触发器：evt_Login_btn  登录事件(Login)触发按钮，InputEvtBuy 购买事件(Buy)触发输入
- 事件参数：arg_Login[name]_txt  登录事件(Login)参数(参数名为name)，参数值为TMP_Text.text
- 事件触发器+事件参数: evt&arg_Buy[num]_slider  购买事件(Buy)触发Slider，事件参数名为num，参数值为Slider.value



**NamingFormat.UnderlineLower | NamingFormat.UI_Prefix**

- 事件触发器：btn_evt_Login 登录事件(Login)触发按钮，InputEvtBuy 购买事件(Buy)触发输入
- 事件参数：txt_arg_Login[name] 登录事件(Login)参数(参数名为name)，参数值为TMP_Text.text
- 事件触发器+事件参数: slider_evt&arg_Buy[num] 购买事件(Buy)触发Slider，事件参数名为num，参数值为Slider.value



**NamingFormat.SpaceLower | NamingFormat.UI_Suffix**

- 事件触发器：evt Login btn  登录事件(Login)触发按钮，InputEvtBuy 购买事件(Buy)触发输入

- 事件参数：arg Login[name] txt  登录事件(Login)参数(参数名为name)，参数值为TMP_Text.text

- 事件触发器+事件参数: evt&arg Buy[num] slider  购买事件(Buy)触发Slider，事件参数名为num，参数值为Slider.value

  


**NamingFormat.SpaceLower | NamingFormat.UI_Prefix**

- 事件触发器：btn evt Login 登录事件(Login)触发按钮，InputEvtBuy 购买事件(Buy)触发输入

- 事件参数：txt arg Login[name] 登录事件(Login)参数(参数名为name)，参数值为TMP_Text.text

- 事件触发器+事件参数: slider evt&arg Buy[num] 购买事件(Buy)触发Slider，事件参数名为num，参数值为Slider.value

  

**NamingFormat.SpaceUpper | NamingFormat.UI_Suffix**

- 事件触发器：Evt Login Btn  登录事件(Login)触发按钮，InputEvtBuy 购买事件(Buy)触发输入

- 事件参数：Arg Login[name] Txt  登录事件(Login)参数(参数名为name)，参数值为TMP_Text.text

- 事件触发器+事件参数: Evt&Arg Buy[num] Slider  购买事件(Buy)触发Slider，事件参数名为num，参数值为Slider.value

  

**NamingFormat.SpaceUpper | NamingFormat.UI_Prefix**

- 事件触发器：Btn Evt Login 登录事件(Login)触发按钮，InputEvtBuy 购买事件(Buy)触发输入

- 事件参数：Txt Arg Login[name] 登录事件(Login)参数(参数名为name)，参数值为TMP_Text.text

- 事件触发器+事件参数: Slider Evt&Arg Buy[num] 购买事件(Buy)触发Slider，事件参数名为num，参数值为Slider.value

  

**NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix**

- 事件触发器：Evt_Login_Btn  登录事件(Login)触发按钮，InputEvtBuy 购买事件(Buy)触发输入

- 事件参数：Arg_Login[name]_Txt  登录事件(Login)参数(参数名为name)，参数值为TMP_Text.text

- 事件触发器+事件参数: Evt&Arg_Buy[num]_Slider  购买事件(Buy)触发Slider，事件参数名为num，参数值为Slider.value

  

**NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix**

- 事件触发器：Btn_Evt_Login 登录事件(Login)触发按钮，InputEvtBuy 购买事件(Buy)触发输入

- 事件参数：Txt_Arg_Login[name] 登录事件(Login)参数(参数名为name)，参数值为TMP_Text.text

- 事件触发器+事件参数: Slider_Evt&Arg_Buy[num] 购买事件(Buy)触发Slider，事件参数名为num，参数值为Slider.value

  

### 6.综合命名

这儿以**NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix**风格举例说明。

如果一个UI组件即绑定了数据又是一个事件参数，则使用" & "进行隔开两个命名规则，**注意'' & ''中&字符的前后均有一个空格！**如：**Player_Name_Txt & Arg_Login[Name]_Txt**。

注意，多个命名规则下的顺序：**数据绑定 & 路由事件 & 自定义事件**

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



## 八、补充讲解

Model层要想实现当属性值发生改变时自动通知UI进行更新，那么建议是在属性的Set方法中进行调用NotifyUIUpdate()函数，如下：

```C#
public string Name
{
    get => _name;
    set
    {
        if(_name != value)
        {
            _name = value;
            NotifyUIUpdate(nameof(Name), value);
         }
    }
}
```



View中具有嵌套关系时，在创建被嵌套的视图时，无需指定viewObjectPrefab，而是在root视图中引用它，这样在进行视图构建时会自动为被嵌套的视图的viewObject进行赋值。同时被嵌套的视图无需赋值到CanvasConfig中！

**UniVue中所有具有管理者功能的对象（如ViewRouter、ViewUpdater、EventManger）都通过Vue这个全局单例对象来访问，同时Vue对象封装了一些API来简化某些函数的调用。**

**UIEvent、EventArg、UIBundle、PropertyUI都是基于视图隔离的，即使有两个相同的事件，但是他们在不同的视图中，那么UIEvent也是不同的。**



## 九、编辑器扩展功能

在Unity的最上方的工具栏可以看见"**UniVue**"字样，点击，会出现提供的三个扩展功能。

### **ViewEditorWindow**

位置: **UniVu > ViewEditor**

功能：提供了创建视图、视图配置的功能



### RenameEditorWindow

位置: **UniVu > RenameEditor**

功能：对视图下的所有GameObject按一定的规则进行重新命名，重新命名可以极大的加快组件查找效率，删去所有不必要的组件匹配与查找，提高程序性能。**（通过对树结构进行减枝，来实现加快查找）**

重名后的访问规则如下：

- **以字符'~'开头的GameObject及其后代GameObject都不会被进行组件查找；**
- **以字符'@'开头的GameObject不会被进行组件查找，但是其后代GameObject会被进行查找；**



### ScriptEditorWindow

位置: **UniVu > ScriptEditor**

功能：自动生成继承自BaseModel或UnityModel的脚本，同时能自动重写IBindable的所有方法，重写这些方法减少反射调用，是由必要的，如果你极度关心程序性能。

