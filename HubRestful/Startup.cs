using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using HubRestful.Automapper;
using HubRestful.Configure;
using HubRestful.DotNetty;
using HubRestful.Model;
using HubRestful.OpcClientServer;
using HubRestful.OpcClientServer.Client;
using HubRestful.QuartzServer;
using Microsoft.Extensions.Options;
using HubRestful.RestClient;
using HubRestful.Server;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.PlatformAbstractions;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;

namespace HubRestful
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// ���ò���
        /// </summary>
        public IConfiguration Configuration { get; set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        /// <summary>
        /// ���÷�����
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<MongoSetting>(
                Configuration.GetSection(nameof(MongoSetting)));
            
            services.Configure<OpcSetting>(
                Configuration.GetSection(nameof(OpcSetting)));
            
            services.Configure<OpcSetting>(
                Configuration.GetSection(nameof(S7Setting)));
            
            services.AddSingleton<IMongoDataBaseSettings>(sp=>
                sp.GetRequiredService<IOptions<MongoSetting>>().Value);
            
            services.AddSingleton<IOpcSetting>(sp =>
                sp.GetRequiredService<IOptions<OpcSetting>>().Value);
            
            services.AddSingleton<IS7setting>(sp =>
                sp.GetRequiredService<IOptions<S7Setting>>().Value);
            
            services.AddControllers().AddJsonOptions(config =>
            {
                //���趨���JsonResult���ı����������
                config.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                config.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                config.JsonSerializerOptions.Converters.Add(new DateTimeNullableConvert());
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "����������Rest API", 
                    Version = "v1",
                    Description = "����������API�ĵ�",
                });

                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath,"HubRestful.xml");
                c.IncludeXmlComments(xmlPath);
            });
            services.AddMemoryCache(options =>
            {
                //��󻺴�ռ��С����Ϊ 1024
                options.SizeLimit = 1024;
                //�����������Ϊ����ѹ����Ϊ 2%
                options.CompactionPercentage = 0.02d;
                //ÿ 5 ���ӽ���һ�ι��ڻ����ɨ��
                options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
            });

            services.AddHttpClient("github", c =>
            {
                c.BaseAddress = new Uri("https://127.0.0.1:441");

            }).AddTypedClient(c => Refit.RestService.For<IRestInterface>(c))
                .ConfigurePrimaryHttpMessageHandler(provider => new HttpClientHandler {
                AllowAutoRedirect = false,
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12
            });

            services.AddSingleton(typeof(MemoryCacheExtensions));
            
            services.AddSingleton<ScanDataBaseServer>();
            
            services.AddSingleton<OrderDataBaseServer>();

            services.AddSingleton<TagDataBaseServer>();

            services.AddSingleton<ErrDataBaseServer>();
            
            services.AddSingleton<OpcuaClient,OpcuaClientService>();

            services.AddSingleton<IRestfulClient,RestfulClient>();
            
            services.AddTransient<OpcServerSyncjob>();      // ����ʹ��˲ʱ����ע��
            
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();//ע��ISchedulerFactory��ʵ����
            
            services.AddSingleton<QuartzStartup>();
            
            services.AddSingleton<IJobFactory,IocJobFactory>();
            
        //    services.AddTransient<IHostedService, StartJob>();
            services.AddSingleton<QuoteOfTheMomentClientHandler>();
            
            services.AddSingleton<QuoteUdpStartup>();

            services.AddSingleton<OpcClient>();

            services.AddSingleton<IS7ConnectFactory, S7ConnectFactory>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// ���÷���
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="appLifetime"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
                app.UseSwagger();
                
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HubRestful v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            
            app.UseWebSockets(new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            });
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            //�����������뵥��
            var quartz = app.ApplicationServices.GetRequiredService<QuartzStartup>();
            var quote = app.ApplicationServices.GetRequiredService<QuoteUdpStartup>();
            var opc = app.ApplicationServices.GetRequiredService<OpcClient>();
            appLifetime.ApplicationStarted.Register(() =>
            {
         //       quartz.Start().Wait(); //��վ�������ִ��
                quote.Start().Wait();
            //    opc.Start().Wait();
            });
          
            appLifetime.ApplicationStopped.Register(() =>
            {
         //       quartz.Stop();  //��վֹͣ���ִ��
                quote.Stop();
               // opc.Stop();
            });
        }
    }
}
