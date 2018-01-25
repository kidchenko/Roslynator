// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Pihrtsoft.Markdown.Linq;
using Xunit;
using static Pihrtsoft.Markdown.Tests.TestHelpers;

#pragma warning disable CS1718

namespace Pihrtsoft.Markdown.Tests
{
    public class TableColumnTests
    {
        [Fact]
        public void TableColumn_Equals()
        {
            MTableColumn column = CreateTableColumn();

            Assert.True(column.Equals((object)column));
        }

        [Fact]
        public void TableColumn_GetHashCode_Equal()
        {
            MTableColumn column = CreateTableColumn();

            Assert.Equal(column.GetHashCode(), column.GetHashCode());
        }

        [Fact]
        public void TableColumn_OperatorEquals()
        {
            MTableColumn column = CreateTableColumn();
            MTableColumn column2 = column;

            Assert.True(column == column2);
        }

        [Fact]
        public void TableColumn_Constructor_AssignAlignment()
        {
            ColumnAlignment alignment = TableColumnAlignment();
            var column = new MTableColumn(alignment: alignment, content: StringValue());

            Assert.Equal(alignment, column.Alignment);
        }
    }
}
