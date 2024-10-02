using System.Collections;
using System.Text;
using UniVue.Extensions;

namespace UniVue.Common
{
    internal static class CommonUtil
    {
        /// <summary>
        /// 将删除元素至于尾端再进行删除
        /// </summary>
        public static void TrailDelete(IList list, int deleteIndex)
        {
            int trialIdx = list.Count - 1;
            if (deleteIndex == trialIdx)
            {
                list.RemoveAt(deleteIndex);
            }
            else
            {
                object del = list[deleteIndex];
                object trail = list[trialIdx];
                list[trialIdx] = del;
                list[deleteIndex] = trail;
                list.RemoveAt(trialIdx);
            }
        }

        /// <summary>
        /// 打印一个操作权限的字符串
        /// </summary>
        public static string ToString(OperablePermission permission)
        {
            if ((permission & OperablePermission.All) == OperablePermission.All)
            {
                return "[All]";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            bool flag = false;
            if ((permission & OperablePermission.Read) == OperablePermission.Read)
            {
                builder.Append("Read");
                flag = true;
            }
            if ((permission & OperablePermission.Add) == OperablePermission.Add)
            {
                if (flag) builder.Append(" | ");
                builder.Append("Add");
                flag = true;
            }
            if ((permission & OperablePermission.Insert) == OperablePermission.Insert)
            {
                if (flag) builder.Append(" | ");
                builder.Append("Insert");
                flag = true;
            }
            if ((permission & OperablePermission.Remove) == OperablePermission.Remove)
            {
                if (flag) builder.Append(" | ");
                builder.Append("Remove");
                flag = true;
            }
            if ((permission & OperablePermission.Clear) == OperablePermission.Clear)
            {
                if (flag) builder.Append(" | ");
                builder.Append("Clear");
                flag = true;
            }
            if ((permission & OperablePermission.Replace) == OperablePermission.Replace)
            {
                if (flag) builder.Append(" | ");
                builder.Append("Replace");
                flag = true;
            }
            if ((permission & OperablePermission.Sort) == OperablePermission.Sort)
            {
                if (flag) builder.Append(" | ");
                builder.Append("Sort");
                flag = true;
            }
            if ((permission & OperablePermission.Reverse) == OperablePermission.Reverse)
            {
                if (flag) builder.Append(" | ");
                builder.Append("Reverse");
            }
            builder.Append(']');
            return builder.ToString();
        }
    }
}
