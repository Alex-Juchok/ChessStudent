using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using ChessSchoolAPI.Models;


public class IgnoreIdSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(ChessStudent))
        {
            schema.Properties.Remove("id");
            schema.Properties.Remove("user_id");
            schema.Properties.Remove("confirmationTime");
        }
    }
}
