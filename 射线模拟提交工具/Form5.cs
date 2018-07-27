using EnvDTE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 射线模拟提交工具
{
    public partial class Form5 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        String str = String.Empty;
        public Form5(string labeltxt)
        {
            this.str = labeltxt;
            InitializeComponent();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(str))
            {
                labelControl1.Text = str;
            }
            if(this.Text == "提示")
            {
                
                simpleButton3.Image = Properties.Resources.Tips;
                //simpleButton3.ImageUri = "../Resources/提示.png";
            }
            if(this.Text == "错误")
            {
                simpleButton3.ImageUri = "Cancel";
            }
            if(this.Text == "成功")
            {
                simpleButton3.ImageUri = "Apply";
            }
            if (this.Text == "失败")
            {
                simpleButton3.Image = Properties.Resources.警告;

            }
            if (this.Text == "警告")
            {
                simpleButton3.Image = Properties.Resources.Warning;
            }
        }
    }
}
