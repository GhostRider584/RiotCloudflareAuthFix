# RiotCloudflareAuthFix

Mimicking TLS handshake and headers of Riot Client to bypass CloudFlare restrictions

## Headers

| Name | Value |
| --- | --- |
| User-Agent | RiotClient/51.0.0.4429735.4381201 rso-auth (Windows;10;;Professional, x64) |
| X-Curl-Source | Api |

## TLS Versions
- TLS 1.3
- TLS 1.2
- TLS 1.1
- TLS 1.0

## Cipher Suites
- TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384
- TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384
- TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256
- TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256
- TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256
- TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256
- TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384
- TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384
- TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA
- TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA
- TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA
- TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA
- TLS_RSA_WITH_AES_128_GCM_SHA256
- TLS_RSA_WITH_AES_256_GCM_SHA384
- TLS_RSA_WITH_AES_128_CBC_SHA
- TLS_RSA_WITH_AES_256_CBC_SHA
- TLS_RSA_WITH_3DES_EDE_CBC_SHA
- TLS_EMPTY_RENEGOTIATION_INFO_SCSV

## Signature Algorithms

- ECDSA with SHA-256
- RSASSA-PKCS1-v1_5 with SHA-256
- RSASSA-PKCS1-v1_5 with SHA-384
- ECDSA with SHA-384
- RSASSA-PKCS1-v1_5 with SHA-512
- RSASSA-PKCS1-v1_5 with SHA-1

## Groups
- secp256r1 (23)
- secp384r1 (24)
- secp521r1 (25)