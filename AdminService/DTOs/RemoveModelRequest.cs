namespace AdminService.DTOs
{
    public class RemoveModelRequest
    {
        public string ModelName { get; set; }
        public bool DeleteFromDB { get; set; }

    }
}