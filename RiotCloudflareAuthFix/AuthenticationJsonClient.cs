using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RiotCloudflareAuthFix {
	public record RequestResult<T>(HttpResponseMessage Message, T? Value) where T : class {
		
	}

	public partial class AuthenticationJsonClient {
		public DecompressionMethods DecompressionMethods { get; set; } = DecompressionMethods.None;
		public Dictionary<string, string?> DefaultRequestHeaders { get; } = new();
		public JsonSerializerOptions? SerializerOptions { get; set; }

		#region Send methods

		public async Task<RequestResult<T>> SendAsync<T>(
			HttpMethod method,
			Uri? requestUri,
			object? body = null,
			IDictionary<string, string?>? headers = null,
			IEnumerable<Cookie>? cookies = null,
			JsonSerializerOptions? serializerOptions = null
		) where T : class {

			var message = CreateRequestMessage(method, requestUri, body, headers, cookies, serializerOptions);
			return await SendAsync<T>(message, serializerOptions);
		}

		public async Task<RequestResult<T>> SendAsync<T>(
			HttpRequestMessage message,
			JsonSerializerOptions? serializerOptions = null
		) where T : class {
			var response = await AuthenticationClient.RequestAsync(message, DecompressionMethods);

			try {
				var result = await JsonSerializer.DeserializeAsync<T>(
					await response.Content.ReadAsStreamAsync(),
					serializerOptions ?? SerializerOptions
				);

				return new(response, result);
			} catch {
				return new(response, null);
			}
		}

		#endregion

		#region Common http methods

		public async Task<RequestResult<T>> GetAsync<T>(
			Uri? requestUri,
			object? body = null,
			IDictionary<string, string?>? headers = null,
			IEnumerable<Cookie>? cookies = null,
			JsonSerializerOptions? serializerOptions = null
		) where T : class =>
			await SendAsync<T>(HttpMethod.Get, requestUri, body, headers, cookies, serializerOptions);

		public async Task<RequestResult<T>> PostAsync<T>(
			Uri? requestUri,
			object? body = null,
			IDictionary<string, string?>? headers = null,
			IEnumerable<Cookie>? cookies = null,
			JsonSerializerOptions? serializerOptions = null
		) where T : class =>
			await SendAsync<T>(HttpMethod.Post, requestUri, body, headers, cookies, serializerOptions);

		public async Task<RequestResult<T>> PutAsync<T>(
			Uri? requestUri,
			object? body = null,
			IDictionary<string, string?>? headers = null,
			IEnumerable<Cookie>? cookies = null,
			JsonSerializerOptions? serializerOptions = null
		) where T : class =>
			await SendAsync<T>(HttpMethod.Put, requestUri, body, headers, cookies, serializerOptions);

		public async Task<RequestResult<T>> PatchAsync<T>(
			Uri? requestUri,
			object? body = null,
			IDictionary<string, string?>? headers = null,
			IEnumerable<Cookie>? cookies = null,
			JsonSerializerOptions? serializerOptions = null
		) where T : class =>
			await SendAsync<T>(HttpMethod.Patch, requestUri, body, headers, cookies, serializerOptions);

		public async Task<RequestResult<T>> DeleteAsync<T>(
			Uri? requestUri,
			object? body = null,
			IDictionary<string, string?>? headers = null,
			IEnumerable<Cookie>? cookies = null,
			JsonSerializerOptions? serializerOptions = null
		) where T : class =>
			await SendAsync<T>(HttpMethod.Delete, requestUri, body, headers, cookies, serializerOptions);

		#endregion

		#region Helper methods

		public HttpRequestMessage CreateRequestMessage(
			HttpMethod method,
			Uri? uri,
			object? body = null,
			IDictionary<string, string?>? headers = null,
			IEnumerable<Cookie>? cookies = null,
			JsonSerializerOptions? serializerOptions = null
		) {
			var message = new HttpRequestMessage(method, uri) {
				Version = new Version(1, 1)
			};

			if (body != null) {
				message.Content = JsonContent.Create(body, options: serializerOptions);
			}

			foreach (var header in DefaultRequestHeaders) {
				message.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			if (headers != null) {
				foreach (var (name, value) in headers) {
					message.Headers.TryAddWithoutValidation(name, value);
				}
			}

			if (cookies != null) {
				message.Headers.TryAddWithoutValidation("Cookie", string.Join("; ", cookies));
			}

			return message;
		}

		#endregion
	}
}
