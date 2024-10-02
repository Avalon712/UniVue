using UnityEngine;
using UniVue.i18n;


namespace UniVue
{
    public sealed class VueConfig : ScriptableObject
    {
        [SerializeField, Header("两个规则之间的分隔符号(前后留有空格)")]
        private string _ruleSeparator = " & ";

        [SerializeField, Header("视图对象匹配模式")]
        private MatchViewMode _mode = MatchViewMode.NameEndWith_View;

        [SerializeField, Header("默认语言")]
        private Language _defaultLanguage = Language.None;

        [SerializeField, Header("使用缓存")]
        private bool _useCache = true;

        [SerializeField, Header("视图打开/关闭的最大历史记录"), Tooltip("如果发现视图的打开逻辑不正确，请将此参数调大一些"), Min(5)]
        private int _maxHistoryRecord = 10;

        [SerializeField, Header("显示[Flags]枚举时指定两个枚举值之间的分隔符")]
        private string _flagsEnumSeparator = " | ";

        [SerializeField, Header("组件查找优化符号")]
        [Tooltip("以此符号命名开头的GameObject,其所有后代(包括自身)GameObject不会被执行组件查找")]
        private char _ignoreSymbol = '~';

        [SerializeField, Tooltip("以此符号命名开头的GameObject,其自身不会被执行组件查找")]
        private char _skipSymbol = '@';

        [SerializeField, Header("显示Float数据时指定显示的位数")]
        private int _floatKeepBit = 2;

        [SerializeField, Header("初始时VM表的大小")]
        private int _tableSize = 20;

        [SerializeField, Header("更新到UI上的值为null或Empty时隐藏UI的显示")]
        private bool _whenValueIsNullThenHide = true;

        [SerializeField, Header("更新List到UI上时,对多余的UI进行隐藏")]
        private bool _whenListLessUICountThenHideSurplus = true;

        [SerializeField, Header("Image(Filled)的fillAmount=0时隐藏显示")]
        private bool _whenFillAmountEqualZeroThenHide = true;

        public static VueConfig New => CreateInstance<VueConfig>();

        #region 属性

        /// <summary>
        /// 两个规则之间的分隔符号
        /// </summary>
        public string RuleSeparator
        {
            get
            {
                if (string.IsNullOrEmpty(_ruleSeparator)) _ruleSeparator = " & ";
                return _ruleSeparator;
            }
        }

        /// <summary>
        /// 是否使用缓存
        /// </summary>
        public bool UseCache => _useCache;

        /// <summary>
        /// 检查一个GameObject是否是一个ViewObject的匹配模式
        /// </summary>
        public MatchViewMode Mode => _mode;

        /// <summary>
        /// 游戏默认使用的语言标识
        /// </summary>
        public Language DefaultLanguage { get { return _defaultLanguage; } }

        /// <summary>
        /// 当更新Sprite、string类型时，如果Sprite的值为null类型则隐藏图片的显示，即: gameObject.SetActive(false);
        /// </summary>
        /// <remarks>默认为true</remarks>
        public bool WhenValueIsNullThenHide { get => _whenValueIsNullThenHide; set => _whenValueIsNullThenHide = value; }

        /// <summary>
        /// 当更新List类型的数据时，如果当前数据量小于UI数量，则将多余的UI进行隐藏不显示
        /// </summary>
        /// <remarks>默认为true</remarks>
        public bool WhenListLessUICountThenHideSurplus { get => _whenListLessUICountThenHideSurplus; set => _whenListLessUICountThenHideSurplus = value; }

        /// <summary>
        /// 设置视图允许的最大历史记录，默认为10
        /// </summary>
        /// <remarks>如果你发现你的视图打开逻辑不正确,建议将此值设置大一点</remarks>
        public int MaxHistoryRecord { get => _maxHistoryRecord; set => _maxHistoryRecord = value; }

        /// <summary>
        /// 当[Flags]标记的枚举绑定到TMP_Text上时,指定两两之间使用的分隔符号
        /// </summary>
        public string FlagsEnumSeparator { get => _flagsEnumSeparator; set => _flagsEnumSeparator = value; }

        /// <summary>
        /// 在优化组件查找中，以此字符开头的GameObject以及它所有的后代都不会被进行组件查找
        /// </summary>
        public char IgnoreSymbol { get => _ignoreSymbol; set => _ignoreSymbol = value; }

        /// <summary>
        /// 在优化组件查找中，以此字符开头的GameObject不会被进行组件查找，但其后代节点会进行组件查找
        /// </summary>
        public char SkipSymbol { get => _skipSymbol; set => _skipSymbol = value; }

        /// <summary>
        /// 显示浮点数时指定浮点数保留的位数
        /// </summary>
        public int FloatKeepBit { get => _floatKeepBit; set => _floatKeepBit = value; }

        /// <summary>
        /// 渲染表的大小，这个值根据项目的大小来填写，这个值主要是放在集合的多次扩容
        /// </summary>
        public int TabelSize { get => _tableSize; set => _tableSize = value; }

        /// <summary>
        /// 对应类型为Filled的Image的UI，当fillAmount=0时隐藏UI的显示
        /// </summary>
        public bool WhenFillAmountEqualZeroThenHide { get => _whenFillAmountEqualZeroThenHide; set => _whenFillAmountEqualZeroThenHide = value; }
        #endregion
    }

    /// <summary>
    /// 匹配视图的模式
    /// </summary>
    public enum MatchViewMode
    {
        /// <summary>
        /// 名称以"View"结尾的GameObject是一个ViewObject
        /// </summary>
        NameEndWith_View,

        /// <summary>
        /// 如果一个GameObject的tag标签为"ViewObject"那么它是一个ViewObject
        /// </summary>
        Tag_ViewObject,
    }
}
