using NUnit.Framework;
using Sklad1.Models;
using System;

namespace Sklad1.Tests.Helpers
{
    [TestFixture]
    public class ShipmentItemTempTests
    {
        [Test]
        public void Test_ShipmentItem_Properties_Work_Correctly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var shipmentId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            // Act
            var item = new ShipmentItem
            {
                Id = id,
                ShipmentId = shipmentId,
                ProductId = productId,
                Quantity = 15,
                PriceAtShipment = 1250.50m,
                CostAtShipment = 900.00m
            };

            // Assert
            Assert.AreEqual(id, item.Id);
            Assert.AreEqual(shipmentId, item.ShipmentId);
            Assert.AreEqual(productId, item.ProductId);
            Assert.AreEqual(15, item.Quantity);
            Assert.AreEqual(1250.50m, item.PriceAtShipment);
            Assert.AreEqual(900.00m, item.CostAtShipment);
        }
    }
}