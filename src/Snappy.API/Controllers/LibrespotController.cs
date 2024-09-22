using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Zeroconf;

namespace Snappy.API.Controllers
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Snappy.Services;

    [ApiController]
    [Route("[controller]")]
    public class LibrespotController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ILibrespotService _librespotService;

        public LibrespotController(ILogger logger,
            ILibrespotService librespotService
            )
        {
            _logger = logger;
            _librespotService = librespotService;
        }

        [HttpGet]
        [Route("GetZeroconfDevices")]
        public async Task<ActionResult<IReadOnlyList<IZeroconfHost>>> GetSpotifyZeroConfHostsAsync()
        {
            var zeroconfHosts = await _librespotService.GetSpotifyZeroConfHostsAsync();
            return Ok(zeroconfHosts);
        }


        [HttpGet]
        [Route("GetZeroconfDevice/{displayName}")]
        public async Task<ActionResult<IZeroconfHost>> GetSpotifyZeroConfHostAsync(string displayName)
        {
            IZeroconfHost zeroconfHost;
            try
            {
                var zeroconfHosts = await _librespotService.GetSpotifyZeroConfHostsAsync();
                zeroconfHost = zeroconfHosts.Where(h => h.DisplayName == displayName).First();
            }
            catch (System.Exception ex)
            {
                _logger.Error($"GetSpotifyZeroConfHostAsync/{displayName} - {ex}", displayName, ex);
                return BadRequest(ex.ToString());
            }

            return Ok(zeroconfHost);
        }

        [HttpGet]
        [Route("GetInfo/{port}")]
        public async Task<ActionResult<IReadOnlyList<IZeroconfHost>>> GetInfoAsync(int port)
        {
            var librespotInfo = await _librespotService.GetInfoAsync(port);

            return Ok(librespotInfo);
        }

        [HttpPost]
        [Route("AddUser/{port}")]
        public async Task<ActionResult<IReadOnlyList<IZeroconfHost>>> AddUserAsync(int port)
        {
            var clientKey = "1anWY1uRYyGOy/Nj/2E5WxmxUPba9/E02TmpKbUJ+VTPgxWlXG3k3qBDYVdrrLYonQ+KPdBEzm8KnQfIP/3lCxr2GUCB3Nu3/2AMG+ZR1haYWZcG6qU7rCWGb9zrsv46";
            var clientKeyBase64 = Convert.FromBase64String(clientKey);

            var encryptedMessage = Encoding.UTF8.GetBytes("/4X6yexJSTq2tbXEIB6F+QG+szAfd81eULSBhPHzosAWMV+cP758PPCyaT7aOInfJUgJ+Ffq2qW6zAdIcTaKo2FzKIZ8w+9SFBhFxuC6J2+zZEkDypWlVyYoX5N+FdWnUyNWIx9mwdzRKs9Fqf63uQlLKl5lBDJonYUCXFmfR8IjuTgYHuQiEoo1/+vsCnUfwwV+88ouHOIJB40DM7tE4iokFOv38sRbuxA5BPPtQY/OWwVu+Uzc/l96HGzXnuRFfY3NSC9Flv0vpwjlP9z6u68qZMSE06ga4TzxnPAFBXX5q6YIwnZpHIYLxuQ0F+cvr1gXc2qURNYBjlQu");

            // var encryptedMessage = Encrypt(clientKey, "");
            var aes = new AesCryptoServiceProvider();

            var decryptedMessage = Decrypt(clientKeyBase64, encryptedMessage, aes.IV);

            return Ok(decryptedMessage);
        }

        private string Decrypt(byte[] publicKey, byte[] encryptedMessage, byte[] iv)
        {
            string decryptedMessage;

            var aes = new AesCryptoServiceProvider();

            var diffieHellman = new ECDiffieHellmanCng
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.Sha256
            };

            try
            {
                var key = CngKey.Import(publicKey, CngKeyBlobFormat.EccFullPublicBlob);
                var derivedKey = diffieHellman.DeriveKeyMaterial(key);

                aes.Key = derivedKey;
                aes.IV = iv;

                using (var plainText = new MemoryStream())
                {
                    using (var decryptor = aes.CreateDecryptor())
                    {
                        using (var cryptoStream = new CryptoStream(plainText, decryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(encryptedMessage, 0, encryptedMessage.Length);
                        }
                    }

                    decryptedMessage = Encoding.UTF8.GetString(plainText.ToArray());
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }


            return decryptedMessage;
        }


        private byte[] Encrypt(byte[] publicKey, string secretMessage)
        {
            var aes = new AesCryptoServiceProvider();

            var diffieHellman = new ECDiffieHellmanCng
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.Sha256
            };

            byte[] encryptedMessage;
            var key = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob);
            var derivedKey = diffieHellman.DeriveKeyMaterial(key); // "Common secret"

            aes.Key = derivedKey;

            using (var cipherText = new MemoryStream())
            {
                using (var encryptor = aes.CreateEncryptor())
                {
                    using (var cryptoStream = new CryptoStream(cipherText, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] ciphertextMessage = Encoding.UTF8.GetBytes(secretMessage);
                        cryptoStream.Write(ciphertextMessage, 0, ciphertextMessage.Length);
                    }
                }

                encryptedMessage = cipherText.ToArray();
            }

            return encryptedMessage;
        }
    }
}