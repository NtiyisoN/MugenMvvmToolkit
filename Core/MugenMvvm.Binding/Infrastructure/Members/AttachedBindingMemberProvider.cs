﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Collections;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Members
{
    public sealed class AttachedBindingMemberProvider : AttachableComponentBase<IBindingMemberProvider>, IAttachedBindingMemberProvider
    {
        #region Fields

        private readonly CacheDictionary _cache;

        #endregion

        #region Constructors

        public AttachedBindingMemberProvider()
        {
            _cache = new CacheDictionary();
        }

        #endregion

        #region Implementation of interfaces

        public IBindingMemberInfo GetMember(Type type, string name, IReadOnlyMetadataContext metadata)
        {
            _cache.TryGetValue(new CacheKey(type, name), out var result);
            return result;
        }

        public IReadOnlyList<AttachedMemberRegistration> GetMembers(Type type, IReadOnlyMetadataContext metadata)
        {
            var members = new List<AttachedMemberRegistration>();
            foreach (var member in _cache)
            {
                if (member.Key.Type.IsAssignableFromUnified(type))
                    members.Add(new AttachedMemberRegistration(member.Key.Name, member.Value));
            }

            return members;
        }

        public void Register(Type type, IBindingMemberInfo member, string? name, IReadOnlyMetadataContext metadata)
        {
            _cache[new CacheKey(type, name ?? member.Name)] = member;
        }

        public bool Unregister(Type type, string? name, IReadOnlyMetadataContext metadata)
        {
            if (name != null)
                return _cache.Remove(new CacheKey(type, name));

            List<CacheKey>? toRemove = null;
            foreach (var keyValuePair in _cache)
            {
                if (keyValuePair.Key.Type != type)
                    continue;
                if (toRemove == null)
                    toRemove = new List<CacheKey>();
                toRemove.Add(keyValuePair.Key);
            }

            if (toRemove == null)
                return false;
            for (var index = 0; index < toRemove.Count; index++)
                _cache.Remove(toRemove[index]);
            return true;
        }

        #endregion

        #region Nested types

        private sealed class CacheDictionary : LightDictionaryBase<CacheKey, IBindingMemberInfo>
        {
            #region Constructors

            public CacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                return x.Name.Equals(y.Name) && x.Type.EqualsEx(y.Type);
            }

            protected override int GetHashCode(CacheKey key)
            {
                unchecked
                {
                    return (key.Type.GetHashCode() * 397 ^ key.Name.GetHashCode()) * 397;
                }
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct CacheKey
        {
            #region Fields

            public readonly string Name;
            public readonly Type Type;

            #endregion

            #region Constructors

            public CacheKey(Type type, string name)
            {
                Type = type;
                Name = name;
            }

            #endregion
        }

        #endregion
    }
}