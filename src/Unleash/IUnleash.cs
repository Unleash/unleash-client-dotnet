using System;

namespace Unleash
{
    public interface IUnleash : IDisposable
    {
        bool IsEnabled(string toggleName);
        bool IsEnabled(string toggleName, bool defaultSetting);
    }
}