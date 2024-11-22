namespace BlackListApi.Infrastructure;

public class CustomApiKeyAuthenticationMiddleware(RequestDelegate next)
{
	private const string ApikeyName = "X-Api-Key";

	public async Task InvokeAsync(HttpContext context)
	{
		// If the request is going to swagger, skip the middleware
		if (context.Request.Path.StartsWithSegments("/swagger") || context.Request.Path.StartsWithSegments("/health"))
		{
			await next(context);
			return;
		}
		
		if (!context.Request.Headers.TryGetValue(ApikeyName, out var extractedApiKey))
		{
			context.Response.StatusCode = 401;
			await context.Response.WriteAsync("Api Key was not provided ");
			return;
		}

		var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();

		var apiKey = appSettings.GetValue<string>(ApikeyName) ?? "123456";

		if (!apiKey.Equals(extractedApiKey))
		{
			context.Response.StatusCode = 401;
			await context.Response.WriteAsync("Unauthorized");
			return;
		}

		await next(context);
	}
}