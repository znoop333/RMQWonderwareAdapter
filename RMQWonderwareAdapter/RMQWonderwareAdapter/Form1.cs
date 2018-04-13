using Newtonsoft.Json;
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
    /// This program subscribes to "/exchanges/PLC_TAG_MANAGER" and listens for command messages requesting:
    /// <list type="numbered">
    ///     <item>subscribe to a given PLC tag and publish any changes into "/exchanges/PLC_TAGS" with the topic as the PLC Tag name</item>
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

            switch (ParsedMessage.Command)
            {
                case "SUBSCRIBE":
                    b = WWMgr.Subscribe(ParsedMessage.TagName, e.Message.BasicProperties.CorrelationId);
                    if(b)
                    {
                        LogToGUI("Subscribed to " + ParsedMessage.TagName);
                        this.labelStatus.Text = "Subscribed to " + ParsedMessage.TagName;
                    }
                    else
                    {
                        LogToGUI("FAILED to subscribed to " + ParsedMessage.TagName);
                        this.labelStatus.Text = "Subscribed to " + ParsedMessage.TagName;
                    }

                    break;
                case "UNSUBSCRIBE":
                    WWMgr.Unsubscribe(ParsedMessage.TagName, e.Message.BasicProperties.CorrelationId);
                    break;
                case "READ":
                    var ii = this.AllTags.Where(t => t.ItemName == ParsedMessage.TagName).FirstOrDefault();
                    if(ii != null)
                    {
                        LogToGUI("GetLastValue for " + ParsedMessage.TagName );
                        SendResponse(ii);
                    }
                    else
                        LogToGUI("Item " + ParsedMessage.TagName + " not advised");
                    //WWMgr.ReadOnce(ParsedMessage.TagName, e.Message.BasicProperties.CorrelationId);
                    break;
                case "WRITE":
                    WWMgr.Write(ParsedMessage.TagName, ParsedMessage.Value, e.Message.BasicProperties.CorrelationId);
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
            m1.TagName = i.ItemName;
            m1.Value = i.LastValue?.ToString();
            m1.CorrelationId = i.CorrelationId;
            m1.Timestamp = i.LastTimestamp;
            m1.DataType = i.LastValue?.GetType().Name;

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
            WWMgr_LogMessage(sender, !e.WriteOK ? " write failed!" : e.Item.ItemName + " was written" );
        }

        private void WWMgr_DataChange(object sender, WWMxItem e)
        {
            WWMgr_LogMessage(sender, e.ItemName + " was changed to " + e.LastValue);
            this.labelStatus.Text = e.ItemName + " was changed to " + e.LastValue;

            RmqResponseMessage m = new RmqResponseMessage();
            m.Command = "DataChange";
            m.TagName = e.ItemName;
            m.Value = e.LastValue.ToString();
            m.CorrelationId = e.CorrelationId;
            m.Timestamp = e.LastTimestamp;
            m.DataType = e.LastValue.GetType().Name;

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

        private void showSubscriptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogToGUI("GetSubScriptions:");

            var L = WWMgr.GetSubScriptions();
            foreach (var i in L)
            {
                LogToGUI(String.Format("Tag {0}, handle {1}, Last Value {2}, timestamp {3}, quality {4}", i.ItemName, i.hItem, i.LastValue, i.LastTimestamp, i.LastQuality));
            }

            if (L.Count == 0)
                LogToGUI("No subscriptions are advised");
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
                    LogToGUI("Subscribing PLC tag " + s);
                    WWMgr.Subscribe(s, null);
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
                Name = "ItemName",
                HeaderText = "Name",
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

    }
}
