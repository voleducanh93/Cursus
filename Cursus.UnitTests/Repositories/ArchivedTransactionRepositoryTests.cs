using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cursus.Repository.Repository;
using Cursus.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Cursus.Data.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class ArchivedTransactionRepositoryTests
    {
        private ArchivedTransactionRepository _repository;
        private Mock<CursusDbContext> _mockDbContext;
        private Mock<DbSet<ArchivedTransaction>> _mockArchivedTransactionDbSet;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = new Mock<CursusDbContext>();
            _mockArchivedTransactionDbSet = CreateMockDbSet(new List<ArchivedTransaction>().AsQueryable());
            _mockDbContext.Setup(db => db.ArchivedTransactions).Returns(_mockArchivedTransactionDbSet.Object);
            _repository = new ArchivedTransactionRepository(_mockDbContext.Object);
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockDbSet = new Mock<DbSet<T>>();
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockDbSet;
        }

        [Test]
        public async Task AddAsync_ShouldAddArchivedTransaction()
        {
            // Arrange
            var archivedTransaction = new ArchivedTransaction { Id = 1 };
            _mockArchivedTransactionDbSet.Setup(db => db.AddAsync(archivedTransaction, default)).ReturnsAsync((EntityEntry<ArchivedTransaction>)null);

            // Act
            var result = await _repository.AddAsync(archivedTransaction);

            // Assert
            Assert.That(result, Is.EqualTo(archivedTransaction));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveArchivedTransaction()
        {
            // Arrange
            var archivedTransaction = new ArchivedTransaction { Id = 1 };
            _mockArchivedTransactionDbSet.Setup(db => db.Remove(archivedTransaction));

            // Act
            var result = await _repository.DeleteAsync(archivedTransaction);

            // Assert
            Assert.That(result, Is.EqualTo(archivedTransaction));
        }


        [Test]
        public async Task GetAsync_ShouldReturnNull_WhenTransactionDoesNotExist()
        {
            // Arrange
            var archivedTransactions = new List<ArchivedTransaction>().AsQueryable();
            _mockArchivedTransactionDbSet = CreateMockDbSet(archivedTransactions);
            _mockDbContext.Setup(db => db.ArchivedTransactions).Returns(_mockArchivedTransactionDbSet.Object);

            // Act
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.GetAsync(at => at.Id == 1));
        }
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public T Current => _inner.Current;
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return new TestAsyncEnumerable<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return Execute<TResult>(expression);
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }
}
