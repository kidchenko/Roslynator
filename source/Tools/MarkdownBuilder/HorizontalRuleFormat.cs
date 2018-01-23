﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Pihrtsoft.Markdown
{
    [DebuggerDisplay("Text = {Text,nq} Count = {Count}{SeparatorDebuggerDisplay,nq}")]
    public struct HorizontalRuleFormat : IEquatable<HorizontalRuleFormat>
    {
        internal const string DefaultValue = "-";
        internal const int DefaultCount = 3;
        internal const string DefaultSeparator = " ";

        public HorizontalRuleFormat(string text, int count, string separator)
        {
            Error.ThrowOnInvalidHorizontalRuleText(text);
            Error.ThrowOnInvalidHorizontalRuleCount(count);
            Error.ThrowOnInvalidHorizontalRuleSeparator(separator);

            Text = text;
            Count = count;
            Separator = separator;
        }

        public static HorizontalRuleFormat Default { get; } = new HorizontalRuleFormat(DefaultValue, DefaultCount, DefaultSeparator);

        public string Text { get; }

        public int Count { get; }

        public string Separator { get; }

        private string SeparatorDebuggerDisplay
        {
            get { return (!string.IsNullOrEmpty(Separator)) ? $" {nameof(Separator)} = \"{Separator}\"" : ""; }
        }

        public bool IsValid
        {
            get
            {
                return IsValidText(Text)
                    && IsValidCount(Count)
                    && IsValidSeparator(Separator);
            }
        }

        internal static bool IsValidText(string text)
        {
            if (text == null)
                return false;

            if (text.Length != 1)
                return false;

            return text == "-" || text == "_" || text == "*";
        }

        internal static bool IsValidCount(int count)
        {
            return count >= 3;
        }

        internal static bool IsValidSeparator(string separator)
        {
            if (separator == null)
                return true;

            for (int i = 0; i < separator.Length; i++)
            {
                if (separator[i] != ' ')
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return (obj is HorizontalRuleFormat other)
                && Equals(other);
        }

        public bool Equals(HorizontalRuleFormat other)
        {
            return Text == other.Text
                   && Count == other.Count
                   && Separator == other.Separator;
        }

        public override int GetHashCode()
        {
            return Hash.Combine(Text, Hash.Combine(Count, Hash.Create(Separator)));
        }

        public static bool operator ==(HorizontalRuleFormat format1, HorizontalRuleFormat format2)
        {
            return format1.Equals(format2);
        }

        public static bool operator !=(HorizontalRuleFormat format1, HorizontalRuleFormat format2)
        {
            return !(format1 == format2);
        }
    }
}
