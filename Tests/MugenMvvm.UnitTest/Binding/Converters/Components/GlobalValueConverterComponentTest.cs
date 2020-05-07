﻿using System;
using System.Globalization;
using MugenMvvm.Binding.Converters.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Converters.Components
{
    public class GlobalValueConverterComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryConvertShouldHandleNull()
        {
            var component = new GlobalValueConverterComponent();
            object? value = null;
            component.TryConvert(ref value, typeof(object), null, null).ShouldEqual(true);
            value.ShouldBeNull();

            value = null;
            component.TryConvert(ref value, typeof(bool), null, null).ShouldEqual(true);
            value.ShouldEqual(false);
        }

        [Fact]
        public void TryConvertShouldHandleInstanceOfType()
        {
            var component = new GlobalValueConverterComponent();
            object? value = this;
            component.TryConvert(ref value, typeof(object), null, null).ShouldEqual(true);
            value.ShouldEqual(this);
        }

        [Fact]
        public void TryConvertShouldHandleString()
        {
            var component = new GlobalValueConverterComponent();
            object? value = this;
            component.TryConvert(ref value, typeof(string), null, null).ShouldEqual(true);
            value.ShouldEqual(ToString());

            const float f = 1.1f;
            value = f;
            component.TryConvert(ref value, typeof(string), null, null).ShouldEqual(true);
            value.ShouldEqual(f.ToString());

            component.FormatProvider = () => new NumberFormatInfo {CurrencyDecimalSeparator = ";", NumberDecimalSeparator = ";"};
            value = f;
            component.TryConvert(ref value, typeof(string), null, null).ShouldEqual(true);
            value.ShouldEqual(f.ToString(component.FormatProvider()));
        }

        [Fact]
        public void TryConvertShouldHandleConvertible()
        {
            var component = new GlobalValueConverterComponent();
            object? value = int.MaxValue.ToString();

            component.TryConvert(ref value, typeof(int), null, null).ShouldEqual(true);
            value.ShouldEqual(int.MaxValue);

            const float f = 1.1f;
            component.FormatProvider = () => new NumberFormatInfo {CurrencyDecimalSeparator = ";", NumberDecimalSeparator = ";"};
            value = f.ToString(component.FormatProvider());
            component.TryConvert(ref value, typeof(float), null, null).ShouldEqual(true);
            value.ShouldEqual(f);
        }

        [Fact]
        public void TryConvertShouldHandleEnum()
        {
            var component = new GlobalValueConverterComponent();
            object? value = StringComparison.CurrentCulture.ToString();

            component.TryConvert(ref value, typeof(StringComparison), null, null).ShouldEqual(true);
            value.ShouldEqual(StringComparison.CurrentCulture);
        }

        [Fact]
        public void TryConvertShouldIgnoreNotConvertibleValue()
        {
            var component = new GlobalValueConverterComponent();
            var v = new object();
            object? value = v;

            component.TryConvert(ref value, GetType(), null, null).ShouldBeFalse();
            value.ShouldEqual(v);
        }

        #endregion
    }
}