using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace MFAWebDemo
{
    public static class Program
    {
        /// <summary>
        /// �������
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.ConfigureServices(builder);
            builder.Services.AddInjection(builder);
            var app = builder.Build();
            app.Configure(builder);
            app.Run();
        }

        /// <summary>
        /// �÷���ͨ������ʱ����
        /// ʹ�ô˷������������ӵ�������
        /// </summary>
        /// <param name="services">����</param>
        /// <param name="builder">��վ����</param>
        public static void ConfigureServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            //������������ĸ��Сд
            services.AddMvc().AddJsonOptions(options =>
            {
                //�����������ֲ���
                //PropertyNamingPolicy = null Ĭ�ϲ��ı�
                //PropertyNamingPolicy = JsonNamingPolicy.CamelCase Ĭ��Сд
                //https://docs.microsoft.com/zh-cn/dotnet/api/system.text.json.jsonserializeroptions.propertynamingpolicy?view=net-6.0
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                //�������л�
                //options.JsonSerializerOptions.Converters.Add(new DateTimeJson());
                //ȡ�� Unicode ����
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                //��ֵ������ǰ��
                //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                //�����������
                //options.JsonSerializerOptions.AllowTrailingCommas = true;
                //�����л����������������Ƿ�ʹ�ò����ִ�Сд�ıȽ�
                //options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
            });
            //���û��湦��
            services.AddMemoryCache();
            //���� Session
            services.AddSession(options =>
            {
                options.Cookie.Name = ".AspNetCore.Session";//����Session��Cookie��Key
                options.IdleTimeout = TimeSpan.FromMinutes(20);//����Session�Ĺ���ʱ��
                options.Cookie.HttpOnly = true;//���������������ͨ��js��ø�Cookie��ֵ
                options.Cookie.IsEssential = true;
            });
            //���� Options ģʽ
            services.AddOptions();
            //���� MVC
            services.AddMvc();
        }

        /// <summary>
        /// �÷���ͨ������ʱ����
        /// ʹ�ô˷�������HTTP������ˮ��
        /// </summary>
        /// <param name="app">Ӧ��</param>
        /// <param name="builder">��վ����</param>
        public static void Configure(this WebApplication app, WebApplicationBuilder builder)
        {
            //����ģʽ
            if (app.Environment.IsDevelopment())
            {
                //��������չʾ�����ջҳ
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //��ʽ�����Զ������ҳ
                app.UseExceptionHandler("/Help/Error");
                //����ȫ�ֵ�����
                app.Use(async (context, next) =>
                {
                    await next();
                    //401 ����
                    if (context.Response.StatusCode == 401)
                    {
                        context.Request.Path = "/Admin/Index";
                        //await next();
                    }
                    //404 ����
                    if (context.Response.StatusCode == 404)
                    {
                        context.Request.Path = "/Help/Error";
                        //await next();
                    }
                    //500 ����
                    if (context.Response.StatusCode == 500)
                    {
                        context.Request.Path = "/Help/Error";
                        //await next();
                    }
                });
            }
            //Ĭ�ϵľ�̬Ŀ¼·��
            app.UseStaticFiles();
            //�û�·��
            app.UseRouting();
            //�û���Ȩ
            app.UseAuthentication();
            app.UseAuthorization();
            //�û� Session
            app.UseSession();
            //�û�Ĭ��·��
            app.MapControllerRoute(
                name: "areaRoute",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        }

        /// <summary>
        /// ����ע��
        /// </summary>
        /// <param name="services">����</param>
        /// <param name="builder">��վ����</param>
        public static void AddInjection(this IServiceCollection services, WebApplicationBuilder builder)
        {
            //���� HttpContext ��ȡ��
            services.AddHttpContextAccessor();
        }
    }
}