using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.Persistence.Core.Pagination;
using VK.Blocks.Persistence.EFCore.Infrastructure;
using VK.Blocks.Persistence.EFCore.Repositories;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Repositories;

/// <summary>
/// Unit tests for <see cref="EfCoreReadRepository{TEntity}"/>.
/// </summary>
public class EfCoreReadRepositoryTests : IDisposable
{
    private readonly IFixture _fixture;

    private readonly Mock<ICursorSerializer> _cursorSerializerMock;

    private readonly Mock<ILogger<EfCoreReadRepository<TestEntity>>> _loggerMock;

    private readonly TestsDbContext _context;

    /// <summary>
    /// A test entity for repository tests.
    /// </summary>
    public class TestEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public int Value { get; set; }
    }

    /// <summary>
    /// A test database context for repository tests.
    /// </summary>
    private class TestsDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestsDbContext"/> class.
        /// </summary>
        /// <param name="options">The database context options.</param>
        public TestsDbContext(DbContextOptions<TestsDbContext> options) : base(options) { }

        /// <summary>
        /// Gets or sets the test entities.
        /// </summary>
        public DbSet<TestEntity> TestEntities { get; set; } = null!;

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedNever();
            });
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EfCoreReadRepositoryTests"/> class.
    /// </summary>
    public EfCoreReadRepositoryTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _cursorSerializerMock = _fixture.Freeze<Mock<ICursorSerializer>>();
        _loggerMock = _fixture.Freeze<Mock<ILogger<EfCoreReadRepository<TestEntity>>>>();

        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<TestsDbContext>()
            .UseSqlite(connection)
            .Options;
        _context = new TestsDbContext(options);

        // Rationale: Ensure the schema is created before running tests.
        _context.Database.EnsureCreated();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Helper method to create the system under test.
    /// </summary>
    private EfCoreReadRepository<TestEntity> CreateSut()
    {
        return new EfCoreReadRepository<TestEntity>(
            _context,
            _loggerMock.Object,
            _cursorSerializerMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="EfCoreReadRepository{TEntity}.GetPagedAsync"/> returns the correct items for a given page.
    /// </summary>
    [Fact]
    public async Task GetPagedAsync_ValidInput_ReturnsPagedResult()
    {
        // Arrange
        var entities = _fixture.CreateMany<TestEntity>(10).OrderBy(e => e.Id).ToList();
        _context.TestEntities.AddRange(entities);
        await _context.SaveChangesAsync();

        var sut = CreateSut();

        // Act
        var result = await sut.GetPagedAsync(e => true, e => e.Id, pageNumber: 2, pageSize: 3);

        // Assert
        result.TotalCount.Should().Be(10);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(3);
        result.Items.Should().HaveCount(3);

        // Rationale: Verify that the items returned correspond to the second page (skipping the first 3).
        result.Items.Should().BeEquivalentTo(entities.Skip(3).Take(3));
    }

    /// <summary>
    /// Verifies that <see cref="EfCoreReadRepository{TEntity}.GetCursorPagedAsync"/> returns the next page and a valid next cursor.
    /// </summary>
    [Fact]
    public async Task GetCursorPagedAsync_Forward_ReturnsNextPage()
    {
        // Arrange
        var entities = Enumerable.Range(1, 10).Select(i => new TestEntity { Id = i, Name = $"Name{i}" }).ToList();
        _context.TestEntities.AddRange(entities);
        await _context.SaveChangesAsync();

        var sut = CreateSut();

        // Rationale: Mock the cursor serializer to handle simple integer cursors as strings.
        _cursorSerializerMock.Setup(x => x.Serialize(It.IsAny<int>())).Returns((int val) => val.ToString());
        _cursorSerializerMock.Setup(x => x.Deserialize<int>(It.IsAny<string>())).Returns((string val) => int.Parse(val));

        // Act - Page 1
        var result1 = await sut.GetCursorPagedAsync(e => true, e => e.Id, pageSize: 3);

        // Assert - Page 1
        result1.Items.Should().HaveCount(3);
        result1.Items.First().Id.Should().Be(1);
        result1.Items.Last().Id.Should().Be(3);
        result1.HasNextPage.Should().BeTrue();
        result1.NextCursor.Should().Be("3");

        // Act - Page 2
        var cursor = int.Parse(result1.NextCursor!);
        var result2 = await sut.GetCursorPagedAsync(e => true, e => e.Id, cursor: cursor, pageSize: 3);

        // Assert - Page 2
        result2.Items.Should().HaveCount(3);
        result2.Items.First().Id.Should().Be(4);
    }

    /// <summary>
    /// Verifies that <see cref="EfCoreReadRepository{TEntity}.GetFirstOrDefaultAsync"/> returns the first matching entity.
    /// </summary>
    [Fact]
    public async Task GetFirstOrDefaultAsync_ReturnsFirstMatching()
    {
        // Arrange
        _context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A" },
            new TestEntity { Id = 2, Name = "A" }
        );
        await _context.SaveChangesAsync();
        var sut = CreateSut();

        // Act
        var result = await sut.GetFirstOrDefaultAsync(x => x.Name == "A", x => x.OrderBy(e => e.Id));

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    /// <summary>
    /// Verifies that <see cref="EfCoreReadRepository{TEntity}.GetSingleOrDefaultAsync"/> throws <see cref="InvalidOperationException"/> when multiple matches exist.
    /// </summary>
    [Fact]
    public async Task GetSingleOrDefaultAsync_MultipleMatches_ThrowsInvalidOperationException()
    {
        // Arrange
        _context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A" },
            new TestEntity { Id = 2, Name = "A" }
        );
        await _context.SaveChangesAsync();
        var sut = CreateSut();

        // Act
        Func<Task> act = async () => await sut.GetSingleOrDefaultAsync(x => x.Name == "A");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that <see cref="EfCoreReadRepository{TEntity}.GetListAsNoTrackingAsync"/> returns entities without tracking.
    /// </summary>
    [Fact]
    public async Task GetListAsNoTrackingAsync_ReturnsEntities()
    {
        // Arrange
        _context.TestEntities.Add(new TestEntity { Id = 1, Name = "A" });
        await _context.SaveChangesAsync();
        var sut = CreateSut();

        // Act
        var result = await sut.GetListAsNoTrackingAsync(x => x.Name == "A");

        // Assert
        result.Should().HaveCount(1);
    }

    /// <summary>
    /// Verifies that <see cref="EfCoreReadRepository{TEntity}.GetByIdAsync"/> returns an entity by its identifier.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_CompositeKey_ReturnsEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "A" };
        _context.TestEntities.Add(entity);
        await _context.SaveChangesAsync();
        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync([1]);

        // Assert
        result.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that <see cref="EfCoreReadRepository{TEntity}.ExecuteAsync"/> returns a projected list of results.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ReturnsProjectedList()
    {
        // Arrange
        _context.TestEntities.Add(new TestEntity { Id = 1, Name = "A", Value = 10 });
        await _context.SaveChangesAsync();
        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(q => q.Select(x => x.Value));

        // Assert
        result.Should().Contain(10);
    }

    /// <summary>
    /// Verifies that <see cref="EfCoreReadRepository{TEntity}.ExecuteSingleAsync"/> returns a projected single result.
    /// </summary>
    [Fact]
    public async Task ExecuteSingleAsync_ReturnsProjectedValue()
    {
        // Arrange
        _context.TestEntities.Add(new TestEntity { Id = 1, Name = "A", Value = 10 });
        await _context.SaveChangesAsync();
        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteSingleAsync(q => q.Where(x => x.Id == 1).Select(x => x.Value));

        // Assert
        result.Should().Be(10);
    }

    /// <summary>
    /// Verifies that <see cref="EfCoreReadRepository{TEntity}.FromSqlRawAsync"/> correctly queries the database using raw SQL.
    /// </summary>
    [Fact]
    public async Task FromSqlRawAsync_ReturnsEntities()
    {
        // Arrange
        _context.TestEntities.Add(new TestEntity { Id = 1, Name = "A" });
        await _context.SaveChangesAsync();
        var sut = CreateSut();

        // Act
        var result = await sut.FromSqlRawAsync("SELECT * FROM TestEntities WHERE Name = 'A'");

        // Assert
        result.Should().HaveCount(1);
    }
}
