using NUnit.Framework;
using Sklad1.Models; 
using System;

namespace Sklad1.Tests.Models
{
    [TestFixture]
    public class CategoryTests
    {
        [Test]
        public void Category_Properties_SetCorrectly()
        {
            // Arrange & Act
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Винкс",
                Description = "Мультфильм"
            };

            // Assert
            Assert.AreEqual("Винкс", category.Name);
            Assert.AreEqual("Мультфильм", category.Description);
        }

        [Test]
        public void Category_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var category = new Category();

            // Assert
            Assert.AreEqual(string.Empty, category.Name);
            Assert.AreEqual(string.Empty, category.Description);
        }
    }
}