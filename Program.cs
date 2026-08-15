using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();
app.MapControllers();
app.Run();