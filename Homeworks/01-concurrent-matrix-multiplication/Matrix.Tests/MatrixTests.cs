// <copyright file="MatrixTests.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace Matrix.Tests;

using System.Text;
using ConcurrentMatrixMultiplication;

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
        string[] testFiles = [TestFilePath, "left.txt", "right.txt", "result.txt",
                            "test_output.txt", "single.txt", "large.txt", "test_file.txt"];

        foreach (var file in testFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

    [Test]
    public void Multiply_WithFilePaths_ShouldCreateResultFileWithCorrectContent()
    {
        this.CreateTestFile("left.txt", "1 2\n3 4");
        this.CreateTestFile("right.txt", "5 6\n7 8");
        string resultPath = "result.txt";

        Matrix.Muliply("left.txt", "right.txt", resultPath);

        Assert.That(File.Exists(resultPath), Is.True);
        var content = File.ReadAllText(resultPath);
        Assert.That(content, Does.Contain("19 22"));
        Assert.That(content, Does.Contain("43 50"));
    }

    [Test]
    public void Multiply_WithFilePaths_ShouldThrowFileNotFoundExceptionWhenLeftFileNotFound()
    {
        this.CreateTestFile("right.txt", "1 2\n3 4");

        Assert.That(
            () => Matrix.Muliply("nonexistent.txt", "right.txt", "result.txt"),
            Throws.TypeOf<FileNotFoundException>());
    }

    [Test]
    public void Multiply_WithFilePaths_ShouldThrowFileNotFoundExceptionWhenRightFileNotFound()
    {
        this.CreateTestFile("left.txt", "1 2\n3 4");

        Assert.That(
            () => Matrix.Muliply("left.txt", "nonexistent.txt", "result.txt"),
            Throws.TypeOf<FileNotFoundException>());
    }

    [Test]
    public void Multiply_WithFilePaths_ShouldThrowWhenResultPathIsNull()
    {
        this.CreateTestFile("left.txt", "1 2\n3 4");
        this.CreateTestFile("right.txt", "5 6\n7 8");

        Assert.That(
            () => Matrix.Muliply("left.txt", "right.txt", null!),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Multiply_WithFilePaths_ShouldThrowWhenMatricesIncompatible()
    {
        this.CreateTestFile("left.txt", "1 2 3\n4 5 6");
        this.CreateTestFile("right.txt", "1 2\n3 4");

        Assert.That(
            () => Matrix.Muliply("left.txt", "right.txt", "result.txt"),
            Throws.ArgumentException);
    }

    [Test]
    public void MultiplyFromFiles_ShouldReturnCorrectMatrix()
    {
        this.CreateTestFile("left.txt", "1 -2\n3 4");
        this.CreateTestFile("right.txt", "5 6\n7 8");

        var result = Matrix.MultiplyFromFiles("left.txt", "right.txt");

        Assert.Multiple(() =>
        {
            Assert.That(result.NumberOfRows, Is.EqualTo(2));
            Assert.That(result.NumberOfColumns, Is.EqualTo(2));
            Assert.That(result[0, 0], Is.EqualTo(-9));
            Assert.That(result[1, 1], Is.EqualTo(50));
        });
    }

    [Test]
    public void MultiplyFromFiles_ShouldThrowWhenFilesNotFound()
    {
        Assert.That(
            () => Matrix.MultiplyFromFiles("nonexistent1.txt", "nonexistent2.txt"),
            Throws.TypeOf<FileNotFoundException>());
    }

    [Test]
    public void MultiplyFromFiles_ShouldThrowWhenPathsAreNullOrEmpty()
    {
        Assert.That(
            () => Matrix.MultiplyFromFiles(null!, "right.txt"),
            Throws.ArgumentNullException);
        Assert.That(
            () => Matrix.MultiplyFromFiles("left.txt", string.Empty),
            Throws.ArgumentException);
    }

    [Test]
    public void SaveToFile_ShouldThrowWhenPathIsNull()
    {
        var matrix = new Matrix(2, 2);

        Assert.That(
            () => matrix.SaveToFile(null!),
            Throws.ArgumentNullException);
    }

    [Test]
    public void SaveToFile_ShouldThrowWhenPathIsEmpty()
    {
        var matrix = new Matrix(2, 2);

        Assert.That(
            () => matrix.SaveToFile(string.Empty),
            Throws.ArgumentException);
    }

    [Test]
    public void SaveToFile_ShouldCreateFileWithCorrectFormat()
    {
        int[,] data =
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
        };
        var matrix = new Matrix(data);
        string filePath = "test_output.txt";

        matrix.SaveToFile(filePath);

        Assert.That(File.Exists(filePath), Is.True);
        var lines = File.ReadAllLines(filePath);
        Assert.That(lines, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(lines[0], Is.EqualTo("1 2 3"));
            Assert.That(lines[1], Is.EqualTo("4 5 6"));
        });
    }

    [Test]
    public void SaveToFile_ShouldOverwriteExistingFile()
    {
        int[,] data =
        {
            { 1, 2 },
            { 3, 4 },
        };
        var matrix = new Matrix(data);
        string filePath = "test_file.txt";
        File.WriteAllText(filePath, "old content");

        matrix.SaveToFile(filePath);

        var content = File.ReadAllText(filePath);
        Assert.That(content, Does.Not.Contain("old content"));
        Assert.That(content, Does.Contain("1 2"));
        Assert.That(content, Does.Contain("3 4"));
    }

    [Test]
    public void SaveToFile_ShouldHandleSingleElementMatrix()
    {
        int[,] data = { { 42 } };
        var matrix = new Matrix(data);
        string filePath = "single.txt";

        matrix.SaveToFile(filePath);

        var content = File.ReadAllText(filePath).Trim();
        Assert.That(content, Is.EqualTo("42"));
    }

    [Test]
    public void Indexer_ShouldThrowWhenIndicesOutOfRange()
    {
        var matrix = new Matrix(2, 2);

        Assert.That(() => matrix[-1, 0], Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => matrix[2, 0], Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => matrix[0, -1], Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => matrix[0, 2], Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Multiply_ShouldThrowArgumentNullException_WhenLeftMatrixIsNull()
    {
        Matrix right = new(2, 2);
        Assert.That(() => Matrix.Multiply(null!, right), Throws.ArgumentNullException);
    }

    [Test]
    public void Multiply_ShouldThrowArgumentNullException_WhenRightMatrixIsNull()
    {
        Matrix left = new(2, 2);
        Assert.That(() => Matrix.Multiply(left, null!), Throws.ArgumentNullException);
    }

    [Test]
    public void Multiply_ShouldThrowArgumentException_WhenMatricesCannotBeMultiplied()
    {
        Matrix left = new(2, 3);
        Matrix right = new(2, 2);

        Assert.That(
            () => Matrix.Multiply(left, right),
            Throws.ArgumentException.With.Message.Contains("cannot be multiplied"));
    }

    [Test]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenNonPositiveDimensions()
    {
        Assert.That(() => new Matrix(0, 5), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new Matrix(5, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Constructor_WithFilePath_ShouldCreateMatrixFromValidFile()
    {
        this.CreateTestFile(TestContent);

        var matrix = new Matrix(TestFilePath);

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
            () => new Matrix(nullPath!),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_WithFilePath_ShouldThrowArgumentExceptionWhenPathIsEmpty()
    {
        var emptyPath = string.Empty;

        Assert.That(
            () => new Matrix(emptyPath),
            Throws.ArgumentException);
    }

    [Test]
    public void Constructor_WithFilePath_ShouldThrowFileNotFoundExceptionWhenFileDoesNotExist()
    {
        var nonExistentPath = "nonexistent.txt";

        Assert.That(
            () => new Matrix(nonExistentPath),
            Throws.TypeOf<FileNotFoundException>());
    }

    [Test]
    public void Constructor_WithFilePath_ShouldThrowInvalidDataExceptionWhenFileIsEmpty()
    {
        this.CreateTestFile(string.Empty);

        Assert.That(
            () => new Matrix(TestFilePath),
            Throws.TypeOf<InvalidDataException>());
    }

    [Test]
    public void Constructor_WithFilePath_ShouldThrowFormatExceptionWhenRowsHaveDifferentLengths()
    {
        this.CreateTestFile("1 2 3\n4 5\n6 7 8");

        Assert.That(
            () => new Matrix(TestFilePath),
            Throws.TypeOf<FormatException>());
    }

    [Test]
    public void Constructor_WithFilePath_ShouldThrowFormatExceptionWhenContainsNonIntegerValues()
    {
        this.CreateTestFile("1 2 3\n4 abc 6\n7 8 9");

        Assert.That(
            () => new Matrix(TestFilePath),
            Throws.TypeOf<FormatException>().With.Message.Contains("Invalid number format"));
    }

    [Test]
    public void Constructor_WithFilePath_ShouldHandleSingleRowMatrix()
    {
        this.CreateTestFile("1 2 3 4 5");

        var matrix = new Matrix(TestFilePath);

        Assert.That(matrix.NumberOfRows, Is.EqualTo(1));
        Assert.That(matrix.NumberOfColumns, Is.EqualTo(5));
        Assert.That(matrix[0, 4], Is.EqualTo(5));
    }

    [Test]
    public void Constructor_WithFilePath_ShouldHandleSingleElementMatrix()
    {
        this.CreateTestFile("42");

        var matrix = new Matrix(TestFilePath);

        Assert.That(matrix.NumberOfRows, Is.EqualTo(1));
        Assert.That(matrix.NumberOfColumns, Is.EqualTo(1));
        Assert.That(matrix[0, 0], Is.EqualTo(42));
    }

    [Test]
    public void Constructor_WithFileStream_ShouldCreateMatrixFromValidStream()
    {
        this.CreateTestFile(TestContent);
        using var fileStream = File.OpenRead(TestFilePath);

        var matrix = new Matrix(fileStream);

        Assert.That(matrix.NumberOfRows, Is.EqualTo(3));
        Assert.That(matrix.NumberOfColumns, Is.EqualTo(3));
    }

    [Test]
    public void Constructor_WithFileStream_ShouldThrowArgumentNullExceptionWhenStreamIsNull()
    {
        FileStream? nullStream = null;

        Assert.That(
            () => new Matrix(nullStream!),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_WithFileStream_ShouldThrowInvalidDataExceptionWhenStreamIsEmpty()
    {
        this.CreateTestFile(string.Empty);
        using var fileStream = File.OpenRead(TestFilePath);

        Assert.That(
            () => new Matrix(fileStream),
            Throws.TypeOf<InvalidDataException>());
    }

    [Test]
    public void Constructor_WithFileStream_ShouldWorkWithStreamAtDifferentPosition()
    {
        this.CreateTestFile(TestContent);
        using var fileStream = File.OpenRead(TestFilePath);
        fileStream.Position = 3;

        var matrix = new Matrix(fileStream);

        Assert.That(matrix.NumberOfRows, Is.EqualTo(3));
    }

    [Test]
    public void Constructor_WithStream_ShouldCreateMatrixFromMemoryStream()
    {
        var contentBytes = Encoding.UTF8.GetBytes(TestContent);
        using var memoryStream = new MemoryStream(contentBytes);

        var matrix = new Matrix(memoryStream);

        Assert.That(matrix.NumberOfRows, Is.EqualTo(3));
        Assert.That(matrix.NumberOfColumns, Is.EqualTo(3));
    }

    [Test]
    public void Constructor_WithStream_ShouldThrowArgumentNullExceptionWhenStreamIsNull()
    {
        Stream? nullStream = null;

        Assert.That(
            () => new Matrix(nullStream!),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_WithStream_ShouldThrowInvalidDataExceptionWhenStreamIsEmpty()
    {
        using var emptyStream = new MemoryStream();

        Assert.That(
            () => new Matrix(emptyStream),
            Throws.TypeOf<InvalidDataException>().With.Message.Contains("empty"));
    }

    [Test]
    public void Constructor_WithStream_ShouldHandleStreamWithDifferentEncodings()
    {
        var content = "1 2\n3 4";
        var contentBytes = Encoding.UTF8.GetBytes(content);
        using var memoryStream = new MemoryStream(contentBytes);

        var matrix = new Matrix(memoryStream);

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
            () => new Matrix(memoryStream),
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

        var matrix = new Matrix(memoryStream);

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

        var matrixFromPath = new Matrix(TestFilePath);
        var matrixFromFileStream = new Matrix(fileStream);
        var matrixFromStream = new Matrix(memoryStream);

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

        Assert.That(() => new Matrix(TestFilePath), Throws.TypeOf<FormatException>());
        Assert.That(() => new Matrix(fileStream), Throws.TypeOf<FormatException>());
        Assert.That(() => new Matrix(memoryStream), Throws.TypeOf<FormatException>());
    }

    private void CreateTestFile(string content)
    {
        File.WriteAllText(TestFilePath, content, Encoding.UTF8);
    }

    private void CreateTestFile(string fileName, string content)
    {
        File.WriteAllText(fileName, content, Encoding.UTF8);
    }
}
