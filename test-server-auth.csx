// dotnet-script test-server-auth.csx
// Bearer JWT ile sunucuya change-set endpoint testi
#r "src/ErpBridge.CentralApi/bin/Debug/net10.0/ErpBridge.CentralApi.dll"
#r "src/ErpBridge.Shared/bin/Debug/net10.0/ErpBridge.Shared.dll"
#r "nuget: System.IdentityModel.Tokens.Jwt, 7.0.3"
#r "nuget: Microsoft.IdentityModel.Tokens, 7.0.3"

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var secret = "REPLACE_WITH_PROD_JWT_SIGNING_KEY";
var tenantId = Guid.NewGuid();
var agentId = Guid.NewGuid();
var scope = "agent";
var issuer = "erpb";
var audience = "erpb";

var handler = new JwtSecurityTokenHandler();
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
var token = new JwtSecurityToken(
    issuer: issuer,
    audience: audience,
    claims: new[] { new Claim("sub", agentId.ToString()), new Claim("scope", scope), new Claim("tenant", tenantId.ToString()) },
    expires: DateTime.UtcNow.AddHours(1),
    signingCredentials: creds);

var jwt = handler.WriteToken(token);
Console.WriteLine($"Bearer: {jwt[..30]}...");

using var client = new HttpClient { BaseAddress = new Uri("https://lisans.appsgo.cloud/") };
var req = new HttpRequestMessage(HttpMethod.Post, "api/v1/ingest/changeset");
req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
req.Headers.Add("X-Tenant-Id", tenantId.ToString());
req.Content = new StringContent("{\"tenantId\":\"" + tenantId + "\",\"sourceDatabase\":\"TEST\",\"pulledAtUtc\":\"2026-09-08T08:00:00Z\",\"tables\":[]}", Encoding.UTF8, "application/json");

var resp = await client.SendAsync(req);
Console.WriteLine($"Status: {(int)resp.StatusCode} {resp.StatusCode}");
Console.WriteLine($"Body: {await resp.Content.ReadAsStringAsync()}");
