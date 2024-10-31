using ChatWithYourData.WebApp.Bootstrapper;

// Get WebApplication instance
WebApplication app = AppBuilder.GetApp(args);

// Configure Request Pipeline
RequestPipelineBuilder.Configure(app);

// Start the app
app.Run();
