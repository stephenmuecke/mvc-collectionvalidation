using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Sandtrap.Web.Validation
{

    public abstract class CollectionValidationAttribute : ValidationAttribute
    {

        #region .Declarations 

        protected string _PropertyName;
        protected object _Value;

        #endregion

        #region .Methods 

        protected void ValidateCollection(IEnumerable collection)
        {
            // Validate that the property the attribute is applied to is a collection
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

        #endregion

    }

}
