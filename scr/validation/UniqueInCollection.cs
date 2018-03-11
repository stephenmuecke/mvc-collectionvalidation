using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sandtrap.Web.Validation
{

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class UniqueInCollectionAttribute : CollectionValidationAttribute
    {

        #region .Declarations 

        private const string _DefaultErrorMessage = "....";

        #endregion

        #region .Constructors 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName">
        /// 
        /// </param>
        public UniqueInCollectionAttribute(string propertyName)
        {
            _PropertyName = propertyName;
            ErrorMessage = _DefaultErrorMessage;
        }

        #endregion

        #region .Methods 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">
        /// 
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        public override bool IsValid(object value)
        {
            var collection = value as IEnumerable;
            // Validate arguments
            ValidateCollection(collection);
            // Loop through the collection to determine if valid
            List<object> values = new List<object>();
            foreach (var item in collection)
            {
                PropertyInfo property = item.GetType().GetProperty(_PropertyName);
                object propertyValue = property.GetValue(item);
                if (values.Any(x => x.Equals(propertyValue)))
                {
                    return false;
                }
                else
                {
                    values.Add(propertyValue);
                }
            }
            // If we got here, all values are unique;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">
        /// 
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        public override string FormatErrorMessage(string name)
        {
            // TODO: Format message
            return "This is the error";
        }

        #endregion

    }

}