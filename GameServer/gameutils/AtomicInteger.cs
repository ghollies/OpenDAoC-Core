﻿
using System.Threading;

namespace DOL.GS
{
	internal class AtomicInteger
	{
		private int _value;
		public AtomicInteger(int value) { _value = value; }

		/// <summary>
		///     Add <paramref name="value" /> to this instance and return the resulting value.
		/// </summary>
		/// <param name="value">The amount to add.</param>
		/// <returns>The value of this instance + the amount added.</returns>
		public int Add(int value) { return Interlocked.Add(ref _value, value); }

		/// <summary>
		///     Replace the value of this instance, if the current value is equal to the <paramref name="expected" /> value.
		/// </summary>
		/// <param name="expected">Value this instance is expected to be equal with.</param>
		/// <param name="updated">Value to set this instance to, if the current value is equal to the expected value</param>
		/// <returns>True if the update was made, false otherwise.</returns>
		public bool CompareAndSwap(int expected, int updated) { return Interlocked.CompareExchange(ref _value, updated, expected) == expected; }

		/// <summary>
		///     Decrement this instance and return the value after the decrement.
		/// </summary>
		/// <returns>The value of the instance *after* the decrement.</returns>
		public int Decrement() { return Interlocked.Decrement(ref _value); }

		/// <summary>
		///     Decrement this instance with <paramref name="value" /> and return the value after the decrement.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		///     The value of the instance *after* the decrement.
		/// </returns>
		public int Decrement(int value) { return Add(-value); }

		/// <summary>
		///     Add <paramref name="value" /> to this instance and return the value this instance had before the add operation.
		/// </summary>
		/// <param name="value">The amount to add.</param>
		/// <returns>The value of this instance before the amount was added.</returns>
		public int GetAndAdd(int value) { return Add(value) - value; }

		/// <summary>
		///     Decrement this instance and return the value the instance had before the decrement.
		/// </summary>
		/// <returns>
		///     The value of the instance *before* the decrement.
		/// </returns>
		public int GetAndDecrement() { return Decrement() + 1; }

		/// <summary>
		///     Decrement this instance with <paramref name="value" /> and return the value the instance had before the decrement.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		///     The value of the instance *before* the decrement.
		/// </returns>
		public int GetAndDecrement(int value) { return Decrement(value) + value; }

		/// <summary>
		///     Increment this instance and return the value the instance had before the increment.
		/// </summary>
		/// <returns>
		///     The value of the instance *before* the increment.
		/// </returns>
		public int GetAndIncrement() { return Increment() - 1; }

		/// <summary>
		///     Increment this instance with <paramref name="value" /> and return the value the instance had before the increment.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		///     The value of the instance *before* the increment.
		/// </returns>
		public int GetAndIncrement(int value) { return Increment(value) - value; }

		/// <summary>
		///     Returns the current value of the instance and sets it to zero as an atomic operation.
		/// </summary>
		/// <returns>
		///     The current value of the instance.
		/// </returns>
		public int GetAndReset() { return GetAndSet(0); }

		/// <summary>
		///     Returns the current value of the instance and sets it to <paramref name="newValue" /> as an atomic operation.
		/// </summary>
		/// <param name="newValue">The new value.</param>
		/// <returns>
		///     The current value of the instance.
		/// </returns>
		public int GetAndSet(int newValue) { return Interlocked.Exchange(ref _value, newValue); }

		/// <summary>
		///     Returns the latest value of this instance written by any processor.
		/// </summary>
		/// <returns>
		///     The latest written value of this instance.
		/// </returns>
		public int GetValue() { return Volatile.Read(ref _value); }

		/// <summary>
		///     Increment this instance and return the value after the increment.
		/// </summary>
		/// <returns>The value of the instance *after* the increment.</returns>
		public int Increment() { return Interlocked.Increment(ref _value); }

		/// <summary>
		///     Increment this instance with <paramref name="value" /> and return the value after the increment.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		///     The value of the instance *after* the increment.
		/// </returns>
		public int Increment(int value) { return Add(value); }

		/// <summary>
		///     Returns the current value of the instance without using Volatile.Read fence and ordering.
		/// </summary>
		/// <returns>The current value of the instance in a non-volatile way (might not observe changes on other threads).</returns>
		public int NonVolatileGetValue() { return _value; }

		/// <summary>
		///     Set the value without using Volatile.Write fence and ordering.
		/// </summary>
		/// <param name="value">The new value for this instance.</param>
		public void NonVolatileSetValue(int value) { _value = value; }

		/// <summary>
		///     Write a new value to this instance. The value is immediately seen by all processors.
		/// </summary>
		/// <param name="value">The new value for this instance.</param>
		public void SetValue(int value) { Volatile.Write(ref _value, value); }

	}
}
