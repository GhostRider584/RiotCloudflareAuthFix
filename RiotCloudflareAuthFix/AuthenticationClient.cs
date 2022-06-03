using Org.BouncyCastle.Tls;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RiotCloudflareAuthFix {
	public class AuthenticationClient {
		
		public static async Task<HttpResponseMessage> RequestAsync(
			HttpRequestMessage requestMessage,
			DecompressionMethods decompressionMethods = DecompressionMethods.None
		) {
			var host = requestMessage.RequestUri!.Host;
			using var client = new TcpClient(host, 443);

			var protocol = new TlsClientProtocol(client.GetStream());
			protocol.Connect(new AuthenticationTlsClient {
				ServerNames = new[] { host }
			});

			var content = CreateContent(requestMessage, decompressionMethods);

			using var stream = protocol.Stream;
			await stream.WriteAsync(content);

			using var responseStream = new MemoryStream();
			try {
				await stream.CopyToAsync(responseStream);
			} catch (TlsNoCloseNotifyException) {
				// sometimes this gets thrown but its all good
			}

			var data = responseStream.ToArray();
			
			responseStream.Position = 0;
			var responseMessage = ParseResponse(responseStream);
			
			responseMessage.RequestMessage = requestMessage;
			return responseMessage;
		}

		private static byte[] CreateContent(HttpRequestMessage requestMessage, DecompressionMethods decompressionMethods) {
			var builder = new StringBuilder();
			builder.AppendLine($"{requestMessage.Method} {requestMessage.RequestUri!.PathAndQuery} HTTP/1.1");
			builder.AppendLine($"Host: {requestMessage.RequestUri!.Host}");
			builder.AppendLine("Connection: close");

			var encodings = string.Join(", ", GetUniqueFlags(decompressionMethods)
				.Where(m => m != DecompressionMethods.None)
				.Select(GetDecompressionMethodName));

			if (!string.IsNullOrEmpty(encodings)) {
				builder.AppendLine($"Accept-Encoding: {encodings}");
			}

			if (requestMessage.Content != null) {
				builder.AppendLine($"Content-Length: {requestMessage.Content.ReadAsStringAsync().Result.Length}");
				builder.Append(requestMessage.Content.Headers.ToString());
			}

			if (requestMessage.Headers != null) {
				builder.Append(requestMessage.Headers.ToString());
			}

			builder.AppendLine();

			if (requestMessage.Content != null) {
				builder.Append(requestMessage.Content?.ReadAsStringAsync().Result);
			}

			//Console.WriteLine(builder.ToString());
			return Encoding.UTF8.GetBytes(builder.ToString());
		}

		private static HttpResponseMessage ParseResponse(MemoryStream responseStream) {
			responseStream.Position = 0;
			var data = responseStream.ToArray();
			
			var index = BinaryMatch(data, Encoding.UTF8.GetBytes("\r\n\r\n")) + 4;
			var headerLines = Encoding.UTF8.GetString(data, 0, index).Split("\r\n");
			responseStream.Position = index;

			var statusLine = headerLines[0].Split(' ');
			var versionStr = statusLine[0].Split('/')[1].Split('.');
			var version = new Version(int.Parse(versionStr[0]), int.Parse(versionStr[1]));
			var statusCode = (HttpStatusCode)int.Parse(statusLine[1]);

			var responseHeaders = headerLines.Skip(1)
				.Select(x => x.Split(':'))
				.Where(x => !string.IsNullOrWhiteSpace(x[0]))
				.GroupBy(x => x[0])
				.ToDictionary(
					x => x.Key.Trim(),
					x => x.FirstOrDefault()?.Length > 1 ? x.FirstOrDefault()?[1].Trim() : null
				);

			var responseMessage = new HttpResponseMessage {
				StatusCode = statusCode,
				Version = version
			};

			if (responseHeaders.ContainsKey("Transfer-Encoding") && responseHeaders["Transfer-Encoding"] == "chunked") {
				var outputStream = new MemoryStream();

				// extremely disgusting and hacky way to do it because idc ඞ
				// this wont work if content encoding is present
				var strParts = Encoding.UTF8.GetString(data, index, data.Length - index).Split("\r\n");
				for (int i = 1; i < strParts.Length; i += 2) {
					outputStream.Write(Encoding.UTF8.GetBytes(strParts[i]));
				}
				
				outputStream.Seek(0, SeekOrigin.Begin);
				responseMessage.Content = ParseResponseContent(responseHeaders.GetValueOrDefault("Content-Encoding", null), outputStream, 0);
			} else {
				responseMessage.Content = ParseResponseContent(responseHeaders.GetValueOrDefault("Content-Encoding", null), responseStream, index);
			}

			foreach (var header in responseHeaders) {
				responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			return responseMessage;
		}

		private static HttpContent ParseResponseContent(string? contentEncoding, MemoryStream stream, int contentIndex) {
			if (contentEncoding == null) {
				var data = stream.ToArray();
				var contentStr = Encoding.UTF8.GetString(data, contentIndex, data.Length - contentIndex);
				return new StringContent(contentStr);
			} else {
				Console.WriteLine(contentEncoding);

				Stream? decompressionStream = contentEncoding switch {
					"gzip" => new GZipStream(stream, CompressionMode.Decompress),
					"br" => new BrotliStream(stream, CompressionMode.Decompress),
					"deflate" => new DeflateStream(stream, CompressionMode.Decompress),
					_ => throw new NotSupportedException($"Unsupported Content-Encoding: {contentEncoding}")
				};

				using var decompressedMemory = new MemoryStream();
				decompressionStream.CopyTo(decompressedMemory);
				decompressionStream.Close();

				var content = Encoding.UTF8.GetString(decompressedMemory.ToArray());
				return new StringContent(content);
			}
		}
		
		private static string? GetDecompressionMethodName(DecompressionMethods method) => method switch {
			DecompressionMethods.GZip => "gzip",
			DecompressionMethods.Deflate => "deflate",
			DecompressionMethods.Brotli => "br",
			DecompressionMethods.All => "*",
			DecompressionMethods.None => null
		};

		private static IEnumerable<T> GetUniqueFlags<T>(T flags) where T : Enum {
			foreach (Enum value in Enum.GetValues(flags.GetType())) {
				if (flags.HasFlag(value)) {
					yield return (T)value;
				}
			}
		}

		private static int BinaryMatch(byte[] input, byte[] pattern) {
			int sLen = input.Length - pattern.Length + 1;
			for (int i = 0; i < sLen; ++i) {
				bool match = true;
				for (int j = 0; j < pattern.Length; ++j) {
					if (input[i + j] != pattern[j]) {
						match = false;
						break;
					}
				}
				if (match) {
					return i;
				}
			}
			return -1;
		}
	}
}
