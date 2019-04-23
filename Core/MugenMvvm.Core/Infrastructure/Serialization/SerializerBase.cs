﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Infrastructure.Serialization
{
    public abstract class SerializerBase : ISerializer, IComponentOwnerAddedCallback<ISerializerHandler>, IComponentOwnerRemovedCallback<ISerializerHandler>
    {
        #region Fields

        private IComponentCollection<ISerializerHandler>? _handlers;

        #endregion

        #region Constructors

        protected SerializerBase(IServiceProvider serviceProvider, IComponentCollectionProvider componentCollectionProvider, IMetadataContextProvider metadataContextProvider)
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            Should.NotBeNull(metadataContextProvider, nameof(metadataContextProvider));
            ServiceProvider = serviceProvider;
            ComponentCollectionProvider = componentCollectionProvider;
            MetadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        protected IComponentCollectionProvider ComponentCollectionProvider { get; }

        protected IMetadataContextProvider MetadataContextProvider { get; }

        protected IServiceProvider ServiceProvider { get; }

        public IComponentCollection<ISerializerHandler> Handlers
        {
            get
            {
                if (_handlers == null)
                    ComponentCollectionProvider.LazyInitialize(ref _handlers, this);
                return _handlers;
            }
        }

        public abstract bool IsOnSerializingSupported { get; }

        public abstract bool IsOnSerializedSupported { get; }

        public abstract bool IsOnDeserializingSupported { get; }

        public abstract bool IsOnDeserializedSupported { get; }

        [field: ThreadStatic]
        public static ISerializationContext? CurrentSerializationContext { get; private set; }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<ISerializerHandler>.OnComponentAdded(object collection, ISerializerHandler component, IReadOnlyMetadataContext metadata)
        {
            OnHandlerAdded(component, metadata);
        }

        void IComponentOwnerRemovedCallback<ISerializerHandler>.OnComponentRemoved(object collection, ISerializerHandler component, IReadOnlyMetadataContext metadata)
        {
            OnHandlerRemoved(component, metadata);
        }

        public ISerializationContext GetSerializationContext(IServiceProvider? serviceProvider, IMetadataContext? metadata)
        {
            var context = GetSerializationContextInternal(serviceProvider, metadata);
            OnContextCreated(context);
            return context;
        }

        public bool CanSerialize(Type type, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(type, nameof(type));
            return CanSerializeInternal(type, metadata ?? Default.MetadataContext);
        }

        public Stream Serialize(object item, ISerializationContext? serializationContext)
        {
            Should.NotBeNull(item, nameof(item));
            if (serializationContext == null)
                serializationContext = GetSerializationContext(null, null);
            try
            {
                CurrentSerializationContext = serializationContext;
                return SerializeInternal(item);
            }
            finally
            {
                CurrentSerializationContext = null;
            }
        }

        public object Deserialize(Stream stream, ISerializationContext? serializationContext)
        {
            Should.NotBeNull(stream, nameof(stream));
            if (serializationContext == null)
                serializationContext = GetSerializationContext(null, null);
            try
            {
                CurrentSerializationContext = serializationContext;
                return DeserializeInternal(stream);
            }
            finally
            {
                CurrentSerializationContext = null;
            }
        }

        #endregion

        #region Methods

        protected abstract Stream SerializeInternal(object item);

        protected abstract object DeserializeInternal(Stream stream);

        protected virtual bool CanSerializeInternal(Type type, IReadOnlyMetadataContext metadata)
        {
            return type.IsSerializableUnified() || TryGetSurrogateSerializerHandler(type, out _, out _);
        }

        protected virtual void OnContextCreated(ISerializationContext context)
        {
            var handlers = GetHandlers();
            for (var i = 0; i < handlers.Length; i++)
                handlers[i].OnContextCreated(this, context);
        }

        protected virtual void OnSerializing(object? instance)
        {
            var handlers = GetHandlers();
            for (var i = 0; i < handlers.Length; i++)
                handlers[i].OnSerializing(this, instance, CurrentSerializationContext);
        }

        protected virtual void OnSerialized(object? instance)
        {
            var handlers = GetHandlers();
            for (var i = 0; i < handlers.Length; i++)
                handlers[i].OnSerialized(this, instance, CurrentSerializationContext);
        }

        protected virtual void OnDeserializing(object? instance)
        {
            var handlers = GetHandlers();
            for (var i = 0; i < handlers.Length; i++)
                handlers[i].OnDeserializing(this, instance, CurrentSerializationContext);
        }

        protected virtual void OnDeserialized(object? instance)
        {
            var handlers = GetHandlers();
            for (var i = 0; i < handlers.Length; i++)
                handlers[i].OnDeserialized(this, instance, CurrentSerializationContext);
        }

        protected virtual bool TryGetSurrogateSerializerHandler(Type type, [NotNullWhenTrue] out ISurrogateProviderSerializerHandler? provider,
            [NotNullWhenTrue] out Type? surrogateType)
        {
            var handlers = GetHandlers();
            for (var i = 0; i < handlers.Length; i++)
            {
                if (handlers[i] is ISurrogateProviderSerializerHandler surrogate)
                {
                    surrogateType = surrogate.TryGetSerializationType(this, type);
                    if (surrogateType != null)
                    {
                        provider = surrogate;
                        return true;
                    }
                }
            }

            provider = null;
            surrogateType = null;
            return false;
        }

        protected virtual Type? TryResolveType(string assemblyName, string typeName)
        {
            var handlers = GetHandlers();
            for (var i = 0; i < handlers.Length; i++)
            {
                if (handlers[i] is ITypeResolverSerializationHandler resolver)
                {
                    var type = resolver.TryResolveType(this, assemblyName, typeName);
                    if (type != null)
                        return type;
                }
            }

            return null;
        }

        protected virtual bool TryResolveName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            var handlers = GetHandlers();
            for (var i = 0; i < handlers.Length; i++)
            {
                if (handlers[i] is ITypeResolverSerializationHandler resolver && resolver.TryResolveName(this, serializedType, out assemblyName, out typeName))
                    return true;
            }

            assemblyName = null;
            typeName = null;
            return false;
        }

        protected virtual ISerializationContext GetSerializationContextInternal(IServiceProvider? serviceProvider, IMetadataContext? metadata)
        {
            return new SerializationContext(this, serviceProvider ?? ServiceProvider, metadata ?? MetadataContextProvider.GetMetadataContext(this, null));
        }

        protected virtual void OnHandlerAdded(ISerializerHandler handler, IReadOnlyMetadataContext metadata)
        {
        }

        protected virtual void OnHandlerRemoved(ISerializerHandler handler, IReadOnlyMetadataContext metadata)
        {
        }

        protected ISerializerHandler[] GetHandlers()
        {
            return _handlers.GetItemsOrDefault();
        }

        #endregion
    }
}