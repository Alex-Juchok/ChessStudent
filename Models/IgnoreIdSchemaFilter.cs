using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using ChessSchoolAPI.Models;


public class IgnoreIdSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Проверяем, что текущая схема относится к модели ChessStudent
        if (context.Type == typeof(ChessStudent))
        {
            // Удаляем свойство "id" из Swagger-схемы
            schema.Properties.Remove("id");
        }
    }
}
