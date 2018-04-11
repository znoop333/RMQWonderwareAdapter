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

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
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

            WWMgr = new WWMxAccessManager();
            WWMgr.LogMessage += WWMgr_LogMessage;
            WWMgr.DataChange += WWMgr_DataChange;
            WWMgr.WriteCompleted += WWMgr_WriteCompleted;

            WWMgr.Register();
        }

        private void WWMgr_WriteCompleted(object sender, WWMxWriteItemInfo e)
        {
            WWMgr_LogMessage(sender, !e.WriteOK ? " write failed!" : e.Item.ItemName + " was written" );
        }

        private void WWMgr_DataChange(object sender, WWMxItem e)
        {
            WWMgr_LogMessage(sender, e.ItemName + " was changed to " + e.LastValue);
        }

        private void WWMgr_LogMessage(object sender, string e)
        {
            _syncContext.Post(
                        delegate
                        {
                            if (textBoxLog.Created && !textBoxLog.IsDisposed)
                                LogHelper.AppendToTextbox(textBoxLog, e);

                            LogHelper.AppendToLogfile("main.txt", e);
                        }, null);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogHelper.AppendToLogfile("main.txt", "Form1_FormClosing");

            WWMgr.Unregister();

            LogHelper.AppendToLogfile("main.txt", "Unregister");

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

        private void button1_Click(object sender, EventArgs e)
        {
            WWMgr.Subscribe("DDESuite_CLX.MLeak.PSteering_OK");
            WWMgr.Subscribe("DDESuite_CLX.MLamp.M2Start");
            WWMgr.Subscribe("DDESuite_CLX.MLamp.M2Start_VTL_H");
            WWMgr.Subscribe("DDESuite_CLX.MTqPoint.M1_TQ06A");
            WWMgr.Subscribe("DDESuite_CLX.MTqPoint.M1_TQ06L");
            
        }

        bool debugVal = false;
        private void button2_Click(object sender, EventArgs e)
        {

            WWMgr.Write("DEBUG.HORN", false);

        }
    }
}
