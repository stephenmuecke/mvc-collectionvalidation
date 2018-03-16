using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Sandtrap.Web.Validation
{

    /// <summary>
    /// Base class for all collection validation attributes.
    /// </summary>
    public abstract class CollectionValidationAttribute : ValidationAttribute
    {

        #region .Declarations 

        #endregion

        #region .Properties 

        /// <summary>
        /// Gets the name of the property to validate.
        /// </summary>
        public string PropertyName { get; protected set; }

        #endregion

        #region .Methods 

        // TODO: Should we also pass the model (as IEnumerable collection) to this so that
        // we can call CheckCollection() and validate the arguments?
        // If not, do extra checking in the script

        /// <summary>
        /// Returns a Dictionary containing the name/value pairs used to generate the
        /// html data-* attributes used for client side validation.
        /// </summary>
        /// <param name="name">
        /// The fully qualified name of the property the attribute is applied to.
        /// </param>
        public abstract Dictionary<string, object> GetHtmlDataAttrbutes(string name);

        /// <summary>
        /// Checks that the property the attribute is applied to is a collection, and that the
        /// collection contains objects with the specified property.
        /// </summary>
        /// <param name="collection">
        /// The collection the attribute is applied to.
        /// </param>
        /// <exception cref="ArgumentException">
        /// is thrown of the property the attibute is applied to is not a collection, or 
        /// the object in the collection does not contain <see cref="CollectionValidationAttribute.PropertyName"/>.
        /// </exception>
        protected void CheckCollection(IEnumerable collection)
        {
            // Check that the property the attribute is applied to is a collection
            if (collection == null)
            {
                // TODO: Add to resource file
                string errMsg = "The property the attribute is applied to must be IEnumerable";
                // TODO: What would be correct exception type?
                throw new ArgumentException(errMsg);
            }
            Type type = GetTypeInCollection(collection);
            // This probably cannot happen, but just in case
            if (type == null)
            {
                // TODO: Add to resource file
                string errMsg = "The type in the collection cannot be resolved";
                // TODO: What would be correct exception type?
                throw new ArgumentException(errMsg);
            }
            // Validate the type in the collection contains the property name
            if (type.GetProperty(PropertyName) == null)
            {
                // TODO: Add to resource file
                string errMsg = "'{0}' does not contain a property named '{2}'";
                string errorMessage = String.Format(errMsg, type.Name, PropertyName);
                throw new ArgumentException(errMsg);
            }
        }

        #endregion

        #region .Helper methods 

        /// <summary>
        /// Returns the type of the object in the collection.
        /// </summary>
        /// <param name="collection">
        /// The collection the attribute is applied to.
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

        #endregion

    }

}