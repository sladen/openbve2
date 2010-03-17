using System;

namespace Plugin {
	internal static partial class Parser {
		
		// --- functions ---
		
		/// <summary>Sorts a list of expressions.</summary>
		/// <param name="expressions">The list of expressions.</param>
		/// <param name="index">The index in the list at which to start sorting.</param>
		/// <param name="count">The number of items in the list which to sort.</param>
		private static void SortExpressions(Expression[] expressions, int index, int count) {
			Expression[] dummy = new Expression[expressions.Length];
			SortExpressions(expressions, dummy, index, count);
		}
		
		/// <summary>Sorts a list of expressions.</summary>
		/// <param name="expressions">The list of expressions.</param>
		/// <param name="dummy">A dummy list of the same length as the list of expressions.</param>
		/// <param name="index">The index in the list at which to start sorting.</param>
		/// <param name="count">The number of items in the list which to sort.</param>
		/// <remarks>This method implements a stable merge sort that switches to an in-place stable insertion sort with sufficiently few elements.</remarks>
		private static void SortExpressions(Expression[] expressions, Expression[] dummy, int index, int count) {
			if (count < 25) {
				/*
				 * Use an insertion sort for less than 25 elements
				 * */
				for (int i = 1; i < count; i++) {
					int j;
					for (j = i - 1; j >= 0; j--) {
						if (expressions[index + i].Position >= expressions[index + j].Position) {
							break;
						}
					}
					Expression temp = expressions[index + i];
					for (int k = i; k > j + 1; k--) {
						expressions[index + k] = expressions[index + k - 1];
					}
					expressions[index + j + 1] = temp;
				}
			} else {
				/*
				 * For more elements, split the list in half,
				 * recursively sort the two lists, then merge
				 * them back together.
				 * */
				int halfCount = count / 2;
				SortExpressions(expressions, dummy, index, halfCount);
				SortExpressions(expressions, dummy, index + halfCount, count - halfCount);
				int left = index;
				int right = index + halfCount;
				for (int i = index; i < index + count; i++) {
					if (left == index + halfCount) {
						while (right != index + count) {
							dummy[i] = expressions[right];
							right++;
							i++;
						}
						break;
					} else if (right == index + count) {
						while (left != index + halfCount) {
							dummy[i] = expressions[left];
							left++;
							i++;
						}
						break;
					}
					if (expressions[left].Position <= expressions[right].Position) {
						dummy[i] = expressions[left];
						left++;
					} else {
						dummy[i] = expressions[right];
						right++;
					}
				}
				for (int i = index; i < index + count; i++) {
					expressions[i] = dummy[i];
				}
			}
		}
		
	}
}