﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace dnSpy.Contracts.Decompiler {
	/// <summary>
	/// Binary span
	/// </summary>
	public struct BinSpan : IEquatable<BinSpan> {
		readonly uint start, end;

		/// <summary>
		/// Start offset
		/// </summary>
		public uint Start => start;

		/// <summary>
		/// End offset, exclusive
		/// </summary>
		public uint End => end;

		/// <summary>
		/// Length (<see cref="End"/> - <see cref="Start"/>)
		/// </summary>
		public uint Length => end - start;

		/// <summary>
		/// true if it's empty (<see cref="Length"/> is 0)
		/// </summary>
		public bool IsEmpty => end == start;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="start">Start offset</param>
		/// <param name="length">Length</param>
		public BinSpan(uint start, uint length) {
			this.start = start;
			this.end = start + length;
		}

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="start">Start offset</param>
		/// <param name="end">End offset</param>
		/// <returns></returns>
		public static BinSpan FromBounds(uint start, uint end) {
			if (end < start)
				throw new ArgumentOutOfRangeException(nameof(end));
			return new BinSpan(start, end - start);
		}

		/// <summary>
		/// Sorts and compacts <paramref name="input"/>
		/// </summary>
		/// <param name="input">Input values</param>
		/// <returns></returns>
		public static List<BinSpan> OrderAndCompact(IEnumerable<BinSpan> input) => OrderAndCompactList(input.ToList());

		/// <summary>
		/// Sorts and compacts <paramref name="input"/>
		/// </summary>
		/// <param name="input">Input list. It can be sorted, and it can also be returned to the caller.</param>
		/// <returns></returns>
		public static List<BinSpan> OrderAndCompactList(List<BinSpan> input) {// Don't rename to OrderAndCompact() since some pass in a list that shouldn't be modified
			if (input.Count <= 1)
				return input;

			input.Sort(BinSpanComparer.Instance);
			var res = new List<BinSpan>();
			var curr = input[0];
			res.Add(curr);
			for (int i = 1; i < input.Count; i++) {
				var next = input[i];
				if (curr.End == next.Start)
					res[res.Count - 1] = curr = new BinSpan(curr.Start, next.End - curr.Start);
				else if (next.Start > curr.End) {
					res.Add(next);
					curr = next;
				}
				else if (next.End > curr.End)
					res[res.Count - 1] = curr = new BinSpan(curr.Start, next.End - curr.Start);
			}

			return res;
		}

		sealed class BinSpanComparer : IComparer<BinSpan> {
			public static readonly IComparer<BinSpan> Instance = new BinSpanComparer();
			public int Compare(BinSpan x, BinSpan y) {
				int c = unchecked((int)x.Start - (int)y.Start);
				if (c != 0)
					return c;
				return unchecked((int)y.End - (int)x.End);
			}
		}

		/// <summary>
		/// operator ==()
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(BinSpan left, BinSpan right) => left.Equals(right);

		/// <summary>
		/// operator !=()
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(BinSpan left, BinSpan right) => !left.Equals(right);

		/// <summary>
		/// Equals()
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(BinSpan other) => start == other.start && end == other.end;

		/// <summary>
		/// Equals()
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj) => obj is BinSpan && Equals((BinSpan)obj);

		/// <summary>
		/// GetHashCode()
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode() => (int)(start ^ ((end << 16) | (end >> 16)));

		/// <summary>
		/// ToString()
		/// </summary>
		/// <returns></returns>
		public override string ToString() => "[" + start.ToString("X4") + "," + end.ToString("X4") + ")";
	}
}
