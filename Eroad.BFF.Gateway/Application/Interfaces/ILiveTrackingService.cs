using Eroad.BFF.Gateway.Application.DTOs;

namespace Eroad.BFF.Gateway.Application.Interfaces;

public interface ILiveTrackingService
{
    Task<LiveTrackingView> GetLiveTrackingAsync();
}
