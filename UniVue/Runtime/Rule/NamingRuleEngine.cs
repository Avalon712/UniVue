using System;
using System.Text.RegularExpressions;
using UniVue.Utils;
using UniVue.View;

namespace UniVue.Rule
{
    /// <summary>
    /// <para>数据绑定的基本名称匹配规则如下: </para>
    /// <para>"【ModelName 或 TypeName】+ PropertyName + UI组件名称(可简写)"  
    /// 注: 括号部分为可选，但是当一个视图绑定了两个模型数据，如果这两个模型不是同一类型
    /// ，可用使用TypeName区分；如果是相同类型且具有相同属性名称的则应该使用一个指定的模型
    /// 名称去区分；第一个括号完全可省的情况是这个视图只绑定了一个模型数据，且都是基本数据
    /// (即:基本数据类型\enum\string)；
    /// </para>
    /// <para>视图导航事件(如:打开视图、关闭视图、返回上一个视图、跳转到另一个视图)的名称匹配规则如下: </para>
    /// <para>"事件+视图名称+Btn|btn"  注：视图事件有:Open、Close、Return、Skip。 此外注意命名的Btn或btn是不可省略的</para>
    /// <para>自定义事件匹配规则：</para>
    /// <para>事件触发器：Evt + EvtName + UI组件名称（可简写）</para>
    /// <para>事件参数：Arg + EvtName[ArgName] + UI组件名称（可简写）</para>
    /// <para>如果一个UI即是事件参数又是事件触发器，则命名规则为: Evt&amp;Arg + EvtName[ArgName] + UI组件名称（可简写）</para>
    /// <para>事件触发器命名举例：Evt_Buy_Btn ：购买事件，事件名称为"Buy"，触发器为Button</para>
    /// <para>事件参数命名：Arg_Buy[Num]_Slider：购买事件的参数，参数名称为Num，UI为Slider</para>
    /// <para>如果想要参数对象映射到一个对象，则可以按以下规则进行：</para>
    /// <para>
    /// 登录事件举例：Evt_Login_Btn。Arg_Login[Name]_Input、 Arg_Login[Password]_Input，如果想要映射
    /// 为一个User对象，则这个User对象必须有属性Name、Password，即参数名称于事件名称一致，这样当Login
    /// 事件触发是，通过"GetCurrentEventArg&lt;User&gt;()"函数可用获得一个User对象。
    /// </para>
    /// <para>
    /// 以上四种命名规则可以进行组合，类型之间的命名分隔规则为" &amp; "（注意空格是必要的）,如：
    /// "Player_Name_Txt &amp; Arg_Login[name]_Txt"。同时其顺序也是固定的：数据绑定 &amp; 视图事件 &amp; 自定义事件绑定
    /// </para>
    /// </summary>
    public static class NamingRuleEngine
    {
        /// <summary>
        /// 检查绑定数据的UI的名称uiName的命名是否符合规则
        /// </summary>
        /// <param name="uiName">绑定数据的UI的名称</param>
        /// <param name="propertyName">绑定的属性名称</param>
        /// <param name="modelName">模型名称</param>
        /// <returns>是否匹配</returns>
        public static bool CheckDataBindMatch(string uiName,string modelName,string propertyName)
        {
            if(!uiName.Contains(" & "))
            {
                NamingFormat format = Vue.Config.Format;
                return Regex.IsMatch(uiName, GetDataBindRegex(ref format, modelName, propertyName));
            }
            else
            {
                //数据绑定的索引为0
                return CheckDataBindMatch(uiName.Split(" & ")[0], modelName, propertyName);
            }
        }

        /// <summary>
        /// 检查绑定事件的UI的名称uiName的命名是否符合命名规则
        /// </summary>
        /// <param name="uiName">绑定视图事件的UI的名称</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="viewName">如果匹配成功，表示动作对象(即视图名称)的名称</param>
        /// <returns>是否匹配</returns>
        public static bool CheckRouterEventMatch(string uiName,RouterEvent eventName, out string viewName)
        {
            viewName = null;
            NamingFormat format = Vue.Config.Format;
            if (!uiName.Contains(" & "))
            {
                int viewNameGroupIdx;
                Match match = Regex.Match(uiName, GetRouterEventRegex(ref format,ref eventName, out viewNameGroupIdx));
                if (eventName != RouterEvent.Return && match.Success)
                {
                    viewName = match.Groups[viewNameGroupIdx].Value;
                }
                return match.Success;
            }
            else
            {
                //视图事件的索引为倒数第二
                string[] strs = uiName.Split(" & ");
                return CheckRouterEventMatch(strs[strs.Length-2], eventName, out viewName);
            }
        }

        /// <summary>
        /// 检查绑定自定义的事件参数的UI的名称命名是否符合规则
        /// </summary>
        /// <param name="uiName">绑定自定义事件参数的UI的名称</param>
        /// <param name="eventName">如果匹配成功则返回事件名称</param>
        /// <param name="argName">如果匹配成功则返回事件参数的名称</param>
        /// <param name="isOnlyEvt">当前是否只是事件触发器</param>
        /// <param name="isOnlyArg">当前是否只是事件参数</param>
        /// <returns>是否匹配成功</returns>
        public static bool CheckCustomEventAndArgMatch(string uiName, out string eventName, out string argName,out bool isOnlyEvt,out bool isOnlyArg)
        {
            eventName = argName = null;
            NamingFormat format = Vue.Config.Format;
            if (!uiName.Contains(" & "))
            {
                isOnlyEvt = false;
                isOnlyArg = false;
                int eventNameIdx, argNameIdx=-1;
                Match match;
                if (uiName.Contains('&')) //Evt&Arg
                {
                    match = Regex.Match(uiName, GetCustomEventAndArgRegex(out eventNameIdx, ref format, out argNameIdx));
                }
                else if(uiName.Contains("Arg",StringComparison.OrdinalIgnoreCase)) //Arg
                {
                    isOnlyArg = true;
                    match = Regex.Match(uiName, GetCustomEventArgRegex(out eventNameIdx, ref format, out argNameIdx));
                }
                else //Evt
                {
                    isOnlyEvt = true;

                    match = Regex.Match(uiName, GetCustomEventRegex(ref format,out eventNameIdx));
                }

                if (match.Success)
                {
                    eventName = match.Groups[eventNameIdx].Value;
                    if (argNameIdx != -1) { argName = match.Groups[argNameIdx].Value; }
                }

                return match.Success;
            }
            else
            {
                string[] names = uiName.Split(" & ");
                //自定义事件一定是在最后一个
                return CheckCustomEventAndArgMatch(names[names.Length - 1], out eventName, out argName,out isOnlyEvt,out isOnlyArg);
            }
        }

        /// <summary>
        /// 简单的进行命名匹配(简单地判断一个UI的命名是否是特殊命名，这个判断结果可能是错误的)
        /// 注：想要实现高精度的匹配办法就是使用完全不一样的命名风格，比如：以下划线隔开的可以选驼峰式或空格式这样的命名
        /// </summary>
        /// <param name="uiName"></param>
        /// <returns></returns>
        public static bool FullFuzzyMatch(NamingFormat format,string uiName)
        {
            if (UITypeUtil.GetUIType(uiName) == UIType.None) return false;

            int f = (int)format;
            int t = f > 64 ? f - 64 : f - 32;

            switch (t)
            {
                case 1:
                    return !uiName.Contains('_') && (!uiName.Contains(' ') || uiName.Contains(" & "));
                case 2:
                    return uiName.Contains('_') && (!uiName.Contains(' ') || uiName.Contains(" & "));
                case 4:
                    return uiName.Contains(' ') && !uiName.Contains('_');
                case 8:
                    return uiName.Contains(' ') && !uiName.Contains('_');
                case 16:
                    return uiName.Contains('_') && (!uiName.Contains(' ') || uiName.Contains(" & "));
            }
            return false;
        }

        private static string GetCustomEventAndArgRegex(out int eventNameIdx,ref NamingFormat format ,out int argNameIdx)
        {
            eventNameIdx = argNameIdx = -1;
            switch (format)
            {
                case NamingFormat.CamelCase | NamingFormat.UI_Suffix:
                    eventNameIdx = 1; argNameIdx = 2;
                    return @"^Evt&Arg(\w{1,})\[(\w{1,})\](Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)$";
                case NamingFormat.CamelCase | NamingFormat.UI_Prefix:
                    eventNameIdx = 2; argNameIdx = 3;
                    return @"^(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)Evt&Arg(\w{1,})\[(\w{1,})\]$";

                case NamingFormat.UnderlineLower | NamingFormat.UI_Suffix:
                    eventNameIdx = 1; argNameIdx = 2;
                    return @"^evt&arg_(\w{1,})\[(\w{1,})\]_(slider|txt|text|input|dropdown|toggle|btn|img|button|image)$";
                case NamingFormat.UnderlineLower | NamingFormat.UI_Prefix:
                    eventNameIdx = 2; argNameIdx = 3;
                    return @"^(slider|txt|text|input|dropdown|toggle|btn|img|button|image)_evt&arg_(\w{1,})\[(\w{1,})\]$";

                case NamingFormat.SpaceLower | NamingFormat.UI_Suffix:
                    eventNameIdx = 1; argNameIdx = 2;
                    return @"^evt&arg (\w{1,})\[(\w{1,})\] (slider|txt|text|input|dropdown|toggle|btn|img|button|image)$";
                case NamingFormat.SpaceLower | NamingFormat.UI_Prefix:
                    eventNameIdx = 2; argNameIdx = 3;
                    return @"^(slider|txt|text|input|dropdown|toggle|btn|img|button|image) evt&arg (\w{1,})\[(\w{1,})\]$";

                case NamingFormat.SpaceUpper | NamingFormat.UI_Suffix:
                    eventNameIdx = 1; argNameIdx = 2;
                    return @"^Evt&Arg (\w{1,})\[(\w{1,})\] (Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)$";
                case NamingFormat.SpaceUpper | NamingFormat.UI_Prefix:
                    eventNameIdx = 2; argNameIdx = 3;
                    return @"^(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image) Evt&Arg (\w{1,})\[(\w{1,})\]$";

                case NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix:
                    eventNameIdx = 1; argNameIdx = 2;
                    return @"^Evt&Arg_(\w{1,})\[(\w{1,})\]_(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)$";
                case NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix:
                    eventNameIdx = 2; argNameIdx = 3;
                    return @"^(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)_Evt&Arg_(\w{1,})\[(\w{1,})\]$";
            }
            return null; //不能执行到这儿
        }

        private static string GetCustomEventArgRegex(out int eventNameIdx, ref NamingFormat format, out int argNameIdx)
        {
            eventNameIdx = argNameIdx = -1;
            switch (format)
            {
                case NamingFormat.CamelCase | NamingFormat.UI_Suffix:
                    eventNameIdx = 1; argNameIdx = 2;
                    return @"^Arg(\w{1,})\[(\w{1,})\](Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)$";
                case NamingFormat.CamelCase | NamingFormat.UI_Prefix:
                    eventNameIdx = 2; argNameIdx = 3;
                    return @"^(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)Arg(\w{1,})\[(\w{1,})\]$";

                case NamingFormat.UnderlineLower | NamingFormat.UI_Suffix:
                    eventNameIdx = 1; argNameIdx = 2;
                    return @"^arg_(\w{1,})\[(\w{1,})\]_(slider|txt|text|input|dropdown|toggle|btn|img|button|image)$";
                case NamingFormat.UnderlineLower | NamingFormat.UI_Prefix:
                    eventNameIdx = 2; argNameIdx = 3;
                    return @"^(slider|txt|text|input|dropdown|toggle|btn|img|button|image)_arg_(\w{1,})\[(\w{1,})\]$";

                case NamingFormat.SpaceLower | NamingFormat.UI_Suffix:
                    eventNameIdx = 1; argNameIdx = 2;
                    return @"^arg (\w{1,})\[(\w{1,})\] (slider|txt|text|input|dropdown|toggle|btn|img|button|image)$";
                case NamingFormat.SpaceLower | NamingFormat.UI_Prefix:
                    eventNameIdx = 2; argNameIdx = 3;
                    return @"^(slider|txt|text|input|dropdown|toggle|btn|img|button|image) arg (\w{1,})\[(\w{1,})\]$";

                case NamingFormat.SpaceUpper | NamingFormat.UI_Suffix:
                    eventNameIdx = 1; argNameIdx = 2;
                    return @"^Arg (\w{1,})\[(\w{1,})\] (Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)$";
                case NamingFormat.SpaceUpper | NamingFormat.UI_Prefix:
                    eventNameIdx = 2; argNameIdx = 3;
                    return @"^(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image) Arg (\w{1,})\[(\w{1,})\]$";

                case NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix:
                    eventNameIdx = 1; argNameIdx = 2;
                    return @"^Arg_(\w{1,})\[(\w{1,})\]_(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)$";
                case NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix:
                    eventNameIdx = 2; argNameIdx = 3;
                    return @"^(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)_Arg_(\w{1,})\[(\w{1,})\]$";
            }
            return null; //不可能执行到这儿
        }

        private static string GetCustomEventRegex(ref NamingFormat format, out int eventNameIdx)
        {
            eventNameIdx = -1;
            switch (format)
            {
                case NamingFormat.CamelCase | NamingFormat.UI_Suffix:
                    eventNameIdx = 1;
                    return @"^Evt(\w{1,})(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)$";
                case NamingFormat.CamelCase | NamingFormat.UI_Prefix:
                    eventNameIdx = 2;
                    return @"^(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)Evt(\w{1,})$";

                case NamingFormat.UnderlineLower | NamingFormat.UI_Suffix:
                    eventNameIdx = 1;
                    return @"^evt_(\w{1,})_(slider|txt|text|input|dropdown|toggle|btn|img|button|image)$";
                case NamingFormat.UnderlineLower | NamingFormat.UI_Prefix:
                    eventNameIdx = 2;
                    return @"^(slider|txt|text|input|dropdown|toggle|btn|img|button|image)_evt_(\w{1,})$";

                case NamingFormat.SpaceLower | NamingFormat.UI_Suffix:
                    eventNameIdx = 1;
                    return @"^evt (\w{1,}) (slider|txt|text|input|dropdown|toggle|btn|img|button|image)$";
                case NamingFormat.SpaceLower | NamingFormat.UI_Prefix:
                    eventNameIdx = 2;
                    return @"^(slider|txt|text|input|dropdown|toggle|btn|img|button|image) evt (\w{1,})$";

                case NamingFormat.SpaceUpper | NamingFormat.UI_Suffix:
                    eventNameIdx = 1;
                    return @"^Evt (\w{1,}) (Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)$";
                case NamingFormat.SpaceUpper | NamingFormat.UI_Prefix:
                    eventNameIdx = 2;
                    return @"^(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image) Evt (\w{1,})$";

                case NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix:
                    eventNameIdx = 1;
                    return @"^Evt_(\w{1,})_(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)$";
                case NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix:
                    eventNameIdx = 2;
                    return @"^(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)_Evt_(\w{1,})$";
            }
            return null; //不能执行到这儿
        }

        private static string GetRouterEventRegex(ref NamingFormat format, ref RouterEvent eventName, out int viewNameGroupIdx)
        {
            viewNameGroupIdx = (Vue.Config.Format & NamingFormat.UI_Suffix) == NamingFormat.UI_Suffix ? 1 : 2;

            switch (format)
            {
                case NamingFormat.CamelCase | NamingFormat.UI_Suffix:
                    return eventName == RouterEvent.Return ? @"Return(Btn|Button|Toggle)" : @$"^{GetViewEventName(ref eventName)}(\w{{1,}})(Btn|Button|Toggle)$";
                case NamingFormat.CamelCase | NamingFormat.UI_Prefix:
                    return eventName == RouterEvent.Return ? @"(Btn|Button|Toggle)Return" : @$"^(Btn|Button|Toggle){GetViewEventName(ref eventName)}(\w{{1,}})$";

                case NamingFormat.UnderlineLower | NamingFormat.UI_Suffix:
                    return eventName == RouterEvent.Return ? "return_(btn|button|toggle)" : @$"^{GetViewEventName(ref eventName, true)}_(\w{{1,}})_(btn|button|toggle)$";
                case NamingFormat.UnderlineLower | NamingFormat.UI_Prefix:
                    return eventName == RouterEvent.Return ? "(btn|button|toggle)_return" : @$"^(btn|button|toggle)_{GetViewEventName(ref eventName, true)}_(\w{{1,}})$";

                case NamingFormat.SpaceLower | NamingFormat.UI_Suffix:
                    return eventName == RouterEvent.Return ? "return (btn|button|toggle)" : @$"^{GetViewEventName(ref eventName, true)} (\w{{1,}}) (btn|button|toggle)$";
                case NamingFormat.SpaceLower | NamingFormat.UI_Prefix:
                    return eventName == RouterEvent.Return ? "(btn|button|toggle) return" : @$"^(btn|button|toggle) {GetViewEventName(ref eventName, true)} (\w{{1,}})$";

                case NamingFormat.SpaceUpper | NamingFormat.UI_Suffix:
                    return eventName == RouterEvent.Return ? "Return (Btn|Button|Toggle)" : @$"^{GetViewEventName(ref eventName)} (\w{{1,}}) (Btn|Button|Toggle)$";
                case NamingFormat.SpaceUpper | NamingFormat.UI_Prefix:
                    return eventName == RouterEvent.Return ? "(Btn|Button|Toggle) Return" : @$"^(Btn|Button|Toggle) {GetViewEventName(ref eventName)} (\w{{1,}})$";

                case NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix:
                    return eventName == RouterEvent.Return ? "Return_(Btn|Button|Toggle)" : @$"^{GetViewEventName(ref eventName)}_(\w{{1,}})_(Btn|Button|Toggle)$";
                case NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix:
                    return eventName == RouterEvent.Return ? "(Btn|Button|Toggle)_Return" : @$"^(Btn|Button|Toggle)_{GetViewEventName(ref eventName)}_(\w{{1,}})$";
            }
            return null;
        }

        private static string GetDataBindRegex(ref NamingFormat format, string modelName, string propertyName)
        {
            switch (format)
            {
                case NamingFormat.CamelCase | NamingFormat.UI_Suffix:
                    return @$"^({modelName}){{0,1}}{propertyName}(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)$";
                case NamingFormat.CamelCase | NamingFormat.UI_Prefix:
                    return @$"^(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)({modelName}){{0,1}}{propertyName}$";

                case NamingFormat.UnderlineLower | NamingFormat.UI_Suffix:
                    return @$"^({modelName}_){{0,1}}{propertyName}_(slider|txt|text|input|dropdown|toggle|btn|img|button|image)$";
                case NamingFormat.UnderlineLower | NamingFormat.UI_Prefix:
                    return @$"^(slider|txt|text|input|dropdown|toggle|btn|img|button|image)_({modelName}_){{0,1}}{propertyName}$";

                case  NamingFormat.SpaceLower | NamingFormat.UI_Suffix:
                    return @$"^({modelName} ){{0,1}}{propertyName} (slider|txt|text|input|dropdown|toggle|btn|img|button|image)$";
                case NamingFormat.SpaceLower | NamingFormat.UI_Prefix:
                    return @$"^(slider|txt|text|input|dropdown|toggle|btn|img|button|image) ({modelName} ){{0,1}}{propertyName}$";

                case NamingFormat.SpaceUpper | NamingFormat.UI_Suffix:
                    return @$"^({modelName} ){{0,1}}{propertyName} (Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)$";
                case NamingFormat.SpaceUpper | NamingFormat.UI_Prefix:
                    return @$"^(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image) ({modelName} ){{0,1}}{propertyName}$";

                case NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix:
                    return @$"^({modelName}_){{0,1}}{propertyName}_(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)$";
                case NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix:
                    return @$"^(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)_({modelName}_){{0,1}}{propertyName}$";
            }
            return null; //不能执行到这儿
        }

        //防止ToString()和ToLower()频繁产生新字符串对象
        private static string GetViewEventName(ref RouterEvent viewEvent, bool lower = false)
        {
            switch (viewEvent)
            {
                case RouterEvent.Open:
                    return lower ? "open" : "Open";
                case RouterEvent.Close:
                    return lower ? "close" : "Close";
                case RouterEvent.Skip:
                    return lower ? "skip" : "Skip";
                case RouterEvent.Return:
                    return lower ? "return" : "Return";
            }

            return null;
        }

    }
}
