namespace AdminService.Services.InferenceOperations.DTOs
{
    public class RemoveModelRequest
    {
        public string ModelName { get; set; }
        public bool DeleteFromDB { get; set; }

    }
}