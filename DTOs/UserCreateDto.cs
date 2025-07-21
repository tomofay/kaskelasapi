namespace KasKelasApi.DTOs
{
    public class UserCreateDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int ClassId { get; set; }
        public ClassDto? Class { get; set; } // opsional
        public ParentDto? Parent { get; set; } // opsional
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ClassDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ParentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // Tambahkan field lain sesuai kebutuhan
    }
}
