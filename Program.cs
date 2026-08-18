using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// configure Azure Blob settings (set via appsettings or environment variables)
// Accept either hierarchical key `AzureStorage:ConnectionString` or legacy env var `AZURE_STORAGE_CONNECTION_STRING`.
var azureConn = builder.Configuration["AzureStorage:ConnectionString"]
                ?? builder.Configuration["AzureStorage__ConnectionString"]
                ?? builder.Configuration["AZURE_STORAGE_CONNECTION_STRING"];
var azureContainer = builder.Configuration["AzureStorage:ContainerName"] ?? builder.Configuration["AzureStorage__ContainerName"] ?? "coffeeimages";
if (!string.IsNullOrEmpty(azureConn))
{
    try
    {
        // Try creating the blob service now; if the connection string is malformed the SDK
        // may throw. Catch and continue so the API can still run without blob storage.
        var blobSvc = new CoffeeShopAPI.Services.AzureBlobService(azureConn, azureContainer);
        builder.Services.AddSingleton<CoffeeShopAPI.Services.IAzureBlobService>(blobSvc);
    }
    catch (Exception ex)
    {
        // Log to console; container will continue without blob support.
        Console.WriteLine($"AzureBlobService not registered: {ex.Message}");
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// handledEventsAllowedBeforeBreaking: internal counter. When the policy observes 3 handled failures it opens the circuit.
// onBreak hook: invoked when the circuit opens; it does not increment the counter — it’s a notification you can log/act on.
// onReset / onHalfOpen hooks: invoked when the circuit resets or moves to half-open for a trial call.


// --- PATTERN 1: RETRY PATTERN ---
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2));

// --- PATTERN 2: CIRCUIT BREAKER ---
// With 3 consecutive failures, the circuit breaker will "trip" and block any further calls for 15 seconds. After that, it will allow a single call to test if the service is back up.
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(15)
    );

// ---  Combined (PolicyWrap) ---
// Combine the Retry and Circuit Breaker policies. The retry policy will be applied first, followed by the circuit breaker policy.
var combinedPolicy = Policy.WrapAsync(circuitBreakerPolicy, retryPolicy);

builder.Services.AddHttpClient("BankService", client =>
{
    client.BaseAddress = new Uri("http://localhost:9999/");
})
.AddPolicyHandler(combinedPolicy);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();
app.Run();