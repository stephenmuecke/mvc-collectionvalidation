﻿(function ($) {

    formValidator = function (form) {
        var form = $(form);
        var validators = [];
        // flag to indicate if validation is performed when an form control value changes
        // initially false so that validation is only performed on first submit (and thereafter on change)
        var validateOnElement = false;

        this.init = function () {
            // Get all the elements generated by @Html.CollectionValidationMessageFor()
            var validationElements = form.find('[data-col-message-for]');
            $.each(validationElements, function () {
                var self = $(this);
                // Add validators
                if (self.data('col-require')) {
                    validators.push(new requiredValidator(self));
                }
                if (self.data('col-unique')) {
                    validators.push(new uniqueValidator(self));
                }
                if (self.data('col-total')) {
                    validators.push(new totalValidator(self));
                }
            });
        }

        // Handle the forms submit event
        form.submit(function () {
            var isValid = true;
            $.each(validators, function (index, item) {
                if (!validateOnElement) {
                    item.validate();
                }
                if (!item.isValid) {
                    isValid = false;
                }
            });
            if ((!isValid || !$(this).valid()) && !validateOnElement) {
                validateOnElement = true;
                // add handlers for each element
                $.each(validators, function (index, item) {
                    var validator = item;
                    $.each(item.controls, function (index, item) {
                        // use keyup for textboxes?
                        $(this).change(function () {
                            validator.validate();
                        })
                    })
                })
            }
            return isValid;
        })

        // Initialize
        this.init();
    }

    // Asserts that a property of an object in the collection must have a specified value
    requiredValidator = function (element) {
        this.element = element;
        this.errorMessage = $(element).data('col-require');
        this.requiredValue = $(element).data('col-require-value');
        this.minRequired = $(element).data('col-require-minimum');
        this.maxRequired = $(element).data('col-require-maximum');

        // Get the form controls to validate
        var prefix = $(element).data('col-message-for');
        var name = $(element).data('col-require-property');
        var selector = '[name^="' + prefix + '"][name$="' + name + '"]';
        this.controls = $(selector);
     
        // for checkboxes, we need to discard the associated hidden inputs
        this.isCheckBoxes = false;
        var checkboxes = this.controls.filter(':checkbox');
        if (checkboxes.length > 0) {
            this.controls = checkboxes;
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
        }
    }

    // Asserts that a property of an object in a collection must have a unique value
    uniqueValidator = function (element) {
        this.element = element;
        this.errorMessage = $(element).data('col-unique');

        // Get the form controls to validate
        var prefix = $(element).data('col-message-for');
        var name = $(element).data('col-unique-property');
        var selector = '[name^="' + prefix + '"][name$="' + name + '"]';
        this.controls = $(selector);

        this.isValid = undefined;

        this.validate = function () {
            var isValid = true;
            var values = [];
            // Is there any point in validating checkboxes?
            $.each(this.controls, function (index, item) {
                var value = $(this).val();
                if (values.indexOf(value) >= 0) {
                    isValid = false;
                    return false;
                }
                values.push(value);
            });
            this.isValid = isValid;
            formatError(this.element, this.isValid, this.errorMessage)
        }

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
