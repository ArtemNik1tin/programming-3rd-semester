// <copyright file="Matrix.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace ConcurrentMatrixMultiplication;

/// <summary>
/// Class for concurent multiplication of matrices loaded from a user file.
/// </summary>
public class Matrix
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix"/> class.
    /// </summary>
    /// <param name="filePath">Path to file with matrix.</param>
    public Matrix(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        var result = this.ReadFromFilePath(filePath);
        this.Data = result.Data;
        this.NumberOfRows = result.Rows;
        this.NumberOfColumns = result.Columns;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix"/> class.
    /// </summary>
    /// <param name="fileStream">File stream with matrix data.</param>
    public Matrix(FileStream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        var result = this.ReadFromFileStream(fileStream);
        this.Data = result.Data;
        this.NumberOfRows = result.Rows;
        this.NumberOfColumns = result.Columns;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix"/> class.
    /// </summary>
    /// <param name="stream">Stream with matrix data.</param>
    public Matrix(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var result = this.ReadFromStream(stream);
        this.Data = result.Data;
        this.NumberOfRows = result.Rows;
        this.NumberOfColumns = result.Columns;
    }

    /// <summary>
    /// Gets contents of the matrix.
    /// </summary>
    public int[,] Data { private get; init; }

    /// <summary>
    /// Gets number of rows.
    /// </summary>
    public int NumberOfRows { get; init; }

    /// <summary>
    /// Gets number of columns.
    /// </summary>
    public int NumberOfColumns { get; init; }

    /// <summary>
    /// Gets element at specified position.
    /// </summary>
    /// <param name="row">Row index.</param>
    /// <param name="column">Column index.</param>
    /// <returns>Element value.</returns>
    public int this[int row, int column]
    {
        get
        {
            if (row < 0 || row >= this.NumberOfRows)
            {
                throw new ArgumentOutOfRangeException(nameof(row), $"Row index must be between 0 and {this.NumberOfRows - 1}");
            }

            if (column < 0 || column >= this.NumberOfColumns)
            {
                throw new ArgumentOutOfRangeException(nameof(column), $"Column index must be between 0 and {this.NumberOfColumns - 1}");
            }

            return this.Data[row, column];
        }
    }

    private (int[,] Data, int Rows, int Columns) ReadFromFilePath(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        using var fileStream = File.OpenRead(filePath);
        return this.ReadFromFileStream(fileStream);
    }

    private (int[,] Data, int Rows, int Columns) ReadFromFileStream(FileStream fileStream)
    {
        return this.ReadFromStream(fileStream);
    }

    private (int[,] Data, int Rows, int Columns) ReadFromStream(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        return this.ParseMatrixContent(content);
    }

    private (int[,] Data, int Rows, int Columns) ParseMatrixContent(string content)
    {
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
        {
            throw new InvalidDataException("File is empty.");
        }

        var firstLineValues = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int columns = firstLineValues.Length;
        int rows = lines.Length;

        var data = new int[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            var values = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (values.Length != columns)
            {
                throw new FormatException($"Mismatch in number of elements in row {i + 1}. Expected: {columns}, Actual: {values.Length}");
            }

            for (int j = 0; j < columns; j++)
            {
                if (!int.TryParse(values[j], out int value))
                {
                    throw new FormatException($"Invalid number format in row {i + 1}, column {j + 1}: '{values[j]}'");
                }

                data[i, j] = value;
            }
        }

        return (data, rows, columns);
    }
}