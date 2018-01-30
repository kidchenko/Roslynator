// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Pihrtsoft.Markdown.Linq;
using Xunit;
using static Pihrtsoft.Markdown.Tests.TestHelpers;

#pragma warning disable CS1718

namespace Pihrtsoft.Markdown.Tests
{
    public class BlockQuotekTests
    {
        [Fact]
        public void BlockQuote_Equals()
        {
            MBlockQuote blockQuote = CreateBlockQuote();

            Assert.True(blockQuote.Equals((object)blockQuote));
        }

        [Fact]
        public void BlockQuote_GetHashCode_Equal()
        {
            MBlockQuote blockQuote = CreateBlockQuote();

            Assert.Equal(blockQuote.GetHashCode(), blockQuote.GetHashCode());
        }

        [Fact]
        public void BlockQuote_OperatorEquals()
        {
            MBlockQuote blockQuote = CreateBlockQuote();
            MBlockQuote blockQuote2 = blockQuote;

            Assert.True(blockQuote == blockQuote2);
        }
    }
}
