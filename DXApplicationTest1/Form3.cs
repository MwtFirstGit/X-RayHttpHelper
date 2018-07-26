using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DXApplicationTest1
{
    public partial class Form3 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public Form3()
        {
            InitializeComponent();
        }
        public CreateHelper createHelper = new CreateHelper();
        private void simpleButton5_Click(object sender, EventArgs e)
        {
            String proxy_api = textEdit3.Text;
            String regstr = textEdit1.Text;
            String threadnum = textEdit4.Text;
            String xpathstr = textEdit2.Text;
            createHelper.Check = checkEdit1.Checked;
            createHelper.CheckXpath = checkEdit2.Checked;
            if (checkEdit1.Checked)
            {
                createHelper.RegStr = regstr;
            }
            if (checkEdit2.Checked)
            {
                createHelper.XpathStr = xpathstr;
            }
            createHelper.TimerEnable = checkEdit3.Checked;
            if (!String.IsNullOrEmpty(threadnum))
            {
                createHelper.ThreadNum = Convert.ToInt32(threadnum);
            }
            createHelper.ProxyEnable = checkEdit4.Checked;
            createHelper.LoopGet = checkEdit5.Checked;
            if (checkEdit4.Checked)
            {
                if (!String.IsNullOrEmpty(proxy_api))
                {
                    createHelper.ProxyAPI = proxy_api;
                }
                else
                {
                    Form4 form4 = new Form4("错误：请输入代理IP");
                    form4.Text = "错误";
                    form4.ShowDialog();
                    return;
                }
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        #region  辅助配置

        private void checkEdit1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkEdit1.Checked)
            {
                checkEdit2.Checked = false;
            }
            textEdit1.Enabled = checkEdit1.Checked;
        }

        private void checkEdit2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkEdit2.Checked)
            {
                checkEdit1.Checked = false;
            }
            textEdit2.Enabled = checkEdit2.Checked;
        }

        private void simpleButton6_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textEdit1.Text))
            {
                textEdit1.Text = "";
            }
            if (!String.IsNullOrEmpty(textEdit2.Text))
            {
                textEdit2.Text = "";
            }
            if (!String.IsNullOrEmpty(textEdit3.Text))
            {
                textEdit3.Text = "";
            }
            if (!String.IsNullOrEmpty(textEdit4.Text))
            {
                textEdit4.Text = "";
            }
            textEdit1.Enabled = false;
            if (checkEdit1.Checked)
            {
                checkEdit1.Checked = false;
            }
            if (checkEdit2.Checked)
            {
                checkEdit2.Checked = false;
            }
            if (checkEdit3.Checked)
            {
                checkEdit3.Checked = false;
            }
            if (checkEdit4.Checked)
            {
                checkEdit4.Checked = false;
            }
            if (checkEdit5.Checked)
            {
                checkEdit5.Checked = false;
            }
        }

        private void checkEdit4_CheckedChanged(object sender, EventArgs e)
        {
            textEdit3.Enabled = checkEdit4.Checked;
        }
        #endregion

        private void simpleButton7_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            textEdit1.Enabled = false;
            textEdit2.Enabled = false;
            textEdit3.Enabled = false;
        }
    }
}
