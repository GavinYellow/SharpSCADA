using System;
using System.Windows.Forms;

namespace TagConfig
{
    public partial class GroupParam : Form
    {
        Group _grp;

        public GroupParam(Group grp)
        {
            _grp = grp;
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            if (_grp != null)
            {
                txtName.Text = _grp.Name;
                txtUpdate.Value = _grp.UpdateRate;
                txtDeadband.Value = (decimal)_grp.DeadBand;
                chkActive.Checked = _grp.Active;
            }
        }

        private void Form4_FormClosed(object sender, FormClosedEventArgs e)
        {
            txtName.Text = _grp.Name = txtName.Text;
            _grp.UpdateRate = (int)txtUpdate.Value;
            _grp.DeadBand = (float)txtDeadband.Value;
            _grp.Active = chkActive.Checked;
        }
    }
}
