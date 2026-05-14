namespace ThesisBackend.DTOs.UserDTOs
{
    public class StudentSearchResultDTO
    {
        public int StudentId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int? TeacherId { get; set; }
    }

    public class PaginatedStudentsResponseDTO
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<StudentSearchResultDTO> Students { get; set; } = new List<StudentSearchResultDTO>();
    }
}
