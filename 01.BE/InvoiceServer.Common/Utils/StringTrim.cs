using System;
using System.ComponentModel.DataAnnotations;

namespace InvoiceServer.Common
{
    public class StringTrimAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //try to modify text
            try
            {
                var property = validationContext.ObjectType.GetProperty(validationContext.MemberName);
                if (property.PropertyType == typeof(string) && value != null)
                {
                    property.SetValue(validationContext.ObjectInstance, value.ToString().Trim(), null);
                }
            }
            catch (Exception ex)
            {
                Logger logger = new Logger();
                logger.Error("Class:StringTrim Method:IsValid", ex);
            }

            //return null to make sure this attribute never say iam invalid
            return null;
        }
    }
}
