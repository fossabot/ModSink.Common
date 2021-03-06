﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ModSink.Core.Models.Repo
{
    [Serializable]
    [DebuggerDisplay("{Value}")]
    public struct HashValue : IEquatable<HashValue>
    {
        public HashValue(byte[] value) => this.Value = value;

        public HashValue(IEnumerable<byte> value) => this.Value = value.ToArray();

        public byte[] Value { get; }

        public bool Equals(HashValue other) => this.Value.SequenceEqual(other.Value);

        public override int GetHashCode() => this.Value.GetHashCode();

        public override string ToString() => BitConverter.ToString(this.Value);
    }
}