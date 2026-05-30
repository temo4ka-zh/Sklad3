using NUnit.Framework;
using Sklad1.Data;
using Sklad1.Models;
using System;

namespace Sklad1.Tests.Data
{
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void Test_User_Properties_And_Role_Assignment()
        {
            // Arrange
            var newId = Guid.NewGuid();

            // Act
            var user = new User
            {
                Id = newId,
                FirstName = "Иван",
                LastName = "Иванов",
                Email = "ivan@mail.ru",
                PasswordHash = "qwerty123456", 
                Role = UserRole.Storekeeper 
            };

            // Assert
            Assert.AreEqual(newId, user.Id);
            Assert.AreEqual("Иван", user.FirstName);
            Assert.AreEqual("Иванов", user.LastName);
            Assert.AreEqual("ivan@mail.ru", user.Email);
            Assert.AreEqual("qwerty123456", user.PasswordHash);
            Assert.AreEqual(UserRole.Storekeeper, user.Role);
        }
    }
}