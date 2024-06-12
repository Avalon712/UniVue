using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UniVue.View.Widgets
{
    [Serializable]
    public sealed class Carousel : Widget
    {
        private List<GameObject> _pages;        //所有的页
        private int _currPage;                  //当前页
        [SerializeField]
        [Range(1f,60f)] 
        private float _intervalTime;            //每隔多少秒切换一次
        [SerializeField]
        private bool _screenInput;              //能够通过屏幕交互进行页面切换

        /// <summary>
        /// 为Unity序列化提供接口
        /// </summary>
        public Carousel(){ }

        /// <summary>
        /// 手动创建组件
        /// </summary>
        /// <param name="intervalTime">每隔多少秒切换一次</param>
        /// <param name="screenInput">是否能够通过屏幕交互进行页面切换</param>
        /// <param name="pages">所有的页</param>
        public Carousel(float intervalTime, bool screenInput, params GameObject[] pages)
        {
            _intervalTime = intervalTime;
            _screenInput = screenInput;
            _pages = new(4);
            if (pages != null)
                for (int i = 0; i < pages.Length; i++)
                    _pages.Add(pages[i]);
        }

        /// <summary>
        /// 绑定轮播图导航栏
        /// </summary>
        /// <param name="navigators">导航器</param>
        /// <param name="interactive">是否允许通过导航栏设置当前显示的页</param>
        public void UseNavigators(Toggle[] navigators, bool interactive=true)
        {

        }

        /// <summary>
        /// 绑定轮播图导航栏
        /// </summary>
        /// <param name="nextPage">点击显示下一页按钮</param>
        /// <param name="lastPage">点击显示上一页按钮</param>
        /// <param name="interactive">是否允许通过导航栏设置当前显示的页</param>
        public void UseNavigators(Button nextPage, Button lastPage, bool interactive = true)
        {

        }

        /// <summary>
        /// 绑定用于显示当前是位于第几页的状态栏
        /// </summary>
        /// <param name="states">显示状态的图片</param>
        /// <param name="active">处于当前页是高亮的颜色</param>
        /// <param name="disactive">没有处于当前页时的颜色</param>
        public void UseNavigators(Image[] states, Color active, Color disactive)
        {

        }

        /// <summary>
        /// 滚动到指定页
        /// </summary>
        /// <param name="pageNumber">页数</param>
        public void ScrollTo(int pageNumber)
        {

        }

        /// <summary>
        /// 添加一个新的页
        /// </summary>
        /// <param name="page">页</param>
        public void AddPage(GameObject page)
        {
            InsertPage(_pages.Count, page);
        }

        /// <summary>
        /// 在指定位置插入一页
        /// </summary>
        /// <param name="insertPageNumber">插入的页面</param>
        /// <param name="page">插入的页</param>
        public void InsertPage(int insertPageNumber, GameObject page)
        {

        }

        /// <summary>
        /// 移除指定的页
        /// </summary>
        /// <param name="pageNumber">要移除的页的页码</param>
        public void RemovePage(int pageNumber)
        {

        }

        /// <summary>
        /// 移除指定页
        /// </summary>
        /// <param name="page">要移除的页</param>
        public void RemovePage(GameObject page)
        {

        }

        public override void Destroy()
        {

        }
    }
}
