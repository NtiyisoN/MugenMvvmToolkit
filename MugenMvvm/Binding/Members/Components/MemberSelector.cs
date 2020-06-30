﻿using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class MemberSelector : IMemberManagerComponent
    {
        #region Fields

        private readonly SelectorDictionary _selectorDictionary;

        private const int AttachedPriority = 1000000;
        private const int InstancePriority = 100000;
        private const int ExtensionPriority = 10000;
        private const int DynamicPriority = 1000;
        private const int MaxDeclaringTypePriority = 100;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberSelector()
        {
            _selectorDictionary = new SelectorDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Selector;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers<TRequest>(Type type, MemberType memberTypes, MemberFlags flags, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (TypeChecker.IsValueType<TRequest>() || !(request is IReadOnlyList<IMemberInfo> members))
                return default;

            _selectorDictionary.Clear();
            for (var i = 0; i < members.Count; i++)
            {
                var member = members[i];
                var memberType = member.MemberType;
                if (!memberTypes.HasFlagEx(memberType) || !flags.HasFlagEx(member.AccessModifiers))
                    continue;

                if (_selectorDictionary.TryGetValue(member, out var list))
                {
                    if (list.AddMember(member, GetPriority(member, type)))
                        _selectorDictionary[member] = list;
                }
                else
                    _selectorDictionary[member] = new MemberList(member, GetPriority(member, type));
            }

            if (_selectorDictionary.Count == 0)
                return default;
            if (_selectorDictionary.Count == 1)
                return new ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>(_selectorDictionary.FirstOrDefault.Value.GetBestMember());
            var result = new IMemberInfo[_selectorDictionary.Count];
            var index = 0;
            foreach (var pair in _selectorDictionary)
                result[index++] = pair.Value.GetBestMember();
            return result;
        }

        #endregion

        #region Methods

        private static int GetPriority(IMemberInfo member, Type requestedType)
        {
            var priority = (requestedType == member.DeclaringType ? MaxDeclaringTypePriority : 0) + GetArgsPriority(member);
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Attached))
                return AttachedPriority + priority;
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Extension))
                return ExtensionPriority + priority;
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Dynamic))
                return DynamicPriority + priority;
            return InstancePriority + priority;
        }

        private static int GetArgsPriority(IMemberInfo member)
        {
            if (!(member is IHasArgsMemberInfo hasArgs))
                return 0;
            switch (hasArgs.ArgumentFlags)
            {
                case ArgumentFlags.Metadata:
                    return -1;
                case ArgumentFlags.Optional:
                    return -2;
                case ArgumentFlags.ParamArray:
                    return -3;
                case ArgumentFlags.EmptyParamArray:
                    return -4;
                default:
                    return 0;
            }
        }

        #endregion

        #region Nested types

        private struct MemberList
        {
            #region Fields

            private int _currentPriority;
            private object? _members;

            #endregion

            #region Constructors

            public MemberList(IMemberInfo member, int priority)
            {
                _members = member;
                _currentPriority = priority;
            }

            #endregion

            #region Methods

            public bool AddMember(IMemberInfo member, int priority)
            {
                if (priority < _currentPriority)
                    return false;

                if (priority > _currentPriority)
                {
                    _currentPriority = priority;
                    if (_members is List<IMemberInfo> list)
                        list.Clear();
                    else
                        _members = null;
                }

                if (_members == null)
                    _members = member;
                else if (_members is List<IMemberInfo> list)
                    list.Add(member);
                else
                    _members = new List<IMemberInfo> { (IMemberInfo)_members, member };
                return true;
            }

            public IMemberInfo GetBestMember()
            {
                if (!(_members is List<IMemberInfo> members))
                    return (IMemberInfo)_members!;

                for (var i = 0; i < members.Count; i++)
                {
                    var memberValue = members[i];
                    var isInterface = memberValue.DeclaringType.IsInterface;
                    for (var j = 0; j < members.Count; j++)
                    {
                        if (i == j)
                            continue;
                        var pair = members[j];
                        if (isInterface && memberValue.DeclaringType.IsAssignableFrom(pair.DeclaringType))
                        {
                            members.RemoveAt(i);
                            i--;
                            break;
                        }

                        if (pair.DeclaringType.IsSubclassOf(memberValue.DeclaringType))
                        {
                            members.RemoveAt(i);
                            i--;
                            break;
                        }
                    }
                }

                return members[0];
            }

            #endregion
        }

        private sealed class SelectorDictionary : LightDictionary<IMemberInfo, MemberList>
        {
            #region Constructors

            public SelectorDictionary() : base(17)
            {
            }

            #endregion

            #region Methods

            protected override int GetHashCode(IMemberInfo key)
            {
                if (key is IMethodMemberInfo method)
                    return HashCode.Combine((int)key.MemberType, method.GetParameters().Count);
                return HashCode.Combine((int)key.MemberType);
            }

            protected override bool Equals(IMemberInfo x, IMemberInfo y)
            {
                if (x.MemberType != y.MemberType)
                    return false;

                if (x.MemberType != MemberType.Method)
                    return true;

                var xM = ((IMethodMemberInfo)x).GetParameters();
                var yM = ((IMethodMemberInfo)y).GetParameters();
                if (xM.Count != yM.Count)
                    return false;

                for (var i = 0; i < xM.Count; i++)
                {
                    if (xM[i].ParameterType != yM[i].ParameterType)
                        return false;
                }

                return true;
            }

            #endregion
        }

        #endregion
    }
}