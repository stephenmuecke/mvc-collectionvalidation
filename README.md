# asp.net-mvc Collection Validation
## Description
An asp.net-mvc plugin to provide client and server side validation for collection properties of a model.

The project consists of 3 components:
- Validation Attributes that are applied to collection properties which provide server side validation
- A `HtmlHelper` extension method to generate a placeholder in the view for client and server side error messages
- A jQuery plugin that provides client side validation

### Validation Attributes
The validation attributes include (so far)
#### `RequireInCollectionAttribute`
Asserts that at least one object in the collection must have a specified property set to a specified value.

For example, a `UserViewModel` might have a `List<RoleViewModel>` property, where `RoleViewModel` contains a `bool IsSelected` property that is rendered as a checkbox in the view. To assert that at least one Role is selected, the attribute would be applied as

    [RequireInCollection("IsSelected", true)]
    public List<RoleViewModel> Roles { get; set; }

The attribute also includes `Minumum` and `Maximum` properties to assert that, for example, a minumun of 3 Roles must be selected, or  between 2 and 4 Roles must be selected.

#### `UniqueInCollectionAttribute`

Asserts that for each object in a collection, that the value of a specified property must have a unique value.

For example, a `SurveyViewModel` might have a `List<ProductViewModel>` property, where `ProductViewModel` contains a `int Rating` property that is rendered in the view as a numeric textbox (and is used to rate each product in order of preference). To assert that the value of each `Rating` is unique, the attribute would be applied as

    [UniqueInCollection("Rating")]
    public List<ProductViewModel> Products { get; set; }

#### `TotalInCollection`

Asserts that the total of a specified property of the object in the collection must be within a specified range.

### HtmlHelper

The `@Html.CollectionValidationMessageFor()` extension method is used to generate the placeholder element in the view to display the error message if the the collection property is invalid (the equivalent if `@Html.ValidationMessageFor()`).

The default element is `<span>`.

### jQuery plugin



