using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Unleash
{
    internal class UnleashServiceCollection : IUnleashServiceCollection
    {
        private readonly IServiceCollection _serviceCollection;

        public IConfiguration UnleashConfiguration { get; }

        public UnleashServiceCollection(IServiceCollection serviceCollection, IConfiguration unleashConfiguration)
        {
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            UnleashConfiguration = unleashConfiguration;
        }

        /// <inheritdoc />
        public IEnumerator<ServiceDescriptor> GetEnumerator() => _serviceCollection.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => _serviceCollection.GetEnumerator();

        /// <inheritdoc />
        public void Add(ServiceDescriptor item) => _serviceCollection.Add(item);

        /// <inheritdoc />
        public void Clear() => _serviceCollection.Clear();

        /// <inheritdoc />
        public bool Contains(ServiceDescriptor item) => _serviceCollection.Contains(item);

        /// <inheritdoc />
        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => _serviceCollection.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(ServiceDescriptor item) => _serviceCollection.Remove(item);

        /// <inheritdoc />
        public int Count => _serviceCollection.Count;

        /// <inheritdoc />
        public bool IsReadOnly => _serviceCollection.IsReadOnly;

        /// <inheritdoc />
        public int IndexOf(ServiceDescriptor item) => _serviceCollection.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, ServiceDescriptor item) => _serviceCollection.Insert(index, item);

        /// <inheritdoc />
        public void RemoveAt(int index) => _serviceCollection.RemoveAt(index);

        /// <inheritdoc />
        public ServiceDescriptor this[int index]
        {
            get => _serviceCollection[index];
            set => _serviceCollection[index] = value;
        }
    }
}
