using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Unleash;

namespace WinFormsApp
{
    public partial class UnleashForm : Form
    {
        public IUnleash Unleash { get; set; }
        public UnleashSettings Settings { get; set; }

        public UnleashForm()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            var enabled = Unleash.IsEnabled(ToggleNameTextBox.Text);

            var enabledString = enabled 
                ? "enabled" 
                : "disabled";

            ResultLabel.Text = $"{ToggleNameTextBox.Text} is {enabledString} for user {UsernameTextBox.Text}";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start($"http://unleash.herokuapp.com/#/features/view/{ToggleNameTextBox.Text}");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            File.Delete(Settings.GetFeatureToggleFilePath());
            File.Delete(Settings.GetFeatureToggleETagFilePath());
        }

        private void ViewJson_Click(object sender, EventArgs e)
        {
            Process.Start(Settings.GetFeatureToggleFilePath());
        }

        private void ViewEtag_Click(object sender, EventArgs e)
        {
            Process.Start(Settings.GetFeatureToggleETagFilePath());
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            var enabled = Unleash.IsEnabled(ToggleNameTextBox.Text);
        }
    }
}
