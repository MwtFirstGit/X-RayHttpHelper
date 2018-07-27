using ExtractLib;
using System;
using System.Collections.Concurrent;
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
    public partial class Form2 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            labelControl2.Enabled = false;
            textEdit2.Enabled = false;
            ribbonControl1.Minimized = true;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            //Form1 form1 = new Form1();
            string key = string.Empty;
            if (!checkEdit1.Checked)
            {
                switch (comboBoxEdit1.SelectedIndex)
                {
                    case 0:
                        key = "Host";
                        break;
                    case 1:
                        key = "ProtocolVersion";
                        break;
                    case 2:
                        key = "proxy";
                        break;
                }
                if(comboBoxEdit1.SelectedIndex == 0)
                {
                    if (String.IsNullOrEmpty(textEdit1.Text))
                    {
                        Form4 form4 = new Form4("警告：请填写Host的Value值");
                        form4.Text = "警告";
                        form4.ShowDialog();
                        return;
                    }
                    if(Form1.HeadDic.TryAdd(key, textEdit1.Text))
                    {

                    }
                    else
                    {
                        Form4 form4 = new Form4("错误：Key已存在");
                        form4.Text = "错误";
                        form4.ShowDialog();
                    }
                }
                if(comboBoxEdit1.SelectedIndex == 1)
                {
                    if(textEdit1.Text == "1.1")
                    {
                        if(Form1.HeadDic.TryAdd(key, "1.1"))
                        {

                        }
                        else
                        {
                            Form4 form4 = new Form4("错误：Key已存在");
                            form4.Text = "错误";
                            form4.ShowDialog();
                        }
                    }
                    else
                    {
                        if(Form1.HeadDic.TryAdd(key, "1.0"))
                        {

                        }
                        else
                        {
                            Form4 form4 = new Form4("错误：Key已存在");
                            form4.Text = "错误";
                            form4.ShowDialog();
                        }
                    }
                }
                if(comboBoxEdit1.SelectedIndex == 2)
                {
                    string str = RegexMethod.GetSingleResult("[0-9]+?.[0-9]+?.[0-9]+?.[0-9]+?:[0-9]+", textEdit1.Text);
                    if (String.IsNullOrEmpty(textEdit1.Text)||String.IsNullOrEmpty(str))
                    {
                        Form4 form4 = new Form4("警告：请按指定格式填写Proxy的Value值");
                        form4.Text = "警告";
                        form4.ShowDialog();
                        return;
                    }
                    if(Form1.HeadDic.TryAdd(key, textEdit1.Text))
                    {

                    }
                    else
                    {
                        Form4 form4 = new Form4("错误：Key已存在");
                        form4.Text = "错误";
                        form4.ShowDialog();
                    }
                }
            }
            else
            {
                if (String.IsNullOrEmpty(textEdit1.Text) || String.IsNullOrEmpty(textEdit2.Text))
                {
                    Form4 form4 = new Form4("警告：请填写自定义的key和Value值");
                    form4.Text = "警告";
                    form4.ShowDialog();
                    return;
                }
                if (Form1.HeadDic.TryAdd(textEdit2.Text,textEdit1.Text))
                {

                }
                else
                {
                    Form4 form4 = new Form4("错误：Key已存在");
                    form4.Text = "错误";
                    form4.ShowDialog();
                }
            }
            key = null;
            this.Close();
        }

        private void checkEdit1_CheckedChanged(object sender, EventArgs e)
        {
            labelControl1.Enabled = !checkEdit1.Checked;
            comboBoxEdit1.Enabled = !checkEdit1.Checked;
            labelControl2.Enabled = checkEdit1.Checked;
            textEdit2.Enabled = checkEdit1.Checked;
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textEdit1_Click(object sender, EventArgs e)
        {
            if(comboBoxEdit1.SelectedIndex == 1)
            {
                this.textEdit1.Text = "1.0|1.1(选填,默认为1.0)";
                this.textEdit1.SelectAll();
            }
            if(comboBoxEdit1.SelectedIndex == 2)
            {
                this.textEdit1.Text = "IP地址:端口号|代理IP用户名|代理IP密码(用户名和密码选填)";
                this.textEdit1.SelectAll();
            }
        }

        private void simpleButton1_MouseEnter(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.InitialDelay = 200;
            toolTip.ReshowDelay = 300;
            toolTip.ShowAlways = true;
            toolTip.IsBalloon = true;
            toolTip.SetToolTip(simpleButton1, "确认添加");
        }

        private void simpleButton3_MouseEnter(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.InitialDelay = 200;
            toolTip.ReshowDelay = 300;
            toolTip.ShowAlways = true;
            toolTip.IsBalloon = true;
            toolTip.SetToolTip(simpleButton3, "取消添加");
        }
    }
}
