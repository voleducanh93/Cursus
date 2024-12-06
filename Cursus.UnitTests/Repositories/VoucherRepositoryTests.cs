using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories;

[TestFixture]
public class VoucherRepositoryTests
{
    private CursusDbContext _context;
    private VoucherRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CursusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new CursusDbContext(options);
        _repository = new VoucherRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetByCodeAsync_ShouldReturnVoucher_WhenVoucherExists()
    {
        // Arrange
        var voucher = new Voucher { VoucherCode = "TEST123", Name = "Test Voucher", IsValid = true };
        _context.Vouchers.Add(voucher);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCodeAsync("TEST123");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.VoucherCode, Is.EqualTo("TEST123"));
    }

    [Test]
    public void GetByCodeAsync_ShouldThrowException_WhenVoucherDoesNotExist()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _repository.GetByCodeAsync("INVALID"));
        Assert.That(ex.Message, Is.EqualTo("Voucher not found"));
    }

    [Test]
    public async Task GetByVourcherIdAsync_ShouldReturnVoucher_WhenVoucherExists()
    {
        // Arrange
        var voucher = new Voucher { Id = 1, VoucherCode = "TEST123", Name = "Test Voucher", IsValid = true };
        _context.Vouchers.Add(voucher);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByVourcherIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
    }

    [Test]
    public void GetByVourcherIdAsync_ShouldThrowException_WhenVoucherDoesNotExist()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _repository.GetByVourcherIdAsync(999));
        Assert.That(ex.Message, Is.EqualTo("Voucher not found"));
    }

    [Test]
    public async Task AddVoucher_ShouldAddVoucherSuccessfully()
    {
        // Arrange
        var voucher = new Voucher { VoucherCode = "NEWVOUCHER", Name = "New Voucher", IsValid = true };

        // Act
        await _repository.AddAsync(voucher);
        await _context.SaveChangesAsync();

        // Assert
        var addedVoucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.VoucherCode == "NEWVOUCHER");
        Assert.That(addedVoucher, Is.Not.Null);
        Assert.That(addedVoucher.Name, Is.EqualTo("New Voucher"));
    }

    [Test]
    public async Task DeleteVoucher_ShouldRemoveVoucherSuccessfully()
    {
        // Arrange
        var voucher = new Voucher { Id = 1, VoucherCode = "TODELETE", Name = "To Be Deleted", IsValid = true };
        _context.Vouchers.Add(voucher);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(voucher);
        await _context.SaveChangesAsync();

        // Assert
        var deletedVoucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.VoucherCode == "TODELETE");
        Assert.That(deletedVoucher, Is.Null);
    }

    [Test]
    public async Task UpdateVoucher_ShouldUpdateVoucherSuccessfully()
    {
        // Arrange
        var voucher = new Voucher { Id = 1, VoucherCode = "TOUPDATE", Name = "Old Name", IsValid = true };
        _context.Vouchers.Add(voucher);
        await _context.SaveChangesAsync();

        // Act
        voucher.Name = "Updated Name";
        await _repository.UpdateAsync(voucher);
        await _context.SaveChangesAsync();

        // Assert
        var updatedVoucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.VoucherCode == "TOUPDATE");
        Assert.That(updatedVoucher, Is.Not.Null);
        Assert.That(updatedVoucher.Name, Is.EqualTo("Updated Name"));
    }
}
