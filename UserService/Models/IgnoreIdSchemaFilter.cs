using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserService.Models;


public class IgnoreIdSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(User))
        {
            schema.Properties.Remove("id");
            schema.Properties.Remove("registeredObjects");
        }
    }
}
