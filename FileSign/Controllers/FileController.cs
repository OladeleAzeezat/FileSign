using FileSign.DTOs;
using FileSign.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileSign.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FileController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost("upload")]
        public async Task<IActionResult> UploadToFileSystem(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            //Validate file size (e.g max 10 mb)
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest("File size exceeds the 10 MB limit.");
            }

            var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Documents");

            // Create folder if it doesn't exist
            if (!Directory.Exists(uploadsFolderPath))
                Directory.CreateDirectory(uploadsFolderPath);

            // Create a unique file name
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolderPath, file.FileName);


            //Save the file to the server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Optionally save file path to database
            var uploadedFile = new FileRecord
            {
                FileName = file.FileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                UploadDate = DateTime.Now,
            };

            _context.FileRecords.Add(uploadedFile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "File saved to file system." });
        }

        [HttpGet("documents")]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                // Fetch all records from the database
                var documents = await _context.FileRecords.ToListAsync();

                // Return the result
                return Ok(documents);
            }
            catch (Exception ex)
            {
                // Handle errors
                return StatusCode(500, new { message = "An error occurred while retrieving documents", error = ex.Message });
            }
        }

        [HttpGet("documents-with-files")]
        public async Task<IActionResult> GetAllDocumentsWithFiles()
        {
            try
            {
                // Fetch all records from the database
                var documents = await _context.FileRecords.ToListAsync();

                // Map records to the DTO
                var documentDtos = documents.Select(doc =>
                {
                    string fileData = null;

                    // Read the file from the FilePath and convert to Base64
                    if (System.IO.File.Exists(doc.FilePath))
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(doc.FilePath);
                        fileData = Convert.ToBase64String(fileBytes); // Encode file as Base64
                    }

                    return new DocumentWithFileDto
                    {
                        Id = doc.Id,
                        FileName = doc.FileName,
                        ContentType = doc.ContentType,
                        UploadedAt = doc.UploadDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        FileData = fileData
                    };
                }).ToList();

                return Ok(documentDtos);
            }
            catch (Exception ex)
            {
                // Handle errors
                return StatusCode(500, new { message = "An error occurred while retrieving documents", error = ex.Message });
            }
        }

        [HttpGet("get-documents-by-email")]
        public async Task<IActionResult> GetDocumentsByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Email parameter is required." });
            }

            try
            {
                // Fetch records from the database filtered by email
                var documents = await _context.FileRecords
                    .Where(doc => doc.UserEmail == email)
                    .ToListAsync();

                if (documents == null || documents.Count == 0)
                {
                    return NotFound(new { message = "No documents found for the provided email." });
                }

                // Map records to the DTO
                var documentDtos = documents.Select(doc =>
                {
                    string fileData = null;

                    // Read the file from the FilePath and convert to Base64
                    if (!string.IsNullOrEmpty(doc.FilePath) && System.IO.File.Exists(doc.FilePath))
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(doc.FilePath);
                        fileData = Convert.ToBase64String(fileBytes); // Encode file as Base64
                    }

                    return new DocumentWithFileDto
                    {
                        Id = doc.Id,
                        FileName = doc.FileName,
                        ContentType = doc.ContentType,
                        UploadedAt = doc.UploadDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        FileData = fileData
                    };
                }).ToList();

                return Ok(documentDtos);
            }
            catch (Exception ex)
            {
                // Handle errors
                return StatusCode(500, new { message = "An error occurred while retrieving documents", error = ex.Message });
            }
        }




    }
}
