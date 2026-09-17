using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Validators;

namespace QLTBYT.API.Tests.Phase1;

public class CreateDeviceRequestValidatorTests
{
    private readonly CreateDeviceRequestValidator _validator = new();

    [Fact]
    public void Empty_DeviceCode_Fails()
    {
        var result = _validator.Validate(new CreateDeviceRequest
        {
            DeviceCode = "",
            Name = "Ten",
            TypeId = 1,
            OrganizationId = 1,
            StatusId = 1
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateDeviceRequest.DeviceCode));
    }
}
