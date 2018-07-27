using DevExpress.XtraEditors;
using DevExpress.XtraRichEdit;
using ExtractLib;
using HttpToolsLib;
using PControlsLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace 射线模拟提交工具
{
    public partial class Form1 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        String html = String.Empty;
        String Jhtml = String.Empty;
        HttpInfo info;
        public static ConcurrentDictionary<String, String> HeadDic = new ConcurrentDictionary<string, string>();
        ConcurrentStack<string> ipstack = new ConcurrentStack<string>();
        bool loopget = false;
        public static CreateHelper createHelper;
        private bool thread_flag = false;
        RichEditControl control = new RichEditControl();
        delegate void UpRichTextBox(object txt);
        String path = "配置文件";


        public Form1()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// 请求按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            GC.Collect();
            #region 请求前检查
            if (String.IsNullOrEmpty(textEdit1.Text))
            {
                Form4 form4 = new Form4("错误：请求地址不合法");
                form4.Text = "错误";
                form4.ShowDialog();
                return;
            }
            #endregion
            #region 清空上次请求内容
            xtraTabPage1.Controls.Clear();
            xtraTabPage2.Controls.Clear();
            xtraTabPage3.Controls.Clear();
            html = String.Empty;
            Jhtml = String.Empty;
            #endregion

            #region 状态显示
            barStaticItem1.Caption = String.Format("当前状态：以{0}方式{1}请求本链接", String.IsNullOrEmpty(richEditControl1.Text) ? "GET" : "POST", thread_flag ? "并发" : "普通");
            #endregion

            #region 配置请求头

            info = CreateHttp();
            #endregion
            if (!thread_flag)
            {
                //发送请求
                html = HttpMethod.HttpWork(ref info);
                //显示
                RichEditControl control1 = new RichEditControl();
                control1.Text = "Ret-Cookie:" + info.Cookie.ConventToString() + "\n" + html;
                control1.ActiveViewType = RichEditViewType.Simple;
                control1.Dock = DockStyle.Fill;
                control1.ReadOnly = true;
                xtraTabPage3.Controls.Add(control1);

                //渲染
                EWebBrowser browser = new EWebBrowser();
                browser.DocumentText = html;
                browser.Dock = DockStyle.Fill;
                xtraTabPage1.Controls.Add(browser);
                RichEditControl control2 = new RichEditControl();
                if (EWebBrowser.WaitWebPageLoad(browser))
                {
                    Jhtml = browser.Document.Body.OuterHtml;
                }
                control2.Text = Jhtml;
                control2.ActiveViewType = RichEditViewType.Simple;
                control2.Dock = DockStyle.Fill;
                control2.ReadOnly = true;
                xtraTabPage2.Controls.Add(control2);
                barStaticItem1.Caption = String.Format("当前状态：以{0}方式{1}请求完毕", String.IsNullOrEmpty(richEditControl1.Text) ? "GET" : "POST", thread_flag ? "并发" : "普通");
            }
            else
            {
                control = new RichEditControl();
                control.Dock = DockStyle.Fill;
                control.ReadOnly = true;
                control.ActiveViewType = RichEditViewType.Simple;
                xtraTabPage3.Controls.Add(control);
                ConfigFinishFunc();
            }
            tabPane1.SelectedPage = tabNavigationPage2;
        }
        #region 并发请求相关
        private void ConfigFinishFunc()
        {
            String RegStr = String.Empty;
            String proxyapi_url = String.Empty;
            String XpathStr = String.Empty;
            int ThreadNum = 0;
            if (createHelper.Check)
            {
                RegStr = createHelper.RegStr;
            }
            if (createHelper.CheckXpath)
            {
                XpathStr = createHelper.XpathStr;
            }
            if (createHelper.ProxyEnable)
            {
                proxyapi_url = createHelper.ProxyAPI;
                Thread ipthread = new Thread(new ThreadStart(delegate {
                    while (true)
                    {
                        if (ipstack.Count == 0)
                        {
                            List<string> iplist = GetIp(proxyapi_url);
                            foreach (var item in iplist)
                            {
                                ipstack.Push(item);
                            }
                        }
                        Thread.Sleep(1);
                    }
                }));
                ipthread.IsBackground = true;
                ipthread.Start();
            }
            loopget = createHelper.LoopGet;
            ThreadNum = createHelper.ThreadNum;
            Thread[] thread = new Thread[ThreadNum];
            for (int i = 0; i < ThreadNum; i++)
            {
                thread[i] = new Thread(new ThreadStart(delegate {
                    string nowip = string.Empty;
                    if (!String.IsNullOrEmpty(proxyapi_url))
                    {
                        while (String.IsNullOrEmpty(nowip))
                        {
                            if (ipstack.Count > 0)
                            {
                                string ip = string.Empty;
                                if (ipstack.TryPop(out ip))
                                {
                                    nowip = ip;
                                }
                            }
                            Thread.Sleep(1);
                        }
                    }
                    RunFunc(XpathStr, RegStr, proxyapi_url, createHelper.TimerEnable, nowip);
                    while (loopget)
                    {
                        RunFunc(XpathStr, RegStr, proxyapi_url, createHelper.TimerEnable, nowip);
                        Thread.Sleep(1);
                    }
                }));
                thread[i].IsBackground = true;
                thread[i].Start();
            }
            //for(int i = 0; i < ThreadNum; i++)
            //{
            //    thread[i].Join();
            //}
            if (!loopget)
            {
                barButtonItem1.Enabled = true;
                barButtonItem2.Enabled = true;
                barButtonItem12.Enabled = true;
                barButtonItem14.Enabled = true;
                barButtonItem4.Enabled = true;
                barButtonItem3.Enabled = true;
                barButtonItem16.Enabled = true;
            }
        }

        private void RunFunc(string xpathstr, string regstr, string proxyapi_url, bool timeflag, string ip)
        {
            GC.Collect();
            UpRichTextBox uptxt = new UpRichTextBox(UpRichTxt);

            #region 请求前检查
            if (String.IsNullOrEmpty(info.RequestUrl))
            {
                Form4 form4 = new Form4("错误，请求地址不合法");
                form4.Text = "错误";
                form4.ShowDialog();
                return;
            }
            #endregion

            #region 配置请求头
            HttpInfo info_goto = CreateHttp();
            if (!String.IsNullOrEmpty(ip))
            {
                info_goto.Ip = ip;
            }
            #endregion
            double time = 0;
            Stopwatch sw = new Stopwatch();
            if (timeflag)
            {
                sw.Start();
            }
            String Html = HttpMethod.HttpWork(ref info_goto);
            if (sw.IsRunning)
            {
                sw.Stop();
                time = sw.Elapsed.TotalSeconds;
            }
            String cookie = info_goto.Cookie.ConventToString();
            String _ip = info_goto.Ip;

            #region 正则判断访问结果
            if (!String.IsNullOrEmpty(regstr) && String.IsNullOrEmpty(xpathstr))
            {
                String regtxt = RegexMethod.GetSingleResult(regstr, Html);
                String status = String.Empty;
                String txt = String.Empty;
                if (String.IsNullOrEmpty(regtxt))
                {
                    status = "访问失败";
                }
                else
                {
                    status = "访问成功";
                }
                if (timeflag)
                {
                    if (!String.IsNullOrEmpty(proxyapi_url))
                    {
                        txt = "返回结果:" + status + "\r\n" + "ret-cookie:" + cookie + "\r\n" + "请求IP:" + _ip + "\r\n" + "请求时间:" + time + "\r\n";
                    }
                    else
                    {
                        txt = "返回结果:" + status + "\r\n" + "ret-cookie:" + cookie + "\r\n" + "请求时间:" + time + "\r\n";
                    }

                }
                else
                {
                    if (!String.IsNullOrEmpty(proxyapi_url))
                    {
                        txt = "返回结果:" + status + "\r\n" + "ret-cookie:" + cookie + "\r\n" + "请求IP:" + _ip + "\r\n";
                    }
                    else
                    {
                        txt = "返回结果:" + status + "\r\n" + "ret-cookie:" + cookie + "\r\n";
                    }
                }
                object[] arg = { txt };
                this.Invoke(uptxt, arg);
            }
            #endregion

            #region Xpath判断访问结果
            if (!String.IsNullOrEmpty(xpathstr) && String.IsNullOrEmpty(regstr))
            {
                String regtxt = XpathMethod.GetSingleResult(xpathstr, Html);
                String status = String.Empty;
                String txt = String.Empty;
                if (String.IsNullOrEmpty(regtxt))
                {
                    status = "访问失败";
                }
                else
                {
                    status = "访问成功";
                }
                if (timeflag)
                {
                    if (!String.IsNullOrEmpty(proxyapi_url))
                    {
                        txt = "返回结果:" + status + "\r\n" + "ret-cookie:" + cookie + "\r\n" + "请求IP:" + _ip + "\r\n" + "请求时间:" + time + "\r\n";
                    }
                    else
                    {
                        txt = "返回结果:" + status + "\r\n" + "ret-cookie:" + cookie + "\r\n" + "请求时间:" + time + "\r\n";
                    }

                }
                else
                {
                    if (!String.IsNullOrEmpty(proxyapi_url))
                    {
                        txt = "返回结果:" + status + "\r\n" + "ret-cookie:" + cookie + "\r\n" + "请求IP:" + _ip + "\r\n";
                    }
                    else
                    {
                        txt = "返回结果:" + status + "\r\n" + "ret-cookie:" + cookie + "\r\n";
                    }
                }
                object[] arg = { txt };
                this.Invoke(uptxt, arg);
            }
            #endregion

            #region 直接输出结果
            if (String.IsNullOrEmpty(regstr) && String.IsNullOrEmpty(xpathstr))
            {
                String txt = String.Empty;
                if (timeflag)
                {
                    if (!String.IsNullOrEmpty(proxyapi_url))
                    {
                        txt = "返回结果:" + Html + "\r\n" + "ret-cookie:" + cookie + "\r\n" + "请求IP:" + _ip + "\r\n" + "请求时间:" + time + "\r\n";
                    }
                    else
                    {
                        txt = "返回结果:" + Html + "\r\n" + "ret-cookie:" + cookie + "\r\n" + "请求时间:" + time + "\r\n";
                    }

                }
                else
                {
                    if (!String.IsNullOrEmpty(proxyapi_url))
                    {
                        txt = "返回结果:" + Html + "\r\n" + "ret-cookie:" + cookie + "\r\n" + "请求IP:" + _ip + "\r\n";
                    }
                    else
                    {
                        txt = "返回结果:" + Html + "\r\n" + "ret-cookie:" + cookie + "\r\n";
                    }
                }
                object[] arg = { txt };
                this.Invoke(uptxt, arg);
            }
            #endregion


        }
        /// <summary>
        /// 展示数据
        /// </summary>
        /// <param name="txt"></param>
        private void UpRichTxt(object txt)
        {
            if (txt != null)
            {
                control.Text = control.Text + txt.ToString();
                control.ScrollToCaret();
                //box1.Focus();
            }
        }

        /// <summary>
        /// 通过API接口获取IP
        /// </summary>
        /// <param name="proxyapi"></param>
        /// <returns></returns>
        public List<string> GetIp(String proxyapi)
        {
            List<string> iplist = new List<string>();
            String ip = String.Empty;
            String Html = String.Empty;
            Html = HttpMethod.FastGetMethod(proxyapi);
            iplist = RegexMethod.GetMutResult("[0-9]+?.[0-9]+?.[0-9]+?.[0-9]+?:[0-9]+", Html);
            return iplist;
        }
        #endregion

        /// <summary>
        /// 配置请求头
        /// </summary>
        /// <returns></returns>
        public HttpInfo CreateHttp()
        {
            HttpInfo info = new HttpInfo();
            #region 请求头配置
            info = new HttpInfo(textEdit1.Text);
            if (!String.IsNullOrEmpty(richEditControl1.Text))
            {
                info.PostData = richEditControl1.Text;
            }
            info.UseSystemProxy = checkEdit7.Checked;
            //info.Host = textBox17.Text;
            info.IgnoreWebException = true;
            info.User_Agent = textEdit2.Text;
            info.Referer = textEdit4.Text;
            info.ContentType = textEdit3.Text;
            info.Accept = textEdit5.Text;
            info.AcceptEncoding = textEdit6.Text;
            info.CheckUrl = checkEdit4.Checked;
            info.Expect100Continue = checkEdit5.Checked;
            if (!String.IsNullOrEmpty(textEdit8.Text))
            {
                info.Encoding = Encoding.GetEncoding(textEdit8.Text);
            }
            if (!String.IsNullOrEmpty(richEditControl2.Text))
            {
                info.Cookie = new CookieString(richEditControl2.Text, true);
            }
            else
            {
                info.CC = new CookieContainer();
            }
            info.AllowAutoRedirect = checkEdit1.Checked;
            info.KeepLive = checkEdit2.Checked;
            if (checkEdit3.Checked)
            {
                info.Header.Add("X-Requested-With", "XMLHttpRequest");
            }
            foreach (var item in HeadDic)
            {
                if(item.Key == "Host")
                {
                    info.Host = item.Value;
                    continue;
                }
                if(item.Key == "ProtocolVersion")
                {
                    if(item.Value == "1.0")
                    {
                        info.ProtocolVersion = ProtocolVersionEnum.V10;
                    }
                    else
                    {
                        info.ProtocolVersion = ProtocolVersionEnum.V11;
                    }
                    continue;
                }
                if(item.Key == "proxy")
                {
                    var arr = item.Value.Split('|');
                    if (arr.Length > 0)
                    {
                        info.Ip = arr[0];
                    }
                    if (arr.Length > 1)
                    {
                        info.Proxy_UserName = arr[1];
                    }
                    if (arr.Length > 2)
                    {
                        info.Proxy_PassWord = arr[2];
                    }
                    continue;
                }
                info.Header.Add(item.Key, item.Value);
            }
            #endregion

            return info;

        }
        /// <summary>
        /// 开启或关闭并发请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void repositoryItemCheckEdit17_CheckedChanged(object sender, EventArgs e)
        {
            if (barEditItem25.EditValue.Equals(true))
            {
                barStaticItem1.Caption = "当前状态：开启并发请求且配置并发请求参数";
                Form3 form3 = new Form3();
                form3.ShowDialog();
                if(form3.DialogResult == DialogResult.OK)
                {
                    thread_flag = true;
                    createHelper = form3.createHelper;
                    barStaticItem1.Caption = "当前状态：并发请求配置完毕";

                }
                if(form3.DialogResult == DialogResult.Cancel)
                {
                    thread_flag = false;
                    barStaticItem1.Caption = "当前状态：取消并发请求配置";
                    //将状态改为关闭
                }
            }
            else
            {
                barStaticItem1.Caption = "当前状态：关闭并发请求";
                thread_flag = false;
            }
        }

        #region 自定义请求头相关
        /// <summary>
        /// 添加请求头按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();
            AddItem(HeadDic);
        }
        /// <summary>
        /// 将请求头显示在listView方法
        /// </summary>
        /// <param name="dic"></param>
        private void AddItem(ConcurrentDictionary<String, string> dic)
        {
            listViewNF1.Items.Clear();
            foreach (var itemdic in dic)
            {
                ListViewItem item = new ListViewItem();
                item.Text = Convert.ToString(listViewNF1.Items.Count + 1);
                item.SubItems.Add(itemdic.Key);
                item.SubItems.Add(itemdic.Value);
                listViewNF1.Items.Add(item);
                listViewNF1.Items[listViewNF1.Items.Count - 1].EnsureVisible();
            }

        }
        /// <summary>
        /// 删除请求头按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            String key = listViewNF1.SelectedItems[0].SubItems[1].Text;
            String values = string.Empty;
            if (HeadDic.TryRemove(key,out values))
            {
                listViewNF1.Items[listViewNF1.SelectedItems[0].Index].Remove();
                AddItem(HeadDic);
                //listViewNF1.Items.RemoveAt(listViewNF1.SelectedItems[0].Index);
            }
            barStaticItem1.Caption = "当前状态：删除自定义请求头" + key;
        }
        /// <summary>
        /// 清空请求头按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void simpleButton3_Click(object sender, EventArgs e)
        {
            HeadDic = new ConcurrentDictionary<string, string>();
            Application.DoEvents();
            listViewNF1.BeginUpdate();
            while (listViewNF1.Items.Count > 0)
            {
                listViewNF1.Items.RemoveAt(listViewNF1.Items.Count - 1);
            }
            listViewNF1.EndUpdate();
            Application.DoEvents();
            barStaticItem1.Caption = "当前状态：清空自定义请求头";
        }
        /// <summary>
        /// 显示添加请求头按钮含义
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void simpleButton1_MouseEnter(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.InitialDelay = 200;
            toolTip.ReshowDelay = 300;
            toolTip.ShowAlways = true;
            toolTip.IsBalloon = true;
            toolTip.SetToolTip(simpleButton1, "添加请求头");
        }
        /// <summary>
        /// 显示删除选中请求头按钮含义
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void simpleButton2_MouseEnter(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.InitialDelay = 200;
            toolTip.ReshowDelay = 300;
            toolTip.ShowAlways = true;
            toolTip.IsBalloon = true;
            toolTip.SetToolTip(simpleButton2, "删除选中请求头");
        }
        /// <summary>
        /// 显示清空请求头按钮含义
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void simpleButton3_MouseEnter(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.InitialDelay = 200;
            toolTip.ReshowDelay = 300;
            toolTip.ShowAlways = true;
            toolTip.IsBalloon = true;
            toolTip.SetToolTip(simpleButton3, "清空请求头");
        }
        #endregion

        /// <summary>
        /// 下载图片按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem12_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            GC.Collect();
            xtraTabPage3.Controls.Clear();
            String checkurl = ".+\\..+";
            if (String.IsNullOrEmpty(textEdit1.Text))
            {
                Form4 form4 = new Form4("错误：请求地址不合法");
                form4.Text = "错误";
                form4.ShowDialog();
                barStaticItem1.Caption = "当前状态：取消图片请求";
                return;
            }
            if (!RegexMethod.CheckRegex(checkurl, textEdit1.Text))
            {
                Form5 form5 = new Form5("警告:未检测到常规格式的下载地址，是否继续");
                form5.Text = "警告";
                form5.ShowDialog();
                if(form5.DialogResult == DialogResult.Cancel)
                {
                    barStaticItem1.Caption = "当前状态：取消图片下载";
                    return;
                }
            }
            barStaticItem1.Caption = "下载图片:" + Path.GetFileName(textEdit1.Text);
            xtraTabPage1.Controls.Clear();
            xtraTabPage2.Controls.Clear();
            html = String.Empty;
            Jhtml = String.Empty;


            #region 请求头配置
            info = CreateHttp();

            #endregion

            try
            {
                Image img = HttpMethod.DownPic(info);
                if (img == null)
                {
                    Form4 form4 = new Form4("失败：下载失败");
                    form4.Text = "失败";
                    form4.ShowDialog();
                    return;
                }
                PictureEdit pictureEdit = new PictureEdit();
                pictureEdit.Dock = DockStyle.Fill;
                pictureEdit.Image = img;
                xtraTabPage3.Controls.Add(pictureEdit);
            }
            catch (Exception ex)
            {
                Form4 form4 = new Form4("程序错误：请联系程序员！");
                form4.Text = "错误";
                form4.ShowDialog();
            }
            barStaticItem1.Caption = "下载图片:" + Path.GetFileName(textEdit1.Text) + "完毕";
        }
        /// <summary>
        /// 下载文件按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem14_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            GC.Collect();

            #region 请求头配置
            info = CreateHttp();
            #endregion

            xtraTabPage3.Controls.Clear();
            String checkurl = ".+\\..+";
            if (String.IsNullOrEmpty(textEdit1.Text))
            {
                Form4 form4 = new Form4("错误：请求地址不合法");
                form4.Text = "错误";
                form4.ShowDialog();
                barStaticItem1.Caption = "当前状态：取消文件下载";
                return;
            }
            if (!RegexMethod.CheckRegex(checkurl, textEdit1.Text))
            {
                Form5 form5 = new Form5("警告:未检测到常规格式的下载地址，是否继续");
                form5.Text = "警告";
                form5.ShowDialog();
                if (form5.DialogResult == DialogResult.Cancel)
                {
                    barStaticItem1.Caption = "当前状态：取消文件下载";
                    return;
                }
            }
            SaveFileDialog sf = new SaveFileDialog();
            var arr = textEdit1.Text.Split('/');
            String filename = arr[arr.Length - 1];
            String type = filename.Split('.')[1];
            String name = filename.Split('.')[0];
            String filter = String.Format("{0}(*.{0})|*.{0}|所有文件(*.*)|*.*", type, type, type);
            sf.Filter = filter;//可以保存的格式
            sf.FileName = arr[arr.Length - 1];
            if (sf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (HttpMethod.DownLoadFile_ABPath(info, sf.FileName))
                {
                    Form4 form4 = new Form4("成功：下载成功");
                    form4.Text = "成功";
                    form4.ShowDialog();
                }
                else
                {
                    Form4 form4 = new Form4("失败：下载失败");
                    form4.Text = "失败";
                    form4.ShowDialog();
                }
            }
            barStaticItem1.Caption = "当前状态：完毕";
        }
        /// <summary>
        /// 重置配置按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            simpleButton3_Click(null, null);
            textEdit1.Text = "";
            textEdit2.Text = "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.04";
            textEdit4.Text = "";
            textEdit3.Text = "application/x-www-form-urlencoded; charset=UTF-8";
            textEdit5.Text = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            textEdit6.Text = "gzip, deflate";
            textEdit8.Text = "";
            textEdit9.Text = "";
            textEdit10.Text = "";
            textEdit11.Text = "";
            textEdit12.Text = "";
            richEditControl1.Text = "";
            richEditControl2.Text = "";
            richEditControl6.Text = "";
            richEditControl7.Text = "";
            checkEdit1.Checked = false;
            checkEdit2.Checked = true;
            checkEdit3.Checked = false;
            checkEdit4.Checked = true;
            checkEdit5.Checked = false;
            checkEdit6.Checked = false;
            checkEdit7.Checked = false;
            checkEdit8.Checked = false;
            checkEdit11.Checked = false;
            barStaticItem1.Caption = "当前状态：重置配置";
        }

        #region 使Xpath的两个rediobutton互斥
        /// <summary>
        /// 使用js执行后的Html开关点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkEdit9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkEdit9.Checked)
            {
                checkEdit10.Checked = false;
            }
            if (!checkEdit9.Checked)
            {
                checkEdit10.Checked = true;
            }
        }
        /// <summary>
        /// 多条开关点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkEdit10_CheckedChanged(object sender, EventArgs e)
        {
            if (checkEdit10.Checked)
            {
                checkEdit9.Checked = false;
            }
            if (!checkEdit10.Checked)
            {
                checkEdit9.Checked = true;
            }
        }
        #endregion

        private void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        #region 结果匹配操作相关
        /// <summary>
        /// 正则抽取匹配结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem6_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (String.IsNullOrEmpty(html))
            {
                Form4 form4 = new Form4("错误：网页源码为空");
                form4.Text = "错误";
                form4.ShowDialog();
                return;
            }
            if (String.IsNullOrEmpty(textEdit9.Text))
            {
                Form4 form4 = new Form4("错误：正则表达式为空");
                form4.Text = "错误";
                form4.ShowDialog();
                return;
            }
            if (String.IsNullOrEmpty(textEdit10.Text))
            {
                Form4 form4 = new Form4("提示：层数为空，将使用默认值0");
                form4.Text = "提示";
                form4.ShowDialog();
                textEdit10.Text = "0";
            }
            richEditControl6.Text = "";
            int lay = Convert.ToInt32(textEdit10.Text);
            if (checkEdit11.Checked)
            {
                try
                {
                    var list = RegexMethod.GetMutResult(textEdit9.Text, html, lay);
                    richEditControl6.Text = String.Join("\n", list);
                }
                catch
                {
                    richEditControl6.Text = "未找到匹配结果";
                }
            }
            else
            {
                try
                {
                    richEditControl6.Text = RegexMethod.GetSingleResult(textEdit9.Text, html, lay);
                }
                catch
                {
                    richEditControl6.Text = "未找到匹配结果";
                }
            }
        }
        /// <summary>
        /// Xpath抽取匹配结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem7_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (String.IsNullOrEmpty(textEdit11.Text))
            {
                Form4 form4 = new Form4("错误：Xpath表达式为空");
                form4.Text = "错误";
                form4.ShowDialog();
                return;
            }
            richEditControl7.Text = "";
            String text = checkEdit6.Checked ? Jhtml : html;
            //标识匹配或抽取
            int flag = 1;
            if (checkEdit9.Checked)
            {
                flag = 0;
            }
            if (checkEdit10.Checked)
            {
                flag = 1;
            }
            if (checkEdit8.Checked)
            {
                try
                {
                    var list = String.IsNullOrEmpty(textEdit12.Text) ? XpathMethod.GetMutResult(textEdit11.Text, text, flag) : XpathMethod.GetMutResult(textEdit11.Text, text, textEdit12.Text);
                    richEditControl7.Text = String.Join("\n", list);
                }
                catch
                {
                    richEditControl7.Text = "抽取失败";
                }
            }
            else
            {
                try
                {
                    richEditControl7.Text = String.IsNullOrEmpty(textEdit12.Text) ? XpathMethod.GetSingleResult(textEdit11.Text, text, flag) : XpathMethod.GetSingleResult(textEdit11.Text, text, textEdit12.Text);
                }
                catch
                {
                    richEditControl7.Text = "抽取失败";
                }
            }
        }
        #endregion

        private void barButtonItem16_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (loopget)
            {
                loopget = false;
                barButtonItem1.Enabled = true;
                barButtonItem2.Enabled = true;
                barButtonItem12.Enabled = true;
                barButtonItem14.Enabled = true;
                barButtonItem4.Enabled = true;
                barButtonItem3.Enabled = true;
            }
        }

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            String newpath = String.Empty;
            if(!Directory.Exists(path))
            {
                Form4 form4 = new Form4("错误：没有保存请求配置");
                form4.Text = "错误";
                form4.ShowDialog();
                return;
            }
            Form6 form6 = new Form6();
            form6.Text = "读取配置";
            form6.ShowDialog();
            if(form6.DialogResult == DialogResult.Cancel)
            {
                return;
            }
            if (form6.DialogResult == DialogResult.No)
            {
                Form4 form4 = new Form4("错误：程序错误，请联系程序员");
                form4.Text = "错误";
                form4.ShowDialog();
                return;
            }
            if (form6.DialogResult == DialogResult.OK)
            {
                newpath = form6.pathname;
            }
            using(StreamReader sr = new StreamReader(newpath))
            {
                string str = sr.ReadToEnd();
                var reglist = RegexMethod.GetMutResult(".*?----(.*)", str, 1);
                var newreglist = RegexMethod.GetMutResult("(.*?)----(.*)", str, 1);
                textEdit1.Text = reglist[reglist.Count - 9].Replace("\r", "");
                string postdata = reglist[reglist.Count - 8];
                postdata = postdata.Replace("\r\n", "");
                richEditControl1.Text = postdata.Replace("\r", ""); ;
                textEdit2.Text = reglist[reglist.Count - 7].Replace("\r", ""); ;
                textEdit4.Text = reglist[reglist.Count - 6].Replace("\r", ""); ;
                textEdit3.Text = reglist[reglist.Count - 5].Replace("\r", ""); ;
                textEdit5.Text = reglist[reglist.Count - 4].Replace("\r", ""); ;
                textEdit6.Text = reglist[reglist.Count - 3].Replace("\r", ""); ;
                textEdit8.Text = reglist[reglist.Count - 2].Replace("\r", ""); ;
                richEditControl2.Text = reglist[reglist.Count - 1].Replace("\r", "");
                if (reglist.Count > 9)
                {
                   for(int i = 0; i < reglist.Count - 9; i++)
                    {
                        HeadDic.TryAdd(newreglist[i], reglist[i]);
                    }
                }
                if (HeadDic.Count > 0)
                {
                    AddItem(HeadDic);
                }
            }
            barStaticItem1.Caption = "当前状态:读取配置完毕";
        }

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            String newpath = String.Empty;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                File.SetAttributes(path, FileAttributes.Hidden);
            }
            if (!String.IsNullOrEmpty(textEdit1.Text))
            {
                if (!textEdit1.Text.Contains("http"))
                {
                    Form4 form4 = new Form4("错误：RequestUrl格式错误");
                    form4.Text = "错误";
                    form4.ShowDialog();
                    return;
                }
                Form6 form6 = new Form6();
                form6.Text = "保存配置";
                form6.ShowDialog();
                if(form6.DialogResult == DialogResult.OK)
                {
                    newpath = form6.pathname;
                }
                if(form6.DialogResult == DialogResult.Cancel)
                {
                    return;
                }
                if(form6.DialogResult == DialogResult.No)
                {
                    Form4 form4 = new Form4("错误：程序错误，请联系程序员");
                    form4.Text = "错误";
                    form4.ShowDialog();
                    return;
                }
                if (!newpath.Contains(path + "\\"))
                {
                    newpath = path + "/" + newpath + ".txt";
                }
                try
                {
                    using (StreamWriter sw = new StreamWriter(newpath))
                    {
                        if (HeadDic.Count > 0)
                        {
                            foreach (var dic in HeadDic)
                            {
                                sw.WriteLine(dic.Key + "----" + dic.Value);
                            }
                        }
                        sw.WriteLine("RequestUrl----" + textEdit1.Text);
                        sw.WriteLine("PostData----" + richEditControl1.Text);
                        sw.WriteLine("User-Agent----" + textEdit2.Text);
                        sw.WriteLine("Referer----" + textEdit4.Text);
                        sw.WriteLine("Content-Type----" + textEdit3.Text);
                        sw.WriteLine("Accept----" + textEdit5.Text);
                        sw.WriteLine("Accept-Enconding----" + textEdit6.Text);
                        sw.WriteLine("Encoding----" + textEdit8.Text);
                        sw.Write("Cookie----" + richEditControl2.Text);
                    }
                    Form4 form4 = new Form4("成功：保存成功！");
                    form4.Text = "成功";
                    form4.ShowDialog();
                }
                catch
                {
                    Form4 form4 = new Form4("失败：程序错误，请联系程序员");
                    form4.Text = "失败";
                    form4.ShowDialog();
                }
                
            }
            else
            {
                Form4 form4 = new Form4("错误：RequestUrl不能为空");
                form4.Text = "错误";
                form4.ShowDialog();
                return;
            }
            barStaticItem1.Caption = "当前状态：保存配置完毕";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Show();
            textEdit1.Focus();
            richEditControl1.Text = "";
        }
    }
}
