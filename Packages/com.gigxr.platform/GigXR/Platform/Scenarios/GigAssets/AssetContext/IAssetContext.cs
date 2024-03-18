using System;


namespace GIGXR.Platform.Scenarios.GigAssets
{
    /// <summary>
    /// Key-value storage for anything that an asset needs to know
    /// outside of GIGXR.Platform.Scenarios namespace.
    /// </summary>
    public interface IAssetContext
    {
        /// <summary>
        /// Map a string to a generic value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetContext<T>(string name, T value);

        /// <summary>
        /// Map a string to a function used to derive its value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="valueSource"></param>
        void SetContext<T>(string name, Func<T> valueSource);

        /// <summary>
        /// Get the value associated with a specified string.
        /// Default value of specified type is returned if string
        /// is not found or if the value cannot be converted to the
        /// specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        T GetContext<T>(string name);

        /// <summary>
        /// Get the value associated with a specified string.
        /// Return the specified default value if the string is
        /// not found or if its associated value cannot be converted
        /// to the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        T GetContext<T>(string name, T defaultValue);

        /// <summary>
        /// Remove a string key and its value from the asset context.
        /// </summary>
        /// <param name="name"></param>
        void RemoveContext(string name);
    }

    interface IAssetContextValue<T>
    {
        T Value { get; }
    }
}
