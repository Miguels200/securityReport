using System;
using Microsoft.AspNetCore.Identity;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Infrastructure.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly PasswordHasher<object> _hasher = new PasswordHasher<object>();

        public string Hash(string password)
        {
            return _hasher.HashPassword(null, password);
        }

        public int Verify(string hash, string password)
        {
            var res = _hasher.VerifyHashedPassword(null, hash, password);
            return res == PasswordVerificationResult.Success ? 1 : 0;
        }
    }
}