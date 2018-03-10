using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Sandtrap.Web.Validation
{

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RequireInCollection : ValidationAttribute
    {
        // throw error if applied to property that is not an exception
        // throw error if property name does not exist in collection


        #region .Declarations 

        private const string _DefaultErrorMessage = "Please select at least one item";
        private readonly string _PropertyName;
        private readonly object _Value;
        private int _Minimum;
        private int? _Maximum;

        #endregion

        #region .Constructors 

        /// <summary>
        /// Validates that 
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property 
        /// </param>
        /// <param name="value">
        /// 
        /// </param>
        public RequireInCollection(string propertyName, object value)
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
            // Validate Minumum and Maximum arguments
            ValidateMinMax();
            if (value == null)
            {
                // In normal validation attributes, this would return true because a RequiredAttribute
                // is responsible for asserting a proprty has a value.
                return false;
            }
            // Validate that the property the attribute is applied to is a collection
            var collection = value as IEnumerable;
            if (collection == null)
            {
                // TODO: Add to resource file
                string errMsg = "The property the attribute is applied to must be IEnumerable";
                // TODO: What would be correct exception type?
                throw new Exception(errMsg);
            }
            Type type = GetTypeInCollection(collection);
            // This probably cannot happen, but just in case
            if (type == null)
            {
                // TODO: Add to resource file
                string errMsg = "The type in the collection cannot be resolved";
                // TODO: What would be correct exception type?
                throw new Exception(errMsg);
            }
            // Validate the type in the collection contains the property name
            if (type.GetProperty(_PropertyName) == null)
            {
                // TODO: Add to resource file
                string errMsg = "'{0}' does not contain a property named '{2}'";

                string errorMessage = String.Format(errMsg, type.Name, _PropertyName);
                throw new ArgumentException(errMsg);
            }
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

        public override string FormatErrorMessage(string name)
        {
            // TODO: Format message based on min and max values
            return base.FormatErrorMessage(name);
        }

        #endregion

        #region .Helper methods 

        /// <summary>
        /// Returns the type of the object in the collection.
        /// </summary>
        /// <param name="collection">
        /// 
        /// </param>
        private Type GetTypeInCollection(IEnumerable collection)
        {
            Type type = collection.GetType();
            if (type.IsGenericType)
            {
                return type.GetInterfaces().Where(t => t.IsGenericType)
                    .Where(t => t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Single().GetGenericArguments()[0];
            }
            else if (type.IsArray)
            {
                return type.GetElementType();
            }
            return null;
        }

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

    public class UniqueInCollection : ValidationAttribute
    {

    }

    public class TotalInCollection : ValidationAttribute
    {
    }
}