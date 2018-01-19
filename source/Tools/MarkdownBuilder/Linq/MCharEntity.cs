﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;

namespace Pihrtsoft.Markdown.Linq
{
    [DebuggerDisplay("{Kind} {Value} {ValueAsHexadecimalString}")]
    public class MCharEntity : MElement
    {
        private char _value;

        public MCharEntity(char value)
        {
            Value = value;
        }

        public MCharEntity(MCharEntity other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            _value = other.Value;
        }

        public char Value
        {
            get { return _value; }
            set
            {
                Error.ThrowOnInvalidCharEntity(value);

                _value = value;
            }
        }

        internal string ValueAsHexadecimalString => NumberAsString(CharEntityFormat.Hexadecimal);

        public override MarkdownKind Kind => MarkdownKind.CharEntity;

        public override MarkdownWriter WriteTo(MarkdownWriter writer)
        {
            return writer.WriteCharEntity(Value);
        }

        internal override MElement Clone()
        {
            return new MCharEntity(this);
        }

        internal string NumberAsString(CharEntityFormat format)
        {
            switch (format)
            {
                case CharEntityFormat.Hexadecimal:
                    return ((int)Value).ToString("x", CultureInfo.InvariantCulture);
                case CharEntityFormat.Decimal:
                    return ((int)Value).ToString(CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentException(ErrorMessages.UnknownEnumValue(format), nameof(format));
            }
        }
    }
}
