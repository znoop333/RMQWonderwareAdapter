using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RMQWonderwareAdapter
{
    public partial class FormInputSubscription : Form
    {
        public FormInputSubscription()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void FormInputSubscription_Load(object sender, EventArgs e)
        {
            this.textBoxInput.Focus();
            this.ActiveControl = textBoxInput;
        }
    }
}
