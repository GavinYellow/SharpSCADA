using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TagConfig
{
    public partial class ScaleParam : Form
    {
        List<Scaling> _list;
        TagData _tag;
        Scaling _scale = null;

        public ScaleParam(List<Scaling> list, TagData tag)
        {
            InitializeComponent();
            _list = list;
            _tag = tag;
        }

        private void ScaleParam_Load(object sender, EventArgs e)
        {
            int index = _list.BinarySearch(new Scaling(_tag.ID, 0, 0, 0, 0, 0));
            if (index >= 0)
            {
                _scale = _list[index];
                rdNone.Checked = _scale.ScaleType == 0;
                rdLine.Checked = _scale.ScaleType == 1;
                rdSqure.Checked = _scale.ScaleType == 2;
                nmEUHI.Value = (decimal)_scale.EUHi;
                nmEULO.Value = (decimal)_scale.EULo;
                nmRWHI.Value = (decimal)_scale.RawHi;
                nmRWLO.Value = (decimal)_scale.RawLo;
            }
            
        }

        void Save()
        {
            _tag.HasScale = (!rdNone.Checked) && (nmEUHI.Value > nmEULO.Value) && (nmRWHI.Value > nmRWLO.Value); 
            if (!_tag.HasScale)
            {
                if (_scale != null)
                    _list.Remove(_scale);
                return;
            }
            if (_scale == null)
            {
                _list.Add(new Scaling(_tag.ID, (byte)(rdLine.Checked ? 1 : 2), (float)nmEUHI.Value, (float)nmEULO.Value, (float)nmRWHI.Value, (float)nmRWLO.Value));
            }
            else
            {
                _scale.ScaleType = (byte)(rdLine.Checked ? 1 : 2);
                _scale.EUHi = (float)nmEUHI.Value;
                _scale.EULo = (float)nmEULO.Value;
                _scale.RawHi = (float)nmRWHI.Value;
                _scale.RawLo=(float)nmRWLO.Value;
            }
            _tag.HasScale = true;
        }

        private void ScaleParam_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (MessageBox.Show("退出之前是否需要保存？", "警告", MessageBoxButtons.OKCancel) == DialogResult.OK)
            //{
            //    Save();
            //}
            Save();
        }
    }
}
