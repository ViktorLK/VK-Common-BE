using System.Linq.Expressions;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.Persistence.Core.Pagination;
using VK.Blocks.Core.Results;
using VK.Blocks.Persistence.EFCore.Infrastructure;
using VK.Blocks.Persistence.EFCore.Repositories;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Repositories;

public class EfCoreReadRepositoryTests : IDisposable
{
    private readonly IFixture _fixture;
    private readonly Mock<ICursorSerializer> _cursorSerializerMock;
    private readonly Mock<ILogger<EfCoreReadRepository<TestEntity>>> _loggerMock;
    private readonly TestsDbContext _context;

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class TestsDbContext : DbContext
    {
        public TestsDbContext(DbContextOptions<TestsDbContext> options) : base(options) { }
        public DbSet<TestEntity> TestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedNever();
            });
        }
    }

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
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private EfCoreReadRepository<TestEntity> CreateSut()
    {
        return new EfCoreReadRepository<TestEntity>(
            _context,
            _loggerMock.Object,
            _cursorSerializerMock.Object);
    }

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
        // Page 1: 0,1,2. Page 2: 3,4,5.
        // But auto-generated IDs might be random. We sorted entities first.
        // Wait, EF Core InMemory saves them.

        // Let's verify content broadly
        result.Items.Should().BeEquivalentTo(entities.Skip(3).Take(3));
    }

    [Fact]
    public async Task GetCursorPagedAsync_Forward_ReturnsNextPage()
    {
        // Arrange
        var entities = Enumerable.Range(1, 10).Select(i => new TestEntity { Id = i, Name = $"Name{i}" }).ToList();
        _context.TestEntities.AddRange(entities);
        await _context.SaveChangesAsync();

        var sut = CreateSut();

        // Mock Serializer for int cursors
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

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsEntity()
    {
        // Arrange
        var entity = _fixture.Create<TestEntity>();
        _context.TestEntities.Add(entity);
        await _context.SaveChangesAsync();

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(-1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetListAsync_ReturnsMatchingEntities()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 1, Name = "Alpha" };
        var entity2 = new TestEntity { Id = 2, Name = "Bravo" };
        var entity3 = new TestEntity { Id = 3, Name = "Alpha" };
        _context.TestEntities.AddRange(entity1, entity2, entity3);
        await _context.SaveChangesAsync();

        var sut = CreateSut();

        // Act
        var result = await sut.GetListAsync(x => x.Name == "Alpha");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.Id == 1);
        result.Should().Contain(e => e.Id == 3);
    }

    [Fact]
    public async Task AnyAsync_ReturnsTrue_WhenExists()
    {
        // Arrange
        _context.TestEntities.Add(new TestEntity { Id = 1, Name = "Exist" });
        await _context.SaveChangesAsync();

        var sut = CreateSut();

        // Act
        var exists = await sut.AnyAsync(x => x.Name == "Exist");
        var notExists = await sut.AnyAsync(x => x.Name == "Nope");

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        // Arrange
        _context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A" },
            new TestEntity { Id = 2, Name = "A" },
            new TestEntity { Id = 3, Name = "B" }
        );
        await _context.SaveChangesAsync();

        var sut = CreateSut();

        // Act
        var countA = await sut.CountAsync(x => x.Name == "A");
        var countAll = await sut.CountAsync();

        // Assert
        countA.Should().Be(2);
        countAll.Should().Be(3);
    }
}
