// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Pihrtsoft.Markdown.Linq;
using Xunit;
using static Pihrtsoft.Markdown.Tests.TestHelpers;

#pragma warning disable CS1718

namespace Pihrtsoft.Markdown.Tests
{
    public class HorizontalRuleTests
    {
        [Fact]
        public void HorizontalRule_Equals()
        {
            MHorizontalRule horizontalRule = CreateHorizontalRule();

            Assert.True(horizontalRule.Equals((object)horizontalRule));
        }

        [Fact]
        public void HorizontalRule_GetHashCode_Equal()
        {
            MHorizontalRule horizontalRule = CreateHorizontalRule();

            Assert.Equal(horizontalRule.GetHashCode(), horizontalRule.GetHashCode());
        }

        [Fact]
        public void HorizontalRule_OperatorEquals()
        {
            MHorizontalRule horizontalRule = CreateHorizontalRule();
            MHorizontalRule horizontalRule2 = horizontalRule;

            Assert.True(horizontalRule == horizontalRule2);
        }

        [Fact]
        public void HorizontalRule_Constructor_AssignStyle()
        {
            HorizontalRuleStyle style = HorizontalRuleStyle();

            var horizontalRule = new MHorizontalRule(style, HorizontalRuleCount(), HorizontalRuleSpace());

            Assert.Equal(style, horizontalRule.Style);
        }

        [Fact]
        public void HorizontalRule_Constructor_AssignCount()
        {
            int count = HorizontalRuleCount();

            var horizontalRule = new MHorizontalRule(HorizontalRuleStyle(), count, HorizontalRuleSpace());

            Assert.Equal(count, horizontalRule.Count);
        }

        [Fact]
        public void HorizontalRule_Constructor_AssignSpace()
        {
            string space = HorizontalRuleSpace();

            var horizontalRule = new MHorizontalRule(HorizontalRuleStyle(), HorizontalRuleCount(), space);

            Assert.Equal(space, horizontalRule.Separator);
        }
    }
}
