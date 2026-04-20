using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using TechMove.Services;
using Xunit;

namespace TechMoveMvcFinal.Tests
{
    public class FileValidationTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<ILogger<FileStorageService>> _mockLogger;
        private readonly FileStorageService _service;

        public FileValidationTests()
        {
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.WebRootPath).Returns("C:\\test");
            _mockLogger = new Mock<ILogger<FileStorageService>>();
            _service = new FileStorageService(_mockEnv.Object, _mockLogger.Object);
        }

        [Fact]
        public void IsValidPdfFile_WithPdfFile_ReturnsTrue()
        {
            // Arrange
            var mockFile = CreateMockFile("test.pdf", "application/pdf", 1024);

            // Act
            var result = _service.IsValidPdfFile(mockFile.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidPdfFile_WithExeFile_ReturnsFalse()
        {
            // Arrange
            var mockFile = CreateMockFile("malware.exe", "application/x-msdownload", 1024);

            // Act
            var result = _service.IsValidPdfFile(mockFile.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidPdfFile_WithOversizedFile_ReturnsFalse()
        {
            // Arrange
            var mockFile = CreateMockFile("large.pdf", "application/pdf", 11 * 1024 * 1024); // 11MB

            // Act
            var result = _service.IsValidPdfFile(mockFile.Object);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("document.pdf", "application/pdf", true)]
        [InlineData("image.jpg", "image/jpeg", false)]
        [InlineData("script.js", "text/javascript", false)]
        [InlineData("virus.exe", "application/octet-stream", false)]
        public void IsValidPdfFile_WithVariousFileTypes_ValidatesCorrectly(
            string fileName, string contentType, bool expected)
        {
            // Arrange
            var mockFile = CreateMockFile(fileName, contentType, 1024);

            // Act
            var result = _service.IsValidPdfFile(mockFile.Object);

            // Assert
            Assert.Equal(expected, result);
        }

        private Mock<IFormFile> CreateMockFile(string fileName, string contentType, long length)
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.Length).Returns(length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("test content")));
            return mockFile;
        }
    }
}