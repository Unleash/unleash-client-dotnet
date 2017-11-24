using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Unleash;

namespace WinFormsApp
{
    static class Program
    {
        private static IUnleash unleash;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new UnleashForm();

            var unleashConfig = new UnleashConfig()
                    .SetAppName("dotnet-forms-test")
                    .SetInstanceId("instance 1")
                    .SetFetchTogglesInterval(TimeSpan.FromSeconds(2))
                    .SetSendMetricsInterval(TimeSpan.FromSeconds(20))
                    .EnableMetrics()
                    .UnleashContextProvider(new WinFormsContextProvider(form))
                    .SetUnleashApi("http://unleash.herokuapp.com/");

            unleash = new DefaultUnleash(unleashConfig);
            form.Unleash = unleash;

            Application.ApplicationExit += (sender, args) =>
            {
                unleash?.Dispose();
            };

            Application.Run(form);
        }
    }

    internal class WinFormsContextProvider : IUnleashContextProvider
    {
        private readonly UnleashForm form;

        public WinFormsContextProvider(UnleashForm form)
        {
            this.form = form;
        }

        public UnleashContext Context => new UnleashContext(
            form.UsernameTextBox.Text, 
            "session1", 
            "remoteAddress", 
            new Dictionary<string, string>());
    }
}
