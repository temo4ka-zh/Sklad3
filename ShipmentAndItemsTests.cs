using NUnit.Framework;
using Sklad1.Models;
using System;

namespace Sklad1.Tests.Models
{
    [TestFixture]
    public class ProductBatchTests
    {
        [Test]
        public void Test_BatchStatus_DefaultIsActive()
        {
            // Arrange & Act
            var batch = new ProductBatch();

            // Assert 
            Assert.AreEqual("Активен", batch.Status);
        }

        [Test]
        public void Test_ExpiryDate_Is_Correct()
        {
            // Arrange
            var b = new ProductBatch();
            DateTime today = DateTime.Now;

            // Act
            b.ExpiryDate = today;

            // Assert
            Assert.AreEqual(today, b.ExpiryDate);
        }
    }
}