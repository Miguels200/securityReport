using System;
using Xunit;
using SecurityReport.Infrastructure.Services;

namespace Tests.Unit
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashAndVerifyPassword()
        {
            var service = new PasswordHasherService();
            var pwd = "My$ecureP@ssw0rd";
            var hash = service.Hash(pwd);
            var res = service.Verify(hash, pwd);
            Assert.Equal(1, res);
        }
    }
}