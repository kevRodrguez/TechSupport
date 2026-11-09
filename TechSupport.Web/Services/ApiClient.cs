using System.Net.Http.Headers;
using System.Text.Json;

namespace TechSupport.Web.Services;

public class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _http;

    public ApiClient(IHttpClientFactory factory, IHttpContextAccessor http)
    {
        _factory = factory;
        _http = http;
    }

    public Task<ApiResult<LoginResponse>> LoginAsync(string nombreUsuario, string password) =>
        SendAsync<LoginResponse>(HttpMethod.Post, "api/auth/login", new { nombreUsuario, password }, withToken: false);

    public Task<ApiResult<List<IncidenteDto>>> ListarAsync() =>
        SendAsync<List<IncidenteDto>>(HttpMethod.Get, "api/incidentes");

    public Task<ApiResult<IncidenteDto>> ObtenerAsync(int id) =>
        SendAsync<IncidenteDto>(HttpMethod.Get, $"api/incidentes/{id}");

    public Task<ApiResult<IncidenteDto>> CrearAsync(string titulo, string? descripcion, string prioridad) =>
        SendAsync<IncidenteDto>(HttpMethod.Post, "api/incidentes", new { titulo, descripcion, prioridad });

    public Task<ApiResult<IncidenteDto>> AsignarAsync(int id, int tecnicoId) =>
        SendAsync<IncidenteDto>(HttpMethod.Put, $"api/incidentes/{id}/asignar", new { tecnicoId });

    public Task<ApiResult<IncidenteDto>> CambiarEstadoAsync(int id, string estado) =>
        SendAsync<IncidenteDto>(HttpMethod.Put, $"api/incidentes/{id}/estado", new { estado });

    public Task<ApiResult<bool>> EliminarAsync(int id) =>
        SendAsync<bool>(HttpMethod.Delete, $"api/incidentes/{id}");

    public Task<ApiResult<List<TecnicoDto>>> TecnicosAsync() =>
        SendAsync<List<TecnicoDto>>(HttpMethod.Get, "api/usuarios/tecnicos");

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body = null, bool withToken = true)
    {
        try
        {
            var request = new HttpRequestMessage(method, url);
            if (body is not null)
                request.Content = JsonContent.Create(body);

            using var response = await Create(withToken).SendAsync(request);
            var raw = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return ApiResult<T>.Fail((int)response.StatusCode, ReadMessage(raw));

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent || string.IsNullOrWhiteSpace(raw))
                return ApiResult<T>.Ok(default);

            var data = JsonSerializer.Deserialize<T>(raw, JsonOptions);
            return ApiResult<T>.Ok(data);
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Fail(0, "No se pudo contactar la API. Verifique que esté en ejecución en http://localhost:5080.");
        }
    }

    private HttpClient Create(bool withToken)
    {
        var client = _factory.CreateClient("Api");
        if (!withToken)
            return client;

        var token = _http.HttpContext?.Session.GetString("Jwt");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private static string ReadMessage(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "La operación fue rechazada.";

        try
        {
            using var document = JsonDocument.Parse(raw);
            if (document.RootElement.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String)
            {
                return message.GetString() ?? "La operación fue rechazada.";
            }
        }
        catch (JsonException)
        {
        }

        return "La operación fue rechazada.";
    }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;
}

public class IncidenteDto
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string Prioridad { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public int UsuarioId { get; set; }

    public string ReportadoPor { get; set; } = string.Empty;

    public int? TecnicoId { get; set; }

    public string? Tecnico { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaResolucion { get; set; }
}

public class TecnicoDto
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;
}
