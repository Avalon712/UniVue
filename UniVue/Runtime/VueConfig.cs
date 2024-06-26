using System;
using UnityEngine;
using UniVue.Rule;


namespace UniVue
{
    public sealed class VueConfig : ScriptableObject
    {
        [SerializeField] private NamingFormat _format = NamingFormat.UI_Suffix | NamingFormat.UnderlineUpper;
        [SerializeField] private bool _whenValueIsNullThenHide = true;
        [SerializeField] private bool _whenListLessUICountThenHideSurplus = true;
        [SerializeField] private int _maxHistoryRecord = 10;
        [SerializeField] private string _flagsEnumSeparator = " | ";
        [SerializeField] private char _skipDescendantNodeSeparator = '~';
        [SerializeField] private char _skipCurrentNodeSeparator = '@';
        [SerializeField] private int _floatKeepBit = 2;
        [SerializeField] private float _perItemScrollTime = 0.1f;
        [SerializeField] private bool _renderModelOnScroll;
        [SerializeField] private int _tableSize = 20;

        public static VueConfig Create()
        {
            return ScriptableObject.CreateInstance<VueConfig>();
        }

        /// <summary>
        /// 命名风格
        /// </summary>
        /// <remarks>默认风格为 NamingFormat.UI_Suffix | NamingFormat.UnderlineUpper</remarks>
        public NamingFormat Format
        {
            get => _format;
            set
            {
                if ((value & NamingFormat.UI_Prefix) != NamingFormat.UI_Prefix && (value & NamingFormat.UI_Suffix) != NamingFormat.UI_Suffix)
                {
                    throw new ArgumentException("命名格式必须指定UI名称的格式，指定NamedFormat.UI_Prefix或NamedFormat.UI_Suffix");
                }
                _format = value;
            }
        }

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
        public char SkipDescendantNodeSeparator { get => _skipDescendantNodeSeparator; set => _skipDescendantNodeSeparator = value; }

        /// <summary>
        /// 在优化组件查找中，以此字符开头的GameObject不会被进行组件查找，但其后代节点会进行组件查找
        /// </summary>
        public char SkipCurrentNodeSeparator { get => _skipCurrentNodeSeparator; set => _skipCurrentNodeSeparator = value; }

        /// <summary>
        /// 显示浮点数时指定浮点数保留的位数
        /// </summary>
        public int FloatKeepBit { get => _floatKeepBit; set => _floatKeepBit = value; }

        /// <summary>
        /// 当ListWidget和GridWidget使用刷新时滚动属性时指定滚动一个Item的距离使用的时间
        /// </summary>
        public float PerItemScrollTime { get => _perItemScrollTime; set => _perItemScrollTime = value; }

        /// <summary>
        /// 当ListWidget和GridWidget在使用滚动动画时是否刷新数据
        /// </summary>
        /// <remarks>这儿是全局配置，也可以单独为每个ListWidget和GridWidget组件进行设置，局部设置优先级高于全局设置</remarks>
        public bool RenderModelOnScroll { get => _renderModelOnScroll; set => _renderModelOnScroll = value; }

        /// <summary>
        /// 渲染表的大小，这个值根据项目的大小来填写，这个值主要是放在集合的多次扩容
        /// </summary>
        public int TabelSize { get => _tableSize; set => _tableSize = value; }
    }
}
