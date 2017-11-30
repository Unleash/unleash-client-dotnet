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
    }
}
