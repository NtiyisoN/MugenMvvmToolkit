﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class CacheMemberManagerDecorator : ComponentDecoratorBase<IMemberManager, IMemberManagerComponent>, IMemberManagerComponent, IHasPriority, IHasCache
    {
        #region Fields

        private readonly TempCacheDictionary _cache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public CacheMemberManagerDecorator()
        {
            _cache = new TempCacheDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public void Invalidate<TState>(in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (!Default.IsValueType<TState>() && state is Type type)
            {
                LazyList<CacheKey> keys = default;
                Invalidate(_cache, type, ref keys);
                keys.List?.Clear();
            }
            else
                _cache.Clear();
        }

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers<TRequest>(Type type, MemberType memberTypes, MemberFlags flags, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TRequest>() || !(request is string name))
                return Components.TryGetMembers(type, memberTypes, flags, request, metadata);

            var cacheKey = new CacheKey(type, name, memberTypes, flags);
            if (!_cache.TryGetValue(cacheKey, out var members))
            {
                members = Components.TryGetMembers(type, memberTypes, flags, name, metadata).GetRawValue();
                _cache[cacheKey] = members;
            }

            return ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>.FromRawValue(members);
        }

        #endregion

        #region Methods

        protected override void DecorateInternal(IList<IMemberManagerComponent> components, IReadOnlyMetadataContext? metadata)
        {
            base.DecorateInternal(components, metadata);
            Invalidate<object?>(null, metadata);
        }

        protected override void OnAttachedInternal(IMemberManager owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            Invalidate<object?>(null, metadata);
        }

        protected override void OnDetachedInternal(IMemberManager owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            Invalidate<object?>(null, metadata);
        }

        private static void Invalidate(TempCacheDictionary dictionary, Type type, ref LazyList<CacheKey> keys)
        {
            foreach (var pair in dictionary)
            {
                if (pair.Key.Type == type)
                    keys.Add(pair.Key);
            }

            var list = keys.List;
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                    dictionary.Remove(list[i]);
            }
        }

        #endregion

        #region Nested types

        private sealed class TempCacheDictionary : LightDictionary<CacheKey, object?>
        {
            #region Constructors

            public TempCacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                return x.MemberType == y.MemberType && x.MemberFlags == y.MemberFlags && x.Key.Equals(y.Key) && x.Type == y.Type;
            }

            protected override int GetHashCode(CacheKey key)
            {
                return HashCode.Combine(key.Key, key.Type, (int)key.MemberType, (int)key.MemberFlags);
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct CacheKey
        {
            #region Fields

            public readonly string Key;
            public readonly Type Type;
            public readonly MemberType MemberType;
            public readonly MemberFlags MemberFlags;

            #endregion

            #region Constructors

            public CacheKey(Type type, string key, MemberType memberType, MemberFlags memberFlags)
            {
                Type = type;
                Key = key;
                MemberType = memberType;
                MemberFlags = memberFlags;
            }

            #endregion
        }

        #endregion
    }
}