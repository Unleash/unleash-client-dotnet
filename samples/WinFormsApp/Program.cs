using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Unleash;
using Unleash.Serialization;

namespace WinFormsApp
{
    static class Program
    {
        private static IUnleash unleash;
        private static UnleashSettings settings;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new UnleashForm();

            JsonSerializerTester.Assert(new NewtonsoftJson7Serializer());

            settings = new UnleashSettings
            {
                UnleashApi = new Uri("http://unleash.herokuapp.com/api/"),
                //UnleashApi = new Uri("http://localhost:4242/api/"),
                AppName = "dotnet-forms-test",
                InstanceTag = "instance 1",
                SendMetricsInterval = TimeSpan.FromSeconds(5),
                FetchTogglesInterval = TimeSpan.FromSeconds(10),
                UnleashContextProvider = new WinFormsContextProvider(form),
                JsonSerializer = new NewtonsoftJson7Serializer()
            };

            unleash = new DefaultUnleash(settings);
            form.Unleash = unleash;
            form.Settings = settings;

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

        public UnleashContext Context => new UnleashContext()
        {
            UserId = form.UsernameTextBox.Text,
            SessionId = "session1",
            RemoteAddress = "remoteAddress",
            Properties = new Dictionary<string, string>()
            {
                {"machineName", Environment.MachineName } // E.g.
            }
        };
    }

    public class NewtonsoftJson7Serializer : IJsonSerializer
    {
        private readonly Encoding utf8 = Encoding.UTF8;

        private static readonly JsonSerializer Serializer = new JsonSerializer()
        {
            ContractResolver = new CamelCaseExceptDictionaryKeysResolver()
        };

        public T Deserialize<T>(Stream stream)
        {
            using (var streamReader = new StreamReader(stream, utf8))
            using (var textReader = new JsonTextReader(streamReader))
            {
                return Serializer.Deserialize<T>(textReader);
            }
        }

        public void Serialize<T>(Stream stream, T instance)
        {
            using (var writer = new StreamWriter(stream, utf8, 1024 * 4, leaveOpen: true))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                Serializer.Serialize(jsonWriter, instance);

                jsonWriter.Flush();
                stream.Position = 0;
            }
        }

        class CamelCaseExceptDictionaryKeysResolver : CamelCasePropertyNamesContractResolver
        {
            protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
            {
                var contract = base.CreateDictionaryContract(objectType);

                contract.DictionaryKeyResolver = propertyName =>
                {
                    return propertyName;
                };

                return contract;
            }
        }
    }
}
