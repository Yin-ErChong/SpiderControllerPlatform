using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpiderUtil;
using SpiderUtil.TCP_UDPHelper;
using Microsoft.OpenApi.Models;
using Consul;
using Snai.Mysql.DataAccess.Base;
using Microsoft.EntityFrameworkCore;
using SpiderCore.ServiceInterFace;
using SpiderCore.ServiceImp;

namespace SpiderControllerPlatform
{
    public class Startup
    {
        public static ILoggerRepository repository { get; set; }
        private  IHostingEnvironment _hostingEnvironment { get; set; }
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            
            LogInit();
            TCPInit();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            EFInit(services);
            //Swagger配置
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            services.AddMvc(options => { options.EnableEndpointRouting = false; })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            defaultFilesOptions.DefaultFileNames.Clear();
            defaultFilesOptions.DefaultFileNames.Add("html/index.html");
            app.UseDefaultFiles(defaultFilesOptions);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            // 启用Swagger中间件
            app.UseSwagger();

            // 配置SwaggerUI
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreWebApi");
                //c.RoutePrefix = string.Empty;
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
           // Consul();
        }
        #region 组件配置  
        /// <summary>
        /// EF框架初始化
        /// </summary>
        /// <param name="services"></param>
        public void EFInit(IServiceCollection services)
        {
            services.AddDbContext<DataAccess>(options => options.UseMySQL(Configuration.GetConnectionString("Connection")));
            services.AddScoped<IFirstTestService, FirstTestService>();
        }
        /// <summary>
        /// 日志初始化
        /// </summary>
        public void LogInit()
        {
            // log4net 仓储
            repository = LogManager.CreateRepository("CoreLogRepository");
            dynamic type = (new Program()).GetType();
            string currentDirectory = Path.GetDirectoryName(type.Assembly.Location);
            var fileinfo = new FileInfo(currentDirectory + "\\log4net.config");
            XmlConfigurator.Configure(repository, fileinfo);
            Log4NetRepository.loggerRepository = repository;
        }
        /// <summary>
        /// TCP初始化
        /// </summary>
        public void TCPInit()
        {
            TcpHelper tcpHelper = new TcpHelper();
            tcpHelper.OpenServer(2624);
        }
        /// <summary>
        /// Consul初始化
        /// </summary>
        public void Consul()
        {
            String ip = Configuration["ip"];//部署到不同服务器的时候不能写成127.0.0.1或者0.0.0.0,因为这是让服务消费者调用的地址
            Int32 port = Int32.Parse(Configuration["port"]);
            //向consul注册服务
            ConsulClient client = new ConsulClient(ConfigurationOverview);
            var consulAgent = new AgentServiceRegistration()
            {
                ID = "apiservice1" + Guid.NewGuid(),//服务编号，不能重复，用Guid最简单
                Name = "apiservice1",//服务的名字
                Address = ip,//我的ip地址(可以被其他应用访问的地址，本地测试可以用127.0.0.1，机房环境中一定要写自己的内网ip地址)
                Port = port,//我的端口
                Check = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务停止多久后反注册
                    Interval = TimeSpan.FromSeconds(10),//健康检查时间间隔，或者称为心跳间隔
                    HTTP = $"https://{ip}:{port}/api/Test/hehe",//健康检查地址,//https://localhost:44371/api/Test
                    Timeout = TimeSpan.FromSeconds(5)
                }
            };
            Task<WriteResult> result = client.Agent.ServiceRegister(consulAgent);
        }
        private static void ConfigurationOverview(ConsulClientConfiguration obj)
        {
            obj.Address = new Uri("http://127.0.0.1:8500");
            obj.Datacenter = "dc1";
        }
        #endregion
    }
}
