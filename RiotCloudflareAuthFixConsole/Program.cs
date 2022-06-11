using RiotCloudflareAuthFix;
using System.Net;
using System.Net.Http.Headers;

const string username = "Amongi";
const string password = "sussy.baka!";

var client = new AuthenticationJsonClient {
	SerializerOptions = new() { PropertyNameCaseInsensitive = true }
};

client.DefaultRequestHeaders.Add("User-Agent", "RiotClient/44.0.1.4223069.4190634 rso-auth (Windows;10;;Professional, x64)");
client.DefaultRequestHeaders.Add("X-Curl-Source", "Api");

// retrieve the auth cookies
var authCookiesRequestResult = await client.PostAsync<object>(
	new Uri("https://auth.riotgames.com/api/v1/authorization"),
	new {
		client_id = "play-valorant-web-prod",
		redirect_uri = "https://playvalorant.com/opt_in",
		response_type = "token id_token",
		response_mode = "query",
		scope = "account openid",
		nonce = 1,
	}
);

// get those cookies
var authCookies = ParseSetCookie(authCookiesRequestResult.Message.Headers);

// proceed with authorization (you can deserialize it to whatever object you want but this is just an example)
var authRequestResult = await client.PutAsync<object>(
	new Uri("https://auth.riotgames.com/api/v1/authorization"),
	new {
		username,
		password,
		type = "auth"
	},
	cookies: authCookies
);

// we print out the response content
Console.WriteLine(await authRequestResult.Message.Content.ReadAsStringAsync());

// utility method to parse the Set-Cookie header
IEnumerable<Cookie> ParseSetCookie(HttpHeaders headers) {
	if (headers.TryGetValues("Set-Cookie", out var cookies)) {
		return cookies.Select(cookie => cookie.Split('=', 2))
			.Select(cookieParts => new Cookie(cookieParts[0], cookieParts[1]));
	}
	return Enumerable.Empty<Cookie>();
}