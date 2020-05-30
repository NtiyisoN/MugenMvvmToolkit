﻿using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members.Components
{
    public class CacheMemberManagerDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldUseCache()
        {
            var invokeCount = 0;
            var type = typeof(string);
            var memberType = MemberType.Accessor;
            var memberFlags = MemberFlags.All;
            var request = "test";
            var result = new TestMemberAccessorInfo();
            var providerComponent = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, tt, meta) =>
                {
                    ++invokeCount;
                    t.ShouldEqual(type);
                    m.ShouldEqual(memberType);
                    f.ShouldEqual(memberFlags);
                    r.ShouldEqual(request);
                    tt.ShouldEqual(request.GetType());
                    meta.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };
            var cacheComponent = new CacheMemberManagerDecorator();
            ((IComponentCollectionDecorator<IMemberManagerComponent>) cacheComponent).Decorate(new List<IMemberManagerComponent> {cacheComponent, providerComponent}, DefaultMetadata);

            cacheComponent.TryGetMembers(type, memberType, memberFlags, request, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(type, memberType, memberFlags, request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void InvalidateShouldClearCache()
        {
            var invokeCount = 0;
            var type1 = typeof(string);
            var type2 = typeof(object);
            var memberType = MemberType.Accessor;
            var memberFlags = MemberFlags.All;
            var request1 = "test1";
            var request2 = "test2";
            var result = new TestMemberAccessorInfo();
            var providerComponent = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, tt, meta) =>
                {
                    ++invokeCount;
                    return result;
                }
            };
            var cacheComponent = new CacheMemberManagerDecorator();
            ((IComponentCollectionDecorator<IMemberManagerComponent>) cacheComponent).Decorate(new List<IMemberManagerComponent> {cacheComponent, providerComponent}, DefaultMetadata);

            cacheComponent.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            cacheComponent.Invalidate<object?>(null, DefaultMetadata);
            invokeCount = 0;
            cacheComponent.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            cacheComponent.Invalidate<object?>(type1, DefaultMetadata);
            invokeCount = 0;
            cacheComponent.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void AttachDetachShouldClearCache()
        {
            var invokeCount = 0;
            var type1 = typeof(string);
            var type2 = typeof(object);
            var memberType = MemberType.Accessor;
            var memberFlags = MemberFlags.All;
            var request1 = "test1";
            var request2 = "test2";
            var result = new TestMemberAccessorInfo();
            var providerComponent = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, tt, meta) =>
                {
                    ++invokeCount;
                    return result;
                }
            };
            var cacheComponent = new CacheMemberManagerDecorator();
            var memberManager = new MemberManager();
            memberManager.AddComponent(providerComponent);
            memberManager.AddComponent(cacheComponent);

            memberManager.GetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            memberManager.GetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            memberManager.GetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            memberManager.GetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            memberManager.RemoveComponent(cacheComponent);
            invokeCount = 0;
            cacheComponent.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            cacheComponent.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            cacheComponent.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            cacheComponent.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            invokeCount.ShouldEqual(0);
        }

        #endregion
    }
}