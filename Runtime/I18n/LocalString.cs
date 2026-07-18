#region

using System;

#endregion

namespace UniVue.I18n
{
    public readonly struct LocalString : IEquatable<LocalString>
    {
        public readonly long textId;

        public string Text => I18N.LocalText(textId);

        /// <summary>
        /// </summary>
        /// <param name="text">默认语言下的文本</param>
        public LocalString(string text)
        {
            textId = I18N.LocalTextId(text);
        }

        public LocalString(long textId)
        {
            this.textId = textId;
        }

        public bool Equals(LocalString other)
        {
            return textId == other.textId;
        }

        public override bool Equals(object obj)
        {
            return obj is LocalString other && Equals(other);
        }

        public override int GetHashCode()
        {
            return textId.GetHashCode();
        }

        public override string ToString()
        {
            return Text;
        }
    }
}