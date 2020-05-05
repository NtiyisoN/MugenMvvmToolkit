﻿using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Binding.Observers.MemberPaths;
using MugenMvvm.Binding.Observers.PathObservers;
using MugenMvvm.Binding.Parsing.Expressions.Binding;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Observers;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing
{
    public class BindingInstanceMemberExpressionNodeTest : BindingMemberExpressionNodeBaseTest
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var exp = new BindingInstanceMemberExpressionNode(this, Path);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.BindingMember);
            exp.Instance.ShouldEqual(this);
            exp.Path.ShouldEqual(Path);
            exp.Index.ShouldEqual(-1);
        }

        [Fact]
        public void GetTargetSourceShouldReturnInstance()
        {
            var path = new SingleMemberPath(Path);
            var observerProvider = new ObserverProvider();
            var component = new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, type, arg3) =>
                {
                    o.ShouldEqual(Path);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            };
            observerProvider.AddComponent(component);

            var exp = new BindingInstanceMemberExpressionNode(this, Path, observerProvider)
            {
                MemberFlags = MemberFlags.All
            };

            var target = exp.GetTarget("", "", DefaultMetadata, out var p, out var flags);
            target.ShouldEqual(this);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            target = exp.GetSource("", "", DefaultMetadata, out p, out flags);
            target.ShouldEqual(this);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);
        }

        [Fact]
        public void GetBindingTargetSourceShouldReturnInstanceObserver()
        {
            var path = new SingleMemberPath(Path);
            var observer = EmptyPathObserver.Empty;
            var observerProvider = new ObserverProvider();

            var exp = new BindingInstanceMemberExpressionNode(this, Path, observerProvider)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethod,
                ObservableMethodName = "M"
            };

            observerProvider.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, type, arg3) =>
                {
                    o.ShouldEqual(Path);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            });
            observerProvider.AddComponent(new TestMemberPathObserverProviderComponent
            {
                TryGetMemberPathObserver = (t, req, arg3, arg4) =>
                {
                    t.ShouldEqual(this);
                    var request = (MemberPathObserverRequest) req;
                    request.Path.ShouldEqual(path);
                    request.MemberFlags.ShouldEqual(exp.MemberFlags);
                    request.ObservableMethodName.ShouldEqual(exp.ObservableMethodName);
                    request.HasStablePath.ShouldBeTrue();
                    request.Optional.ShouldBeTrue();
                    request.Observable.ShouldBeTrue();
                    arg4.ShouldEqual(DefaultMetadata);
                    return observer;
                }
            });

            exp.GetBindingTarget("", "", DefaultMetadata).ShouldEqual(observer);
            exp.GetBindingSource("", "", DefaultMetadata).ShouldEqual(observer);
        }

        protected override BindingMemberExpressionNodeBase GetExpression()
        {
            return new BindingInstanceMemberExpressionNode(this, Path);
        }

        #endregion
    }
}