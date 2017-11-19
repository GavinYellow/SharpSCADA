using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TagConfig
{
    public partial class AlarmParam : Form
    {
        List<Condition> _conditions;
        List<SubCondition> _subconds;
        TagData _tag;

        public static readonly string[] _severitys = new string[]
        {
             "正常","消息","低","中低","中","中高","高","错误"
        };

        public AlarmParam(List<Condition> conds, List<SubCondition> subconds, TagData tag)
        {
            InitializeComponent();
            _conditions = conds;
            _subconds = subconds;
            _tag = tag;
            this.Text = this.Text + ":" + tag.Name;
        }

        private void AlarmParam_Load(object sender, EventArgs e)
        {
            if (_tag != null)
            {
                switch (_tag.DataType)
                {
                    case 1:
                        grpDev.Enabled = false;
                        grpLimit.Enabled = false;
                        grpRate.Enabled = false;
                        cboQua.DataSource = _severitys.Clone(); cboDig.DataSource = _severitys;
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        grpRate.Enabled = false;
                        grpDig.Enabled = false;
                        cboQua.DataSource = _severitys.Clone(); cboLO.DataSource = _severitys.Clone(); cboLOLO.DataSource = _severitys;
                        cboHI.DataSource = _severitys.Clone(); cboHIHI.DataSource = _severitys.Clone();
                        cboMajDev.DataSource = _severitys.Clone(); cboMinDev.DataSource = _severitys.Clone();
                        break;
                    case 8:
                        grpDig.Enabled = false;
                        cboQua.DataSource = _severitys.Clone(); cboLO.DataSource = _severitys.Clone(); cboLOLO.DataSource = _severitys;
                        cboHI.DataSource = _severitys.Clone(); cboHIHI.DataSource = _severitys.Clone();
                        cboHIRt.DataSource = _severitys.Clone(); cboLORt.DataSource = _severitys.Clone();
                        cboMajDev.DataSource = _severitys.Clone(); cboMinDev.DataSource = _severitys.Clone();
                        ;
                        break;
                    case 11:
                        grpDev.Enabled = false;
                        grpLimit.Enabled = false;
                        grpRate.Enabled = false;
                        grpDig.Enabled = false;
                        cboQua.DataSource = _severitys;
                        break;
                }
            }
            var _conds = GetCondition(_tag);
            foreach (Condition cond in _conds)
            {
                switch (cond.AlarmType)
                {
                    case 1://LIMIT 
                        txtDebLim.Text = cond.DeadBand.ToString();
                        txtDelayLim.Text = cond.Delay.ToString();
                        foreach (var sub in cond.SubConditions)
                        {
                            switch (sub.SubAlarmType)
                            {
                                case 1:
                                    chkLOLO.Checked = true;
                                    txtThLOLO.Text = sub.Threshold.ToString();
                                    txtMsgLOLO.Text = sub.Message;
                                    cboLOLO.SelectedIndex = sub.Severity;
                                    break;
                                case 2:
                                    chkLO.Checked = true;
                                    txtThLO.Text = sub.Threshold.ToString();
                                    txtMsgLO.Text = sub.Message;
                                    cboLO.SelectedIndex = sub.Severity;
                                    break;
                                case 4:
                                    chkHI.Checked = true;
                                    txtThHI.Text = sub.Threshold.ToString();
                                    txtMsgHI.Text = sub.Message;
                                    cboHI.SelectedIndex = sub.Severity;
                                    break;
                                case 8:
                                    chkHIHI.Checked = true;
                                    txtThHIHI.Text = sub.Threshold.ToString();
                                    txtMsgHIHI.Text = sub.Message;
                                    cboHIHI.SelectedIndex = sub.Severity;
                                    break;
                            }
                        }
                        break;
                    case 2://DEV
                        txtDebDev.Text = cond.DeadBand.ToString();
                        txtDelayDev.Text = cond.Delay.ToString();
                        txtParaDev.Text = cond.Para.ToString();
                        rdValue.Checked = cond.ConditionType == 0;
                        rdPercent.Checked = cond.ConditionType == 1;
                        foreach (var sub in cond.SubConditions)
                        {
                            switch (sub.SubAlarmType)
                            {
                                case 16:
                                    chkMajDev.Checked = true;
                                    txtThMajDev.Text = sub.Threshold.ToString();
                                    txtMsgMaj.Text = sub.Message;
                                    cboMajDev.SelectedIndex = sub.Severity;
                                    break;
                                case 32:
                                    chkMinDev.Checked = true;
                                    txtThMinDev.Text = sub.Threshold.ToString();
                                    txtMsgMin.Text = sub.Message;
                                    cboMinDev.SelectedIndex = sub.Severity;
                                    break;
                            }
                        }
                        break;
                    case 3://RATE
                        txtDebRt.Text = cond.DeadBand.ToString();
                        txtDelayRt.Text = cond.Delay.ToString();
                        foreach (var sub in cond.SubConditions)
                        {
                            switch (sub.SubAlarmType)
                            {
                                case 256:
                                    chkHIRt.Checked = true;
                                    txtThHIRt.Text = sub.Threshold.ToString();
                                    txtMsgHIRt.Text = sub.Message;
                                    cboHIRt.SelectedIndex = sub.Severity;
                                    break;
                                case 512:
                                    chkLORt.Checked = true;
                                    txtThLORt.Text = sub.Threshold.ToString();
                                    txtMsgLOLO.Text = sub.Message;
                                    cboLORt.SelectedIndex = sub.Severity;
                                    break;
                            }
                        }
                        break;
                    case 4://DIGIT;
                        txtDelayDig.Text = cond.Delay.ToString();
                        if (cond.SubConditions.Count > 0)
                        {
                            var sub = cond.SubConditions[0];
                            chkDig.Checked = true;
                            rdDig.Checked = sub.Threshold > 0;
                            radioButton2.Checked = sub.Threshold == 0;
                            txtMsgDig.Text = sub.Message;
                            cboDig.SelectedIndex = sub.Severity;
                        }
                        break;
                    case 5://QUA
                        txtDelayQua.Text = cond.Delay.ToString();
                        if (cond.SubConditions.Count > 0)
                        {
                            var sub = cond.SubConditions[0];
                            chkQua.Checked = true;
                            txtMsgQua.Text = sub.Message;
                            cboQua.SelectedIndex = sub.Severity;
                        }
                        break;
                }
            }
        }

        private void AlarmParam_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("退出之前是否需要保存？", "警告", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Save();
            }
        }

        void Save()
        {
            RemoveCondition(_tag);
            _tag.HasAlarm = false;
            if (chkLOLO.Checked || chkLO.Checked || chkHI.Checked || chkHIHI.Checked)
            {
                float para;
                float.TryParse(txtDebLim.Text, out para);
                int delay;
                int.TryParse(txtDelayLim.Text, out delay);
                var condition = new Condition(++Program.MAXCONDITIONID, _tag.Name, 1, 4, 0, 0, true, para, delay);
                if (chkLOLO.Checked)
                {
                    float thr;
                    float.TryParse(txtThLOLO.Text, out thr);
                    var sub = new SubCondition(true, cboLOLO.SelectedIndex, condition.TypeId, 1, thr, txtMsgLOLO.Text);
                    condition.SubConditions.Add(sub);
                    _subconds.Add(sub);
                }
                if (chkLO.Checked)
                {
                    float thr;
                    float.TryParse(txtThLO.Text, out thr);
                    var sub = new SubCondition(true, cboLO.SelectedIndex, condition.TypeId, 2, thr, txtMsgLO.Text);
                    condition.SubConditions.Add(sub);
                    _subconds.Add(sub);
                }
                if (chkHI.Checked)
                {
                    float thr;
                    float.TryParse(txtThHI.Text, out thr);
                    var sub = new SubCondition(true, cboHI.SelectedIndex, condition.TypeId, 4, thr, txtMsgHI.Text);
                    condition.SubConditions.Add(sub);
                    _subconds.Add(sub);
                }
                if (chkHIHI.Checked)
                {
                    float thr;
                    float.TryParse(txtThHIHI.Text, out thr);
                    var sub = new SubCondition(true, cboHIHI.SelectedIndex, condition.TypeId, 8, thr, txtMsgHIHI.Text);
                    condition.SubConditions.Add(sub);
                    _subconds.Add(sub);
                }
                _conditions.Add(condition);
                _tag.HasAlarm = true;
            }
            if (chkMajDev.Checked || chkMinDev.Checked)
            {
                float para;
                float.TryParse(txtParaDev.Text, out para);
                float deb;
                float.TryParse(txtDebDev.Text, out deb);
                int delay;
                int.TryParse(txtDelayDev.Text, out delay);
                var condition = new Condition(++Program.MAXCONDITIONID, _tag.Name, 2, 4, rdValue.Checked ? (byte)0 : (byte)1, para, true, deb, delay);
                if (chkMajDev.Checked)
                {
                    float thr;
                    float.TryParse(txtThMajDev.Text, out thr);
                    var sub = new SubCondition(true, cboMajDev.SelectedIndex, condition.TypeId, 16, thr, txtMsgMaj.Text);
                    condition.SubConditions.Add(sub);
                    _subconds.Add(sub);
                }
                if (chkMinDev.Checked)
                {
                    float thr;
                    float.TryParse(txtThMinDev.Text, out thr);
                    var sub = new SubCondition(true, cboMinDev.SelectedIndex, condition.TypeId, 32, thr, txtMsgMin.Text);
                    condition.SubConditions.Add(sub);
                    _subconds.Add(sub);
                }
                _conditions.Add(condition);
                _tag.HasAlarm = true;
            }
            if (chkHIRt.Checked || chkLORt.Checked)
            {
                float deb;
                float.TryParse(txtDebRt.Text, out deb);
                int delay;
                int.TryParse(txtDelayRt.Text, out delay);
                var condition = new Condition(++Program.MAXCONDITIONID, _tag.Name, 3, 4, 0, 0, true, deb, delay);
                if (chkHIRt.Checked)
                {
                    float thr;
                    float.TryParse(txtThHIRt.Text, out thr);
                    var sub = new SubCondition(true, cboHIRt.SelectedIndex, condition.TypeId, 256, thr, txtMsgHIRt.Text);
                    condition.SubConditions.Add(sub);
                    _subconds.Add(sub);
                }
                if (chkLORt.Checked)
                {
                    float thr;
                    float.TryParse(txtThLORt.Text, out thr);
                    var sub = new SubCondition(true, cboLORt.SelectedIndex, condition.TypeId, 512, thr, txtMsgLORt.Text);
                    condition.SubConditions.Add(sub);
                    _subconds.Add(sub);
                }
                _conditions.Add(condition);
                _tag.HasAlarm = true;
            }
            if (chkDig.Checked)
            {
                int delay;
                int.TryParse(txtDelayDig.Text, out delay);
                var condition = new Condition(++Program.MAXCONDITIONID, _tag.Name, 4, 4, 0, 0, true, 0, delay);
                var sub = new SubCondition(true, cboDig.SelectedIndex, condition.TypeId, 64, rdDig.Checked ? 1 : 0, txtMsgDig.Text);
                condition.SubConditions.Add(sub);
                _conditions.Add(condition);
                _subconds.Add(sub);
                _tag.HasAlarm = true;
            }
            if (chkQua.Checked)
            {
                int delay;
                int.TryParse(txtDelayQua.Text, out delay);
                var condition = new Condition(++Program.MAXCONDITIONID, _tag.Name, 5, 4, 0, 0, true, 0, delay);
                var sub = new SubCondition(true, cboQua.SelectedIndex, condition.TypeId, 128, 0, txtMsgQua.Text);
                condition.SubConditions.Add(sub);
                _conditions.Add(condition);
                _subconds.Add(sub);
                _tag.HasAlarm = true;
            }
        }


        List<Condition> GetCondition(TagData tag)
        {
            List<Condition> condList = new List<Condition>();
            if (_conditions == null || tag == null) return condList;
            string id = tag.Name;
            foreach (var cond in _conditions)
            {
                if (cond.Source == id)
                    condList.Add(cond);
            }
            return condList;
        }

        int RemoveCondition(TagData tag)
        {
            if (_conditions == null || tag == null) return -1;
            string id = tag.Name;
            for (int i = _conditions.Count - 1; i >= 0; i--)
            {
                if (_conditions[i].Source == id)
                {
                    for (int j = _subconds.Count - 1; j >= 0; j--)
                    {
                        if (_subconds[j].ConditionId == _conditions[i].TypeId)
                        {
                            _subconds.RemoveAt(j);
                        }
                    }
                    _conditions.RemoveAt(i);
                }
            }
            return 1;
        }
    }
}
        //NONE = 0,
        //BOOL = 1,
        //BYTE = 3,
        //SHORT = 4,
        //WORD = 5,
        //TIME = 6,
        //INT = 7,
        //FLOAT = 8,
        //SYS = 9,
        //STR = 11


        //None = 0,
        //LoLo = 1,
        //Low = 2,
        //High = 4,
        //HiHi = 8,
        //MajDev = 16,
        //MinDev = 32,
        //Dsc = 64,
        //BadPV = 128,
        //MajROC = 256,
        //MinROC = 512

        //Error = 7,
        //High = 6,
        //MediumHigh = 5,
        //Medium = 4,
        //MediumLow = 3,
        //Low = 2,
        //Information = 1,
        //Normal = 0