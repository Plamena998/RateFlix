using System.Text.Json;

namespace RateFlix.Middleware
{
    public class IPBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IPBlacklistMiddleware> _logger;
        private readonly string _blacklistFilePath;
        private readonly string _accessDeniedPagePath;

        private HashSet<string> _blacklistedIPs = new();
        private DateTime _lastFileCheck = DateTime.MinValue;

        // Cache HTML page in memory
        private string? _accessDeniedHtml;

        public IPBlacklistMiddleware(
            RequestDelegate next,
            ILogger<IPBlacklistMiddleware> logger,
            IWebHostEnvironment env,
            string accessDeniedPagePath)
        {
            _next = next;
            _logger = logger;

            _blacklistFilePath = Path.Combine(env.ContentRootPath, "blacklisted-ips.json");

            // Resolve absolute path safely
            _accessDeniedPagePath = Path.IsPathRooted(accessDeniedPagePath)
                ? accessDeniedPagePath
                : Path.Combine(env.WebRootPath, accessDeniedPagePath);

            LoadBlacklistedIPs();
            LoadAccessDeniedPage();
        }

        private void LoadBlacklistedIPs()
        {
            try
            {
                if (!File.Exists(_blacklistFilePath))
                    return;

                var json = File.ReadAllText(_blacklistFilePath);
                var config = JsonSerializer.Deserialize<IPBlacklistConfig>(json);

                _blacklistedIPs = new HashSet<string>(config?.BlockedIPs ?? []);
                _lastFileCheck = DateTime.UtcNow;

                _logger.LogInformation("Loaded {Count} blacklisted IPs", _blacklistedIPs.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading IP blacklist");
            }
        }

        private void LoadAccessDeniedPage()
        {
            try
            {
                if (File.Exists(_accessDeniedPagePath))
                {
                    _accessDeniedHtml = File.ReadAllText(_accessDeniedPagePath);
                    _logger.LogInformation("Access denied page loaded");
                }
                else
                {
                    _logger.LogWarning("Access denied page not found at {Path}", _accessDeniedPagePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load access denied page");
            }
        }

        private void ReloadIfNeeded()
        {
            if ((DateTime.UtcNow - _lastFileCheck).TotalSeconds > 30)
            {
                LoadBlacklistedIPs();
                LoadAccessDeniedPage();
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            ReloadIfNeeded();

            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            _logger.LogDebug("Incoming request from IP: {IP}", ipAddress);

            if (!string.IsNullOrEmpty(ipAddress) && _blacklistedIPs.Contains(ipAddress))
            {
                _logger.LogWarning("Blocked access from blacklisted IP: {IP}", ipAddress);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "text/html; charset=utf-8";

                await context.Response.WriteAsync(
                    _accessDeniedHtml ??
                    "<h1 style='text-align:center'>Access Denied</h1>"
                );

                return;
            }

            await _next(context);
        }
    }

    public class IPBlacklistConfig
    {
        public List<string> BlockedIPs { get; set; } = new();
    }
}
