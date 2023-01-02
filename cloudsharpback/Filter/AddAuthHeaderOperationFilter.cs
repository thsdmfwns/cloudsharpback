using cloudsharpback.Attribute;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Ubiety.Dns.Core;

namespace cloudsharpback.Filter
{
    public class AddAuthHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var isAuthorized = context.ApiDescription.ActionDescriptor.EndpointMetadata.Any(attribute => attribute is AuthAttribute);
            if (isAuthorized)
            {
                operation.Parameters ??= new List<OpenApiParameter>();
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "auth",
                    In = ParameterLocation.Header,
                    Required = true,
                    Schema = new OpenApiSchema()
                    {
                        Format = "String",
                    }
                });
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });
            }
        }
    }
}
