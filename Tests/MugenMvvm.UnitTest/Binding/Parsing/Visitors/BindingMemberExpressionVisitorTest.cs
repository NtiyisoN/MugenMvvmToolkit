﻿using System;
using MugenMvvm.Binding;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Binding.Observers.MemberPaths;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions.Binding;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Binding.Resources;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using MugenMvvm.UnitTest.Binding.Resources.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Visitors
{
    public class BindingMemberExpressionVisitorTest : UnitTestBase
    {
        #region Fields

        private const string MemberName = "Test1";
        private const string MethodName = "Test2";
        private const string MemberName2 = "Test3";
        private const string TypeName = "T";
        private const string ResourceName = "R";

        #endregion

        #region Methods

        [Fact]
        public void VisitShouldHandleMethodCall1()
        {
            var expression = new MethodCallExpressionNode(null, MethodName, Default.EmptyArray<IExpressionNode>());
            var visitor = new BindingMemberExpressionVisitor { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, $"{expression.Method}()")
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });

            visitor.IgnoreMethodMembers = true;
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, string.Empty)
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            }));
        }

        [Fact]
        public void VisitShouldHandleMethodCall2()
        {
            var expression = new MethodCallExpressionNode(null, MethodName, new[] { ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2) });
            var visitor = new BindingMemberExpressionVisitor { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, $"{expression.Method}(1,2)")
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });

            visitor.IgnoreMethodMembers = true;
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, string.Empty)
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            }));
        }

        [Fact]
        public void VisitShouldHandleMethodCall3()
        {
            var expression = new MethodCallExpressionNode(new MemberExpressionNode(null, MemberName), MethodName, new[] { ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2) });
            var visitor = new BindingMemberExpressionVisitor { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, $"{MemberName}.{expression.Method}(1,2)")
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });

            visitor.IgnoreMethodMembers = true;
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, MemberName)
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = MethodName
            }));
        }

        [Fact]
        public void VisitShouldHandleMethodCall4()
        {
            var expression = new MethodCallExpressionNode(new IndexExpressionNode(new MemberExpressionNode(null, MemberName), new[] { ConstantExpressionNode.Get(1) }), MethodName,
                new[]
                {
                    ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2)
                });
            var visitor = new BindingMemberExpressionVisitor { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, $"{MemberName}[1].{expression.Method}(1,2)")
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });

            visitor.IgnoreMethodMembers = true;
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, $"{MemberName}[1]")
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = MethodName
            }));
        }

        [Fact]
        public void VisitShouldHandleIndexer1()
        {
            var expression = new IndexExpressionNode(null, new[] { ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2) });
            var visitor = new BindingMemberExpressionVisitor { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, "[1,2]")
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });

            visitor.IgnoreIndexMembers = true;
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, string.Empty)
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            }));
        }

        [Fact]
        public void VisitShouldHandleIndexer2()
        {
            var expression = new IndexExpressionNode(new MemberExpressionNode(null, MemberName), new[] { ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2) });
            var visitor = new BindingMemberExpressionVisitor { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, $"{MemberName}[1,2]")
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });

            visitor.IgnoreIndexMembers = true;
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, MemberName)
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = MethodName
            }));
        }

        [Fact]
        public void VisitShouldHandleMember1()
        {
            var expression = new MemberExpressionNode(null, MemberName);
            var visitor = new BindingMemberExpressionVisitor { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, MemberName)
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });
        }

        [Fact]
        public void VisitShouldHandleMember2()
        {
            var expression = new MemberExpressionNode(new MemberExpressionNode(null, MemberName2), MemberName);
            var visitor = new BindingMemberExpressionVisitor { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, $"{MemberName2}.{MemberName}")
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });
        }

        [Theory]
        [InlineData(MacrosConstant.Target)]
        [InlineData(MacrosConstant.Self)]
        [InlineData(MacrosConstant.This)]
        public void VisitShouldHandleTargetMacros(string macros)
        {
            var visitor = new BindingMemberExpressionVisitor { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };

            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, macros));
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.TargetOnly, string.Empty)
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });

            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, macros)), MemberName);
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.TargetOnly, MemberName)
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });
        }

        [Fact]
        public void VisitShouldHandleSourceMacros()
        {
            var visitor = new BindingMemberExpressionVisitor { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };

            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, MacrosConstant.Source));
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.SourceOnly, string.Empty)
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });

            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, MacrosConstant.Source)), MemberName);
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.SourceOnly, MemberName)
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });
        }

        [Fact]
        public void VisitShouldHandleSourceContext()
        {
            var visitor = new BindingMemberExpressionVisitor { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };

            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, MacrosConstant.Context));
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.TargetOnly, BindableMembers.Object.DataContext)
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });

            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, MacrosConstant.Context)), MemberName);
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.TargetOnly, $"{BindableMembers.Object.DataContext}.{MemberName}")
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(false),
                ObservableMethodName = null
            });
        }

        [Fact]
        public void VisitShouldHandleType()
        {
            var returnType = typeof(string);
            var invokeCount = 0;
            var resolver = new ResourceResolver();
            resolver.AddComponent(new TestTypeResolverComponent
            {
                TryGetType = (s, o, arg3, arg4) =>
                {
                    s.ShouldEqual(TypeName);
                    arg4.ShouldEqual(DefaultMetadata);
                    ++invokeCount;
                    return returnType;
                }
            });

            var visitor = new BindingMemberExpressionVisitor(null, resolver) { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };
            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TypeName));
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingInstanceMemberExpressionNode(returnType, string.Empty)
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(true),
                ObservableMethodName = null
            });
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TypeName)), MemberName);
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingInstanceMemberExpressionNode(returnType, MemberName)
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(true),
                ObservableMethodName = null
            });
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void VisitShouldHandleTypeStatic()
        {
            var returnType = typeof(string);
            var member1Result = 1;
            var member2Result = "";
            var member1 = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldBeNull();
                    context.ShouldEqual(DefaultMetadata);
                    return member1Result;
                }
            };
            var member2 = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(member1Result);
                    context.ShouldEqual(DefaultMetadata);
                    return member2Result;
                }
            };

            var invokeCount = 0;
            var resolver = new ResourceResolver();
            resolver.AddComponent(new TestTypeResolverComponent
            {
                TryGetType = (s, o, arg3, arg4) =>
                {
                    s.ShouldEqual(TypeName);
                    arg4.ShouldEqual(DefaultMetadata);
                    ++invokeCount;
                    return returnType;
                }
            });
            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (o, type, arg3) =>
                {
                    var request = (MemberManagerRequest)o;
                    request.MemberTypes.ShouldEqual(MemberType.Accessor);
                    if (request.Name == MemberName)
                    {
                        request.Type.ShouldEqual(returnType);
                        request.Flags.ShouldEqual(MemberFlags.All.SetInstanceOrStaticFlags(true));
                        return member1;
                    }

                    if (request.Name == MemberName2)
                    {
                        request.Type.ShouldEqual(member1Result.GetType());
                        request.Flags.ShouldEqual(MemberFlags.All.SetInstanceOrStaticFlags(false));
                        return member2;
                    }

                    throw new NotSupportedException();
                }
            });

            var observerProvider = new ObserverProvider();
            observerProvider.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, type, arg3) =>
                {
                    var path = (string)o!;
                    if (path == "")
                        return EmptyMemberPath.Instance;
                    if (path == MemberName)
                        return new SingleMemberPath(MemberName);
                    if (path == $"{MemberName}.{MemberName2}")
                        return new MultiMemberPath(path);
                    throw new NotSupportedException();
                }
            });

            var visitor = new BindingMemberExpressionVisitor(observerProvider, resolver, memberManager) { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };
            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, TypeName));
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(ConstantExpressionNode.Get(returnType));
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, TypeName)), MemberName);
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(ConstantExpressionNode.Get(member1Result));
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            expression = new MemberExpressionNode(new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, TypeName)), MemberName), MemberName2);
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(ConstantExpressionNode.Get(member2Result));
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void VisitShouldHandleResource()
        {
            var visitor = new BindingMemberExpressionVisitor() { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };
            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, ResourceName));
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingResourceMemberExpressionNode(ResourceName, nameof(IResourceValue.Value))
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(true),
                ObservableMethodName = null
            });

            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, ResourceName)), MemberName);
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(new BindingResourceMemberExpressionNode(ResourceName, $"{nameof(IResourceValue.Value)}.{MemberName}")
            {
                Flags = visitor.Flags,
                MemberFlags = visitor.MemberFlags.SetInstanceOrStaticFlags(true),
                ObservableMethodName = null
            });
        }

        [Fact]
        public void VisitShouldHandleResourceStatic()
        {
            var resource = new TestResourceValue { Value = 1 };
            var invokeCount = 0;
            var resolver = new ResourceResolver();
            resolver.AddComponent(new TestResourceResolverComponent
            {
                TryGetResourceValue = (s, o, arg3, arg4) =>
                {
                    s.ShouldEqual(ResourceName);
                    arg4.ShouldEqual(DefaultMetadata);
                    ++invokeCount;
                    return resource;
                }
            });
            var memberResult = "w";
            var member = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(resource.Value);
                    context.ShouldEqual(DefaultMetadata);
                    return memberResult;
                }
            };

            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (o, type, arg3) =>
                {
                    var request = (MemberManagerRequest)o;
                    request.MemberTypes.ShouldEqual(MemberType.Accessor);
                    if (request.Name == MemberName)
                    {
                        request.Type.ShouldEqual(resource.Value.GetType());
                        request.Flags.ShouldEqual(MemberFlags.All.SetInstanceOrStaticFlags(false));
                        return member;
                    }

                    throw new NotSupportedException();
                }
            });

            var observerProvider = new ObserverProvider();
            observerProvider.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, type, arg3) =>
                {
                    var path = (string)o!;
                    if (path == "")
                        return EmptyMemberPath.Instance;
                    if (path == MemberName)
                        return new MultiMemberPath(path);
                    throw new NotSupportedException();
                }
            });

            var visitor = new BindingMemberExpressionVisitor(observerProvider, resolver, memberManager) { MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable };
            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, ResourceName));
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(ConstantExpressionNode.Get(resource.Value));
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, ResourceName)), MemberName);
            visitor.Visit(expression, DefaultMetadata).ShouldEqual(ConstantExpressionNode.Get(memberResult));
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void VisitShouldCacheMethodCallMember()
        {
            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new MethodCallExpressionNode(null, MethodName, Default.EmptyArray<IExpressionNode>()),
                new MethodCallExpressionNode(null, MethodName, Default.EmptyArray<IExpressionNode>()));
            var visitor = new BindingMemberExpressionVisitor();
            var expressionNode = (BinaryExpressionNode)visitor.Visit(expression, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Fact]
        public void VisitShouldCacheIndexerMember()
        {
            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new IndexExpressionNode(null, new[] { ConstantExpressionNode.Null }), new IndexExpressionNode(null, new[] { ConstantExpressionNode.Null }));
            var visitor = new BindingMemberExpressionVisitor();
            var expressionNode = (BinaryExpressionNode)visitor.Visit(expression, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Fact]
        public void VisitShouldCacheMember()
        {
            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new MemberExpressionNode(new MemberExpressionNode(null, MemberName2), MemberName), new MemberExpressionNode(new MemberExpressionNode(null, MemberName2), MemberName));
            var visitor = new BindingMemberExpressionVisitor();
            var expressionNode = (BinaryExpressionNode)visitor.Visit(expression, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Theory]
        [InlineData(MacrosConstant.Target)]
        [InlineData(MacrosConstant.Self)]
        [InlineData(MacrosConstant.This)]
        [InlineData(MacrosConstant.Source)]
        [InlineData(MacrosConstant.Context)]
        public void VisitShouldCacheMacrosExpression(string macros)
        {
            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, macros)),
                new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, macros)));
            var visitor = new BindingMemberExpressionVisitor();
            var expressionNode = (BinaryExpressionNode)visitor.Visit(expression, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Fact]
        public void VisitShouldCacheTypeMember()
        {
            var resolver = new ResourceResolver();
            resolver.AddComponent(new TestTypeResolverComponent
            {
                TryGetType = (s, o, arg3, arg4) => typeof(object)
            });

            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TypeName)),
                new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TypeName)));
            var visitor = new BindingMemberExpressionVisitor(null, resolver);
            var expressionNode = (BinaryExpressionNode)visitor.Visit(expression, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Fact]
        public void VisitShouldCacheResourceMember()
        {
            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, ResourceName)),
                new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, ResourceName)));
            var visitor = new BindingMemberExpressionVisitor();
            var expressionNode = (BinaryExpressionNode)visitor.Visit(expression, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Fact]
        public void VisitShouldCacheTypeMemberStatic()
        {
            var resolver = new ResourceResolver();
            resolver.AddComponent(new TestTypeResolverComponent
            {
                TryGetType = (s, o, arg3, arg4) => typeof(string)
            });
            var observerProvider = new ObserverProvider();
            observerProvider.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, type, arg3) => EmptyMemberPath.Instance
            });

            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, TypeName)),
                new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, TypeName)));
            var visitor = new BindingMemberExpressionVisitor(observerProvider, resolver);
            var expressionNode = (BinaryExpressionNode)visitor.Visit(expression, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Fact]
        public void VisitShouldCacheResourceMemberStatic()
        {
            var resolver = new ResourceResolver();
            resolver.AddComponent(new TestResourceResolverComponent
            {
                TryGetResourceValue = (s, o, arg3, arg4) => new TestResourceValue()
            });
            var observerProvider = new ObserverProvider();
            observerProvider.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, type, arg3) => EmptyMemberPath.Instance
            });

            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, ResourceName)),
                new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, ResourceName)));
            var visitor = new BindingMemberExpressionVisitor(observerProvider, resolver);
            var expressionNode = (BinaryExpressionNode)visitor.Visit(expression, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        #endregion
    }
}