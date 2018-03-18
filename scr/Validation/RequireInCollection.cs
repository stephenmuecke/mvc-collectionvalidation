using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Sandtrap.Web.Validation
{

    /// <summary>
    /// Validation attribute to assert that at least one object in the collection must
    /// have a property equal to a specified value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RequireInCollectionAttribute : CollectionValidationAttribute
    {

        #region .Declarations 

        private int? _Maximum;

        #endregion

        #region .Constructors 

        /// <summary>
        /// Constructor to specify the property name and required value.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to validate.
        /// </param>
        /// <param name="requiredValue">
        /// The required value of the property.
        /// </param>
        /// <remarks>
        /// The following placeholders can be used when setting <see cref="ValidationAttribute.ErrorMessage"/> 
        /// - {0} The display name of the collection property the attribute is applied to
        /// - {1} The display name of the property
        /// - {2} The required value
        /// - {3} The minimum value
        /// - {4} The maximum value
        /// </remarks>
        public RequireInCollectionAttribute(string propertyName, object requiredValue)
        {
            PropertyName = propertyName;
            RequiredValue = requiredValue;
            Minimum = 1;
        }

        #endregion

        #region .Properties 

        /// <summary>
        /// Gets the required value for the <see cref="CollectionValidationAttribute.PropertyName"/>
        /// </summary>
        public object RequiredValue { get; private set; }

        /// <summary>
        /// Gets or sets the minimum number of items in the collection where the
        /// <see cref="CollectionValidationAttribute.PropertyName"/> must have 
        /// the <see cref="RequiredValue"/>.
        /// </summary>
        /// <remarks>
        /// The default is 1. The value cannot be less that 1.
        /// </remarks>
        public int Minimum { get; set; }

        /// <summary>
        /// Gets of sets the maximum number of items in the collection where the
        /// property has the value.
        /// </summary>
        public int Maximum
        {
            get { return _Maximum ?? int.MaxValue; }
            set { _Maximum = value; }
        }

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
        /// <exception cref="ArgumentOutOfRangeException">
        /// is thrown is the <see cref="Minimum"/> is less than 1, 
        /// or if <see cref="Maximum"/> is less than <see cref="Minimum"/>
        /// </exception>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                // In normal validation attributes, this would return true because a RequiredAttribute
                // is responsible for asserting a property has a value
                return false;
            }
            var collection = value as IEnumerable;
            // Check arguments
            CheckCollection(collection);
            CheckMinMax();
            // Loop through the collection to determine the number of valid matches
            int matches = 0;
            foreach (var item in collection)
            {
                PropertyInfo property = item.GetType().GetProperty(PropertyName);
                if (property.GetValue(item).Equals(RequiredValue))
                {
                    matches++;
                }
                // Exit the loop as soon as its known to be valid or invalid
                if (matches >= Minimum && !_Maximum.HasValue)
                {
                    // Its valid
                    break;
                }
                else if (_Maximum.HasValue && matches > _Maximum.Value)
                {
                    // Its invalid
                    break;
                }
            }
            return (matches >= Minimum) && (_Maximum.HasValue ? matches <= _Maximum.Value : true);
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
            if (ErrorMessage == null)
            {
                // Create the default message
                string defaultMessage = null;
                if (RequiredValue is bool)
                {
                    // Assume the view is generating checkboxes for selection
                    // TODO: what if the value is false (would it even make sense?)
                    defaultMessage = "Please select at least one {0}";
                    if (Minimum > 1 && !_Maximum.HasValue)
                    {
                        defaultMessage = "Please select at least {3} {0}";
                    }
                    if (_Maximum.HasValue)
                    {
                        defaultMessage = "Please select between {3} and {4} {0}";
                    }
                }
                else
                {
                    defaultMessage = "At least one {0} must have it {1} property equal to {2}";
                    if (Minimum > 1 && !_Maximum.HasValue)
                    {
                        defaultMessage = "At least {3} {0} must have their {1} property equal to {2}";
                    }
                    if (_Maximum.HasValue)
                    {
                        defaultMessage = "At least {3} and not more than {4} {0} must have their {1} property equal to {2}";
                    }
                }
                ErrorMessage = defaultMessage;
            }
            return string.Format(ErrorMessageString, name, PropertyName, RequiredValue, Minimum, _Maximum);
        }

        /// <summary>
        /// Returns a Dictionary containing the name/value pairs used to generate the
        /// html data-* attributes used for client side validation.
        /// </summary>
        /// <param name="name">
        /// The fully qualified name of the property the attribute is applied to.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// is thrown is the <see cref="Minimum"/> is less than 1, 
        /// or if <see cref="Maximum"/> is less than <see cref="Minimum"/>
        /// </exception>
        public override Dictionary<string, object> GetHtmlDataAttrbutes(string name)
        {
            CheckMinMax();
            string errorMessage = FormatErrorMessage(name);
            object requiredValue = RequiredValue is bool ? RequiredValue.ToString().ToLower() : RequiredValue;
            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add("data-col-require", errorMessage);
            attributes.Add("data-col-require-property", PropertyName);
            attributes.Add("data-col-require-value", requiredValue);
            attributes.Add("data-col-require-minimum", Minimum);
            if (_Maximum.HasValue)
            {
                attributes.Add("data-col-require-maximum", _Maximum.Value);
            }
            return attributes;
        }

        #endregion

        #region .Helper methods 

        /// <summary>
        /// Checks that the values for <see cref="Minimum"/> and <see cref="Maximum"/> are valid.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// is thrown is the <see cref="Minimum"/> is less than 1, 
        /// or if <see cref="Maximum"/> is less than <see cref="Minimum"/>
        /// </exception>
        private void CheckMinMax()
        {
            if (Minimum < 1)
            {
                // TODO: Add to resource file
                string errMsg = "The minimum must be greater than 0";
                throw new ArgumentOutOfRangeException(errMsg);
            }
            if (_Maximum.HasValue && _Maximum.Value < Minimum)
            {
                // TODO: Add to resource file
                string errMsg = "The maximum must be greater than or equal to the minumum ('{1}')";
                string errorMessage = String.Format(errMsg, Minimum);
                throw new ArgumentOutOfRangeException(errorMessage);
            }
        }

        #endregion

    }

}
