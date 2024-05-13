using System;

namespace UniVue.Rule
{
    /// <summary>
    /// 常见的命名格式
    /// </summary>
    [Flags]
    public enum NamingFormat
    {
        /// <summary>
        /// 驼峰式: 单词间没有间隔，单词首字母大写。 如: PlayerNameTxt
        /// </summary>
        CamelCase = 1,

        /// <summary>
        /// 下划线式:单词间以下划线分开，单词首字母小写。如: player_name_txt
        /// </summary>
        UnderlineLower = 2,

        /// <summary>
        /// 空格式: 单词间使用空格分开，单词首字母小写。如: player name txt
        /// </summary>
        SpaceLower = 4,

        /// <summary>
        /// 空格式: 单词间使用空格分开，单词首字母大写。如: Player Name Txt
        /// </summary>
        SpaceUpper = 8,

        /// <summary>
        /// 大写下划线式: 单词间使用下划线隔开，单词首字母大写 如: Player_Info_View
        /// </summary>
        UnderlineUpper = 16,

        /// <summary>
        /// 命名以UI组件类型开头 如: Btn_Close
        /// </summary>
        UI_Prefix = 32,

        /// <summary>
        /// 命名以UI后缀类型结尾，如: Close_Btn
        /// </summary>
        UI_Suffix = 64
    }
}
