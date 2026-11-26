using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SmartTravelPlanner.Models;

public class IsCity : ValidationAttribute
{
    private readonly string? _citiesCollectionPropertyName;

    public IsCity(string citiesCollectionPropertyName)
    {
        _citiesCollectionPropertyName = citiesCollectionPropertyName;
    }

    public IsCity(string errorMessage, string ErrorMessage) : base(errorMessage)
    {
    }

    protected override ValidationResult? IsValid(
        object? value, ValidationContext context)
    {
        if (value is not string input || string.IsNullOrWhiteSpace(input))
        {
            return ValidationResult.Success;
        }

        if (_citiesCollectionPropertyName is null || context.ObjectType
            .GetProperty(_citiesCollectionPropertyName)?
            .GetValue(context.ObjectInstance) is not IEnumerable<string> collection)
        {
            return new ValidationResult(ErrorMessage ?? "Please enter current location");
        }

        if (!collection.Any())
        {
            return ValidationResult.Success;
        }

        if (!collection.Contains(input))
        {
            return new ValidationResult(ErrorMessage ?? "Please enter existing city from the map");
        }

        return ValidationResult.Success;
    }
}