using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace LMS_GV.Helpers
{
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileUploadMime = "multipart/form-data";
            if (operation.RequestBody?.Content?.Any(x => x.Key.Equals(fileUploadMime, StringComparison.InvariantCultureIgnoreCase)) == true)
            {
                var uploadFileMediaType = operation.RequestBody.Content[fileUploadMime];

                uploadFileMediaType.Schema = new OpenApiSchema()
                {
                    Type = "object",
                    Properties =
                    {
                        ["file"] = new OpenApiSchema()
                        {
                            Description = "Upload file",
                            Type = "string",
                            Format = "binary"
                        }
                    }
                };
            }
        }
    }
}