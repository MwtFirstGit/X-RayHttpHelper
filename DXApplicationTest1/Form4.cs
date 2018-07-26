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
    public partial class Form4 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        String str = String.Empty;
        public Form4(String labeltext)
        {
            this.str = labeltext;
            InitializeComponent();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            str = null;
            this.Close();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
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
