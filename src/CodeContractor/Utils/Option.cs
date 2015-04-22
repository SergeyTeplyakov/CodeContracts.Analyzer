using System;
using Microsoft.CodeAnalysis;

namespace CodeContractor.Utils
{
    /// <summary>
    /// Represents a value type that can be assigned null.
    /// </summary>
    public struct Option<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance to the specified value.
        /// </summary>
        /// <param name="value"></param>
        public Option(T value)
        {
            HasValue = value != default(T);
            Value = value;
        }

        public Option<U> Bind<U>(Func<T, U> func) where U : class
        {
            if (!HasValue)
                return null;

            return func(Value);
        }

        public Option<U> Bind<U>(Func<T, Option<U>> func) where U : class
        {
            if (!HasValue)
                return null;

            return func(Value);
        }

        /// <summary>
        /// Gets a value indicating whether the current object has a value.
        /// </summary>
        /// <returns></returns>
        public bool HasValue { get; }

        /// <summary>
        /// Gets the value of the current object.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Creates a new object initialized to a specified value. 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Option<T>(T value)
        {
            return new Option<T>(value);
        }
    }
}