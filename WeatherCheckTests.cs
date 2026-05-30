using NUnit.Framework;
using Sklad1.Models;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sklad1.Tests.Models
{
    [TestFixture]
    public class SettingTests
    {
        [Test]
        public void Test_Setting_Properties_Work_Correctly()
        {
            // Arrange
            var settingId = Guid.NewGuid();

            // Act
            var setting = new Setting
            {
                Id = settingId,
                DisplayCurrency = "USD",
                ExpiryWarningDays = 7,
                ExpiryDangerDays = 3
            };

            // Assert
            Assert.AreEqual(settingId, setting.Id);
            Assert.AreEqual("USD", setting.DisplayCurrency);
            Assert.AreEqual(7, setting.ExpiryWarningDays);
            Assert.AreEqual(3, setting.ExpiryDangerDays);
        }
    }
}