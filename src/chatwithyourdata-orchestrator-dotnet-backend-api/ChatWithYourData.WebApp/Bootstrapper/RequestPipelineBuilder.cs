namespace ChatWithYourData.WebApp.Bootstrapper
{
    /// <summary>
    /// Request Pipeline Builder
    /// </summary>
    public static class RequestPipelineBuilder
    {
        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="app"></param>
        public static void Configure(WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
        }
    }
}
