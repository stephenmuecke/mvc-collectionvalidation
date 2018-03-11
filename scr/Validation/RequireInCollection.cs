using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Sandtrap.Web.Validation
{

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RequireInCollectionAttribute : CollectionValidationAttribute
    {

        #region .Declarations 

        private const string _DefaultErrorMessage = "Please select at least one item";
        private int _Minimum;
        private int? _Maximum;

        #endregion

        #region .Constructors 

        /// <summary>

        /// </summary>
        /// <param name="propertyName">

        /// </param>
        /// <param name="value">
        /// 
        /// </param>
        public RequireInCollectionAttribute(string propertyName, object value)
        {
            _PropertyName = propertyName;
            _Value = value;
            _Minimum = 1;
            ErrorMessage = _DefaultErrorMessage;
        }

        #endregion

        #region .Properties 

        /// <summary>
        /// Gets of sets the minimum number of items in the collection where the
        /// specified property must have the specified value.
        /// </summary>
        public int Minimum
        {
            get { return _Minimum; }
            set { _Minimum = value; }
        }

        /// <summary>
        /// Gets of sets the maximum number of items in the collection where the
        /// specified property has the specified value.
        /// </summary>
        public int Maximum
        {
            get { return _Maximum ?? int.MaxValue; }
            set { _Maximum = value; }
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
            if (value == null)
            {
                // In normal validation attributes, this would return true because a RequiredAttribute
                // is responsible for asserting a proprty has a value.
                return false;
            }
            var collection = value as IEnumerable;
            // Validate arguments
            ValidateCollection(collection);
            ValidateMinMax();
            // Loop through the collection to determine the number of valid matches
            int matches = 0;
            foreach (var item in collection)
            {
                PropertyInfo property = item.GetType().GetProperty(_PropertyName);
                if (property.GetValue(item).Equals(_Value))
                {
                    matches++;
                }
                // Exit the loop as soon as its known to be valid or invalid
                if (matches >= _Minimum && !_Maximum.HasValue)
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
            return (matches >= _Minimum) && (_Maximum.HasValue ? matches <= _Maximum.Value : true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string FormatErrorMessage(string name)
        {
            // TODO: Format message based on min and max values
            //return base.FormatErrorMessage(name);

            // If _Value is a bool, assume the message should be "Please select ....
            // otherwise the message should be like "At least x items must have the _PropertyName equal to _Value ..

            return "This is the error";
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, object> GetHtmlDataAttrbutes()
        {
            Dictionary<string, object> attributes = new Dictionary<string, object>
                {
                    { "data-collection-require", ErrorMessage },
                    { "data-collection-require-property", _PropertyName },
                    { "data-collection-require-value", _Value },
                    { "data-collection-require-minimum", _Minimum },
                };
            if (_Maximum.HasValue)
            {
                attributes.Add("data-collection-require-maximum", _Maximum.Value);
            }
            return attributes;
        }

        #endregion

        #region .Helper methods 

        /// <summary>
        /// Validates that the values for <see cref="Minimum"/> and <see cref="Maximum"/> Maximum are valid.
        /// </summary>
        private void ValidateMinMax()
        {
            if (_Minimum < 1)
            {
                // TODO: Add to resource file
                string errMsg = "The minimum must be greater than 0";

                throw new ArgumentOutOfRangeException(errMsg);
            }
            if (_Maximum.HasValue && _Maximum.Value < _Minimum)
            {
                // TODO: Add to resource file
                string errMsg = "The maximum must be greater than or equal to the minumum ('{1}')";

                string errorMessage = String.Format(errMsg, _Minimum);
                throw new ArgumentOutOfRangeException(errorMessage);
            }
        }

        #endregion

    }

}