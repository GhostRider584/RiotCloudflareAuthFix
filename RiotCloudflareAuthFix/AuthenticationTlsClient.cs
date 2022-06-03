using System.Collections;
using System.Text;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;

namespace RiotCloudflareAuthFix {
	public class AuthenticationTlsClient : DefaultTlsClient {
		private class EmptyTlsAuthentication : TlsAuthentication {
			public TlsCredentials? GetClientCredentials(CertificateRequest certificateRequest) => null;
			public void NotifyServerCertificate(TlsServerCertificate serverCertificate) { }
		}

		private ServerName[]? _serverNames;
		public string[]? ServerNames {
			set {
				if (value == null) {
					_serverNames = null;
				} else {
					_serverNames = value.Select(x => new ServerName(NameType.host_name, Encoding.ASCII.GetBytes(x))).ToArray();
				}
			}
		}

		public IList<SignatureAndHashAlgorithm> SignatureAlgorithms { get; set; } = new[] {
			CreateSignatureAlgorithm(SignatureScheme.ecdsa_secp256r1_sha256),
			CreateSignatureAlgorithm(SignatureScheme.rsa_pss_rsae_sha256),
			CreateSignatureAlgorithm(SignatureScheme.rsa_pkcs1_sha256),
			CreateSignatureAlgorithm(SignatureScheme.ecdsa_secp384r1_sha384),
			CreateSignatureAlgorithm(SignatureScheme.rsa_pss_rsae_sha384),
			CreateSignatureAlgorithm(SignatureScheme.rsa_pkcs1_sha384),
			CreateSignatureAlgorithm(SignatureScheme.rsa_pss_rsae_sha512),
			CreateSignatureAlgorithm(SignatureScheme.rsa_pkcs1_sha512),
			CreateSignatureAlgorithm(SignatureScheme.rsa_pkcs1_sha1),
		};

		public int[] SupportedCiphers { get; set; } = new[] {
			CipherSuite.TLS_CHACHA20_POLY1305_SHA256,
			CipherSuite.TLS_AES_128_GCM_SHA256,
			CipherSuite.TLS_AES_256_GCM_SHA384,
			CipherSuite.TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256,
			CipherSuite.TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256,
			CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
			CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
			CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,
			CipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
			CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA,
			CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,
			CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA,
			CipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,
			CipherSuite.TLS_RSA_WITH_AES_128_GCM_SHA256,
			CipherSuite.TLS_RSA_WITH_AES_256_GCM_SHA384,
			CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA,
			CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA
		};

		public int[] SupportedGroups { get; set; } = new[] {
			NamedGroup.x25519,
			NamedGroup.secp256r1,
			NamedGroup.secp384r1,
			NamedGroup.secp521r1,
			NamedGroup.x448
		};

		public ProtocolVersion[] SupportedVersions { get; set; } = ProtocolVersion.TLSv13.DownTo(ProtocolVersion.TLSv12);

		public AuthenticationTlsClient() : base(new BcTlsCrypto(new SecureRandom())) {
		}

		public override TlsAuthentication GetAuthentication() => new EmptyTlsAuthentication();
		protected override ProtocolVersion[] GetSupportedVersions() => SupportedVersions;
		protected override IList GetSupportedSignatureAlgorithms() => (IList)SignatureAlgorithms;
		protected override int[] GetSupportedCipherSuites() => SupportedCiphers;
		protected override IList? GetSniServerNames() => _serverNames;

		protected override IList GetSupportedGroups(IList namedGroupRoles) {
			var supportedGroups = new ArrayList();
			TlsUtilities.AddIfSupported(supportedGroups, Crypto, SupportedGroups);
			return supportedGroups;
		}

		private static SignatureAndHashAlgorithm CreateSignatureAlgorithm(int signatureScheme) {
			short hashAlgorithm = SignatureScheme.GetHashAlgorithm(signatureScheme);
			short signatureAlgorithm = SignatureScheme.GetSignatureAlgorithm(signatureScheme);
			return new(hashAlgorithm, signatureAlgorithm);
		}
	}
}
