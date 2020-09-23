using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Common.Core;
using Common.Infrastructure.IOC;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace WebHost
{
    public class Startup
    {
        /// <summary>
        /// 泛型主机仅支持这2个类型的构造函数注入
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Configuration = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json", true, true)
               .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables()                      //主机配置环境变量
               .Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(option => option.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,//是否验证Issuer
                   ValidIssuer = JwtToken.KEY,//Issuer，这两项和前面签发jwt的设置一致
                   ValidateAudience = true,//是否验证Audience
                   ValidAudience = JwtToken.KEY,//Audience
                   ValidateLifetime = true,//是否验证失效时间
                   ClockSkew = TimeSpan.FromSeconds(30),
                   ValidateIssuerSigningKey = true,//是否验证SecurityKey                                   
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtToken.SECURITY_KEY))  //拿到SecurityKey
               }
               );

            services.AddOptions();
            services.AddHttpClient();
            services.AddResponseCompression();      //返回压缩，类似iis压缩
            services.AddHttpContextAccessor();   //HttpContext

            services.AddControllers().AddNewtonsoftJson();
            //支持跨域
            services.AddCors(options =>
            {
                options.AddPolicy("any", b =>
                {
                    b.AllowAnyOrigin() //允许任何来源的主机访问
                    .AllowAnyMethod()
                    .AllowAnyHeader();  //指定处理cookie

                });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Netcore API",
                    Description = "A simple example framework of ASP.NET Core Web API",
                });

                var xmls = Directory.CreateDirectory(AppContext.BaseDirectory).GetFiles("*.xml", SearchOption.AllDirectories);
                foreach (var xml in xmls)
                    c.IncludeXmlComments(xml.FullName, true);        //添加控制器层注释（true表示显示控制器注释）

                c.CustomSchemaIds(x => x.FullName);    //防止重名
            });
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you. If you
        // need a reference to the container, you need to use the
        // "Without ConfigureContainer" mechanism shown later.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            new DependencyRegister().Register(builder);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            //Authentication必须在authorization前，Authorization必须在Routing和Endpoints之间
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapDefaultControllerRoute();
                endpoints.MapControllerRoute("ActionApi", "api/{controller=Home}/{action=Index}/{id?}");
                //endpoints.MapHealthChecks("/health", new HealthCheckOptions() { });     //监控地址
            });

            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}
