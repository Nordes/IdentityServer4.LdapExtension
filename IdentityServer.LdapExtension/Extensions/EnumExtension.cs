using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace IdentityServer.LdapExtension.Extensions
{
    /// <summary> 
    /// Enum Extension Methods
    /// </summary>
    /// <typeparam name="T"> type of Enum </typeparam>
    internal class Enum<T> where T : struct, IConvertible
    {
        /// <summary>
        /// Gets the descriptions attribute from an Enum (all of them at once).
        /// </summary>
        /// <exception cref="ArgumentException">T must be an enumerated type</exception>
        public static string[] Descriptions
        {
            get
            {
                if (!typeof(T).IsEnum)
                    throw new ArgumentException("T must be an enumerated type");

                List<string> result = new List<string>();
                foreach (var e in Enum.GetValues(typeof(T)))
                {
                    var fi = e.GetType().GetField(e.ToString());
                    var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    var description = attributes[0].Description;
                    if (!result.Contains(description))
                    {
                        result.Add(description);
                    }
                }

                return result.ToArray();
            }
        }
    }
}
