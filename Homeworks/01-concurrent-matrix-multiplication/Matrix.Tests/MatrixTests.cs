// <copyright file="MatrixTests.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace Matrix.Tests;

using System.Text;

#pragma warning disable SA1600
public class MatrixTests
{
    private const string TestFilePath = "test_matrix.txt";
    private const string TestContent = "1 2 3\n4 5 6\n7 8 9";

    [SetUp]
    public void SetUp()
    {
        if (File.Exists(TestFilePath))
        {
            File.Delete(TestFilePath);
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(TestFilePath))
        {
            File.Delete(TestFilePath);
        }
    }

    [Test]
    public void Constructor_WithFilePath_ShouldCreateMatrixFromValidFile()
    {
        this.CreateTestFile(TestContent);

        var matrix = new ConcurrentMatrixMultiplication.Matrix(TestFilePath);

        Assert.That(matrix.NumberOfRows, Is.EqualTo(3));
        Assert.That(matrix.NumberOfColumns, Is.EqualTo(3));
        Assert.That(matrix[0, 0], Is.EqualTo(1));
        Assert.That(matrix[2, 2], Is.EqualTo(9));
    }

    [Test]
    public void Constructor_WithFilePath_ShouldThrowArgumentNullExceptionWhenPathIsNull()
    {
        string? nullPath = null;

        Assert.That(
            () => new ConcurrentMatrixMultiplication.Matrix(nullPath!),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_WithFilePath_ShouldThrowArgumentExceptionWhenPathIsEmpty()
    {
        var emptyPath = string.Empty;

        Assert.That(
            () => new ConcurrentMatrixMultiplication.Matrix(emptyPath),
            Throws.ArgumentException);
    }

    [Test]
    public void Constructor_WithFilePath_ShouldThrowFileNotFoundExceptionWhenFileDoesNotExist()
    {
        var nonExistentPath = "nonexistent.txt";

        Assert.That(
            () => new ConcurrentMatrixMultiplication.Matrix(nonExistentPath),
            Throws.TypeOf<FileNotFoundException>());
    }

    [Test]
    public void Constructor_WithFilePath_ShouldThrowInvalidDataExceptionWhenFileIsEmpty()
    {
        this.CreateTestFile(string.Empty);

        Assert.That(
            () => new ConcurrentMatrixMultiplication.Matrix(TestFilePath),
            Throws.TypeOf<InvalidDataException>());
    }

    [Test]
    public void Constructor_WithFilePath_ShouldThrowFormatExceptionWhenRowsHaveDifferentLengths()
    {
        this.CreateTestFile("1 2 3\n4 5\n6 7 8");

        Assert.That(
            () => new ConcurrentMatrixMultiplication.Matrix(TestFilePath),
            Throws.TypeOf<FormatException>());
    }

    [Test]
    public void Constructor_WithFilePath_ShouldThrowFormatExceptionWhenContainsNonIntegerValues()
    {
        this.CreateTestFile("1 2 3\n4 abc 6\n7 8 9");

        Assert.That(
            () => new ConcurrentMatrixMultiplication.Matrix(TestFilePath),
            Throws.TypeOf<FormatException>().With.Message.Contains("Invalid number format"));
    }

    [Test]
    public void Constructor_WithFilePath_ShouldHandleSingleRowMatrix()
    {
        this.CreateTestFile("1 2 3 4 5");

        var matrix = new ConcurrentMatrixMultiplication.Matrix(TestFilePath);

        Assert.That(matrix.NumberOfRows, Is.EqualTo(1));
        Assert.That(matrix.NumberOfColumns, Is.EqualTo(5));
        Assert.That(matrix[0, 4], Is.EqualTo(5));
    }

    [Test]
    public void Constructor_WithFilePath_ShouldHandleSingleElementMatrix()
    {
        this.CreateTestFile("42");

        var matrix = new ConcurrentMatrixMultiplication.Matrix(TestFilePath);

        Assert.That(matrix.NumberOfRows, Is.EqualTo(1));
        Assert.That(matrix.NumberOfColumns, Is.EqualTo(1));
        Assert.That(matrix[0, 0], Is.EqualTo(42));
    }

    [Test]
    public void Constructor_WithFileStream_ShouldCreateMatrixFromValidStream()
    {
        this.CreateTestFile(TestContent);
        using var fileStream = File.OpenRead(TestFilePath);

        var matrix = new ConcurrentMatrixMultiplication.Matrix(fileStream);

        Assert.That(matrix.NumberOfRows, Is.EqualTo(3));
        Assert.That(matrix.NumberOfColumns, Is.EqualTo(3));
    }

    [Test]
    public void Constructor_WithFileStream_ShouldThrowArgumentNullExceptionWhenStreamIsNull()
    {
        FileStream? nullStream = null;

        Assert.That(
            () => new ConcurrentMatrixMultiplication.Matrix(nullStream!),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_WithFileStream_ShouldThrowInvalidDataExceptionWhenStreamIsEmpty()
    {
        this.CreateTestFile(string.Empty);
        using var fileStream = File.OpenRead(TestFilePath);

        Assert.That(
            () => new ConcurrentMatrixMultiplication.Matrix(fileStream),
            Throws.TypeOf<InvalidDataException>());
    }

    [Test]
    public void Constructor_WithFileStream_ShouldWorkWithStreamAtDifferentPosition()
    {
        this.CreateTestFile(TestContent);
        using var fileStream = File.OpenRead(TestFilePath);
        fileStream.Position = 3;

        var matrix = new ConcurrentMatrixMultiplication.Matrix(fileStream);

        Assert.That(matrix.NumberOfRows, Is.EqualTo(3));
    }

    [Test]
    public void Constructor_WithStream_ShouldCreateMatrixFromMemoryStream()
    {
        var contentBytes = Encoding.UTF8.GetBytes(TestContent);
        using var memoryStream = new MemoryStream(contentBytes);

        var matrix = new ConcurrentMatrixMultiplication.Matrix(memoryStream);

        Assert.That(matrix.NumberOfRows, Is.EqualTo(3));
        Assert.That(matrix.NumberOfColumns, Is.EqualTo(3));
    }

    [Test]
    public void Constructor_WithStream_ShouldThrowArgumentNullExceptionWhenStreamIsNull()
    {
        Stream? nullStream = null;

        Assert.That(
            () => new ConcurrentMatrixMultiplication.Matrix(nullStream!),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_WithStream_ShouldThrowInvalidDataExceptionWhenStreamIsEmpty()
    {
        using var emptyStream = new MemoryStream();

        Assert.That(
            () => new ConcurrentMatrixMultiplication.Matrix(emptyStream),
            Throws.TypeOf<InvalidDataException>().With.Message.Contains("empty"));
    }

    [Test]
    public void Constructor_WithStream_ShouldHandleStreamWithDifferentEncodings()
    {
        var content = "1 2\n3 4";
        var contentBytes = Encoding.UTF8.GetBytes(content);
        using var memoryStream = new MemoryStream(contentBytes);

        var matrix = new ConcurrentMatrixMultiplication.Matrix(memoryStream);

        Assert.That(matrix.NumberOfRows, Is.EqualTo(2));
        Assert.That(matrix.NumberOfColumns, Is.EqualTo(2));
    }

    [Test]
    public void Constructor_WithStream_ShouldThrowFormatExceptionForInvalidContent()
    {
        var invalidContent = "1 2\n3 x";
        var contentBytes = Encoding.UTF8.GetBytes(invalidContent);
        using var memoryStream = new MemoryStream(contentBytes);

        Assert.That(
            () => new ConcurrentMatrixMultiplication.Matrix(memoryStream),
            Throws.TypeOf<FormatException>().With.Message.Contains("Invalid number format"));
    }

    [Test]
    public void Constructor_WithStream_ShouldHandleLargeMatrix()
    {
        var largeContent = new StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            largeContent.Append($"{i} {i + 1}");
            if (i < 99)
            {
                largeContent.AppendLine();
            }
        }

        var contentBytes = Encoding.UTF8.GetBytes(largeContent.ToString());
        using var memoryStream = new MemoryStream(contentBytes);

        var matrix = new ConcurrentMatrixMultiplication.Matrix(memoryStream);

        Assert.That(matrix.NumberOfRows, Is.EqualTo(100));
        Assert.That(matrix.NumberOfColumns, Is.EqualTo(2));
        Assert.That(matrix[99, 0], Is.EqualTo(99));
        Assert.That(matrix[99, 1], Is.EqualTo(100));
    }

    [Test]
    public void AllConstructors_ShouldCreateIdenticalMatricesFromSameContent()
    {
        this.CreateTestFile(TestContent);
        using var fileStream = File.OpenRead(TestFilePath);
        var contentBytes = Encoding.UTF8.GetBytes(TestContent);
        using var memoryStream = new MemoryStream(contentBytes);

        var matrixFromPath = new ConcurrentMatrixMultiplication.Matrix(TestFilePath);
        var matrixFromFileStream = new ConcurrentMatrixMultiplication.Matrix(fileStream);
        var matrixFromStream = new ConcurrentMatrixMultiplication.Matrix(memoryStream);

        Assert.That(matrixFromPath.NumberOfRows, Is.EqualTo(matrixFromFileStream.NumberOfRows));
        Assert.That(matrixFromPath.NumberOfColumns, Is.EqualTo(matrixFromFileStream.NumberOfColumns));
        Assert.That(matrixFromPath[1, 1], Is.EqualTo(matrixFromFileStream[1, 1]));

        Assert.That(matrixFromPath.NumberOfRows, Is.EqualTo(matrixFromStream.NumberOfRows));
        Assert.That(matrixFromPath.NumberOfColumns, Is.EqualTo(matrixFromStream.NumberOfColumns));
        Assert.That(matrixFromPath[2, 2], Is.EqualTo(matrixFromStream[2, 2]));
    }

    [Test]
    public void AllConstructors_ShouldThrowOnInvalidMatrixFormat()
    {
        var invalidContent = "1 2 3\n4 5\n6 7 8";
        this.CreateTestFile(invalidContent);
        using var fileStream = File.OpenRead(TestFilePath);
        var contentBytes = Encoding.UTF8.GetBytes(invalidContent);
        using var memoryStream = new MemoryStream(contentBytes);

        Assert.That(() => new ConcurrentMatrixMultiplication.Matrix(TestFilePath), Throws.TypeOf<FormatException>());
        Assert.That(() => new ConcurrentMatrixMultiplication.Matrix(fileStream), Throws.TypeOf<FormatException>());
        Assert.That(() => new ConcurrentMatrixMultiplication.Matrix(memoryStream), Throws.TypeOf<FormatException>());
    }

    private void CreateTestFile(string content)
    {
        File.WriteAllText(TestFilePath, content, Encoding.UTF8);
    }
}
