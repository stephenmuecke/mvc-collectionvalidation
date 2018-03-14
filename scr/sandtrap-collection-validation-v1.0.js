(function ($) {
    formValidator = function (form) {
        this.form = $(form); // expose?
        this.validators = []; // expose?
        var validators = this.validators;
        var validateOnElement = false; // flag to indicate if validation is performed when an form control value changes

        this.init = function () {
            var validationElements = this.form.find('[data-collection-message-for]');
            $.each(validationElements, function () {
                if ($(this).data('collection-require')) {
                    validators.push(new requiredValidator($(this)));
                } else if ($(this).data('collection-require')) {
                    validators.push(new uniqueValidator($(this)));
                }
                // TODO: add 'collection-total'
            });
        }

        this.form.submit(function () {
            var isValid = true;
            $.each(validators, function (index, item) {
                item.validate(); // dont need to call this if validateOnElement = true
                if (!item.isValid) {
                    isValid = false;
                }
            });
            if ((!isValid || !$(this).valid()) && !validateOnElement) {
                validateOnElement = true;
                // add handlers for each element
                console.log('the form is invalid');
                $.each(validators, function (index, item) {
                    var validator = item;
                    $.each(item.controls, function (index, item) {
                        $(this).change(function () {
                            validator.validate();
                        })
                    })
                })
            }
            return isValid;
        })
        this.init();
    }
    // Validates that a property of an object in the collection must have a specified value
    requiredValidator = function (element) {
        this.element = element;
        this.errorMessage = $(element).data('collection-require');
        this.requiredValue = $(element).data('collection-require-value');
        this.minRequired = $(element).data('collection-require-minimum');
        this.maxRequired = $(element).data('collection-require-maximum');
        // Get the elements to validate
        var prefix = $(element).data('collection-message-for');
        var name = $(element).data('collection-require-property');
        var selector = '[name^="' + prefix + '"][name$="' + name + '"]';
        this.controls = $(selector);
        this.isCheckBoxes = false;
        if (this.controls.first().attr('type') == "checkbox") {
            this.controls = this.controls.not(':hidden');
            this.isCheckBoxes = true;
        }
        this.isValid = undefined;
        this.validate = function () {
            // TODO: modify this to use loops so that we can exit as early as possible (as per ValidationAttribute code)
            var matches;
            if (this.isCheckBoxes) {
                if (typeof this.requiredValue === 'boolean') {
                    matches = this.controls.filter(':checked').length;
                } else {
                    matches = this.controls.filter(':not(:checked)').length;
                }
            } else {
                var value = this.requiredValue;
                matches = this.controls.filter(function () { return $(this).val() === value; }); // trim?
            }
            this.isValid = (matches >= this.minRequired) && (this.maxRequired ? matches <= this.maxRequired : true);
            formatError(this.element, this.isValid, this.errorMessage)

            // add handler for change events

        }
    }
    // Validates that a property of an object in a collection must have a unique value
    uniqueValidator = function (element) {

    }
    // Validates that the total of a specific property must 
    totalValidator = function (element) {
    }
    // Formats the error message
    formatError = function (element, isValid, errorMessage) {
        if (isValid) {
            $(element).addClass('field-validation-valid').removeClass('field-validation-error').text('');
        } else {
            $(element).addClass('field-validation-error').removeClass('field-validation-valid').text(errorMessage);
        }
    }

    var forms = [];
    $.each($(document).find('form'), function () {
        forms.push(new formValidator($(this)));
    });
}(jQuery));
