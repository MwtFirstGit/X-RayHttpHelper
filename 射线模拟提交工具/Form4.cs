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
    public partial class Form4 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        String str = String.Empty;
        public string filter = string.Empty;
        public Form4(String labeltext)
        {
            this.str = labeltext;
            this.ControlBox = false;
            InitializeComponent();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            str = null;
            if (textEdit1.Visible)
            {
                if (!String.IsNullOrEmpty(textEdit1.Text))
                {
                    filter = textEdit1.Text;
                }
                else
                {
                    this.Text = "错误";
                    this.str = "错误：请输入要保存的文件后缀名";
                    this.ShowDialog();
                }
            }
            this.Close();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            if(this.Text == "自定义文件后缀")
            {
                labelControl2.Visible = true;
                labelControl1.Visible = false;
                textEdit1.Visible = true;
            }
            if(!String.IsNullOrEmpty(str))
            {
                labelControl1.Text = str;
            }
            if (this.Text == "错误")
            {
                simpleButton2.ImageUri = "Cancel";
            }
            if(this.Text == "成功")
            {
                simpleButton2.ImageUri = "Apply";
            }
            if (this.Text == "失败")
            {
                simpleButton2.Image = Properties.Resources.警告;
            }
            if (this.Text == "警告")
            {
                simpleButton2.Image = Properties.Resources.Warning;
            }
            if (this.Text == "提示")
            {
                simpleButton2.Image = Properties.Resources.Tips;
            }
        }
    }
}
