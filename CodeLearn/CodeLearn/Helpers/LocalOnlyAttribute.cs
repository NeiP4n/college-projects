using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CodeLearn.Helpers
{
    /// <summary>
    /// Разрешает доступ к контроллеру/action только с loopback-адреса (127.0.0.1 / ::1).
    /// Все остальные получают 404 — Builder как будто не существует.
    /// </summary>
    public class LocalOnlyAttribute : ActionFilterAttribute
    {
        // Заголовки, которые добавляет reverse-proxy. Если хоть один присутствует —
        // запрос пришёл НЕ напрямую, а через прокси (который может сидеть на loopback),
        // поэтому RemoteIpAddress=127.0.0.1 доверять нельзя.
        private static readonly string[] _proxyHeaders =
        {
            "X-Forwarded-For", "X-Real-IP", "Forwarded",
            "X-Forwarded-Host", "X-Original-For"
        };

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var ip = context.HttpContext.Connection.RemoteIpAddress;

            // IPv4-mapped IPv6 (::ffff:127.0.0.1) → нормализуем к IPv4
            if (ip != null && ip.IsIPv4MappedToIPv6)
                ip = ip.MapToIPv4();

            var headers = context.HttpContext.Request.Headers;
            var viaProxy = false;
            foreach (var h in _proxyHeaders)
                if (headers.ContainsKey(h)) { viaProxy = true; break; }

            // Доступ только с настоящего loopback и без признаков прокси
            if (ip == null || !IPAddress.IsLoopback(ip) || viaProxy)
            {
                context.Result = new NotFoundResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
