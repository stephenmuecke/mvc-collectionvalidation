using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sandtrap.Web.Validation
{

    /// <summary>
    /// Validation attribute to assert a property of each item in a collection
    /// must have a unique value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class UniqueInCollectionAttribute : CollectionValidationAttribute
    {

        #region .Declarations 

        private const string _DefaultErrorMessage = "The value of {1} must be unique";

        #endregion

        #region .Constructors 

        /// <summary>
        /// Constructor to specify the property name.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to validate.
        /// </param>
        /// <remarks>
        /// The following placeholders can be used when setting <see cref="ValidationAttribute.ErrorMessage"/> 
        /// - {0} The display name of the collection property the attribute is applied to
        /// - {1} The display name of the property
        /// </remarks>
        public UniqueInCollectionAttribute(string propertyName)
        {
            PropertyName = propertyName;
            ErrorMessage = _DefaultErrorMessage;
        }

        #endregion

        #region .Properties 

        #endregion

        #region .Methods 

        /// <summary>
        /// Override of <see cref="ValidationAttribute.IsValid(object)"/>
        /// </summary>
        /// <param name="value">
        /// The collection to test.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the collection is valid.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// is thrown of the property the attibute is applied to is not a collection, or 
        /// the object in the collection does not contain <see cref="CollectionValidationAttribute.PropertyName"/>.
        /// </exception>
        public override bool IsValid(object value)
        {
            var collection = value as IEnumerable;
            // Validate arguments
            CheckCollection(collection);
            // Loop through the collection to determine if valid
            List<object> values = new List<object>();
            foreach (var item in collection)
            {
                PropertyInfo property = item.GetType().GetProperty(PropertyName);
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
        /// Override of <see cref="ValidationAttribute.FormatErrorMessage"/>
        /// </summary>
        /// <param name="name">
        /// The name to of the collection property the attribute is applied to.
        /// </param>
        /// <returns>
        /// A localized string to describe the requirement.
        /// </returns>
        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessage, name, PropertyName);
        }


        /// <summary>
        /// Returns a Dictionary containing the name/value pairs used to generate the
        /// html data-* attributes used for client side validation.
        /// </summary>
        /// <param name="name">
        /// The fully qualified name of the property the attribute is applied to.
        /// </param>
        public override Dictionary<string, object> GetHtmlDataAttrbutes(string name)
        {
            string errorMessage = FormatErrorMessage(name);
            Dictionary<string, object> attributes = new Dictionary<string, object>
            {
                { "data-col-unique", errorMessage },
                { "data-col-unique-property", PropertyName }
            };
            return attributes;
        }

        #endregion

    }

}
