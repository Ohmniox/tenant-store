using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Extensions
{

    public class HttpRequestBody<T>
    {
        public bool IsValid { get; set; }
        public T Value { get; set; }

        public IEnumerable<ValidationResult> ValidationResults { get; set; }
        public string ValidationMessage { get; set; }
    }
    public static class ModelValidationExtension
    {
        public static async Task<HttpRequestBody<T>> GetBodyAsync<T>(this HttpRequest request)
        {
            var body = new HttpRequestBody<T>();
            try
            {
                string bodyString = await new StreamReader(request.Body).ReadToEndAsync();
                body.Value = JsonConvert.DeserializeObject<T>(bodyString);
                var results = new List<ValidationResult>();
                body.IsValid = Validator.TryValidateObject(body.Value, new ValidationContext(body.Value, null, null), results, true);
                body.ValidationResults = results;
                body.ValidationMessage = $"Model is invalid: {string.Join(", ", body.ValidationResults.Select(s => s.ErrorMessage).ToArray())}";
            }
            catch (Exception)
            {
                body.IsValid = false;
                body.ValidationMessage = $"Error parsing the model";
            }
            return body;
        }
    }
}
