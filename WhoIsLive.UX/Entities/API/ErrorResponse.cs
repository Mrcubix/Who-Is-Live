using System.Text.Json.Serialization;

namespace WhoIsLive.UX.Entities.API;

public class ErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonConstructor]
    public ErrorResponse()
    {
    }

    public ErrorResponse(string error, int status, string message)
    {
        Error = error;
        Status = status;
        Message = message;
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Message))
        {
            if (string.IsNullOrEmpty(Error))
            {
                return "Unknown error.";
            }
            else
            {
                return Error;
            }
        }
        else
        {
            return Message;
        }
    }
}
