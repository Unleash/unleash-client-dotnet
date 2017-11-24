using System;
using System.Diagnostics;
using System.Windows.Forms;
using Unleash;

namespace WinFormsApp
{
    public partial class UnleashForm : Form
    {
        public IUnleash Unleash { get; set; }

        public UnleashForm()
        {
            InitializeComponent();
        }

        private void IsEnabledButton_Click(object sender, EventArgs e)
        {
            UpdateLabel();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            var enabled = Unleash.IsEnabled(ToggleNameTextBox.Text);
            var enabledString = enabled ? "enabled" : "disabled";

            ResultLabel.Text = $"{ToggleNameTextBox.Text} is {enabledString} for user {UsernameTextBox.Text}";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://unleash.herokuapp.com/#/features/view/" + ToggleNameTextBox.Text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
