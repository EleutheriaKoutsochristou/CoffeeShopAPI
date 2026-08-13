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

// ΠΡΟΣΟΧΗ: Εδώ γράφουμε AddHttpClient
builder.Services.AddHttpClient("BankService", client =>
{
    client.BaseAddress = new Uri("https://httpstat.us/"); 
})
.AddPolicyHandler(retryPolicy);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();