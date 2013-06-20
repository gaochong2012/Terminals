﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Terminals.Data.DB;

namespace Terminals.Data.Validation
{
    /// <summary>
    /// Stupid tranformations to validate input values when storing to the database
    /// </summary>
    internal static class Validations
    {
        internal const string MAX_255_CHARACTERS = "Property maximum lenght is 255 characters.";

        internal const string UNKNOWN_PROTOCOL = "Protocol is unknown";

        internal const string PORT_RANGE = "Port has to be a number in range 0-65535.";

        static Validations()
        {
            RegisterSqlValidations();
            RegisterProvider(typeof(Favorite), typeof(FavoriteMetadata));

            // todo replace the validation in NewFavorite Form
        }

        private static void RegisterSqlValidations()
        {
            RegisterProvider(typeof (DbFavorite), typeof (DbFavoriteMetadata));
            RegisterProvider(typeof (DbBeforeConnectExecute), typeof (DbBeforeConnectExecuteMetadata));
            RegisterProvider(typeof (DbGroup), typeof (DbGroupMetadata));
            RegisterProvider(typeof (DbCredentialSet), typeof (DbCredentialSetMetadata));
        }

        private static void RegisterProvider(Type itemType, Type metadataType)
        {
            var association = new AssociatedMetadataTypeTypeDescriptionProvider(itemType, metadataType);
            TypeDescriptor.AddProviderTransparent(association, itemType);
        }

        internal static List<ValidationState> Validate(IFavorite favorite)
        {
            List<ValidationState> results = ValidateObject(favorite);
            var executeResults = ValidateObject(favorite.ExecuteBeforeConnect);
            results.AddRange(executeResults);
            return results;
        }

        internal static List<ValidationState> Validate(ICredentialSet credentialSet)
        {
            return ValidateObject(credentialSet);
        }

        internal static List<ValidationState> Validate(IGroup group)
        {
            return ValidateObject(group);
        }

        private static List<ValidationState> ValidateObject(object toValidate)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(toValidate, new ValidationContext(toValidate, null, null), results, true);
            return ConvertResultsToStates(results); 
        }

        private static List<ValidationState> ConvertResultsToStates(List<ValidationResult> results)
        {
            return results.Where(result => result.MemberNames.Any())
                .Select(ToState)
                .ToList();
        }

        private static ValidationState ToState(ValidationResult result)
        {
            return new ValidationState()
                {
                    PropertyName = result.MemberNames.First(),
                    Message = result.ErrorMessage
                };
        }
    }
}