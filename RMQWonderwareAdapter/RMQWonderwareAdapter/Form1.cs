﻿using Newtonsoft.Json;
using RMQWonderwareAdapter.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Win32Helper;

namespace RMQWonderwareAdapter
{
    /// <summary>
    /// Subscribes to PLC tags from Wonderware DA server and publish them into RabbitMQ.
    /// </summary>
    /// <remarks>
    /// This program subscribes to "/exchanges/plctags_in" and listens for command messages requesting:
    /// <list type="numbered">
    ///     <item>subscribe to a given PLC tag and publish any changes into "/exchanges/plctags_out" with the topic as the PLC Tag name</item>
    ///     <item>unsubscribe a given PLC tag (stop publishing)</item>
    ///     <item>immediately write a specific value to a given PLC tag and send a response (RFC-style) confirming the write</item>
    ///     <item>immediately read a given PLC tag (without subscribing) and send a response (RFC-style) with the current value</item>
    /// </list>
    /// JSON format is used for all messages
    /// </remarks>
    public partial class Form1 : Form
    {
        private string PC_ID;
        RMQManager rmq;
        private SynchronizationContext _syncContext = null;
        WWMxAccessManager WWMgr;

        private BindingSource SBind;
        List<WWMxItem> AllTags = new List<WWMxItem>();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            _syncContext = WindowsFormsSynchronizationContext.Current;
            PC_ID = Settings.Default.PC_ID;

            Directory.CreateDirectory(Settings.Default.LOG_PATH);

            MyDbLib.UPDATE_TRV_PCUPTIME(PC_ID);

            rmq = new RMQManager();
            rmq.HostName = Settings.Default.RMQHostName;
            rmq.VirtualHost = Settings.Default.RMQVirtualHost;
            rmq.UserName = Settings.Default.RMQUserName;
            rmq.Password = Settings.Default.RMQPassword;
            rmq.Port = Settings.Default.RMQPort;
            rmq.InboundExchangeName = Settings.Default.RMQInboundExchangeName;
            rmq.OutboundExchangeName = Settings.Default.RMQOutboundExchangeName;
            rmq.PC_ID = Settings.Default.PC_ID;

            rmq.MessageArrived += Rmq_MessageArrived;
            rmq.LogMessage += Rmq_LogMessage;

            bool connected = await rmq.Connect();
            if (connected)
            {
                if (!rmq.Subscribe())
                    LogToGUI("Failed to Subscribe!");
            }
            else
                LogToGUI("Failed to Connect!");

            WWMgr = new WWMxAccessManager();
            WWMgr.LogMessage += WWMgr_LogMessage;
            WWMgr.DataChange += WWMgr_DataChange;
            WWMgr.WriteCompleted += WWMgr_WriteCompleted;

            WWMgr.Register();

            SetupDataGridView();
            RefreshData();

            LogToGUI("Program loaded");

            ReadSubscriptionsFromDatabase();

        }

        private void ReadSubscriptionsFromDatabase()
        {
            var dt = MyDbLib.PLC_TAG_LIST_USED();
            if (dt == null || dt.Rows.Count == 0)
                return;

            var pi = new ProgramInfo();
            LogToGUI("Reading Subscriptions From Database");

            foreach (var dr in dt)
            {
                var info = MyDbLib.PLC_TAG_LOOKUP_WW_ITEM_NAME(dr.PLC_IP, dr.TAG_ID);
                if (info == null)
                    continue;

                var b = WWMgr.Subscribe(dr.PLC_IP, dr.TAG_ID, info.WW_ITEM_NAME, dr.CorrelationId, info.DESCRIPTION);

                if (b)
                {
                    LogToGUI("Resubscribed to " + info.WW_ITEM_NAME + " from PLC " + dr.PLC_IP + " Tag " + dr.TAG_ID + " (" + info.DESCRIPTION + ")");
                    this.labelStatus.Text = "Resubscribed to " + info.WW_ITEM_NAME;
                    MyDbLib.PLC_TAG_SUBSCRIBED(dr.PLC_IP, dr.TAG_ID, pi.IP, dr.RequesterName, dr.CorrelationId);
                }
                else
                {
                    LogToGUI("FAILED to resubscribe to " + info.WW_ITEM_NAME + " from PLC " + dr.PLC_IP + " Tag " + dr.TAG_ID + " (" + info.DESCRIPTION + ")");
                    this.labelStatus.Text = "FAILED to resubscribe to " + info.WW_ITEM_NAME;
                    MyDbLib.PLC_TAG_SUBSCRIPTION_CHANGED(dr.PLC_IP, dr.TAG_ID, "N", "N", pi.IP, dr.RequesterName, dr.CorrelationId);
                }
            }

        }

        private void Rmq_LogMessage(object sender, string e)
        {
            _syncContext.Post(
                        delegate
                        {
                            Console.WriteLine(e);
                            LogToGUI(e);
                        }, null);
        }

        private void Rmq_MessageArrived(object sender, RMQManager.EventArgsMessageArrived e)
        {
            _syncContext.Post(
                        delegate
                        {
                            Console.WriteLine(e.ToString());
                            HandleMessage(sender, e);
                        }, null);
        }

        private void HandleMessage(object sender, RMQManager.EventArgsMessageArrived e)
        {
            string s;
            RmqCommandMessage ParsedMessage;
            try
            {
                ParsedMessage = JsonConvert.DeserializeObject<RmqCommandMessage>(e.OriginalMessageString);
            }
            catch (JsonReaderException jre)
            {
                s = "JSON parsing error! " + jre.ToString();
                LogToGUI(s);
                Win32Helper.LogHelper.AppendToLogfile(FilenameForLog("JSON"), s);
                return;
            }

            s = "Received JSON: " + e.OriginalMessageString;
            LogToGUI(s);
            Win32Helper.LogHelper.AppendToLogfile(FilenameForLog("JSON"), s);
            bool b;

            InternalState = 2;
            SEQUENCE_NO = ParsedMessage.TagName;
            VIN = ParsedMessage.Command;
            LastAction = DateTime.Now;

            var dr = MyDbLib.PLC_TAG_LOOKUP_WW_ITEM_NAME(ParsedMessage.PLC_IP, ParsedMessage.TagName);
            if (dr == null)
            {
                LogToGUI("PLC " + ParsedMessage.PLC_IP + " Tag " + ParsedMessage.TagName + " not found in MST_PLC_TAGS!");
                return;
            }

            var itemName = dr.WW_ITEM_NAME;

            switch (ParsedMessage.Command)
            {
                case "SUBSCRIBE":
                    b = WWMgr.Subscribe(ParsedMessage.PLC_IP, ParsedMessage.TagName, itemName, e.Message.BasicProperties.CorrelationId, dr.DESCRIPTION);
                    if(b)
                    {
                        LogToGUI("Subscribed to " + ParsedMessage.TagName + " from PLC " + ParsedMessage.PLC_IP + " Tag " + ParsedMessage.TagName + " (" + dr.DESCRIPTION + ")");
                        this.labelStatus.Text = "Subscribed to " + ParsedMessage.TagName;
                        MyDbLib.PLC_TAG_SUBSCRIBED(ParsedMessage.PLC_IP, ParsedMessage.TagName, ParsedMessage.RequesterIP, ParsedMessage.RequesterName, e.Message.BasicProperties.CorrelationId);
                    }
                    else
                    {
                        LogToGUI("FAILED to subscribed to " + ParsedMessage.TagName + " from PLC " + ParsedMessage.PLC_IP + " Tag " + ParsedMessage.TagName + " (" + dr.DESCRIPTION + ")");
                        this.labelStatus.Text = "Subscribed to " + ParsedMessage.TagName;
                    }

                    break;
                case "UNSUBSCRIBE":
                    WWMgr.Unsubscribe(ParsedMessage.TagName, e.Message.BasicProperties.CorrelationId);
                    MyDbLib.PLC_TAG_SUBSCRIPTION_CHANGED(ParsedMessage.PLC_IP, ParsedMessage.TagName, "N", "N", null, null, null);
                    break;
                case "READ":

                    LogToGUI("GetLastValue for " + ParsedMessage.PLC_IP + ", " + ParsedMessage.TagName + ", " + ParsedMessage.CorrelationId );
                    var ii = this.AllTags.Where(t => t.TagName == ParsedMessage.TagName && t.PLC_IP == ParsedMessage.PLC_IP).FirstOrDefault();
                    if(ii != null)
                    {
                        LogToGUI("GetLastValue for " + ParsedMessage.TagName );
                        SendResponse(ii);
                    }
                    else
                        LogToGUI("Item " + ParsedMessage.TagName + " not advised");
                    break;
                case "WRITE":
                    LogToGUI("Trying to write to tag " + ParsedMessage.TagName  + " with value " + ParsedMessage.Value);
                    WWMgr.Write(ParsedMessage.PLC_IP, ParsedMessage.TagName, itemName, ParsedMessage.Value, e.Message.BasicProperties.CorrelationId, dr.DESCRIPTION);
                    MyDbLib.PLC_TAG_UPDATED(ParsedMessage.PLC_IP, ParsedMessage.TagName, ParsedMessage.Value, DateTime.Now, 0);
                    break;
                default:
                    LogToGUI("Unknown command " + ParsedMessage.Command);
                    break;
            }

            RefreshData();

        }

        private void SendResponse(WWMxItem i)
        {
            RmqResponseMessage m1 = new RmqResponseMessage();
            if (i == null || m1 == null)
            {
                LogToGUI("new RmqResponseMessage failed??");
                return;
            }

            m1.Command = "LastValue";
            m1.PLC_IP = i.PLC_IP;
            m1.TagName = i.TagName;
            m1.ItemName = i.ItemName;
            m1.Value = i.LastValue?.ToString();
            m1.CorrelationId = i.CorrelationId;
            m1.Timestamp = i.LastTimestamp;
            m1.DataType = i.LastValue?.GetType().Name;
            m1.Description = i.Description;

            string key = i.ItemName, Message = JsonConvert.SerializeObject(m1);
            rmq.PutMessage(key, Message);
        }

        private void LogToGUI(string s)
        {
            if (textBoxLog.Created && !textBoxLog.IsDisposed)
                LogHelper.AppendToTextbox(textBoxLog, s);

            LogHelper.AppendToLogfile(FilenameForLog("main"), s);
        }

        public static string FilenameForLog(string filename)
        {
            return String.Format("{0}\\{1}\\{2}-{3}.log", Application.StartupPath, Settings.Default.LOG_PATH, filename, DateTime.Now.ToString("yyyyMMdd"));
        }

        private void WWMgr_WriteCompleted(object sender, WWMxWriteItemInfo e)
        {
            WWMgr_LogMessage(sender, !e.WriteOK ? " write failed!" : e.Item?.ItemName + " was written" );

            if(e.WriteOK && e.Item != null)
            {
                // update the in-memory cache
                AllTags.Where(t => t.ItemName == e.Item.ItemName).ToList().ForEach( t => t.LastValue = e.Item.LastValue );
            }
        }

        private void WWMgr_DataChange(object sender, WWMxItem e)
        {
            WWMgr_LogMessage(sender, e.ItemName + " was changed to " + e.LastValue);
            this.labelStatus.Text = e.ItemName + " was changed to " + e.LastValue;

            InternalState = 1;
            SEQUENCE_NO = e.ItemName;
            VIN = e.LastValue.ToString();
            LastAction = DateTime.Now;

            DateTime updated;
            if (!DateTime.TryParse(e.LastTimestamp, out updated))
                updated = DateTime.Now;

            MyDbLib.PLC_TAG_UPDATED(e.PLC_IP, e.TagName, e.LastValue.ToString(), updated, e.LastQuality);

            RmqResponseMessage m = new RmqResponseMessage();
            m.Command = "DataChange";
            m.PLC_IP = e.PLC_IP;
            m.ItemName = e.ItemName;
            m.TagName = e.TagName;
            m.Value = e.LastValue.ToString();
            m.CorrelationId = e.CorrelationId;
            m.Timestamp = e.LastTimestamp;
            m.DataType = e.LastValue.GetType().Name;
            m.Description = e.Description;

            string key = e.ItemName, Message = JsonConvert.SerializeObject(m);
            rmq.PutMessage(key, Message);

            RefreshData();
        }

        private void WWMgr_LogMessage(object sender, string e)
        {
            _syncContext.Post(
                        delegate
                        {
                            Console.WriteLine(e);
                            LogToGUI(e);
                        }, null);
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var fn = FilenameForLog("main");
            LogHelper.AppendToLogfile(fn, "Form1_FormClosing");
            LogHelper.FlushLogFiles();

            WWMgr.Unregister();

            LogHelper.AppendToLogfile(fn, "Unregister");
            LogHelper.FlushLogFiles();
            LogHelper.CloseLogFiles();

            //await Task.Delay(15000);
            //Environment.Exit(0);

            // is channel.BasicCancel() causing a hang here? see https://github.com/rabbitmq/rabbitmq-dotnet-client/issues/341
            //await rmq.Unsubscribe();
            await rmq.Disconnect();

            LogHelper.AppendToLogfile(fn, "RMQ exited");

            LogHelper.FlushLogFiles();
            LogHelper.CloseLogFiles();
        }

        private void timerFlushLogs_Tick(object sender, EventArgs e)
        {
            LogHelper.FlushLogFiles();
        }

        private void timerFlushTextboxes_Tick(object sender, EventArgs e)
        {
            LogHelper.FlushTextbox(this.textBoxLog);
        }

        private void timerCloseLogFiles_Tick(object sender, EventArgs e)
        {
            LogHelper.CloseLogFiles();
        }

        private async void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();

            await Task.Delay(15000);
            Environment.Exit(0);
        }

        private void addSubscriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var inputDlg = new FormInputSubscription())
            {
                if (inputDlg.ShowDialog(this) != DialogResult.OK)
                    return;

                var s = inputDlg.textBoxInput.Text;
                if (s.Length > 0)
                {
                    var dr = MyDbLib.PLC_TAG_LOOKUP_PLC_IP(s);
                    if(dr == null)
                    {
                        LogToGUI("Wonderware item " + s + " not found in MST_PLC_TAGS!");
                        return;
                    }

                    LogToGUI("Subscribing Wonderware item " + s + " from PLC + " + dr.PLC_IP + " Tag " + dr.TAG_ID + " (" + dr.DESCRIPTION + ")");
                    WWMgr.Subscribe(dr.PLC_IP, dr.TAG_ID, s, null, dr.DESCRIPTION);

                    ProgramInfo pi = new ProgramInfo();
                    MyDbLib.PLC_TAG_SUBSCRIBED(dr.PLC_IP, dr.TAG_ID, pi.IP, "RMQWonderwareAdapter", "");
                    RefreshData();
                }
            }
        }

        private void removeAllSubscriptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogToGUI("Removing All Subscriptions...");
            WWMgr.RemoveAll();
            RefreshData();
            LogToGUI("Remove All Subscriptions done");
        }

        private void SetupDataGridView()
        {
            SBind = new BindingSource();
            SBind.DataSource = AllTags;

            this.dataGridViewTags.AutoGenerateColumns = false;
            this.dataGridViewTags.DataSource = SBind;

            var c = new DataGridViewTextBoxColumn()
            {
                Name = "PLC_IP",
                HeaderText = "PLC",
                DataPropertyName = "PLC_IP",
                SortMode = DataGridViewColumnSortMode.Automatic
            };
            dataGridViewTags.Columns.Add(c);

            c = new DataGridViewTextBoxColumn()
            {
                Name = "TagName",
                HeaderText = "Tag",
                DataPropertyName = "TagName",
                SortMode = DataGridViewColumnSortMode.Automatic
            };
            dataGridViewTags.Columns.Add(c);

            c = new DataGridViewTextBoxColumn()
            {
                Name = "ItemName",
                HeaderText = "WW Item",
                DataPropertyName = "ItemName",
                SortMode = DataGridViewColumnSortMode.Automatic
            };
            dataGridViewTags.Columns.Add(c);

            c = new DataGridViewTextBoxColumn()
            {
                Name = "LastValue",
                HeaderText = "Value",
                DataPropertyName = "LastValue",
                SortMode = DataGridViewColumnSortMode.Automatic
            };
            dataGridViewTags.Columns.Add(c);

            c = new DataGridViewTextBoxColumn()
            {
                Name = "LastTimestamp",
                HeaderText = "Timestamp",
                DataPropertyName = "LastTimestamp",
                SortMode = DataGridViewColumnSortMode.Automatic
            };
            dataGridViewTags.Columns.Add(c);

            c = new DataGridViewTextBoxColumn()
            {
                Name = "Description",
                HeaderText = "Description",
                DataPropertyName = "Description",
                SortMode = DataGridViewColumnSortMode.Automatic
            };
            dataGridViewTags.Columns.Add(c);

            c = new DataGridViewTextBoxColumn()
            {
                Name = "OnAdvise",
                HeaderText = "Advised",
                DataPropertyName = "OnAdvise",
                SortMode = DataGridViewColumnSortMode.Automatic
            };
            dataGridViewTags.Columns.Add(c);

            c = new DataGridViewTextBoxColumn()
            {
                Name = "CorrelationId",
                HeaderText = "Correlation ID",
                DataPropertyName = "CorrelationId",
                SortMode = DataGridViewColumnSortMode.Automatic
            };
            dataGridViewTags.Columns.Add(c);
            
        }

        void RefreshData()
        {
            this.AllTags = WWMgr.GetAllTags();
            this.SBind.DataSource = this.AllTags;
            this.dataGridViewTags.Refresh();
        }

        private void writeValueToTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var inputDlg = new FormWriteTag())
            {
                var Tag1 = "";
                var selectedRowCount = dataGridViewTags.Rows.GetRowCount(DataGridViewElementStates.Selected);
                if (selectedRowCount > 0)
                {
                    Tag1 = dataGridViewTags.SelectedRows[0].Cells["ItemName"].Value as string;
                }

                inputDlg.textBoxInput.Text = Tag1;

                if (inputDlg.ShowDialog(this) != DialogResult.OK)
                    return;

                var Tag = inputDlg.textBoxInput.Text;
                var Value = inputDlg.textBoxValue.Text;
                if (Tag.Length > 0 && Value.Length > 0)
                {
                    var i = AllTags.Where(t => t.ItemName == Tag).FirstOrDefault();
                    var dr = MyDbLib.PLC_TAG_LOOKUP_PLC_IP(Tag);
                    if (dr == null)
                    {
                        LogToGUI("Failed to find Wonderware item " + Tag + " in MST_PLC_TAGS");
                        return;
                    }

                    if (i == null || !i.OnAdvise)
                    {
                        LogToGUI("Subscribing Wonderware item " + Tag);
                        WWMgr.Subscribe(dr.PLC_IP, dr.TAG_ID, Tag, null, dr.DESCRIPTION);
                    }

                    LogToGUI("Trying to write to tag " + Tag  + " with value " + Value);
                    WWMgr.Write(dr.PLC_IP, dr.TAG_ID, Tag, Value, null, dr.DESCRIPTION);

                    RefreshData();
                }
            }
        }


        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Program.WM_SHOWME_RESTORE_WINDOW)
            {
                ShowMe();
            }
            base.WndProc(ref m);
        }
        private void ShowMe()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
            // get our current "TopMost" value (ours will always be false though)
            bool top = TopMost;
            // make our form jump to the top of everything
            TopMost = true;
            // set it back to whatever it was
            TopMost = top;
        }

        private int InternalState = 0;
        private string SEQUENCE_NO = "", VIN = "";
        private DateTime LastAction = DateTime.Now;

        private void timerDeadmanSwitch_Tick(object sender, EventArgs e)
        {
            MyDbLib.UPDATE_TRV_PCINFO(PC_ID, DateTime.Now, InternalState, SEQUENCE_NO, VIN, LastAction);
        }
    }
}
