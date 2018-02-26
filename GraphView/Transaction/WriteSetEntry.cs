﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphView.Transaction
{
    internal class WriteSetEntry
    {
        internal object Key { get; }
        internal long BeginTimestamp { get; }
        internal bool IsOld { get; }

        public WriteSetEntry(object key, long beginTimestamp, bool isOld)
        {
            this.Key = key;
            this.BeginTimestamp = beginTimestamp;
            this.IsOld = isOld;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + this.Key.GetHashCode();
            hash = hash * 23 + this.BeginTimestamp.GetHashCode();
            hash = hash * 23 + this.IsOld.GetHashCode();

            return hash;
        }

        public override bool Equals(object obj)
        {
            WriteSetEntry entry = obj as WriteSetEntry;
            if (entry == null)
            {
                return false;
            }

            return this.Key == entry.Key && this.BeginTimestamp == entry.BeginTimestamp
                                         && this.IsOld == entry.IsOld;
        }
    }
}