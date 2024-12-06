using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class VoucherServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mockMapper;
        private Mock<IVoucherRepository> _mockVoucherRepository;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private VoucherService _service;

        [SetUp]
        public void SetUp()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockVoucherRepository = new Mock<IVoucherRepository>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var mockHttpContext = new Mock<HttpContext>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }));
            mockHttpContext.Setup(ctx => ctx.User).Returns(user);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            _service = new VoucherService(_mockUnitOfWork.Object, _mockMapper.Object, _mockVoucherRepository.Object, _mockHttpContextAccessor.Object);
        }

        [Test]
        public async Task CreateVoucher_ShouldCreateVoucher_WhenValidDTOProvided()
        {
            // Arrange
            var dto = new VoucherDTO { VoucherCode = "TEST", CreateDate = DateTime.MinValue, ExpireDate = DateTime.Now.AddDays(-1) };
            var entity = new Voucher();
            var mappedResult = new VoucherDTO { VoucherCode = "TEST", CreateDate = DateTime.UtcNow };

            _mockMapper.Setup(m => m.Map<Voucher>(dto)).Returns(entity);
            _mockMapper.Setup(m => m.Map<VoucherDTO>(entity)).Returns(mappedResult);
            _mockVoucherRepository.Setup(r => r.AddAsync(entity)).ReturnsAsync(entity);
            _mockUnitOfWork.Setup(u => u.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateVoucher(dto);

            // Assert
            Assert.That(result.VoucherCode, Is.EqualTo("TEST"));
            Assert.That(result.CreateDate != DateTime.MinValue, Is.True);
            _mockVoucherRepository.Verify(r => r.AddAsync(entity), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Test]
        public void DeleteVoucher_ShouldThrowException_WhenVoucherNotFound()
        {
            // Arrange
            _mockVoucherRepository.Setup(r => r.GetByVourcherIdAsync(It.IsAny<int>())).ReturnsAsync((Voucher)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _service.DeleteVoucher(1));
            Assert.That(ex.Message, Is.EqualTo("Voucher not found"));
        }

        [Test]
        public async Task DeleteVoucher_ShouldDeleteVoucher_WhenVoucherFound()
        {
            // Arrange
            var entity = new Voucher { VoucherCode = "TEST" };

            _mockVoucherRepository.Setup(r => r.GetByVourcherIdAsync(It.IsAny<int>())).ReturnsAsync(entity);
            _mockVoucherRepository.Setup(r => r.DeleteAsync(entity));
            _mockUnitOfWork.Setup(u => u.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteVoucher(1);

            // Assert
            _mockVoucherRepository.Verify(r => r.DeleteAsync(entity), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Test]
        public async Task ReceiveVoucher_ShouldUpdateVoucher_WhenValidVoucherIDProvided()
        {
            // Arrange
            var entity = new Voucher { VoucherCode = "TEST", IsValid = true };

            _mockVoucherRepository.Setup(r => r.GetByVourcherIdAsync(1)).ReturnsAsync(entity);
            _mockVoucherRepository.Setup(r => r.UpdateAsync(entity));
            _mockUnitOfWork.Setup(u => u.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.ReceiveVoucher(1);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(entity.UserId, Is.EqualTo("user1"));
            _mockVoucherRepository.Verify(r => r.UpdateAsync(entity), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Test]
        public void ReceiveVoucher_ShouldThrowException_WhenVoucherNotFound()
        {
            // Arrange
            _mockVoucherRepository.Setup(r => r.GetByVourcherIdAsync(It.IsAny<int>())).ReturnsAsync((Voucher)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _service.ReceiveVoucher(1));
            Assert.That(ex.Message, Is.EqualTo("Voucher is not valid or does not exist."));
        }

        [Test]
        public async Task GiveVoucher_ShouldAssignVoucher_WhenValidInputProvided()
        {
            // Arrange
            string receiverID = "receiver456";
            int voucherID = 1;
            var voucher = new Voucher { Id = voucherID, IsValid = true };
            var receiver = new ApplicationUser { Id = receiverID };

            _mockVoucherRepository.Setup(r => r.GetByVourcherIdAsync(voucherID)).ReturnsAsync(voucher);
            _mockUnitOfWork.Setup(u => u.UserRepository.ExiProfile(receiverID)).ReturnsAsync(receiver);
            _mockVoucherRepository.Setup(r => r.UpdateAsync(voucher));
            _mockUnitOfWork.Setup(u => u.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.GiveVoucher(receiverID, voucherID);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(voucher.UserId, Is.EqualTo(receiverID));
            _mockVoucherRepository.Verify(r => r.UpdateAsync(voucher), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Test]
        public void GiveVoucher_ShouldThrowException_WhenVoucherNotFound()
        {
            // Arrange
            string receiverID = "receiver456";
            int voucherID = 1;

            _mockVoucherRepository.Setup(r => r.GetByVourcherIdAsync(voucherID)).ReturnsAsync((Voucher)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _service.GiveVoucher(receiverID, voucherID));
            Assert.That(ex.Message, Is.EqualTo("Voucher is not valid or does not exist."));
        }
    }
}
