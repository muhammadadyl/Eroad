namespace Eroad.BFF.Gateway.Application.Models;

public class AddDriverModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string DriverLicense { get; set; } = string.Empty;
}
