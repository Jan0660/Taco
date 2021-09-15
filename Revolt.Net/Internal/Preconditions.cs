using System;

namespace Revolt.Internal
{
    // borrowed from Discord.Net :)
    internal static class Preconditions
    {
        //Objects
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> must not be <see langword="null"/>.</exception>
        public static void NotNull<T>(T obj, string name, string msg = null) where T : class { if (obj == null) throw CreateNotNullException(name, msg); }
        private static ArgumentNullException CreateNotNullException(string name, string msg)
        {
            if (msg == null) return new ArgumentNullException(paramName: name);
            else return new ArgumentNullException(paramName: name, message: msg);
        }
        private static ArgumentException CreateLessThanException<T>(string name, string msg, T value)
            => new ArgumentException(message: msg ?? $"Value must be less than {value}.", paramName: name);
    }
}