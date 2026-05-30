using NUnit.Framework;
using Sklad1.Models;

namespace Sklad1.Tests.Models
{
    [TestFixture]
    public class ProductItemTests
    {
        [Test]
        public void Test_ProductItem_Properties_Work()
        {
            // Arrange & Act
            var dropDownItem = new ProductItem
            {
                Article = "SHP-102",
                Name = "Коробка картонная",
                Quantity = 45,
                PurchasePrice = 12.00m
            };

            // Assert
            Assert.AreEqual("SHP-102", dropDownItem.Article);
            Assert.AreEqual("Коробка картонная", dropDownItem.Name);
            Assert.AreEqual(45, dropDownItem.Quantity);
            Assert.AreEqual(12.00m, dropDownItem.PurchasePrice);
        }
    }
}