using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Prometheus;
using System;
using System.IO;
using TrackingService.Services.Db;
using TrackingService.Services.Mqtt;

namespace TrackingService
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseKestrel()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    config.AddJsonFile($"appsettings.{ env }.json", optional: false, reloadOnChange: true);
                });

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // Add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            var config = Configuration.GetSection("Mqtt");

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TrackingService", Version = "v1" });
            });

            services.AddTransient<ITrackingServiceAccountUnit, TrackingServiceAccountUnit>();
            services.AddSingleton<IMqttService, MqttService>();

            var connString =
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SQL_SERVER_NAME"))
               && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SA_PASSWORD")) ?
               $"Server={Environment.GetEnvironmentVariable("SQL_SERVER_NAME")};Database=Accounts;User Id=sa;Password={Environment.GetEnvironmentVariable("SA_PASSWORD")};" :
                Configuration.GetConnectionString("TrackingServiceDbContext");

            services.AddDbContext<TrackingServiceDbContext>(options =>
            options.UseSqlServer(connString));

            services.Configure<MqttOptions>
              (options => Configuration.GetSection("Mqtt").Bind(options));

            services.AddSingleton<MetricCollector>();

            services.PostConfigure<MqttOptions>((options) =>
            {
                var mqtt_host = Environment.GetEnvironmentVariable("MQTT_HOST");
                if (!string.IsNullOrEmpty(mqtt_host))
                    options.Host = mqtt_host;

                var mqtt_root_topic = Environment.GetEnvironmentVariable("MQTT_ROOT_TOPIC");
                if (!string.IsNullOrEmpty(mqtt_root_topic))
                    options.RootTopic = mqtt_root_topic;

            });
        }

        // Configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.EnvironmentName == "Development")
            // {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TrackingService5 v1"));
            //}

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            
            app.UseMetricServer();
            app.UseMiddleware<ResponseMetricMiddleware>();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
