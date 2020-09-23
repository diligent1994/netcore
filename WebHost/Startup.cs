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
        /// ����������֧����2�����͵Ĺ��캯��ע��
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
               .AddEnvironmentVariables()                      //�������û�������
               .Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(option => option.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,//�Ƿ���֤Issuer
                   ValidIssuer = JwtToken.KEY,//Issuer���������ǰ��ǩ��jwt������һ��
                   ValidateAudience = true,//�Ƿ���֤Audience
                   ValidAudience = JwtToken.KEY,//Audience
                   ValidateLifetime = true,//�Ƿ���֤ʧЧʱ��
                   ClockSkew = TimeSpan.FromSeconds(30),
                   ValidateIssuerSigningKey = true,//�Ƿ���֤SecurityKey                                   
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtToken.SECURITY_KEY))  //�õ�SecurityKey
               }
               );

            services.AddOptions();
            services.AddHttpClient();
            services.AddResponseCompression();      //����ѹ��������iisѹ��
            services.AddHttpContextAccessor();   //HttpContext

            services.AddControllers().AddNewtonsoftJson();
            //֧�ֿ���
            services.AddCors(options =>
            {
                options.AddPolicy("any", b =>
                {
                    b.AllowAnyOrigin() //�����κ���Դ����������
                    .AllowAnyMethod()
                    .AllowAnyHeader();  //ָ������cookie

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
                    c.IncludeXmlComments(xml.FullName, true);        //��ӿ�������ע�ͣ�true��ʾ��ʾ������ע�ͣ�

                c.CustomSchemaIds(x => x.FullName);    //��ֹ����
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
            //Authentication������authorizationǰ��Authorization������Routing��Endpoints֮��
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapDefaultControllerRoute();
                endpoints.MapControllerRoute("ActionApi", "api/{controller=Home}/{action=Index}/{id?}");
                //endpoints.MapHealthChecks("/health", new HealthCheckOptions() { });     //��ص�ַ
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
