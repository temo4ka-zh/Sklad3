using NUnit.Framework;
using Sklad1.Models; 
using System;

namespace Sklad1.Tests.Models
{
    [TestFixture]
    public class CurrencyRateTests
    {
        [Test]
        public void Properties_SetCorrectly()
        {
            // Arrange & Act
            var rateInfo = new CurrencyRate
            {
                Id = Guid.NewGuid(),
                Code = "USD",
                RateToRub = 100.50m,
                UpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.AreEqual("USD", rateInfo.Code);
            Assert.AreEqual(100.50m, rateInfo.RateToRub);
        }

        [Test]
        public void DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var rateInfo = new CurrencyRate();

            // Assert
            Assert.IsNull(rateInfo.Code);
            Assert.AreEqual(0, rateInfo.RateToRub);
        }
    }
}