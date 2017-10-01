using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HMIControl.HMIEx
{
    public class HMITextBox : TextBox, ITagReader, ITagWriter
    {
        public static readonly DependencyProperty TagReadTextProperty = DependencyProperty.Register("TagReadText", typeof(string), typeof(HMITextBox));
        public static readonly DependencyProperty TagWriteTextProperty = DependencyProperty.Register("TagWriteText", typeof(string), typeof(HMITextBox));

        /// <summary>
        ///Tag Property
        /// </summary>
        public string TagReadText
        {
            get
            {
                return (string)base.GetValue(TagReadTextProperty);
            }
            set
            {
                base.SetValue(TagReadTextProperty, value);
            }
        }

        protected SortedList<string, Action> _dict;
        public IDictionary<string, Action> OnTagChangedActions
        {
            get
            {
                return _dict;
            }
        }

        public string Node
        {
            get { return Name; }
        }

        public IList<ITagLink> Children
        {
            get { return null; }
        }

        public string TagWriteText
        {
            get
            {
                return (string)base.GetValue(TagWriteTextProperty);
            }
            set
            {
                base.SetValue(TagWriteTextProperty, value);
            }
        }

        protected Func<object, int> _funcWrite;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            this.Focus();
            if (e.Key == Key.Return)
            {
                if (_funcWrite != null)
                {
                    _funcWrite(this.Text.Trim());
                }
            }
        }


        public string[] GetActions()
        {
            return new string[] { TagActions.TEXT };
        }

        public Action SetTagReader(string key, Delegate tagChanged)
        {
            if (key == TagActions.TEXT)
            {
                var _funcText = tagChanged as Func<string>;
                if (_funcText != null)
                {
                    return delegate { this.Text = _funcText(); }; ;
                }
                else
                {
                    var _funcFloat = tagChanged as Func<float>;
                    if (_funcFloat != null)
                    {
                        return delegate { this.Text = _funcFloat().ToString(); };
                    }
                    else
                    {
                        var _funcbool = tagChanged as Func<bool>;
                        if (_funcbool != null)
                        {
                            return delegate { this.Text = _funcbool() ? "1" : "0"; };
                        }
                    }
                }
            }
            return null;
        }

        public bool IsPulse
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool SetTagWriter(Delegate tagWriter)
        {
            _funcWrite = tagWriter as Func<object, int>;
            return _funcWrite != null;
        }
    }
}
