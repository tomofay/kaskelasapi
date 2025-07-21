using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class FormFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Periksa apakah endpoint memiliki atribut [Consumes("multipart/form-data")]
        var consumesAttribute = context.ApiDescription.CustomAttributes()
            .OfType<Microsoft.AspNetCore.Mvc.ConsumesAttribute>()
            .FirstOrDefault(attr => attr.ContentTypes.Contains("multipart/form-data"));

        if (consumesAttribute == null)
        {
            // Jika tidak ada atribut [Consumes("multipart/form-data")], abaikan filter
            return;
        }

        // Tambahkan skema untuk file upload
        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["file"] = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            }
                        }
                    }
                }
            }
        };
    }
}