using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DXApplicationTest1
{
    public partial class Form6 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public Form6()
        {
            InitializeComponent();
        }
        public String pathname = String.Empty;
        private void Form6_Load(object sender, EventArgs e)
        {
            if (Directory.Exists("配置文件"))
            {
                var files = Directory.GetFiles("配置文件", "*.txt");
                foreach(var file in files)
                {
                    comboBoxEdit1.Properties.Items.Add(file);
                    comboBoxEdit1.SelectedIndex = 0;
                    comboBoxEdit1.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                }
            }
            if(this.Text == "保存配置")
            {
                labelControl2.Text = "可覆盖文件名";
            }
            if(this.Text == "读取配置")
            {
                labelControl1.Visible = false;
                textEdit1.Visible = false;
                labelControl2.Text = "可选择文件名";
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(textEdit1.Text))
                {
                    pathname = textEdit1.Text;
                }
                else if(comboBoxEdit1.Properties.Items.Count>0)
                {
                    pathname = comboBoxEdit1.Text;
                    if(this.Text == "保存配置")
                    {
                        Form5 form5 = new Form5("提示：本次保存将会覆盖上一次，确认保存？");
                        form5.Text = "提示";
                        form5.ShowDialog();
                        if (form5.DialogResult == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    Form4 form4 = new Form4("错误：请输入要保存的文件名");
                    form4.Text = "错误";
                    form4.ShowDialog();
                    return;
                }
                this.DialogResult = DialogResult.OK;
            }
            catch
            {
                this.DialogResult = DialogResult.No;
            }
            this.Close();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
