using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Configuration;
using CoreWCF.Description;
using SoapDemo.Contracts;
using SoapDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Register CoreWCF
builder.Services.AddServiceModelServices();   // CoreWCF core
builder.Services.AddServiceModelMetadata();   // Enables WSDL generation
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

// ── Register your service
builder.Services.AddTransient<CalculatorService>();
builder.Services.AddTransient<BankAccountService>();

var app = builder.Build();

var httpsBinding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);

// ── Map the SOAP endpoint
app.UseServiceModel(svcBuilder => {
    svcBuilder.AddService<CalculatorService>(options => {
        // Expose WSDL at: /CalculatorService?wsdl
        options.DebugBehavior.IncludeExceptionDetailInFaults =
            app.Environment.IsDevelopment();
    });

    svcBuilder.AddServiceEndpoint<CalculatorService, ICalculatorService>(
        httpsBinding,        // SOAP 1.1 over HTTPS
        "/CalculatorService"
    );


#region Bank Account Add Service
    svcBuilder.AddService<BankAccountService>(options => 
    {
        // Expose WSDL at: /BankAccountService?wsdl
        options.DebugBehavior.IncludeExceptionDetailInFaults =
            app.Environment.IsDevelopment();
    });

    svcBuilder.AddServiceEndpoint<BankAccountService, IBankAccountService>( 
        httpsBinding,       // SOAP 1.1 over HTTPS
        "/BankAccountService"
    );
#endregion
});


// ── Expose WSDL metadata
var metaBehaviorCalc = app.Services.GetRequiredService<ServiceMetadataBehavior>();
metaBehaviorCalc.HttpsGetEnabled = true;

var metaBehaviorBank = app.Services.GetRequiredService<ServiceMetadataBehavior>();
metaBehaviorBank.HttpsGetEnabled = true;

app.Run();